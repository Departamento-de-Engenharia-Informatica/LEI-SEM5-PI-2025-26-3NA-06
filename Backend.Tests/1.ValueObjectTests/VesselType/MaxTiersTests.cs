using FluentAssertions;
using ProjArqsi.Domain.VesselTypeAggregate;
using Xunit;

namespace ProjArqsi.Tests.ValueObjectTests.VesselType
{
    public class MaxTiersTests
    {
        #region Valid MaxTiers Tests

        [Fact]
        public void Constructor_WithValidMaxTiers_ShouldCreateMaxTiers()
        {
            // Arrange & Act
            var maxTiers = new MaxTiers(8);

            // Assert
            maxTiers.Value.Should().Be(8);
        }

        [Theory]
        [InlineData(1)]
        [InlineData(5)]
        [InlineData(6)] // Panamax typical
        [InlineData(8)] // Post-Panamax
        [InlineData(11)] // ULCV maximum
        public void Constructor_WithMultipleValidMaxTiers_ShouldCreateMaxTiers(int validMaxTiers)
        {
            // Arrange & Act
            var maxTiers = new MaxTiers(validMaxTiers);

            // Assert
            maxTiers.Value.Should().Be(validMaxTiers);
        }

        [Fact]
        public void Constructor_WithMinimumMaxTiers_ShouldCreateMaxTiers()
        {
            // Arrange & Act
            var maxTiers = new MaxTiers(1);

            // Assert
            maxTiers.Value.Should().Be(1);
        }

        [Fact]
        public void Constructor_WithLargeMaxTiers_ShouldCreateMaxTiers()
        {
            // Arrange & Act
            var maxTiers = new MaxTiers(15);

            // Assert
            maxTiers.Value.Should().Be(15);
        }

        #endregion

        #region Invalid MaxTiers Tests

        [Fact]
        public void Constructor_WithZero_ShouldThrowArgumentException()
        {
            // Arrange & Act
            Action act = () => new MaxTiers(0);

            // Assert
            act.Should().Throw<ArgumentException>()
                .WithMessage("Max tiers must be greater than zero*")
                .And.ParamName.Should().Be("value");
        }

        [Fact]
        public void Constructor_WithNegativeValue_ShouldThrowArgumentException()
        {
            // Arrange & Act
            Action act = () => new MaxTiers(-8);

            // Assert
            act.Should().Throw<ArgumentException>()
                .WithMessage("Max tiers must be greater than zero*")
                .And.ParamName.Should().Be("value");
        }

        [Theory]
        [InlineData(-1)]
        [InlineData(-5)]
        [InlineData(-100)]
        public void Constructor_WithMultipleNegativeValues_ShouldThrowArgumentException(int invalidMaxTiers)
        {
            // Arrange & Act
            Action act = () => new MaxTiers(invalidMaxTiers);

            // Assert
            act.Should().Throw<ArgumentException>()
                .WithMessage("Max tiers must be greater than zero*");
        }

        #endregion

        #region ToString Tests

        [Fact]
        public void ToString_ShouldReturnMaxTiersAsString()
        {
            // Arrange
            var maxTiers = new MaxTiers(8);

            // Act
            var result = maxTiers.ToString();

            // Assert
            result.Should().Be("8");
        }

        [Theory]
        [InlineData(1, "1")]
        [InlineData(6, "6")]
        [InlineData(11, "11")]
        public void ToString_WithMultipleMaxTiers_ShouldReturnCorrectFormat(int value, string expected)
        {
            // Arrange
            var maxTiers = new MaxTiers(value);

            // Act
            var result = maxTiers.ToString();

            // Assert
            result.Should().Be(expected);
        }

        #endregion

        #region Equality Tests

        [Fact]
        public void Equals_WithSameMaxTiers_ShouldReturnTrue()
        {
            // Arrange
            var maxTiers1 = new MaxTiers(8);
            var maxTiers2 = new MaxTiers(8);

            // Act & Assert
            maxTiers1.Equals(maxTiers2).Should().BeTrue();
        }

        [Fact]
        public void Equals_WithDifferentMaxTiers_ShouldReturnFalse()
        {
            // Arrange
            var maxTiers1 = new MaxTiers(8);
            var maxTiers2 = new MaxTiers(11);

            // Act & Assert
            maxTiers1.Equals(maxTiers2).Should().BeFalse();
        }

        [Fact]
        public void GetHashCode_WithSameMaxTiers_ShouldReturnSameHashCode()
        {
            // Arrange
            var maxTiers1 = new MaxTiers(8);
            var maxTiers2 = new MaxTiers(8);

            // Act & Assert
            maxTiers1.GetHashCode().Should().Be(maxTiers2.GetHashCode());
        }

        [Fact]
        public void GetHashCode_WithDifferentMaxTiers_ShouldReturnDifferentHashCodes()
        {
            // Arrange
            var maxTiers1 = new MaxTiers(8);
            var maxTiers2 = new MaxTiers(11);

            // Act & Assert
            maxTiers1.GetHashCode().Should().NotBe(maxTiers2.GetHashCode());
        }

        #endregion

        #region Edge Cases

        [Fact]
        public void Constructor_WithMaxIntValue_ShouldCreateMaxTiers()
        {
            // Arrange & Act
            var maxTiers = new MaxTiers(int.MaxValue);

            // Assert
            maxTiers.Value.Should().Be(int.MaxValue);
        }

        [Fact]
        public void Value_Property_ShouldBeReadOnly()
        {
            // Arrange
            var maxTiers = new MaxTiers(8);

            // Act - The Value property should not have a public setter
            var propertyInfo = typeof(MaxTiers).GetProperty(nameof(MaxTiers.Value));

            // Assert
            propertyInfo.Should().NotBeNull();
            propertyInfo.SetMethod.Should().NotBeNull();
            propertyInfo.SetMethod.IsPublic.Should().BeFalse();
        }

        [Fact]
        public void Constructor_WithOne_ShouldCreateMinimumMaxTiers()
        {
            // Arrange & Act
            var maxTiers = new MaxTiers(1);

            // Assert
            maxTiers.Value.Should().Be(1);
            maxTiers.ToString().Should().Be("1");
        }

        #endregion

        #region Real-World Vessel Type Tier Configurations

        [Theory]
        [InlineData(3)] // Small feeder vessels
        [InlineData(4)] // Feeder vessels
        [InlineData(5)] // Smaller Panamax
        [InlineData(6)] // Panamax vessels (Bridge of Americas clearance)
        [InlineData(7)] // Mid-size Post-Panamax
        [InlineData(8)] // Neopanamax typical
        [InlineData(9)] // Ever Given class
        [InlineData(10)] // Large container vessels
        [InlineData(11)] // ULCV maximum (MSC Gülsün)
        public void Constructor_WithRealWorldMaxTiers_ShouldCreateValidMaxTiers(int realMaxTiers)
        {
            // Arrange & Act
            var maxTiers = new MaxTiers(realMaxTiers);

            // Assert
            maxTiers.Value.Should().Be(realMaxTiers);
            maxTiers.ToString().Should().Be(realMaxTiers.ToString());
        }

        [Fact]
        public void Constructor_WithFeederTypeMaxTiers_ShouldRepresentSmallVesselType()
        {
            // Arrange - Feeder vessel types typically have 3-5 tiers
            var feederMaxTiers = 4;

            // Act
            var maxTiers = new MaxTiers(feederMaxTiers);

            // Assert
            maxTiers.Value.Should().Be(4);
            maxTiers.Value.Should().BeLessThan(6);
        }

        [Fact]
        public void Constructor_WithPanamaxTypeMaxTiers_ShouldRepresentHeightConstraint()
        {
            // Arrange - Panamax vessel types limited to 5-6 tiers (Bridge of Americas 57.91m clearance)
            var panamaxMaxTiers = 6;

            // Act
            var maxTiers = new MaxTiers(panamaxMaxTiers);

            // Assert
            maxTiers.Value.Should().Be(6);
            maxTiers.Value.Should().BeLessThanOrEqualTo(7); // Height constraint
        }

        [Fact]
        public void Constructor_WithNeopanamaxTypeMaxTiers_ShouldRepresentExpandedCanal()
        {
            // Arrange - Neopanamax types can have 7-8 tiers
            var neopanamaxMaxTiers = 8;

            // Act
            var maxTiers = new MaxTiers(neopanamaxMaxTiers);

            // Assert
            maxTiers.Value.Should().Be(8);
            maxTiers.Value.Should().BeGreaterThan(6); // Exceeds Panamax
            maxTiers.Value.Should().BeLessThanOrEqualTo(9); // Typical limit
        }

        [Fact]
        public void Constructor_WithULCVTypeMaxTiers_ShouldRepresentLargestClass()
        {
            // Arrange - ULCV vessel types can stack up to 11 tiers on deck
            var ulcvMaxTiers = 11;

            // Act
            var maxTiers = new MaxTiers(ulcvMaxTiers);

            // Assert
            maxTiers.Value.Should().Be(11);
            maxTiers.Value.Should().BeGreaterThan(9);
        }

        [Fact]
        public void Constructor_WithEverGivenTypeMaxTiers_ShouldRepresentLargePostPanamax()
        {
            // Arrange - Ever Given type vessels can stack 9 tiers on deck
            var everGivenTypeTiers = 9;

            // Act
            var maxTiers = new MaxTiers(everGivenTypeTiers);

            // Assert
            maxTiers.Value.Should().Be(9);
        }

        #endregion

        #region Vessel Type Classification

        [Fact]
        public void Constructor_WithMaxTiers_ShouldDefineTypeHeightCapability()
        {
            // Arrange - MaxTiers defines the vertical stacking capacity for a vessel type
            var feederType = new MaxTiers(4);
            var panamaxType = new MaxTiers(6);
            var ulcvType = new MaxTiers(11);

            // Act & Assert
            feederType.Value.Should().BeLessThan(6); // Feeder range
            panamaxType.Value.Should().BeLessThanOrEqualTo(7); // Panamax constraint
            ulcvType.Value.Should().BeGreaterThan(9); // ULCV range
        }

        [Fact]
        public void Constructor_WithPanamaxMaxTiers_ShouldIndicateBridgeClearance()
        {
            // Arrange - Panamax types limited by Bridge of Americas clearance (57.91m)
            var panamaxMaxTiers = 6;

            // Act
            var maxTiers = new MaxTiers(panamaxMaxTiers);

            // Assert
            maxTiers.Value.Should().Be(6);
            maxTiers.Value.Should().BeLessThanOrEqualTo(7); // Bridge clearance constraint
        }

        [Fact]
        public void Constructor_WithStabilityConsideration_ShouldRespectCenterOfGravity()
        {
            // Arrange - Maximum tiers limited by vessel stability (center of gravity)
            // ULCV can safely stack 10-11 tiers due to their wide beam
            var maxSafeTiers = 11;

            // Act
            var maxTiers = new MaxTiers(maxSafeTiers);

            // Assert
            maxTiers.Value.Should().Be(11);
            maxTiers.Value.Should().BeLessThanOrEqualTo(11); // Practical stability limit
        }

        [Fact]
        public void Constructor_WithContainerHeightCalculation_ShouldConsiderDimensions()
        {
            // Arrange - Each tier adds ~2.59m (8.5 feet) container height
            // 8 tiers = approximately 20.72m of containers on deck
            var maxTiers = new MaxTiers(8);
            var approximateHeight = maxTiers.Value * 2.59; // meters

            // Act & Assert
            maxTiers.Value.Should().Be(8);
            approximateHeight.Should().BeApproximately(20.72, 0.5);
        }

        #endregion
    }
}
