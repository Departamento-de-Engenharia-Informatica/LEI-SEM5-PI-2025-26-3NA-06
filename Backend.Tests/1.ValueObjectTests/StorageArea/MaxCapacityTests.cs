using FluentAssertions;
using ProjArqsi.Domain.StorageAreaAggregate;
using Xunit;

namespace ProjArqsi.Tests.ValueObjectTests.StorageArea
{
    public class MaxCapacityTests
    {
        #region Valid Max Capacity Tests

        [Fact]
        public void Constructor_WithValidCapacity_ShouldCreateMaxCapacity()
        {
            // Arrange
            var capacity = 100;

            // Act
            var maxCapacity = new MaxCapacity(capacity);

            // Assert
            maxCapacity.Should().NotBeNull();
            maxCapacity.Value.Should().Be(capacity);
        }

        [Theory]
        [InlineData(1)]
        [InlineData(10)]
        [InlineData(100)]
        [InlineData(1000)]
        [InlineData(10000)]
        public void Constructor_WithVariousValidCapacities_ShouldCreateMaxCapacity(int capacity)
        {
            // Act
            var maxCapacity = new MaxCapacity(capacity);

            // Assert
            maxCapacity.Value.Should().Be(capacity);
        }

        [Fact]
        public void Constructor_WithSmallCapacity_ShouldCreateMaxCapacity()
        {
            // Arrange
            var capacity = 5;

            // Act
            var maxCapacity = new MaxCapacity(capacity);

            // Assert
            maxCapacity.Value.Should().Be(capacity);
        }

        [Fact]
        public void Constructor_WithLargeCapacity_ShouldCreateMaxCapacity()
        {
            // Arrange
            var capacity = 999999;

            // Act
            var maxCapacity = new MaxCapacity(capacity);

            // Assert
            maxCapacity.Value.Should().Be(capacity);
        }

        [Fact]
        public void Constructor_WithMaxIntValue_ShouldCreateMaxCapacity()
        {
            // Arrange
            var capacity = int.MaxValue;

            // Act
            var maxCapacity = new MaxCapacity(capacity);

            // Assert
            maxCapacity.Value.Should().Be(capacity);
        }

        [Fact]
        public void Constructor_WithTypicalWarehouseCapacity_ShouldCreateMaxCapacity()
        {
            // Arrange
            var capacity = 500; // Typical warehouse capacity

            // Act
            var maxCapacity = new MaxCapacity(capacity);

            // Assert
            maxCapacity.Value.Should().Be(capacity);
        }

        [Fact]
        public void Constructor_WithTypicalYardCapacity_ShouldCreateMaxCapacity()
        {
            // Arrange
            var capacity = 2000; // Typical yard capacity

            // Act
            var maxCapacity = new MaxCapacity(capacity);

            // Assert
            maxCapacity.Value.Should().Be(capacity);
        }

        #endregion

        #region Invalid Max Capacity Tests

        [Fact]
        public void Constructor_WithZero_ShouldThrowArgumentOutOfRangeException()
        {
            // Arrange
            var capacity = 0;

            // Act
            Action act = () => new MaxCapacity(capacity);

            // Assert
            act.Should().Throw<ArgumentOutOfRangeException>()
                .WithMessage("*Max capacity must be greater than zero.*")
                .And.ParamName.Should().Be("value");
        }

        [Fact]
        public void Constructor_WithNegativeValue_ShouldThrowArgumentOutOfRangeException()
        {
            // Arrange
            var capacity = -1;

            // Act
            Action act = () => new MaxCapacity(capacity);

            // Assert
            act.Should().Throw<ArgumentOutOfRangeException>()
                .WithMessage("*Max capacity must be greater than zero.*")
                .And.ParamName.Should().Be("value");
        }

        [Theory]
        [InlineData(-1)]
        [InlineData(-10)]
        [InlineData(-100)]
        [InlineData(-1000)]
        public void Constructor_WithVariousNegativeValues_ShouldThrowArgumentOutOfRangeException(int capacity)
        {
            // Act
            Action act = () => new MaxCapacity(capacity);

            // Assert
            act.Should().Throw<ArgumentOutOfRangeException>()
                .WithMessage("*Max capacity must be greater than zero.*");
        }

        [Fact]
        public void Constructor_WithMinIntValue_ShouldThrowArgumentOutOfRangeException()
        {
            // Arrange
            var capacity = int.MinValue;

            // Act
            Action act = () => new MaxCapacity(capacity);

            // Assert
            act.Should().Throw<ArgumentOutOfRangeException>()
                .WithMessage("*Max capacity must be greater than zero.*");
        }

        #endregion

        #region Equality Tests

        [Fact]
        public void Equals_WithSameCapacity_ShouldReturnTrue()
        {
            // Arrange
            var capacity1 = new MaxCapacity(100);
            var capacity2 = new MaxCapacity(100);

            // Act
            var result = capacity1.Equals(capacity2);

            // Assert
            result.Should().BeTrue();
        }

        [Fact]
        public void Equals_WithDifferentCapacity_ShouldReturnFalse()
        {
            // Arrange
            var capacity1 = new MaxCapacity(100);
            var capacity2 = new MaxCapacity(200);

            // Act
            var result = capacity1.Equals(capacity2);

            // Assert
            result.Should().BeFalse();
        }

        [Fact]
        public void Equals_WithSlightlyDifferentCapacity_ShouldReturnFalse()
        {
            // Arrange
            var capacity1 = new MaxCapacity(100);
            var capacity2 = new MaxCapacity(101);

            // Act
            var result = capacity1.Equals(capacity2);

            // Assert
            result.Should().BeFalse();
        }

        [Fact]
        public void GetHashCode_WithSameCapacity_ShouldReturnSameHashCode()
        {
            // Arrange
            var capacity1 = new MaxCapacity(150);
            var capacity2 = new MaxCapacity(150);

            // Act
            var hash1 = capacity1.GetHashCode();
            var hash2 = capacity2.GetHashCode();

            // Assert
            hash1.Should().Be(hash2);
        }

        [Fact]
        public void GetHashCode_WithDifferentCapacity_ShouldReturnDifferentHashCode()
        {
            // Arrange
            var capacity1 = new MaxCapacity(150);
            var capacity2 = new MaxCapacity(250);

            // Act
            var hash1 = capacity1.GetHashCode();
            var hash2 = capacity2.GetHashCode();

            // Assert
            hash1.Should().NotBe(hash2);
        }

        #endregion

        #region Edge Cases

        [Fact]
        public void Constructor_WithOne_ShouldCreateMaxCapacity()
        {
            // Arrange
            var capacity = 1;

            // Act
            var maxCapacity = new MaxCapacity(capacity);

            // Assert
            maxCapacity.Value.Should().Be(1);
        }

        [Fact]
        public void Value_AfterConstruction_ShouldBeImmutable()
        {
            // Arrange
            var capacity = 100;
            var maxCapacity = new MaxCapacity(capacity);

            // Act
            var retrievedValue = maxCapacity.Value;

            // Assert
            retrievedValue.Should().Be(capacity);
        }

        [Fact]
        public void Equals_WithNull_ShouldReturnFalse()
        {
            // Arrange
            var capacity = new MaxCapacity(100);

            // Act
            var result = capacity.Equals(null);

            // Assert
            result.Should().BeFalse();
        }

        [Fact]
        public void Equals_WithArbitraryObject_ShouldReturnFalse()
        {
            // Arrange
            var capacity = new MaxCapacity(100);
            var arbitraryObject = new { Capacity = 100 };

            // Act
            var result = capacity.Equals(arbitraryObject);

            // Assert
            result.Should().BeFalse();
        }

        [Theory]
        [InlineData(1)]
        [InlineData(50)]
        [InlineData(100)]
        [InlineData(500)]
        [InlineData(1000)]
        [InlineData(5000)]
        public void GetHashCode_WithDifferentCapacities_ShouldProduceDifferentHashes(int capacity)
        {
            // Arrange
            var maxCapacity = new MaxCapacity(capacity);
            var differentCapacity = new MaxCapacity(capacity + 1);

            // Act
            var hash1 = maxCapacity.GetHashCode();
            var hash2 = differentCapacity.GetHashCode();

            // Assert
            hash1.Should().NotBe(hash2);
        }

        [Fact]
        public void Constructor_CalledMultipleTimes_ShouldCreateIndependentObjects()
        {
            // Arrange & Act
            var capacity1 = new MaxCapacity(100);
            var capacity2 = new MaxCapacity(100);

            // Assert
            capacity1.Should().NotBeSameAs(capacity2);
            capacity1.Should().Be(capacity2);
        }

        [Fact]
        public void Value_ShouldBeReadOnly()
        {
            // Arrange
            var capacity = new MaxCapacity(100);

            // Act - Attempting to access Value multiple times
            var value1 = capacity.Value;
            var value2 = capacity.Value;

            // Assert
            value1.Should().Be(value2);
            value1.Should().Be(100);
        }

        #endregion

        #region Real-World Scenario Tests

        [Fact]
        public void MaxCapacity_ForSmallWarehouse_ShouldBeValid()
        {
            // Arrange - Small warehouse typically holds 50-200 containers
            var capacity = 150;

            // Act
            var maxCapacity = new MaxCapacity(capacity);

            // Assert
            maxCapacity.Value.Should().Be(capacity);
        }

        [Fact]
        public void MaxCapacity_ForLargeYard_ShouldBeValid()
        {
            // Arrange - Large yard can hold 5000+ containers
            var capacity = 5000;

            // Act
            var maxCapacity = new MaxCapacity(capacity);

            // Assert
            maxCapacity.Value.Should().Be(capacity);
        }

        [Fact]
        public void MaxCapacity_ForMediumTerminal_ShouldBeValid()
        {
            // Arrange - Medium terminal holds 1000-3000 containers
            var capacity = 2000;

            // Act
            var maxCapacity = new MaxCapacity(capacity);

            // Assert
            maxCapacity.Value.Should().Be(capacity);
        }

        #endregion
    }
}
