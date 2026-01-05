using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using ProjArqsi.Infrastructure;

namespace Backend.Tests.SystemTests.Infrastructure;

/// <summary>
/// Custom WebApplicationFactory for E2E integration tests.
/// Configures in-memory database and provides test isolation.
/// </summary>
public class CustomWebApplicationFactory : WebApplicationFactory<Program>
{
    private readonly string _databaseName;

    public CustomWebApplicationFactory()
    {
        // Use a unique database name for each test factory instance
        _databaseName = $"IntegrationTestDb_{Guid.NewGuid()}";
        
        // Set environment variables before any configuration is loaded
        // This ensures JWT settings are available when Program.cs runs
        Environment.SetEnvironmentVariable("JwtSettings__SigningKey", "TestSecretKeyForJwtTokenGeneration123456789012345678901234567890");
        Environment.SetEnvironmentVariable("JwtSettings__Issuer", "TestIssuer");
        Environment.SetEnvironmentVariable("JwtSettings__Audience", "TestAudience");
        Environment.SetEnvironmentVariable("JwtSettings__ExpirationMinutes", "60");
    }

    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        // Use Testing environment
        builder.UseEnvironment("Testing");

        builder.ConfigureAppConfiguration((context, config) =>
        {
            // Add in-memory configuration with highest priority to override all settings
            config.AddInMemoryCollection(new Dictionary<string, string?>
            {
                // JWT Configuration - must be at least 32 characters
                ["JwtSettings:SigningKey"] = "TestSecretKeyForJwtTokenGeneration123456789012345678901234567890",
                ["JwtSettings:Issuer"] = "TestIssuer",
                ["JwtSettings:Audience"] = "TestAudience",
                ["JwtSettings:ExpirationMinutes"] = "60",
                
                // Database Configuration
                ["ConnectionStrings:DefaultConnection"] = $"DataSource={_databaseName};Mode=Memory;Cache=Shared",
                
                // Logging Configuration
                ["Logging:LogLevel:Default"] = "Warning",
                ["Logging:LogLevel:Microsoft.AspNetCore"] = "Warning"
            });
        });

        builder.ConfigureServices(services =>
        {
            // Remove the real database context
            var descriptor = services.SingleOrDefault(
                d => d.ServiceType == typeof(DbContextOptions<AppDbContext>));

            if (descriptor != null)
            {
                services.Remove(descriptor);
            }

            // Add in-memory database for testing with unique name
            services.AddDbContext<AppDbContext>(options =>
            {
                options.UseInMemoryDatabase(_databaseName);
                // Suppress warnings in test environment
                options.EnableSensitiveDataLogging();
                options.EnableDetailedErrors();
            });
        });
    }

    /// <summary>
    /// Resets the database by deleting and recreating it.
    /// Should be called before each test class to ensure isolation.
    /// </summary>
    public async Task ResetDatabaseAsync()
    {
        using var scope = Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
        await db.Database.EnsureDeletedAsync();
        await db.Database.EnsureCreatedAsync();
    }

    /// <summary>
    /// Seeds minimal test data required for tests.
    /// Can be customized per test class.
    /// </summary>
    public async Task SeedTestDataAsync(Action<AppDbContext>? seedAction = null)
    {
        using var scope = Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
        
        seedAction?.Invoke(db);
        
        await db.SaveChangesAsync();
    }
}
