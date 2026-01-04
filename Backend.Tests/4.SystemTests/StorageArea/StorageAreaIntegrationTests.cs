using System.Net;
using Backend.Tests.SystemTests.Infrastructure;
using FluentAssertions;
using ProjArqsi.Application.DTOs.StorageArea;
using Xunit;

namespace Backend.Tests.SystemTests.StorageArea;

/// <summary>
/// Integration tests for StorageArea endpoints.
/// Tests the complete flow: HTTP → Controller → Service → Repository → Database
/// </summary>
public class StorageAreaIntegrationTests : IClassFixture<CustomWebApplicationFactory>, IAsyncLifetime
{
    private readonly CustomWebApplicationFactory _factory;
    private readonly HttpClient _client;

    public StorageAreaIntegrationTests(CustomWebApplicationFactory factory)
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
    public async Task CreateStorageArea_WithValidData_ShouldReturn201Created()
    {
        // Arrange
        var token = AuthenticationHelper.GenerateToken(roles: ["PortAuthorityOfficer"]);
        _client.SetAuthorizationHeader(token);

        var dto = new StorageAreaUpsertDto
        {
            AreaName = "North Yard A1",
            AreaType = "Yard",
            Location = "North terminal area, section A1",
            MaxCapacity = 1000,
            ServesEntirePort = false,
            ServedDockIds = new List<string>()
        };

        // Act
        var response = await _client.PostAsJsonAsync("/api/StorageArea", dto);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.Created);
        response.Headers.Location.Should().NotBeNull();

        var result = await response.Content.ReadFromJsonAsync<StorageAreaDto>();
        result.Should().NotBeNull();
        result!.AreaName.Should().Be(dto.AreaName);
        result.AreaType.Should().Be(dto.AreaType);
        result.Location.Should().Be(dto.Location);
        result.MaxCapacity.Should().Be(dto.MaxCapacity);
        result.ServesEntirePort.Should().Be(dto.ServesEntirePort);
    }

    [Fact]
    public async Task CreateStorageArea_WithWarehouseType_ShouldReturn201Created()
    {
        // Arrange
        var token = AuthenticationHelper.GenerateToken(roles: ["PortAuthorityOfficer"]);
        _client.SetAuthorizationHeader(token);

        var dto = new StorageAreaUpsertDto
        {
            AreaName = "Central Warehouse B2",
            AreaType = "Warehouse",
            Location = "Central terminal, building B2",
            MaxCapacity = 500,
            ServesEntirePort = true,
            ServedDockIds = new List<string>()
        };

        // Act
        var response = await _client.PostAsJsonAsync("/api/StorageArea", dto);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.Created);
        
        var result = await response.Content.ReadFromJsonAsync<StorageAreaDto>();
        result.Should().NotBeNull();
        result!.AreaType.Should().Be("Warehouse");
        result.ServesEntirePort.Should().BeTrue();
    }

    [Fact]
    public async Task CreateStorageArea_WithDuplicateName_ShouldReturn400BadRequest()
    {
        // Arrange
        var token = AuthenticationHelper.GenerateToken("PortAuthorityOfficer");
        _client.SetAuthorizationHeader(token);

        var uniqueName = $"Storage Area {Guid.NewGuid()}";
        var dto = new StorageAreaUpsertDto
        {
            AreaName = uniqueName,
            AreaType = "Yard",
            Location = "Test location",
            MaxCapacity = 800,
            ServesEntirePort = false,
            ServedDockIds = new List<string>()
        };

        // Create first storage area
        var firstResponse = await _client.PostAsJsonAsync("/api/StorageArea", dto);
        firstResponse.StatusCode.Should().Be(HttpStatusCode.Created);

        // Act - Try to create duplicate
        var secondResponse = await _client.PostAsJsonAsync("/api/StorageArea", dto);

        // Assert
        secondResponse.StatusCode.Should().Be(HttpStatusCode.BadRequest);
        var errorContent = await secondResponse.Content.ReadAsStringAsync();
        errorContent.Should().Contain("already exists");
    }

    [Fact]
    public async Task CreateStorageArea_WithEmptyName_ShouldReturn400BadRequest()
    {
        // Arrange
        var token = AuthenticationHelper.GenerateToken("PortAuthorityOfficer");
        _client.SetAuthorizationHeader(token);

        var dto = new StorageAreaUpsertDto
        {
            AreaName = "",
            AreaType = "Yard",
            Location = "Test location",
            MaxCapacity = 500,
            ServesEntirePort = false,
            ServedDockIds = new List<string>()
        };

        // Act
        var response = await _client.PostAsJsonAsync("/api/StorageArea", dto);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }

    [Fact]
    public async Task CreateStorageArea_WithInvalidCapacity_ShouldReturn500InternalServerError()
    {
        // Arrange
        var token = AuthenticationHelper.GenerateToken("PortAuthorityOfficer");
        _client.SetAuthorizationHeader(token);

        var dto = new StorageAreaUpsertDto
        {
            AreaName = "Test Area",
            AreaType = "Yard",
            Location = "Test location",
            MaxCapacity = -100, // Invalid negative capacity - throws ArgumentOutOfRangeException
            ServesEntirePort = false,
            ServedDockIds = new List<string>()
        };

        // Act
        var response = await _client.PostAsJsonAsync("/api/StorageArea", dto);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.InternalServerError);
    }

    [Fact]
    public async Task CreateStorageArea_WithZeroCapacity_ShouldReturn500InternalServerError()
    {
        // Arrange
        var token = AuthenticationHelper.GenerateToken("PortAuthorityOfficer");
        _client.SetAuthorizationHeader(token);

        var dto = new StorageAreaUpsertDto
        {
            AreaName = "Test Area Zero",
            AreaType = "Yard",
            Location = "Test location",
            MaxCapacity = 0, // Invalid zero capacity - throws ArgumentOutOfRangeException
            ServesEntirePort = false,
            ServedDockIds = new List<string>()
        };

        // Act
        var response = await _client.PostAsJsonAsync("/api/StorageArea", dto);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.InternalServerError);
    }

    [Fact]
    public async Task CreateStorageArea_WithoutAuthentication_ShouldReturn401Unauthorized()
    {
        // Arrange - No token set
        var dto = new StorageAreaUpsertDto
        {
            AreaName = "Unauthorized Area",
            AreaType = "Yard",
            Location = "Test location",
            MaxCapacity = 500,
            ServesEntirePort = false,
            ServedDockIds = new List<string>()
        };

        // Act
        var response = await _client.PostAsJsonAsync("/api/StorageArea", dto);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    #endregion

    #region Get Tests

    [Fact]
    public async Task GetAll_ShouldReturnAllStorageAreas()
    {
        // Arrange
        var token = AuthenticationHelper.GenerateToken("PortAuthorityOfficer");
        _client.SetAuthorizationHeader(token);

        var guid = Guid.NewGuid().ToString().Substring(0, 8);

        // Create multiple storage areas with unique names
        var areas = new[]
        {
            new StorageAreaUpsertDto { AreaName = $"Area Alpha {guid}", AreaType = "Yard", Location = "Location A", MaxCapacity = 1000, ServesEntirePort = false, ServedDockIds = new List<string>() },
            new StorageAreaUpsertDto { AreaName = $"Area Beta {guid}", AreaType = "Warehouse", Location = "Location B", MaxCapacity = 800, ServesEntirePort = true, ServedDockIds = new List<string>() },
            new StorageAreaUpsertDto { AreaName = $"Area Gamma {guid}", AreaType = "Yard", Location = "Location C", MaxCapacity = 1200, ServesEntirePort = false, ServedDockIds = new List<string>() }
        };

        foreach (var dto in areas)
        {
            var createResponse = await _client.PostAsJsonAsync("/api/StorageArea", dto);
            createResponse.StatusCode.Should().Be(HttpStatusCode.Created);
        }

        // Act
        var response = await _client.GetAsync("/api/StorageArea");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var result = await response.Content.ReadFromJsonAsync<List<StorageAreaDto>>();
        result.Should().NotBeNull();
        result!.Count.Should().BeGreaterOrEqualTo(3);
        result.Should().Contain(a => a.AreaName == areas[0].AreaName);
        result.Should().Contain(a => a.AreaName == areas[1].AreaName);
        result.Should().Contain(a => a.AreaName == areas[2].AreaName);
    }

    [Fact]
    public async Task GetById_WithExistingId_ShouldReturn200Ok()
    {
        // Arrange
        var token = AuthenticationHelper.GenerateToken(roles: ["PortAuthorityOfficer"]);
        _client.SetAuthorizationHeader(token);

        var createDto = new StorageAreaUpsertDto
        {
            AreaName = "GetById Test Area",
            AreaType = "Yard",
            Location = "Test location for GetById",
            MaxCapacity = 900,
            ServesEntirePort = false,
            ServedDockIds = new List<string>()
        };

        var createResponse = await _client.PostAsJsonAsync("/api/StorageArea", createDto);
        createResponse.StatusCode.Should().Be(HttpStatusCode.Created);
        var created = await createResponse.Content.ReadFromJsonAsync<StorageAreaDto>();
        created.Should().NotBeNull();

        // Act
        var response = await _client.GetAsync($"/api/StorageArea/{created!.Id}");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        
        var result = await response.Content.ReadFromJsonAsync<StorageAreaDto>();
        result.Should().NotBeNull();
        result!.Id.Should().Be(created.Id);
        result.AreaName.Should().Be(createDto.AreaName);
    }

    [Fact]
    public async Task GetById_WithNonExistentId_ShouldReturn404NotFound()
    {
        // Arrange
        var token = AuthenticationHelper.GenerateToken(roles: ["PortAuthorityOfficer"]);
        _client.SetAuthorizationHeader(token);
        var nonExistentId = Guid.NewGuid();

        // Act
        var response = await _client.GetAsync($"/api/StorageArea/{nonExistentId}");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }

    [Fact]
    public async Task GetById_WithInvalidGuid_ShouldReturn400BadRequest()
    {
        // Arrange
        var token = AuthenticationHelper.GenerateToken(roles: ["PortAuthorityOfficer"]);
        _client.SetAuthorizationHeader(token);

        // Act
        var response = await _client.GetAsync("/api/StorageArea/invalid-guid-format");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }

    #endregion

    #region Update Tests

    [Fact]
    public async Task UpdateStorageArea_WithValidData_ShouldReturn200Ok()
    {
        // Arrange
        var token = AuthenticationHelper.GenerateToken(roles: ["PortAuthorityOfficer"]);
        _client.SetAuthorizationHeader(token);

        var createDto = new StorageAreaUpsertDto
        {
            AreaName = "Original Area Name",
            AreaType = "Yard",
            Location = "Original location",
            MaxCapacity = 1000,
            ServesEntirePort = false,
            ServedDockIds = new List<string>()
        };

        var createResponse = await _client.PostAsJsonAsync("/api/StorageArea", createDto);
        var created = await createResponse.Content.ReadFromJsonAsync<StorageAreaDto>();

        var updateDto = new StorageAreaUpsertDto
        {
            AreaName = "Updated Area Name",
            AreaType = "Warehouse",
            Location = "Updated location description",
            MaxCapacity = 1500,
            ServesEntirePort = true,
            ServedDockIds = new List<string>()
        };

        // Act
        var response = await _client.PutAsJsonAsync($"/api/StorageArea/{created!.Id}", updateDto);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        
        var result = await response.Content.ReadFromJsonAsync<StorageAreaDto>();
        result.Should().NotBeNull();
        result!.AreaName.Should().Be(updateDto.AreaName);
        result.AreaType.Should().Be(updateDto.AreaType);
        result.Location.Should().Be(updateDto.Location);
        result.MaxCapacity.Should().Be(updateDto.MaxCapacity);
        result.ServesEntirePort.Should().Be(updateDto.ServesEntirePort);
    }

    [Fact]
    public async Task UpdateStorageArea_WithNonExistentId_ShouldReturn400BadRequest()
    {
        // Arrange
        var token = AuthenticationHelper.GenerateToken(roles: ["PortAuthorityOfficer"]);
        _client.SetAuthorizationHeader(token);
        var nonExistentId = Guid.NewGuid();

        var updateDto = new StorageAreaUpsertDto
        {
            AreaName = "Non-existent Area",
            AreaType = "Yard",
            Location = "Some location",
            MaxCapacity = 1000,
            ServesEntirePort = false,
            ServedDockIds = new List<string>()
        };

        // Act
        var response = await _client.PutAsJsonAsync($"/api/StorageArea/{nonExistentId}", updateDto);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }

    [Fact]
    public async Task UpdateStorageArea_WithInvalidData_ShouldReturn500InternalServerError()
    {
        // Arrange
        var token = AuthenticationHelper.GenerateToken(roles: ["PortAuthorityOfficer"]);
        _client.SetAuthorizationHeader(token);

        var createDto = new StorageAreaUpsertDto
        {
            AreaName = "Test Area for Update",
            AreaType = "Yard",
            Location = "Test location",
            MaxCapacity = 1000,
            ServesEntirePort = false,
            ServedDockIds = new List<string>()
        };

        var createResponse = await _client.PostAsJsonAsync("/api/StorageArea", createDto);
        var created = await createResponse.Content.ReadFromJsonAsync<StorageAreaDto>();

        var updateDto = new StorageAreaUpsertDto
        {
            AreaName = "Updated Name",
            AreaType = "Yard",
            Location = "Updated location",
            MaxCapacity = -500, // Invalid negative capacity - throws ArgumentOutOfRangeException
            ServesEntirePort = false,
            ServedDockIds = new List<string>()
        };

        // Act
        var response = await _client.PutAsJsonAsync($"/api/StorageArea/{created!.Id}", updateDto);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.InternalServerError);
    }

    [Fact]
    public async Task UpdateStorageArea_WithDuplicateName_ShouldReturn400BadRequest()
    {
        // Arrange
        var token = AuthenticationHelper.GenerateToken(roles: ["PortAuthorityOfficer"]);
        _client.SetAuthorizationHeader(token);

        var guid = Guid.NewGuid().ToString().Substring(0, 8);
        var existingName = $"Existing Area {guid}";

        // Create first storage area
        var firstDto = new StorageAreaUpsertDto
        {
            AreaName = existingName,
            AreaType = "Yard",
            Location = "Location 1",
            MaxCapacity = 1000,
            ServesEntirePort = false,
            ServedDockIds = new List<string>()
        };
        await _client.PostAsJsonAsync("/api/StorageArea", firstDto);

        // Create second storage area
        var secondDto = new StorageAreaUpsertDto
        {
            AreaName = $"Second Area {guid}",
            AreaType = "Warehouse",
            Location = "Location 2",
            MaxCapacity = 800,
            ServesEntirePort = true,
            ServedDockIds = new List<string>()
        };
        var createResponse = await _client.PostAsJsonAsync("/api/StorageArea", secondDto);
        var created = await createResponse.Content.ReadFromJsonAsync<StorageAreaDto>();

        // Try to update second area with first area's name
        var updateDto = new StorageAreaUpsertDto
        {
            AreaName = existingName, // Duplicate name
            AreaType = "Yard",
            Location = "Updated location",
            MaxCapacity = 900,
            ServesEntirePort = false,
            ServedDockIds = new List<string>()
        };

        // Act
        var response = await _client.PutAsJsonAsync($"/api/StorageArea/{created!.Id}", updateDto);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
        var errorContent = await response.Content.ReadAsStringAsync();
        errorContent.Should().Contain("already exists");
    }

    [Fact]
    public async Task UpdateStorageArea_WithoutAuthentication_ShouldReturn401Unauthorized()
    {
        // Arrange - No token set
        var areaId = Guid.NewGuid();
        var updateDto = new StorageAreaUpsertDto
        {
            AreaName = "Unauthorized Update",
            AreaType = "Yard",
            Location = "Test location",
            MaxCapacity = 1000,
            ServesEntirePort = false,
            ServedDockIds = new List<string>()
        };

        // Act
        var response = await _client.PutAsJsonAsync($"/api/StorageArea/{areaId}", updateDto);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    #endregion

    #region Authorization Tests

    [Theory]
    [InlineData("ShippingAgentRepresentative")]
    [InlineData("LogisticOperator")]
    public async Task GetAll_WithAuthorizedRoles_ShouldReturn200Ok(string role)
    {
        // Arrange
        var token = AuthenticationHelper.GenerateToken(role);
        _client.SetAuthorizationHeader(token);

        // Act
        var response = await _client.GetAsync("/api/StorageArea");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
    }

    [Fact]
    public async Task GetAll_WithoutAuthentication_ShouldReturn401Unauthorized()
    {
        // Arrange - No token set

        // Act
        var response = await _client.GetAsync("/api/StorageArea");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    #endregion
}
