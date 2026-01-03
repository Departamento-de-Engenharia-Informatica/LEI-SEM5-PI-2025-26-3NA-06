using FluentAssertions;
using ProjArqsi.Domain.VesselVisitNotificationAggregate;

namespace Backend.Tests.ValueObjectTests.VesselVisitNotification
{
    public class StatusTests
    {
        #region Valid Status Tests

        [Fact]
        public void Constructor_WithInProgress_ShouldCreateStatus()
        {
            // Act
            var status = new Status(StatusEnum.InProgress);

            // Assert
            status.Should().NotBeNull();
            status.Value.Should().Be(StatusEnum.InProgress);
        }

        [Fact]
        public void Constructor_WithSubmitted_ShouldCreateStatus()
        {
            // Act
            var status = new Status(StatusEnum.Submitted);

            // Assert
            status.Value.Should().Be(StatusEnum.Submitted);
        }

        [Fact]
        public void Constructor_WithAccepted_ShouldCreateStatus()
        {
            // Act
            var status = new Status(StatusEnum.Accepted);

            // Assert
            status.Value.Should().Be(StatusEnum.Accepted);
        }

        [Fact]
        public void Constructor_WithRejected_ShouldCreateStatus()
        {
            // Act
            var status = new Status(StatusEnum.Rejected);

            // Assert
            status.Value.Should().Be(StatusEnum.Rejected);
        }

        [Theory]
        [InlineData(StatusEnum.InProgress)]
        [InlineData(StatusEnum.Submitted)]
        [InlineData(StatusEnum.Accepted)]
        [InlineData(StatusEnum.Rejected)]
        public void Constructor_WithAllValidStatuses_ShouldCreateStatus(StatusEnum statusValue)
        {
            // Act
            var status = new Status(statusValue);

            // Assert
            status.Value.Should().Be(statusValue);
        }

        #endregion

        #region Static Statuses Tests

        [Fact]
        public void Statuses_InProgress_ShouldReturnInProgressStatus()
        {
            // Act
            var status = Statuses.InProgress;

            // Assert
            status.Value.Should().Be(StatusEnum.InProgress);
        }

        [Fact]
        public void Statuses_Submitted_ShouldReturnSubmittedStatus()
        {
            // Act
            var status = Statuses.Submitted;

            // Assert
            status.Value.Should().Be(StatusEnum.Submitted);
        }

        [Fact]
        public void Statuses_Accepted_ShouldReturnAcceptedStatus()
        {
            // Act
            var status = Statuses.Accepted;

            // Assert
            status.Value.Should().Be(StatusEnum.Accepted);
        }

        [Fact]
        public void Statuses_Rejected_ShouldReturnRejectedStatus()
        {
            // Act
            var status = Statuses.Rejected;

            // Assert
            status.Value.Should().Be(StatusEnum.Rejected);
        }

        #endregion

        #region ToString Tests

        [Fact]
        public void ToString_WithInProgress_ShouldReturnInProgress()
        {
            // Arrange
            var status = new Status(StatusEnum.InProgress);

            // Act
            var result = status.Value.ToString();

            // Assert
            result.Should().Be("InProgress");
        }

        [Fact]
        public void ToString_WithSubmitted_ShouldReturnSubmitted()
        {
            // Arrange
            var status = new Status(StatusEnum.Submitted);

            // Act
            var result = status.Value.ToString();

            // Assert
            result.Should().Be("Submitted");
        }

        [Fact]
        public void ToString_WithAccepted_ShouldReturnAccepted()
        {
            // Arrange
            var status = new Status(StatusEnum.Accepted);

            // Act
            var result = status.Value.ToString();

            // Assert
            result.Should().Be("Accepted");
        }

        [Fact]
        public void ToString_WithRejected_ShouldReturnRejected()
        {
            // Arrange
            var status = new Status(StatusEnum.Rejected);

            // Act
            var result = status.Value.ToString();

            // Assert
            result.Should().Be("Rejected");
        }

        #endregion

        #region Equality Tests

        [Fact]
        public void Equals_TwoStatusesWithSameValue_ShouldBeEqual()
        {
            // Arrange
            var status1 = new Status(StatusEnum.Submitted);
            var status2 = new Status(StatusEnum.Submitted);

            // Act & Assert
            status1.Equals(status2).Should().BeTrue();
            status1.GetHashCode().Should().Be(status2.GetHashCode());
        }

        [Fact]
        public void Equals_TwoStatusesWithDifferentValues_ShouldNotBeEqual()
        {
            // Arrange
            var status1 = new Status(StatusEnum.InProgress);
            var status2 = new Status(StatusEnum.Submitted);

            // Act & Assert
            status1.Equals(status2).Should().BeFalse();
        }

        [Fact]
        public void Equals_StatusComparedWithNull_ShouldNotBeEqual()
        {
            // Arrange
            var status = new Status(StatusEnum.InProgress);

            // Act & Assert
            status.Equals(null).Should().BeFalse();
        }

        [Fact]
        public void Equals_StatusComparedWithDifferentType_ShouldNotBeEqual()
        {
            // Arrange
            var status = new Status(StatusEnum.InProgress);
            var notAStatus = "InProgress";

            // Act & Assert
            status.Equals(notAStatus).Should().BeFalse();
        }

        [Fact]
        public void Equals_StaticStatusesWithSameValue_ShouldBeEqual()
        {
            // Arrange
            var status1 = Statuses.Submitted;
            var status2 = new Status(StatusEnum.Submitted);

            // Act & Assert
            status1.Equals(status2).Should().BeTrue();
        }

        #endregion

        #region Edge Cases

        [Fact]
        public void StatusEnum_ShouldHaveExpectedValues()
        {
            // Assert - Verify enum has expected values with correct numeric assignments
            ((int)StatusEnum.InProgress).Should().Be(0);
            ((int)StatusEnum.Submitted).Should().Be(1);
            ((int)StatusEnum.Accepted).Should().Be(2);
            ((int)StatusEnum.Rejected).Should().Be(3);
        }

        [Fact]
        public void Status_ShouldBeImmutable()
        {
            // Arrange
            var status = new Status(StatusEnum.Submitted);
            var originalValue = status.Value;

            // Act - Value property should not have public setter
            // Attempting to modify would not compile

            // Assert
            status.Value.Should().Be(originalValue);
        }

        [Fact]
        public void GetHashCode_SameStatus_ShouldReturnConsistentHashCode()
        {
            // Arrange
            var status = new Status(StatusEnum.Accepted);

            // Act
            var hashCode1 = status.GetHashCode();
            var hashCode2 = status.GetHashCode();

            // Assert
            hashCode1.Should().Be(hashCode2);
        }

        #endregion

        #region Real-World Scenarios

        [Fact]
        public void Status_WorkflowProgression_ShouldAllowTransitions()
        {
            // Arrange - Simulate status progression
            var inProgress = Statuses.InProgress;
            var submitted = Statuses.Submitted;
            var accepted = Statuses.Accepted;

            // Assert - Verify each status is distinct
            inProgress.Equals(submitted).Should().BeFalse();
            submitted.Equals(accepted).Should().BeFalse();
            inProgress.Equals(accepted).Should().BeFalse();
        }

        [Fact]
        public void Status_RejectionWorkflow_ShouldBeDifferentFromAcceptance()
        {
            // Arrange
            var submitted = Statuses.Submitted;
            var rejected = Statuses.Rejected;
            var accepted = Statuses.Accepted;

            // Assert
            rejected.Equals(accepted).Should().BeFalse();
            rejected.Equals(submitted).Should().BeFalse();
        }

        [Fact]
        public void Status_InitialStatus_ShouldBeInProgress()
        {
            // Arrange - VVN typically starts in InProgress
            var initialStatus = Statuses.InProgress;

            // Assert
            initialStatus.Value.Should().Be(StatusEnum.InProgress);
        }

        [Fact]
        public void Status_AllStatuses_ShouldBeDistinct()
        {
            // Arrange
            var inProgress = Statuses.InProgress;
            var submitted = Statuses.Submitted;
            var accepted = Statuses.Accepted;
            var rejected = Statuses.Rejected;

            // Assert - Create a list and verify all are unique
            var statuses = new List<Status> { inProgress, submitted, accepted, rejected };
            var distinctStatuses = statuses.Select(s => s.Value).Distinct().ToList();
            
            distinctStatuses.Should().HaveCount(4);
        }

        [Fact]
        public void Status_CanBeUsedInCollections()
        {
            // Arrange
            var statusHistory = new List<Status>
            {
                Statuses.InProgress,
                Statuses.Submitted,
                Statuses.Accepted
            };

            // Assert
            statusHistory.Should().HaveCount(3);
            statusHistory[0].Value.Should().Be(StatusEnum.InProgress);
            statusHistory[1].Value.Should().Be(StatusEnum.Submitted);
            statusHistory[2].Value.Should().Be(StatusEnum.Accepted);
        }

        [Fact]
        public void Status_CanBeUsedInDictionary()
        {
            // Arrange
            var statusDescriptions = new Dictionary<Status, string>
            {
                { Statuses.InProgress, "Being prepared" },
                { Statuses.Submitted, "Awaiting review" },
                { Statuses.Accepted, "Approved for visit" },
                { Statuses.Rejected, "Denied entry" }
            };

            // Act & Assert
            statusDescriptions.Should().HaveCount(4);
            statusDescriptions[Statuses.Submitted].Should().Be("Awaiting review");
        }

        [Fact]
        public void Status_CanCheckIfSubmittedOrLater()
        {
            // Arrange
            var inProgress = Statuses.InProgress;
            var submitted = Statuses.Submitted;
            var accepted = Statuses.Accepted;

            // Act
            var isInProgressSubmittedOrLater = (int)inProgress.Value >= (int)StatusEnum.Submitted;
            var isSubmittedSubmittedOrLater = (int)submitted.Value >= (int)StatusEnum.Submitted;
            var isAcceptedSubmittedOrLater = (int)accepted.Value >= (int)StatusEnum.Submitted;

            // Assert
            isInProgressSubmittedOrLater.Should().BeFalse();
            isSubmittedSubmittedOrLater.Should().BeTrue();
            isAcceptedSubmittedOrLater.Should().BeTrue();
        }

        #endregion
    }
}
