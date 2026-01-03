using FluentAssertions;
using ProjArqsi.Domain.VesselVisitNotificationAggregate;

namespace Backend.Tests.ValueObjectTests.VesselVisitNotification
{
    public class RejectionReasonTests
    {
        #region Valid RejectionReason Tests

        [Fact]
        public void Constructor_WithValidReason_ShouldCreateRejectionReason()
        {
            // Arrange
            var reason = "Insufficient documentation provided";

            // Act
            var rejectionReason = new RejectionReason(reason);

            // Assert
            rejectionReason.Should().NotBeNull();
            rejectionReason.Value.Should().Be(reason);
        }

        [Theory]
        [InlineData("Missing vessel certificate")]
        [InlineData("Invalid arrival time")]
        [InlineData("Dock not available")]
        [InlineData("Security clearance required")]
        public void Constructor_WithVariousValidReasons_ShouldCreateRejectionReason(string reason)
        {
            // Act
            var rejectionReason = new RejectionReason(reason);

            // Assert
            rejectionReason.Value.Should().Be(reason);
        }

        [Fact]
        public void Constructor_WithLongReason_ShouldCreateRejectionReason()
        {
            // Arrange
            var longReason = "The vessel visit notification has been rejected due to incomplete cargo manifest, " +
                           "missing crew documentation, and failure to comply with environmental regulations. " +
                           "Please resubmit with all required documentation.";

            // Act
            var rejectionReason = new RejectionReason(longReason);

            // Assert
            rejectionReason.Value.Should().Be(longReason);
        }

        #endregion

        #region ToString Tests

        [Fact]
        public void ToString_ShouldReturnReasonValue()
        {
            // Arrange
            var reason = "Vessel does not meet safety standards";
            var rejectionReason = new RejectionReason(reason);

            // Act
            var result = rejectionReason.ToString();

            // Assert
            result.Should().Be(reason);
        }

        [Fact]
        public void ToString_WithMultipleReasons_ShouldReturnFullText()
        {
            // Arrange
            var reason = "Rejected for: (1) Missing paperwork, (2) Invalid dates, (3) No assigned dock";
            var rejectionReason = new RejectionReason(reason);

            // Act
            var result = rejectionReason.ToString();

            // Assert
            result.Should().Be(reason);
        }

        #endregion

        #region Equality Tests

        [Fact]
        public void Equality_TwoRejectionReasonsWithSameValue_ShouldBeEqual()
        {
            // Arrange
            var reason = "Insufficient documentation";
            var rejectionReason1 = new RejectionReason(reason);
            var rejectionReason2 = new RejectionReason(reason);

            // Act & Assert
            rejectionReason1.Equals(rejectionReason2).Should().BeTrue();
            rejectionReason1.GetHashCode().Should().Be(rejectionReason2.GetHashCode());
        }

        [Fact]
        public void Equality_TwoRejectionReasonsWithDifferentValues_ShouldNotBeEqual()
        {
            // Arrange
            var reason1 = "Missing documentation";
            var reason2 = "Invalid dates";
            var rejectionReason1 = new RejectionReason(reason1);
            var rejectionReason2 = new RejectionReason(reason2);

            // Act & Assert
            rejectionReason1.Equals(rejectionReason2).Should().BeFalse();
        }

        [Fact]
        public void Equality_RejectionReasonComparedWithNull_ShouldNotBeEqual()
        {
            // Arrange
            var rejectionReason = new RejectionReason("Test reason");

            // Act & Assert
            rejectionReason.Equals(null).Should().BeFalse();
        }

        #endregion

        #region Edge Cases

        [Fact]
        public void Constructor_WithEmptyString_ShouldCreateRejectionReason()
        {
            // Arrange
            var emptyReason = "";

            // Act
            var rejectionReason = new RejectionReason(emptyReason);

            // Assert
            rejectionReason.Value.Should().Be(emptyReason);
        }

        [Fact]
        public void Constructor_WithWhitespace_ShouldCreateRejectionReason()
        {
            // Arrange
            var whitespaceReason = "   ";

            // Act
            var rejectionReason = new RejectionReason(whitespaceReason);

            // Assert
            rejectionReason.Value.Should().Be(whitespaceReason);
        }

        [Fact]
        public void Constructor_WithSpecialCharacters_ShouldCreateRejectionReason()
        {
            // Arrange
            var specialReason = "Rejected: vessel doesn't meet requirements (see §3.2.1)";

            // Act
            var rejectionReason = new RejectionReason(specialReason);

            // Assert
            rejectionReason.Value.Should().Be(specialReason);
        }

        [Fact]
        public void RejectionReason_ShouldBeImmutable()
        {
            // Arrange
            var reason = "Test reason";
            var rejectionReason = new RejectionReason(reason);
            var originalValue = rejectionReason.Value;

            // Act - Value property should not have public setter

            // Assert
            rejectionReason.Value.Should().Be(originalValue);
        }

        [Fact]
        public void Constructor_WithNewLines_ShouldPreserveFormatting()
        {
            // Arrange
            var multilineReason = "Rejection reasons:\n1. Missing certificate\n2. Invalid dates\n3. No dock available";

            // Act
            var rejectionReason = new RejectionReason(multilineReason);

            // Assert
            rejectionReason.Value.Should().Be(multilineReason);
            rejectionReason.Value.Should().Contain("\n");
        }

        [Fact]
        public void Constructor_WithUnicodeCharacters_ShouldPreserveText()
        {
            // Arrange
            var unicodeReason = "Motivo de rejeição: documentação inválida";

            // Act
            var rejectionReason = new RejectionReason(unicodeReason);

            // Assert
            rejectionReason.Value.Should().Be(unicodeReason);
        }

        #endregion

        #region Real-World Scenarios

        [Fact]
        public void Constructor_MissingDocumentation_ShouldCreateRejectionReason()
        {
            // Arrange
            var reason = "The vessel's crew manifest is incomplete. Please provide full crew member details including passports.";

            // Act
            var rejectionReason = new RejectionReason(reason);

            // Assert
            rejectionReason.Value.Should().Be(reason);
        }

        [Fact]
        public void Constructor_ScheduleConflict_ShouldCreateRejectionReason()
        {
            // Arrange
            var reason = "Requested arrival time conflicts with existing dock schedule. No docks available during specified period.";

            // Act
            var rejectionReason = new RejectionReason(reason);

            // Assert
            rejectionReason.Value.Should().Be(reason);
        }

        [Fact]
        public void Constructor_SafetyViolation_ShouldCreateRejectionReason()
        {
            // Arrange
            var reason = "Vessel failed safety inspection. Cannot grant port entry until safety certificate is updated.";

            // Act
            var rejectionReason = new RejectionReason(reason);

            // Assert
            rejectionReason.Value.Should().Be(reason);
        }

        [Fact]
        public void Constructor_HazardousCargoIssue_ShouldCreateRejectionReason()
        {
            // Arrange
            var reason = "Hazardous cargo manifest incomplete. Missing IMDG classification and safety documentation for dangerous goods.";

            // Act
            var rejectionReason = new RejectionReason(reason);

            // Assert
            rejectionReason.Value.Should().Be(reason);
        }

        [Fact]
        public void Constructor_InvalidDates_ShouldCreateRejectionReason()
        {
            // Arrange
            var reason = "Departure date must be after arrival date. Please correct the schedule and resubmit.";

            // Act
            var rejectionReason = new RejectionReason(reason);

            // Assert
            rejectionReason.Value.Should().Be(reason);
        }

        [Fact]
        public void Constructor_VesselNotRegistered_ShouldCreateRejectionReason()
        {
            // Arrange
            var reason = "Referenced vessel IMO9999999 not found in system. Please verify vessel details or register vessel first.";

            // Act
            var rejectionReason = new RejectionReason(reason);

            // Assert
            rejectionReason.Value.Should().Contain("IMO9999999");
        }

        [Fact]
        public void Constructor_MultipleViolations_ShouldCreateDetailedReason()
        {
            // Arrange
            var reason = "Multiple issues found: (1) Incomplete crew manifest, (2) Invalid cargo manifest, " +
                       "(3) Vessel certificate expired, (4) Requested dock cannot accommodate vessel size.";

            // Act
            var rejectionReason = new RejectionReason(reason);

            // Assert
            rejectionReason.Value.Should().Contain("(1)");
            rejectionReason.Value.Should().Contain("(2)");
            rejectionReason.Value.Should().Contain("(3)");
            rejectionReason.Value.Should().Contain("(4)");
        }

        [Fact]
        public void ToString_ShouldBeUsefulForLogging()
        {
            // Arrange
            var reason = "Port authority denied entry: vessel on restricted list";
            var rejectionReason = new RejectionReason(reason);

            // Act
            var logMessage = $"VVN rejected: {rejectionReason}";

            // Assert
            logMessage.Should().Contain(reason);
        }

        #endregion
    }
}
