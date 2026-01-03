using FluentAssertions;
using ProjArqsi.Domain.Shared;
using ProjArqsi.Domain.UserAggregate.ValueObjects;
using Xunit;

namespace ProjArqsi.Tests.ValueObjectTests.User
{
    public class EmailTests
    {
        #region Valid Email Tests

        [Fact]
        public void Constructor_WithValidEmail_ShouldCreateEmail()
        {
            // Arrange
            var emailString = "user@example.com";

            // Act
            var email = new Email(emailString);

            // Assert
            email.Should().NotBeNull();
            email.Value.Should().Be(emailString);
        }

        [Theory]
        [InlineData("simple@example.com")]
        [InlineData("user.name@example.com")]
        [InlineData("user+tag@example.co.uk")]
        [InlineData("user_name@example.org")]
        [InlineData("user123@example.net")]
        [InlineData("123user@example.io")]
        [InlineData("a@example.com")]
        [InlineData("test@sub.example.com")]
        public void Constructor_WithVariousValidEmails_ShouldCreateEmail(string emailString)
        {
            // Act
            var email = new Email(emailString);

            // Assert
            email.Value.Should().Be(emailString);
        }

        [Fact]
        public void Constructor_WithSubdomain_ShouldCreateEmail()
        {
            // Arrange
            var emailString = "user@mail.example.com";

            // Act
            var email = new Email(emailString);

            // Assert
            email.Value.Should().Be(emailString);
        }

        [Fact]
        public void Constructor_WithMultipleSubdomains_ShouldCreateEmail()
        {
            // Arrange
            var emailString = "user@mail.corporate.example.com";

            // Act
            var email = new Email(emailString);

            // Assert
            email.Value.Should().Be(emailString);
        }

        [Fact]
        public void Constructor_WithPlusSign_ShouldCreateEmail()
        {
            // Arrange
            var emailString = "user+filter@example.com";

            // Act
            var email = new Email(emailString);

            // Assert
            email.Value.Should().Be(emailString);
        }

        [Fact]
        public void Constructor_WithDotInLocalPart_ShouldCreateEmail()
        {
            // Arrange
            var emailString = "first.last@example.com";

            // Act
            var email = new Email(emailString);

            // Assert
            email.Value.Should().Be(emailString);
        }

        [Fact]
        public void Constructor_WithUnderscoreInLocalPart_ShouldCreateEmail()
        {
            // Arrange
            var emailString = "user_name@example.com";

            // Act
            var email = new Email(emailString);

            // Assert
            email.Value.Should().Be(emailString);
        }

        [Fact]
        public void Constructor_WithNumbersInLocalPart_ShouldCreateEmail()
        {
            // Arrange
            var emailString = "user123@example.com";

            // Act
            var email = new Email(emailString);

            // Assert
            email.Value.Should().Be(emailString);
        }

        [Fact]
        public void Constructor_WithShortTLD_ShouldCreateEmail()
        {
            // Arrange
            var emailString = "user@example.co";

            // Act
            var email = new Email(emailString);

            // Assert
            email.Value.Should().Be(emailString);
        }

        [Fact]
        public void Constructor_WithLongTLD_ShouldCreateEmail()
        {
            // Arrange
            var emailString = "user@example.travel";

            // Act
            var email = new Email(emailString);

            // Assert
            email.Value.Should().Be(emailString);
        }

        #endregion

        #region Invalid Email Tests

        [Fact]
        public void Constructor_WithNull_ShouldThrowBusinessRuleValidationException()
        {
            // Arrange
            string emailString = null!;

            // Act
            Action act = () => new Email(emailString);

            // Assert
            act.Should().Throw<BusinessRuleValidationException>()
                .WithMessage("Email cannot be empty.");
        }

        [Fact]
        public void Constructor_WithEmptyString_ShouldThrowBusinessRuleValidationException()
        {
            // Arrange
            var emailString = "";

            // Act
            Action act = () => new Email(emailString);

            // Assert
            act.Should().Throw<BusinessRuleValidationException>()
                .WithMessage("Email cannot be empty.");
        }

        [Fact]
        public void Constructor_WithWhitespaceOnly_ShouldThrowBusinessRuleValidationException()
        {
            // Arrange
            var emailString = "   ";

            // Act
            Action act = () => new Email(emailString);

            // Assert
            act.Should().Throw<BusinessRuleValidationException>()
                .WithMessage("Email cannot be empty.");
        }

        [Fact]
        public void Constructor_WithoutAtSign_ShouldThrowBusinessRuleValidationException()
        {
            // Arrange
            var emailString = "userexample.com";

            // Act
            Action act = () => new Email(emailString);

            // Assert
            act.Should().Throw<BusinessRuleValidationException>()
                .WithMessage("Invalid email format.");
        }

        [Fact]
        public void Constructor_WithMultipleAtSigns_ShouldThrowBusinessRuleValidationException()
        {
            // Arrange
            var emailString = "user@@example.com";

            // Act
            Action act = () => new Email(emailString);

            // Assert
            act.Should().Throw<BusinessRuleValidationException>()
                .WithMessage("Invalid email format.");
        }

        [Fact]
        public void Constructor_WithoutLocalPart_ShouldThrowBusinessRuleValidationException()
        {
            // Arrange
            var emailString = "@example.com";

            // Act
            Action act = () => new Email(emailString);

            // Assert
            act.Should().Throw<BusinessRuleValidationException>()
                .WithMessage("Invalid email format.");
        }

        [Fact]
        public void Constructor_WithoutDomainPart_ShouldThrowBusinessRuleValidationException()
        {
            // Arrange
            var emailString = "user@";

            // Act
            Action act = () => new Email(emailString);

            // Assert
            act.Should().Throw<BusinessRuleValidationException>()
                .WithMessage("Invalid email format.");
        }

        [Fact]
        public void Constructor_WithoutDotInDomain_ShouldThrowBusinessRuleValidationException()
        {
            // Arrange
            var emailString = "user@example";

            // Act
            Action act = () => new Email(emailString);

            // Assert
            act.Should().Throw<BusinessRuleValidationException>()
                .WithMessage("Invalid email format.");
        }

        [Fact]
        public void Constructor_WithDotAtEndOfDomain_ShouldThrowBusinessRuleValidationException()
        {
            // Arrange
            var emailString = "user@example.";

            // Act
            Action act = () => new Email(emailString);

            // Assert
            act.Should().Throw<BusinessRuleValidationException>()
                .WithMessage("Invalid email format.");
        }

        [Fact]
        public void Constructor_WithDotAtStartOfDomain_ShouldThrowBusinessRuleValidationException()
        {
            // Arrange
            var emailString = "user@.example.com";

            // Act
            Action act = () => new Email(emailString);

            // Assert
            act.Should().Throw<BusinessRuleValidationException>()
                .WithMessage("Invalid email format.");
        }

        [Fact]
        public void Constructor_WithConsecutiveDotsInDomain_ShouldThrowBusinessRuleValidationException()
        {
            // Arrange
            var emailString = "user@example..com";

            // Act
            Action act = () => new Email(emailString);

            // Assert
            act.Should().Throw<BusinessRuleValidationException>()
                .WithMessage("Invalid email format.");
        }

        [Fact]
        public void Constructor_WithOnlyOneCharacterTLD_ShouldThrowBusinessRuleValidationException()
        {
            // Arrange
            var emailString = "user@example.c";

            // Act
            Action act = () => new Email(emailString);

            // Assert
            act.Should().Throw<BusinessRuleValidationException>()
                .WithMessage("Invalid email format.");
        }

        [Theory]
        [InlineData("user")]
        [InlineData("user@")]
        [InlineData("@example.com")]
        [InlineData("user@@example.com")]
        [InlineData("user@example")]
        public void Constructor_WithVariousInvalidFormats_ShouldThrowBusinessRuleValidationException(string emailString)
        {
            // Act
            Action act = () => new Email(emailString);

            // Assert
            act.Should().Throw<BusinessRuleValidationException>()
                .WithMessage("Invalid email format.");
        }

        #endregion

        #region ToString Tests

        [Fact]
        public void ToString_ShouldReturnEmailValue()
        {
            // Arrange
            var emailString = "user@example.com";
            var email = new Email(emailString);

            // Act
            var result = email.ToString();

            // Assert
            result.Should().Be(emailString);
        }

        [Theory]
        [InlineData("test@example.com")]
        [InlineData("admin@company.org")]
        [InlineData("user+tag@service.co.uk")]
        public void ToString_WithVariousEmails_ShouldReturnCorrectValue(string emailString)
        {
            // Arrange
            var email = new Email(emailString);

            // Act
            var result = email.ToString();

            // Assert
            result.Should().Be(emailString);
        }

        #endregion

        #region Equality Tests

        [Fact]
        public void Equals_WithSameEmail_ShouldReturnTrue()
        {
            // Arrange
            var email1 = new Email("user@example.com");
            var email2 = new Email("user@example.com");

            // Act
            var result = email1.Equals(email2);

            // Assert
            result.Should().BeTrue();
        }

        [Fact]
        public void Equals_WithDifferentEmail_ShouldReturnFalse()
        {
            // Arrange
            var email1 = new Email("user1@example.com");
            var email2 = new Email("user2@example.com");

            // Act
            var result = email1.Equals(email2);

            // Assert
            result.Should().BeFalse();
        }

        [Fact]
        public void Equals_WithDifferentCasing_ShouldReturnFalse()
        {
            // Arrange
            var email1 = new Email("user@example.com");
            var email2 = new Email("User@Example.Com");

            // Act
            var result = email1.Equals(email2);

            // Assert
            result.Should().BeFalse("email comparison is case-sensitive");
        }

        [Fact]
        public void GetHashCode_WithSameEmail_ShouldReturnSameHashCode()
        {
            // Arrange
            var email1 = new Email("test@example.com");
            var email2 = new Email("test@example.com");

            // Act
            var hash1 = email1.GetHashCode();
            var hash2 = email2.GetHashCode();

            // Assert
            hash1.Should().Be(hash2);
        }

        [Fact]
        public void GetHashCode_WithDifferentEmail_ShouldReturnDifferentHashCode()
        {
            // Arrange
            var email1 = new Email("user1@example.com");
            var email2 = new Email("user2@example.com");

            // Act
            var hash1 = email1.GetHashCode();
            var hash2 = email2.GetHashCode();

            // Assert
            hash1.Should().NotBe(hash2);
        }

        #endregion

        #region Edge Cases

        [Fact]
        public void Constructor_WithVeryLongLocalPart_ShouldCreateEmail()
        {
            // Arrange
            var localPart = new string('a', 64);
            var emailString = $"{localPart}@example.com";

            // Act
            var email = new Email(emailString);

            // Assert
            email.Value.Should().Be(emailString);
        }

        [Fact]
        public void Constructor_WithVeryLongDomain_ShouldCreateEmail()
        {
            // Arrange
            var emailString = "user@verylongdomainnamewithmanychara ctersfortest.example.com";

            // Act
            var email = new Email(emailString);

            // Assert
            email.Value.Should().Be(emailString);
        }

        [Fact]
        public void Constructor_WithSingleCharacterLocalPart_ShouldCreateEmail()
        {
            // Arrange
            var emailString = "a@example.com";

            // Act
            var email = new Email(emailString);

            // Assert
            email.Value.Should().Be(emailString);
        }

        [Fact]
        public void Value_AfterConstruction_ShouldBeImmutable()
        {
            // Arrange
            var emailString = "user@example.com";
            var email = new Email(emailString);

            // Act
            var retrievedValue = email.Value;

            // Assert
            retrievedValue.Should().Be(emailString);
        }

        [Fact]
        public void Equals_WithNull_ShouldReturnFalse()
        {
            // Arrange
            var email = new Email("user@example.com");

            // Act
            var result = email.Equals(null);

            // Assert
            result.Should().BeFalse();
        }

        [Fact]
        public void Equals_WithArbitraryObject_ShouldReturnFalse()
        {
            // Arrange
            var email = new Email("user@example.com");
            var arbitraryObject = new { Email = "user@example.com" };

            // Act
            var result = email.Equals(arbitraryObject);

            // Assert
            result.Should().BeFalse();
        }

        #endregion

        #region Real-World Scenarios

        [Fact]
        public void Constructor_WithCompanyEmail_ShouldCreateEmail()
        {
            // Arrange
            var emailString = "john.doe@company.com";

            // Act
            var email = new Email(emailString);

            // Assert
            email.Value.Should().Be(emailString);
        }

        [Fact]
        public void Constructor_WithGoogleEmail_ShouldCreateEmail()
        {
            // Arrange
            var emailString = "user@gmail.com";

            // Act
            var email = new Email(emailString);

            // Assert
            email.Value.Should().Be(emailString);
        }

        [Fact]
        public void Constructor_WithEducationalEmail_ShouldCreateEmail()
        {
            // Arrange
            var emailString = "student@university.edu";

            // Act
            var email = new Email(emailString);

            // Assert
            email.Value.Should().Be(emailString);
        }

        [Fact]
        public void Constructor_WithGovernmentEmail_ShouldCreateEmail()
        {
            // Arrange
            var emailString = "official@government.gov";

            // Act
            var email = new Email(emailString);

            // Assert
            email.Value.Should().Be(emailString);
        }

        #endregion
    }
}
