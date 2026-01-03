using FluentAssertions;
using ProjArqsi.Domain.VesselTypeAggregate;
using Xunit;

namespace ProjArqsi.Tests.ValueObjectTests.VesselType
{
    public class MaxBaysTests
    {
        #region Valid MaxBays Tests

        [Fact]
        public void Constructor_WithValidMaxBays_ShouldCreateMaxBays()
        {
            // Arrange & Act
            var maxBays = new MaxBays(20);

            // Assert
            maxBays.Value.Should().Be(20);
        }

        [Theory]
        [InlineData(1)]
        [InlineData(10)]
        [InlineData(15)] // Panamax typical
        [InlineData(20)] // Post-Panamax
        [InlineData(24)] // ULCV typical
        public void Constructor_WithMultipleValidMaxBays_ShouldCreateMaxBays(int validMaxBays)
        {
            // Arrange & Act
            var maxBays = new MaxBays(validMaxBays);

            // Assert
            maxBays.Value.Should().Be(validMaxBays);
        }

        [Fact]
        public void Constructor_WithMinimumMaxBays_ShouldCreateMaxBays()
        {
            // Arrange & Act
            var maxBays = new MaxBays(1);

            // Assert
            maxBays.Value.Should().Be(1);
        }

        [Fact]
        public void Constructor_WithLargeMaxBays_ShouldCreateMaxBays()
        {
            // Arrange & Act
            var maxBays = new MaxBays(30);

            // Assert
            maxBays.Value.Should().Be(30);
        }

        #endregion

        #region Invalid MaxBays Tests

        [Fact]
        public void Constructor_WithZero_ShouldThrowArgumentException()
        {
            // Arrange & Act
            Action act = () => new MaxBays(0);

            // Assert
            act.Should().Throw<ArgumentException>()
                .WithMessage("Max bays must be greater than zero*")
                .And.ParamName.Should().Be("value");
        }

        [Fact]
        public void Constructor_WithNegativeValue_ShouldThrowArgumentException()
        {
            // Arrange & Act
            Action act = () => new MaxBays(-10);

            // Assert
            act.Should().Throw<ArgumentException>()
                .WithMessage("Max bays must be greater than zero*")
                .And.ParamName.Should().Be("value");
        }

        [Theory]
        [InlineData(-1)]
        [InlineData(-5)]
        [InlineData(-100)]
        public void Constructor_WithMultipleNegativeValues_ShouldThrowArgumentException(int invalidMaxBays)
        {
            // Arrange & Act
            Action act = () => new MaxBays(invalidMaxBays);

            // Assert
            act.Should().Throw<ArgumentException>()
                .WithMessage("Max bays must be greater than zero*");
        }

        #endregion

        #region ToString Tests

        [Fact]
        public void ToString_ShouldReturnMaxBaysAsString()
        {
            // Arrange
            var maxBays = new MaxBays(20);

            // Act
            var result = maxBays.ToString();

            // Assert
            result.Should().Be("20");
        }

        [Theory]
        [InlineData(1, "1")]
        [InlineData(15, "15")]
        [InlineData(24, "24")]
        public void ToString_WithMultipleMaxBays_ShouldReturnCorrectFormat(int value, string expected)
        {
            // Arrange
            var maxBays = new MaxBays(value);

            // Act
            var result = maxBays.ToString();

            // Assert
            result.Should().Be(expected);
        }

        #endregion

        #region Equality Tests

        [Fact]
        public void Equals_WithSameMaxBays_ShouldReturnTrue()
        {
            // Arrange
            var maxBays1 = new MaxBays(20);
            var maxBays2 = new MaxBays(20);

            // Act & Assert
            maxBays1.Equals(maxBays2).Should().BeTrue();
        }

        [Fact]
        public void Equals_WithDifferentMaxBays_ShouldReturnFalse()
        {
            // Arrange
            var maxBays1 = new MaxBays(20);
            var maxBays2 = new MaxBays(24);

            // Act & Assert
            maxBays1.Equals(maxBays2).Should().BeFalse();
        }

        [Fact]
        public void GetHashCode_WithSameMaxBays_ShouldReturnSameHashCode()
        {
            // Arrange
            var maxBays1 = new MaxBays(20);
            var maxBays2 = new MaxBays(20);

            // Act & Assert
            maxBays1.GetHashCode().Should().Be(maxBays2.GetHashCode());
        }

        [Fact]
        public void GetHashCode_WithDifferentMaxBays_ShouldReturnDifferentHashCodes()
        {
            // Arrange
            var maxBays1 = new MaxBays(20);
            var maxBays2 = new MaxBays(24);

            // Act & Assert
            maxBays1.GetHashCode().Should().NotBe(maxBays2.GetHashCode());
        }

        #endregion

        #region Edge Cases

        [Fact]
        public void Constructor_WithMaxIntValue_ShouldCreateMaxBays()
        {
            // Arrange & Act
            var maxBays = new MaxBays(int.MaxValue);

            // Assert
            maxBays.Value.Should().Be(int.MaxValue);
        }

        [Fact]
        public void Value_Property_ShouldBeReadOnly()
        {
            // Arrange
            var maxBays = new MaxBays(20);

            // Act - The Value property should not have a public setter
            var propertyInfo = typeof(MaxBays).GetProperty(nameof(MaxBays.Value));

            // Assert
            propertyInfo.Should().NotBeNull();
            propertyInfo.SetMethod.Should().NotBeNull();
            propertyInfo.SetMethod.IsPublic.Should().BeFalse();
        }

        [Fact]
        public void Constructor_WithOne_ShouldCreateMinimumMaxBays()
        {
            // Arrange & Act
            var maxBays = new MaxBays(1);

            // Assert
            maxBays.Value.Should().Be(1);
            maxBays.ToString().Should().Be("1");
        }

        #endregion

        #region Real-World Vessel Type Bay Configurations

        [Theory]
        [InlineData(8)] // Small feeder vessels
        [InlineData(10)] // Coastal feeders
        [InlineData(12)] // Feeder vessels
        [InlineData(15)] // Panamax vessels
        [InlineData(18)] // Smaller Post-Panamax
        [InlineData(20)] // Mid-size Post-Panamax
        [InlineData(23)] // Large Post-Panamax (Ever Given)
        [InlineData(24)] // ULCV typical (MSC Gülsün, HMM Algeciras)
        public void Constructor_WithRealWorldMaxBays_ShouldCreateValidMaxBays(int realMaxBays)
        {
            // Arrange & Act
            var maxBays = new MaxBays(realMaxBays);

            // Assert
            maxBays.Value.Should().Be(realMaxBays);
            maxBays.ToString().Should().Be(realMaxBays.ToString());
        }

        [Fact]
        public void Constructor_WithFeederTypeMaxBays_ShouldRepresentSmallVesselType()
        {
            // Arrange - Feeder vessel types typically have 8-12 bays
            var feederMaxBays = 10;

            // Act
            var maxBays = new MaxBays(feederMaxBays);

            // Assert
            maxBays.Value.Should().Be(10);
            maxBays.Value.Should().BeLessThan(13);
        }

        [Fact]
        public void Constructor_WithPanamaxTypeMaxBays_ShouldRepresentPanamaxClass()
        {
            // Arrange - Panamax vessel types typically have 13-15 bays
            var panamaxMaxBays = 15;

            // Act
            var maxBays = new MaxBays(panamaxMaxBays);

            // Assert
            maxBays.Value.Should().Be(15);
            maxBays.Value.Should().BeLessThan(18);
        }

        [Fact]
        public void Constructor_WithULCVTypeMaxBays_ShouldRepresentLargestClass()
        {
            // Arrange - ULCV vessel types typically have 24 bays
            var ulcvMaxBays = 24;

            // Act
            var maxBays = new MaxBays(ulcvMaxBays);

            // Assert
            maxBays.Value.Should().Be(24);
            maxBays.Value.Should().BeGreaterThan(20);
        }

        [Fact]
        public void Constructor_WithEverGivenTypeMaxBays_ShouldRepresentLargePostPanamax()
        {
            // Arrange - Ever Given type vessels have 23 bays
            var everGivenTypeBays = 23;

            // Act
            var maxBays = new MaxBays(everGivenTypeBays);

            // Assert
            maxBays.Value.Should().Be(23);
        }

        #endregion

        #region Vessel Type Classification

        [Fact]
        public void Constructor_WithMaxBays_ShouldDefineTypeCapability()
        {
            // Arrange - MaxBays defines the lengthwise container capacity for a vessel type
            var feederType = new MaxBays(10);
            var panamaxType = new MaxBays(15);
            var ulcvType = new MaxBays(24);

            // Act & Assert
            feederType.Value.Should().BeLessThan(13); // Feeder range
            panamaxType.Value.Should().BeInRange(13, 17); // Panamax range
            ulcvType.Value.Should().BeGreaterThan(20); // ULCV range
        }

        [Fact]
        public void Constructor_WithTypicalPanamaxMaxBays_ShouldIndicateDimensionalLimit()
        {
            // Arrange - Panamax types limited to ~15 bays due to 294m length constraint
            var panamaxMaxBays = 15;

            // Act
            var maxBays = new MaxBays(panamaxMaxBays);

            // Assert
            maxBays.Value.Should().Be(15);
            maxBays.Value.Should().BeLessThanOrEqualTo(17); // Practical Panamax limit
        }

        #endregion
    }
}
