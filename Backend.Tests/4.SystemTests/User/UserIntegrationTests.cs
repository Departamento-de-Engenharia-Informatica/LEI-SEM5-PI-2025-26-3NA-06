using System.Net;
using Backend.Tests.SystemTests.Infrastructure;
using FluentAssertions;
using ProjArqsi.DTOs.User;
using Xunit;

namespace Backend.Tests.SystemTests.User;

/// <summary>
/// Integration tests for User endpoints.
/// Tests the complete flow: HTTP → Controller → Service → Repository → Database
/// </summary>
public class UserIntegrationTests : IClassFixture<CustomWebApplicationFactory>, IAsyncLifetime
{
    private readonly CustomWebApplicationFactory _factory;
    private readonly HttpClient _client;

    public UserIntegrationTests(CustomWebApplicationFactory factory)
    {
        _factory = factory;
        _client = factory.CreateClient();
    }

    public async Task InitializeAsync()
    {
        // Reset database before each test class
        await _factory.ResetDatabaseAsync();
    }

    public Task DisposeAsync() => Task.CompletedTask;

    #region Register Tests

    [Fact]
    public async Task Register_WithValidData_ShouldReturn200Ok()
    {
        // Arrange - No authentication needed for registration
        var dto = new UserUpsertDto
        {
            Username = "testuser123",
            Email = "testuser@example.com",
            Role = "PortAuthorityOfficer"
        };

        // Act
        var response = await _client.PostAsJsonAsync("/api/User/register", dto);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var content = await response.Content.ReadAsStringAsync();
        content.Should().Contain("Registration successful");
    }

    [Fact]
    public async Task Register_WithInvalidEmail_ShouldReturn400BadRequest()
    {
        // Arrange
        var dto = new UserUpsertDto
        {
            Username = "testuser",
            Email = "invalid-email", // Invalid email format
            Role = "PortAuthorityOfficer"
        };

        // Act
        var response = await _client.PostAsJsonAsync("/api/User/register", dto);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }

    [Fact]
    public async Task Register_WithInvalidUsername_ShouldReturn400BadRequest()
    {
        // Arrange
        var dto = new UserUpsertDto
        {
            Username = "ab", // Too short (< 3 characters)
            Email = "test@example.com",
            Role = "PortAuthorityOfficer"
        };

        // Act
        var response = await _client.PostAsJsonAsync("/api/User/register", dto);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }

    [Fact]
    public async Task Register_WithUsernameStartingWithNumber_ShouldReturn400BadRequest()
    {
        // Arrange
        var dto = new UserUpsertDto
        {
            Username = "123user", // Starts with number
            Email = "test@example.com",
            Role = "PortAuthorityOfficer"
        };

        // Act
        var response = await _client.PostAsJsonAsync("/api/User/register", dto);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }

    [Fact]
    public async Task Register_WithUsernameContainingSpaces_ShouldReturn400BadRequest()
    {
        // Arrange
        var dto = new UserUpsertDto
        {
            Username = "test user", // Contains spaces
            Email = "test@example.com",
            Role = "PortAuthorityOfficer"
        };

        // Act
        var response = await _client.PostAsJsonAsync("/api/User/register", dto);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }

    [Fact]
    public async Task Register_WithDuplicateEmail_ShouldReturn400BadRequest()
    {
        // Arrange
        var email = $"duplicate{Guid.NewGuid().ToString().Substring(0, 8)}@example.com";
        
        var firstDto = new UserUpsertDto
        {
            Username = "firstuser",
            Email = email,
            Role = "PortAuthorityOfficer"
        };

        // Register first user
        var firstResponse = await _client.PostAsJsonAsync("/api/User/register", firstDto);
        firstResponse.StatusCode.Should().Be(HttpStatusCode.OK);

        // Try to register with same email
        var secondDto = new UserUpsertDto
        {
            Username = "seconduser",
            Email = email, // Duplicate email
            Role = "LogisticOperator"
        };

        // Act
        var response = await _client.PostAsJsonAsync("/api/User/register", secondDto);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }

    #endregion

    #region Get Tests

    [Fact]
    public async Task GetAllUsers_WithAdminRole_ShouldReturn200Ok()
    {
        // Arrange
        var token = AuthenticationHelper.GenerateToken("Admin");
        _client.SetAuthorizationHeader(token);

        // Act
        var response = await _client.GetAsync("/api/User/all-users");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var result = await response.Content.ReadFromJsonAsync<List<UserDto>>();
        result.Should().NotBeNull();
    }

    [Fact]
    public async Task GetAllUsers_WithoutAuthentication_ShouldReturn401Unauthorized()
    {
        // Arrange - No token set

        // Act
        var response = await _client.GetAsync("/api/User/all-users");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    [Fact]
    public async Task GetAllUsers_WithNonAdminRole_ShouldReturn403Forbidden()
    {
        // Arrange
        var token = AuthenticationHelper.GenerateToken("PortAuthorityOfficer");
        _client.SetAuthorizationHeader(token);

        // Act
        var response = await _client.GetAsync("/api/User/all-users");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.Forbidden);
    }

    [Fact]
    public async Task GetInactiveUsers_WithAdminRole_ShouldReturn200Ok()
    {
        // Arrange
        var token = AuthenticationHelper.GenerateToken("Admin");
        _client.SetAuthorizationHeader(token);

        // Register a user (will be inactive by default)
        var registerDto = new UserUpsertDto
        {
            Username = "inactiveuser",
            Email = "inactive@example.com",
            Role = "PortAuthorityOfficer"
        };
        await _client.PostAsJsonAsync("/api/User/register", registerDto);

        // Act
        var response = await _client.GetAsync("/api/User/inactive-users");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var result = await response.Content.ReadFromJsonAsync<List<UserDto>>();
        result.Should().NotBeNull();
        result!.Should().Contain(u => u.Email == "inactive@example.com");
        result.Should().OnlyContain(u => u.IsActive == false);
    }

    #endregion

    #region Toggle Active Tests

    [Fact]
    public async Task ToggleUserActive_WithValidId_ShouldReturn200Ok()
    {
        // Arrange
        var token = AuthenticationHelper.GenerateToken("Admin");
        _client.SetAuthorizationHeader(token);

        // Register a user
        var registerDto = new UserUpsertDto
        {
            Username = "toggleuser",
            Email = $"toggle{Guid.NewGuid().ToString().Substring(0, 8)}@example.com",
            Role = "PortAuthorityOfficer"
        };
        await _client.PostAsJsonAsync("/api/User/register", registerDto);

        // Get all users to find the created user
        var getAllResponse = await _client.GetAsync("/api/User/all-users");
        var users = await getAllResponse.Content.ReadFromJsonAsync<List<UserDto>>();
        var user = users!.First(u => u.Email == registerDto.Email);

        // Act - Toggle active status
        var response = await _client.PutAsync($"/api/User/{user.Id}/toggle-active", null);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var content = await response.Content.ReadAsStringAsync();
        content.Should().Contain("activated");
    }

    [Fact]
    public async Task ToggleUserActive_WithNonExistentId_ShouldReturn400BadRequest()
    {
        // Arrange
        var token = AuthenticationHelper.GenerateToken("Admin");
        _client.SetAuthorizationHeader(token);
        var nonExistentId = Guid.NewGuid();

        // Act
        var response = await _client.PutAsync($"/api/User/{nonExistentId}/toggle-active", null);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }

    [Fact]
    public async Task ToggleUserActive_WithoutAuthentication_ShouldReturn401Unauthorized()
    {
        // Arrange - No token set
        var userId = Guid.NewGuid();

        // Act
        var response = await _client.PutAsync($"/api/User/{userId}/toggle-active", null);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    [Fact]
    public async Task ToggleUserActive_WithNonAdminRole_ShouldReturn403Forbidden()
    {
        // Arrange
        var token = AuthenticationHelper.GenerateToken("LogisticOperator");
        _client.SetAuthorizationHeader(token);
        var userId = Guid.NewGuid();

        // Act
        var response = await _client.PutAsync($"/api/User/{userId}/toggle-active", null);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.Forbidden);
    }

    #endregion

    #region Assign Role Tests

    [Fact]
    public async Task AssignRole_WithValidData_ShouldReturn200Ok()
    {
        // Arrange
        var token = AuthenticationHelper.GenerateToken("Admin");
        _client.SetAuthorizationHeader(token);

        // Register a user
        var registerDto = new UserUpsertDto
        {
            Username = "roleuser",
            Email = $"role{Guid.NewGuid().ToString().Substring(0, 8)}@example.com",
            Role = "PortAuthorityOfficer"
        };
        await _client.PostAsJsonAsync("/api/User/register", registerDto);

        // Get all users to find the created user
        var getAllResponse = await _client.GetAsync("/api/User/all-users");
        var users = await getAllResponse.Content.ReadFromJsonAsync<List<UserDto>>();
        var user = users!.First(u => u.Email == registerDto.Email);

        // Act - Assign new role
        var response = await _client.PutAsJsonAsync($"/api/User/{user.Id}/assign-role", "LogisticOperator");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var content = await response.Content.ReadAsStringAsync();
        content.Should().Contain("Role assigned");
    }

    [Fact]
    public async Task AssignRole_WithInvalidRole_ShouldReturn400BadRequest()
    {
        // Arrange
        var token = AuthenticationHelper.GenerateToken("Admin");
        _client.SetAuthorizationHeader(token);

        // Register a user
        var registerDto = new UserUpsertDto
        {
            Username = "invalidrole",
            Email = $"invalidrole{Guid.NewGuid().ToString().Substring(0, 8)}@example.com",
            Role = "PortAuthorityOfficer"
        };
        await _client.PostAsJsonAsync("/api/User/register", registerDto);

        // Get user
        var getAllResponse = await _client.GetAsync("/api/User/all-users");
        var users = await getAllResponse.Content.ReadFromJsonAsync<List<UserDto>>();
        var user = users!.First(u => u.Email == registerDto.Email);

        // Act - Assign invalid role
        var response = await _client.PutAsJsonAsync($"/api/User/{user.Id}/assign-role", "InvalidRole");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }

    [Fact]
    public async Task AssignRole_WithNonExistentId_ShouldReturn400BadRequest()
    {
        // Arrange
        var token = AuthenticationHelper.GenerateToken("Admin");
        _client.SetAuthorizationHeader(token);
        var nonExistentId = Guid.NewGuid();

        // Act
        var response = await _client.PutAsJsonAsync($"/api/User/{nonExistentId}/assign-role", "Admin");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }

    [Fact]
    public async Task AssignRole_WithoutAuthentication_ShouldReturn401Unauthorized()
    {
        // Arrange - No token set
        var userId = Guid.NewGuid();

        // Act
        var response = await _client.PutAsJsonAsync($"/api/User/{userId}/assign-role", "Admin");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    [Fact]
    public async Task AssignRole_WithNonAdminRole_ShouldReturn403Forbidden()
    {
        // Arrange
        var token = AuthenticationHelper.GenerateToken("ShippingAgentRepresentative");
        _client.SetAuthorizationHeader(token);
        var userId = Guid.NewGuid();

        // Act
        var response = await _client.PutAsJsonAsync($"/api/User/{userId}/assign-role", "Admin");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.Forbidden);
    }

    #endregion

    #region Email Validation Tests

    [Theory]
    [InlineData("plainaddress")] // No @
    [InlineData("@missinglocal.com")] // Missing local part
    [InlineData("missing@domain")] // Missing TLD
    [InlineData("missing@.com")] // Missing domain
    [InlineData("user@domain.")] // Ending with dot
    [InlineData("user@domain.c")] // TLD too short
    public async Task Register_WithInvalidEmailFormats_ShouldReturn400BadRequest(string invalidEmail)
    {
        // Arrange
        var dto = new UserUpsertDto
        {
            Username = "testuser",
            Email = invalidEmail,
            Role = "PortAuthorityOfficer"
        };

        // Act
        var response = await _client.PostAsJsonAsync("/api/User/register", dto);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }

    #endregion
}
