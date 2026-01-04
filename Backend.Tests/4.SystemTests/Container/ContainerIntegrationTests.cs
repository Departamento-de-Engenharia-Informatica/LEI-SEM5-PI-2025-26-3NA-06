using System.Net;
using Backend.Tests.SystemTests.Infrastructure;
using FluentAssertions;
using ProjArqsi.Application.DTOs;
using Xunit;

namespace Backend.Tests.SystemTests.Container;

/// <summary>
/// Integration tests for Container endpoints.
/// Tests the complete flow: HTTP → Controller → Service → Repository → Database
/// </summary>
public class ContainerIntegrationTests : IClassFixture<CustomWebApplicationFactory>, IAsyncLifetime
{
    private readonly CustomWebApplicationFactory _factory;
    private readonly HttpClient _client;

    public ContainerIntegrationTests(CustomWebApplicationFactory factory)
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

    #region Create Tests

    [Fact]
    public async Task CreateContainer_WithValidData_ShouldReturn201Created()
    {
        // Arrange
        var token = AuthenticationHelper.GenerateToken(roles: ["ShippingAgentRepresentative"]);
        _client.SetAuthorizationHeader(token);

        var dto = new UpsertContainerDto
        {
            IsoCode = "CSQU3054383", // Valid ISO code with correct check digit
            IsHazardous = false,
            CargoType = "DRY",
            Description = "Standard 20ft container"
        };

        // Act
        var response = await _client.PostAsJsonAsync("/api/Container", dto);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.Created);
        
        var result = await response.Content.ReadFromJsonAsync<ContainerDto>();
        result.Should().NotBeNull();
        result!.IsoCode.Should().Be(dto.IsoCode);
        result.IsHazardous.Should().Be(dto.IsHazardous);
        result.CargoType.Should().Be(dto.CargoType);
        result.Description.Should().Be(dto.Description);
        
        response.Headers.Location.Should().NotBeNull();
    }

    [Fact]
    public async Task CreateContainer_WithDuplicateIsoCode_ShouldReturn409Conflict()
    {
        // Arrange
        var token = AuthenticationHelper.GenerateToken(roles: ["ShippingAgentRepresentative"]);
        _client.SetAuthorizationHeader(token);

        var dto = new UpsertContainerDto
        {
            IsoCode = "MSCU6360000",
            IsHazardous = false,
            CargoType = "DRY",
            Description = "First container"
        };

        // Create first container
        await _client.PostAsJsonAsync("/api/Container", dto);

        // Act - Try to create duplicate
        var response = await _client.PostAsJsonAsync("/api/Container", dto);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.Conflict);
    }

    [Fact]
    public async Task CreateContainer_WithInvalidIsoCode_ShouldReturn400BadRequest()
    {
        // Arrange
        var token = AuthenticationHelper.GenerateToken(roles: ["ShippingAgentRepresentative"]);
        _client.SetAuthorizationHeader(token);

        var dto = new UpsertContainerDto
        {
            IsoCode = "INVALID", // Too short
            IsHazardous = false,
            CargoType = "DRY",
            Description = "Invalid container"
        };

        // Act
        var response = await _client.PostAsJsonAsync("/api/Container", dto);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }

    #endregion

    #region Get Tests

    [Fact]
    public async Task GetById_WithExistingId_ShouldReturn200Ok()
    {
        // Arrange
        var token = AuthenticationHelper.GenerateToken(roles: ["ShippingAgentRepresentative"]);
        _client.SetAuthorizationHeader(token);

        var createDto = new UpsertContainerDto
        {
            IsoCode = "CSQU3054383",
            IsHazardous = true,
            CargoType = "REEFER",
            Description = "Refrigerated container"
        };

        var createResponse = await _client.PostAsJsonAsync("/api/Container", createDto);
        createResponse.StatusCode.Should().Be(HttpStatusCode.Created);
        var created = await createResponse.Content.ReadFromJsonAsync<ContainerDto>();
        created.Should().NotBeNull();

        // Act
        var response = await _client.GetAsync($"/api/Container/{created!.Id}");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        
        var result = await response.Content.ReadFromJsonAsync<ContainerDto>();
        result.Should().NotBeNull();
        result!.Id.Should().Be(created.Id);
        result.IsoCode.Should().Be(createDto.IsoCode);
    }

    [Fact]
    public async Task GetById_WithNonExistentId_ShouldReturn404NotFound()
    {
        // Arrange
        var token = AuthenticationHelper.GenerateToken(roles: ["PortAuthorityOfficer"]);
        _client.SetAuthorizationHeader(token);

        var nonExistentId = Guid.NewGuid();

        // Act
        var response = await _client.GetAsync($"/api/Container/{nonExistentId}");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }

    [Fact]
    public async Task GetByIsoCode_WithExistingIsoCode_ShouldReturn200Ok()
    {
        // Arrange
        var token = AuthenticationHelper.GenerateToken(roles: ["ShippingAgentRepresentative"]);
        _client.SetAuthorizationHeader(token);

        var createDto = new UpsertContainerDto
        {
            IsoCode = "CSQU3054383",
            IsHazardous = false,
            CargoType = "DRY",
            Description = "Container for ISO code test"
        };

        var createResponse = await _client.PostAsJsonAsync("/api/Container", createDto);
        createResponse.StatusCode.Should().Be(HttpStatusCode.Created);

        // Act
        var response = await _client.GetAsync($"/api/Container/iso/{createDto.IsoCode}");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        
        var result = await response.Content.ReadFromJsonAsync<ContainerDto>();
        result.Should().NotBeNull();
        result!.IsoCode.Should().Be(createDto.IsoCode);
    }

    [Fact]
    public async Task GetByIsoCode_WithNonExistentIsoCode_ShouldReturn404NotFound()
    {
        // Arrange
        var token = AuthenticationHelper.GenerateToken(roles: ["PortAuthorityOfficer"]);
        _client.SetAuthorizationHeader(token);

        // Act - Using a valid ISO code that doesn't exist in DB
        var response = await _client.GetAsync("/api/Container/iso/CSQU9999999");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }

    [Fact]
    public async Task GetAll_ShouldReturnAllContainers()
    {
        // Arrange
        var token = AuthenticationHelper.GenerateToken(roles: ["ShippingAgentRepresentative"]);
        _client.SetAuthorizationHeader(token);

        // Create a container
        var dto = new UpsertContainerDto
        {
            IsoCode = "CSQU3054383",
            IsHazardous = false,
            CargoType = "DRY",
            Description = "Container 1"
        };

        await _client.PostAsJsonAsync("/api/Container", dto);

        // Act
        var response = await _client.GetAsync("/api/Container");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        
        var result = await response.Content.ReadFromJsonAsync<List<ContainerDto>>();
        result.Should().NotBeNull();
        result!.Should().HaveCountGreaterOrEqualTo(1);
        result!.Should().Contain(c => c.IsoCode == dto.IsoCode);
    }

    #endregion

    #region Update Tests

    [Fact]
    public async Task UpdateContainer_WithValidData_ShouldReturn200Ok()
    {
        // Arrange
        var token = AuthenticationHelper.GenerateToken(roles: ["ShippingAgentRepresentative"]);
        _client.SetAuthorizationHeader(token);

        var createDto = new UpsertContainerDto
        {
            IsoCode = "CSQU3054383",
            IsHazardous = false,
            CargoType = "DRY",
            Description = "Original description"
        };

        var createResponse = await _client.PostAsJsonAsync("/api/Container", createDto);
        createResponse.StatusCode.Should().Be(HttpStatusCode.Created);
        var created = await createResponse.Content.ReadFromJsonAsync<ContainerDto>();
        created.Should().NotBeNull();

        var updateDto = new UpsertContainerDto
        {
            IsoCode = "CSQU3054383", // Same ISO code
            IsHazardous = true,
            CargoType = "HAZMAT",
            Description = "Updated description"
        };

        // Act
        var response = await _client.PutAsJsonAsync($"/api/Container/{created!.Id}", updateDto);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        
        var result = await response.Content.ReadFromJsonAsync<ContainerDto>();
        result.Should().NotBeNull();
        result!.Description.Should().Be(updateDto.Description);
        result.IsHazardous.Should().Be(updateDto.IsHazardous);
        result.CargoType.Should().Be(updateDto.CargoType);
    }

    [Fact]
    public async Task UpdateContainer_WithNonExistentId_ShouldReturn404NotFound()
    {
        // Arrange
        var token = AuthenticationHelper.GenerateToken(roles: ["ShippingAgentRepresentative"]);
        _client.SetAuthorizationHeader(token);

        var nonExistentId = Guid.NewGuid();
        var updateDto = new UpsertContainerDto
        {
            IsoCode = "ABCU1234566",
            IsHazardous = false,
            CargoType = "DRY",
            Description = "This should not work"
        };

        // Act
        var response = await _client.PutAsJsonAsync($"/api/Container/{nonExistentId}", updateDto);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }

    [Fact]
    public async Task UpdateContainer_WithInvalidData_ShouldReturn400BadRequest()
    {
        // Arrange
        var token = AuthenticationHelper.GenerateToken(roles: ["ShippingAgentRepresentative"]);
        _client.SetAuthorizationHeader(token);

        var createDto = new UpsertContainerDto
        {
            IsoCode = "CSQU3054383",
            IsHazardous = false,
            CargoType = "DRY",
            Description = "Original"
        };

        var createResponse = await _client.PostAsJsonAsync("/api/Container", createDto);
        createResponse.StatusCode.Should().Be(HttpStatusCode.Created);
        var created = await createResponse.Content.ReadFromJsonAsync<ContainerDto>();
        created.Should().NotBeNull();

        var updateDto = new UpsertContainerDto
        {
            IsoCode = "INVALID",
            IsHazardous = false,
            CargoType = "DRY",
            Description = "Invalid ISO code"
        };

        // Act
        var response = await _client.PutAsJsonAsync($"/api/Container/{created!.Id}", updateDto);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }

    #endregion

    #region Delete Tests

    [Fact]
    public async Task DeleteContainer_WithExistingId_ShouldReturn204NoContent()
    {
        // Arrange
        var token = AuthenticationHelper.GenerateToken(roles: ["ShippingAgentRepresentative"]);
        _client.SetAuthorizationHeader(token);

        var createDto = new UpsertContainerDto
        {
            IsoCode = "CLHU1234561",
            IsHazardous = false,
            CargoType = "DRY",
            Description = "To be deleted"
        };

        var createResponse = await _client.PostAsJsonAsync("/api/Container", createDto);
        var created = await createResponse.Content.ReadFromJsonAsync<ContainerDto>();

        // Act
        var response = await _client.DeleteAsync($"/api/Container/{created!.Id}");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.NoContent);

        // Verify deletion
        var getResponse = await _client.GetAsync($"/api/Container/{created.Id}");
        getResponse.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }

    [Fact]
    public async Task DeleteContainer_WithNonExistentId_ShouldReturn404NotFound()
    {
        // Arrange
        var token = AuthenticationHelper.GenerateToken(roles: ["ShippingAgentRepresentative"]);
        _client.SetAuthorizationHeader(token);

        var nonExistentId = Guid.NewGuid();

        // Act
        var response = await _client.DeleteAsync($"/api/Container/{nonExistentId}");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }

    #endregion

    #region RBAC Tests

    [Fact]
    public async Task CreateContainer_WithoutToken_ShouldReturn401Unauthorized()
    {
        // Arrange - No token
        _client.ClearAuthorizationHeader();

        var dto = new UpsertContainerDto
        {
            IsoCode = "OOLU1234568",
            IsHazardous = false,
            CargoType = "DRY",
            Description = "Unauthorized test"
        };

        // Act
        var response = await _client.PostAsJsonAsync("/api/Container", dto);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    [Fact]
    public async Task CreateContainer_WithWrongRole_ShouldReturn403Forbidden()
    {
        // Arrange - PortAuthorityOfficer cannot create containers
        var token = AuthenticationHelper.GenerateToken(roles: ["PortAuthorityOfficer"]);
        _client.SetAuthorizationHeader(token);

        var dto = new UpsertContainerDto
        {
            IsoCode = "UACU1234565",
            IsHazardous = false,
            CargoType = "DRY",
            Description = "Forbidden test"
        };

        // Act
        var response = await _client.PostAsJsonAsync("/api/Container", dto);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.Forbidden);
    }

    [Fact]
    public async Task GetAll_WithPortAuthorityOfficer_ShouldReturn200Ok()
    {
        // Arrange - PortAuthorityOfficer can view containers
        var token = AuthenticationHelper.GenerateToken(roles: ["PortAuthorityOfficer"]);
        _client.SetAuthorizationHeader(token);

        // Act
        var response = await _client.GetAsync("/api/Container");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
    }

    [Fact]
    public async Task UpdateContainer_WithWrongRole_ShouldReturn403Forbidden()
    {
        // Arrange - Create container as ShippingAgent
        var createToken = AuthenticationHelper.GenerateToken(roles: ["ShippingAgentRepresentative"]);
        _client.SetAuthorizationHeader(createToken);

        var createDto = new UpsertContainerDto
        {
            IsoCode = "CSQU3054383",
            IsHazardous = false,
            CargoType = "DRY",
            Description = "Original"
        };

        var createResponse = await _client.PostAsJsonAsync("/api/Container", createDto);
        createResponse.StatusCode.Should().Be(HttpStatusCode.Created);
        var created = await createResponse.Content.ReadFromJsonAsync<ContainerDto>();
        created.Should().NotBeNull();

        // Change to PortAuthorityOfficer (cannot update)
        var updateToken = AuthenticationHelper.GenerateToken(roles: ["PortAuthorityOfficer"]);
        _client.SetAuthorizationHeader(updateToken);

        var updateDto = new UpsertContainerDto
        {
            IsoCode = "CSQU3054383",
            IsHazardous = true,
            CargoType = "HAZMAT",
            Description = "Should not update"
        };

        // Act
        var response = await _client.PutAsJsonAsync($"/api/Container/{created!.Id}", updateDto);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.Forbidden);
    }

    [Fact]
    public async Task DeleteContainer_WithWrongRole_ShouldReturn403Forbidden()
    {
        // Arrange - Create container as ShippingAgent
        var createToken = AuthenticationHelper.GenerateToken(roles: ["ShippingAgentRepresentative"]);
        _client.SetAuthorizationHeader(createToken);

        var createDto = new UpsertContainerDto
        {
            IsoCode = "CSQU3054383",
            IsHazardous = false,
            CargoType = "DRY",
            Description = "To be deleted"
        };

        var createResponse = await _client.PostAsJsonAsync("/api/Container", createDto);
        createResponse.StatusCode.Should().Be(HttpStatusCode.Created);
        var created = await createResponse.Content.ReadFromJsonAsync<ContainerDto>();
        created.Should().NotBeNull();

        // Change to PortAuthorityOfficer (cannot delete)
        var deleteToken = AuthenticationHelper.GenerateToken(roles: ["PortAuthorityOfficer"]);
        _client.SetAuthorizationHeader(deleteToken);

        // Act
        var response = await _client.DeleteAsync($"/api/Container/{created!.Id}");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.Forbidden);
    }

    #endregion
}
