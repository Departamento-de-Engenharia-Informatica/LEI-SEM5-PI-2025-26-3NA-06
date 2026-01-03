using FluentAssertions;
using ProjArqsi.Domain.DockAggregate;
using ProjArqsi.Domain.Shared;
using Xunit;

namespace ProjArqsi.Tests._1_ValueObjectTests.Dock
{
    public class DockNameTests
    {
        #region Valid Creation Tests

        [Theory]
        [InlineData("Dock A")]
        [InlineData("Terminal 1")]
        [InlineData("Berth 42")]
        [InlineData("Main Dock")]
        [InlineData("D")]
        public void CreateDockName_WithValidName_ShouldSucceed(string validName)
        {
            // Act
            var dockName = new DockName(validName);

            // Assert
            dockName.Should().NotBeNull();
            dockName.Value.Should().Be(validName.Trim());
        }

        [Fact]
        public void CreateDockName_WithMaxLength_ShouldSucceed()
        {
            // Arrange - Exactly 100 characters
            var maxLengthName = new string('A', 100);

            // Act
            var dockName = new DockName(maxLengthName);

            // Assert
            dockName.Should().NotBeNull();
            dockName.Value.Should().HaveLength(100);
        }

        [Fact]
        public void CreateDockName_WithSpecialCharacters_ShouldSucceed()
        {
            // Arrange
            var nameWithSpecialChars = "Dock-A_1.0 #42";

            // Act
            var dockName = new DockName(nameWithSpecialChars);

            // Assert
            dockName.Value.Should().Be("Dock-A_1.0 #42");
        }

        [Fact]
        public void CreateDockName_WithLeadingAndTrailingSpaces_ShouldTrimSpaces()
        {
            // Arrange
            var nameWithSpaces = "   Dock A   ";

            // Act
            var dockName = new DockName(nameWithSpaces);

            // Assert
            dockName.Value.Should().Be("Dock A");
        }

        [Fact]
        public void CreateDockName_WithUnicodeCharacters_ShouldSucceed()
        {
            // Arrange
            var unicodeName = "Doca São João";

            // Act
            var dockName = new DockName(unicodeName);

            // Assert
            dockName.Value.Should().Be("Doca São João");
        }

        [Fact]
        public void CreateDockName_WithNumbers_ShouldSucceed()
        {
            // Arrange
            var numericName = "12345";

            // Act
            var dockName = new DockName(numericName);

            // Assert
            dockName.Value.Should().Be("12345");
        }

        #endregion

        #region Invalid Creation Tests

        [Fact]
        public void CreateDockName_WithNull_ShouldThrowBusinessRuleValidationException()
        {
            // Arrange
            string? nullName = null;

            // Act
            Action act = () => new DockName(nullName!);

            // Assert
            act.Should().Throw<BusinessRuleValidationException>()
                .WithMessage("*cannot be empty*");
        }

        [Fact]
        public void CreateDockName_WithEmptyString_ShouldThrowBusinessRuleValidationException()
        {
            // Arrange
            var emptyName = string.Empty;

            // Act
            Action act = () => new DockName(emptyName);

            // Assert
            act.Should().Throw<BusinessRuleValidationException>()
                .WithMessage("*cannot be empty*");
        }

        [Theory]
        [InlineData(" ")]
        [InlineData("   ")]
        [InlineData("\t")]
        [InlineData("\n")]
        public void CreateDockName_WithWhitespaceOnly_ShouldThrowBusinessRuleValidationException(string whitespaceName)
        {
            // Act
            Action act = () => new DockName(whitespaceName);

            // Assert
            act.Should().Throw<BusinessRuleValidationException>()
                .WithMessage("*cannot be empty*");
        }

        [Fact]
        public void CreateDockName_ExceedingMaxLength_ShouldThrowBusinessRuleValidationException()
        {
            // Arrange - 101 characters
            var tooLongName = new string('A', 101);

            // Act
            Action act = () => new DockName(tooLongName);

            // Assert
            act.Should().Throw<BusinessRuleValidationException>()
                .WithMessage("*cannot exceed 100 characters*");
        }

        [Fact]
        public void CreateDockName_WithSignificantlyExceededLength_ShouldThrowBusinessRuleValidationException()
        {
            // Arrange - 500 characters
            var veryLongName = new string('X', 500);

            // Act
            Action act = () => new DockName(veryLongName);

            // Assert
            act.Should().Throw<BusinessRuleValidationException>()
                .WithMessage("*cannot exceed 100 characters*");
        }

        #endregion

        #region Equality Tests

        [Fact]
        public void DockName_WithSameValue_ShouldBeEqual()
        {
            // Arrange
            var name1 = new DockName("Dock A");
            var name2 = new DockName("Dock A");

            // Act & Assert
            name1.Should().Be(name2);
            name1.GetHashCode().Should().Be(name2.GetHashCode());
        }

        [Fact]
        public void DockName_WithDifferentValues_ShouldNotBeEqual()
        {
            // Arrange
            var name1 = new DockName("Dock A");
            var name2 = new DockName("Dock B");

            // Act & Assert
            name1.Should().NotBe(name2);
            name1.GetHashCode().Should().NotBe(name2.GetHashCode());
        }

        [Fact]
        public void DockName_WithSameValueAfterTrimming_ShouldBeEqual()
        {
            // Arrange
            var name1 = new DockName("  Dock A  ");
            var name2 = new DockName("Dock A");

            // Act & Assert
            name1.Should().Be(name2);
        }

        [Fact]
        public void DockName_ComparedToNull_ShouldNotBeEqual()
        {
            // Arrange
            var dockName = new DockName("Dock A");

            // Act & Assert
            dockName.Should().NotBe(null);
        }

        [Fact]
        public void DockName_CaseeSensitiveComparison_ShouldNotBeEqualForDifferentCases()
        {
            // Arrange
            var name1 = new DockName("Dock A");
            var name2 = new DockName("dock a");

            // Act & Assert
            name1.Should().NotBe(name2);
        }

        #endregion

        #region Edge Case Tests

        [Fact]
        public void CreateDockName_WithSingleCharacter_ShouldSucceed()
        {
            // Arrange
            var singleChar = "A";

            // Act
            var dockName = new DockName(singleChar);

            // Assert
            dockName.Value.Should().Be("A");
        }

        [Fact]
        public void CreateDockName_WithMultipleSpacesBetweenWords_ShouldPreserveInternalSpaces()
        {
            // Arrange
            var nameWithSpaces = "Dock    A";

            // Act
            var dockName = new DockName(nameWithSpaces);

            // Assert
            dockName.Value.Should().Be("Dock    A");
        }

        [Fact]
        public void CreateDockName_WithExactly99Characters_ShouldSucceed()
        {
            // Arrange
            var name99Chars = new string('B', 99);

            // Act
            var dockName = new DockName(name99Chars);

            // Assert
            dockName.Value.Should().HaveLength(99);
        }

        [Fact]
        public void CreateDockName_WithMixedWhitespaceAtEnds_ShouldTrimAll()
        {
            // Arrange
            var mixedWhitespace = " \t Dock A \n ";

            // Act
            var dockName = new DockName(mixedWhitespace);

            // Assert
            dockName.Value.Should().Be("Dock A");
        }

        #endregion
    }
}
