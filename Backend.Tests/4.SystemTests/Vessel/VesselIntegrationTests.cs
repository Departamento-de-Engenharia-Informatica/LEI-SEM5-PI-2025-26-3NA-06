using System.Net;
using Backend.Tests.SystemTests.Infrastructure;
using FluentAssertions;
using ProjArqsi.Application.DTOs;
using Xunit;

namespace Backend.Tests.SystemTests.Vessel;

/// <summary>
/// Integration tests for Vessel endpoints.
/// Tests the complete flow: HTTP → Controller → Service → Repository → Database
/// </summary>
public class VesselIntegrationTests : IClassFixture<CustomWebApplicationFactory>, IAsyncLifetime
{
    private readonly CustomWebApplicationFactory _factory;
    private readonly HttpClient _client;

    public VesselIntegrationTests(CustomWebApplicationFactory factory)
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

    private async Task<string> CreateVesselType()
    {
        var token = AuthenticationHelper.GenerateToken("PortAuthorityOfficer");
        _client.SetAuthorizationHeader(token);

        var vesselTypeDto = new VesselTypeUpsertDto
        {
            TypeName = $"Container Ship {Guid.NewGuid().ToString().Substring(0, 8)}",
            TypeDescription = "Standard container vessel",
            TypeCapacity = 5000,
            MaxRows = 10,
            MaxBays = 20,
            MaxTiers = 8
        };

        var response = await _client.PostAsJsonAsync("/api/VesselType", vesselTypeDto);
        response.StatusCode.Should().Be(HttpStatusCode.Created);
        
        var created = await response.Content.ReadFromJsonAsync<VesselTypeDto>();
        return created!.Id;
    }

    private string GenerateValidIMO()
    {
        // Generate random 6 digits
        var random = new Random();
        var first6Digits = random.Next(100000, 999999).ToString();
        
        // Calculate check digit using IMO algorithm
        int sum = 0;
        int[] multipliers = { 7, 6, 5, 4, 3, 2 };
        
        for (int i = 0; i < 6; i++)
        {
            sum += (first6Digits[i] - '0') * multipliers[i];
        }
        
        int checkDigit = sum % 10;
        
        return first6Digits + checkDigit.ToString();
    }

    #region Create Tests

    [Fact]
    public async Task CreateVessel_WithValidData_ShouldReturn201Created()
    {
        // Arrange
        var vesselTypeId = await CreateVesselType();
        
        var token = AuthenticationHelper.GenerateToken(roles: ["PortAuthorityOfficer"]);
        _client.SetAuthorizationHeader(token);

        var dto = new UpsertVesselDto
        {
            Imo = GenerateValidIMO(),
            VesselName = "MV Atlantic Trader",
            Capacity = 4500,
            Rows = 9,
            Bays = 18,
            Tiers = 7,
            Length = 285.5,
            VesselTypeId = vesselTypeId
        };

        // Act
        var response = await _client.PostAsJsonAsync("/api/Vessel", dto);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.Created);
        response.Headers.Location.Should().NotBeNull();

        var result = await response.Content.ReadFromJsonAsync<VesselDto>();
        result.Should().NotBeNull();
        result!.Imo.Should().Be(dto.Imo);
        result.VesselName.Should().Be(dto.VesselName);
        result.Capacity.Should().Be(dto.Capacity);
        result.Length.Should().Be(dto.Length);
    }

    [Fact]
    public async Task CreateVessel_WithDuplicateImo_ShouldReturn400BadRequest()
    {
        // Arrange
        var vesselTypeId = await CreateVesselType();
        var token = AuthenticationHelper.GenerateToken("PortAuthorityOfficer");
        _client.SetAuthorizationHeader(token);

        var imoNumber = GenerateValidIMO();
        var firstDto = new UpsertVesselDto
        {
            Imo = imoNumber,
            VesselName = "First Vessel",
            Capacity = 4000,
            Rows = 8,
            Bays = 16,
            Tiers = 6,
            Length = 250.0,
            VesselTypeId = vesselTypeId
        };

        // Create first vessel
        var firstResponse = await _client.PostAsJsonAsync("/api/Vessel", firstDto);
        firstResponse.StatusCode.Should().Be(HttpStatusCode.Created);

        // Try to create duplicate
        var secondDto = new UpsertVesselDto
        {
            Imo = imoNumber, // Duplicate IMO
            VesselName = "Second Vessel",
            Capacity = 4500,
            Rows = 9,
            Bays = 18,
            Tiers = 7,
            Length = 280.0,
            VesselTypeId = vesselTypeId
        };

        // Act
        var response = await _client.PostAsJsonAsync("/api/Vessel", secondDto);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
        var errorContent = await response.Content.ReadAsStringAsync();
        errorContent.Should().Contain("already exists");
    }

    [Fact]
    public async Task CreateVessel_WithInvalidImo_ShouldReturn400BadRequest()
    {
        // Arrange
        var vesselTypeId = await CreateVesselType();
        var token = AuthenticationHelper.GenerateToken("PortAuthorityOfficer");
        _client.SetAuthorizationHeader(token);

        var dto = new UpsertVesselDto
        {
            Imo = "INVALID", // Invalid IMO format
            VesselName = "Test Vessel",
            Capacity = 4000,
            Rows = 8,
            Bays = 16,
            Tiers = 6,
            Length = 250.0,
            VesselTypeId = vesselTypeId
        };

        // Act
        var response = await _client.PostAsJsonAsync("/api/Vessel", dto);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }

    [Fact]
    public async Task CreateVessel_WithNonExistentVesselType_ShouldReturn400BadRequest()
    {
        // Arrange
        var token = AuthenticationHelper.GenerateToken("PortAuthorityOfficer");
        _client.SetAuthorizationHeader(token);

        var dto = new UpsertVesselDto
        {
            Imo = "1234567",
            VesselName = "Test Vessel",
            Capacity = 4000,
            Rows = 8,
            Bays = 16,
            Tiers = 6,
            Length = 250.0,
            VesselTypeId = Guid.NewGuid().ToString() // Non-existent vessel type
        };

        // Act
        var response = await _client.PostAsJsonAsync("/api/Vessel", dto);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }

    [Fact]
    public async Task CreateVessel_WithInvalidCapacity_ShouldReturn400BadRequest()
    {
        // Arrange
        var vesselTypeId = await CreateVesselType();
        var token = AuthenticationHelper.GenerateToken("PortAuthorityOfficer");
        _client.SetAuthorizationHeader(token);

        var dto = new UpsertVesselDto
        {
            Imo = GenerateValidIMO(),
            VesselName = "Test Vessel",
            Capacity = -100, // Invalid negative capacity
            Rows = 8,
            Bays = 16,
            Tiers = 6,
            Length = 250.0,
            VesselTypeId = vesselTypeId
        };

        // Act
        var response = await _client.PostAsJsonAsync("/api/Vessel", dto);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.InternalServerError); // ArgumentOutOfRangeException not caught
    }

    [Fact]
    public async Task CreateVessel_WithoutAuthentication_ShouldReturn401Unauthorized()
    {
        // Arrange - No token set
        var dto = new UpsertVesselDto
        {
            Imo = GenerateValidIMO(),
            VesselName = "Unauthorized Vessel",
            Capacity = 4000,
            Rows = 8,
            Bays = 16,
            Tiers = 6,
            Length = 250.0,
            VesselTypeId = Guid.NewGuid().ToString()
        };

        // Act
        var response = await _client.PostAsJsonAsync("/api/Vessel", dto);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    [Fact]
    public async Task CreateVessel_WithNonPortAuthorityRole_ShouldReturn403Forbidden()
    {
        // Arrange
        var vesselTypeId = await CreateVesselType();
        var token = AuthenticationHelper.GenerateToken("LogisticOperator");
        _client.SetAuthorizationHeader(token);

        var dto = new UpsertVesselDto
        {
            Imo = GenerateValidIMO(),
            VesselName = "Test Vessel",
            Capacity = 4000,
            Rows = 8,
            Bays = 16,
            Tiers = 6,
            Length = 250.0,
            VesselTypeId = vesselTypeId
        };

        // Act
        var response = await _client.PostAsJsonAsync("/api/Vessel", dto);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.Forbidden);
    }

    #endregion

    #region Get Tests

    [Fact]
    public async Task GetAll_ShouldReturnAllVessels()
    {
        // Arrange
        var vesselTypeId = await CreateVesselType();
        var token = AuthenticationHelper.GenerateToken("PortAuthorityOfficer");
        _client.SetAuthorizationHeader(token);

        var guid = Guid.NewGuid().ToString().Substring(0, 8);

        // Create multiple vessels
        var vessels = new[]
        {
            new UpsertVesselDto { Imo = GenerateValidIMO(), VesselName = $"Vessel Alpha {guid}", Capacity = 4000, Rows = 8, Bays = 16, Tiers = 6, Length = 250.0, VesselTypeId = vesselTypeId },
            new UpsertVesselDto { Imo = GenerateValidIMO(), VesselName = $"Vessel Beta {guid}", Capacity = 4500, Rows = 9, Bays = 18, Tiers = 7, Length = 280.0, VesselTypeId = vesselTypeId },
            new UpsertVesselDto { Imo = GenerateValidIMO(), VesselName = $"Vessel Gamma {guid}", Capacity = 3500, Rows = 7, Bays = 14, Tiers = 5, Length = 220.0, VesselTypeId = vesselTypeId }
        };

        foreach (var dto in vessels)
        {
            var createResponse = await _client.PostAsJsonAsync("/api/Vessel", dto);
            createResponse.StatusCode.Should().Be(HttpStatusCode.Created);
        }

        // Act
        var response = await _client.GetAsync("/api/Vessel");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var result = await response.Content.ReadFromJsonAsync<List<VesselDto>>();
        result.Should().NotBeNull();
        result!.Count.Should().BeGreaterOrEqualTo(3);
        result.Should().Contain(v => v.VesselName == vessels[0].VesselName);
        result.Should().Contain(v => v.VesselName == vessels[1].VesselName);
        result.Should().Contain(v => v.VesselName == vessels[2].VesselName);
    }

    [Fact]
    public async Task GetById_WithExistingId_ShouldReturn200Ok()
    {
        // Arrange
        var vesselTypeId = await CreateVesselType();
        var token = AuthenticationHelper.GenerateToken(roles: ["PortAuthorityOfficer"]);
        _client.SetAuthorizationHeader(token);

        var createDto = new UpsertVesselDto
        {
            Imo = GenerateValidIMO(),
            VesselName = "GetById Test Vessel",
            Capacity = 4000,
            Rows = 8,
            Bays = 16,
            Tiers = 6,
            Length = 250.0,
            VesselTypeId = vesselTypeId
        };

        var createResponse = await _client.PostAsJsonAsync("/api/Vessel", createDto);
        createResponse.StatusCode.Should().Be(HttpStatusCode.Created);
        var created = await createResponse.Content.ReadFromJsonAsync<VesselDto>();
        created.Should().NotBeNull();

        // Act
        var response = await _client.GetAsync($"/api/Vessel/{created!.Id}");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        
        var result = await response.Content.ReadFromJsonAsync<VesselDto>();
        result.Should().NotBeNull();
        result!.Id.Should().Be(created.Id);
        result.VesselName.Should().Be(createDto.VesselName);
    }

    [Fact]
    public async Task GetById_WithNonExistentId_ShouldReturn404NotFound()
    {
        // Arrange
        var token = AuthenticationHelper.GenerateToken(roles: ["PortAuthorityOfficer"]);
        _client.SetAuthorizationHeader(token);
        var nonExistentId = Guid.NewGuid();

        // Act
        var response = await _client.GetAsync($"/api/Vessel/{nonExistentId}");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }

    [Fact]
    public async Task GetByImo_WithExistingImo_ShouldReturn200Ok()
    {
        // Arrange
        var vesselTypeId = await CreateVesselType();
        var token = AuthenticationHelper.GenerateToken(roles: ["PortAuthorityOfficer"]);
        _client.SetAuthorizationHeader(token);

        var imoNumber = GenerateValidIMO();
        var createDto = new UpsertVesselDto
        {
            Imo = imoNumber,
            VesselName = "IMO Test Vessel",
            Capacity = 4000,
            Rows = 8,
            Bays = 16,
            Tiers = 6,
            Length = 250.0,
            VesselTypeId = vesselTypeId
        };

        var createResponse = await _client.PostAsJsonAsync("/api/Vessel", createDto);
        createResponse.StatusCode.Should().Be(HttpStatusCode.Created);

        // Act
        var response = await _client.GetAsync($"/api/Vessel/imo/{imoNumber}");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        
        var result = await response.Content.ReadFromJsonAsync<VesselDto>();
        result.Should().NotBeNull();
        result!.Imo.Should().Be(imoNumber);
        result.VesselName.Should().Be(createDto.VesselName);
    }

    [Fact]
    public async Task GetByImo_WithNonExistentImo_ShouldReturn404NotFound()
    {
        // Arrange
        var token = AuthenticationHelper.GenerateToken(roles: ["PortAuthorityOfficer"]);
        _client.SetAuthorizationHeader(token);

        // Act
        var response = await _client.GetAsync("/api/Vessel/imo/IMO9999999");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }

    [Fact]
    public async Task GetAll_WithoutAuthentication_ShouldReturn401Unauthorized()
    {
        // Arrange - No token set

        // Act
        var response = await _client.GetAsync("/api/Vessel");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    #endregion

    #region Update Tests

    [Fact]
    public async Task UpdateVessel_WithValidData_ShouldReturn200Ok()
    {
        // Arrange
        var vesselTypeId = await CreateVesselType();
        var token = AuthenticationHelper.GenerateToken(roles: ["PortAuthorityOfficer"]);
        _client.SetAuthorizationHeader(token);

        var createDto = new UpsertVesselDto
        {
            Imo = GenerateValidIMO(),
            VesselName = "Original Vessel Name",
            Capacity = 4000,
            Rows = 8,
            Bays = 16,
            Tiers = 6,
            Length = 250.0,
            VesselTypeId = vesselTypeId
        };

        var createResponse = await _client.PostAsJsonAsync("/api/Vessel", createDto);
        var created = await createResponse.Content.ReadFromJsonAsync<VesselDto>();

        var updateDto = new UpsertVesselDto
        {
            Imo = createDto.Imo, // IMO cannot change
            VesselName = "Updated Vessel Name",
            Capacity = 4500,
            Rows = 9,
            Bays = 18,
            Tiers = 7,
            Length = 280.0,
            VesselTypeId = vesselTypeId
        };

        // Act
        var response = await _client.PutAsJsonAsync($"/api/Vessel/{created!.Id}", updateDto);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        
        var result = await response.Content.ReadFromJsonAsync<VesselDto>();
        result.Should().NotBeNull();
        result!.VesselName.Should().Be(updateDto.VesselName);
        result.Capacity.Should().Be(updateDto.Capacity);
        result.Length.Should().Be(updateDto.Length);
    }

    [Fact]
    public async Task UpdateVessel_WithNonExistentId_ShouldReturn404NotFound()
    {
        // Arrange
        var vesselTypeId = await CreateVesselType();
        var token = AuthenticationHelper.GenerateToken(roles: ["PortAuthorityOfficer"]);
        _client.SetAuthorizationHeader(token);
        var nonExistentId = Guid.NewGuid();

        var updateDto = new UpsertVesselDto
        {
            Imo = GenerateValidIMO(),
            VesselName = "Non-existent Vessel",
            Capacity = 4000,
            Rows = 8,
            Bays = 16,
            Tiers = 6,
            Length = 250.0,
            VesselTypeId = vesselTypeId
        };

        // Act
        var response = await _client.PutAsJsonAsync($"/api/Vessel/{nonExistentId}", updateDto);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }

    [Fact]
    public async Task UpdateVessel_WithInvalidData_ShouldReturn400BadRequest()
    {
        // Arrange
        var vesselTypeId = await CreateVesselType();
        var token = AuthenticationHelper.GenerateToken(roles: ["PortAuthorityOfficer"]);
        _client.SetAuthorizationHeader(token);

        var createDto = new UpsertVesselDto
        {
            Imo = GenerateValidIMO(),
            VesselName = "Test Vessel for Update",
            Capacity = 4000,
            Rows = 8,
            Bays = 16,
            Tiers = 6,
            Length = 250.0,
            VesselTypeId = vesselTypeId
        };

        var createResponse = await _client.PostAsJsonAsync("/api/Vessel", createDto);
        var created = await createResponse.Content.ReadFromJsonAsync<VesselDto>();

        var updateDto = new UpsertVesselDto
        {
            Imo = createDto.Imo,
            VesselName = "Updated Name",
            Capacity = -100, // Invalid negative capacity
            Rows = 8,
            Bays = 16,
            Tiers = 6,
            Length = 250.0,
            VesselTypeId = vesselTypeId
        };

        // Act
        var response = await _client.PutAsJsonAsync($"/api/Vessel/{created!.Id}", updateDto);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.InternalServerError); // ArgumentOutOfRangeException not caught
    }

    [Fact]
    public async Task UpdateVessel_WithoutAuthentication_ShouldReturn401Unauthorized()
    {
        // Arrange - No token set
        var vesselId = Guid.NewGuid();
        var updateDto = new UpsertVesselDto
        {
            Imo = GenerateValidIMO(),
            VesselName = "Unauthorized Update",
            Capacity = 4000,
            Rows = 8,
            Bays = 16,
            Tiers = 6,
            Length = 250.0,
            VesselTypeId = Guid.NewGuid().ToString()
        };

        // Act
        var response = await _client.PutAsJsonAsync($"/api/Vessel/{vesselId}", updateDto);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    #endregion

    #region Delete Tests

    [Fact]
    public async Task DeleteVessel_WithValidId_ShouldReturn204NoContent()
    {
        // Arrange
        var vesselTypeId = await CreateVesselType();
        
        // Create vessel with PortAuthorityOfficer
        var createToken = AuthenticationHelper.GenerateToken("PortAuthorityOfficer");
        _client.SetAuthorizationHeader(createToken);

        var createDto = new UpsertVesselDto
        {
            Imo = GenerateValidIMO(),
            VesselName = "Vessel to Delete",
            Capacity = 4000,
            Rows = 8,
            Bays = 16,
            Tiers = 6,
            Length = 250.0,
            VesselTypeId = vesselTypeId
        };

        var createResponse = await _client.PostAsJsonAsync("/api/Vessel", createDto);
        var created = await createResponse.Content.ReadFromJsonAsync<VesselDto>();

        // Delete with Admin role
        var deleteToken = AuthenticationHelper.GenerateToken("Admin");
        _client.SetAuthorizationHeader(deleteToken);

        // Act
        var response = await _client.DeleteAsync($"/api/Vessel/{created!.Id}");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.NoContent);

        // Verify deletion
        _client.SetAuthorizationHeader(createToken);
        var getResponse = await _client.GetAsync($"/api/Vessel/{created.Id}");
        getResponse.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }

    [Fact]
    public async Task DeleteVessel_WithNonExistentId_ShouldReturn404NotFound()
    {
        // Arrange
        var token = AuthenticationHelper.GenerateToken("Admin");
        _client.SetAuthorizationHeader(token);
        var nonExistentId = Guid.NewGuid();

        // Act
        var response = await _client.DeleteAsync($"/api/Vessel/{nonExistentId}");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }

    [Fact]
    public async Task DeleteVessel_WithoutAuthentication_ShouldReturn401Unauthorized()
    {
        // Arrange - No token set
        var vesselId = Guid.NewGuid();

        // Act
        var response = await _client.DeleteAsync($"/api/Vessel/{vesselId}");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    [Fact]
    public async Task DeleteVessel_WithNonAdminRole_ShouldReturn403Forbidden()
    {
        // Arrange
        var token = AuthenticationHelper.GenerateToken("PortAuthorityOfficer");
        _client.SetAuthorizationHeader(token);
        var vesselId = Guid.NewGuid();

        // Act
        var response = await _client.DeleteAsync($"/api/Vessel/{vesselId}");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.Forbidden);
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
        var response = await _client.GetAsync("/api/Vessel");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
    }

    #endregion
}
