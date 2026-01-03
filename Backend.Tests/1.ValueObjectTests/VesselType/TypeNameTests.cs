using FluentAssertions;
using ProjArqsi.Domain.VesselTypeAggregate;
using Xunit;

namespace ProjArqsi.Tests.ValueObjectTests.VesselType
{
    public class TypeNameTests
    {
        #region Valid TypeName Tests

        [Fact]
        public void Constructor_WithValidName_ShouldCreateTypeName()
        {
            // Arrange & Act
            var typeName = new TypeName("Container Carrier");

            // Assert
            typeName.Value.Should().Be("Container Carrier");
        }

        [Theory]
        [InlineData("Panamax")]
        [InlineData("Post-Panamax")]
        [InlineData("Neopanamax")]
        [InlineData("ULCV")]
        [InlineData("Feeder")]
        public void Constructor_WithMultipleValidNames_ShouldCreateTypeName(string validName)
        {
            // Arrange & Act
            var typeName = new TypeName(validName);

            // Assert
            typeName.Value.Should().Be(validName);
        }

        [Fact]
        public void Constructor_WithSingleCharacter_ShouldCreateTypeName()
        {
            // Arrange & Act
            var typeName = new TypeName("A");

            // Assert
            typeName.Value.Should().Be("A");
        }

        [Fact]
        public void Constructor_WithLongName_ShouldCreateTypeName()
        {
            // Arrange & Act
            var typeName = new TypeName("Ultra Large Container Vessel Type A");

            // Assert
            typeName.Value.Should().Be("Ultra Large Container Vessel Type A");
        }

        #endregion

        #region Invalid TypeName Tests

        [Fact]
        public void Constructor_WithNull_ShouldThrowArgumentException()
        {
            // Arrange & Act
            Action act = () => new TypeName(null);

            // Assert
            act.Should().Throw<ArgumentException>()
                .WithMessage("Type name cannot be empty*")
                .And.ParamName.Should().Be("value");
        }

        [Fact]
        public void Constructor_WithEmptyString_ShouldThrowArgumentException()
        {
            // Arrange & Act
            Action act = () => new TypeName("");

            // Assert
            act.Should().Throw<ArgumentException>()
                .WithMessage("Type name cannot be empty*")
                .And.ParamName.Should().Be("value");
        }

        [Fact]
        public void Constructor_WithWhitespace_ShouldThrowArgumentException()
        {
            // Arrange & Act
            Action act = () => new TypeName("   ");

            // Assert
            act.Should().Throw<ArgumentException>()
                .WithMessage("Type name cannot be empty*")
                .And.ParamName.Should().Be("value");
        }

        [Fact]
        public void Constructor_WithOnlyTabs_ShouldThrowArgumentException()
        {
            // Arrange & Act
            Action act = () => new TypeName("\t\t");

            // Assert
            act.Should().Throw<ArgumentException>()
                .WithMessage("Type name cannot be empty*");
        }

        #endregion

        #region ToString Tests

        [Fact]
        public void ToString_ShouldReturnTypeName()
        {
            // Arrange
            var typeName = new TypeName("Panamax");

            // Act
            var result = typeName.ToString();

            // Assert
            result.Should().Be("Panamax");
        }

        [Theory]
        [InlineData("ULCV")]
        [InlineData("Feeder")]
        [InlineData("Post-Panamax")]
        public void ToString_WithMultipleNames_ShouldReturnCorrectName(string name)
        {
            // Arrange
            var typeName = new TypeName(name);

            // Act
            var result = typeName.ToString();

            // Assert
            result.Should().Be(name);
        }

        #endregion

        #region Equality Tests

        [Fact]
        public void Equals_WithSameNames_ShouldReturnTrue()
        {
            // Arrange
            var typeName1 = new TypeName("Panamax");
            var typeName2 = new TypeName("Panamax");

            // Act & Assert
            typeName1.Equals(typeName2).Should().BeTrue();
        }

        [Fact]
        public void Equals_WithDifferentNames_ShouldReturnFalse()
        {
            // Arrange
            var typeName1 = new TypeName("Panamax");
            var typeName2 = new TypeName("Post-Panamax");

            // Act & Assert
            typeName1.Equals(typeName2).Should().BeFalse();
        }

        [Fact]
        public void Equals_IsCaseSensitive()
        {
            // Arrange
            var typeName1 = new TypeName("Panamax");
            var typeName2 = new TypeName("panamax");

            // Act & Assert
            typeName1.Equals(typeName2).Should().BeFalse();
        }

        [Fact]
        public void GetHashCode_WithSameNames_ShouldReturnSameHashCode()
        {
            // Arrange
            var typeName1 = new TypeName("Panamax");
            var typeName2 = new TypeName("Panamax");

            // Act & Assert
            typeName1.GetHashCode().Should().Be(typeName2.GetHashCode());
        }

        [Fact]
        public void GetHashCode_WithDifferentNames_ShouldReturnDifferentHashCodes()
        {
            // Arrange
            var typeName1 = new TypeName("Panamax");
            var typeName2 = new TypeName("Post-Panamax");

            // Act & Assert
            typeName1.GetHashCode().Should().NotBe(typeName2.GetHashCode());
        }

        #endregion

        #region Edge Cases

        [Fact]
        public void Value_Property_ShouldBeReadOnly()
        {
            // Arrange
            var typeName = new TypeName("Panamax");

            // Act - The Value property should not have a public setter
            var propertyInfo = typeof(TypeName).GetProperty(nameof(TypeName.Value));

            // Assert
            propertyInfo.Should().NotBeNull();
            propertyInfo.SetMethod.Should().NotBeNull();
            propertyInfo.SetMethod.IsPublic.Should().BeFalse();
        }

        [Fact]
        public void Constructor_WithVeryLongName_ShouldCreateTypeName()
        {
            // Arrange
            var longName = new string('A', 500);

            // Act
            var typeName = new TypeName(longName);

            // Assert
            typeName.Value.Should().Be(longName);
            typeName.Value.Length.Should().Be(500);
        }

        [Fact]
        public void Constructor_ShouldNotTrimWhitespace()
        {
            // Arrange & Act
            var typeName = new TypeName(" Panamax ");

            // Assert
            typeName.Value.Should().Be(" Panamax ");
        }

        #endregion

        #region Real-World Vessel Type Names

        [Theory]
        [InlineData("Panamax")] // Classic Panama Canal constraint (13 rows, ~5,000 TEU)
        [InlineData("Post-Panamax")] // Exceeds Panamax dimensions
        [InlineData("Neopanamax")] // Expanded Panama Canal (up to 366m length)
        [InlineData("ULCV")] // Ultra Large Container Vessel (14,500+ TEU)
        [InlineData("Megamax")] // Largest class (23,000+ TEU)
        [InlineData("Feeder")] // Small port-to-port vessels (500-3,000 TEU)
        [InlineData("Handy")] // Small general cargo (10,000-35,000 DWT)
        [InlineData("Handymax")] // Medium bulk carrier (35,000-60,000 DWT)
        public void Constructor_WithRealWorldTypeNames_ShouldCreateValidTypeName(string realName)
        {
            // Arrange & Act
            var typeName = new TypeName(realName);

            // Assert
            typeName.Value.Should().Be(realName);
            typeName.ToString().Should().Be(realName);
        }

        [Fact]
        public void Constructor_WithPanamaxType_ShouldRepresentClassicConstraint()
        {
            // Arrange - Panamax vessels designed for original Panama Canal
            var name = "Panamax";

            // Act
            var typeName = new TypeName(name);

            // Assert
            typeName.Value.Should().Be("Panamax");
        }

        [Fact]
        public void Constructor_WithULCVType_ShouldRepresentLargestVessels()
        {
            // Arrange - ULCV for vessels over 14,500 TEU
            var name = "ULCV";

            // Act
            var typeName = new TypeName(name);

            // Assert
            typeName.Value.Should().Be("ULCV");
        }

        [Fact]
        public void Constructor_WithFeederType_ShouldRepresentSmallVessels()
        {
            // Arrange - Feeder vessels for port-to-port distribution
            var name = "Feeder";

            // Act
            var typeName = new TypeName(name);

            // Assert
            typeName.Value.Should().Be("Feeder");
        }

        #endregion
    }
}
