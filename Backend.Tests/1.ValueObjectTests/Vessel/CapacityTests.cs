using FluentAssertions;
using ProjArqsi.Domain.VesselAggregate;
using Xunit;

namespace ProjArqsi.Tests.ValueObjectTests.Vessel
{
    public class CapacityTests
    {
        #region Valid Capacity Tests

        [Fact]
        public void Constructor_WithValidCapacity_ShouldCreateCapacity()
        {
            // Arrange & Act
            var capacity = new Capacity(10000);

            // Assert
            capacity.Value.Should().Be(10000);
        }

        [Theory]
        [InlineData(1)]
        [InlineData(100)]
        [InlineData(1000)]
        [InlineData(10000)]
        [InlineData(23756)] // MSC Gülsün - world's largest container ship capacity
        public void Constructor_WithMultipleValidCapacities_ShouldCreateCapacity(int validCapacity)
        {
            // Arrange & Act
            var capacity = new Capacity(validCapacity);

            // Assert
            capacity.Value.Should().Be(validCapacity);
        }

        [Fact]
        public void Constructor_WithMinimumCapacity_ShouldCreateCapacity()
        {
            // Arrange & Act
            var capacity = new Capacity(1);

            // Assert
            capacity.Value.Should().Be(1);
        }

        [Fact]
        public void Constructor_WithLargeCapacity_ShouldCreateCapacity()
        {
            // Arrange & Act
            var capacity = new Capacity(25000);

            // Assert
            capacity.Value.Should().Be(25000);
        }

        #endregion

        #region Invalid Capacity Tests

        [Fact]
        public void Constructor_WithZero_ShouldThrowArgumentException()
        {
            // Arrange & Act
            Action act = () => new Capacity(0);

            // Assert
            act.Should().Throw<ArgumentException>()
                .WithMessage("Capacity must be greater than zero*")
                .And.ParamName.Should().Be("value");
        }

        [Fact]
        public void Constructor_WithNegativeValue_ShouldThrowArgumentException()
        {
            // Arrange & Act
            Action act = () => new Capacity(-1000);

            // Assert
            act.Should().Throw<ArgumentException>()
                .WithMessage("Capacity must be greater than zero*")
                .And.ParamName.Should().Be("value");
        }

        [Theory]
        [InlineData(-1)]
        [InlineData(-100)]
        [InlineData(-10000)]
        public void Constructor_WithMultipleNegativeValues_ShouldThrowArgumentException(int invalidCapacity)
        {
            // Arrange & Act
            Action act = () => new Capacity(invalidCapacity);

            // Assert
            act.Should().Throw<ArgumentException>()
                .WithMessage("Capacity must be greater than zero*");
        }

        #endregion

        #region ToString Tests

        [Fact]
        public void ToString_ShouldReturnCapacityWithTEU()
        {
            // Arrange
            var capacity = new Capacity(10000);

            // Act
            var result = capacity.ToString();

            // Assert
            result.Should().Be("10000 TEU");
        }

        [Theory]
        [InlineData(1, "1 TEU")]
        [InlineData(1000, "1000 TEU")]
        [InlineData(23756, "23756 TEU")]
        public void ToString_WithMultipleCapacities_ShouldReturnCorrectFormat(int value, string expected)
        {
            // Arrange
            var capacity = new Capacity(value);

            // Act
            var result = capacity.ToString();

            // Assert
            result.Should().Be(expected);
        }

        [Fact]
        public void ToString_ShouldIncludeTEUUnit()
        {
            // Arrange
            var capacity = new Capacity(5000);

            // Act
            var result = capacity.ToString();

            // Assert
            result.Should().Contain("TEU");
        }

        #endregion

        #region Equality Tests

        [Fact]
        public void Equals_WithSameCapacities_ShouldReturnTrue()
        {
            // Arrange
            var capacity1 = new Capacity(10000);
            var capacity2 = new Capacity(10000);

            // Act & Assert
            capacity1.Equals(capacity2).Should().BeTrue();
        }

        [Fact]
        public void Equals_WithDifferentCapacities_ShouldReturnFalse()
        {
            // Arrange
            var capacity1 = new Capacity(10000);
            var capacity2 = new Capacity(20000);

            // Act & Assert
            capacity1.Equals(capacity2).Should().BeFalse();
        }

        [Fact]
        public void GetHashCode_WithSameCapacities_ShouldReturnSameHashCode()
        {
            // Arrange
            var capacity1 = new Capacity(10000);
            var capacity2 = new Capacity(10000);

            // Act & Assert
            capacity1.GetHashCode().Should().Be(capacity2.GetHashCode());
        }

        [Fact]
        public void GetHashCode_WithDifferentCapacities_ShouldReturnDifferentHashCodes()
        {
            // Arrange
            var capacity1 = new Capacity(10000);
            var capacity2 = new Capacity(20000);

            // Act & Assert
            capacity1.GetHashCode().Should().NotBe(capacity2.GetHashCode());
        }

        #endregion

        #region Edge Cases

        [Fact]
        public void Constructor_WithMaxIntValue_ShouldCreateCapacity()
        {
            // Arrange & Act
            var capacity = new Capacity(int.MaxValue);

            // Assert
            capacity.Value.Should().Be(int.MaxValue);
        }

        [Fact]
        public void Value_Property_ShouldBeReadOnly()
        {
            // Arrange
            var capacity = new Capacity(10000);

            // Act - The Value property should not have a public setter
            var propertyInfo = typeof(Capacity).GetProperty(nameof(Capacity.Value));

            // Assert
            propertyInfo.Should().NotBeNull();
            propertyInfo.SetMethod.Should().NotBeNull();
            propertyInfo.SetMethod.IsPublic.Should().BeFalse();
        }

        [Fact]
        public void Constructor_WithOne_ShouldCreateMinimumCapacity()
        {
            // Arrange & Act
            var capacity = new Capacity(1);

            // Assert
            capacity.Value.Should().Be(1);
            capacity.ToString().Should().Be("1 TEU");
        }

        #endregion

        #region Real-World Vessel Capacities

        [Theory]
        [InlineData(23756)] // MSC Gülsün - world's largest (23,756 TEU)
        [InlineData(23964)] // HMM Algeciras class (23,964 TEU)
        [InlineData(21413)] // OOCL Hong Kong (21,413 TEU)
        [InlineData(20124)] // Ever Given (20,124 TEU)
        [InlineData(14000)] // Large Panamax vessel
        [InlineData(8000)] // Mid-size Post-Panamax vessel
        [InlineData(5000)] // Panamax vessel
        [InlineData(3000)] // Feeder vessel
        [InlineData(1000)] // Small feeder vessel
        [InlineData(500)] // Coastal feeder
        public void Constructor_WithRealWorldVesselCapacities_ShouldCreateValidCapacity(int realCapacity)
        {
            // Arrange & Act
            var capacity = new Capacity(realCapacity);

            // Assert
            capacity.Value.Should().Be(realCapacity);
            capacity.ToString().Should().Contain("TEU");
        }

        [Fact]
        public void Constructor_WithEverGivenCapacity_ShouldRepresentActualVessel()
        {
            // Arrange - Ever Given: 20,124 TEU capacity, famous Suez Canal vessel
            var everGivenCapacity = 20124;

            // Act
            var capacity = new Capacity(everGivenCapacity);

            // Assert
            capacity.Value.Should().Be(20124);
            capacity.ToString().Should().Be("20124 TEU");
        }

        [Fact]
        public void Constructor_WithMSCGulsunCapacity_ShouldRepresentLargestVessel()
        {
            // Arrange - MSC Gülsün: 23,756 TEU, one of the world's largest container ships
            var mscGulsunCapacity = 23756;

            // Act
            var capacity = new Capacity(mscGulsunCapacity);

            // Assert
            capacity.Value.Should().Be(23756);
            capacity.Value.Should().BeGreaterThan(20000);
            capacity.ToString().Should().Be("23756 TEU");
        }

        [Fact]
        public void Constructor_WithPanamaxCapacity_ShouldRepresentPanamaxVessel()
        {
            // Arrange - Panamax vessels typically have 4,000-5,000 TEU capacity
            var panamaxCapacity = 5000;

            // Act
            var capacity = new Capacity(panamaxCapacity);

            // Assert
            capacity.Value.Should().Be(5000);
            capacity.Value.Should().BeLessThan(6000); // Typical Panamax limit
        }

        [Fact]
        public void Constructor_WithNewPanamaxCapacity_ShouldRepresentNeopanamaxVessel()
        {
            // Arrange - Neopanamax vessels can carry 10,000-14,000 TEU
            var neopanamaxCapacity = 14000;

            // Act
            var capacity = new Capacity(neopanamaxCapacity);

            // Assert
            capacity.Value.Should().Be(14000);
            capacity.Value.Should().BeGreaterThan(10000);
            capacity.Value.Should().BeLessThan(15000);
        }

        [Fact]
        public void Constructor_WithFeederVesselCapacity_ShouldRepresentSmallVessel()
        {
            // Arrange - Feeder vessels typically 500-3,000 TEU for port-to-port distribution
            var feederCapacity = 1500;

            // Act
            var capacity = new Capacity(feederCapacity);

            // Assert
            capacity.Value.Should().Be(1500);
            capacity.Value.Should().BeLessThan(3000);
        }

        [Fact]
        public void Constructor_WithUltraLargeContainerVessel_ShouldRepresentULCV()
        {
            // Arrange - ULCV (Ultra Large Container Vessel) are vessels over 14,500 TEU
            var ulcvCapacity = 20000;

            // Act
            var capacity = new Capacity(ulcvCapacity);

            // Assert
            capacity.Value.Should().Be(20000);
            capacity.Value.Should().BeGreaterThan(14500); // ULCV threshold
        }

        #endregion
    }
}
