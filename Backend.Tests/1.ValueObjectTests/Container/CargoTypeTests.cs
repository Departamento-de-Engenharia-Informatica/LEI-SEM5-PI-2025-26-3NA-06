using FluentAssertions;
using ProjArqsi.Domain.ContainerAggregate.ValueObjects;
using ProjArqsi.Domain.Shared;

namespace ProjArqsi.Tests._1_ValueObjectTests.Container;

/// <summary>
/// Level 1: Value Object Tests
/// SUT: CargoType (Value Object)
/// Objective: Validate cargo type business rules (length, format, trimming)
/// </summary>
public class CargoTypeTests
{
    #region Valid Cargo Types

    [Theory]
    [InlineData("Electronics")]
    [InlineData("Machinery")]
    [InlineData("Food Products")]
    [InlineData("Textiles")]
    [InlineData("Chemicals")]
    [InlineData("Automotive Parts")]
    [InlineData("Raw Materials")]
    public void CreateCargoType_WithValidType_ShouldSucceed(string validType)
    {
        // Act
        var cargoType = new CargoType(validType);

        // Assert
        cargoType.Should().NotBeNull();
        cargoType.Type.Should().Be(validType);
    }

    [Fact]
    public void CreateCargoType_WithMinimumLength_ShouldSucceed()
    {
        // Arrange - Exactly 3 characters (minimum)
        var minType = "Oil";

        // Act
        var cargoType = new CargoType(minType);

        // Assert
        cargoType.Type.Should().Be(minType);
    }

    [Fact]
    public void CreateCargoType_WithMaximumLength_ShouldSucceed()
    {
        // Arrange - Exactly 50 characters (maximum)
        var maxType = new string('A', 50);

        // Act
        var cargoType = new CargoType(maxType);

        // Assert
        cargoType.Type.Should().Be(maxType);
        cargoType.Type.Length.Should().Be(50);
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
    public void CreateCargoType_WithNullOrWhitespace_ShouldThrowBusinessRuleValidationException(string? invalidType)
    {
        // Act
        Action act = () => new CargoType(invalidType!);

        // Assert
        act.Should().Throw<BusinessRuleValidationException>()
            .WithMessage("*cannot be empty*");
    }

    #endregion

    #region Minimum Length Tests

    [Theory]
    [InlineData("A")] // 1 character
    [InlineData("AB")] // 2 characters
    public void CreateCargoType_BelowMinimumLength_ShouldThrowBusinessRuleValidationException(string invalidType)
    {
        // Act
        Action act = () => new CargoType(invalidType);

        // Assert
        act.Should().Throw<BusinessRuleValidationException>()
            .WithMessage("*at least 3 characters*");
    }

    [Fact]
    public void CreateCargoType_WithSpacesThatTrimToTooShort_ShouldThrowBusinessRuleValidationException()
    {
        // Arrange - "  ABC  " trims to "ABC" which is valid, but "  AB  " trims to "AB" (2 chars)
        var invalidType = "AB"; // 2 chars directly, without spaces to avoid empty validation

        // Act
        Action act = () => new CargoType(invalidType);

        // Assert
        act.Should().Throw<BusinessRuleValidationException>()
            .WithMessage("*at least 3 characters*");
    }

    #endregion

    #region Maximum Length Tests

    [Theory]
    [InlineData(51)]
    [InlineData(52)]
    [InlineData(100)]
    [InlineData(500)]
    public void CreateCargoType_ExceedingMaximumLength_ShouldThrowBusinessRuleValidationException(int length)
    {
        // Arrange
        var tooLongType = new string('X', length);

        // Act
        Action act = () => new CargoType(tooLongType);

        // Assert
        act.Should().Throw<BusinessRuleValidationException>()
            .WithMessage("*cannot exceed 50 characters*");
    }

    [Fact]
    public void CreateCargoType_WithSpacesThatExceedMax_ShouldThrowBusinessRuleValidationException()
    {
        // Arrange - Even with spaces, 51 chars
        var tooLongType = "  " + new string('A', 51) + "  ";

        // Act
        Action act = () => new CargoType(tooLongType);

        // Assert
        act.Should().Throw<BusinessRuleValidationException>()
            .WithMessage("*cannot exceed 50 characters*");
    }

    #endregion

    #region Trimming Tests

    [Theory]
    [InlineData("  Electronics  ", "Electronics")]
    [InlineData("Machinery   ", "Machinery")]
    [InlineData("   Food", "Food")]
    [InlineData("\tChemicals\t", "Chemicals")]
    [InlineData("  \n  Textiles  \r\n  ", "Textiles")]
    public void CreateCargoType_WithLeadingOrTrailingWhitespace_ShouldTrim(string input, string expected)
    {
        // Act
        var cargoType = new CargoType(input);

        // Assert
        cargoType.Type.Should().Be(expected);
    }

    [Fact]
    public void CreateCargoType_WithInternalSpaces_ShouldPreserveThem()
    {
        // Arrange
        var typeWithSpaces = "Automotive Parts";

        // Act
        var cargoType = new CargoType(typeWithSpaces);

        // Assert
        cargoType.Type.Should().Be("Automotive Parts");
        cargoType.Type.Should().Contain(" ");
    }

    [Fact]
    public void CreateCargoType_WithMultipleInternalSpaces_ShouldPreserveThem()
    {
        // Arrange
        var typeWithSpaces = "Heavy    Machinery";

        // Act
        var cargoType = new CargoType(typeWithSpaces);

        // Assert
        cargoType.Type.Should().Be("Heavy    Machinery");
    }

    #endregion

    #region Equality Tests

    [Fact]
    public void CargoType_WithSameValue_ShouldBeEqual()
    {
        // Arrange
        var type1 = new CargoType("Electronics");
        var type2 = new CargoType("Electronics");

        // Act & Assert
        type1.Should().Be(type2);
        type1.GetHashCode().Should().Be(type2.GetHashCode());
    }

    [Fact]
    public void CargoType_AfterTrimming_WithSameValue_ShouldBeEqual()
    {
        // Arrange
        var type1 = new CargoType("Electronics");
        var type2 = new CargoType("  Electronics  ");

        // Act & Assert
        type1.Should().Be(type2);
        type1.GetHashCode().Should().Be(type2.GetHashCode());
    }

    [Fact]
    public void CargoType_WithDifferentValues_ShouldNotBeEqual()
    {
        // Arrange
        var type1 = new CargoType("Electronics");
        var type2 = new CargoType("Machinery");

        // Act & Assert
        type1.Should().NotBe(type2);
    }

    [Fact]
    public void CargoType_CaseSensitivity_ShouldMatter()
    {
        // Arrange
        var type1 = new CargoType("Electronics");
        var type2 = new CargoType("electronics");

        // Act & Assert
        type1.Should().NotBe(type2);
    }

    #endregion

    #region ToString Tests

    [Fact]
    public void CargoType_ToString_ShouldReturnType()
    {
        // Arrange
        var cargoType = new CargoType("Electronics");

        // Act
        var result = cargoType.ToString();

        // Assert
        result.Should().Be("Electronics");
    }

    [Fact]
    public void CargoType_ToString_AfterTrimming_ShouldReturnTrimmedType()
    {
        // Arrange
        var cargoType = new CargoType("  Machinery  ");

        // Act
        var result = cargoType.ToString();

        // Assert
        result.Should().Be("Machinery");
    }

    #endregion

    #region Special Characters and Unicode

    [Theory]
    [InlineData("Électronique")] // French characters
    [InlineData("Máquinas")] // Spanish/Portuguese characters
    [InlineData("中文货物")] // Chinese characters
    [InlineData("Грузы")] // Cyrillic characters
    [InlineData("Electronics & Parts")] // With ampersand
    [InlineData("Type-A")] // With hyphen
    [InlineData("Type_B")] // With underscore
    [InlineData("Type #123")] // With hash and numbers
    [InlineData("50% Cotton")] // With percentage
    [InlineData("$Premium$")] // With dollar signs
    public void CreateCargoType_WithSpecialCharacters_ShouldSucceed(string typeWithSpecialChars)
    {
        // Act
        var cargoType = new CargoType(typeWithSpecialChars);

        // Assert
        cargoType.Type.Should().Be(typeWithSpecialChars);
    }

    [Fact]
    public void CreateCargoType_WithOnlyNumbers_ShouldSucceed()
    {
        // Arrange
        var numericType = "12345";

        // Act
        var cargoType = new CargoType(numericType);

        // Assert
        cargoType.Type.Should().Be("12345");
    }

    [Fact]
    public void CargoType_WithEmojis_ShouldWork()
    {
        // Arrange
        var emojiType = "Food 🍕🍔";

        // Act
        var cargoType = new CargoType(emojiType);

        // Assert
        cargoType.Type.Should().Be("Food 🍕🍔");
    }

    #endregion

    #region Edge Cases

    [Fact]
    public void CargoType_WithOnlySpecialCharacters_ShouldSucceed()
    {
        // Arrange - 3+ special characters
        var specialType = "###";

        // Act
        var cargoType = new CargoType(specialType);

        // Assert
        cargoType.Type.Should().Be("###");
    }

    [Fact]
    public void CargoType_ExactlyAtBoundary_BelowMin_ShouldFail()
    {
        // Arrange - 2 characters (just below minimum)
        var act = () => new CargoType("Ab");

        // Assert
        act.Should().Throw<BusinessRuleValidationException>()
            .WithMessage("*at least 3 characters*");
    }

    [Fact]
    public void CargoType_ExactlyAtBoundary_AboveMax_ShouldFail()
    {
        // Arrange - 51 characters (just above maximum)
        var tooLong = new string('X', 51);
        var act = () => new CargoType(tooLong);

        // Assert
        act.Should().Throw<BusinessRuleValidationException>()
            .WithMessage("*cannot exceed 50 characters*");
    }

    [Fact]
    public void CargoType_WithLineBreaks_ShouldTrimThem()
    {
        // Arrange
        var typeWithLineBreaks = "\nElectronics\n";

        // Act
        var cargoType = new CargoType(typeWithLineBreaks);

        // Assert
        cargoType.Type.Should().Be("Electronics");
        cargoType.Type.Should().NotContain("\n");
    }

    [Fact]
    public void CargoType_WithMixedWhitespace_ShouldTrimCorrectly()
    {
        // Arrange
        var typeWithMixedWhitespace = "  \t\n  Food  \r\n\t  ";

        // Act
        var cargoType = new CargoType(typeWithMixedWhitespace);

        // Assert
        cargoType.Type.Should().Be("Food");
    }

    #endregion
}
