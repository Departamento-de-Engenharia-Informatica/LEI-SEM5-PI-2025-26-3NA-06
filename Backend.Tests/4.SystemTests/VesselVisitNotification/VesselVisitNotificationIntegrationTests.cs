using System.Net;
using Backend.Tests.SystemTests.Infrastructure;
using FluentAssertions;
using ProjArqsi.Application.DTOs;
using ProjArqsi.Application.DTOs.Dock;
using ProjArqsi.Application.DTOs.VVN;
using Xunit;

namespace Backend.Tests.SystemTests.VesselVisitNotification;

[Collection("Sequential")]
public class VesselVisitNotificationIntegrationTests : IAsyncLifetime
{
    private readonly CustomWebApplicationFactory _factory;
    private readonly HttpClient _client;

    public VesselVisitNotificationIntegrationTests()
    {
        _factory = new CustomWebApplicationFactory();
        _client = _factory.CreateClient();
    }

    public async Task InitializeAsync()
    {
        // Reset database before each test class
        await _factory.ResetDatabaseAsync();
    }

    public Task DisposeAsync() => Task.CompletedTask;

    private async Task<string> CreateVessel()
    {
        var token = AuthenticationHelper.GenerateToken("PortAuthorityOfficer");
        _client.SetAuthorizationHeader(token);

        // First create a vessel type
        var vesselTypeDto = new VesselTypeUpsertDto
        {
            TypeName = $"Container Ship {Guid.NewGuid().ToString().Substring(0, 8)}",
            TypeDescription = "Standard container vessel",
            TypeCapacity = 5000,
            MaxRows = 10,
            MaxBays = 20,
            MaxTiers = 8
        };

        var typeResponse = await _client.PostAsJsonAsync("/api/VesselType", vesselTypeDto);
        var vesselType = await typeResponse.Content.ReadFromJsonAsync<VesselTypeDto>();

        // Now create a vessel
        var vesselDto = new UpsertVesselDto
        {
            Imo = GenerateValidIMO(),
            VesselName = $"MV Vessel {Guid.NewGuid().ToString().Substring(0, 8)}",
            Capacity = 4500,
            Rows = 9,
            Bays = 18,
            Tiers = 7,
            Length = 285.5,
            VesselTypeId = vesselType!.Id
        };

        var vesselResponse = await _client.PostAsJsonAsync("/api/Vessel", vesselDto);
        var vessel = await vesselResponse.Content.ReadFromJsonAsync<VesselDto>();
        return vessel!.Imo;  // Return IMO number, not vessel ID
    }

    private async Task<string> CreateDock(string? vesselTypeId = null)
    {
        var token = AuthenticationHelper.GenerateToken("PortAuthorityOfficer");
        _client.SetAuthorizationHeader(token);

        // If no vessel type provided, create one
        if (vesselTypeId == null)
        {
            var vtDto = new VesselTypeUpsertDto
            {
                TypeName = $"Type_{Guid.NewGuid().ToString().Substring(0, 8)}",
                TypeCapacity = 5000,
                MaxRows = 10,
                MaxBays = 20,
                MaxTiers = 8,
                TypeDescription = "Test vessel type for dock"
            };
            var vtResponse = await _client.PostAsJsonAsync("/api/VesselType", vtDto);
            var vesselType = await vtResponse.Content.ReadFromJsonAsync<VesselTypeDto>();
            vesselTypeId = vesselType!.Id;
        }

        var guid = Guid.NewGuid().ToString().Substring(0, 8);
        var dockDto = new DockUpsertDto
        {
            DockName = $"Dock_{guid}",
            LocationDescription = "Test dock location",
            Length = 350.0,
            Depth = 15.0,
            MaxDraft = 12.5,
            AllowedVesselTypeIds = new List<string> { vesselTypeId }
        };

        var response = await _client.PostAsJsonAsync("/api/Dock", dockDto);
        var dock = await response.Content.ReadFromJsonAsync<DockDto>();
        return dock!.Id;
    }

    private string GenerateValidIMO()
    {
        var random = new Random();
        var first6Digits = random.Next(100000, 999999).ToString();
        
        int sum = 0;
        int[] multipliers = { 7, 6, 5, 4, 3, 2 };
        
        for (int i = 0; i < 6; i++)
        {
            sum += (first6Digits[i] - '0') * multipliers[i];
        }
        
        int checkDigit = sum % 10;
        return first6Digits + checkDigit.ToString();
    }

    #region Draft VVN Tests

    [Fact]
    public async Task DraftVVN_WithValidData_ShouldReturn201Created()
    {
        // Arrange
        var vesselId = await CreateVessel();
        var token = AuthenticationHelper.GenerateToken("ShippingAgentRepresentative");
        _client.SetAuthorizationHeader(token);

        var dto = new VVNDraftDto
        {
            ReferredVesselId = vesselId,
            ArrivalDate = DateTime.UtcNow.AddDays(10),
            DepartureDate = DateTime.UtcNow.AddDays(12)
        };

        // Act
        var response = await _client.PostAsJsonAsync("/api/VesselVisitNotification/draft", dto);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.Created);
        var result = await response.Content.ReadFromJsonAsync<VVNDraftDtoWId>();
        result.Should().NotBeNull();
        result!.ReferredVesselId.Should().Be(vesselId);
    }

    [Fact]
    public async Task DraftVVN_WithNonExistentVessel_ShouldReturn400BadRequest()
    {
        // Arrange
        var token = AuthenticationHelper.GenerateToken("ShippingAgentRepresentative");
        _client.SetAuthorizationHeader(token);

        var dto = new VVNDraftDto
        {
            ReferredVesselId = Guid.NewGuid().ToString(),
            ArrivalDate = DateTime.UtcNow.AddDays(10),
            DepartureDate = DateTime.UtcNow.AddDays(12)
        };

        // Act
        var response = await _client.PostAsJsonAsync("/api/VesselVisitNotification/draft", dto);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }

    [Fact]
    public async Task DraftVVN_WithoutAuthentication_ShouldReturn401Unauthorized()
    {
        // Arrange
        var vesselId = await CreateVessel();
        _client.ClearAuthorizationHeader(); // Clear auth header set by CreateVessel

        var dto = new VVNDraftDto
        {
            ReferredVesselId = vesselId,
            ArrivalDate = DateTime.UtcNow.AddDays(10),
            DepartureDate = DateTime.UtcNow.AddDays(12)
        };

        // Act
        var response = await _client.PostAsJsonAsync("/api/VesselVisitNotification/draft", dto);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    [Fact]
    public async Task DraftVVN_WithWrongRole_ShouldReturn403Forbidden()
    {
        // Arrange
        var vesselId = await CreateVessel();
        var token = AuthenticationHelper.GenerateToken("LogisticOperator");
        _client.SetAuthorizationHeader(token);

        var dto = new VVNDraftDto
        {
            ReferredVesselId = vesselId,
            ArrivalDate = DateTime.UtcNow.AddDays(10),
            DepartureDate = DateTime.UtcNow.AddDays(12)
        };

        // Act
        var response = await _client.PostAsJsonAsync("/api/VesselVisitNotification/draft", dto);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.Forbidden);
    }

    [Fact]
    public async Task GetAllDrafts_ShouldReturnAllDraftedVVNs()
    {
        // Arrange
        var vesselId = await CreateVessel();
        var token = AuthenticationHelper.GenerateToken("ShippingAgentRepresentative");
        _client.SetAuthorizationHeader(token);

        // Create drafts
        for (int i = 0; i < 3; i++)
        {
            var dto = new VVNDraftDto
            {
                ReferredVesselId = vesselId,
                ArrivalDate = DateTime.UtcNow.AddDays(10 + i),
                DepartureDate = DateTime.UtcNow.AddDays(12 + i)
            };
            await _client.PostAsJsonAsync("/api/VesselVisitNotification/draft", dto);
        }

        // Act
        var response = await _client.GetAsync("/api/VesselVisitNotification/drafts");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var result = await response.Content.ReadFromJsonAsync<List<VVNDto>>();
        result.Should().NotBeNull();
        result!.Count.Should().BeGreaterOrEqualTo(3);
    }

    [Fact]
    public async Task GetDraftById_WithExistingId_ShouldReturn200Ok()
    {
        // Arrange
        var vesselId = await CreateVessel();
        var token = AuthenticationHelper.GenerateToken("ShippingAgentRepresentative");
        _client.SetAuthorizationHeader(token);

        var createDto = new VVNDraftDto
        {
            ReferredVesselId = vesselId,
            ArrivalDate = DateTime.UtcNow.AddDays(10),
            DepartureDate = DateTime.UtcNow.AddDays(12)
        };

        var createResponse = await _client.PostAsJsonAsync("/api/VesselVisitNotification/draft", createDto);
        var created = await createResponse.Content.ReadFromJsonAsync<VVNDraftDtoWId>();

        // Act
        var response = await _client.GetAsync($"/api/VesselVisitNotification/drafts/{created!.Id}");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var result = await response.Content.ReadFromJsonAsync<VVNDraftDtoWId>();
        result.Should().NotBeNull();
        result!.Id.Should().Be(created.Id);
    }

    [Fact]
    public async Task GetDraftById_WithNonExistentId_ShouldReturn404NotFound()
    {
        // Arrange
        var token = AuthenticationHelper.GenerateToken("ShippingAgentRepresentative");
        _client.SetAuthorizationHeader(token);
        var nonExistentId = Guid.NewGuid();

        // Act
        var response = await _client.GetAsync($"/api/VesselVisitNotification/drafts/{nonExistentId}");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }

    [Fact]
    public async Task UpdateDraft_WithValidData_ShouldReturn200Ok()
    {
        // Arrange
        var vesselId = await CreateVessel();
        var token = AuthenticationHelper.GenerateToken("ShippingAgentRepresentative");
        _client.SetAuthorizationHeader(token);

        var createDto = new VVNDraftDto
        {
            ReferredVesselId = vesselId,
            ArrivalDate = DateTime.UtcNow.AddDays(10),
            DepartureDate = DateTime.UtcNow.AddDays(12)
        };

        var createResponse = await _client.PostAsJsonAsync("/api/VesselVisitNotification/draft", createDto);
        var created = await createResponse.Content.ReadFromJsonAsync<VVNDraftDtoWId>();

        var updateDto = new VVNDraftDto
        {
            ReferredVesselId = vesselId,
            ArrivalDate = DateTime.UtcNow.AddDays(15),
            DepartureDate = DateTime.UtcNow.AddDays(17)
        };

        // Act
        var response = await _client.PutAsJsonAsync($"/api/VesselVisitNotification/drafts/{created!.Id}", updateDto);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
    }

    [Fact]
    public async Task DeleteDraft_WithValidId_ShouldReturn204NoContent()
    {
        // Arrange
        var vesselId = await CreateVessel();
        var token = AuthenticationHelper.GenerateToken("ShippingAgentRepresentative");
        _client.SetAuthorizationHeader(token);

        var dto = new VVNDraftDto
        {
            ReferredVesselId = vesselId,
            ArrivalDate = DateTime.UtcNow.AddDays(10),
            DepartureDate = DateTime.UtcNow.AddDays(12)
        };

        var createResponse = await _client.PostAsJsonAsync("/api/VesselVisitNotification/draft", dto);
        var created = await createResponse.Content.ReadFromJsonAsync<VVNDraftDtoWId>();

        // Act
        var response = await _client.DeleteAsync($"/api/VesselVisitNotification/drafts/{created!.Id}");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.NoContent);
    }

    #endregion

    #region Submit VVN Tests

    [Fact]
    public async Task SubmitVVN_WithValidData_ShouldReturn201Created()
    {
        // Arrange
        var vesselId = await CreateVessel();
        var token = AuthenticationHelper.GenerateToken("ShippingAgentRepresentative");
        _client.SetAuthorizationHeader(token);

        var dto = new VVNSubmitDto
        {
            ReferredVesselId = vesselId,
            ArrivalDate = DateTime.UtcNow.AddDays(10),
            DepartureDate = DateTime.UtcNow.AddDays(12)
        };

        // Act
        var response = await _client.PostAsJsonAsync("/api/VesselVisitNotification/submit", dto);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.Created);
        var result = await response.Content.ReadFromJsonAsync<VVNSubmitDtoWId>();
        result.Should().NotBeNull();
    }

    [Fact]
    public async Task SubmitVVN_WithNonExistentVessel_ShouldReturn400BadRequest()
    {
        // Arrange
        var token = AuthenticationHelper.GenerateToken("ShippingAgentRepresentative");
        _client.SetAuthorizationHeader(token);

        var dto = new VVNSubmitDto
        {
            ReferredVesselId = Guid.NewGuid().ToString(),
            ArrivalDate = DateTime.UtcNow.AddDays(10),
            DepartureDate = DateTime.UtcNow.AddDays(12)
        };

        // Act
        var response = await _client.PostAsJsonAsync("/api/VesselVisitNotification/submit", dto);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }

    [Fact]
    public async Task SubmitDraft_WithValidId_ShouldReturn200Ok()
    {
        // Arrange
        var vesselId = await CreateVessel();
        var token = AuthenticationHelper.GenerateToken("ShippingAgentRepresentative");
        _client.SetAuthorizationHeader(token);

        var draftDto = new VVNDraftDto
        {
            ReferredVesselId = vesselId,
            ArrivalDate = DateTime.UtcNow.AddDays(10),
            DepartureDate = DateTime.UtcNow.AddDays(12)
        };

        var draftResponse = await _client.PostAsJsonAsync("/api/VesselVisitNotification/draft", draftDto);
        var draft = await draftResponse.Content.ReadFromJsonAsync<VVNDraftDtoWId>();

        // Act
        var response = await _client.PostAsync($"/api/VesselVisitNotification/drafts/{draft!.Id}/submit", null);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var result = await response.Content.ReadFromJsonAsync<VVNSubmitDtoWId>();
        result.Should().NotBeNull();
    }

    #endregion

    #region Approve/Reject VVN Tests

    [Fact]
    public async Task GetAllPending_ShouldReturnPendingVVNs()
    {
        // Arrange - Create and submit a VVN
        var vesselId = await CreateVessel();
        var agentToken = AuthenticationHelper.GenerateToken("ShippingAgentRepresentative");
        _client.SetAuthorizationHeader(agentToken);

        var submitDto = new VVNSubmitDto
        {
            ReferredVesselId = vesselId,
            ArrivalDate = DateTime.UtcNow.AddDays(10),
            DepartureDate = DateTime.UtcNow.AddDays(12)
        };

        await _client.PostAsJsonAsync("/api/VesselVisitNotification/submit", submitDto);

        // Switch to Port Authority Officer
        var officerToken = AuthenticationHelper.GenerateToken("PortAuthorityOfficer");
        _client.SetAuthorizationHeader(officerToken);

        // Act
        var response = await _client.GetAsync("/api/VesselVisitNotification/pending");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var result = await response.Content.ReadFromJsonAsync<List<VVNDto>>();
        result.Should().NotBeNull();
        result!.Count.Should().BeGreaterOrEqualTo(1);
    }

    [Fact]
    public async Task ApproveVVN_WithValidData_ShouldReturn200Ok()
    {
        // Arrange - Create a vessel type, vessel, and dock with matching type
        var officerToken = AuthenticationHelper.GenerateToken("PortAuthorityOfficer");
        _client.SetAuthorizationHeader(officerToken);

        // Create vessel type
        var vesselTypeDto = new VesselTypeUpsertDto
        {
            TypeName = $"Container Ship {Guid.NewGuid().ToString().Substring(0, 8)}",
            TypeDescription = "Standard container vessel",
            TypeCapacity = 5000,
            MaxRows = 10,
            MaxBays = 20,
            MaxTiers = 8
        };
        var typeResponse = await _client.PostAsJsonAsync("/api/VesselType", vesselTypeDto);
        var vesselType = await typeResponse.Content.ReadFromJsonAsync<VesselTypeDto>();

        // Create vessel with this type
        var vesselDto = new UpsertVesselDto
        {
            Imo = GenerateValidIMO(),
            VesselName = $"MV Vessel {Guid.NewGuid().ToString().Substring(0, 8)}",
            Capacity = 4500,
            Rows = 9,
            Bays = 18,
            Tiers = 7,
            Length = 285.5,
            VesselTypeId = vesselType!.Id
        };
        var vesselResponse = await _client.PostAsJsonAsync("/api/Vessel", vesselDto);
        var vessel = await vesselResponse.Content.ReadFromJsonAsync<VesselDto>();
        var vesselId = vessel!.Imo;

        // Create dock that allows this vessel type
        var dockId = await CreateDock(vesselType.Id);

        // Switch to ShippingAgentRepresentative to submit VVN
        var agentToken = AuthenticationHelper.GenerateToken("ShippingAgentRepresentative");
        _client.SetAuthorizationHeader(agentToken);

        var submitDto = new VVNSubmitDto
        {
            ReferredVesselId = vesselId,
            ArrivalDate = DateTime.UtcNow.AddDays(10),
            DepartureDate = DateTime.UtcNow.AddDays(12)
        };

        var submitResponse = await _client.PostAsJsonAsync("/api/VesselVisitNotification/submit", submitDto);
        submitResponse.EnsureSuccessStatusCode();
        var submitted = await submitResponse.Content.ReadFromJsonAsync<VVNSubmitDtoWId>();

        // Switch back to PortAuthorityOfficer to approve
        _client.SetAuthorizationHeader(officerToken);

        var approvalDto = new VVNApprovalDto
        {
            TempAssignedDockId = dockId,
            ConfirmDockConflict = false
        };

        // Act
        var response = await _client.PostAsJsonAsync($"/api/VesselVisitNotification/{submitted!.Id}/approve", approvalDto);

        // Assert  
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var result = await response.Content.ReadFromJsonAsync<VVNDto>();
        result.Should().NotBeNull();
        result!.Status.Should().Be("Accepted");
        result.TempAssignedDockId.Should().Be(dockId);
    }

    [Fact]
    public async Task RejectVVN_WithValidData_ShouldReturn200Ok()
    {
        // Arrange
        var vesselId = await CreateVessel();

        var agentToken = AuthenticationHelper.GenerateToken("ShippingAgentRepresentative");
        _client.SetAuthorizationHeader(agentToken);

        var submitDto = new VVNSubmitDto
        {
            ReferredVesselId = vesselId,
            ArrivalDate = DateTime.UtcNow.AddDays(10),
            DepartureDate = DateTime.UtcNow.AddDays(12)
        };

        var submitResponse = await _client.PostAsJsonAsync("/api/VesselVisitNotification/submit", submitDto);
        var submitted = await submitResponse.Content.ReadFromJsonAsync<VVNSubmitDtoWId>();

        // Switch to Port Authority Officer
        var officerToken = AuthenticationHelper.GenerateToken("PortAuthorityOfficer");
        _client.SetAuthorizationHeader(officerToken);

        var rejectionDto = new VVNRejectionDto
        {
            RejectionReason = "Insufficient documentation"
        };

        // Act
        var response = await _client.PostAsJsonAsync($"/api/VesselVisitNotification/{submitted!.Id}/reject", rejectionDto);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var result = await response.Content.ReadFromJsonAsync<VVNDto>();
        result.Should().NotBeNull();
        result!.Status.Should().Be("Rejected");
        result.RejectionReason.Should().Be("Insufficient documentation");
    }

    [Fact]
    public async Task ApproveVVN_WithoutAuthentication_ShouldReturn401Unauthorized()
    {
        // Arrange
        var vvnId = Guid.NewGuid();
        var dockId = Guid.NewGuid().ToString();

        var approvalDto = new VVNApprovalDto
        {
            TempAssignedDockId = dockId,
            ConfirmDockConflict = false
        };

        // Act
        var response = await _client.PostAsJsonAsync($"/api/VesselVisitNotification/{vvnId}/approve", approvalDto);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    [Fact]
    public async Task ApproveVVN_WithWrongRole_ShouldReturn403Forbidden()
    {
        // Arrange
        var token = AuthenticationHelper.GenerateToken("ShippingAgentRepresentative");
        _client.SetAuthorizationHeader(token);

        var vvnId = Guid.NewGuid();
        var dockId = Guid.NewGuid().ToString();

        var approvalDto = new VVNApprovalDto
        {
            TempAssignedDockId = dockId,
            ConfirmDockConflict = false
        };

        // Act
        var response = await _client.PostAsJsonAsync($"/api/VesselVisitNotification/{vvnId}/approve", approvalDto);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.Forbidden);
    }

    #endregion

    #region Authorization Tests

    [Theory]
    [InlineData("ShippingAgentRepresentative")]
    public async Task GetAllDrafts_WithAuthorizedRole_ShouldReturn200Ok(string role)
    {
        // Arrange
        var token = AuthenticationHelper.GenerateToken(role);
        _client.SetAuthorizationHeader(token);

        // Act
        var response = await _client.GetAsync("/api/VesselVisitNotification/drafts");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
    }

    [Theory]
    [InlineData("PortAuthorityOfficer")]
    public async Task GetAllPending_WithAuthorizedRole_ShouldReturn200Ok(string role)
    {
        // Arrange
        var token = AuthenticationHelper.GenerateToken(role);
        _client.SetAuthorizationHeader(token);

        // Act
        var response = await _client.GetAsync("/api/VesselVisitNotification/pending");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
    }

    [Fact]
    public async Task GetAllDrafts_WithWrongRole_ShouldReturn403Forbidden()
    {
        // Arrange
        var token = AuthenticationHelper.GenerateToken("LogisticOperator");
        _client.SetAuthorizationHeader(token);

        // Act
        var response = await _client.GetAsync("/api/VesselVisitNotification/drafts");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.Forbidden);
    }

    [Fact]
    public async Task GetAllPending_WithWrongRole_ShouldReturn403Forbidden()
    {
        // Arrange
        var token = AuthenticationHelper.GenerateToken("LogisticOperator");
        _client.SetAuthorizationHeader(token);

        // Act
        var response = await _client.GetAsync("/api/VesselVisitNotification/pending");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.Forbidden);
    }

    #endregion
}
