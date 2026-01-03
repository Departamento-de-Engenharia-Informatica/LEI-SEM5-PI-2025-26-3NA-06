using FluentAssertions;
using ProjArqsi.Domain.DockAggregate;
using ProjArqsi.Domain.Shared;
using Xunit;

namespace ProjArqsi.Tests._1_ValueObjectTests.Dock
{
    public class DockLengthTests
    {
        #region Valid Creation Tests

        [Theory]
        [InlineData(1.0)]
        [InlineData(100.5)]
        [InlineData(500.0)]
        [InlineData(1000.75)]
        [InlineData(0.1)]
        public void CreateDockLength_WithValidLength_ShouldSucceed(double validLength)
        {
            // Act
            var dockLength = new DockLength(validLength);

            // Assert
            dockLength.Should().NotBeNull();
            dockLength.Value.Should().Be(validLength);
        }

        [Fact]
        public void CreateDockLength_WithVerySmallPositiveValue_ShouldSucceed()
        {
            // Arrange
            var verySmall = 0.001;

            // Act
            var dockLength = new DockLength(verySmall);

            // Assert
            dockLength.Value.Should().Be(0.001);
        }

        [Fact]
        public void CreateDockLength_WithLargeValue_ShouldSucceed()
        {
            // Arrange
            var largeValue = 10000.0;

            // Act
            var dockLength = new DockLength(largeValue);

            // Assert
            dockLength.Value.Should().Be(10000.0);
        }

        [Fact]
        public void CreateDockLength_WithPreciseDecimal_ShouldPreservePrecision()
        {
            // Arrange
            var preciseValue = 123.456789;

            // Act
            var dockLength = new DockLength(preciseValue);

            // Assert
            dockLength.Value.Should().BeApproximately(123.456789, 0.000001);
        }

        #endregion

        #region Invalid Creation Tests

        [Fact]
        public void CreateDockLength_WithZero_ShouldThrowBusinessRuleValidationException()
        {
            // Arrange
            var zeroLength = 0.0;

            // Act
            Action act = () => new DockLength(zeroLength);

            // Assert
            act.Should().Throw<BusinessRuleValidationException>()
                .WithMessage("*must be greater than 0*");
        }

        [Theory]
        [InlineData(-1.0)]
        [InlineData(-100.5)]
        [InlineData(-0.1)]
        public void CreateDockLength_WithNegativeValue_ShouldThrowBusinessRuleValidationException(double negativeLength)
        {
            // Act
            Action act = () => new DockLength(negativeLength);

            // Assert
            act.Should().Throw<BusinessRuleValidationException>()
                .WithMessage("*must be greater than 0*");
        }

        [Fact]
        public void CreateDockLength_WithVeryLargeNegative_ShouldThrowBusinessRuleValidationException()
        {
            // Arrange
            var veryLargeNegative = -999999.99;

            // Act
            Action act = () => new DockLength(veryLargeNegative);

            // Assert
            act.Should().Throw<BusinessRuleValidationException>()
                .WithMessage("*must be greater than 0*");
        }

        #endregion

        #region Equality Tests

        [Fact]
        public void DockLength_WithSameValue_ShouldBeEqual()
        {
            // Arrange
            var length1 = new DockLength(100.5);
            var length2 = new DockLength(100.5);

            // Act & Assert
            length1.Should().Be(length2);
            length1.GetHashCode().Should().Be(length2.GetHashCode());
        }

        [Fact]
        public void DockLength_WithDifferentValues_ShouldNotBeEqual()
        {
            // Arrange
            var length1 = new DockLength(100.5);
            var length2 = new DockLength(200.5);

            // Act & Assert
            length1.Should().NotBe(length2);
            length1.GetHashCode().Should().NotBe(length2.GetHashCode());
        }

        [Fact]
        public void DockLength_ComparedToNull_ShouldNotBeEqual()
        {
            // Arrange
            var dockLength = new DockLength(100.0);

            // Act & Assert
            dockLength.Should().NotBe(null);
        }

        [Fact]
        public void DockLength_WithSlightlyDifferentValues_ShouldNotBeEqual()
        {
            // Arrange
            var length1 = new DockLength(100.001);
            var length2 = new DockLength(100.002);

            // Act & Assert
            length1.Should().NotBe(length2);
        }

        #endregion

        #region Edge Case Tests

        [Fact]
        public void CreateDockLength_WithMinimumPositiveValue_ShouldSucceed()
        {
            // Arrange
            var minPositive = double.Epsilon;

            // Act
            var dockLength = new DockLength(minPositive);

            // Assert
            dockLength.Value.Should().Be(double.Epsilon);
        }

        [Fact]
        public void CreateDockLength_WithMaxValue_ShouldSucceed()
        {
            // Arrange - Using a large but reasonable value
            var maxValue = 99999.99;

            // Act
            var dockLength = new DockLength(maxValue);

            // Assert
            dockLength.Value.Should().Be(99999.99);
        }

        [Fact]
        public void CreateDockLength_WithTypicalValue_ShouldSucceed()
        {
            // Arrange - Typical dock length in meters
            var typicalLength = 350.0;

            // Act
            var dockLength = new DockLength(typicalLength);

            // Assert
            dockLength.Value.Should().Be(350.0);
        }

        [Fact]
        public void CreateDockLength_WithOneDecimalPlace_ShouldPreservePrecision()
        {
            // Arrange
            var oneDecimal = 100.5;

            // Act
            var dockLength = new DockLength(oneDecimal);

            // Assert
            dockLength.Value.Should().Be(100.5);
        }

        #endregion
    }
}
