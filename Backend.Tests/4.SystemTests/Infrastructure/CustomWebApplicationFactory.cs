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
    }

    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        builder.ConfigureAppConfiguration((context, config) =>
        {
            // Override configuration with test values
            config.AddInMemoryCollection(new Dictionary<string, string?>
            {
                ["JwtSettings:SigningKey"] = "TestSecretKeyForJwtTokenGeneration123456789012345678901234567890", // 64 chars
                ["JwtSettings:Issuer"] = "TestIssuer",
                ["JwtSettings:Audience"] = "TestAudience",
                ["JwtSettings:ExpiresInMinutes"] = "60"
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

        // Use Testing environment to avoid production configurations
        builder.UseEnvironment("Testing");
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
