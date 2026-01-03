using FluentAssertions;
using ProjArqsi.Domain.Shared;
using ProjArqsi.Domain.StorageAreaAggregate;
using Xunit;

namespace ProjArqsi.Tests.ValueObjectTests.StorageArea
{
    public class AreaNameTests
    {
        #region Valid Area Name Tests

        [Fact]
        public void Constructor_WithValidName_ShouldCreateAreaName()
        {
            // Arrange
            var name = "North Yard";

            // Act
            var areaName = new AreaName(name);

            // Assert
            areaName.Should().NotBeNull();
            areaName.Value.Should().Be(name);
        }

        [Theory]
        [InlineData("A")]
        [InlineData("Area1")]
        [InlineData("Storage Area A")]
        [InlineData("North Terminal - Section 3")]
        [InlineData("Área de Armazenamento")]
        public void Constructor_WithVariousValidNames_ShouldCreateAreaName(string name)
        {
            // Act
            var areaName = new AreaName(name);

            // Assert
            areaName.Value.Should().Be(name);
        }

        [Fact]
        public void Constructor_WithMaxLengthName_ShouldCreateAreaName()
        {
            // Arrange
            var name = new string('A', 100);

            // Act
            var areaName = new AreaName(name);

            // Assert
            areaName.Value.Should().Be(name);
            areaName.Value.Length.Should().Be(100);
        }

        [Fact]
        public void Constructor_WithNameContainingNumbers_ShouldCreateAreaName()
        {
            // Arrange
            var name = "Warehouse 123";

            // Act
            var areaName = new AreaName(name);

            // Assert
            areaName.Value.Should().Be(name);
        }

        [Fact]
        public void Constructor_WithNameContainingSpecialCharacters_ShouldCreateAreaName()
        {
            // Arrange
            var name = "Area-North_01 (Main)";

            // Act
            var areaName = new AreaName(name);

            // Assert
            areaName.Value.Should().Be(name);
        }

        [Fact]
        public void Constructor_WithLeadingAndTrailingSpaces_ShouldTrimSpaces()
        {
            // Arrange
            var name = "   Main Area   ";

            // Act
            var areaName = new AreaName(name);

            // Assert
            areaName.Value.Should().Be("Main Area");
        }

        [Fact]
        public void Constructor_WithMultipleInternalSpaces_ShouldPreserveSpaces()
        {
            // Arrange
            var name = "Area   With   Spaces";

            // Act
            var areaName = new AreaName(name);

            // Assert
            areaName.Value.Should().Be(name);
        }

        [Fact]
        public void Constructor_WithUnicodeCharacters_ShouldCreateAreaName()
        {
            // Arrange
            var name = "Área 东区 Зона";

            // Act
            var areaName = new AreaName(name);

            // Assert
            areaName.Value.Should().Be(name);
        }

        #endregion

        #region Invalid Area Name Tests

        [Fact]
        public void Constructor_WithNull_ShouldThrowBusinessRuleValidationException()
        {
            // Arrange
            string name = null!;

            // Act
            Action act = () => new AreaName(name);

            // Assert
            act.Should().Throw<BusinessRuleValidationException>()
                .WithMessage("Area name is required.");
        }

        [Fact]
        public void Constructor_WithEmptyString_ShouldThrowBusinessRuleValidationException()
        {
            // Arrange
            var name = "";

            // Act
            Action act = () => new AreaName(name);

            // Assert
            act.Should().Throw<BusinessRuleValidationException>()
                .WithMessage("Area name is required.");
        }

        [Fact]
        public void Constructor_WithWhitespaceOnly_ShouldThrowBusinessRuleValidationException()
        {
            // Arrange
            var name = "   ";

            // Act
            Action act = () => new AreaName(name);

            // Assert
            act.Should().Throw<BusinessRuleValidationException>()
                .WithMessage("Area name is required.");
        }

        [Fact]
        public void Constructor_WithExceedingMaxLength_ShouldThrowBusinessRuleValidationException()
        {
            // Arrange
            var name = new string('A', 101);

            // Act
            Action act = () => new AreaName(name);

            // Assert
            act.Should().Throw<BusinessRuleValidationException>()
                .WithMessage("Area name cannot exceed 100 characters.");
        }

        [Fact]
        public void Constructor_WithMuchLongerName_ShouldThrowBusinessRuleValidationException()
        {
            // Arrange
            var name = new string('X', 500);

            // Act
            Action act = () => new AreaName(name);

            // Assert
            act.Should().Throw<BusinessRuleValidationException>()
                .WithMessage("Area name cannot exceed 100 characters.");
        }

        [Theory]
        [InlineData("\t")]
        [InlineData("\n")]
        [InlineData("\r\n")]
        [InlineData("     \t\n     ")]
        public void Constructor_WithOnlyWhitespaceCharacters_ShouldThrowBusinessRuleValidationException(string name)
        {
            // Act
            Action act = () => new AreaName(name);

            // Assert
            act.Should().Throw<BusinessRuleValidationException>()
                .WithMessage("Area name is required.");
        }

        #endregion

        #region Equality Tests

        [Fact]
        public void Equals_WithSameName_ShouldReturnTrue()
        {
            // Arrange
            var name1 = new AreaName("North Yard");
            var name2 = new AreaName("North Yard");

            // Act
            var result = name1.Equals(name2);

            // Assert
            result.Should().BeTrue();
        }

        [Fact]
        public void Equals_WithDifferentName_ShouldReturnFalse()
        {
            // Arrange
            var name1 = new AreaName("North Yard");
            var name2 = new AreaName("South Yard");

            // Act
            var result = name1.Equals(name2);

            // Assert
            result.Should().BeFalse();
        }

        [Fact]
        public void Equals_WithSameNameDifferentCasing_ShouldReturnFalse()
        {
            // Arrange
            var name1 = new AreaName("North Yard");
            var name2 = new AreaName("NORTH YARD");

            // Act
            var result = name1.Equals(name2);

            // Assert
            result.Should().BeFalse();
        }

        [Fact]
        public void Equals_WithTrimmedAndUntrimmed_ShouldReturnTrue()
        {
            // Arrange
            var name1 = new AreaName("Main Area");
            var name2 = new AreaName("  Main Area  ");

            // Act
            var result = name1.Equals(name2);

            // Assert
            result.Should().BeTrue("both should be trimmed to the same value");
        }

        [Fact]
        public void GetHashCode_WithSameName_ShouldReturnSameHashCode()
        {
            // Arrange
            var name1 = new AreaName("Warehouse A");
            var name2 = new AreaName("Warehouse A");

            // Act
            var hash1 = name1.GetHashCode();
            var hash2 = name2.GetHashCode();

            // Assert
            hash1.Should().Be(hash2);
        }

        [Fact]
        public void GetHashCode_WithDifferentName_ShouldReturnDifferentHashCode()
        {
            // Arrange
            var name1 = new AreaName("Warehouse A");
            var name2 = new AreaName("Warehouse B");

            // Act
            var hash1 = name1.GetHashCode();
            var hash2 = name2.GetHashCode();

            // Assert
            hash1.Should().NotBe(hash2);
        }

        #endregion

        #region Edge Cases

        [Fact]
        public void Constructor_WithExactly100Characters_ShouldCreateAreaName()
        {
            // Arrange
            var name = new string('A', 100);

            // Act
            var areaName = new AreaName(name);

            // Assert
            areaName.Value.Should().HaveLength(100);
        }

        [Fact]
        public void Constructor_WithSpacesBeforeTrimmingResultingInValid_ShouldCreateAreaName()
        {
            // Arrange
            var name = "  A  ";

            // Act
            var areaName = new AreaName(name);

            // Assert
            areaName.Value.Should().Be("A");
        }

        [Fact]
        public void Constructor_WithSpacesBeforeTrimmingResultingInEmpty_ShouldThrowException()
        {
            // Arrange - 100 chars with spaces, but only spaces
            var name = new string(' ', 100);

            // Act
            Action act = () => new AreaName(name);

            // Assert
            act.Should().Throw<BusinessRuleValidationException>()
                .WithMessage("Area name is required.");
        }

        [Fact]
        public void Value_AfterConstruction_ShouldBeImmutable()
        {
            // Arrange
            var originalName = "Test Area";
            var areaName = new AreaName(originalName);

            // Act
            var retrievedValue = areaName.Value;

            // Assert
            retrievedValue.Should().Be(originalName);
        }

        [Theory]
        [InlineData("Area 1", "Area 1")]
        [InlineData("  Area 2  ", "Area 2")]
        [InlineData("Area\t3", "Area\t3")]
        public void Constructor_WithVariousWhitespaceScenarios_ShouldHandleCorrectly(string input, string expected)
        {
            // Act
            var areaName = new AreaName(input);

            // Assert
            areaName.Value.Should().Be(expected);
        }

        #endregion
    }
}
