using FluentAssertions;
using ProjArqsi.Domain.VesselVisitNotificationAggregate;
using ProjArqsi.Domain.Shared;

namespace Backend.Tests.ValueObjectTests.VesselVisitNotification
{
    public class ManifestTypeTests
    {
        #region Valid ManifestType Tests

        [Fact]
        public void Constructor_WithLoad_ShouldCreateManifestType()
        {
            // Act
            var manifestType = new ManifestType(ManifestTypeEnum.Load);

            // Assert
            manifestType.Should().NotBeNull();
            manifestType.Value.Should().Be(ManifestTypeEnum.Load);
        }

        [Fact]
        public void Constructor_WithUnload_ShouldCreateManifestType()
        {
            // Act
            var manifestType = new ManifestType(ManifestTypeEnum.Unload);

            // Assert
            manifestType.Value.Should().Be(ManifestTypeEnum.Unload);
        }

        [Theory]
        [InlineData(ManifestTypeEnum.Load)]
        [InlineData(ManifestTypeEnum.Unload)]
        public void Constructor_WithAllValidTypes_ShouldCreateManifestType(ManifestTypeEnum type)
        {
            // Act
            var manifestType = new ManifestType(type);

            // Assert
            manifestType.Value.Should().Be(type);
        }

        #endregion

        #region Invalid ManifestType Tests

        [Fact]
        public void Constructor_WithInvalidEnum_ShouldThrowBusinessRuleValidationException()
        {
            // Arrange
            var invalidType = (ManifestTypeEnum)999;

            // Act
            Action act = () => new ManifestType(invalidType);

            // Assert
            act.Should().Throw<BusinessRuleValidationException>()
                .WithMessage("Invalid manifest type.");
        }

        [Fact]
        public void Constructor_WithUndefinedEnum_ShouldThrowBusinessRuleValidationException()
        {
            // Arrange
            var undefinedType = (ManifestTypeEnum)(-1);

            // Act
            Action act = () => new ManifestType(undefinedType);

            // Assert
            act.Should().Throw<BusinessRuleValidationException>()
                .WithMessage("Invalid manifest type.");
        }

        #endregion

        #region Static Factory Properties Tests

        [Fact]
        public void StaticLoad_ShouldReturnLoadManifestType()
        {
            // Act
            var manifestType = ManifestType.Load;

            // Assert
            manifestType.Value.Should().Be(ManifestTypeEnum.Load);
        }

        [Fact]
        public void StaticUnload_ShouldReturnUnloadManifestType()
        {
            // Act
            var manifestType = ManifestType.Unload;

            // Assert
            manifestType.Value.Should().Be(ManifestTypeEnum.Unload);
        }

        [Fact]
        public void StaticFactories_ShouldCreateNewInstances()
        {
            // Act
            var load1 = ManifestType.Load;
            var load2 = ManifestType.Load;

            // Assert - Each call creates a new instance
            load1.Should().NotBeSameAs(load2);
            load1.Equals(load2).Should().BeTrue();
        }

        #endregion

        #region ToString Tests

        [Fact]
        public void ToString_WithLoad_ShouldReturnLoad()
        {
            // Arrange
            var manifestType = new ManifestType(ManifestTypeEnum.Load);

            // Act
            var result = manifestType.ToString();

            // Assert
            result.Should().Be("Load");
        }

        [Fact]
        public void ToString_WithUnload_ShouldReturnUnload()
        {
            // Arrange
            var manifestType = new ManifestType(ManifestTypeEnum.Unload);

            // Act
            var result = manifestType.ToString();

            // Assert
            result.Should().Be("Unload");
        }

        [Fact]
        public void ToString_UsingStaticLoad_ShouldReturnLoad()
        {
            // Arrange
            var manifestType = ManifestType.Load;

            // Act
            var result = manifestType.ToString();

            // Assert
            result.Should().Be("Load");
        }

        #endregion

        #region Equality Tests

        [Fact]
        public void Equality_TwoManifestTypesWithSameValue_ShouldBeEqual()
        {
            // Arrange
            var manifestType1 = new ManifestType(ManifestTypeEnum.Load);
            var manifestType2 = new ManifestType(ManifestTypeEnum.Load);

            // Act & Assert
            manifestType1.Equals(manifestType2).Should().BeTrue();
            manifestType1.GetHashCode().Should().Be(manifestType2.GetHashCode());
        }

        [Fact]
        public void Equality_TwoManifestTypesWithDifferentValues_ShouldNotBeEqual()
        {
            // Arrange
            var manifestType1 = new ManifestType(ManifestTypeEnum.Load);
            var manifestType2 = new ManifestType(ManifestTypeEnum.Unload);

            // Act & Assert
            manifestType1.Equals(manifestType2).Should().BeFalse();
        }

        [Fact]
        public void Equality_ManifestTypeComparedWithNull_ShouldNotBeEqual()
        {
            // Arrange
            var manifestType = new ManifestType(ManifestTypeEnum.Load);

            // Act & Assert
            manifestType.Equals(null).Should().BeFalse();
        }

        [Fact]
        public void Equality_StaticFactoriesWithSameValue_ShouldBeEqual()
        {
            // Arrange
            var load1 = ManifestType.Load;
            var load2 = new ManifestType(ManifestTypeEnum.Load);

            // Act & Assert
            load1.Equals(load2).Should().BeTrue();
        }

        #endregion

        #region ManifestTypeEnum Tests

        [Fact]
        public void ManifestTypeEnum_ShouldHaveExpectedValues()
        {
            // Assert
            ((int)ManifestTypeEnum.Load).Should().Be(1);
            ((int)ManifestTypeEnum.Unload).Should().Be(2);
        }

        [Fact]
        public void ManifestTypeEnum_ShouldHaveOnlyTwoValues()
        {
            // Act
            var values = Enum.GetValues(typeof(ManifestTypeEnum));

            // Assert
            values.Length.Should().Be(2);
        }

        [Fact]
        public void ManifestTypeEnum_ShouldBeDefinedForValidValues()
        {
            // Assert
            Enum.IsDefined(typeof(ManifestTypeEnum), ManifestTypeEnum.Load).Should().BeTrue();
            Enum.IsDefined(typeof(ManifestTypeEnum), ManifestTypeEnum.Unload).Should().BeTrue();
        }

        #endregion

        #region Edge Cases

        [Fact]
        public void ManifestType_ShouldBeImmutable()
        {
            // Arrange
            var manifestType = new ManifestType(ManifestTypeEnum.Load);
            var originalValue = manifestType.Value;

            // Act - Value property should not have public setter

            // Assert
            manifestType.Value.Should().Be(originalValue);
        }

        [Fact]
        public void GetHashCode_SameManifestType_ShouldReturnConsistentHashCode()
        {
            // Arrange
            var manifestType = new ManifestType(ManifestTypeEnum.Unload);

            // Act
            var hashCode1 = manifestType.GetHashCode();
            var hashCode2 = manifestType.GetHashCode();

            // Assert
            hashCode1.Should().Be(hashCode2);
        }

        [Fact]
        public void Constructor_WithZeroValue_ShouldThrowBusinessRuleValidationException()
        {
            // Arrange
            var zeroType = (ManifestTypeEnum)0;

            // Act
            Action act = () => new ManifestType(zeroType);

            // Assert
            act.Should().Throw<BusinessRuleValidationException>();
        }

        #endregion

        #region Real-World Scenarios

        [Fact]
        public void ManifestType_LoadOperation_ShouldRepresentContainersBeingLoaded()
        {
            // Arrange - Vessel is loading containers from port
            var loadManifest = ManifestType.Load;

            // Assert
            loadManifest.Value.Should().Be(ManifestTypeEnum.Load);
            loadManifest.ToString().Should().Be("Load");
        }

        [Fact]
        public void ManifestType_UnloadOperation_ShouldRepresentContainersBeingUnloaded()
        {
            // Arrange - Vessel is unloading containers to port
            var unloadManifest = ManifestType.Unload;

            // Assert
            unloadManifest.Value.Should().Be(ManifestTypeEnum.Unload);
            unloadManifest.ToString().Should().Be("Unload");
        }

        [Fact]
        public void ManifestType_CanBeUsedInCollections()
        {
            // Arrange
            var manifestTypes = new List<ManifestType>
            {
                ManifestType.Load,
                ManifestType.Unload
            };

            // Assert
            manifestTypes.Should().HaveCount(2);
            manifestTypes[0].Value.Should().Be(ManifestTypeEnum.Load);
            manifestTypes[1].Value.Should().Be(ManifestTypeEnum.Unload);
        }

        [Fact]
        public void ManifestType_CanBeUsedInDictionary()
        {
            // Arrange
            var manifestDescriptions = new Dictionary<ManifestType, string>
            {
                { ManifestType.Load, "Containers loaded onto vessel" },
                { ManifestType.Unload, "Containers unloaded from vessel" }
            };

            // Act & Assert
            manifestDescriptions.Should().HaveCount(2);
            manifestDescriptions[ManifestType.Load].Should().Contain("loaded onto");
            manifestDescriptions[ManifestType.Unload].Should().Contain("unloaded from");
        }

        [Fact]
        public void ManifestType_LoadAndUnload_ShouldBeDifferent()
        {
            // Arrange
            var load = ManifestType.Load;
            var unload = ManifestType.Unload;

            // Act & Assert
            load.Equals(unload).Should().BeFalse();
            load.Value.Should().NotBe(unload.Value);
        }

        [Fact]
        public void ManifestType_ForVVNWithBothOperations_ShouldDistinguishTypes()
        {
            // Arrange - VVN can have both loading and unloading manifests
            var loadingManifest = ManifestType.Load;
            var unloadingManifest = ManifestType.Unload;

            // Assert - They should be clearly different
            loadingManifest.Should().NotBe(unloadingManifest);
            ((int)loadingManifest.Value).Should().BeLessThan((int)unloadingManifest.Value);
        }

        [Fact]
        public void StaticFactories_ShouldBeConvenientForCommonUsage()
        {
            // Act - Using static factories is more convenient than constructor
            var load = ManifestType.Load;
            var unload = ManifestType.Unload;

            // Assert
            load.Value.Should().Be(ManifestTypeEnum.Load);
            unload.Value.Should().Be(ManifestTypeEnum.Unload);
        }

        #endregion
    }
}
