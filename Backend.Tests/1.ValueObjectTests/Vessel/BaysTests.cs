using FluentAssertions;
using ProjArqsi.Domain.VesselAggregate;
using Xunit;

namespace ProjArqsi.Tests.ValueObjectTests.Vessel
{
    public class BaysTests
    {
        #region Valid Bays Tests

        [Fact]
        public void Constructor_WithValidBays_ShouldCreateBays()
        {
            // Arrange & Act
            var bays = new Bays(20);

            // Assert
            bays.Value.Should().Be(20);
        }

        [Theory]
        [InlineData(1)]
        [InlineData(10)]
        [InlineData(20)]
        [InlineData(24)] // Typical for large container ships
        [InlineData(30)]
        public void Constructor_WithMultipleValidBays_ShouldCreateBays(int validBays)
        {
            // Arrange & Act
            var bays = new Bays(validBays);

            // Assert
            bays.Value.Should().Be(validBays);
        }

        [Fact]
        public void Constructor_WithMinimumBays_ShouldCreateBays()
        {
            // Arrange & Act
            var bays = new Bays(1);

            // Assert
            bays.Value.Should().Be(1);
        }

        [Fact]
        public void Constructor_WithLargeBays_ShouldCreateBays()
        {
            // Arrange & Act
            var bays = new Bays(50);

            // Assert
            bays.Value.Should().Be(50);
        }

        #endregion

        #region Invalid Bays Tests

        [Fact]
        public void Constructor_WithZero_ShouldThrowArgumentException()
        {
            // Arrange & Act
            Action act = () => new Bays(0);

            // Assert
            act.Should().Throw<ArgumentException>()
                .WithMessage("Bays must be a positive number*")
                .And.ParamName.Should().Be("value");
        }

        [Fact]
        public void Constructor_WithNegativeValue_ShouldThrowArgumentException()
        {
            // Arrange & Act
            Action act = () => new Bays(-10);

            // Assert
            act.Should().Throw<ArgumentException>()
                .WithMessage("Bays must be a positive number*")
                .And.ParamName.Should().Be("value");
        }

        [Theory]
        [InlineData(-1)]
        [InlineData(-5)]
        [InlineData(-100)]
        public void Constructor_WithMultipleNegativeValues_ShouldThrowArgumentException(int invalidBays)
        {
            // Arrange & Act
            Action act = () => new Bays(invalidBays);

            // Assert
            act.Should().Throw<ArgumentException>()
                .WithMessage("Bays must be a positive number*");
        }

        #endregion

        #region Equality Tests

        [Fact]
        public void Equals_WithSameBays_ShouldReturnTrue()
        {
            // Arrange
            var bays1 = new Bays(20);
            var bays2 = new Bays(20);

            // Act & Assert
            bays1.Equals(bays2).Should().BeTrue();
        }

        [Fact]
        public void Equals_WithDifferentBays_ShouldReturnFalse()
        {
            // Arrange
            var bays1 = new Bays(20);
            var bays2 = new Bays(24);

            // Act & Assert
            bays1.Equals(bays2).Should().BeFalse();
        }

        [Fact]
        public void GetHashCode_WithSameBays_ShouldReturnSameHashCode()
        {
            // Arrange
            var bays1 = new Bays(20);
            var bays2 = new Bays(20);

            // Act & Assert
            bays1.GetHashCode().Should().Be(bays2.GetHashCode());
        }

        [Fact]
        public void GetHashCode_WithDifferentBays_ShouldReturnDifferentHashCodes()
        {
            // Arrange
            var bays1 = new Bays(20);
            var bays2 = new Bays(24);

            // Act & Assert
            bays1.GetHashCode().Should().NotBe(bays2.GetHashCode());
        }

        #endregion

        #region Edge Cases

        [Fact]
        public void Constructor_WithMaxIntValue_ShouldCreateBays()
        {
            // Arrange & Act
            var bays = new Bays(int.MaxValue);

            // Assert
            bays.Value.Should().Be(int.MaxValue);
        }

        [Fact]
        public void Value_Property_ShouldBeReadOnly()
        {
            // Arrange
            var bays = new Bays(20);

            // Act - The Value property should not have a public setter
            var propertyInfo = typeof(Bays).GetProperty(nameof(Bays.Value));

            // Assert
            propertyInfo.Should().NotBeNull();
            propertyInfo.SetMethod.Should().NotBeNull();
            propertyInfo.SetMethod.IsPublic.Should().BeFalse();
        }

        [Fact]
        public void Constructor_WithOne_ShouldCreateMinimumBays()
        {
            // Arrange & Act
            var bays = new Bays(1);

            // Assert
            bays.Value.Should().Be(1);
        }

        #endregion

        #region Real-World Vessel Bay Configurations

        [Theory]
        [InlineData(24)] // Large modern container ships (MSC Gülsün, HMM Algeciras)
        [InlineData(23)] // Ever Given has 23 bays
        [InlineData(20)] // Mid-size container ships
        [InlineData(18)] // Smaller Post-Panamax vessels
        [InlineData(15)] // Panamax vessels
        [InlineData(12)] // Feeder vessels
        [InlineData(10)] // Small feeder vessels
        [InlineData(8)] // Coastal feeders
        public void Constructor_WithRealWorldBayConfigurations_ShouldCreateValidBays(int realBays)
        {
            // Arrange & Act
            var bays = new Bays(realBays);

            // Assert
            bays.Value.Should().Be(realBays);
        }

        [Fact]
        public void Constructor_WithEverGivenBays_ShouldRepresentActualVessel()
        {
            // Arrange - Ever Given has 23 bays
            var everGivenBays = 23;

            // Act
            var bays = new Bays(everGivenBays);

            // Assert
            bays.Value.Should().Be(23);
        }

        [Fact]
        public void Constructor_WithUltraLargeVesselBays_ShouldRepresentULCV()
        {
            // Arrange - Ultra Large Container Vessels typically have 24 bays
            var ulcvBays = 24;

            // Act
            var bays = new Bays(ulcvBays);

            // Assert
            bays.Value.Should().Be(24);
            bays.Value.Should().BeGreaterThan(20); // ULCV typically 20+ bays
        }

        [Fact]
        public void Constructor_WithPanamaxBays_ShouldRepresentPanamaxVessel()
        {
            // Arrange - Panamax vessels typically have 13-15 bays
            var panamaxBays = 15;

            // Act
            var bays = new Bays(panamaxBays);

            // Assert
            bays.Value.Should().Be(15);
            bays.Value.Should().BeLessThan(18); // Typical Panamax limit
        }

        [Fact]
        public void Constructor_WithFeederVesselBays_ShouldRepresentSmallVessel()
        {
            // Arrange - Feeder vessels typically have 8-12 bays
            var feederBays = 10;

            // Act
            var bays = new Bays(feederBays);

            // Assert
            bays.Value.Should().Be(10);
            bays.Value.Should().BeLessThan(13);
        }

        [Fact]
        public void Constructor_WithMSCGulsunBays_ShouldRepresentLargestVessel()
        {
            // Arrange - MSC Gülsün typically has 24 bays
            var mscGulsunBays = 24;

            // Act
            var bays = new Bays(mscGulsunBays);

            // Assert
            bays.Value.Should().Be(24);
        }

        #endregion

        #region Container Stacking Context

        [Fact]
        public void Constructor_WithBays_ShouldRepresentLengthwiseSections()
        {
            // Arrange - Bays represent lengthwise sections of the ship (front to back)
            // A vessel with 20 bays means containers can be stacked in 20 longitudinal sections
            var bays = new Bays(20);

            // Act & Assert
            bays.Value.Should().Be(20);
            bays.Value.Should().BeGreaterThan(0);
        }

        [Fact]
        public void Constructor_WithOddNumberBays_ShouldBeValid()
        {
            // Arrange - Container ships typically have odd-numbered bay positions (01, 03, 05...)
            // for 40-foot containers and even numbers for 20-foot containers
            var oddBays = 23;

            // Act
            var bays = new Bays(oddBays);

            // Assert
            bays.Value.Should().Be(23);
        }

        #endregion
    }
}
