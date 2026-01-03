using FluentAssertions;
using ProjArqsi.Domain.VesselTypeAggregate;
using Xunit;

namespace ProjArqsi.Tests.ValueObjectTests.VesselType
{
    public class TypeCapacityTests
    {
        #region Valid TypeCapacity Tests

        [Fact]
        public void Constructor_WithValidCapacity_ShouldCreateTypeCapacity()
        {
            // Arrange & Act
            var capacity = new TypeCapacity(5000);

            // Assert
            capacity.Value.Should().Be(5000);
        }

        [Theory]
        [InlineData(1)]
        [InlineData(500)]
        [InlineData(5000)] // Panamax typical capacity
        [InlineData(14500)] // ULCV threshold
        [InlineData(23756)] // MSC Gülsün capacity
        public void Constructor_WithMultipleValidCapacities_ShouldCreateTypeCapacity(int validCapacity)
        {
            // Arrange & Act
            var capacity = new TypeCapacity(validCapacity);

            // Assert
            capacity.Value.Should().Be(validCapacity);
        }

        [Fact]
        public void Constructor_WithMinimumCapacity_ShouldCreateTypeCapacity()
        {
            // Arrange & Act
            var capacity = new TypeCapacity(1);

            // Assert
            capacity.Value.Should().Be(1);
        }

        [Fact]
        public void Constructor_WithLargeCapacity_ShouldCreateTypeCapacity()
        {
            // Arrange & Act
            var capacity = new TypeCapacity(25000);

            // Assert
            capacity.Value.Should().Be(25000);
        }

        #endregion

        #region Invalid TypeCapacity Tests

        [Fact]
        public void Constructor_WithZero_ShouldThrowArgumentException()
        {
            // Arrange & Act
            Action act = () => new TypeCapacity(0);

            // Assert
            act.Should().Throw<ArgumentException>()
                .WithMessage("Type capacity must be greater than zero*")
                .And.ParamName.Should().Be("value");
        }

        [Fact]
        public void Constructor_WithNegativeValue_ShouldThrowArgumentException()
        {
            // Arrange & Act
            Action act = () => new TypeCapacity(-5000);

            // Assert
            act.Should().Throw<ArgumentException>()
                .WithMessage("Type capacity must be greater than zero*")
                .And.ParamName.Should().Be("value");
        }

        [Theory]
        [InlineData(-1)]
        [InlineData(-100)]
        [InlineData(-10000)]
        public void Constructor_WithMultipleNegativeValues_ShouldThrowArgumentException(int invalidCapacity)
        {
            // Arrange & Act
            Action act = () => new TypeCapacity(invalidCapacity);

            // Assert
            act.Should().Throw<ArgumentException>()
                .WithMessage("Type capacity must be greater than zero*");
        }

        #endregion

        #region ToString Tests

        [Fact]
        public void ToString_ShouldReturnCapacityAsString()
        {
            // Arrange
            var capacity = new TypeCapacity(5000);

            // Act
            var result = capacity.ToString();

            // Assert
            result.Should().Be("5000");
        }

        [Theory]
        [InlineData(1, "1")]
        [InlineData(500, "500")]
        [InlineData(14500, "14500")]
        [InlineData(23756, "23756")]
        public void ToString_WithMultipleCapacities_ShouldReturnCorrectFormat(int value, string expected)
        {
            // Arrange
            var capacity = new TypeCapacity(value);

            // Act
            var result = capacity.ToString();

            // Assert
            result.Should().Be(expected);
        }

        #endregion

        #region Equality Tests

        [Fact]
        public void Equals_WithSameCapacities_ShouldReturnTrue()
        {
            // Arrange
            var capacity1 = new TypeCapacity(5000);
            var capacity2 = new TypeCapacity(5000);

            // Act & Assert
            capacity1.Equals(capacity2).Should().BeTrue();
        }

        [Fact]
        public void Equals_WithDifferentCapacities_ShouldReturnFalse()
        {
            // Arrange
            var capacity1 = new TypeCapacity(5000);
            var capacity2 = new TypeCapacity(10000);

            // Act & Assert
            capacity1.Equals(capacity2).Should().BeFalse();
        }

        [Fact]
        public void GetHashCode_WithSameCapacities_ShouldReturnSameHashCode()
        {
            // Arrange
            var capacity1 = new TypeCapacity(5000);
            var capacity2 = new TypeCapacity(5000);

            // Act & Assert
            capacity1.GetHashCode().Should().Be(capacity2.GetHashCode());
        }

        [Fact]
        public void GetHashCode_WithDifferentCapacities_ShouldReturnDifferentHashCodes()
        {
            // Arrange
            var capacity1 = new TypeCapacity(5000);
            var capacity2 = new TypeCapacity(10000);

            // Act & Assert
            capacity1.GetHashCode().Should().NotBe(capacity2.GetHashCode());
        }

        #endregion

        #region Edge Cases

        [Fact]
        public void Constructor_WithMaxIntValue_ShouldCreateTypeCapacity()
        {
            // Arrange & Act
            var capacity = new TypeCapacity(int.MaxValue);

            // Assert
            capacity.Value.Should().Be(int.MaxValue);
        }

        [Fact]
        public void Value_Property_ShouldBeReadOnly()
        {
            // Arrange
            var capacity = new TypeCapacity(5000);

            // Act - The Value property should not have a public setter
            var propertyInfo = typeof(TypeCapacity).GetProperty(nameof(TypeCapacity.Value));

            // Assert
            propertyInfo.Should().NotBeNull();
            propertyInfo.SetMethod.Should().NotBeNull();
            propertyInfo.SetMethod.IsPublic.Should().BeFalse();
        }

        [Fact]
        public void Constructor_WithOne_ShouldCreateMinimumTypeCapacity()
        {
            // Arrange & Act
            var capacity = new TypeCapacity(1);

            // Assert
            capacity.Value.Should().Be(1);
            capacity.ToString().Should().Be("1");
        }

        #endregion

        #region Real-World Vessel Type Capacities

        [Theory]
        [InlineData(500)] // Small coastal feeder
        [InlineData(1000)] // Feeder vessel
        [InlineData(3000)] // Large feeder
        [InlineData(5000)] // Panamax typical
        [InlineData(8000)] // Post-Panamax
        [InlineData(13000)] // Large Post-Panamax
        [InlineData(14500)] // ULCV threshold
        [InlineData(18000)] // Mid-size ULCV
        [InlineData(23756)] // MSC Gülsün class
        [InlineData(24000)] // Largest vessels
        public void Constructor_WithRealWorldTypeCapacities_ShouldCreateValidTypeCapacity(int realCapacity)
        {
            // Arrange & Act
            var capacity = new TypeCapacity(realCapacity);

            // Assert
            capacity.Value.Should().Be(realCapacity);
            capacity.ToString().Should().Be(realCapacity.ToString());
        }

        [Fact]
        public void Constructor_WithFeederTypeCapacity_ShouldRepresentSmallVessels()
        {
            // Arrange - Feeder vessels typically 500-3,000 TEU
            var feederCapacity = 1500;

            // Act
            var capacity = new TypeCapacity(feederCapacity);

            // Assert
            capacity.Value.Should().Be(1500);
            capacity.Value.Should().BeLessThan(3000);
        }

        [Fact]
        public void Constructor_WithPanamaxTypeCapacity_ShouldRepresentPanamaxClass()
        {
            // Arrange - Panamax vessels typically 4,000-5,000 TEU
            var panamaxCapacity = 5000;

            // Act
            var capacity = new TypeCapacity(panamaxCapacity);

            // Assert
            capacity.Value.Should().Be(5000);
            capacity.Value.Should().BeGreaterThan(4000);
            capacity.Value.Should().BeLessThan(6000);
        }

        [Fact]
        public void Constructor_WithNeopanamaxTypeCapacity_ShouldRepresentNeopanamaxClass()
        {
            // Arrange - Neopanamax vessels can carry 10,000-14,000 TEU
            var neopanamaxCapacity = 13000;

            // Act
            var capacity = new TypeCapacity(neopanamaxCapacity);

            // Assert
            capacity.Value.Should().Be(13000);
            capacity.Value.Should().BeGreaterThan(10000);
            capacity.Value.Should().BeLessThan(14500);
        }

        [Fact]
        public void Constructor_WithULCVTypeCapacity_ShouldRepresentUltraLargeClass()
        {
            // Arrange - ULCV vessels exceed 14,500 TEU threshold
            var ulcvCapacity = 20000;

            // Act
            var capacity = new TypeCapacity(ulcvCapacity);

            // Assert
            capacity.Value.Should().Be(20000);
            capacity.Value.Should().BeGreaterThan(14500); // ULCV threshold
        }

        [Fact]
        public void Constructor_WithMegamaxTypeCapacity_ShouldRepresentLargestClass()
        {
            // Arrange - Megamax vessels are 23,000+ TEU
            var megamaxCapacity = 23756;

            // Act
            var capacity = new TypeCapacity(megamaxCapacity);

            // Assert
            capacity.Value.Should().Be(23756);
            capacity.Value.Should().BeGreaterThan(23000);
        }

        #endregion

        #region Vessel Type Classification

        [Fact]
        public void Constructor_WithCapacity_ShouldAllowTypeClassification()
        {
            // Arrange - Different capacities represent different vessel classes
            var feeder = new TypeCapacity(1500);
            var panamax = new TypeCapacity(5000);
            var ulcv = new TypeCapacity(20000);

            // Act & Assert
            feeder.Value.Should().BeLessThan(3000); // Feeder range
            panamax.Value.Should().BeInRange(4000, 6000); // Panamax range
            ulcv.Value.Should().BeGreaterThan(14500); // ULCV threshold
        }

        [Fact]
        public void Constructor_WithTypicalPanamaxCapacity_ShouldIndicateMaximumConstraint()
        {
            // Arrange - Panamax vessels are limited to ~5,000 TEU due to dimensional constraints
            var panamaxMaxCapacity = 5000;

            // Act
            var capacity = new TypeCapacity(panamaxMaxCapacity);

            // Assert
            capacity.Value.Should().Be(5000);
            capacity.Value.Should().BeLessThanOrEqualTo(5500); // Practical Panamax limit
        }

        #endregion
    }
}
