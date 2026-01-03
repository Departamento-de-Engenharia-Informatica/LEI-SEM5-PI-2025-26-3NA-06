using FluentAssertions;
using ProjArqsi.Domain.VesselTypeAggregate;
using Xunit;

namespace ProjArqsi.Tests.ValueObjectTests.VesselType
{
    public class MaxRowsTests
    {
        #region Valid MaxRows Tests

        [Fact]
        public void Constructor_WithValidMaxRows_ShouldCreateMaxRows()
        {
            // Arrange & Act
            var maxRows = new MaxRows(13);

            // Assert
            maxRows.Value.Should().Be(13);
        }

        [Theory]
        [InlineData(1)]
        [InlineData(8)]
        [InlineData(13)] // Panamax constraint
        [InlineData(20)] // Neopanamax
        [InlineData(24)] // ULCV maximum
        public void Constructor_WithMultipleValidMaxRows_ShouldCreateMaxRows(int validMaxRows)
        {
            // Arrange & Act
            var maxRows = new MaxRows(validMaxRows);

            // Assert
            maxRows.Value.Should().Be(validMaxRows);
        }

        [Fact]
        public void Constructor_WithMinimumMaxRows_ShouldCreateMaxRows()
        {
            // Arrange & Act
            var maxRows = new MaxRows(1);

            // Assert
            maxRows.Value.Should().Be(1);
        }

        [Fact]
        public void Constructor_WithLargeMaxRows_ShouldCreateMaxRows()
        {
            // Arrange & Act
            var maxRows = new MaxRows(30);

            // Assert
            maxRows.Value.Should().Be(30);
        }

        #endregion

        #region Invalid MaxRows Tests

        [Fact]
        public void Constructor_WithZero_ShouldThrowArgumentException()
        {
            // Arrange & Act
            Action act = () => new MaxRows(0);

            // Assert
            act.Should().Throw<ArgumentException>()
                .WithMessage("Max rows must be greater than zero*")
                .And.ParamName.Should().Be("value");
        }

        [Fact]
        public void Constructor_WithNegativeValue_ShouldThrowArgumentException()
        {
            // Arrange & Act
            Action act = () => new MaxRows(-13);

            // Assert
            act.Should().Throw<ArgumentException>()
                .WithMessage("Max rows must be greater than zero*")
                .And.ParamName.Should().Be("value");
        }

        [Theory]
        [InlineData(-1)]
        [InlineData(-5)]
        [InlineData(-100)]
        public void Constructor_WithMultipleNegativeValues_ShouldThrowArgumentException(int invalidMaxRows)
        {
            // Arrange & Act
            Action act = () => new MaxRows(invalidMaxRows);

            // Assert
            act.Should().Throw<ArgumentException>()
                .WithMessage("Max rows must be greater than zero*");
        }

        #endregion

        #region ToString Tests

        [Fact]
        public void ToString_ShouldReturnMaxRowsAsString()
        {
            // Arrange
            var maxRows = new MaxRows(13);

            // Act
            var result = maxRows.ToString();

            // Assert
            result.Should().Be("13");
        }

        [Theory]
        [InlineData(1, "1")]
        [InlineData(13, "13")]
        [InlineData(24, "24")]
        public void ToString_WithMultipleMaxRows_ShouldReturnCorrectFormat(int value, string expected)
        {
            // Arrange
            var maxRows = new MaxRows(value);

            // Act
            var result = maxRows.ToString();

            // Assert
            result.Should().Be(expected);
        }

        #endregion

        #region Equality Tests

        [Fact]
        public void Equals_WithSameMaxRows_ShouldReturnTrue()
        {
            // Arrange
            var maxRows1 = new MaxRows(13);
            var maxRows2 = new MaxRows(13);

            // Act & Assert
            maxRows1.Equals(maxRows2).Should().BeTrue();
        }

        [Fact]
        public void Equals_WithDifferentMaxRows_ShouldReturnFalse()
        {
            // Arrange
            var maxRows1 = new MaxRows(13);
            var maxRows2 = new MaxRows(24);

            // Act & Assert
            maxRows1.Equals(maxRows2).Should().BeFalse();
        }

        [Fact]
        public void GetHashCode_WithSameMaxRows_ShouldReturnSameHashCode()
        {
            // Arrange
            var maxRows1 = new MaxRows(13);
            var maxRows2 = new MaxRows(13);

            // Act & Assert
            maxRows1.GetHashCode().Should().Be(maxRows2.GetHashCode());
        }

        [Fact]
        public void GetHashCode_WithDifferentMaxRows_ShouldReturnDifferentHashCodes()
        {
            // Arrange
            var maxRows1 = new MaxRows(13);
            var maxRows2 = new MaxRows(24);

            // Act & Assert
            maxRows1.GetHashCode().Should().NotBe(maxRows2.GetHashCode());
        }

        #endregion

        #region Edge Cases

        [Fact]
        public void Constructor_WithMaxIntValue_ShouldCreateMaxRows()
        {
            // Arrange & Act
            var maxRows = new MaxRows(int.MaxValue);

            // Assert
            maxRows.Value.Should().Be(int.MaxValue);
        }

        [Fact]
        public void Value_Property_ShouldBeReadOnly()
        {
            // Arrange
            var maxRows = new MaxRows(13);

            // Act - The Value property should not have a public setter
            var propertyInfo = typeof(MaxRows).GetProperty(nameof(MaxRows.Value));

            // Assert
            propertyInfo.Should().NotBeNull();
            propertyInfo.SetMethod.Should().NotBeNull();
            propertyInfo.SetMethod.IsPublic.Should().BeFalse();
        }

        [Fact]
        public void Constructor_WithOne_ShouldCreateMinimumMaxRows()
        {
            // Arrange & Act
            var maxRows = new MaxRows(1);

            // Assert
            maxRows.Value.Should().Be(1);
            maxRows.ToString().Should().Be("1");
        }

        #endregion

        #region Real-World Vessel Type Row Configurations

        [Theory]
        [InlineData(6)] // Small feeder vessels
        [InlineData(8)] // Feeder vessels
        [InlineData(10)] // Small Post-Panamax
        [InlineData(11)] // Smaller Panamax
        [InlineData(13)] // Panamax vessels (32.3m beam constraint)
        [InlineData(18)] // Mid-size Post-Panamax
        [InlineData(20)] // Large Post-Panamax/Neopanamax
        [InlineData(22)] // Ever Given class
        [InlineData(23)] // Large container vessels
        [InlineData(24)] // ULCV maximum (MSC Gülsün - 61m beam)
        public void Constructor_WithRealWorldMaxRows_ShouldCreateValidMaxRows(int realMaxRows)
        {
            // Arrange & Act
            var maxRows = new MaxRows(realMaxRows);

            // Assert
            maxRows.Value.Should().Be(realMaxRows);
            maxRows.ToString().Should().Be(realMaxRows.ToString());
        }

        [Fact]
        public void Constructor_WithFeederTypeMaxRows_ShouldRepresentSmallVesselType()
        {
            // Arrange - Feeder vessel types typically have 6-10 rows
            var feederMaxRows = 8;

            // Act
            var maxRows = new MaxRows(feederMaxRows);

            // Assert
            maxRows.Value.Should().Be(8);
            maxRows.Value.Should().BeLessThan(11);
        }

        [Fact]
        public void Constructor_WithPanamaxTypeMaxRows_ShouldRepresentPanamaxConstraint()
        {
            // Arrange - Panamax vessel types limited to 13 rows (32.3m beam width)
            var panamaxMaxRows = 13;

            // Act
            var maxRows = new MaxRows(panamaxMaxRows);

            // Assert
            maxRows.Value.Should().Be(13);
            maxRows.Value.Should().BeLessThanOrEqualTo(13); // Strict Panamax constraint
        }

        [Fact]
        public void Constructor_WithNeopanamaxTypeMaxRows_ShouldRepresentExpandedCanal()
        {
            // Arrange - Neopanamax types can have up to 18-20 rows (49m beam)
            var neopanamaxMaxRows = 20;

            // Act
            var maxRows = new MaxRows(neopanamaxMaxRows);

            // Assert
            maxRows.Value.Should().Be(20);
            maxRows.Value.Should().BeGreaterThan(13); // Exceeds Panamax
            maxRows.Value.Should().BeLessThanOrEqualTo(22); // Typical Neopanamax limit
        }

        [Fact]
        public void Constructor_WithULCVTypeMaxRows_ShouldRepresentLargestClass()
        {
            // Arrange - ULCV vessel types can have up to 24 rows (61m beam)
            var ulcvMaxRows = 24;

            // Act
            var maxRows = new MaxRows(ulcvMaxRows);

            // Assert
            maxRows.Value.Should().Be(24);
            maxRows.Value.Should().BeGreaterThan(20);
        }

        [Fact]
        public void Constructor_WithEverGivenTypeMaxRows_ShouldRepresentLargePostPanamax()
        {
            // Arrange - Ever Given type vessels have 22 rows
            var everGivenTypeRows = 22;

            // Act
            var maxRows = new MaxRows(everGivenTypeRows);

            // Assert
            maxRows.Value.Should().Be(22);
        }

        #endregion

        #region Vessel Type Classification

        [Fact]
        public void Constructor_WithMaxRows_ShouldDefineTypeBeamCapability()
        {
            // Arrange - MaxRows defines the transverse (beam) container capacity for a vessel type
            var feederType = new MaxRows(8);
            var panamaxType = new MaxRows(13);
            var ulcvType = new MaxRows(24);

            // Act & Assert
            feederType.Value.Should().BeLessThan(11); // Feeder range
            panamaxType.Value.Should().Be(13); // Panamax constraint
            ulcvType.Value.Should().BeGreaterThan(20); // ULCV range
        }

        [Fact]
        public void Constructor_WithPanamaxMaxRows_ShouldIndicateBeamConstraint()
        {
            // Arrange - Panamax types strictly limited to 13 rows due to 32.3m beam constraint
            var panamaxMaxRows = 13;

            // Act
            var maxRows = new MaxRows(panamaxMaxRows);

            // Assert
            maxRows.Value.Should().Be(13);
            maxRows.Value.Should().BeLessThanOrEqualTo(13); // Cannot exceed for Panamax
        }

        [Fact]
        public void Constructor_WithMSCGulsunTypeMaxRows_ShouldRepresentMaximumBeam()
        {
            // Arrange - MSC Gülsün type has 24 rows (61m beam width)
            var mscGulsunTypeRows = 24;

            // Act
            var maxRows = new MaxRows(mscGulsunTypeRows);

            // Assert
            maxRows.Value.Should().Be(24);
            maxRows.Value.Should().BeGreaterThan(22); // Exceeds most vessel types
        }

        #endregion
    }
}
