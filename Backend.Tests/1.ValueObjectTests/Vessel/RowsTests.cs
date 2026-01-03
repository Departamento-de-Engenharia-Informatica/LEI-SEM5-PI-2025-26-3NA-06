using FluentAssertions;
using ProjArqsi.Domain.VesselAggregate;
using Xunit;

namespace ProjArqsi.Tests.ValueObjectTests.Vessel
{
    public class RowsTests
    {
        #region Valid Rows Tests

        [Fact]
        public void Constructor_WithValidRows_ShouldCreateRows()
        {
            // Arrange & Act
            var rows = new Rows(13);

            // Assert
            rows.Value.Should().Be(13);
        }

        [Theory]
        [InlineData(1)]
        [InlineData(10)]
        [InlineData(13)] // Common for large container ships
        [InlineData(24)] // Maximum for ultra-large vessels
        [InlineData(30)]
        public void Constructor_WithMultipleValidRows_ShouldCreateRows(int validRows)
        {
            // Arrange & Act
            var rows = new Rows(validRows);

            // Assert
            rows.Value.Should().Be(validRows);
        }

        [Fact]
        public void Constructor_WithMinimumRows_ShouldCreateRows()
        {
            // Arrange & Act
            var rows = new Rows(1);

            // Assert
            rows.Value.Should().Be(1);
        }

        [Fact]
        public void Constructor_WithLargeRows_ShouldCreateRows()
        {
            // Arrange & Act
            var rows = new Rows(50);

            // Assert
            rows.Value.Should().Be(50);
        }

        #endregion

        #region Invalid Rows Tests

        [Fact]
        public void Constructor_WithZero_ShouldThrowArgumentException()
        {
            // Arrange & Act
            Action act = () => new Rows(0);

            // Assert
            act.Should().Throw<ArgumentException>()
                .WithMessage("Rows must be a positive number*")
                .And.ParamName.Should().Be("value");
        }

        [Fact]
        public void Constructor_WithNegativeValue_ShouldThrowArgumentException()
        {
            // Arrange & Act
            Action act = () => new Rows(-13);

            // Assert
            act.Should().Throw<ArgumentException>()
                .WithMessage("Rows must be a positive number*")
                .And.ParamName.Should().Be("value");
        }

        [Theory]
        [InlineData(-1)]
        [InlineData(-5)]
        [InlineData(-100)]
        public void Constructor_WithMultipleNegativeValues_ShouldThrowArgumentException(int invalidRows)
        {
            // Arrange & Act
            Action act = () => new Rows(invalidRows);

            // Assert
            act.Should().Throw<ArgumentException>()
                .WithMessage("Rows must be a positive number*");
        }

        #endregion

        #region Equality Tests

        [Fact]
        public void Equals_WithSameRows_ShouldReturnTrue()
        {
            // Arrange
            var rows1 = new Rows(13);
            var rows2 = new Rows(13);

            // Act & Assert
            rows1.Equals(rows2).Should().BeTrue();
        }

        [Fact]
        public void Equals_WithDifferentRows_ShouldReturnFalse()
        {
            // Arrange
            var rows1 = new Rows(13);
            var rows2 = new Rows(24);

            // Act & Assert
            rows1.Equals(rows2).Should().BeFalse();
        }

        [Fact]
        public void GetHashCode_WithSameRows_ShouldReturnSameHashCode()
        {
            // Arrange
            var rows1 = new Rows(13);
            var rows2 = new Rows(13);

            // Act & Assert
            rows1.GetHashCode().Should().Be(rows2.GetHashCode());
        }

        [Fact]
        public void GetHashCode_WithDifferentRows_ShouldReturnDifferentHashCodes()
        {
            // Arrange
            var rows1 = new Rows(13);
            var rows2 = new Rows(24);

            // Act & Assert
            rows1.GetHashCode().Should().NotBe(rows2.GetHashCode());
        }

        #endregion

        #region Edge Cases

        [Fact]
        public void Constructor_WithMaxIntValue_ShouldCreateRows()
        {
            // Arrange & Act
            var rows = new Rows(int.MaxValue);

            // Assert
            rows.Value.Should().Be(int.MaxValue);
        }

        [Fact]
        public void Value_Property_ShouldBeReadOnly()
        {
            // Arrange
            var rows = new Rows(13);

            // Act - The Value property should not have a public setter
            var propertyInfo = typeof(Rows).GetProperty(nameof(Rows.Value));

            // Assert
            propertyInfo.Should().NotBeNull();
            propertyInfo.SetMethod.Should().NotBeNull();
            propertyInfo.SetMethod.IsPublic.Should().BeFalse();
        }

        [Fact]
        public void Constructor_WithOne_ShouldCreateMinimumRows()
        {
            // Arrange & Act
            var rows = new Rows(1);

            // Assert
            rows.Value.Should().Be(1);
        }

        #endregion

        #region Real-World Vessel Row Configurations

        [Theory]
        [InlineData(24)] // Ultra-large container vessels (MSC Gülsün, HMM Algeciras) - maximum width
        [InlineData(23)] // Large container vessels
        [InlineData(22)] // Ever Given - 22 rows across
        [InlineData(20)] // Large Post-Panamax vessels
        [InlineData(18)] // Mid-size Post-Panamax vessels
        [InlineData(13)] // Panamax vessels (13 containers wide)
        [InlineData(11)] // Smaller Panamax
        [InlineData(10)] // Small Post-Panamax
        [InlineData(8)] // Feeder vessels
        [InlineData(6)] // Small feeder vessels
        public void Constructor_WithRealWorldRowConfigurations_ShouldCreateValidRows(int realRows)
        {
            // Arrange & Act
            var rows = new Rows(realRows);

            // Assert
            rows.Value.Should().Be(realRows);
        }

        [Fact]
        public void Constructor_WithEverGivenRows_ShouldRepresentActualVessel()
        {
            // Arrange - Ever Given has 22 rows (containers across the beam)
            var everGivenRows = 22;

            // Act
            var rows = new Rows(everGivenRows);

            // Assert
            rows.Value.Should().Be(22);
        }

        [Fact]
        public void Constructor_WithUltraLargeVesselRows_ShouldRepresentULCV()
        {
            // Arrange - Ultra Large Container Vessels can have up to 24 rows across
            var ulcvRows = 24;

            // Act
            var rows = new Rows(ulcvRows);

            // Assert
            rows.Value.Should().Be(24);
            rows.Value.Should().BeGreaterThan(20); // ULCV typically 20+ rows
        }

        [Fact]
        public void Constructor_WithPanamaxRows_ShouldRepresentPanamaxVessel()
        {
            // Arrange - Panamax vessels are limited to 13 rows (containers across beam)
            // This is due to the width constraint of the original Panama Canal locks (32.3m)
            var panamaxRows = 13;

            // Act
            var rows = new Rows(panamaxRows);

            // Assert
            rows.Value.Should().Be(13);
            rows.Value.Should().BeLessThanOrEqualTo(13); // Panamax constraint
        }

        [Fact]
        public void Constructor_WithNewPanamaxRows_ShouldRepresentNeopanamaxVessel()
        {
            // Arrange - Neopanamax vessels can have up to 18-20 rows across
            // Expanded Panama Canal allows 49m width
            var neopanamaxRows = 20;

            // Act
            var rows = new Rows(neopanamaxRows);

            // Assert
            rows.Value.Should().Be(20);
            rows.Value.Should().BeGreaterThan(13); // More than Panamax
            rows.Value.Should().BeLessThanOrEqualTo(22); // Typical Neopanamax limit
        }

        [Fact]
        public void Constructor_WithFeederVesselRows_ShouldRepresentSmallVessel()
        {
            // Arrange - Feeder vessels typically have 6-10 rows
            var feederRows = 8;

            // Act
            var rows = new Rows(feederRows);

            // Assert
            rows.Value.Should().Be(8);
            rows.Value.Should().BeLessThan(11);
        }

        [Fact]
        public void Constructor_WithMSCGulsunRows_ShouldRepresentLargestVessel()
        {
            // Arrange - MSC Gülsün has 24 rows across (61m beam width)
            var mscGulsunRows = 24;

            // Act
            var rows = new Rows(mscGulsunRows);

            // Assert
            rows.Value.Should().Be(24);
            rows.Value.Should().BeGreaterThan(22); // Exceeds most vessels
        }

        #endregion

        #region Container Stacking Context

        [Fact]
        public void Constructor_WithRows_ShouldRepresentTransverseSections()
        {
            // Arrange - Rows represent transverse (side-to-side/beam) positions on the ship
            // A vessel with 13 rows means 13 containers can fit across the width
            var rows = new Rows(13);

            // Act & Assert
            rows.Value.Should().Be(13);
            rows.Value.Should().BeGreaterThan(0);
        }

        [Fact]
        public void Constructor_WithEvenNumberRows_ShouldBeValid()
        {
            // Arrange - Container ships can have even or odd number of rows
            var evenRows = 24;

            // Act
            var rows = new Rows(evenRows);

            // Assert
            rows.Value.Should().Be(24);
            (rows.Value % 2).Should().Be(0); // Even number
        }

        [Fact]
        public void Constructor_WithOddNumberRows_ShouldBeValid()
        {
            // Arrange - Container ships can have even or odd number of rows
            var oddRows = 13;

            // Act
            var rows = new Rows(oddRows);

            // Assert
            rows.Value.Should().Be(13);
            (rows.Value % 2).Should().Be(1); // Odd number
        }

        #endregion
    }
}
