using FluentAssertions;
using ProjArqsi.Domain.VesselAggregate;
using Xunit;

namespace ProjArqsi.Tests.ValueObjectTests.Vessel
{
    public class VesselNameTests
    {
        #region Valid VesselName Tests

        [Fact]
        public void Constructor_WithValidName_ShouldCreateVesselName()
        {
            // Arrange & Act
            var vesselName = new VesselName("Ever Given");

            // Assert
            vesselName.Name.Should().Be("Ever Given");
        }

        [Theory]
        [InlineData("MSC Oscar")]
        [InlineData("OOCL Hong Kong")]
        [InlineData("CMA CGM Antoine de Saint Exupéry")]
        [InlineData("Maersk Mc-Kinney Møller")]
        [InlineData("A")]
        public void Constructor_WithMultipleValidNames_ShouldCreateVesselName(string validName)
        {
            // Arrange & Act
            var vesselName = new VesselName(validName);

            // Assert
            vesselName.Name.Should().Be(validName);
        }

        [Fact]
        public void Constructor_WithNameContainingSpecialCharacters_ShouldCreateVesselName()
        {
            // Arrange & Act
            var vesselName = new VesselName("Queen Mary 2");

            // Assert
            vesselName.Name.Should().Be("Queen Mary 2");
        }

        [Fact]
        public void Constructor_WithNameContainingHyphens_ShouldCreateVesselName()
        {
            // Arrange & Act
            var vesselName = new VesselName("Maersk Mc-Kinney Møller");

            // Assert
            vesselName.Name.Should().Be("Maersk Mc-Kinney Møller");
        }

        [Fact]
        public void Constructor_WithUnicodeCharacters_ShouldCreateVesselName()
        {
            // Arrange & Act
            var vesselName = new VesselName("中远海运宇宙号"); // COSCO Shipping Universe

            // Assert
            vesselName.Name.Should().Be("中远海运宇宙号");
        }

        #endregion

        #region Invalid VesselName Tests

        [Fact]
        public void Constructor_WithNull_ShouldThrowArgumentException()
        {
            // Arrange & Act
            Action act = () => new VesselName(null);

            // Assert
            act.Should().Throw<ArgumentException>()
                .WithMessage("Vessel name cannot be empty*")
                .And.ParamName.Should().Be("name");
        }

        [Fact]
        public void Constructor_WithEmptyString_ShouldThrowArgumentException()
        {
            // Arrange & Act
            Action act = () => new VesselName("");

            // Assert
            act.Should().Throw<ArgumentException>()
                .WithMessage("Vessel name cannot be empty*")
                .And.ParamName.Should().Be("name");
        }

        [Fact]
        public void Constructor_WithWhitespace_ShouldThrowArgumentException()
        {
            // Arrange & Act
            Action act = () => new VesselName("   ");

            // Assert
            act.Should().Throw<ArgumentException>()
                .WithMessage("Vessel name cannot be empty*")
                .And.ParamName.Should().Be("name");
        }

        [Fact]
        public void Constructor_WithOnlyTabs_ShouldThrowArgumentException()
        {
            // Arrange & Act
            Action act = () => new VesselName("\t\t");

            // Assert
            act.Should().Throw<ArgumentException>()
                .WithMessage("Vessel name cannot be empty*");
        }

        [Fact]
        public void Constructor_WithOnlyNewlines_ShouldThrowArgumentException()
        {
            // Arrange & Act
            Action act = () => new VesselName("\n\r");

            // Assert
            act.Should().Throw<ArgumentException>()
                .WithMessage("Vessel name cannot be empty*");
        }

        #endregion

        #region ToString Tests

        [Fact]
        public void ToString_ShouldReturnVesselName()
        {
            // Arrange
            var vesselName = new VesselName("Ever Given");

            // Act
            var result = vesselName.ToString();

            // Assert
            result.Should().Be("Ever Given");
        }

        [Theory]
        [InlineData("MSC Oscar")]
        [InlineData("OOCL Hong Kong")]
        [InlineData("Maersk Mc-Kinney Møller")]
        public void ToString_WithMultipleNames_ShouldReturnCorrectName(string name)
        {
            // Arrange
            var vesselName = new VesselName(name);

            // Act
            var result = vesselName.ToString();

            // Assert
            result.Should().Be(name);
        }

        #endregion

        #region Equality Tests

        [Fact]
        public void Equals_WithSameNames_ShouldReturnTrue()
        {
            // Arrange
            var vesselName1 = new VesselName("Ever Given");
            var vesselName2 = new VesselName("Ever Given");

            // Act & Assert
            vesselName1.Equals(vesselName2).Should().BeTrue();
        }

        [Fact]
        public void Equals_WithDifferentNames_ShouldReturnFalse()
        {
            // Arrange
            var vesselName1 = new VesselName("Ever Given");
            var vesselName2 = new VesselName("MSC Oscar");

            // Act & Assert
            vesselName1.Equals(vesselName2).Should().BeFalse();
        }

        [Fact]
        public void Equals_IsCaseSensitive()
        {
            // Arrange
            var vesselName1 = new VesselName("Ever Given");
            var vesselName2 = new VesselName("ever given");

            // Act & Assert
            vesselName1.Equals(vesselName2).Should().BeFalse();
        }

        [Fact]
        public void GetHashCode_WithSameNames_ShouldReturnSameHashCode()
        {
            // Arrange
            var vesselName1 = new VesselName("Ever Given");
            var vesselName2 = new VesselName("Ever Given");

            // Act & Assert
            vesselName1.GetHashCode().Should().Be(vesselName2.GetHashCode());
        }

        [Fact]
        public void GetHashCode_WithDifferentNames_ShouldReturnDifferentHashCodes()
        {
            // Arrange
            var vesselName1 = new VesselName("Ever Given");
            var vesselName2 = new VesselName("MSC Oscar");

            // Act & Assert
            vesselName1.GetHashCode().Should().NotBe(vesselName2.GetHashCode());
        }

        #endregion

        #region Edge Cases

        [Fact]
        public void Constructor_WithSingleCharacter_ShouldCreateVesselName()
        {
            // Arrange & Act
            var vesselName = new VesselName("A");

            // Assert
            vesselName.Name.Should().Be("A");
        }

        [Fact]
        public void Constructor_WithVeryLongName_ShouldCreateVesselName()
        {
            // Arrange
            var longName = new string('A', 1000);

            // Act
            var vesselName = new VesselName(longName);

            // Assert
            vesselName.Name.Should().Be(longName);
            vesselName.Name.Length.Should().Be(1000);
        }

        [Fact]
        public void Name_Property_ShouldBeReadOnly()
        {
            // Arrange
            var vesselName = new VesselName("Ever Given");

            // Act - The Name property should not have a public setter
            var propertyInfo = typeof(VesselName).GetProperty(nameof(VesselName.Name));

            // Assert
            propertyInfo.Should().NotBeNull();
            propertyInfo.SetMethod.Should().NotBeNull();
            propertyInfo.SetMethod.IsPublic.Should().BeFalse();
        }

        [Fact]
        public void Constructor_ShouldNotTrimWhitespace()
        {
            // Arrange & Act
            var vesselName = new VesselName(" Ever Given ");

            // Assert
            vesselName.Name.Should().Be(" Ever Given ");
        }

        #endregion

        #region Real-World Vessel Names

        [Theory]
        [InlineData("Ever Given")] // Famous Suez Canal incident vessel
        [InlineData("MSC Gülsün")] // One of the world's largest container ships
        [InlineData("HMM Algeciras")] // Ultra-large container vessel
        [InlineData("OOCL Hong Kong")] // Large container ship
        [InlineData("CMA CGM Antoine de Saint Exupéry")] // Long vessel name
        [InlineData("Maersk Mc-Kinney Møller")] // Special characters and unicode
        [InlineData("COSCO Shipping Universe")] // Large Chinese vessel
        public void Constructor_WithRealWorldVesselNames_ShouldCreateValidVesselName(string realName)
        {
            // Arrange & Act
            var vesselName = new VesselName(realName);

            // Assert
            vesselName.Name.Should().Be(realName);
            vesselName.ToString().Should().Be(realName);
        }

        [Fact]
        public void Constructor_WithHistoricalVesselName_TitanicExample()
        {
            // Arrange
            var name = "RMS Titanic";

            // Act
            var vesselName = new VesselName(name);

            // Assert
            vesselName.Name.Should().Be("RMS Titanic");
        }

        [Fact]
        public void Constructor_WithModernMegaVessel_MSCGulsun()
        {
            // Arrange - MSC Gülsün has capacity of 23,756 TEU (one of the largest)
            var name = "MSC Gülsün";

            // Act
            var vesselName = new VesselName(name);

            // Assert
            vesselName.Name.Should().Be("MSC Gülsün");
            vesselName.Name.Should().Contain("ü"); // Contains unicode character
        }

        #endregion
    }
}
