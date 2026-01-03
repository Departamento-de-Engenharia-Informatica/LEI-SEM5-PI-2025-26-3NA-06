using FluentAssertions;
using ProjArqsi.Domain.Shared;
using ProjArqsi.Domain.UserAggregate.ValueObjects;
using Xunit;

namespace ProjArqsi.Tests.ValueObjectTests.User
{
    public class UsernameTests
    {
        #region Valid Username Tests

        [Fact]
        public void Constructor_WithValidUsername_ShouldCreateUsername()
        {
            // Arrange
            var usernameString = "john_doe";

            // Act
            var username = new Username(usernameString);

            // Assert
            username.Should().NotBeNull();
            username.Value.Should().Be(usernameString);
        }

        [Theory]
        [InlineData("abc")]
        [InlineData("user123")]
        [InlineData("john_doe")]
        [InlineData("jane.smith")]
        [InlineData("admin_user")]
        [InlineData("test_account_123")]
        [InlineData("username")]
        public void Constructor_WithVariousValidUsernames_ShouldCreateUsername(string usernameString)
        {
            // Act
            var username = new Username(usernameString);

            // Assert
            username.Value.Should().Be(usernameString);
        }

        [Fact]
        public void Constructor_WithMinimumLength_ShouldCreateUsername()
        {
            // Arrange
            var usernameString = "abc";

            // Act
            var username = new Username(usernameString);

            // Assert
            username.Value.Should().Be(usernameString);
            username.Value.Length.Should().Be(3);
        }

        [Fact]
        public void Constructor_WithMaximumLength_ShouldCreateUsername()
        {
            // Arrange
            var usernameString = new string('a', 50);

            // Act
            var username = new Username(usernameString);

            // Assert
            username.Value.Should().Be(usernameString);
            username.Value.Length.Should().Be(50);
        }

        [Fact]
        public void Constructor_WithUnderscores_ShouldCreateUsername()
        {
            // Arrange
            var usernameString = "user_name_test";

            // Act
            var username = new Username(usernameString);

            // Assert
            username.Value.Should().Be(usernameString);
        }

        [Fact]
        public void Constructor_WithDots_ShouldCreateUsername()
        {
            // Arrange
            var usernameString = "user.name.test";

            // Act
            var username = new Username(usernameString);

            // Assert
            username.Value.Should().Be(usernameString);
        }

        [Fact]
        public void Constructor_WithNumbers_ShouldCreateUsername()
        {
            // Arrange
            var usernameString = "user123";

            // Act
            var username = new Username(usernameString);

            // Assert
            username.Value.Should().Be(usernameString);
        }

        [Fact]
        public void Constructor_WithMixedCharacters_ShouldCreateUsername()
        {
            // Arrange
            var usernameString = "user_123.test";

            // Act
            var username = new Username(usernameString);

            // Assert
            username.Value.Should().Be(usernameString);
        }

        [Fact]
        public void Constructor_WithUppercaseLetters_ShouldCreateUsername()
        {
            // Arrange
            var usernameString = "UserName";

            // Act
            var username = new Username(usernameString);

            // Assert
            username.Value.Should().Be(usernameString);
        }

        [Fact]
        public void Constructor_WithAllUppercase_ShouldCreateUsername()
        {
            // Arrange
            var usernameString = "USERNAME";

            // Act
            var username = new Username(usernameString);

            // Assert
            username.Value.Should().Be(usernameString);
        }

        #endregion

        #region Invalid Username Tests - Empty/Null

        [Fact]
        public void Constructor_WithNull_ShouldThrowBusinessRuleValidationException()
        {
            // Arrange
            string usernameString = null!;

            // Act
            Action act = () => new Username(usernameString);

            // Assert
            act.Should().Throw<BusinessRuleValidationException>()
                .WithMessage("Username cannot be empty.");
        }

        [Fact]
        public void Constructor_WithEmptyString_ShouldThrowBusinessRuleValidationException()
        {
            // Arrange
            var usernameString = "";

            // Act
            Action act = () => new Username(usernameString);

            // Assert
            act.Should().Throw<BusinessRuleValidationException>()
                .WithMessage("Username cannot be empty.");
        }

        [Fact]
        public void Constructor_WithWhitespaceOnly_ShouldThrowBusinessRuleValidationException()
        {
            // Arrange
            var usernameString = "   ";

            // Act
            Action act = () => new Username(usernameString);

            // Assert
            act.Should().Throw<BusinessRuleValidationException>()
                .WithMessage("Username cannot be empty.");
        }

        [Theory]
        [InlineData("\t")]
        [InlineData("\n")]
        [InlineData("     ")]
        public void Constructor_WithOnlyWhitespaceCharacters_ShouldThrowBusinessRuleValidationException(string usernameString)
        {
            // Act
            Action act = () => new Username(usernameString);

            // Assert
            act.Should().Throw<BusinessRuleValidationException>()
                .WithMessage("Username cannot be empty.");
        }

        #endregion

        #region Invalid Username Tests - Length

        [Fact]
        public void Constructor_WithTooShort_ShouldThrowBusinessRuleValidationException()
        {
            // Arrange
            var usernameString = "ab";

            // Act
            Action act = () => new Username(usernameString);

            // Assert
            act.Should().Throw<BusinessRuleValidationException>()
                .WithMessage("Username must be at least 3 characters.");
        }

        [Theory]
        [InlineData("a")]
        [InlineData("ab")]
        public void Constructor_WithVariousShortUsernames_ShouldThrowBusinessRuleValidationException(string usernameString)
        {
            // Act
            Action act = () => new Username(usernameString);

            // Assert
            act.Should().Throw<BusinessRuleValidationException>()
                .WithMessage("Username must be at least 3 characters.");
        }

        [Fact]
        public void Constructor_WithTooLong_ShouldThrowBusinessRuleValidationException()
        {
            // Arrange
            var usernameString = new string('a', 51);

            // Act
            Action act = () => new Username(usernameString);

            // Assert
            act.Should().Throw<BusinessRuleValidationException>()
                .WithMessage("Username must be at most 20 characters.");
        }

        [Fact]
        public void Constructor_WithMuchLongerUsername_ShouldThrowBusinessRuleValidationException()
        {
            // Arrange
            var usernameString = new string('x', 100);

            // Act
            Action act = () => new Username(usernameString);

            // Assert
            act.Should().Throw<BusinessRuleValidationException>()
                .WithMessage("Username must be at most 20 characters.");
        }

        #endregion

        #region Invalid Username Tests - Starting with Number

        [Fact]
        public void Constructor_WithStartingNumber_ShouldThrowBusinessRuleValidationException()
        {
            // Arrange
            var usernameString = "123user";

            // Act
            Action act = () => new Username(usernameString);

            // Assert
            act.Should().Throw<BusinessRuleValidationException>()
                .WithMessage("Username cannot start with a number.");
        }

        [Theory]
        [InlineData("0user")]
        [InlineData("1test")]
        [InlineData("9abc")]
        [InlineData("5username")]
        public void Constructor_WithVariousStartingNumbers_ShouldThrowBusinessRuleValidationException(string usernameString)
        {
            // Act
            Action act = () => new Username(usernameString);

            // Assert
            act.Should().Throw<BusinessRuleValidationException>()
                .WithMessage("Username cannot start with a number.");
        }

        #endregion

        #region Invalid Username Tests - Spaces

        [Fact]
        public void Constructor_WithSpace_ShouldThrowBusinessRuleValidationException()
        {
            // Arrange
            var usernameString = "user name";

            // Act
            Action act = () => new Username(usernameString);

            // Assert
            act.Should().Throw<BusinessRuleValidationException>()
                .WithMessage("Username cannot contain spaces.");
        }

        [Theory]
        [InlineData("user name")]
        [InlineData("first last")]
        [InlineData("test user")]
        [InlineData("a b c")]
        public void Constructor_WithVariousSpaces_ShouldThrowBusinessRuleValidationException(string usernameString)
        {
            // Act
            Action act = () => new Username(usernameString);

            // Assert
            act.Should().Throw<BusinessRuleValidationException>()
                .WithMessage("Username cannot contain spaces.");
        }

        [Fact]
        public void Constructor_WithLeadingSpace_ShouldThrowBusinessRuleValidationException()
        {
            // Arrange
            var usernameString = " username";

            // Act
            Action act = () => new Username(usernameString);

            // Assert
            act.Should().Throw<BusinessRuleValidationException>()
                .WithMessage("Username cannot contain spaces.");
        }

        [Fact]
        public void Constructor_WithTrailingSpace_ShouldThrowBusinessRuleValidationException()
        {
            // Arrange
            var usernameString = "username ";

            // Act
            Action act = () => new Username(usernameString);

            // Assert
            act.Should().Throw<BusinessRuleValidationException>()
                .WithMessage("Username cannot contain spaces.");
        }

        #endregion

        #region Invalid Username Tests - Ending Characters

        [Fact]
        public void Constructor_WithEndingDot_ShouldThrowBusinessRuleValidationException()
        {
            // Arrange
            var usernameString = "username.";

            // Act
            Action act = () => new Username(usernameString);

            // Assert
            act.Should().Throw<BusinessRuleValidationException>()
                .WithMessage("Username cannot end with a dot or underscore.");
        }

        [Fact]
        public void Constructor_WithEndingUnderscore_ShouldThrowBusinessRuleValidationException()
        {
            // Arrange
            var usernameString = "username_";

            // Act
            Action act = () => new Username(usernameString);

            // Assert
            act.Should().Throw<BusinessRuleValidationException>()
                .WithMessage("Username cannot end with a dot or underscore.");
        }

        [Theory]
        [InlineData("user.")]
        [InlineData("test_")]
        [InlineData("username.")]
        [InlineData("testuser_")]
        public void Constructor_WithVariousEndingCharacters_ShouldThrowBusinessRuleValidationException(string usernameString)
        {
            // Act
            Action act = () => new Username(usernameString);

            // Assert
            act.Should().Throw<BusinessRuleValidationException>()
                .WithMessage("Username cannot end with a dot or underscore.");
        }

        #endregion

        #region ToString Tests

        [Fact]
        public void ToString_ShouldReturnUsernameValue()
        {
            // Arrange
            var usernameString = "testuser";
            var username = new Username(usernameString);

            // Act
            var result = username.ToString();

            // Assert
            result.Should().Be(usernameString);
        }

        [Theory]
        [InlineData("admin")]
        [InlineData("user123")]
        [InlineData("john_doe")]
        public void ToString_WithVariousUsernames_ShouldReturnCorrectValue(string usernameString)
        {
            // Arrange
            var username = new Username(usernameString);

            // Act
            var result = username.ToString();

            // Assert
            result.Should().Be(usernameString);
        }

        #endregion

        #region Equality Tests

        [Fact]
        public void Equals_WithSameUsername_ShouldReturnTrue()
        {
            // Arrange
            var username1 = new Username("testuser");
            var username2 = new Username("testuser");

            // Act
            var result = username1.Equals(username2);

            // Assert
            result.Should().BeTrue();
        }

        [Fact]
        public void Equals_WithDifferentUsername_ShouldReturnFalse()
        {
            // Arrange
            var username1 = new Username("user1");
            var username2 = new Username("user2");

            // Act
            var result = username1.Equals(username2);

            // Assert
            result.Should().BeFalse();
        }

        [Fact]
        public void Equals_WithDifferentCasing_ShouldReturnFalse()
        {
            // Arrange
            var username1 = new Username("username");
            var username2 = new Username("USERNAME");

            // Act
            var result = username1.Equals(username2);

            // Assert
            result.Should().BeFalse("username comparison is case-sensitive");
        }

        [Fact]
        public void GetHashCode_WithSameUsername_ShouldReturnSameHashCode()
        {
            // Arrange
            var username1 = new Username("testuser");
            var username2 = new Username("testuser");

            // Act
            var hash1 = username1.GetHashCode();
            var hash2 = username2.GetHashCode();

            // Assert
            hash1.Should().Be(hash2);
        }

        [Fact]
        public void GetHashCode_WithDifferentUsername_ShouldReturnDifferentHashCode()
        {
            // Arrange
            var username1 = new Username("user1");
            var username2 = new Username("user2");

            // Act
            var hash1 = username1.GetHashCode();
            var hash2 = username2.GetHashCode();

            // Assert
            hash1.Should().NotBe(hash2);
        }

        #endregion

        #region Edge Cases

        [Fact]
        public void Constructor_WithExactly3Characters_ShouldCreateUsername()
        {
            // Arrange
            var usernameString = "abc";

            // Act
            var username = new Username(usernameString);

            // Assert
            username.Value.Should().HaveLength(3);
        }

        [Fact]
        public void Constructor_WithExactly50Characters_ShouldCreateUsername()
        {
            // Arrange
            var usernameString = new string('a', 50);

            // Act
            var username = new Username(usernameString);

            // Assert
            username.Value.Should().HaveLength(50);
        }

        [Fact]
        public void Value_AfterConstruction_ShouldBeImmutable()
        {
            // Arrange
            var usernameString = "testuser";
            var username = new Username(usernameString);

            // Act
            var retrievedValue = username.Value;

            // Assert
            retrievedValue.Should().Be(usernameString);
        }

        [Fact]
        public void Equals_WithNull_ShouldReturnFalse()
        {
            // Arrange
            var username = new Username("testuser");

            // Act
            var result = username.Equals(null);

            // Assert
            result.Should().BeFalse();
        }

        [Fact]
        public void Equals_WithArbitraryObject_ShouldReturnFalse()
        {
            // Arrange
            var username = new Username("testuser");
            var arbitraryObject = new { Username = "testuser" };

            // Act
            var result = username.Equals(arbitraryObject);

            // Assert
            result.Should().BeFalse();
        }

        #endregion

        #region Real-World Scenarios

        [Fact]
        public void Constructor_WithCommonUsername_ShouldCreateUsername()
        {
            // Arrange
            var usernameString = "john_doe";

            // Act
            var username = new Username(usernameString);

            // Assert
            username.Value.Should().Be(usernameString);
        }

        [Fact]
        public void Constructor_WithAdminUsername_ShouldCreateUsername()
        {
            // Arrange
            var usernameString = "admin";

            // Act
            var username = new Username(usernameString);

            // Assert
            username.Value.Should().Be(usernameString);
        }

        [Fact]
        public void Constructor_WithModeratorUsername_ShouldCreateUsername()
        {
            // Arrange
            var usernameString = "moderator123";

            // Act
            var username = new Username(usernameString);

            // Assert
            username.Value.Should().Be(usernameString);
        }

        #endregion
    }
}
