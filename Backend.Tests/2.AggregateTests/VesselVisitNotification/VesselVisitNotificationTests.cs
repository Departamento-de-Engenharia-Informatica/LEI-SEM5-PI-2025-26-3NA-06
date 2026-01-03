using FluentAssertions;
using ProjArqsi.Domain.Shared;
using ProjArqsi.Domain.VesselVisitNotificationAggregate;
using ProjArqsi.Domain.ContainerAggregate;
using ProjArqsi.Domain.DockAggregate;
using Xunit;

namespace Backend.Tests.AggregateTests.VesselVisitNotification
{
    public class VesselVisitNotificationTests
    {
        #region Constructor Tests

        [Fact]
        public void Constructor_WithValidParameters_ShouldCreateVesselVisitNotification()
        {
            // Arrange
            var vesselImo = "9074729";
            var arrivalDate = DateTime.UtcNow.AddDays(5);
            var departureDate = DateTime.UtcNow.AddDays(7);

            // Act
            var vvn = new ProjArqsi.Domain.VesselVisitNotificationAggregate.VesselVisitNotification(
                vesselImo, arrivalDate, departureDate);

            // Assert
            vvn.Should().NotBeNull();
            vvn.Id.Should().NotBeNull();
            vvn.ReferredVesselId.Should().NotBeNull();
            vvn.ReferredVesselId.VesselId.Number.Should().Be(vesselImo);
            vvn.ArrivalDate.Should().NotBeNull();
            vvn.ArrivalDate!.Value.Should().Be(arrivalDate);
            vvn.DepartureDate.Should().NotBeNull();
            vvn.DepartureDate!.Value.Should().Be(departureDate);
            vvn.Status.Should().Be(Statuses.InProgress);
            vvn.CargoManifests.Should().BeEmpty();
            vvn.IsHazardous.Should().BeFalse();
            vvn.RejectionReason.Should().BeNull();
            vvn.TempAssignedDockId.Should().BeNull();
        }

        [Fact]
        public void Constructor_WithNullDates_ShouldCreateWithNullDates()
        {
            // Arrange
            var vesselImo = "9305623";

            // Act
            var vvn = new ProjArqsi.Domain.VesselVisitNotificationAggregate.VesselVisitNotification(
                vesselImo, null, null);

            // Assert
            vvn.Should().NotBeNull();
            vvn.ReferredVesselId.VesselId.Number.Should().Be(vesselImo);
            vvn.ArrivalDate.Should().BeNull();
            vvn.DepartureDate.Should().BeNull();
            vvn.Status.Should().Be(Statuses.InProgress);
        }

        [Fact]
        public void Constructor_ShouldGenerateUniqueIds()
        {
            // Arrange
            var vesselImo = "9366213";
            var arrivalDate = DateTime.UtcNow.AddDays(3);
            var departureDate = DateTime.UtcNow.AddDays(5);

            // Act
            var vvn1 = new ProjArqsi.Domain.VesselVisitNotificationAggregate.VesselVisitNotification(
                vesselImo, arrivalDate, departureDate);
            var vvn2 = new ProjArqsi.Domain.VesselVisitNotificationAggregate.VesselVisitNotification(
                vesselImo, arrivalDate, departureDate);

            // Assert
            vvn1.Id.Should().NotBe(vvn2.Id);
        }

        #endregion

        #region UpdateDates Tests

        [Fact]
        public void UpdateDates_WhenInProgress_ShouldUpdateDates()
        {
            // Arrange
            var vvn = CreateTestVVN();
            var newArrivalDate = DateTime.UtcNow.AddDays(10);
            var newDepartureDate = DateTime.UtcNow.AddDays(15);

            // Act
            vvn.UpdateDates(newArrivalDate, newDepartureDate);

            // Assert
            vvn.ArrivalDate!.Value.Should().Be(newArrivalDate);
            vvn.DepartureDate!.Value.Should().Be(newDepartureDate);
        }

        [Fact]
        public void UpdateDates_WhenSubmitted_ShouldThrowException()
        {
            // Arrange
            var vvn = CreateValidSubmittedVVN();
            var newArrivalDate = DateTime.UtcNow.AddDays(10);
            var newDepartureDate = DateTime.UtcNow.AddDays(15);

            // Act & Assert
            var act = () => vvn.UpdateDates(newArrivalDate, newDepartureDate);

            act.Should().Throw<InvalidOperationException>()
                .WithMessage("*in progress*");
        }

        [Fact]
        public void UpdateDates_WithNullValues_ShouldSetToNull()
        {
            // Arrange
            var vvn = CreateTestVVN();

            // Act
            vvn.UpdateDates(null, null);

            // Assert
            vvn.ArrivalDate.Should().BeNull();
            vvn.DepartureDate.Should().BeNull();
        }

        #endregion

        #region Manifest Management Tests

        [Fact]
        public void SetLoadingManifest_WithValidManifest_ShouldAddManifest()
        {
            // Arrange
            var vvn = CreateTestVVN();
            var manifest = CreateLoadingManifest();

            // Act
            vvn.SetLoadingManifest(manifest);

            // Assert
            vvn.CargoManifests.Should().HaveCount(1);
            vvn.GetLoadingManifest().Should().NotBeNull();
            vvn.GetLoadingManifest()!.ManifestType.Value.Should().Be(ManifestTypeEnum.Load);
        }

        [Fact]
        public void SetUnloadingManifest_WithValidManifest_ShouldAddManifest()
        {
            // Arrange
            var vvn = CreateTestVVN();
            var manifest = CreateUnloadingManifest();

            // Act
            vvn.SetUnloadingManifest(manifest);

            // Assert
            vvn.CargoManifests.Should().HaveCount(1);
            vvn.GetUnloadingManifest().Should().NotBeNull();
            vvn.GetUnloadingManifest()!.ManifestType.Value.Should().Be(ManifestTypeEnum.Unload);
        }

        [Fact]
        public void SetLoadingManifest_WhenAlreadyExists_ShouldReplaceExistingManifest()
        {
            // Arrange
            var vvn = CreateTestVVN();
            var manifest1 = CreateLoadingManifest();
            var manifest2 = CreateLoadingManifest();
            
            vvn.SetLoadingManifest(manifest1);

            // Act
            vvn.SetLoadingManifest(manifest2);

            // Assert
            vvn.CargoManifests.Should().HaveCount(1);
        }

        [Fact]
        public void SetLoadingManifest_WithUnloadType_ShouldThrowException()
        {
            // Arrange
            var vvn = CreateTestVVN();
            var manifest = CreateUnloadingManifest();

            // Act & Assert
            var act = () => vvn.SetLoadingManifest(manifest);

            act.Should().Throw<BusinessRuleValidationException>()
                .WithMessage("*type Load*");
        }

        [Fact]
        public void SetUnloadingManifest_WithLoadType_ShouldThrowException()
        {
            // Arrange
            var vvn = CreateTestVVN();
            var manifest = CreateLoadingManifest();

            // Act & Assert
            var act = () => vvn.SetUnloadingManifest(manifest);

            act.Should().Throw<BusinessRuleValidationException>()
                .WithMessage("*type Unload*");
        }

        [Fact]
        public void SetLoadingManifest_WhenSubmitted_ShouldThrowException()
        {
            // Arrange
            var vvn = CreateValidSubmittedVVN();
            var manifest = CreateLoadingManifest();

            // Act & Assert
            var act = () => vvn.SetLoadingManifest(manifest);

            act.Should().Throw<InvalidOperationException>()
                .WithMessage("*in progress*");
        }

        [Fact]
        public void RemoveLoadingManifest_WhenExists_ShouldRemoveManifest()
        {
            // Arrange
            var vvn = CreateTestVVN();
            vvn.SetLoadingManifest(CreateLoadingManifest());

            // Act
            vvn.RemoveLoadingManifest();

            // Assert
            vvn.GetLoadingManifest().Should().BeNull();
        }

        [Fact]
        public void RemoveUnloadingManifest_WhenExists_ShouldRemoveManifest()
        {
            // Arrange
            var vvn = CreateTestVVN();
            vvn.SetUnloadingManifest(CreateUnloadingManifest());

            // Act
            vvn.RemoveUnloadingManifest();

            // Assert
            vvn.GetUnloadingManifest().Should().BeNull();
        }

        [Fact]
        public void ManifestOperations_WithBothTypes_ShouldMaintainBoth()
        {
            // Arrange
            var vvn = CreateTestVVN();

            // Act
            vvn.SetLoadingManifest(CreateLoadingManifest());
            vvn.SetUnloadingManifest(CreateUnloadingManifest());

            // Assert
            vvn.CargoManifests.Should().HaveCount(2);
            vvn.GetLoadingManifest().Should().NotBeNull();
            vvn.GetUnloadingManifest().Should().NotBeNull();
        }

        #endregion

        #region Submit Tests

        [Fact]
        public void Submit_WithValidData_ShouldChangeStatusToSubmitted()
        {
            // Arrange
            var vvn = CreateTestVVN();

            // Act
            vvn.Submit();

            // Assert
            vvn.Status.Should().Be(Statuses.Submitted);
        }

        [Fact]
        public void Submit_WhenAlreadySubmitted_ShouldThrowException()
        {
            // Arrange
            var vvn = CreateValidSubmittedVVN();

            // Act & Assert
            var act = () => vvn.Submit();

            act.Should().Throw<InvalidOperationException>()
                .WithMessage("*in progress*");
        }

        [Fact]
        public void Submit_WithoutArrivalDate_ShouldThrowException()
        {
            // Arrange
            var vvn = new ProjArqsi.Domain.VesselVisitNotificationAggregate.VesselVisitNotification(
                "9432892", null, DateTime.UtcNow.AddDays(5));

            // Act & Assert
            var act = () => vvn.Submit();

            act.Should().Throw<BusinessRuleValidationException>()
                .WithMessage("*Arrival date*required*");
        }

        [Fact]
        public void Submit_WithoutDepartureDate_ShouldThrowException()
        {
            // Arrange
            var vvn = new ProjArqsi.Domain.VesselVisitNotificationAggregate.VesselVisitNotification(
                "9542738", DateTime.UtcNow.AddDays(5), null);

            // Act & Assert
            var act = () => vvn.Submit();

            act.Should().Throw<BusinessRuleValidationException>()
                .WithMessage("*Departure date*required*");
        }

        [Fact]
        public void Submit_WithArrivalAfterDeparture_ShouldThrowException()
        {
            // Arrange
            var departureDate = DateTime.UtcNow.AddDays(5);
            var arrivalDate = DateTime.UtcNow.AddDays(10);
            var vvn = new ProjArqsi.Domain.VesselVisitNotificationAggregate.VesselVisitNotification(
                "9612507", arrivalDate, departureDate);

            // Act & Assert
            var act = () => vvn.Submit();

            act.Should().Throw<BusinessRuleValidationException>()
                .WithMessage("*Arrival date must be before departure*");
        }

        #endregion

        #region Accept and Approve Tests

        [Fact]
        public void Accept_WhenSubmitted_ShouldChangeStatusToAccepted()
        {
            // Arrange
            var vvn = CreateValidSubmittedVVN();

            // Act
            vvn.Accept();

            // Assert
            vvn.Status.Should().Be(Statuses.Accepted);
            vvn.RejectionReason.Should().BeNull();
        }

        [Fact]
        public void Accept_WhenNotSubmitted_ShouldThrowException()
        {
            // Arrange
            var vvn = CreateTestVVN();

            // Act & Assert
            var act = () => vvn.Accept();

            act.Should().Throw<InvalidOperationException>()
                .WithMessage("*submitted*");
        }

        [Fact]
        public void Approve_WithValidParameters_ShouldAcceptAndAssignDock()
        {
            // Arrange
            var vvn = CreateValidSubmittedVVN();
            var dockId = new DockId(Guid.NewGuid());
            var officerId = "officer123";

            // Act
            vvn.Approve(dockId, officerId);

            // Assert
            vvn.Status.Should().Be(Statuses.Accepted);
            vvn.TempAssignedDockId.Should().NotBeNull();
            vvn.TempAssignedDockId!.AsGuid().Should().Be(dockId.Value);
            vvn.RejectionReason.Should().BeNull();
        }

        [Fact]
        public void Approve_WithNullDockId_ShouldThrowException()
        {
            // Arrange
            var vvn = CreateValidSubmittedVVN();

            // Act & Assert
            var act = () => vvn.Approve(null!, "officer123");

            act.Should().Throw<BusinessRuleValidationException>()
                .WithMessage("*dock*required*");
        }

        [Fact]
        public void Approve_WithEmptyOfficerId_ShouldThrowException()
        {
            // Arrange
            var vvn = CreateValidSubmittedVVN();
            var dockId = new DockId(Guid.NewGuid());

            // Act & Assert
            var act = () => vvn.Approve(dockId, "");

            act.Should().Throw<BusinessRuleValidationException>()
                .WithMessage("*Officer ID*required*");
        }

        [Fact]
        public void Approve_WhenNotSubmitted_ShouldThrowException()
        {
            // Arrange
            var vvn = CreateTestVVN();
            var dockId = new DockId(Guid.NewGuid());

            // Act & Assert
            var act = () => vvn.Approve(dockId, "officer123");

            act.Should().Throw<BusinessRuleValidationException>()
                .WithMessage("*submitted*");
        }

        #endregion

        #region Reject Tests

        [Fact]
        public void Reject_WithValidParameters_ShouldChangeStatusAndSetReason()
        {
            // Arrange
            var vvn = CreateValidSubmittedVVN();
            var reason = "Missing required documentation";
            var officerId = "officer456";

            // Act
            vvn.Reject(reason, officerId);

            // Assert
            vvn.Status.Should().Be(Statuses.Rejected);
            vvn.RejectionReason.Should().NotBeNull();
            vvn.RejectionReason!.Value.Should().Be(reason);
            vvn.TempAssignedDockId.Should().BeNull();
        }

        [Fact]
        public void Reject_WithEmptyReason_ShouldThrowException()
        {
            // Arrange
            var vvn = CreateValidSubmittedVVN();

            // Act & Assert
            var act = () => vvn.Reject("", "officer456");

            act.Should().Throw<BusinessRuleValidationException>()
                .WithMessage("*Rejection reason*required*");
        }

        [Fact]
        public void Reject_WithEmptyOfficerId_ShouldThrowException()
        {
            // Arrange
            var vvn = CreateValidSubmittedVVN();

            // Act & Assert
            var act = () => vvn.Reject("Invalid data", "");

            act.Should().Throw<BusinessRuleValidationException>()
                .WithMessage("*Officer ID*required*");
        }

        [Fact]
        public void Reject_WhenNotSubmitted_ShouldThrowException()
        {
            // Arrange
            var vvn = CreateTestVVN();

            // Act & Assert
            var act = () => vvn.Reject("Invalid", "officer456");

            act.Should().Throw<BusinessRuleValidationException>()
                .WithMessage("*submitted*");
        }

        #endregion

        #region Resubmit Tests

        [Fact]
        public void Resubmit_WhenRejected_ShouldChangeStatusToSubmitted()
        {
            // Arrange
            var vvn = CreateRejectedVVN();

            // Act
            vvn.Resubmit();

            // Assert
            vvn.Status.Should().Be(Statuses.Submitted);
            vvn.RejectionReason.Should().BeNull();
        }

        [Fact]
        public void Resubmit_WhenInProgress_ShouldChangeStatusToSubmitted()
        {
            // Arrange
            var vvn = CreateTestVVN();

            // Act
            vvn.Resubmit();

            // Assert
            vvn.Status.Should().Be(Statuses.Submitted);
        }

        [Fact]
        public void Resubmit_WhenAccepted_ShouldThrowException()
        {
            // Arrange
            var vvn = CreateValidSubmittedVVN();
            vvn.Accept();

            // Act & Assert
            var act = () => vvn.Resubmit();

            act.Should().Throw<BusinessRuleValidationException>()
                .WithMessage("*rejected or in-progress*");
        }

        [Fact]
        public void UpdateAndResubmit_WhenRejected_ShouldUpdateDatesAndSetToInProgress()
        {
            // Arrange
            var vvn = CreateRejectedVVN();
            var newArrivalDate = DateTime.UtcNow.AddDays(20);
            var newDepartureDate = DateTime.UtcNow.AddDays(25);

            // Act
            vvn.UpdateAndResubmit(newArrivalDate, newDepartureDate);

            // Assert
            vvn.Status.Should().Be(Statuses.InProgress);
            vvn.ArrivalDate!.Value.Should().Be(newArrivalDate);
            vvn.DepartureDate!.Value.Should().Be(newDepartureDate);
        }

        [Fact]
        public void UpdateAndResubmit_WhenNotRejected_ShouldThrowException()
        {
            // Arrange
            var vvn = CreateTestVVN();

            // Act & Assert
            var act = () => vvn.UpdateAndResubmit(DateTime.UtcNow.AddDays(5), DateTime.UtcNow.AddDays(10));

            act.Should().Throw<BusinessRuleValidationException>()
                .WithMessage("*rejected*");
        }

        #endregion

        #region ConvertToDraft Tests

        [Fact]
        public void ConvertToDraft_WhenRejected_ShouldChangeStatusToInProgress()
        {
            // Arrange
            var vvn = CreateRejectedVVN();

            // Act
            vvn.ConvertToDraft();

            // Assert
            vvn.Status.Should().Be(Statuses.InProgress);
            vvn.RejectionReason.Should().BeNull();
        }

        [Fact]
        public void ConvertToDraft_WhenNotRejected_ShouldThrowException()
        {
            // Arrange
            var vvn = CreateTestVVN();

            // Act & Assert
            var act = () => vvn.ConvertToDraft();

            act.Should().Throw<BusinessRuleValidationException>()
                .WithMessage("*rejected*");
        }

        #endregion

        #region UpdateManifestsForResubmit Tests

        [Fact]
        public void UpdateManifestsForResubmit_WhenInProgress_ShouldReplaceManifests()
        {
            // Arrange
            var vvn = CreateTestVVN();
            vvn.SetLoadingManifest(CreateLoadingManifest());
            vvn.SetUnloadingManifest(CreateUnloadingManifest());

            var newLoadManifest = CreateLoadingManifest();
            var newUnloadManifest = CreateUnloadingManifest();

            // Act
            vvn.UpdateManifestsForResubmit(newLoadManifest, newUnloadManifest);

            // Assert
            vvn.CargoManifests.Should().HaveCount(2);
        }

        [Fact]
        public void UpdateManifestsForResubmit_WithOnlyLoading_ShouldSetOnlyLoadingManifest()
        {
            // Arrange
            var vvn = CreateTestVVN();
            var loadManifest = CreateLoadingManifest();

            // Act
            vvn.UpdateManifestsForResubmit(loadManifest, null);

            // Assert
            vvn.CargoManifests.Should().HaveCount(1);
            vvn.GetLoadingManifest().Should().NotBeNull();
            vvn.GetUnloadingManifest().Should().BeNull();
        }

        [Fact]
        public void UpdateManifestsForResubmit_WithInvalidLoadType_ShouldThrowException()
        {
            // Arrange
            var vvn = CreateTestVVN();
            var wrongManifest = CreateUnloadingManifest();

            // Act & Assert
            var act = () => vvn.UpdateManifestsForResubmit(wrongManifest, null);

            act.Should().Throw<BusinessRuleValidationException>()
                .WithMessage("*type Load*");
        }

        [Fact]
        public void UpdateManifestsForResubmit_WithInvalidUnloadType_ShouldThrowException()
        {
            // Arrange
            var vvn = CreateTestVVN();
            var wrongManifest = CreateLoadingManifest();

            // Act & Assert
            var act = () => vvn.UpdateManifestsForResubmit(null, wrongManifest);

            act.Should().Throw<BusinessRuleValidationException>()
                .WithMessage("*type Unload*");
        }

        [Fact]
        public void UpdateManifestsForResubmit_WhenNotInProgress_ShouldThrowException()
        {
            // Arrange
            var vvn = CreateValidSubmittedVVN();

            // Act & Assert
            var act = () => vvn.UpdateManifestsForResubmit(CreateLoadingManifest(), null);

            act.Should().Throw<BusinessRuleValidationException>()
                .WithMessage("*InProgress*");
        }

        #endregion

        #region Real-World Scenarios

        [Fact]
        public void Scenario_CompleteWorkflow_DraftToAccepted()
        {
            // Arrange - Create draft
            var vvn = CreateTestVVN();
            
            // Act - Add manifests
            vvn.SetLoadingManifest(CreateLoadingManifest());
            vvn.SetUnloadingManifest(CreateUnloadingManifest());

            // Act - Submit
            vvn.Submit();
            vvn.Status.Should().Be(Statuses.Submitted);

            // Act - Approve
            var dockId = new DockId(Guid.NewGuid());
            vvn.Approve(dockId, "officer123");

            // Assert
            vvn.Status.Should().Be(Statuses.Accepted);
            vvn.TempAssignedDockId.Should().NotBeNull();
        }

        [Fact]
        public void Scenario_RejectionAndResubmission()
        {
            // Arrange - Create and submit
            var vvn = CreateValidSubmittedVVN();

            // Act - Reject
            vvn.Reject("Missing cargo documentation", "officer456");
            vvn.Status.Should().Be(Statuses.Rejected);

            // Act - Convert back to draft
            vvn.ConvertToDraft();
            vvn.Status.Should().Be(Statuses.InProgress);

            // Act - Update and resubmit
            vvn.UpdateDates(DateTime.UtcNow.AddDays(8), DateTime.UtcNow.AddDays(12));
            vvn.Submit();

            // Assert
            vvn.Status.Should().Be(Statuses.Submitted);
            vvn.RejectionReason.Should().BeNull();
        }

        [Fact]
        public void Scenario_OnlyLoadingManifest()
        {
            // Arrange
            var vvn = CreateTestVVN();

            // Act - Only loading containers
            vvn.SetLoadingManifest(CreateLoadingManifest());
            vvn.Submit();

            // Assert
            vvn.Status.Should().Be(Statuses.Submitted);
            vvn.GetLoadingManifest().Should().NotBeNull();
            vvn.GetUnloadingManifest().Should().BeNull();
        }

        [Fact]
        public void Scenario_OnlyUnloadingManifest()
        {
            // Arrange
            var vvn = CreateTestVVN();

            // Act - Only unloading containers
            vvn.SetUnloadingManifest(CreateUnloadingManifest());
            vvn.Submit();

            // Assert
            vvn.Status.Should().Be(Statuses.Submitted);
            vvn.GetLoadingManifest().Should().BeNull();
            vvn.GetUnloadingManifest().Should().NotBeNull();
        }

        [Fact]
        public void Scenario_UpdateManifestBeforeSubmission()
        {
            // Arrange
            var vvn = CreateTestVVN();

            // Act - Set initial manifest
            vvn.SetLoadingManifest(CreateLoadingManifest());
            
            // Act - Update manifest
            var newManifest = CreateLoadingManifest();
            vvn.SetLoadingManifest(newManifest);

            // Assert
            vvn.CargoManifests.Should().HaveCount(1);
        }

        [Fact]
        public void Scenario_RemoveManifestBeforeSubmission()
        {
            // Arrange
            var vvn = CreateTestVVN();
            vvn.SetLoadingManifest(CreateLoadingManifest());
            vvn.SetUnloadingManifest(CreateUnloadingManifest());

            // Act
            vvn.RemoveLoadingManifest();

            // Assert
            vvn.CargoManifests.Should().HaveCount(1);
            vvn.GetLoadingManifest().Should().BeNull();
            vvn.GetUnloadingManifest().Should().NotBeNull();
        }

        [Fact]
        public void Scenario_MultipleRejectionResubmissionCycles()
        {
            // Arrange
            var vvn = CreateValidSubmittedVVN();

            // Act - First rejection
            vvn.Reject("Issue 1", "officer1");
            vvn.ConvertToDraft();
            vvn.Submit();

            // Act - Second rejection
            vvn.Reject("Issue 2", "officer2");
            vvn.ConvertToDraft();
            vvn.Submit();

            // Act - Finally accept
            vvn.Approve(new DockId(Guid.NewGuid()), "officer3");

            // Assert
            vvn.Status.Should().Be(Statuses.Accepted);
        }

        [Fact]
        public void Scenario_EmergencyVesselVisit_MinimalData()
        {
            // Arrange - Emergency with minimal required data
            var vvn = new ProjArqsi.Domain.VesselVisitNotificationAggregate.VesselVisitNotification(
                "9802140", 
                DateTime.UtcNow.AddHours(2), 
                DateTime.UtcNow.AddHours(6));

            // Act - No manifests, just dates
            vvn.Submit();

            // Assert
            vvn.Status.Should().Be(Statuses.Submitted);
            vvn.CargoManifests.Should().BeEmpty();
        }

        #endregion

        #region Identity and Equality Tests

        [Fact]
        public void VesselVisitNotifications_WithSameId_ShouldBeEqual()
        {
            // Arrange
            var vvn = CreateTestVVN();

            // Act - Compare with itself
            var result = vvn.Equals(vvn);

            // Assert
            result.Should().BeTrue();
            vvn.GetHashCode().Should().Be(vvn.GetHashCode());
        }

        [Fact]
        public void VesselVisitNotifications_WithDifferentIds_ShouldNotBeEqual()
        {
            // Arrange
            var vvn1 = CreateTestVVN();
            var vvn2 = CreateTestVVN();

            // Act & Assert
            vvn1.Should().NotBe(vvn2);
        }

        [Fact]
        public void VesselVisitNotificationId_ShouldBeImmutable()
        {
            // Arrange
            var vvn = CreateTestVVN();
            var originalId = vvn.Id;

            // Act - Modify properties
            vvn.UpdateDates(DateTime.UtcNow.AddDays(20), DateTime.UtcNow.AddDays(25));
            vvn.SetLoadingManifest(CreateLoadingManifest());

            // Assert
            vvn.Id.Should().Be(originalId);
        }

        [Fact]
        public void ReferredVesselId_ShouldBeImmutable()
        {
            // Arrange
            var vvn = CreateTestVVN();
            var originalVesselId = vvn.ReferredVesselId;

            // Act - Modify properties
            vvn.UpdateDates(DateTime.UtcNow.AddDays(20), DateTime.UtcNow.AddDays(25));
            vvn.Submit();

            // Assert
            vvn.ReferredVesselId.Should().Be(originalVesselId);
        }

        #endregion

        #region Edge Cases

        [Fact]
        public void EdgeCase_SameDayArrivalAndDeparture()
        {
            // Arrange
            var baseDate = DateTime.UtcNow.AddDays(5);
            var arrivalDate = baseDate.AddHours(8);
            var departureDate = baseDate.AddHours(20);

            // Act
            var vvn = new ProjArqsi.Domain.VesselVisitNotificationAggregate.VesselVisitNotification(
                "9234329", arrivalDate, departureDate);
            vvn.Submit();

            // Assert
            vvn.Status.Should().Be(Statuses.Submitted);
        }

        [Fact]
        public void EdgeCase_VeryLongStay()
        {
            // Arrange
            var arrivalDate = DateTime.UtcNow.AddDays(5);
            var departureDate = DateTime.UtcNow.AddDays(60); // 2 months

            // Act
            var vvn = new ProjArqsi.Domain.VesselVisitNotificationAggregate.VesselVisitNotification(
                "9074729", arrivalDate, departureDate);
            vvn.Submit();

            // Assert
            vvn.Status.Should().Be(Statuses.Submitted);
        }

        [Fact]
        public void EdgeCase_ImmediateArrival()
        {
            // Arrange
            var now = DateTime.UtcNow;
            var arrivalDate = now.AddMinutes(30);
            var departureDate = now.AddHours(4);

            // Act
            var vvn = new ProjArqsi.Domain.VesselVisitNotificationAggregate.VesselVisitNotification(
                "9305623", arrivalDate, departureDate);
            vvn.Submit();

            // Assert
            vvn.Status.Should().Be(Statuses.Submitted);
        }

        [Fact]
        public void EdgeCase_MultipleManifestUpdates()
        {
            // Arrange
            var vvn = CreateTestVVN();

            // Act - Multiple updates
            for (int i = 0; i < 5; i++)
            {
                vvn.SetLoadingManifest(CreateLoadingManifest());
                vvn.RemoveLoadingManifest();
            }

            vvn.SetLoadingManifest(CreateLoadingManifest());

            // Assert
            vvn.GetLoadingManifest().Should().NotBeNull();
        }

        #endregion

        #region Status Workflow Tests

        [Fact]
        public void StatusWorkflow_InProgressToSubmittedToAccepted()
        {
            // Arrange
            var vvn = CreateTestVVN();

            // Act & Assert - InProgress
            vvn.Status.Should().Be(Statuses.InProgress);

            // Act & Assert - Submitted
            vvn.Submit();
            vvn.Status.Should().Be(Statuses.Submitted);

            // Act & Assert - Accepted
            vvn.Accept();
            vvn.Status.Should().Be(Statuses.Accepted);
        }

        [Fact]
        public void StatusWorkflow_InProgressToSubmittedToRejectedToInProgress()
        {
            // Arrange
            var vvn = CreateTestVVN();

            // Act & Assert
            vvn.Status.Should().Be(Statuses.InProgress);
            
            vvn.Submit();
            vvn.Status.Should().Be(Statuses.Submitted);
            
            vvn.Reject("Invalid data", "officer123");
            vvn.Status.Should().Be(Statuses.Rejected);
            
            vvn.ConvertToDraft();
            vvn.Status.Should().Be(Statuses.InProgress);
        }

        [Fact]
        public void StatusWorkflow_ValidateAllStatusTransitions()
        {
            // Arrange
            var vvn = CreateTestVVN();
            var statuses = new List<Status>();

            // Act - Collect all status transitions
            statuses.Add(vvn.Status); // InProgress
            
            vvn.Submit();
            statuses.Add(vvn.Status); // Submitted
            
            vvn.Reject("Test", "officer");
            statuses.Add(vvn.Status); // Rejected
            
            vvn.Resubmit();
            statuses.Add(vvn.Status); // Submitted
            
            vvn.Accept();
            statuses.Add(vvn.Status); // Accepted

            // Assert
            statuses.Should().HaveCount(5);
            statuses[0].Should().Be(Statuses.InProgress);
            statuses[1].Should().Be(Statuses.Submitted);
            statuses[2].Should().Be(Statuses.Rejected);
            statuses[3].Should().Be(Statuses.Submitted);
            statuses[4].Should().Be(Statuses.Accepted);
        }

        #endregion

        #region Helper Methods

        private ProjArqsi.Domain.VesselVisitNotificationAggregate.VesselVisitNotification CreateTestVVN()
        {
            return new ProjArqsi.Domain.VesselVisitNotificationAggregate.VesselVisitNotification(
                "9366213",
                DateTime.UtcNow.AddDays(5),
                DateTime.UtcNow.AddDays(10)
            );
        }

        private ProjArqsi.Domain.VesselVisitNotificationAggregate.VesselVisitNotification CreateValidSubmittedVVN()
        {
            var vvn = CreateTestVVN();
            vvn.Submit();
            return vvn;
        }

        private ProjArqsi.Domain.VesselVisitNotificationAggregate.VesselVisitNotification CreateRejectedVVN()
        {
            var vvn = CreateValidSubmittedVVN();
            vvn.Reject("Test rejection", "officer123");
            return vvn;
        }

        private CargoManifest CreateLoadingManifest()
        {
            return new CargoManifest(new ManifestType(ManifestTypeEnum.Load));
        }

        private CargoManifest CreateUnloadingManifest()
        {
            return new CargoManifest(new ManifestType(ManifestTypeEnum.Unload));
        }

        #endregion
    }
}
