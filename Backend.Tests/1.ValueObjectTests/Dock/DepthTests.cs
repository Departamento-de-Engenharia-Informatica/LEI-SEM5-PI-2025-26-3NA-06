using FluentAssertions;
using ProjArqsi.Domain.DockAggregate;
using ProjArqsi.Domain.Shared;
using Xunit;

namespace ProjArqsi.Tests._1_ValueObjectTests.Dock
{
    public class DepthTests
    {
        #region Valid Creation Tests

        [Theory]
        [InlineData(1.0)]
        [InlineData(10.5)]
        [InlineData(50.0)]
        [InlineData(100.75)]
        [InlineData(0.1)]
        public void CreateDepth_WithValidDepth_ShouldSucceed(double validDepth)
        {
            // Act
            var depth = new Depth(validDepth);

            // Assert
            depth.Should().NotBeNull();
            depth.Value.Should().Be(validDepth);
        }

        [Fact]
        public void CreateDepth_WithVerySmallPositiveValue_ShouldSucceed()
        {
            // Arrange
            var verySmall = 0.001;

            // Act
            var depth = new Depth(verySmall);

            // Assert
            depth.Value.Should().Be(0.001);
        }

        [Fact]
        public void CreateDepth_WithLargeValue_ShouldSucceed()
        {
            // Arrange
            var largeValue = 500.0;

            // Act
            var depth = new Depth(largeValue);

            // Assert
            depth.Value.Should().Be(500.0);
        }

        [Fact]
        public void CreateDepth_WithPreciseDecimal_ShouldPreservePrecision()
        {
            // Arrange
            var preciseValue = 15.456789;

            // Act
            var depth = new Depth(preciseValue);

            // Assert
            depth.Value.Should().BeApproximately(15.456789, 0.000001);
        }

        #endregion

        #region Invalid Creation Tests

        [Fact]
        public void CreateDepth_WithZero_ShouldThrowBusinessRuleValidationException()
        {
            // Arrange
            var zeroDepth = 0.0;

            // Act
            Action act = () => new Depth(zeroDepth);

            // Assert
            act.Should().Throw<BusinessRuleValidationException>()
                .WithMessage("*must be greater than 0*");
        }

        [Theory]
        [InlineData(-1.0)]
        [InlineData(-10.5)]
        [InlineData(-0.1)]
        public void CreateDepth_WithNegativeValue_ShouldThrowBusinessRuleValidationException(double negativeDepth)
        {
            // Act
            Action act = () => new Depth(negativeDepth);

            // Assert
            act.Should().Throw<BusinessRuleValidationException>()
                .WithMessage("*must be greater than 0*");
        }

        [Fact]
        public void CreateDepth_WithVeryLargeNegative_ShouldThrowBusinessRuleValidationException()
        {
            // Arrange
            var veryLargeNegative = -99999.99;

            // Act
            Action act = () => new Depth(veryLargeNegative);

            // Assert
            act.Should().Throw<BusinessRuleValidationException>()
                .WithMessage("*must be greater than 0*");
        }

        #endregion

        #region Equality Tests

        [Fact]
        public void Depth_WithSameValue_ShouldBeEqual()
        {
            // Arrange
            var depth1 = new Depth(15.5);
            var depth2 = new Depth(15.5);

            // Act & Assert
            depth1.Should().Be(depth2);
            depth1.GetHashCode().Should().Be(depth2.GetHashCode());
        }

        [Fact]
        public void Depth_WithDifferentValues_ShouldNotBeEqual()
        {
            // Arrange
            var depth1 = new Depth(15.5);
            var depth2 = new Depth(20.5);

            // Act & Assert
            depth1.Should().NotBe(depth2);
            depth1.GetHashCode().Should().NotBe(depth2.GetHashCode());
        }

        [Fact]
        public void Depth_ComparedToNull_ShouldNotBeEqual()
        {
            // Arrange
            var depth = new Depth(15.0);

            // Act & Assert
            depth.Should().NotBe(null);
        }

        [Fact]
        public void Depth_WithSlightlyDifferentValues_ShouldNotBeEqual()
        {
            // Arrange
            var depth1 = new Depth(15.001);
            var depth2 = new Depth(15.002);

            // Act & Assert
            depth1.Should().NotBe(depth2);
        }

        #endregion

        #region Edge Case Tests

        [Fact]
        public void CreateDepth_WithMinimumPositiveValue_ShouldSucceed()
        {
            // Arrange
            var minPositive = double.Epsilon;

            // Act
            var depth = new Depth(minPositive);

            // Assert
            depth.Value.Should().Be(double.Epsilon);
        }

        [Fact]
        public void CreateDepth_WithTypicalPortDepth_ShouldSucceed()
        {
            // Arrange - Typical deep water port depth in meters
            var typicalDepth = 18.0;

            // Act
            var depth = new Depth(typicalDepth);

            // Assert
            depth.Value.Should().Be(18.0);
        }

        [Fact]
        public void CreateDepth_WithShallowWaterDepth_ShouldSucceed()
        {
            // Arrange - Shallow water depth
            var shallowDepth = 5.5;

            // Act
            var depth = new Depth(shallowDepth);

            // Assert
            depth.Value.Should().Be(5.5);
        }

        [Fact]
        public void CreateDepth_WithDeepWaterDepth_ShouldSucceed()
        {
            // Arrange - Deep water port depth
            var deepDepth = 25.0;

            // Act
            var depth = new Depth(deepDepth);

            // Assert
            depth.Value.Should().Be(25.0);
        }

        [Fact]
        public void CreateDepth_WithOneDecimalPlace_ShouldPreservePrecision()
        {
            // Arrange
            var oneDecimal = 12.5;

            // Act
            var depth = new Depth(oneDecimal);

            // Assert
            depth.Value.Should().Be(12.5);
        }

        #endregion
    }
}
