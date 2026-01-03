using FluentAssertions;
using ProjArqsi.Domain.VesselAggregate;
using Xunit;

namespace ProjArqsi.Tests.ValueObjectTests.Vessel
{
    public class LengthTests
    {
        #region Valid Length Tests

        [Fact]
        public void Constructor_WithValidLength_ShouldCreateLength()
        {
            // Arrange & Act
            var length = new Length(300.5);

            // Assert
            length.Value.Should().Be(300.5);
        }

        [Theory]
        [InlineData(0.1)]
        [InlineData(1.0)]
        [InlineData(100.5)]
        [InlineData(400.0)]
        [InlineData(399.99)] // Just under 400m (largest container ships)
        public void Constructor_WithMultipleValidLengths_ShouldCreateLength(double validLength)
        {
            // Arrange & Act
            var length = new Length(validLength);

            // Assert
            length.Value.Should().Be(validLength);
        }

        [Fact]
        public void Constructor_WithSmallPositiveValue_ShouldCreateLength()
        {
            // Arrange & Act
            var length = new Length(0.01);

            // Assert
            length.Value.Should().Be(0.01);
        }

        [Fact]
        public void Constructor_WithLargeValue_ShouldCreateLength()
        {
            // Arrange & Act
            var length = new Length(500.0);

            // Assert
            length.Value.Should().Be(500.0);
        }

        #endregion

        #region Invalid Length Tests

        [Fact]
        public void Constructor_WithZero_ShouldThrowArgumentException()
        {
            // Arrange & Act
            Action act = () => new Length(0);

            // Assert
            act.Should().Throw<ArgumentException>()
                .WithMessage("Length must be greater than zero*")
                .And.ParamName.Should().Be("value");
        }

        [Fact]
        public void Constructor_WithNegativeValue_ShouldThrowArgumentException()
        {
            // Arrange & Act
            Action act = () => new Length(-100.5);

            // Assert
            act.Should().Throw<ArgumentException>()
                .WithMessage("Length must be greater than zero*")
                .And.ParamName.Should().Be("value");
        }

        [Theory]
        [InlineData(-0.01)]
        [InlineData(-1.0)]
        [InlineData(-999.99)]
        public void Constructor_WithMultipleNegativeValues_ShouldThrowArgumentException(double invalidLength)
        {
            // Arrange & Act
            Action act = () => new Length(invalidLength);

            // Assert
            act.Should().Throw<ArgumentException>()
                .WithMessage("Length must be greater than zero*");
        }

        #endregion

        #region ToString Tests

        [Fact]
        public void ToString_ShouldReturnLengthWithMeters()
        {
            // Arrange
            var length = new Length(300.5);

            // Act
            var result = length.ToString();

            // Assert
            result.Should().Contain("300").And.Contain("5").And.Contain("meters");
        }

        [Theory]
        [InlineData(100.0, "100 meters")]
        [InlineData(250.5, "meters")]
        [InlineData(399.87, "meters")]
        public void ToString_WithMultipleLengths_ShouldReturnCorrectFormat(double value, string expected)
        {
            // Arrange
            var length = new Length(value);

            // Act
            var result = length.ToString();

            // Assert
            result.Should().Contain(expected);
        }

        [Fact]
        public void ToString_WithWholeNumber_ShouldNotIncludeDecimals()
        {
            // Arrange
            var length = new Length(300.0);

            // Act
            var result = length.ToString();

            // Assert
            result.Should().Be("300 meters");
        }

        #endregion

        #region Equality Tests

        [Fact]
        public void Equals_WithSameLengths_ShouldReturnTrue()
        {
            // Arrange
            var length1 = new Length(300.5);
            var length2 = new Length(300.5);

            // Act & Assert
            length1.Equals(length2).Should().BeTrue();
        }

        [Fact]
        public void Equals_WithDifferentLengths_ShouldReturnFalse()
        {
            // Arrange
            var length1 = new Length(300.5);
            var length2 = new Length(400.0);

            // Act & Assert
            length1.Equals(length2).Should().BeFalse();
        }

        [Fact]
        public void Equals_WithSlightlyDifferentLengths_ShouldReturnFalse()
        {
            // Arrange
            var length1 = new Length(300.5);
            var length2 = new Length(300.50001);

            // Act & Assert
            length1.Equals(length2).Should().BeFalse();
        }

        [Fact]
        public void GetHashCode_WithSameLengths_ShouldReturnSameHashCode()
        {
            // Arrange
            var length1 = new Length(300.5);
            var length2 = new Length(300.5);

            // Act & Assert
            length1.GetHashCode().Should().Be(length2.GetHashCode());
        }

        [Fact]
        public void GetHashCode_WithDifferentLengths_ShouldReturnDifferentHashCodes()
        {
            // Arrange
            var length1 = new Length(300.5);
            var length2 = new Length(400.0);

            // Act & Assert
            length1.GetHashCode().Should().NotBe(length2.GetHashCode());
        }

        #endregion

        #region Edge Cases

        [Fact]
        public void Constructor_WithVerySmallPositiveValue_ShouldCreateLength()
        {
            // Arrange & Act
            var length = new Length(0.0001);

            // Assert
            length.Value.Should().Be(0.0001);
        }

        [Fact]
        public void Constructor_WithMaxDoubleValue_ShouldCreateLength()
        {
            // Arrange & Act
            var length = new Length(double.MaxValue);

            // Assert
            length.Value.Should().Be(double.MaxValue);
        }

        [Fact]
        public void Value_Property_ShouldBeReadOnly()
        {
            // Arrange
            var length = new Length(300.5);

            // Act - The Value property should not have a public setter
            var propertyInfo = typeof(Length).GetProperty(nameof(Length.Value));

            // Assert
            propertyInfo.Should().NotBeNull();
            propertyInfo.SetMethod.Should().NotBeNull();
            propertyInfo.SetMethod.IsPublic.Should().BeFalse();
        }

        [Fact]
        public void Constructor_WithDoubleEpsilon_ShouldCreateLength()
        {
            // Arrange - Double.Epsilon is the smallest positive double value
            var length = new Length(double.Epsilon);

            // Act & Assert
            length.Value.Should().Be(double.Epsilon);
            length.Value.Should().BeGreaterThan(0);
        }

        #endregion

        #region Real-World Vessel Lengths

        [Theory]
        [InlineData(399.87)] // MSC Gülsün - one of the longest container ships (399.87m)
        [InlineData(399.9)] // HMM Algeciras class (399.9m)
        [InlineData(400.0)] // OOCL Hong Kong (399.87m, rounded to 400)
        [InlineData(366.0)] // Ever Given (366m LOA - Length Overall)
        [InlineData(294.0)] // Mid-size container ship
        [InlineData(200.0)] // Smaller container feeder ship
        [InlineData(100.0)] // Small coastal feeder
        public void Constructor_WithRealWorldVesselLengths_ShouldCreateValidLength(double realLength)
        {
            // Arrange & Act
            var length = new Length(realLength);

            // Assert
            length.Value.Should().Be(realLength);
            length.ToString().Should().Contain("meters");
        }

        [Fact]
        public void Constructor_WithEverGivenLength_ShouldRepresentActualVessel()
        {
            // Arrange - Ever Given: 366m LOA (Length Overall), famous Suez Canal vessel
            var everGivenLength = 366.0;

            // Act
            var length = new Length(everGivenLength);

            // Assert
            length.Value.Should().Be(366.0);
            length.ToString().Should().Be("366 meters");
        }

        [Fact]
        public void Constructor_WithMSCGulsunLength_ShouldRepresentLargestVessel()
        {
            // Arrange - MSC Gülsün: 399.87m, one of the world's largest container ships
            var mscGulsunLength = 399.87;

            // Act
            var length = new Length(mscGulsunLength);

            // Assert
            length.Value.Should().Be(399.87);
            length.Value.Should().BeLessThan(400.0);
            length.ToString().Should().Contain("399").And.Contain("87").And.Contain("meters");
        }

        [Fact]
        public void Constructor_WithPanamaMaxLength_ShouldRepresentPanamaMaxVessel()
        {
            // Arrange - Panamax vessels are limited to ~294m for original Panama Canal
            var panamaxLength = 294.0;

            // Act
            var length = new Length(panamaxLength);

            // Assert
            length.Value.Should().Be(294.0);
            length.Value.Should().BeLessThan(300.0); // Panamax constraint
        }

        [Fact]
        public void Constructor_WithNewPanamaMaxLength_ShouldRepresentNeopanamaxVessel()
        {
            // Arrange - Neopanamax (New Panamax) vessels can be up to 366m for expanded Panama Canal
            var neopanamaxLength = 366.0;

            // Act
            var length = new Length(neopanamaxLength);

            // Assert
            length.Value.Should().Be(366.0);
            length.Value.Should().BeLessThanOrEqualTo(366.0); // Neopanamax constraint
        }

        #endregion
    }
}
