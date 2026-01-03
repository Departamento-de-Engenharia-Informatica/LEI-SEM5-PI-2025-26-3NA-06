using FluentAssertions;
using ProjArqsi.Domain.VesselTypeAggregate;
using Xunit;

namespace ProjArqsi.Tests.ValueObjectTests.VesselType
{
    public class TypeDescriptionTests
    {
        #region Valid TypeDescription Tests

        [Fact]
        public void Constructor_WithValidDescription_ShouldCreateTypeDescription()
        {
            // Arrange & Act
            var description = new TypeDescription("Large container vessels designed for transoceanic routes");

            // Assert
            description.Value.Should().Be("Large container vessels designed for transoceanic routes");
        }

        [Theory]
        [InlineData("Vessels limited by Panama Canal dimensions")]
        [InlineData("Ultra-large container vessels exceeding 14,500 TEU capacity")]
        [InlineData("Small feeder vessels for regional port distribution")]
        [InlineData("General cargo vessel")]
        [InlineData("A")]
        public void Constructor_WithMultipleValidDescriptions_ShouldCreateTypeDescription(string validDescription)
        {
            // Arrange & Act
            var description = new TypeDescription(validDescription);

            // Assert
            description.Value.Should().Be(validDescription);
        }

        [Fact]
        public void Constructor_WithLongDescription_ShouldCreateTypeDescription()
        {
            // Arrange
            var longDesc = "Ultra Large Container Vessels (ULCV) are the largest container ships in operation, " +
                          "with capacities exceeding 14,500 TEU. These vessels are designed for major trade routes " +
                          "between Asia, Europe, and North America, offering significant economies of scale.";

            // Act
            var description = new TypeDescription(longDesc);

            // Assert
            description.Value.Should().Be(longDesc);
            description.Value.Length.Should().BeGreaterThan(200);
        }

        [Fact]
        public void Constructor_WithSpecialCharacters_ShouldCreateTypeDescription()
        {
            // Arrange & Act
            var description = new TypeDescription("Vessels: Capacity 10,000-15,000 TEU (max draft 14.5m)");

            // Assert
            description.Value.Should().Be("Vessels: Capacity 10,000-15,000 TEU (max draft 14.5m)");
        }

        #endregion

        #region Invalid TypeDescription Tests

        [Fact]
        public void Constructor_WithNull_ShouldThrowArgumentException()
        {
            // Arrange & Act
            Action act = () => new TypeDescription(null);

            // Assert
            act.Should().Throw<ArgumentException>()
                .WithMessage("Type description cannot be empty*")
                .And.ParamName.Should().Be("value");
        }

        [Fact]
        public void Constructor_WithEmptyString_ShouldThrowArgumentException()
        {
            // Arrange & Act
            Action act = () => new TypeDescription("");

            // Assert
            act.Should().Throw<ArgumentException>()
                .WithMessage("Type description cannot be empty*")
                .And.ParamName.Should().Be("value");
        }

        [Fact]
        public void Constructor_WithWhitespace_ShouldThrowArgumentException()
        {
            // Arrange & Act
            Action act = () => new TypeDescription("   ");

            // Assert
            act.Should().Throw<ArgumentException>()
                .WithMessage("Type description cannot be empty*")
                .And.ParamName.Should().Be("value");
        }

        [Fact]
        public void Constructor_WithOnlyNewlines_ShouldThrowArgumentException()
        {
            // Arrange & Act
            Action act = () => new TypeDescription("\n\r\n");

            // Assert
            act.Should().Throw<ArgumentException>()
                .WithMessage("Type description cannot be empty*");
        }

        #endregion

        #region ToString Tests

        [Fact]
        public void ToString_ShouldReturnDescription()
        {
            // Arrange
            var description = new TypeDescription("Panamax container vessels");

            // Act
            var result = description.ToString();

            // Assert
            result.Should().Be("Panamax container vessels");
        }

        [Theory]
        [InlineData("Large vessels")]
        [InlineData("Small feeder ships")]
        [InlineData("Ultra-large container carriers")]
        public void ToString_WithMultipleDescriptions_ShouldReturnCorrectDescription(string desc)
        {
            // Arrange
            var description = new TypeDescription(desc);

            // Act
            var result = description.ToString();

            // Assert
            result.Should().Be(desc);
        }

        #endregion

        #region Equality Tests

        [Fact]
        public void Equals_WithSameDescriptions_ShouldReturnTrue()
        {
            // Arrange
            var description1 = new TypeDescription("Panamax vessels");
            var description2 = new TypeDescription("Panamax vessels");

            // Act & Assert
            description1.Equals(description2).Should().BeTrue();
        }

        [Fact]
        public void Equals_WithDifferentDescriptions_ShouldReturnFalse()
        {
            // Arrange
            var description1 = new TypeDescription("Panamax vessels");
            var description2 = new TypeDescription("Post-Panamax vessels");

            // Act & Assert
            description1.Equals(description2).Should().BeFalse();
        }

        [Fact]
        public void Equals_IsCaseSensitive()
        {
            // Arrange
            var description1 = new TypeDescription("Panamax vessels");
            var description2 = new TypeDescription("panamax vessels");

            // Act & Assert
            description1.Equals(description2).Should().BeFalse();
        }

        [Fact]
        public void GetHashCode_WithSameDescriptions_ShouldReturnSameHashCode()
        {
            // Arrange
            var description1 = new TypeDescription("Panamax vessels");
            var description2 = new TypeDescription("Panamax vessels");

            // Act & Assert
            description1.GetHashCode().Should().Be(description2.GetHashCode());
        }

        [Fact]
        public void GetHashCode_WithDifferentDescriptions_ShouldReturnDifferentHashCodes()
        {
            // Arrange
            var description1 = new TypeDescription("Panamax vessels");
            var description2 = new TypeDescription("Post-Panamax vessels");

            // Act & Assert
            description1.GetHashCode().Should().NotBe(description2.GetHashCode());
        }

        #endregion

        #region Edge Cases

        [Fact]
        public void Value_Property_ShouldBeReadOnly()
        {
            // Arrange
            var description = new TypeDescription("Test description");

            // Act - The Value property should not have a public setter
            var propertyInfo = typeof(TypeDescription).GetProperty(nameof(TypeDescription.Value));

            // Assert
            propertyInfo.Should().NotBeNull();
            propertyInfo.SetMethod.Should().NotBeNull();
            propertyInfo.SetMethod.IsPublic.Should().BeFalse();
        }

        [Fact]
        public void Constructor_WithSingleCharacter_ShouldCreateTypeDescription()
        {
            // Arrange & Act
            var description = new TypeDescription("A");

            // Assert
            description.Value.Should().Be("A");
        }

        [Fact]
        public void Constructor_WithVeryLongDescription_ShouldCreateTypeDescription()
        {
            // Arrange
            var longDescription = new string('A', 2000);

            // Act
            var description = new TypeDescription(longDescription);

            // Assert
            description.Value.Should().Be(longDescription);
            description.Value.Length.Should().Be(2000);
        }

        [Fact]
        public void Constructor_ShouldNotTrimWhitespace()
        {
            // Arrange & Act
            var description = new TypeDescription(" Test description ");

            // Assert
            description.Value.Should().Be(" Test description ");
        }

        #endregion

        #region Real-World Vessel Type Descriptions

        [Theory]
        [InlineData("Vessels designed to fit through the original Panama Canal locks (maximum 32.3m beam, 294m length)")]
        [InlineData("Container ships exceeding Panamax dimensions but smaller than ULCV classification")]
        [InlineData("Vessels designed for the expanded Panama Canal (maximum 49m beam, 366m length, 15.2m draft)")]
        [InlineData("Ultra Large Container Vessels with capacity exceeding 14,500 TEU")]
        [InlineData("Small coastal and regional feeder vessels for port-to-port cargo distribution")]
        public void Constructor_WithRealWorldDescriptions_ShouldCreateValidTypeDescription(string realDescription)
        {
            // Arrange & Act
            var description = new TypeDescription(realDescription);

            // Assert
            description.Value.Should().Be(realDescription);
            description.ToString().Should().Be(realDescription);
        }

        [Fact]
        public void Constructor_WithPanamaxDescription_ShouldDescribeConstraints()
        {
            // Arrange
            var panamaxDesc = "Container vessels designed to pass through the original Panama Canal locks, " +
                            "limited to 32.3m beam width, 294m length, and 13 container rows across";

            // Act
            var description = new TypeDescription(panamaxDesc);

            // Assert
            description.Value.Should().Contain("Panama Canal");
            description.Value.Should().Contain("32.3m");
            description.Value.Should().Contain("294m");
        }

        [Fact]
        public void Constructor_WithULCVDescription_ShouldDescribeLargestVessels()
        {
            // Arrange
            var ulcvDesc = "Ultra Large Container Vessels representing the largest container ships, " +
                          "with capacities exceeding 14,500 TEU and up to 24,000 TEU for the largest vessels";

            // Act
            var description = new TypeDescription(ulcvDesc);

            // Assert
            description.Value.Should().Contain("Ultra Large");
            description.Value.Should().Contain("14,500 TEU");
        }

        [Fact]
        public void Constructor_WithFeederDescription_ShouldDescribeSmallVessels()
        {
            // Arrange
            var feederDesc = "Feeder vessels are small container ships (500-3,000 TEU) used for regional " +
                           "distribution between major ports and smaller regional ports";

            // Act
            var description = new TypeDescription(feederDesc);

            // Assert
            description.Value.Should().Contain("Feeder");
            description.Value.Should().Contain("500-3,000 TEU");
            description.Value.Should().Contain("regional");
        }

        #endregion
    }
}
