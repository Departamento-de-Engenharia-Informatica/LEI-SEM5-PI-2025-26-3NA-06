using Xunit;
using ProjArqsi.Domain.VesselAggregate;

namespace ProjArqsi.Tests.Domain
{
    public class VesselTests
    {
        [Fact]
        public void CreateVessel_WithValidData_ShouldSucceed()
        {
            // Arrange
            var imo = new IMO("IMO1234567");
            var ownerId = Guid.NewGuid();
            var vesselTypeId = Guid.NewGuid();
            var vesselName = new VesselName("Test Vessel");
            var maxTeu = new MaxTeu(1000);
            var size = new Size(200);
            var cargoCapacity = new CargoCapacity(50000);

            // Act
            var vessel = new Vessel(imo, ownerId, vesselTypeId, vesselName, maxTeu, size, cargoCapacity);

            // Assert
            Assert.NotNull(vessel);
            Assert.Equal(imo, vessel.Id);
            Assert.Equal(vesselName, vessel.VesselName);
            Assert.Equal(maxTeu, vessel.MaxTeu);
            Assert.Equal(size, vessel.Size);
            Assert.Equal(cargoCapacity, vessel.CargoCapacity);
            Assert.Equal(ownerId, vessel.OwnerId);
            Assert.Equal(vesselTypeId, vessel.VesselTypeId);
        }

        [Fact]
        public void IMO_ShouldCreateFromString()
        {
            // Arrange
            string imoNumber = "IMO9876543";

            // Act
            var imo = new IMO(imoNumber);

            // Assert
            Assert.NotNull(imo);
            Assert.Equal(imoNumber, imo.AsString());
        }

        [Fact]
        public void VesselName_ShouldStoreValue()
        {
            // Arrange
            string name = "Cargo Ship Alpha";

            // Act
            var vesselName = new VesselName(name);

            // Assert
            Assert.NotNull(vesselName);
            Assert.Equal(name, vesselName.Name);
        }
    }
}
