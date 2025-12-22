using ProjArqsi.Application.Services;
using ProjArqsi.Application.Logging;
using ProjArqsi.Domain.VesselTypeAggregate;
using ProjArqsi.Domain.VesselAggregate;
using ProjArqsi.Domain.DockAggregate;
using ProjArqsi.Domain.Shared;
using ProjArqsi.Infrastructure.Repositories;
using ProjArqsi.Infrastructure.Shared;
using ProjArqsi.Domain.UserAggregate;
using ProjArqsi.Services;
using ProjArqsi.Middleware;
using Serilog;
using ProjArqsi.Domain.StorageAreaAggregate;
using ProjArqsi.Domain.VesselVisitNotificationAggregate;
using ProjArqsi.Domain.ContainerAggregate;
using Infrastructure;
using ProjArqsi.Infrastructure;
using Microsoft.EntityFrameworkCore;
using ProjArqsi.Auth.Common;

// Configure Serilog - only log unauthorized access and VVN approval/rejection decisions
Log.Logger = new LoggerConfiguration()
    .MinimumLevel.Information()
    .Filter.ByIncludingOnly(logEvent => 
        // Only log VVN approval/rejection decisions
        logEvent.MessageTemplate.Text.Contains("VVN APPROVED") ||
        logEvent.MessageTemplate.Text.Contains("VVN REJECTED") ||
        // And unauthorized access attempts
        logEvent.MessageTemplate.Text.Contains("UNAUTHORIZED ACCESS") ||
        logEvent.MessageTemplate.Text.Contains("ACCESS DENIED")
    )
    .WriteTo.File("logs/log-.txt", rollingInterval: RollingInterval.Day)
    .CreateLogger();

var builder = WebApplication.CreateBuilder(args);
builder.Host.UseSerilog();

// Enable CORS for frontend
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowFrontend",
        policy => policy
            .WithOrigins("http://localhost:4200")
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
builder.Services.AddScoped<IVesselVisitNotificationRepository, VesselVisitNotificationRepository>();
builder.Services.AddScoped<IContainerRepository, ContainerRepository>();

// Register application services
builder.Services.AddScoped<VesselTypeService>();
builder.Services.AddScoped<VesselService>();
builder.Services.AddScoped<UserService>();
builder.Services.AddScoped<EmailService>();
builder.Services.AddScoped<DockService>();
builder.Services.AddScoped<StorageAreaService>();
builder.Services.AddScoped<VesselVisitNotificationService>();
builder.Services.AddScoped<ContainerService>();
builder.Services.AddAutoMapper(AppDomain.CurrentDomain.GetAssemblies());

// Register logging classes
builder.Services.AddScoped<ICoreApiLogger, CoreApiLogger>();

// Add JWT authentication using shared library
builder.Services.AddJwtAuthentication(builder.Configuration);

builder.Services.AddControllers();
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new() { Title = "ProjArqsi API", Version = "v1" });
    c.AddSecurityDefinition("Bearer", new Microsoft.OpenApi.Models.OpenApiSecurityScheme
    {
        Description = "JWT Authorization header using the Bearer scheme. Enter 'Bearer' [space] and then your token.",
        Name = "Authorization",
        In = Microsoft.OpenApi.Models.ParameterLocation.Header,
        Type = Microsoft.OpenApi.Models.SecuritySchemeType.ApiKey,
        Scheme = "Bearer"
    });
    c.AddSecurityRequirement(new Microsoft.OpenApi.Models.OpenApiSecurityRequirement
    {
        {
            new Microsoft.OpenApi.Models.OpenApiSecurityScheme
            {
                Reference = new Microsoft.OpenApi.Models.OpenApiReference
                {
                    Type = Microsoft.OpenApi.Models.ReferenceType.SecurityScheme,
                    Id = "Bearer"
                }
            },
            Array.Empty<string>()
        }
    });
});

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
