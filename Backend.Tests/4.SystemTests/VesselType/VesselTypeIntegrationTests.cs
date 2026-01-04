using System.Net;
using System.Net.Http.Json;
using Backend.Tests.SystemTests.Infrastructure;
using FluentAssertions;
using ProjArqsi.Application.DTOs;

namespace Backend.Tests.SystemTests.VesselType;

/// <summary>
/// Integration tests for VesselType endpoints (E2E: Controller + Service + Repository + DB).
/// Tests US 2.2.1 - Create and Update Vessel Types.
/// </summary>
public class VesselTypeIntegrationTests : IClassFixture<CustomWebApplicationFactory>, IAsyncLifetime
{
    private readonly HttpClient _client;
    private readonly CustomWebApplicationFactory _factory;

    public VesselTypeIntegrationTests(CustomWebApplicationFactory factory)
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
    public async Task CreateVesselType_WithValidData_ShouldReturn201Created()
    {
        // Arrange
        var token = AuthenticationHelper.GenerateToken("PortAuthorityOfficer");
        _client.SetAuthorizationHeader(token);

        var dto = new VesselTypeUpsertDto
        {
            TypeName = "Container Ship",
            TypeDescription = "Large container vessel for international cargo",
            TypeCapacity = 5000,
            MaxRows = 10,
            MaxBays = 20,
            MaxTiers = 8
        };

        // Act
        var response = await _client.PostAsJsonAsync("/api/VesselType", dto);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.Created);

        var result = await response.Content.ReadFromJsonAsync<VesselTypeDto>();
        result.Should().NotBeNull();
        result!.TypeName.Should().Be("Container Ship");
        result.TypeDescription.Should().Be("Large container vessel for international cargo");
        result.TypeCapacity.Should().Be(5000);
        result.MaxRows.Should().Be(10);
        result.MaxBays.Should().Be(20);
        result.MaxTiers.Should().Be(8);
        result.Id.Should().NotBeNullOrEmpty();

        // Verify Location header
        response.Headers.Location.Should().NotBeNull();
        response.Headers.Location!.ToString().Should().Contain($"/api/VesselType/{result.Id}");
    }

    [Fact]
    public async Task CreateVesselType_WithDuplicateName_ShouldReturn409Conflict()
    {
        // Arrange
        var token = AuthenticationHelper.GenerateToken("PortAuthorityOfficer");
        _client.SetAuthorizationHeader(token);

        var uniqueName = $"Bulk Carrier {Guid.NewGuid()}";
        var dto = new VesselTypeUpsertDto
        {
            TypeName = uniqueName,
            TypeDescription = "Dry bulk cargo vessel",
            TypeCapacity = 3000,
            MaxRows = 8,
            MaxBays = 16,
            MaxTiers = 6
        };

        // Create first vessel type
        var firstResponse = await _client.PostAsJsonAsync("/api/VesselType", dto);
        firstResponse.StatusCode.Should().Be(HttpStatusCode.Created);

        // Act - Try to create duplicate
        var secondResponse = await _client.PostAsJsonAsync("/api/VesselType", dto);

        // Assert
        secondResponse.StatusCode.Should().Be(HttpStatusCode.BadRequest);
        var errorContent = await secondResponse.Content.ReadAsStringAsync();
        errorContent.Should().Contain("already exists");
    }

    [Fact]
    public async Task CreateVesselType_WithInvalidCapacity_ShouldReturn400BadRequest()
    {
        // Arrange
        var token = AuthenticationHelper.GenerateToken("PortAuthorityOfficer");
        _client.SetAuthorizationHeader(token);

        var dto = new VesselTypeUpsertDto
        {
            TypeName = "Invalid Vessel",
            TypeDescription = "Test vessel with invalid data",
            TypeCapacity = -100, // Invalid: negative capacity
            MaxRows = 10,
            MaxBays = 20,
            MaxTiers = 8
        };

        // Act
        var response = await _client.PostAsJsonAsync("/api/VesselType", dto);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }

    [Fact]
    public async Task CreateVesselType_WithInvalidDimensions_ShouldReturn400BadRequest()
    {
        // Arrange
        var token = AuthenticationHelper.GenerateToken("PortAuthorityOfficer");
        _client.SetAuthorizationHeader(token);

        var dto = new VesselTypeUpsertDto
        {
            TypeName = "Invalid Dimensions",
            TypeDescription = "Test vessel with invalid dimensions",
            TypeCapacity = 5000,
            MaxRows = 0, // Invalid: must be > 0
            MaxBays = -5, // Invalid: negative
            MaxTiers = 8
        };

        // Act
        var response = await _client.PostAsJsonAsync("/api/VesselType", dto);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }

    #endregion

    #region Get Tests

    [Fact]
    public async Task GetById_WithExistingId_ShouldReturn200Ok()
    {
        // Arrange
        var token = AuthenticationHelper.GenerateToken("PortAuthorityOfficer");
        _client.SetAuthorizationHeader(token);

        // Create a vessel type first
        var createDto = new VesselTypeUpsertDto
        {
            TypeName = "Tanker",
            TypeDescription = "Oil tanker vessel",
            TypeCapacity = 4000,
            MaxRows = 9,
            MaxBays = 18,
            MaxTiers = 7
        };

        var createResponse = await _client.PostAsJsonAsync("/api/VesselType", createDto);
        var created = await createResponse.Content.ReadFromJsonAsync<VesselTypeDto>();

        // Act
        var response = await _client.GetAsync($"/api/VesselType/{created!.Id}");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var result = await response.Content.ReadFromJsonAsync<VesselTypeDto>();
        result.Should().NotBeNull();
        result!.Id.Should().Be(created.Id);
        result.TypeName.Should().Be("Tanker");
    }

    [Fact]
    public async Task GetById_WithNonExistentId_ShouldReturn404NotFound()
    {
        // Arrange
        var token = AuthenticationHelper.GenerateToken("PortAuthorityOfficer");
        _client.SetAuthorizationHeader(token);
        var nonExistentId = Guid.NewGuid();

        // Act
        var response = await _client.GetAsync($"/api/VesselType/{nonExistentId}");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }

    [Fact]
    public async Task GetAll_ShouldReturnAllVesselTypes()
    {
        // Arrange
        var token = AuthenticationHelper.GenerateToken("PortAuthorityOfficer");
        _client.SetAuthorizationHeader(token);

        var guid = Guid.NewGuid().ToString().Substring(0, 8);

        // Create multiple vessel types with unique names
        var vesselTypes = new[]
        {
            new VesselTypeUpsertDto { TypeName = $"Container Ship {guid}-1", TypeDescription = "Type 1", TypeCapacity = 5000, MaxRows = 10, MaxBays = 20, MaxTiers = 8 },
            new VesselTypeUpsertDto { TypeName = $"Container Ship {guid}-2", TypeDescription = "Type 2", TypeCapacity = 6000, MaxRows = 12, MaxBays = 22, MaxTiers = 9 },
            new VesselTypeUpsertDto { TypeName = $"Bulk Carrier {guid}-1", TypeDescription = "Type 3", TypeCapacity = 4000, MaxRows = 8, MaxBays = 16, MaxTiers = 7 }
        };

        foreach (var dto in vesselTypes)
        {
            var createResponse = await _client.PostAsJsonAsync("/api/VesselType", dto);
            createResponse.StatusCode.Should().Be(HttpStatusCode.Created);
        }

        // Act
        var response = await _client.GetAsync("/api/VesselType");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var result = await response.Content.ReadFromJsonAsync<List<VesselTypeDto>>();
        result.Should().NotBeNull();
        result!.Count.Should().BeGreaterOrEqualTo(3);
        result.Should().Contain(vt => vt.TypeName == vesselTypes[0].TypeName);
        result.Should().Contain(vt => vt.TypeName == vesselTypes[1].TypeName);
        result.Should().Contain(vt => vt.TypeName == vesselTypes[2].TypeName);
    }

    #endregion

    #region Search Tests

    [Fact]
    public async Task Search_ByName_ShouldReturnMatchingVesselTypes()
    {
        // Arrange
        var token = AuthenticationHelper.GenerateToken("PortAuthorityOfficer");
        _client.SetAuthorizationHeader(token);

        var guid = Guid.NewGuid().ToString().Substring(0, 8);
        var searchTerm = $"TestContainer{guid}";

        // Create vessel types with different names
        var response1 = await _client.PostAsJsonAsync("/api/VesselType", new VesselTypeUpsertDto
        {
            TypeName = $"{searchTerm} Alpha",
            TypeDescription = "Description A",
            TypeCapacity = 5000,
            MaxRows = 10,
            MaxBays = 20,
            MaxTiers = 8
        });
        response1.StatusCode.Should().Be(HttpStatusCode.Created);

        var response2 = await _client.PostAsJsonAsync("/api/VesselType", new VesselTypeUpsertDto
        {
            TypeName = $"{searchTerm} Beta",
            TypeDescription = "Description B",
            TypeCapacity = 6000,
            MaxRows = 12,
            MaxBays = 22,
            MaxTiers = 9
        });
        response2.StatusCode.Should().Be(HttpStatusCode.Created);

        await _client.PostAsJsonAsync("/api/VesselType", new VesselTypeUpsertDto
        {
            TypeName = "Tanker Gamma",
            TypeDescription = "Description C",
            TypeCapacity = 4000,
            MaxRows = 8,
            MaxBays = 16,
            MaxTiers = 7
        });

        // Act - Search for unique term
        var response = await _client.GetAsync($"/api/VesselType/search?searchTerm={searchTerm}");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var result = await response.Content.ReadFromJsonAsync<List<VesselTypeDto>>();
        result.Should().NotBeNull();
        result!.Count.Should().Be(2);
        result.Should().AllSatisfy(vt => vt.TypeName.Should().Contain(searchTerm));
    }

    [Fact]
    public async Task Search_ByDescription_ShouldReturnMatchingVesselTypes()
    {
        // Arrange
        var token = AuthenticationHelper.GenerateToken("PortAuthorityOfficer");
        _client.SetAuthorizationHeader(token);

        var guid = Guid.NewGuid().ToString().Substring(0, 8);
        var searchTerm = $"TestCargo{guid}";

        var response1 = await _client.PostAsJsonAsync("/api/VesselType", new VesselTypeUpsertDto
        {
            TypeName = $"Type A {guid}",
            TypeDescription = $"Large {searchTerm} vessel for international trade",
            TypeCapacity = 5000,
            MaxRows = 10,
            MaxBays = 20,
            MaxTiers = 8
        });
        response1.StatusCode.Should().Be(HttpStatusCode.Created);

        var response2 = await _client.PostAsJsonAsync("/api/VesselType", new VesselTypeUpsertDto
        {
            TypeName = $"Type B {guid}",
            TypeDescription = $"Small coastal {searchTerm} ship",
            TypeCapacity = 2000,
            MaxRows = 6,
            MaxBays = 12,
            MaxTiers = 5
        });
        response2.StatusCode.Should().Be(HttpStatusCode.Created);

        // Act - Search for unique term
        var response = await _client.GetAsync($"/api/VesselType/search?searchTerm={searchTerm}");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var result = await response.Content.ReadFromJsonAsync<List<VesselTypeDto>>();
        result.Should().NotBeNull();
        result!.Count.Should().Be(2);
        result.Should().AllSatisfy(vt => vt.TypeDescription.Should().Contain(searchTerm));
    }

    [Fact]
    public async Task Search_WithEmptyTerm_ShouldReturn400BadRequest()
    {
        // Arrange
        var token = AuthenticationHelper.GenerateToken("PortAuthorityOfficer");
        _client.SetAuthorizationHeader(token);

        // Act
        var response = await _client.GetAsync("/api/VesselType/search?searchTerm=");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }

    [Fact]
    public async Task Search_WithNoMatches_ShouldReturnEmptyList()
    {
        // Arrange
        var token = AuthenticationHelper.GenerateToken("PortAuthorityOfficer");
        _client.SetAuthorizationHeader(token);

        await _client.PostAsJsonAsync("/api/VesselType", new VesselTypeUpsertDto
        {
            TypeName = "Container Ship",
            TypeDescription = "Cargo vessel",
            TypeCapacity = 5000,
            MaxRows = 10,
            MaxBays = 20,
            MaxTiers = 8
        });

        // Act - Search for non-existent term
        var response = await _client.GetAsync("/api/VesselType/search?searchTerm=Submarine");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var result = await response.Content.ReadFromJsonAsync<List<VesselTypeDto>>();
        result.Should().NotBeNull();
        result!.Should().BeEmpty();
    }

    #endregion

    #region Update Tests

    [Fact]
    public async Task UpdateVesselType_WithValidData_ShouldReturn200Ok()
    {
        // Arrange
        var token = AuthenticationHelper.GenerateToken("PortAuthorityOfficer");
        _client.SetAuthorizationHeader(token);

        // Create vessel type first
        var createDto = new VesselTypeUpsertDto
        {
            TypeName = "Original Name",
            TypeDescription = "Original Description",
            TypeCapacity = 5000,
            MaxRows = 10,
            MaxBays = 20,
            MaxTiers = 8
        };

        var createResponse = await _client.PostAsJsonAsync("/api/VesselType", createDto);
        var created = await createResponse.Content.ReadFromJsonAsync<VesselTypeDto>();

        // Prepare update
        var updateDto = new VesselTypeUpsertDto
        {
            TypeName = "Updated Name",
            TypeDescription = "Updated Description",
            TypeCapacity = 6000,
            MaxRows = 12,
            MaxBays = 24,
            MaxTiers = 10
        };

        // Act
        var response = await _client.PutAsJsonAsync($"/api/VesselType/{created!.Id}", updateDto);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var result = await response.Content.ReadFromJsonAsync<VesselTypeDto>();
        result.Should().NotBeNull();
        result!.Id.Should().Be(created.Id);
        result.TypeName.Should().Be("Updated Name");
        result.TypeDescription.Should().Be("Updated Description");
        result.TypeCapacity.Should().Be(6000);
        result.MaxRows.Should().Be(12);
        result.MaxBays.Should().Be(24);
        result.MaxTiers.Should().Be(10);
    }

    [Fact]
    public async Task UpdateVesselType_WithNonExistentId_ShouldReturn404NotFound()
    {
        // Arrange
        var token = AuthenticationHelper.GenerateToken("PortAuthorityOfficer");
        _client.SetAuthorizationHeader(token);
        var nonExistentId = Guid.NewGuid();

        var updateDto = new VesselTypeUpsertDto
        {
            TypeName = "Updated Name",
            TypeDescription = "Updated Description",
            TypeCapacity = 6000,
            MaxRows = 12,
            MaxBays = 24,
            MaxTiers = 10
        };

        // Act
        var response = await _client.PutAsJsonAsync($"/api/VesselType/{nonExistentId}", updateDto);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }

    [Fact]
    public async Task UpdateVesselType_WithDuplicateName_ShouldReturn400BadRequest()
    {
        // Arrange
        var token = AuthenticationHelper.GenerateToken("PortAuthorityOfficer");
        _client.SetAuthorizationHeader(token);

        // Create two vessel types
        var createResponse1 = await _client.PostAsJsonAsync("/api/VesselType", new VesselTypeUpsertDto
        {
            TypeName = "Type One",
            TypeDescription = "Description 1",
            TypeCapacity = 5000,
            MaxRows = 10,
            MaxBays = 20,
            MaxTiers = 8
        });
        var type1 = await createResponse1.Content.ReadFromJsonAsync<VesselTypeDto>();

        await _client.PostAsJsonAsync("/api/VesselType", new VesselTypeUpsertDto
        {
            TypeName = "Type Two",
            TypeDescription = "Description 2",
            TypeCapacity = 6000,
            MaxRows = 12,
            MaxBays = 22,
            MaxTiers = 9
        });

        // Act - Try to update type1 with type2's name
        var updateDto = new VesselTypeUpsertDto
        {
            TypeName = "Type Two", // Duplicate name
            TypeDescription = "Updated Description",
            TypeCapacity = 7000,
            MaxRows = 14,
            MaxBays = 26,
            MaxTiers = 11
        };

        var response = await _client.PutAsJsonAsync($"/api/VesselType/{type1!.Id}", updateDto);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
        var errorContent = await response.Content.ReadAsStringAsync();
        errorContent.Should().Contain("already exists");
    }

    #endregion

    #region Authorization Tests (RBAC)

    [Fact]
    public async Task CreateVesselType_WithoutToken_ShouldReturn401Unauthorized()
    {
        // Arrange - No authentication token
        _client.ClearAuthorizationHeader();

        var dto = new VesselTypeUpsertDto
        {
            TypeName = "Test Type",
            TypeDescription = "Test Description",
            TypeCapacity = 5000,
            MaxRows = 10,
            MaxBays = 20,
            MaxTiers = 8
        };

        // Act
        var response = await _client.PostAsJsonAsync("/api/VesselType", dto);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    [Fact]
    public async Task CreateVesselType_WithWrongRole_ShouldReturn403Forbidden()
    {
        // Arrange - User with ShippingAgent role (not PortAuthorityOfficer)
        var token = AuthenticationHelper.GenerateToken("ShippingAgent");
        _client.SetAuthorizationHeader(token);

        var dto = new VesselTypeUpsertDto
        {
            TypeName = "Test Type",
            TypeDescription = "Test Description",
            TypeCapacity = 5000,
            MaxRows = 10,
            MaxBays = 20,
            MaxTiers = 8
        };

        // Act
        var response = await _client.PostAsJsonAsync("/api/VesselType", dto);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.Forbidden);
    }

    [Fact]
    public async Task GetAll_WithAdminRole_ShouldReturn200Ok()
    {
        // Arrange - Admin has access to GetAll
        var token = AuthenticationHelper.GenerateToken("Admin");
        _client.SetAuthorizationHeader(token);

        // Act
        var response = await _client.GetAsync("/api/VesselType");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
    }

    [Fact]
    public async Task UpdateVesselType_WithWrongRole_ShouldReturn403Forbidden()
    {
        // Arrange
        var portAuthorityToken = AuthenticationHelper.GenerateToken("PortAuthorityOfficer");
        _client.SetAuthorizationHeader(portAuthorityToken);

        // Create vessel type
        var createDto = new VesselTypeUpsertDto
        {
            TypeName = "Test Type",
            TypeDescription = "Test Description",
            TypeCapacity = 5000,
            MaxRows = 10,
            MaxBays = 20,
            MaxTiers = 8
        };
        var createResponse = await _client.PostAsJsonAsync("/api/VesselType", createDto);
        var created = await createResponse.Content.ReadFromJsonAsync<VesselTypeDto>();

        // Switch to LogisticsOperator role (wrong role for update)
        var wrongRoleToken = AuthenticationHelper.GenerateToken("LogisticsOperator");
        _client.SetAuthorizationHeader(wrongRoleToken);

        var updateDto = new VesselTypeUpsertDto
        {
            TypeName = "Updated Name",
            TypeDescription = "Updated Description",
            TypeCapacity = 6000,
            MaxRows = 12,
            MaxBays = 24,
            MaxTiers = 10
        };

        // Act
        var response = await _client.PutAsJsonAsync($"/api/VesselType/{created!.Id}", updateDto);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.Forbidden);
    }

    #endregion
}
