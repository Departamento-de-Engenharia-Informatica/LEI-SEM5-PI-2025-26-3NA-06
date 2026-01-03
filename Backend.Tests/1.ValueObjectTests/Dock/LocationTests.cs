using FluentAssertions;
using ProjArqsi.Domain.DockAggregate;
using ProjArqsi.Domain.Shared;
using Xunit;

namespace ProjArqsi.Tests._1_ValueObjectTests.Dock
{
    public class LocationTests
    {
        #region Valid Creation Tests

        [Theory]
        [InlineData("North Terminal")]
        [InlineData("Berth 5, Section A")]
        [InlineData("Main Port Area")]
        [InlineData("L")]
        [InlineData("GPS: 41.1579° N, 8.6291° W")]
        public void CreateLocation_WithValidDescription_ShouldSucceed(string validDescription)
        {
            // Act
            var location = new Location(validDescription);

            // Assert
            location.Should().NotBeNull();
            location.Description.Should().Be(validDescription.Trim());
        }

        [Fact]
        public void CreateLocation_WithLeadingAndTrailingSpaces_ShouldTrimSpaces()
        {
            // Arrange
            var descriptionWithSpaces = "   North Terminal   ";

            // Act
            var location = new Location(descriptionWithSpaces);

            // Assert
            location.Description.Should().Be("North Terminal");
        }

        [Fact]
        public void CreateLocation_WithSpecialCharacters_ShouldSucceed()
        {
            // Arrange
            var descriptionWithSpecialChars = "Port-A, Section #1.2";

            // Act
            var location = new Location(descriptionWithSpecialChars);

            // Assert
            location.Description.Should().Be("Port-A, Section #1.2");
        }

        [Fact]
        public void CreateLocation_WithUnicodeCharacters_ShouldSucceed()
        {
            // Arrange
            var unicodeDescription = "Porto de São João da Caparica";

            // Act
            var location = new Location(unicodeDescription);

            // Assert
            location.Description.Should().Be("Porto de São João da Caparica");
        }

        [Fact]
        public void CreateLocation_WithNumbers_ShouldSucceed()
        {
            // Arrange
            var numericDescription = "12345";

            // Act
            var location = new Location(numericDescription);

            // Assert
            location.Description.Should().Be("12345");
        }

        [Fact]
        public void CreateLocation_WithLongDescription_ShouldSucceed()
        {
            // Arrange
            var longDescription = "North Terminal, Section A, Berth 12, Bay 5, located near the customs office on the eastern side of the port complex";

            // Act
            var location = new Location(longDescription);

            // Assert
            location.Description.Should().Be(longDescription);
        }

        [Fact]
        public void CreateLocation_WithCoordinates_ShouldSucceed()
        {
            // Arrange
            var coordinates = "41.1579°N, 8.6291°W";

            // Act
            var location = new Location(coordinates);

            // Assert
            location.Description.Should().Be(coordinates);
        }

        #endregion

        #region Invalid Creation Tests

        [Fact]
        public void CreateLocation_WithNull_ShouldThrowBusinessRuleValidationException()
        {
            // Arrange
            string? nullDescription = null;

            // Act
            Action act = () => new Location(nullDescription!);

            // Assert
            act.Should().Throw<BusinessRuleValidationException>()
                .WithMessage("*cannot be empty*");
        }

        [Fact]
        public void CreateLocation_WithEmptyString_ShouldThrowBusinessRuleValidationException()
        {
            // Arrange
            var emptyDescription = string.Empty;

            // Act
            Action act = () => new Location(emptyDescription);

            // Assert
            act.Should().Throw<BusinessRuleValidationException>()
                .WithMessage("*cannot be empty*");
        }

        [Theory]
        [InlineData(" ")]
        [InlineData("   ")]
        [InlineData("\t")]
        [InlineData("\n")]
        public void CreateLocation_WithWhitespaceOnly_ShouldThrowBusinessRuleValidationException(string whitespaceDescription)
        {
            // Act
            Action act = () => new Location(whitespaceDescription);

            // Assert
            act.Should().Throw<BusinessRuleValidationException>()
                .WithMessage("*cannot be empty*");
        }

        #endregion

        #region Equality Tests

        [Fact]
        public void Location_WithSameDescription_ShouldBeEqual()
        {
            // Arrange
            var location1 = new Location("North Terminal");
            var location2 = new Location("North Terminal");

            // Act & Assert
            location1.Should().Be(location2);
            location1.GetHashCode().Should().Be(location2.GetHashCode());
        }

        [Fact]
        public void Location_WithDifferentDescriptions_ShouldNotBeEqual()
        {
            // Arrange
            var location1 = new Location("North Terminal");
            var location2 = new Location("South Terminal");

            // Act & Assert
            location1.Should().NotBe(location2);
            location1.GetHashCode().Should().NotBe(location2.GetHashCode());
        }

        [Fact]
        public void Location_WithSameDescriptionAfterTrimming_ShouldBeEqual()
        {
            // Arrange
            var location1 = new Location("  North Terminal  ");
            var location2 = new Location("North Terminal");

            // Act & Assert
            location1.Should().Be(location2);
        }

        [Fact]
        public void Location_ComparedToNull_ShouldNotBeEqual()
        {
            // Arrange
            var location = new Location("North Terminal");

            // Act & Assert
            location.Should().NotBe(null);
        }

        [Fact]
        public void Location_CaseSensitiveComparison_ShouldNotBeEqualForDifferentCases()
        {
            // Arrange
            var location1 = new Location("North Terminal");
            var location2 = new Location("north terminal");

            // Act & Assert
            location1.Should().NotBe(location2);
        }

        #endregion

        #region Edge Case Tests

        [Fact]
        public void CreateLocation_WithSingleCharacter_ShouldSucceed()
        {
            // Arrange
            var singleChar = "A";

            // Act
            var location = new Location(singleChar);

            // Assert
            location.Description.Should().Be("A");
        }

        [Fact]
        public void CreateLocation_WithMultipleSpacesBetweenWords_ShouldPreserveInternalSpaces()
        {
            // Arrange
            var descriptionWithSpaces = "North    Terminal";

            // Act
            var location = new Location(descriptionWithSpaces);

            // Assert
            location.Description.Should().Be("North    Terminal");
        }

        [Fact]
        public void CreateLocation_WithMixedWhitespaceAtEnds_ShouldTrimAll()
        {
            // Arrange
            var mixedWhitespace = " \t North Terminal \n ";

            // Act
            var location = new Location(mixedWhitespace);

            // Assert
            location.Description.Should().Be("North Terminal");
        }

        [Fact]
        public void CreateLocation_WithNewlinesInDescription_ShouldPreserveNewlines()
        {
            // Arrange
            var descriptionWithNewlines = "North Terminal\nSection A\nBerth 12";

            // Act
            var location = new Location(descriptionWithNewlines);

            // Assert
            location.Description.Should().Be("North Terminal\nSection A\nBerth 12");
        }

        [Fact]
        public void CreateLocation_WithAddress_ShouldSucceed()
        {
            // Arrange
            var address = "123 Harbor Street, Port District, Maritime City";

            // Act
            var location = new Location(address);

            // Assert
            location.Description.Should().Be(address);
        }

        #endregion
    }
}
