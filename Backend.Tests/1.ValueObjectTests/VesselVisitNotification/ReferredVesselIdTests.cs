using FluentAssertions;
using ProjArqsi.Domain.VesselAggregate;
using ProjArqsi.Domain.VesselVisitNotificationAggregate;
using ProjArqsi.Domain.Shared;

namespace Backend.Tests.ValueObjectTests.VesselVisitNotification
{
    public class ReferredVesselIdTests
    {
        #region Valid ReferredVesselId Tests

        [Fact]
        public void Constructor_WithValidIMOnumber_ShouldCreateReferredVesselId()
        {
            // Arrange
            var imoNumber = new IMOnumber("9074729");

            // Act
            var referredVesselId = new ReferredVesselId(imoNumber);

            // Assert
            referredVesselId.Should().NotBeNull();
            referredVesselId.VesselId.Should().Be(imoNumber);
        }

        [Theory]
        [InlineData("9074729")]
        [InlineData("1234567")]
        [InlineData("9811000")]
        public void Constructor_WithVariousValidIMOnumbers_ShouldCreateReferredVesselId(string imoString)
        {
            // Arrange
            var imoNumber = new IMOnumber(imoString);

            // Act
            var referredVesselId = new ReferredVesselId(imoNumber);

            // Assert
            referredVesselId.VesselId.Value.Should().Be(imoString);
        }

        #endregion

        #region Edge Cases

        [Fact]
        public void ReferredVesselId_ShouldBeImmutable()
        {
            // Arrange
            var imoNumber = new IMOnumber("9074729");
            var referredVesselId = new ReferredVesselId(imoNumber);
            var originalVesselId = referredVesselId.VesselId;

            // Act - VesselId property should not have public setter

            // Assert
            referredVesselId.VesselId.Should().Be(originalVesselId);
        }

        [Fact]
        public void Constructor_WithIMOnumberFromString_ShouldPreserveValue()
        {
            // Arrange
            var imoString = "9074729";
            var imoNumber = new IMOnumber(imoString);

            // Act
            var referredVesselId = new ReferredVesselId(imoNumber);

            // Assert
            referredVesselId.VesselId.Value.Should().Be(imoString);
        }

        #endregion

        #region Real-World Scenarios

        [Fact]
        public void Constructor_ForMSCOscar_ShouldCreateReferredVesselId()
        {
            // Arrange - MSC Oscar (one of the world's largest container ships)
            var imoNumber = new IMOnumber("9703291");

            // Act
            var referredVesselId = new ReferredVesselId(imoNumber);

            // Assert
            referredVesselId.VesselId.Value.Should().Be("9703291");
        }

        [Fact]
        public void Constructor_ForEverGiven_ShouldCreateReferredVesselId()
        {
            // Arrange - Ever Given (famous for Suez Canal incident)
            var imoNumber = new IMOnumber("9811000");

            // Act
            var referredVesselId = new ReferredVesselId(imoNumber);

            // Assert
            referredVesselId.VesselId.Value.Should().Be("9811000");
        }

        [Fact]
        public void Constructor_ForMultipleVessels_ShouldCreateDistinctReferences()
        {
            // Arrange
            var imo1 = new IMOnumber("9074729");
            var imo2 = new IMOnumber("9703291");
            var imo3 = new IMOnumber("9811000");

            // Act
            var vessel1 = new ReferredVesselId(imo1);
            var vessel2 = new ReferredVesselId(imo2);
            var vessel3 = new ReferredVesselId(imo3);

            // Assert
            vessel1.VesselId.Should().NotBe(vessel2.VesselId);
            vessel2.VesselId.Should().NotBe(vessel3.VesselId);
            vessel1.VesselId.Should().NotBe(vessel3.VesselId);
        }

        [Fact]
        public void Constructor_CanReferenceExistingVessel_ShouldStoreIMOnumber()
        {
            // Arrange - Simulating reference to an existing vessel in the system
            var vesselImo = new IMOnumber("9074729");

            // Act
            var referredVesselId = new ReferredVesselId(vesselImo);

            // Assert
            referredVesselId.VesselId.Should().Be(vesselImo);
        }

        [Fact]
        public void Constructor_ForVVNCreation_ShouldAcceptIMOnumber()
        {
            // Arrange - Typical scenario when creating a VVN
            var imoNumber = new IMOnumber("9074729");

            // Act
            var referredVesselId = new ReferredVesselId(imoNumber);

            // Assert
            referredVesselId.Should().NotBeNull();
            referredVesselId.VesselId.Value.Should().Be("9074729");
        }

        [Fact]
        public void ReferredVesselId_CanBeAccessedMultipleTimes_ShouldReturnSameValue()
        {
            // Arrange
            var imoNumber = new IMOnumber("9074729");
            var referredVesselId = new ReferredVesselId(imoNumber);

            // Act
            var firstAccess = referredVesselId.VesselId;
            var secondAccess = referredVesselId.VesselId;
            var thirdAccess = referredVesselId.VesselId;

            // Assert
            firstAccess.Should().Be(secondAccess);
            secondAccess.Should().Be(thirdAccess);
        }

        #endregion
    }
}
