using FluentAssertions;
using ProjArqsi.Domain.Shared;
using ProjArqsi.Domain.VesselTypeAggregate;
using Xunit;

namespace Backend.Tests.AggregateTests.VesselType
{
    public class VesselTypeTests
    {
        #region Constructor Tests

        [Fact]
        public void Constructor_WithValidParameters_ShouldCreateVesselType()
        {
            // Arrange
            var typeName = new TypeName("Container Ship");
            var typeDescription = new TypeDescription("Large cargo vessel designed for container transport");
            var typeCapacity = new TypeCapacity(20000);
            var maxRows = new MaxRows(24);
            var maxBays = new MaxBays(22);
            var maxTiers = new MaxTiers(10);

            // Act
            var vesselType = new ProjArqsi.Domain.VesselTypeAggregate.VesselType(
                typeName, typeDescription, typeCapacity, maxRows, maxBays, maxTiers);

            // Assert
            vesselType.Should().NotBeNull();
            vesselType.Id.Should().NotBeNull();
            vesselType.TypeName.Should().Be(typeName);
            vesselType.TypeDescription.Should().Be(typeDescription);
            vesselType.TypeCapacity.Should().Be(typeCapacity);
            vesselType.MaxRows.Should().Be(maxRows);
            vesselType.MaxBays.Should().Be(maxBays);
            vesselType.MaxTiers.Should().Be(maxTiers);
        }

        [Fact]
        public void Constructor_ShouldGenerateUniqueIds()
        {
            // Arrange
            var typeName = new TypeName("Bulk Carrier");
            var typeDescription = new TypeDescription("Vessel for bulk cargo");
            var typeCapacity = new TypeCapacity(15000);
            var maxRows = new MaxRows(20);
            var maxBays = new MaxBays(18);
            var maxTiers = new MaxTiers(8);

            // Act
            var vesselType1 = new ProjArqsi.Domain.VesselTypeAggregate.VesselType(
                typeName, typeDescription, typeCapacity, maxRows, maxBays, maxTiers);
            var vesselType2 = new ProjArqsi.Domain.VesselTypeAggregate.VesselType(
                typeName, typeDescription, typeCapacity, maxRows, maxBays, maxTiers);

            // Assert
            vesselType1.Id.Should().NotBe(vesselType2.Id);
        }

        [Fact]
        public void Constructor_WithNullTypeName_ShouldThrowException()
        {
            // Arrange
            var typeDescription = new TypeDescription("Test description");
            var typeCapacity = new TypeCapacity(10000);
            var maxRows = new MaxRows(20);
            var maxBays = new MaxBays(18);
            var maxTiers = new MaxTiers(8);

            // Act & Assert
            var act = () => new ProjArqsi.Domain.VesselTypeAggregate.VesselType(
                null!, typeDescription, typeCapacity, maxRows, maxBays, maxTiers);

            act.Should().Throw<BusinessRuleValidationException>()
                .WithMessage("*Type name*");
        }

        [Fact]
        public void Constructor_WithNullTypeDescription_ShouldThrowException()
        {
            // Arrange
            var typeName = new TypeName("Test Type");
            var typeCapacity = new TypeCapacity(10000);
            var maxRows = new MaxRows(20);
            var maxBays = new MaxBays(18);
            var maxTiers = new MaxTiers(8);

            // Act & Assert
            var act = () => new ProjArqsi.Domain.VesselTypeAggregate.VesselType(
                typeName, null!, typeCapacity, maxRows, maxBays, maxTiers);

            act.Should().Throw<BusinessRuleValidationException>()
                .WithMessage("*Type description*");
        }

        [Fact]
        public void Constructor_WithNullTypeCapacity_ShouldThrowException()
        {
            // Arrange
            var typeName = new TypeName("Test Type");
            var typeDescription = new TypeDescription("Test description");
            var maxRows = new MaxRows(20);
            var maxBays = new MaxBays(18);
            var maxTiers = new MaxTiers(8);

            // Act & Assert
            var act = () => new ProjArqsi.Domain.VesselTypeAggregate.VesselType(
                typeName, typeDescription, null!, maxRows, maxBays, maxTiers);

            act.Should().Throw<BusinessRuleValidationException>()
                .WithMessage("*Type capacity*");
        }

        [Fact]
        public void Constructor_WithNullMaxRows_ShouldThrowException()
        {
            // Arrange
            var typeName = new TypeName("Test Type");
            var typeDescription = new TypeDescription("Test description");
            var typeCapacity = new TypeCapacity(10000);
            var maxBays = new MaxBays(18);
            var maxTiers = new MaxTiers(8);

            // Act & Assert
            var act = () => new ProjArqsi.Domain.VesselTypeAggregate.VesselType(
                typeName, typeDescription, typeCapacity, null!, maxBays, maxTiers);

            act.Should().Throw<BusinessRuleValidationException>()
                .WithMessage("*Max rows*");
        }

        [Fact]
        public void Constructor_WithNullMaxBays_ShouldThrowException()
        {
            // Arrange
            var typeName = new TypeName("Test Type");
            var typeDescription = new TypeDescription("Test description");
            var typeCapacity = new TypeCapacity(10000);
            var maxRows = new MaxRows(20);
            var maxTiers = new MaxTiers(8);

            // Act & Assert
            var act = () => new ProjArqsi.Domain.VesselTypeAggregate.VesselType(
                typeName, typeDescription, typeCapacity, maxRows, null!, maxTiers);

            act.Should().Throw<BusinessRuleValidationException>()
                .WithMessage("*Max bays*");
        }

        [Fact]
        public void Constructor_WithNullMaxTiers_ShouldThrowException()
        {
            // Arrange
            var typeName = new TypeName("Test Type");
            var typeDescription = new TypeDescription("Test description");
            var typeCapacity = new TypeCapacity(10000);
            var maxRows = new MaxRows(20);
            var maxBays = new MaxBays(18);

            // Act & Assert
            var act = () => new ProjArqsi.Domain.VesselTypeAggregate.VesselType(
                typeName, typeDescription, typeCapacity, maxRows, maxBays, null!);

            act.Should().Throw<BusinessRuleValidationException>()
                .WithMessage("*Max tiers*");
        }

        #endregion

        #region UpdateDetails Tests

        [Fact]
        public void UpdateDetails_WithValidParameters_ShouldUpdateProperties()
        {
            // Arrange
            var vesselType = CreateTestVesselType();
            var newTypeName = new TypeName("Updated Type");
            var newTypeDescription = new TypeDescription("Updated description");
            var newTypeCapacity = new TypeCapacity(25000);
            var newMaxRows = new MaxRows(26);
            var newMaxBays = new MaxBays(24);
            var newMaxTiers = new MaxTiers(12);

            // Act
            vesselType.UpdateDetails(newTypeName, newTypeDescription, newTypeCapacity, 
                                    newMaxRows, newMaxBays, newMaxTiers);

            // Assert
            vesselType.TypeName.Should().Be(newTypeName);
            vesselType.TypeDescription.Should().Be(newTypeDescription);
            vesselType.TypeCapacity.Should().Be(newTypeCapacity);
            vesselType.MaxRows.Should().Be(newMaxRows);
            vesselType.MaxBays.Should().Be(newMaxBays);
            vesselType.MaxTiers.Should().Be(newMaxTiers);
        }

        [Fact]
        public void UpdateDetails_ShouldPreserveId()
        {
            // Arrange
            var vesselType = CreateTestVesselType();
            var originalId = vesselType.Id;

            // Act
            vesselType.UpdateDetails(
                new TypeName("New Name"),
                new TypeDescription("New Description"),
                new TypeCapacity(20000),
                new MaxRows(22),
                new MaxBays(20),
                new MaxTiers(9));

            // Assert
            vesselType.Id.Should().Be(originalId);
        }

        [Fact]
        public void UpdateDetails_WithNullTypeName_ShouldThrowException()
        {
            // Arrange
            var vesselType = CreateTestVesselType();

            // Act & Assert
            var act = () => vesselType.UpdateDetails(
                null!,
                new TypeDescription("Description"),
                new TypeCapacity(20000),
                new MaxRows(22),
                new MaxBays(20),
                new MaxTiers(9));

            act.Should().Throw<BusinessRuleValidationException>()
                .WithMessage("*Type name*");
        }

        [Fact]
        public void UpdateDetails_WithNullTypeDescription_ShouldThrowException()
        {
            // Arrange
            var vesselType = CreateTestVesselType();

            // Act & Assert
            var act = () => vesselType.UpdateDetails(
                new TypeName("Name"),
                null!,
                new TypeCapacity(20000),
                new MaxRows(22),
                new MaxBays(20),
                new MaxTiers(9));

            act.Should().Throw<BusinessRuleValidationException>()
                .WithMessage("*Type description*");
        }

        [Fact]
        public void UpdateDetails_WithNullTypeCapacity_ShouldThrowException()
        {
            // Arrange
            var vesselType = CreateTestVesselType();

            // Act & Assert
            var act = () => vesselType.UpdateDetails(
                new TypeName("Name"),
                new TypeDescription("Description"),
                null!,
                new MaxRows(22),
                new MaxBays(20),
                new MaxTiers(9));

            act.Should().Throw<BusinessRuleValidationException>()
                .WithMessage("*Type capacity*");
        }

        [Fact]
        public void UpdateDetails_WithNullMaxRows_ShouldThrowException()
        {
            // Arrange
            var vesselType = CreateTestVesselType();

            // Act & Assert
            var act = () => vesselType.UpdateDetails(
                new TypeName("Name"),
                new TypeDescription("Description"),
                new TypeCapacity(20000),
                null!,
                new MaxBays(20),
                new MaxTiers(9));

            act.Should().Throw<BusinessRuleValidationException>()
                .WithMessage("*Max rows*");
        }

        [Fact]
        public void UpdateDetails_WithNullMaxBays_ShouldThrowException()
        {
            // Arrange
            var vesselType = CreateTestVesselType();

            // Act & Assert
            var act = () => vesselType.UpdateDetails(
                new TypeName("Name"),
                new TypeDescription("Description"),
                new TypeCapacity(20000),
                new MaxRows(22),
                null!,
                new MaxTiers(9));

            act.Should().Throw<BusinessRuleValidationException>()
                .WithMessage("*Max bays*");
        }

        [Fact]
        public void UpdateDetails_WithNullMaxTiers_ShouldThrowException()
        {
            // Arrange
            var vesselType = CreateTestVesselType();

            // Act & Assert
            var act = () => vesselType.UpdateDetails(
                new TypeName("Name"),
                new TypeDescription("Description"),
                new TypeCapacity(20000),
                new MaxRows(22),
                new MaxBays(20),
                null!);

            act.Should().Throw<BusinessRuleValidationException>()
                .WithMessage("*Max tiers*");
        }

        [Fact]
        public void UpdateDetails_CalledMultipleTimes_ShouldApplyLatestValues()
        {
            // Arrange
            var vesselType = CreateTestVesselType();

            // Act
            vesselType.UpdateDetails(
                new TypeName("Update 1"),
                new TypeDescription("First update"),
                new TypeCapacity(16000),
                new MaxRows(21),
                new MaxBays(19),
                new MaxTiers(8));

            vesselType.UpdateDetails(
                new TypeName("Update 2"),
                new TypeDescription("Second update"),
                new TypeCapacity(18000),
                new MaxRows(23),
                new MaxBays(21),
                new MaxTiers(10));

            // Assert
            vesselType.TypeName.Value.Should().Be("Update 2");
            vesselType.TypeDescription.Value.Should().Be("Second update");
            vesselType.TypeCapacity.Value.Should().Be(18000);
        }

        #endregion

        #region Real-World Scenarios

        [Fact]
        public void Scenario_UltraLargeContainerVessel_ULCV()
        {
            // Arrange - Modern ULCV type with extreme capacity
            var typeName = new TypeName("Ultra Large Container Vessel");
            var typeDescription = new TypeDescription("Largest class of container ships, 24000+ TEU capacity");
            var typeCapacity = new TypeCapacity(24000);
            var maxRows = new MaxRows(25);
            var maxBays = new MaxBays(24);
            var maxTiers = new MaxTiers(11);

            // Act
            var vesselType = new ProjArqsi.Domain.VesselTypeAggregate.VesselType(
                typeName, typeDescription, typeCapacity, maxRows, maxBays, maxTiers);

            // Assert
            vesselType.TypeCapacity.Value.Should().BeGreaterThan(20000);
            vesselType.MaxTiers.Value.Should().BeGreaterThanOrEqualTo(10);
            vesselType.TypeName.Value.Should().Contain("Ultra Large");
        }

        [Fact]
        public void Scenario_FeederVesselType()
        {
            // Arrange - Small feeder vessel type
            var typeName = new TypeName("Feeder");
            var typeDescription = new TypeDescription("Small container vessels for short-sea routes");
            var typeCapacity = new TypeCapacity(1000);
            var maxRows = new MaxRows(13);
            var maxBays = new MaxBays(10);
            var maxTiers = new MaxTiers(5);

            // Act
            var vesselType = new ProjArqsi.Domain.VesselTypeAggregate.VesselType(
                typeName, typeDescription, typeCapacity, maxRows, maxBays, maxTiers);

            // Assert
            vesselType.TypeCapacity.Value.Should().BeLessThan(2000);
            vesselType.MaxTiers.Value.Should().BeLessThanOrEqualTo(6);
            vesselType.TypeName.Value.Should().Be("Feeder");
        }

        [Fact]
        public void Scenario_PanamaxType_ClassicDimensions()
        {
            // Arrange - Panamax class for original Panama Canal
            var typeName = new TypeName("Panamax");
            var typeDescription = new TypeDescription("Vessels sized to fit original Panama Canal locks");
            var typeCapacity = new TypeCapacity(5000);
            var maxRows = new MaxRows(17);
            var maxBays = new MaxBays(17);
            var maxTiers = new MaxTiers(7);

            // Act
            var vesselType = new ProjArqsi.Domain.VesselTypeAggregate.VesselType(
                typeName, typeDescription, typeCapacity, maxRows, maxBays, maxTiers);

            // Assert
            vesselType.TypeCapacity.Value.Should().BeInRange(4000, 5500);
            vesselType.TypeName.Value.Should().Contain("Panamax");
        }

        [Fact]
        public void Scenario_NewPanamaxType_ExpandedCanal()
        {
            // Arrange - New Panamax for expanded canal
            var typeName = new TypeName("New Panamax");
            var typeDescription = new TypeDescription("Post-Panamax vessels for expanded canal locks");
            var typeCapacity = new TypeCapacity(13000);
            var maxRows = new MaxRows(20);
            var maxBays = new MaxBays(20);
            var maxTiers = new MaxTiers(9);

            // Act
            var vesselType = new ProjArqsi.Domain.VesselTypeAggregate.VesselType(
                typeName, typeDescription, typeCapacity, maxRows, maxBays, maxTiers);

            // Assert
            vesselType.TypeCapacity.Value.Should().BeInRange(10000, 14500);
            vesselType.TypeName.Value.Should().Contain("New Panamax");
        }

        [Fact]
        public void Scenario_VesselTypeSpecificationUpdate()
        {
            // Arrange - Type specs need updating based on industry standards
            var vesselType = CreateTestVesselType();
            var originalCapacity = vesselType.TypeCapacity.Value;

            // Act - Update to reflect new container stacking regulations
            vesselType.UpdateDetails(
                vesselType.TypeName,
                new TypeDescription("Updated to meet new IMO regulations"),
                new TypeCapacity(originalCapacity + 2000),
                new MaxRows(vesselType.MaxRows.Value + 2),
                vesselType.MaxBays,
                new MaxTiers(vesselType.MaxTiers.Value + 1));

            // Assert
            vesselType.TypeCapacity.Value.Should().BeGreaterThan(originalCapacity);
            vesselType.TypeDescription.Value.Should().Contain("IMO regulations");
        }

        [Fact]
        public void Scenario_RenameVesselTypeCategory()
        {
            // Arrange
            var vesselType = CreateTestVesselType();
            var originalName = vesselType.TypeName.Value;

            // Act - Rename based on industry classification change
            vesselType.UpdateDetails(
                new TypeName("Post-Panamax Plus"),
                vesselType.TypeDescription,
                vesselType.TypeCapacity,
                vesselType.MaxRows,
                vesselType.MaxBays,
                vesselType.MaxTiers);

            // Assert
            vesselType.TypeName.Value.Should().NotBe(originalName);
            vesselType.TypeName.Value.Should().Be("Post-Panamax Plus");
        }

        [Fact]
        public void Scenario_SubPanamaxType()
        {
            // Arrange - Smaller than Panamax
            var typeName = new TypeName("Sub-Panamax");
            var typeDescription = new TypeDescription("Container ships smaller than Panamax standard");
            var typeCapacity = new TypeCapacity(3000);
            var maxRows = new MaxRows(15);
            var maxBays = new MaxBays(14);
            var maxTiers = new MaxTiers(6);

            // Act
            var vesselType = new ProjArqsi.Domain.VesselTypeAggregate.VesselType(
                typeName, typeDescription, typeCapacity, maxRows, maxBays, maxTiers);

            // Assert
            vesselType.TypeCapacity.Value.Should().BeLessThan(4000);
            vesselType.TypeName.Value.Should().Contain("Sub-Panamax");
        }

        [Fact]
        public void Scenario_TripleEClassType()
        {
            // Arrange - Maersk Triple E specifications
            var typeName = new TypeName("Triple-E Class");
            var typeDescription = new TypeDescription("Economy of scale, Energy efficiency, Environmentally improved");
            var typeCapacity = new TypeCapacity(18000);
            var maxRows = new MaxRows(23);
            var maxBays = new MaxBays(23);
            var maxTiers = new MaxTiers(10);

            // Act
            var vesselType = new ProjArqsi.Domain.VesselTypeAggregate.VesselType(
                typeName, typeDescription, typeCapacity, maxRows, maxBays, maxTiers);

            // Assert
            vesselType.TypeCapacity.Value.Should().BeGreaterThanOrEqualTo(18000);
            vesselType.TypeDescription.Value.Should().Contain("Economy");
            vesselType.TypeName.Value.Should().Contain("Triple-E");
        }

        #endregion

        #region Identity and Equality Tests

        [Fact]
        public void VesselTypes_WithSameId_ShouldBeEqual()
        {
            // Arrange
            var vesselType1 = CreateTestVesselType();

            // Act - Compare with itself
            var result = vesselType1.Equals(vesselType1);

            // Assert
            result.Should().BeTrue();
            vesselType1.GetHashCode().Should().Be(vesselType1.GetHashCode());
        }

        [Fact]
        public void VesselTypes_WithDifferentIds_ShouldNotBeEqual()
        {
            // Arrange
            var vesselType1 = CreateTestVesselType();
            var vesselType2 = CreateTestVesselType();

            // Act & Assert
            vesselType1.Should().NotBe(vesselType2);
        }

        [Fact]
        public void VesselTypeId_ShouldBeImmutable()
        {
            // Arrange
            var vesselType = CreateTestVesselType();
            var originalId = vesselType.Id;

            // Act - Update details
            vesselType.UpdateDetails(
                new TypeName("New Name"),
                new TypeDescription("New Description"),
                new TypeCapacity(20000),
                new MaxRows(22),
                new MaxBays(20),
                new MaxTiers(9));

            // Assert
            vesselType.Id.Should().Be(originalId);
        }

        #endregion

        #region Edge Cases

        [Fact]
        public void EdgeCase_MinimumViableVesselType()
        {
            // Arrange - Smallest realistic vessel type
            var typeName = new TypeName("Mini");
            var typeDescription = new TypeDescription("Minimal");
            var typeCapacity = new TypeCapacity(100);
            var maxRows = new MaxRows(5);
            var maxBays = new MaxBays(5);
            var maxTiers = new MaxTiers(3);

            // Act
            var vesselType = new ProjArqsi.Domain.VesselTypeAggregate.VesselType(
                typeName, typeDescription, typeCapacity, maxRows, maxBays, maxTiers);

            // Assert
            vesselType.Should().NotBeNull();
            vesselType.TypeCapacity.Value.Should().Be(100);
        }

        [Fact]
        public void EdgeCase_MaximumCapacityVesselType()
        {
            // Arrange - Hypothetical future mega vessel type
            var typeName = new TypeName("Megamax");
            var typeDescription = new TypeDescription("Future ultra-high capacity vessel type");
            var typeCapacity = new TypeCapacity(30000);
            var maxRows = new MaxRows(30);
            var maxBays = new MaxBays(30);
            var maxTiers = new MaxTiers(15);

            // Act
            var vesselType = new ProjArqsi.Domain.VesselTypeAggregate.VesselType(
                typeName, typeDescription, typeCapacity, maxRows, maxBays, maxTiers);

            // Assert
            vesselType.Should().NotBeNull();
            vesselType.TypeCapacity.Value.Should().Be(30000);
        }

        [Fact]
        public void EdgeCase_VeryLongTypeName()
        {
            // Arrange
            var vesselType = CreateTestVesselType();
            var longName = new TypeName("Ultra Large Post-Panamax New Generation Container Vessel Type");

            // Act
            vesselType.UpdateDetails(
                longName,
                vesselType.TypeDescription,
                vesselType.TypeCapacity,
                vesselType.MaxRows,
                vesselType.MaxBays,
                vesselType.MaxTiers);

            // Assert
            vesselType.TypeName.Should().Be(longName);
        }

        [Fact]
        public void EdgeCase_VeryLongDescription()
        {
            // Arrange
            var vesselType = CreateTestVesselType();
            var longDescription = new TypeDescription(
                "This is an extremely detailed description of a vessel type that includes " +
                "comprehensive information about its design, capacity, operational characteristics, " +
                "historical context, and technical specifications for maritime logistics operations.");

            // Act
            vesselType.UpdateDetails(
                vesselType.TypeName,
                longDescription,
                vesselType.TypeCapacity,
                vesselType.MaxRows,
                vesselType.MaxBays,
                vesselType.MaxTiers);

            // Assert
            vesselType.TypeDescription.Should().Be(longDescription);
        }

        #endregion

        #region Multiple Update Tests

        [Fact]
        public void MultipleUpdates_ShouldApplyAllChangesSequentially()
        {
            // Arrange
            var vesselType = CreateTestVesselType();

            // Act - Perform 10 sequential updates
            for (int i = 1; i <= 10; i++)
            {
                vesselType.UpdateDetails(
                    new TypeName($"Type {i}"),
                    new TypeDescription($"Description {i}"),
                    new TypeCapacity(10000 + (i * 1000)),
                    new MaxRows(20 + i),
                    new MaxBays(18 + i),
                    new MaxTiers(8 + i));
            }

            // Assert
            vesselType.TypeName.Value.Should().Be("Type 10");
            vesselType.TypeDescription.Value.Should().Be("Description 10");
            vesselType.TypeCapacity.Value.Should().Be(20000);
            vesselType.MaxRows.Value.Should().Be(30);
        }

        #endregion

        #region Helper Methods

        private ProjArqsi.Domain.VesselTypeAggregate.VesselType CreateTestVesselType()
        {
            return new ProjArqsi.Domain.VesselTypeAggregate.VesselType(
                new TypeName("Standard Container Ship"),
                new TypeDescription("General purpose container vessel type"),
                new TypeCapacity(15000),
                new MaxRows(22),
                new MaxBays(20),
                new MaxTiers(9)
            );
        }

        #endregion
    }
}
