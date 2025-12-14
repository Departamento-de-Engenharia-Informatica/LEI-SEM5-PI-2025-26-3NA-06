using ProjArqsi.Application.Services;
using ProjArqsi.Domain.VesselTypeAggregate;
using ProjArqsi.Domain.VesselAggregate;
using ProjArqsi.Domain.DockAggregate;
using ProjArqsi.Domain.Shared;
using ProjArqsi.Infrastructure.Repositories;
using ProjArqsi.Infrastructure.Shared;
using ProjArqsi.Infrastructure;
using Microsoft.EntityFrameworkCore;
using ProjArqsi.Domain.UserAggregate;
using ProjArqsi.Services;
using Microsoft.AspNetCore.Authentication.Google;
using Microsoft.AspNetCore.Authentication.Cookies;
using System.Security.Claims;
using Microsoft.AspNetCore.DataProtection;
using ProjArqsi.Middleware;
using Serilog;
using ProjArqsi.Domain.StorageAreaAggregate;

// Configure Serilog - only log warnings and errors, suppress all informational logs
Log.Logger = new LoggerConfiguration()
    .MinimumLevel.Warning()
    .Filter.ByExcluding(logEvent => 
        logEvent.MessageTemplate.Text.Contains("Failed to determine the https port"))
    .WriteTo.File("logs/log-.txt", rollingInterval: RollingInterval.Day)
    .CreateLogger();

var builder = WebApplication.CreateBuilder(args);
builder.Host.UseSerilog();

// Enable CORS for frontend
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowFrontend",
        policy => policy
            .WithOrigins("http://localhost:5173")
            .AllowAnyHeader()
            .AllowAnyMethod()
            .AllowCredentials());
});

// Add services to the container.

// Add EF Core DbContext for SQL Server with retry logic for transient failures
builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseSqlServer(
        builder.Configuration.GetConnectionString("DefaultConnection"),
        sqlServerOptions => sqlServerOptions.EnableRetryOnFailure(
            maxRetryCount: 5,
            maxRetryDelay: TimeSpan.FromSeconds(30),
            errorNumbersToAdd: null
        )
    ));

// Register repositories
builder.Services.AddScoped<IUnitOfWork, UnitOfWork>();
builder.Services.AddScoped<IVesselTypeRepository, VesselTypeRepository>();
builder.Services.AddScoped<IUserRepository, UserRepository>();
builder.Services.AddScoped<IVesselRepository, VesselRepository>();
builder.Services.AddScoped<IDockRepository, DockRepository>();
builder.Services.AddScoped<IStorageAreaRepository, StorageAreaRepository>();

// Register application services
builder.Services.AddScoped<VesselTypeService>();
builder.Services.AddScoped<VesselService>();
builder.Services.AddScoped<UserService>();
builder.Services.AddScoped<EmailService>();
builder.Services.AddScoped<RegistrationService>();
builder.Services.AddScoped<DockService>();
builder.Services.AddScoped<StorageAreaService>();
builder.Services.AddAutoMapper(AppDomain.CurrentDomain.GetAssemblies());

// Add authentication (Google & Cookie) with custom event
builder.Services.AddAuthentication(options =>
{
    options.DefaultScheme = CookieAuthenticationDefaults.AuthenticationScheme;
    options.DefaultChallengeScheme = GoogleDefaults.AuthenticationScheme;
})
    .AddCookie(options =>
    {
        options.Cookie.SameSite = SameSiteMode.None;
        options.Cookie.SecurePolicy = CookieSecurePolicy.Always;
        options.ExpireTimeSpan = TimeSpan.FromDays(150);
    })
    .AddGoogle(GoogleDefaults.AuthenticationScheme, options =>
    {
        options.ClientId = builder.Configuration["GoogleKeys:ClientId"] 
            ?? throw new InvalidOperationException("Google ClientId not configured");
        options.ClientSecret = builder.Configuration["GoogleKeys:ClientSecret"] 
            ?? throw new InvalidOperationException("Google ClientSecret not configured");
        options.Events.OnCreatingTicket = async context =>
        {
            var email = context.Principal?.FindFirstValue(ClaimTypes.Email);
            var userService = context.HttpContext.RequestServices.GetRequiredService<UserService>();
            var registrationService = context.HttpContext.RequestServices.GetRequiredService<RegistrationService>();
            var user = await userService.FindByEmailAsync(email ?? "");
            
            var identity = (ClaimsIdentity)context.Principal?.Identity!;
            identity.AddClaim(new Claim("urn:google:access_token", context.AccessToken ?? ""));
            identity.AddClaim(new Claim("urn:google:expires_in", context.ExpiresIn.ToString() ?? ""));
            
            // Check if this is an activation flow (token present in redirect URI)
            var redirectUri = context.Properties?.RedirectUri;
            string? activationToken = null;
            if (!string.IsNullOrEmpty(redirectUri) && redirectUri.Contains("token="))
            {
                var uri = new Uri(redirectUri, UriKind.RelativeOrAbsolute);
                var query = uri.IsAbsoluteUri ? uri.Query : redirectUri.Substring(redirectUri.IndexOf('?'));
                var queryParams = System.Web.HttpUtility.ParseQueryString(query);
                activationToken = queryParams["token"];
            }
            
            // if user logs in for the first time
            if (user is null)
            {
                var dataProtectionProvider = context.HttpContext.RequestServices.GetRequiredService<IDataProtectionProvider>();
                var protector = dataProtectionProvider.CreateProtector("CustomCookieProtector");
                var encryptedEmail = protector.Protect(email ?? "");
                context.HttpContext.Response.Cookies.Append(".AspNetCore.CustomCookies", encryptedEmail, new CookieOptions
                {
                    HttpOnly = true,
                    Secure = true,
                    Expires = DateTimeOffset.UtcNow.AddMinutes(30)
                });
                // Add a claim to indicate user needs registration
                identity.AddClaim(new Claim("needs_registration", "true"));
                identity.AddClaim(new Claim(ClaimTypes.Role, "Pending"));
            }
            else if (!user.IsActive && !string.IsNullOrEmpty(activationToken))
            {
                // User is inactive but has activation token - activate them now
                try
                {
                    await registrationService.ConfirmEmailAsync(activationToken);
                    // Reload user to get updated status
                    user = await userService.FindByEmailAsync(email ?? "");
                    if (user != null && user.IsActive)
                    {
                        identity.AddClaim(new Claim(ClaimTypes.Role, user.Role.ToString()));
                    }
                }
                catch (Exception)
                {
                    identity.AddClaim(new Claim("access_denied", "true"));
                    identity.AddClaim(new Claim("access_denied_reason", "Activation failed. Please contact support."));
                    identity.AddClaim(new Claim(ClaimTypes.Role, "Inactive"));
                }
            }
            else if (!user.IsActive)
            {
                // User exists but is inactive - deny access
                identity.AddClaim(new Claim("access_denied", "true"));
                identity.AddClaim(new Claim("access_denied_reason", "Your account is inactive. Please contact an administrator."));
                identity.AddClaim(new Claim(ClaimTypes.Role, "Inactive"));
            }
            else
            {
                identity.AddClaim(new Claim(ClaimTypes.Role, user.Role.ToString()));
            }
        };
        
        options.Events.OnTicketReceived = context =>
        {
            // Check if access is denied (inactive user)
            var accessDenied = context.Principal?.FindFirstValue("access_denied");
            if (accessDenied == "true")
            {
                var reason = context.Principal?.FindFirstValue("access_denied_reason");
                context.ReturnUri = $"http://localhost:5173/access-denied?reason={Uri.EscapeDataString(reason ?? "Account inactive")}";
                return Task.CompletedTask;
            }
            
            // Check if user needs registration and redirect
            var needsRegistration = context.Principal?.FindFirstValue("needs_registration");
            if (needsRegistration == "true")
            {
                context.ReturnUri = "http://localhost:5173/register";
            }
            else
            {
                // User exists, redirect to role-based dashboard WITH role, email, and name query parameters
                var role = context.Principal?.FindFirstValue(ClaimTypes.Role);
                var email = context.Principal?.FindFirstValue(ClaimTypes.Email);
                var name = context.Principal?.FindFirstValue(ClaimTypes.Name);
                
                // URL encode the parameters
                var encodedEmail = Uri.EscapeDataString(email ?? "");
                var encodedName = Uri.EscapeDataString(name ?? "");
                
                context.ReturnUri = role switch
                {
                    "Admin" => $"http://localhost:5173/admin?role=Admin&email={encodedEmail}&name={encodedName}",
                    "PortAuthorityOfficer" => $"http://localhost:5173/port-authority?role=PortAuthorityOfficer&email={encodedEmail}&name={encodedName}",
                    "LogisticOperator" => $"http://localhost:5173/logistic-operator?role=LogisticOperator&email={encodedEmail}&name={encodedName}",
                    "ShippingAgentRepresentative" => $"http://localhost:5173/shipping-agent?role=ShippingAgentRepresentative&email={encodedEmail}&name={encodedName}",
                    _ => "http://localhost:5173/login"
                };
            }
            return Task.CompletedTask;
        };
    });

builder.Services.AddControllers();
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

var app = builder.Build();

// Apply pending migrations on startup
using (var scope = app.Services.CreateScope())
{
    var dbContext = scope.ServiceProvider.GetRequiredService<AppDbContext>();
    dbContext.Database.Migrate();
}

// Use CORS before authentication
app.UseCors("AllowFrontend");

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();

app.UseAuthentication();

app.UseAuthorization();

// Log unauthorized access attempts
app.UseMiddleware<AuthorizationLoggingMiddleware>();

app.MapControllers();

Console.WriteLine("\n✓ App is ready\n");

app.Run();
