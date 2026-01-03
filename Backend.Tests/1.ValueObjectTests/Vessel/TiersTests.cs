using FluentAssertions;
using ProjArqsi.Domain.VesselAggregate;
using Xunit;

namespace ProjArqsi.Tests.ValueObjectTests.Vessel
{
    public class TiersTests
    {
        #region Valid Tiers Tests

        [Fact]
        public void Constructor_WithValidTiers_ShouldCreateTiers()
        {
            // Arrange & Act
            var tiers = new Tiers(8);

            // Assert
            tiers.Value.Should().Be(8);
        }

        [Theory]
        [InlineData(1)]
        [InlineData(5)]
        [InlineData(8)] // Common for large container ships
        [InlineData(11)] // Maximum for ultra-large vessels
        [InlineData(15)]
        public void Constructor_WithMultipleValidTiers_ShouldCreateTiers(int validTiers)
        {
            // Arrange & Act
            var tiers = new Tiers(validTiers);

            // Assert
            tiers.Value.Should().Be(validTiers);
        }

        [Fact]
        public void Constructor_WithMinimumTiers_ShouldCreateTiers()
        {
            // Arrange & Act
            var tiers = new Tiers(1);

            // Assert
            tiers.Value.Should().Be(1);
        }

        [Fact]
        public void Constructor_WithLargeTiers_ShouldCreateTiers()
        {
            // Arrange & Act
            var tiers = new Tiers(20);

            // Assert
            tiers.Value.Should().Be(20);
        }

        #endregion

        #region Invalid Tiers Tests

        [Fact]
        public void Constructor_WithZero_ShouldThrowArgumentException()
        {
            // Arrange & Act
            Action act = () => new Tiers(0);

            // Assert
            act.Should().Throw<ArgumentException>()
                .WithMessage("Tiers must be a positive number*")
                .And.ParamName.Should().Be("value");
        }

        [Fact]
        public void Constructor_WithNegativeValue_ShouldThrowArgumentException()
        {
            // Arrange & Act
            Action act = () => new Tiers(-8);

            // Assert
            act.Should().Throw<ArgumentException>()
                .WithMessage("Tiers must be a positive number*")
                .And.ParamName.Should().Be("value");
        }

        [Theory]
        [InlineData(-1)]
        [InlineData(-5)]
        [InlineData(-100)]
        public void Constructor_WithMultipleNegativeValues_ShouldThrowArgumentException(int invalidTiers)
        {
            // Arrange & Act
            Action act = () => new Tiers(invalidTiers);

            // Assert
            act.Should().Throw<ArgumentException>()
                .WithMessage("Tiers must be a positive number*");
        }

        #endregion

        #region Equality Tests

        [Fact]
        public void Equals_WithSameTiers_ShouldReturnTrue()
        {
            // Arrange
            var tiers1 = new Tiers(8);
            var tiers2 = new Tiers(8);

            // Act & Assert
            tiers1.Equals(tiers2).Should().BeTrue();
        }

        [Fact]
        public void Equals_WithDifferentTiers_ShouldReturnFalse()
        {
            // Arrange
            var tiers1 = new Tiers(8);
            var tiers2 = new Tiers(11);

            // Act & Assert
            tiers1.Equals(tiers2).Should().BeFalse();
        }

        [Fact]
        public void GetHashCode_WithSameTiers_ShouldReturnSameHashCode()
        {
            // Arrange
            var tiers1 = new Tiers(8);
            var tiers2 = new Tiers(8);

            // Act & Assert
            tiers1.GetHashCode().Should().Be(tiers2.GetHashCode());
        }

        [Fact]
        public void GetHashCode_WithDifferentTiers_ShouldReturnDifferentHashCodes()
        {
            // Arrange
            var tiers1 = new Tiers(8);
            var tiers2 = new Tiers(11);

            // Act & Assert
            tiers1.GetHashCode().Should().NotBe(tiers2.GetHashCode());
        }

        #endregion

        #region Edge Cases

        [Fact]
        public void Constructor_WithMaxIntValue_ShouldCreateTiers()
        {
            // Arrange & Act
            var tiers = new Tiers(int.MaxValue);

            // Assert
            tiers.Value.Should().Be(int.MaxValue);
        }

        [Fact]
        public void Value_Property_ShouldBeReadOnly()
        {
            // Arrange
            var tiers = new Tiers(8);

            // Act - The Value property should not have a public setter
            var propertyInfo = typeof(Tiers).GetProperty(nameof(Tiers.Value));

            // Assert
            propertyInfo.Should().NotBeNull();
            propertyInfo.SetMethod.Should().NotBeNull();
            propertyInfo.SetMethod.IsPublic.Should().BeFalse();
        }

        [Fact]
        public void Constructor_WithOne_ShouldCreateMinimumTiers()
        {
            // Arrange & Act
            var tiers = new Tiers(1);

            // Assert
            tiers.Value.Should().Be(1);
        }

        #endregion

        #region Real-World Vessel Tier Configurations

        [Theory]
        [InlineData(11)] // Ultra-large container vessels (MSC Gülsün, HMM Algeciras) - maximum height
        [InlineData(10)] // Large container vessels
        [InlineData(9)] // Ever Given - 9 tiers on deck
        [InlineData(8)] // Typical large Post-Panamax vessels
        [InlineData(7)] // Mid-size Post-Panamax vessels
        [InlineData(6)] // Panamax vessels (height limited by bridge clearances)
        [InlineData(5)] // Smaller Panamax
        [InlineData(4)] // Feeder vessels
        [InlineData(3)] // Small feeder vessels
        public void Constructor_WithRealWorldTierConfigurations_ShouldCreateValidTiers(int realTiers)
        {
            // Arrange & Act
            var tiers = new Tiers(realTiers);

            // Assert
            tiers.Value.Should().Be(realTiers);
        }

        [Fact]
        public void Constructor_WithEverGivenTiers_ShouldRepresentActualVessel()
        {
            // Arrange - Ever Given can stack containers 9 tiers high on deck (plus below deck)
            var everGivenTiers = 9;

            // Act
            var tiers = new Tiers(everGivenTiers);

            // Assert
            tiers.Value.Should().Be(9);
        }

        [Fact]
        public void Constructor_WithUltraLargeVesselTiers_ShouldRepresentULCV()
        {
            // Arrange - Ultra Large Container Vessels can stack up to 11 tiers high on deck
            var ulcvTiers = 11;

            // Act
            var tiers = new Tiers(ulcvTiers);

            // Assert
            tiers.Value.Should().Be(11);
            tiers.Value.Should().BeGreaterThan(8); // ULCV typically 9+ tiers
        }

        [Fact]
        public void Constructor_WithPanamaxTiers_ShouldRepresentPanamaxVessel()
        {
            // Arrange - Panamax vessels typically limited to 5-6 tiers on deck
            // Due to the Bridge of the Americas height clearance (57.91m at high tide)
            var panamaxTiers = 6;

            // Act
            var tiers = new Tiers(panamaxTiers);

            // Assert
            tiers.Value.Should().Be(6);
            tiers.Value.Should().BeLessThanOrEqualTo(7); // Height constraint
        }

        [Fact]
        public void Constructor_WithNewPanamaxTiers_ShouldRepresentNeopanamaxVessel()
        {
            // Arrange - Neopanamax vessels can have 7-8 tiers on deck
            var neopanamaxTiers = 8;

            // Act
            var tiers = new Tiers(neopanamaxTiers);

            // Assert
            tiers.Value.Should().Be(8);
            tiers.Value.Should().BeGreaterThan(6); // More than Panamax
            tiers.Value.Should().BeLessThanOrEqualTo(9); // Typical Neopanamax limit
        }

        [Fact]
        public void Constructor_WithFeederVesselTiers_ShouldRepresentSmallVessel()
        {
            // Arrange - Feeder vessels typically have 3-5 tiers
            var feederTiers = 4;

            // Act
            var tiers = new Tiers(feederTiers);

            // Assert
            tiers.Value.Should().Be(4);
            tiers.Value.Should().BeLessThan(6);
        }

        [Fact]
        public void Constructor_WithMSCGulsunTiers_ShouldRepresentLargestVessel()
        {
            // Arrange - MSC Gülsün can stack up to 11 tiers on deck
            var mscGulsunTiers = 11;

            // Act
            var tiers = new Tiers(mscGulsunTiers);

            // Assert
            tiers.Value.Should().Be(11);
            tiers.Value.Should().BeGreaterThan(9); // Exceeds most vessels
        }

        #endregion

        #region Container Stacking Context

        [Fact]
        public void Constructor_WithTiers_ShouldRepresentVerticalStacking()
        {
            // Arrange - Tiers represent vertical stacking height (containers stacked on top of each other)
            // A vessel with 8 tiers means containers can be stacked 8 high on deck
            var tiers = new Tiers(8);

            // Act & Assert
            tiers.Value.Should().Be(8);
            tiers.Value.Should().BeGreaterThan(0);
        }

        [Fact]
        public void Constructor_WithOnDeckTiers_ShouldRepresentAboveDeckStacking()
        {
            // Arrange - Tiers typically refer to on-deck stacking
            // Modern container ships also have below-deck cargo holds (not counted in tiers)
            var onDeckTiers = 9;

            // Act
            var tiers = new Tiers(onDeckTiers);

            // Assert
            tiers.Value.Should().Be(9);
        }

        [Fact]
        public void Constructor_WithMaxTiers_ShouldConsiderStabilityLimits()
        {
            // Arrange - Maximum tier height is limited by vessel stability and center of gravity
            // Ultra-large vessels can safely stack 10-11 tiers due to their beam width
            var maxTiers = 11;

            // Act
            var tiers = new Tiers(maxTiers);

            // Assert
            tiers.Value.Should().Be(11);
            tiers.Value.Should().BeLessThanOrEqualTo(11); // Practical maximum for stability
        }

        [Fact]
        public void Constructor_WithLowTiers_ShouldRepresentSaferConfiguration()
        {
            // Arrange - Lower tier counts provide better stability and easier access
            var lowTiers = 5;

            // Act
            var tiers = new Tiers(lowTiers);

            // Assert
            tiers.Value.Should().Be(5);
            tiers.Value.Should().BeLessThan(7);
        }

        #endregion

        #region Height and Clearance Calculations

        [Fact]
        public void Constructor_WithTiers_ShouldConsiderContainerHeight()
        {
            // Arrange - Standard container height is ~2.59m (8.5 feet)
            // 8 tiers would be approximately 20.72m high (plus lashing equipment)
            var tiers = new Tiers(8);
            var approximateHeight = tiers.Value * 2.59; // meters

            // Act & Assert
            tiers.Value.Should().Be(8);
            approximateHeight.Should().BeApproximately(20.72, 0.5);
        }

        [Fact]
        public void Constructor_WithBridgeClearanceConstraint_ShouldConsiderPanamaCanal()
        {
            // Arrange - Panama Canal Bridge of the Americas clearance: 57.91m at high tide
            // Vessel draft (~15m) + hull (~10m) + below deck (~10m) + on-deck tiers
            // Typically limits to 6-7 tiers for Panamax vessels
            var panamaxTiers = 6;

            // Act
            var tiers = new Tiers(panamaxTiers);

            // Assert
            tiers.Value.Should().Be(6);
        }

        #endregion
    }
}
