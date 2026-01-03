using FluentAssertions;
using ProjArqsi.Domain.ContainerAggregate.ValueObjects;
using ProjArqsi.Domain.Shared;

namespace ProjArqsi.Tests._1_ValueObjectTests.Container;

/// <summary>
/// Level 1: Value Object Tests
/// SUT: ContainerDescription (Value Object)
/// Objective: Validate container description business rules (length, format, trimming)
/// </summary>
public class ContainerDescriptionTests
{
    #region Valid Descriptions

    [Theory]
    [InlineData("Standard dry container")]
    [InlineData("Refrigerated container for food transport")]
    [InlineData("40ft high cube container")]
    [InlineData("Contains electronics")]
    [InlineData("Mixed cargo")]
    public void CreateContainerDescription_WithValidDescription_ShouldSucceed(string validDescription)
    {
        // Act
        var description = new ContainerDescription(validDescription);

        // Assert
        description.Should().NotBeNull();
        description.Text.Should().Be(validDescription);
    }

    [Fact]
    public void CreateContainerDescription_WithSingleCharacter_ShouldSucceed()
    {
        // Arrange - No minimum length specified, so even 1 char should work
        var shortDescription = "A";

        // Act
        var description = new ContainerDescription(shortDescription);

        // Assert
        description.Text.Should().Be(shortDescription);
    }

    [Fact]
    public void CreateContainerDescription_WithMaximumLength_ShouldSucceed()
    {
        // Arrange - Exactly 500 characters (maximum)
        var maxDescription = new string('A', 500);

        // Act
        var description = new ContainerDescription(maxDescription);

        // Assert
        description.Text.Should().Be(maxDescription);
        description.Text.Length.Should().Be(500);
    }

    #endregion

    #region Null/Empty/Whitespace Tests

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("   ")]
    [InlineData("\t")]
    [InlineData("\n")]
    [InlineData("\r\n")]
    [InlineData("     \t\n     ")]
    [InlineData("          ")]
    public void CreateContainerDescription_WithNullOrWhitespace_ShouldThrowBusinessRuleValidationException(string? invalidDescription)
    {
        // Act
        Action act = () => new ContainerDescription(invalidDescription!);

        // Assert
        act.Should().Throw<BusinessRuleValidationException>()
            .WithMessage("*cannot be empty*");
    }

    [Fact]
    public void CreateContainerDescription_WithOnlySpacesThatTrimToEmpty_ShouldThrowBusinessRuleValidationException()
    {
        // Arrange - Multiple spaces that trim to empty
        var onlySpaces = "          ";

        // Act
        Action act = () => new ContainerDescription(onlySpaces);

        // Assert
        act.Should().Throw<BusinessRuleValidationException>()
            .WithMessage("*cannot be empty*");
    }

    #endregion

    #region Maximum Length Tests

    [Theory]
    [InlineData(501)]
    [InlineData(502)]
    [InlineData(600)]
    [InlineData(1000)]
    public void CreateContainerDescription_ExceedingMaximumLength_ShouldThrowBusinessRuleValidationException(int length)
    {
        // Arrange
        var tooLongDescription = new string('X', length);

        // Act
        Action act = () => new ContainerDescription(tooLongDescription);

        // Assert
        act.Should().Throw<BusinessRuleValidationException>()
            .WithMessage("*cannot exceed 500 characters*");
    }

    [Fact]
    public void CreateContainerDescription_WithSpacesThatExceedMax_ShouldThrowBusinessRuleValidationException()
    {
        // Arrange - 501 characters after removing leading/trailing spaces
        var tooLongDescription = new string('A', 501);

        // Act
        Action act = () => new ContainerDescription(tooLongDescription);

        // Assert
        act.Should().Throw<BusinessRuleValidationException>()
            .WithMessage("*cannot exceed 500 characters*");
    }

    [Fact]
    public void CreateContainerDescription_ExactlyAtMaxBoundary_ShouldSucceed()
    {
        // Arrange - Exactly 500 characters
        var maxDescription = new string('X', 500);

        // Act
        var description = new ContainerDescription(maxDescription);

        // Assert
        description.Text.Length.Should().Be(500);
    }

    [Fact]
    public void CreateContainerDescription_JustOverMaxBoundary_ShouldFail()
    {
        // Arrange - 501 characters (just over maximum)
        var tooLong = new string('X', 501);
        var act = () => new ContainerDescription(tooLong);

        // Assert
        act.Should().Throw<BusinessRuleValidationException>()
            .WithMessage("*cannot exceed 500 characters*");
    }

    #endregion

    #region Trimming Tests

    [Theory]
    [InlineData("  Standard container  ", "Standard container")]
    [InlineData("Refrigerated container   ", "Refrigerated container")]
    [InlineData("   High cube", "High cube")]
    [InlineData("\tContains goods\t", "Contains goods")]
    [InlineData("  \n  Mixed cargo  \r\n  ", "Mixed cargo")]
    public void CreateContainerDescription_WithLeadingOrTrailingWhitespace_ShouldTrim(string input, string expected)
    {
        // Act
        var description = new ContainerDescription(input);

        // Assert
        description.Text.Should().Be(expected);
    }

    [Fact]
    public void CreateContainerDescription_WithInternalSpaces_ShouldPreserveThem()
    {
        // Arrange
        var descriptionWithSpaces = "40ft high cube container";

        // Act
        var description = new ContainerDescription(descriptionWithSpaces);

        // Assert
        description.Text.Should().Be("40ft high cube container");
        description.Text.Should().Contain(" ");
    }

    [Fact]
    public void CreateContainerDescription_WithMultipleInternalSpaces_ShouldPreserveThem()
    {
        // Arrange
        var descriptionWithSpaces = "Heavy    machinery    inside";

        // Act
        var description = new ContainerDescription(descriptionWithSpaces);

        // Assert
        description.Text.Should().Be("Heavy    machinery    inside");
    }

    [Fact]
    public void CreateContainerDescription_WithInternalLineBreaks_ShouldPreserveThem()
    {
        // Arrange - Line breaks in the middle should be preserved
        var descriptionWithLineBreaks = "Line 1\nLine 2\nLine 3";

        // Act
        var description = new ContainerDescription(descriptionWithLineBreaks);

        // Assert
        description.Text.Should().Be("Line 1\nLine 2\nLine 3");
        description.Text.Should().Contain("\n");
    }

    #endregion

    #region Equality Tests

    [Fact]
    public void ContainerDescription_WithSameValue_ShouldBeEqual()
    {
        // Arrange
        var desc1 = new ContainerDescription("Standard container");
        var desc2 = new ContainerDescription("Standard container");

        // Act & Assert
        desc1.Should().Be(desc2);
        desc1.GetHashCode().Should().Be(desc2.GetHashCode());
    }

    [Fact]
    public void ContainerDescription_AfterTrimming_WithSameValue_ShouldBeEqual()
    {
        // Arrange
        var desc1 = new ContainerDescription("Standard container");
        var desc2 = new ContainerDescription("  Standard container  ");

        // Act & Assert
        desc1.Should().Be(desc2);
        desc1.GetHashCode().Should().Be(desc2.GetHashCode());
    }

    [Fact]
    public void ContainerDescription_WithDifferentValues_ShouldNotBeEqual()
    {
        // Arrange
        var desc1 = new ContainerDescription("Standard container");
        var desc2 = new ContainerDescription("Refrigerated container");

        // Act & Assert
        desc1.Should().NotBe(desc2);
    }

    [Fact]
    public void ContainerDescription_CaseSensitivity_ShouldMatter()
    {
        // Arrange
        var desc1 = new ContainerDescription("Standard Container");
        var desc2 = new ContainerDescription("standard container");

        // Act & Assert
        desc1.Should().NotBe(desc2);
    }

    #endregion

    #region ToString Tests

    [Fact]
    public void ContainerDescription_ToString_ShouldReturnText()
    {
        // Arrange
        var description = new ContainerDescription("Standard container");

        // Act
        var result = description.ToString();

        // Assert
        result.Should().Be("Standard container");
    }

    [Fact]
    public void ContainerDescription_ToString_AfterTrimming_ShouldReturnTrimmedText()
    {
        // Arrange
        var description = new ContainerDescription("  Refrigerated container  ");

        // Act
        var result = description.ToString();

        // Assert
        result.Should().Be("Refrigerated container");
    }

    #endregion

    #region Special Characters and Unicode

    [Theory]
    [InlineData("Container with special chars: @#$%^&*()")] 
    [InlineData("Français: conteneur réfrigéré")] // French
    [InlineData("Español: contenedor estándar")] // Spanish
    [InlineData("中文描述：标准集装箱")] // Chinese
    [InlineData("Русский: стандартный контейнер")] // Russian
    [InlineData("日本語：標準コンテナ")] // Japanese
    [InlineData("Container #123-ABC")] // With numbers and hyphens
    [InlineData("Container @ 50% capacity")] // With special symbols
    [InlineData("Temperature: -20°C to +20°C")] // With degrees symbol
    public void CreateContainerDescription_WithSpecialCharacters_ShouldSucceed(string descriptionWithSpecialChars)
    {
        // Act
        var description = new ContainerDescription(descriptionWithSpecialChars);

        // Assert
        description.Text.Should().Be(descriptionWithSpecialChars);
    }

    [Fact]
    public void ContainerDescription_WithOnlyNumbers_ShouldSucceed()
    {
        // Arrange
        var numericDescription = "1234567890";

        // Act
        var description = new ContainerDescription(numericDescription);

        // Assert
        description.Text.Should().Be("1234567890");
    }

    [Fact]
    public void ContainerDescription_WithEmojis_ShouldWork()
    {
        // Arrange
        var emojiDescription = "Container with food 🍕🍔🍟";

        // Act
        var description = new ContainerDescription(emojiDescription);

        // Assert
        description.Text.Should().Be("Container with food 🍕🍔🍟");
    }

    [Fact]
    public void ContainerDescription_WithQuotes_ShouldPreserveThem()
    {
        // Arrange
        var descWithQuotes = "Container marked as \"fragile\"";

        // Act
        var description = new ContainerDescription(descWithQuotes);

        // Assert
        description.Text.Should().Contain("\"");
        description.Text.Should().Be("Container marked as \"fragile\"");
    }

    #endregion

    #region Long Text and Formatting

    [Fact]
    public void ContainerDescription_WithLongDetailedText_ShouldSucceed()
    {
        // Arrange - A realistic long description
        var longDescription = 
            "Standard 40ft high cube dry container. " +
            "Loaded with automotive spare parts including engines, transmissions, and electronic components. " +
            "Temperature-controlled during transport. " +
            "Requires special handling due to fragile items. " +
            "Destination: Port of Rotterdam. " +
            "Estimated delivery: 15 days. " +
            "Total weight: 25,000 kg.";

        // Act
        var description = new ContainerDescription(longDescription);

        // Assert
        description.Text.Should().Be(longDescription);
        description.Text.Length.Should().BeLessThan(500);
    }

    [Fact]
    public void ContainerDescription_WithMultilineText_ShouldPreserveFormatting()
    {
        // Arrange
        var multilineDescription = 
            "Container Details:\n" +
            "- Type: Refrigerated\n" +
            "- Size: 40ft\n" +
            "- Cargo: Food products\n" +
            "- Temperature: -18°C";

        // Act
        var description = new ContainerDescription(multilineDescription);

        // Assert
        description.Text.Should().Be(multilineDescription);
        description.Text.Should().Contain("\n");
    }

    [Fact]
    public void ContainerDescription_WithTabsAndNewlines_ShouldPreserveInternal()
    {
        // Arrange - Internal tabs/newlines preserved, external trimmed
        var formattedDescription = "\tColumn1\tColumn2\nRow1\tData1\tData2\n";

        // Act
        var description = new ContainerDescription(formattedDescription);

        // Assert
        description.Text.Should().Be("Column1\tColumn2\nRow1\tData1\tData2");
        description.Text.Should().Contain("\t");
        description.Text.Should().Contain("\n");
    }

    #endregion

    #region Edge Cases

    [Fact]
    public void ContainerDescription_WithOnlySpecialCharacters_ShouldSucceed()
    {
        // Arrange
        var specialCharsOnly = "@#$%^&*()";

        // Act
        var description = new ContainerDescription(specialCharsOnly);

        // Assert
        description.Text.Should().Be("@#$%^&*()");
    }

    [Fact]
    public void ContainerDescription_WithRepeatingPattern_ShouldSucceed()
    {
        // Arrange - 200 characters of repeating pattern
        var repeatingPattern = string.Concat(Enumerable.Repeat("ABC ", 50));

        // Act
        var description = new ContainerDescription(repeatingPattern);

        // Assert
        description.Text.Should().Be(repeatingPattern.Trim());
    }

    [Fact]
    public void ContainerDescription_WithMixedWhitespaceAtEdges_ShouldTrimAll()
    {
        // Arrange
        var mixedWhitespace = "  \t\n\r  Container description  \r\n\t  ";

        // Act
        var description = new ContainerDescription(mixedWhitespace);

        // Assert
        description.Text.Should().Be("Container description");
        description.Text.Should().NotStartWith(" ");
        description.Text.Should().NotEndWith(" ");
    }

    [Fact]
    public void ContainerDescription_ExactlyAtBoundary_499Chars_ShouldSucceed()
    {
        // Arrange - 499 characters (just below maximum)
        var almostMax = new string('X', 499);

        // Act
        var description = new ContainerDescription(almostMax);

        // Assert
        description.Text.Length.Should().Be(499);
    }

    [Fact]
    public void ContainerDescription_WithURLs_ShouldSucceed()
    {
        // Arrange
        var descWithUrl = "Documentation available at: https://example.com/container/details";

        // Act
        var description = new ContainerDescription(descWithUrl);

        // Assert
        description.Text.Should().Be(descWithUrl);
        description.Text.Should().Contain("https://");
    }

    [Fact]
    public void ContainerDescription_WithJSONFormat_ShouldSucceed()
    {
        // Arrange
        var jsonDescription = "{\"type\": \"standard\", \"size\": \"40ft\", \"cargo\": \"electronics\"}";

        // Act
        var description = new ContainerDescription(jsonDescription);

        // Assert
        description.Text.Should().Be(jsonDescription);
    }

    #endregion
}
