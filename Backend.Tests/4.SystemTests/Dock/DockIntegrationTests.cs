using System.Net;
using Backend.Tests.SystemTests.Infrastructure;
using FluentAssertions;
using ProjArqsi.Application.DTOs.Dock;
using Xunit;

namespace Backend.Tests.SystemTests.Dock;

/// <summary>
/// Integration tests for Dock endpoints.
/// Tests the complete flow: HTTP → Controller → Service → Repository → Database
/// </summary>
public class DockIntegrationTests : IClassFixture<CustomWebApplicationFactory>, IAsyncLifetime
{
    private readonly CustomWebApplicationFactory _factory;
    private readonly HttpClient _client;

    public DockIntegrationTests(CustomWebApplicationFactory factory)
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
    public async Task CreateDock_WithValidData_ShouldReturn201Created()
    {
        // Arrange
        var token = AuthenticationHelper.GenerateToken(roles: ["PortAuthorityOfficer"]);
        _client.SetAuthorizationHeader(token);

        var dto = new DockUpsertDto
        {
            DockName = "North Terminal Dock 1",
            LocationDescription = "North terminal, berth position 1",
            Length = 350.5,
            Depth = 15.0,
            MaxDraft = 12.5,
            AllowedVesselTypeIds = new List<string>()
        };

        // Act
        var response = await _client.PostAsJsonAsync("/api/Dock", dto);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.Created);
        response.Headers.Location.Should().NotBeNull();

        var result = await response.Content.ReadFromJsonAsync<DockDto>();
        result.Should().NotBeNull();
        result!.DockName.Should().Be(dto.DockName);
        result.LocationDescription.Should().Be(dto.LocationDescription);
        result.Length.Should().Be(dto.Length);
        result.Depth.Should().Be(dto.Depth);
        result.MaxDraft.Should().Be(dto.MaxDraft);
    }

    [Fact]
    public async Task CreateDock_WithDuplicateName_ShouldReturn400BadRequest()
    {
        // Arrange
        var token = AuthenticationHelper.GenerateToken("PortAuthorityOfficer");
        _client.SetAuthorizationHeader(token);

        var uniqueName = $"Terminal Dock {Guid.NewGuid()}";
        var dto = new DockUpsertDto
        {
            DockName = uniqueName,
            LocationDescription = "Main terminal area",
            Length = 300.0,
            Depth = 12.0,
            MaxDraft = 10.0,
            AllowedVesselTypeIds = new List<string>()
        };

        // Create first dock
        var firstResponse = await _client.PostAsJsonAsync("/api/Dock", dto);
        firstResponse.StatusCode.Should().Be(HttpStatusCode.Created);

        // Act - Try to create duplicate
        var secondResponse = await _client.PostAsJsonAsync("/api/Dock", dto);

        // Assert
        secondResponse.StatusCode.Should().Be(HttpStatusCode.BadRequest);
        var errorContent = await secondResponse.Content.ReadAsStringAsync();
        errorContent.Should().Contain("already exists");
    }

    [Fact]
    public async Task CreateDock_WithEmptyName_ShouldReturn400BadRequest()
    {
        // Arrange
        var token = AuthenticationHelper.GenerateToken("PortAuthorityOfficer");
        _client.SetAuthorizationHeader(token);

        var dto = new DockUpsertDto
        {
            DockName = "",
            LocationDescription = "Test location",
            Length = 200.0,
            Depth = 10.0,
            MaxDraft = 8.0,
            AllowedVesselTypeIds = new List<string>()
        };

        // Act
        var response = await _client.PostAsJsonAsync("/api/Dock", dto);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }

    [Fact]
    public async Task CreateDock_WithInvalidLength_ShouldReturn400BadRequest()
    {
        // Arrange
        var token = AuthenticationHelper.GenerateToken("PortAuthorityOfficer");
        _client.SetAuthorizationHeader(token);

        var dto = new DockUpsertDto
        {
            DockName = "Test Dock",
            LocationDescription = "Test location",
            Length = -50.0, // Invalid negative length
            Depth = 10.0,
            MaxDraft = 8.0,
            AllowedVesselTypeIds = new List<string>()
        };

        // Act
        var response = await _client.PostAsJsonAsync("/api/Dock", dto);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }

    [Fact]
    public async Task CreateDock_WithZeroLength_ShouldReturn400BadRequest()
    {
        // Arrange
        var token = AuthenticationHelper.GenerateToken("PortAuthorityOfficer");
        _client.SetAuthorizationHeader(token);

        var dto = new DockUpsertDto
        {
            DockName = "Test Dock Zero",
            LocationDescription = "Test location",
            Length = 0.0, // Invalid zero length
            Depth = 10.0,
            MaxDraft = 8.0,
            AllowedVesselTypeIds = new List<string>()
        };

        // Act
        var response = await _client.PostAsJsonAsync("/api/Dock", dto);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }

    [Fact]
    public async Task CreateDock_WithoutAuthentication_ShouldReturn401Unauthorized()
    {
        // Arrange - No token set
        var dto = new DockUpsertDto
        {
            DockName = "Unauthorized Dock",
            LocationDescription = "Test location",
            Length = 200.0,
            Depth = 10.0,
            MaxDraft = 8.0,
            AllowedVesselTypeIds = new List<string>()
        };

        // Act
        var response = await _client.PostAsJsonAsync("/api/Dock", dto);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    #endregion

    #region Get Tests

    [Fact]
    public async Task GetAll_ShouldReturnAllDocks()
    {
        // Arrange
        var token = AuthenticationHelper.GenerateToken("PortAuthorityOfficer");
        _client.SetAuthorizationHeader(token);

        var guid = Guid.NewGuid().ToString().Substring(0, 8);

        // Create multiple docks with unique names
        var docks = new[]
        {
            new DockUpsertDto { DockName = $"Dock Alpha {guid}", LocationDescription = "Location A", Length = 300.0, Depth = 12.0, MaxDraft = 10.0, AllowedVesselTypeIds = new List<string>() },
            new DockUpsertDto { DockName = $"Dock Beta {guid}", LocationDescription = "Location B", Length = 350.0, Depth = 15.0, MaxDraft = 12.0, AllowedVesselTypeIds = new List<string>() },
            new DockUpsertDto { DockName = $"Dock Gamma {guid}", LocationDescription = "Location C", Length = 250.0, Depth = 10.0, MaxDraft = 8.0, AllowedVesselTypeIds = new List<string>() }
        };

        foreach (var dto in docks)
        {
            var createResponse = await _client.PostAsJsonAsync("/api/Dock", dto);
            createResponse.StatusCode.Should().Be(HttpStatusCode.Created);
        }

        // Act
        var response = await _client.GetAsync("/api/Dock");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var result = await response.Content.ReadFromJsonAsync<List<DockDto>>();
        result.Should().NotBeNull();
        result!.Count.Should().BeGreaterOrEqualTo(3);
        result.Should().Contain(d => d.DockName == docks[0].DockName);
        result.Should().Contain(d => d.DockName == docks[1].DockName);
        result.Should().Contain(d => d.DockName == docks[2].DockName);
    }

    [Fact]
    public async Task GetById_WithExistingId_ShouldReturn200Ok()
    {
        // Arrange
        var token = AuthenticationHelper.GenerateToken(roles: ["PortAuthorityOfficer"]);
        _client.SetAuthorizationHeader(token);

        var createDto = new DockUpsertDto
        {
            DockName = "GetById Test Dock",
            LocationDescription = "Test location for GetById",
            Length = 280.0,
            Depth = 13.0,
            MaxDraft = 11.0,
            AllowedVesselTypeIds = new List<string>()
        };

        var createResponse = await _client.PostAsJsonAsync("/api/Dock", createDto);
        createResponse.StatusCode.Should().Be(HttpStatusCode.Created);
        var created = await createResponse.Content.ReadFromJsonAsync<DockDto>();
        created.Should().NotBeNull();

        // Act
        var response = await _client.GetAsync($"/api/Dock/{created!.Id}");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        
        var result = await response.Content.ReadFromJsonAsync<DockDto>();
        result.Should().NotBeNull();
        result!.Id.Should().Be(created.Id);
        result.DockName.Should().Be(createDto.DockName);
    }

    [Fact]
    public async Task GetById_WithNonExistentId_ShouldReturn404NotFound()
    {
        // Arrange
        var token = AuthenticationHelper.GenerateToken(roles: ["PortAuthorityOfficer"]);
        _client.SetAuthorizationHeader(token);
        var nonExistentId = Guid.NewGuid();

        // Act
        var response = await _client.GetAsync($"/api/Dock/{nonExistentId}");

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
        var response = await _client.GetAsync("/api/Dock/invalid-guid-format");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }

    #endregion

    #region Update Tests

    [Fact]
    public async Task UpdateDock_WithValidData_ShouldReturn200Ok()
    {
        // Arrange
        var token = AuthenticationHelper.GenerateToken(roles: ["PortAuthorityOfficer"]);
        _client.SetAuthorizationHeader(token);

        var createDto = new DockUpsertDto
        {
            DockName = "Original Dock Name",
            LocationDescription = "Original location",
            Length = 300.0,
            Depth = 12.0,
            MaxDraft = 10.0,
            AllowedVesselTypeIds = new List<string>()
        };

        var createResponse = await _client.PostAsJsonAsync("/api/Dock", createDto);
        var created = await createResponse.Content.ReadFromJsonAsync<DockDto>();

        var updateDto = new DockUpsertDto
        {
            DockName = "Updated Dock Name",
            LocationDescription = "Updated location description",
            Length = 350.0,
            Depth = 15.0,
            MaxDraft = 12.5,
            AllowedVesselTypeIds = new List<string>()
        };

        // Act
        var response = await _client.PutAsJsonAsync($"/api/Dock/{created!.Id}", updateDto);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        
        var result = await response.Content.ReadFromJsonAsync<DockDto>();
        result.Should().NotBeNull();
        result!.DockName.Should().Be(updateDto.DockName);
        result.LocationDescription.Should().Be(updateDto.LocationDescription);
        result.Length.Should().Be(updateDto.Length);
        result.Depth.Should().Be(updateDto.Depth);
        result.MaxDraft.Should().Be(updateDto.MaxDraft);
    }

    [Fact]
    public async Task UpdateDock_WithNonExistentId_ShouldReturn404NotFound()
    {
        // Arrange
        var token = AuthenticationHelper.GenerateToken(roles: ["PortAuthorityOfficer"]);
        _client.SetAuthorizationHeader(token);
        var nonExistentId = Guid.NewGuid();

        var updateDto = new DockUpsertDto
        {
            DockName = "Non-existent Dock",
            LocationDescription = "Some location",
            Length = 300.0,
            Depth = 12.0,
            MaxDraft = 10.0,
            AllowedVesselTypeIds = new List<string>()
        };

        // Act
        var response = await _client.PutAsJsonAsync($"/api/Dock/{nonExistentId}", updateDto);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }

    [Fact]
    public async Task UpdateDock_WithInvalidData_ShouldReturn400BadRequest()
    {
        // Arrange
        var token = AuthenticationHelper.GenerateToken(roles: ["PortAuthorityOfficer"]);
        _client.SetAuthorizationHeader(token);

        var createDto = new DockUpsertDto
        {
            DockName = "Test Dock for Update",
            LocationDescription = "Test location",
            Length = 300.0,
            Depth = 12.0,
            MaxDraft = 10.0,
            AllowedVesselTypeIds = new List<string>()
        };

        var createResponse = await _client.PostAsJsonAsync("/api/Dock", createDto);
        var created = await createResponse.Content.ReadFromJsonAsync<DockDto>();

        var updateDto = new DockUpsertDto
        {
            DockName = "Updated Name",
            LocationDescription = "Updated location",
            Length = -100.0, // Invalid negative length
            Depth = 12.0,
            MaxDraft = 10.0,
            AllowedVesselTypeIds = new List<string>()
        };

        // Act
        var response = await _client.PutAsJsonAsync($"/api/Dock/{created!.Id}", updateDto);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }

    [Fact]
    public async Task UpdateDock_WithDuplicateName_ShouldReturn400BadRequest()
    {
        // Arrange
        var token = AuthenticationHelper.GenerateToken(roles: ["PortAuthorityOfficer"]);
        _client.SetAuthorizationHeader(token);

        var guid = Guid.NewGuid().ToString().Substring(0, 8);
        var existingName = $"Existing Dock {guid}";

        // Create first dock
        var firstDto = new DockUpsertDto
        {
            DockName = existingName,
            LocationDescription = "Location 1",
            Length = 300.0,
            Depth = 12.0,
            MaxDraft = 10.0,
            AllowedVesselTypeIds = new List<string>()
        };
        await _client.PostAsJsonAsync("/api/Dock", firstDto);

        // Create second dock
        var secondDto = new DockUpsertDto
        {
            DockName = $"Second Dock {guid}",
            LocationDescription = "Location 2",
            Length = 250.0,
            Depth = 10.0,
            MaxDraft = 8.0,
            AllowedVesselTypeIds = new List<string>()
        };
        var createResponse = await _client.PostAsJsonAsync("/api/Dock", secondDto);
        var created = await createResponse.Content.ReadFromJsonAsync<DockDto>();

        // Try to update second dock with first dock's name
        var updateDto = new DockUpsertDto
        {
            DockName = existingName, // Duplicate name
            LocationDescription = "Updated location",
            Length = 280.0,
            Depth = 11.0,
            MaxDraft = 9.0,
            AllowedVesselTypeIds = new List<string>()
        };

        // Act
        var response = await _client.PutAsJsonAsync($"/api/Dock/{created!.Id}", updateDto);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
        var errorContent = await response.Content.ReadAsStringAsync();
        errorContent.Should().Contain("already exists");
    }

    [Fact]
    public async Task UpdateDock_WithoutAuthentication_ShouldReturn401Unauthorized()
    {
        // Arrange - No token set
        var dockId = Guid.NewGuid();
        var updateDto = new DockUpsertDto
        {
            DockName = "Unauthorized Update",
            LocationDescription = "Test location",
            Length = 300.0,
            Depth = 12.0,
            MaxDraft = 10.0,
            AllowedVesselTypeIds = new List<string>()
        };

        // Act
        var response = await _client.PutAsJsonAsync($"/api/Dock/{dockId}", updateDto);

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
        var response = await _client.GetAsync("/api/Dock");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
    }

    [Fact]
    public async Task GetAll_WithoutAuthentication_ShouldReturn401Unauthorized()
    {
        // Arrange - No token set

        // Act
        var response = await _client.GetAsync("/api/Dock");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    #endregion
}
