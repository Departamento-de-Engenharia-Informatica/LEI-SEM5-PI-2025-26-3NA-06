using ProjArqsi.Application.Services;
using ProjArqsi.Domain.VesselTypeAggregate;
using ProjArqsi.Domain.Shared;
using ProjArqsi.Infrastructure.Repositories;
using ProjArqsi.Infrastructure.Shared;
using ProjArqsi.Infrastructure;
using Microsoft.EntityFrameworkCore;
using ProjArqsi.Domain.UserAggregate;
using ProjArqsi.Services;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Google;
using Microsoft.AspNetCore.Authentication.Cookies;
using System.Security.Claims;

using Microsoft.AspNetCore.DataProtection;
using ProjArqsi.Domain.UserAggregate.ValueObjects;
using Microsoft.Extensions.DependencyInjection;

var builder = WebApplication.CreateBuilder(args);
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

// Register application services
builder.Services.AddScoped<VesselTypeService>();
builder.Services.AddScoped<UserService>();
builder.Services.AddScoped<EmailService>();
builder.Services.AddScoped<RegistrationService>();
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
            var user = await userService.FindByEmailAsync(email ?? "");
            
            var identity = (ClaimsIdentity)context.Principal?.Identity!;
            identity.AddClaim(new Claim("urn:google:access_token", context.AccessToken ?? ""));
            identity.AddClaim(new Claim("urn:google:expires_in", context.ExpiresIn.ToString() ?? ""));
            
            // if user logs in for the first time
            if (user is null)
            {
                Console.WriteLine("User not found, needs registration: " + email);
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
            else
            {
                Console.WriteLine("User authenticated: " + email);
                identity.AddClaim(new Claim(ClaimTypes.Role, user.Role.ToString()));
            }
        };
        
        options.Events.OnTicketReceived = context =>
        {
            // Check if user needs registration and redirect
            var needsRegistration = context.Principal?.FindFirstValue("needs_registration");
            if (needsRegistration == "true")
            {
                Console.WriteLine("Redirecting to registration page");
                context.ReturnUri = "http://localhost:5173/register";
            }
            return Task.CompletedTask;
        };
    });

builder.Services.AddControllers();
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

var app = builder.Build();
// Use CORS before authentication
app.UseCors("AllowFrontend");
//Console.WriteLine("Backend app started!");

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();

app.UseAuthentication();

app.UseAuthorization();

app.MapControllers();

app.Run();
