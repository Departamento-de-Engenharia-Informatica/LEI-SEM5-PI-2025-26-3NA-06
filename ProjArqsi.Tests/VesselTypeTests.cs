using Xunit;
using ProjArqsi.Domain.VesselTypeAggregate;

public class VesselTypeTests
{
    [Fact]
    public void Constructor_ShouldCreateVesselType_WithValidValues()
    {
        // Arrange
        var name = new TypeName("Container Ship");
        var description = new TypeDescription("Large container vessel");
        var capacity = new TypeCapacity(10000);
        var maxRows = new MaxRows(20);
        var maxBays = new MaxBays(10);
        var maxTiers = new MaxTiers(8);

        // Act
        var vesselType = new VesselType(
            new VesselTypeId(Guid.NewGuid()),
            name,
            description,
            capacity,
            maxRows,
            maxBays,
            maxTiers
        );

        // Assert
        Assert.Equal("Container Ship", vesselType.TypeName.Value);
        Assert.Equal("Large container vessel", vesselType.TypeDescription.Value);
        Assert.Equal(10000, vesselType.TypeCapacity.Value);
        Assert.Equal(20, vesselType.MaxRows.Value);
        Assert.Equal(10, vesselType.MaxBays.Value);
        Assert.Equal(8, vesselType.MaxTiers.Value);
    }
}
