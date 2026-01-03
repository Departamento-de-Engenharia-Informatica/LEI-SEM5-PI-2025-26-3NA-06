using FluentAssertions;
using ProjArqsi.Domain.Shared;
using Xunit;

namespace ProjArqsi.Tests.ValueObjectTests.Shared
{
    public class BusinessRuleValidationExceptionTests
    {
        #region Constructor Tests

        [Fact]
        public void Constructor_WithMessage_ShouldSetMessageAndDetails()
        {
            // Arrange
            var message = "Validation failed";

            // Act
            var exception = new BusinessRuleValidationException(message);

            // Assert
            exception.Message.Should().Be(message);
            exception.Details.Should().Be(message);
        }

        [Fact]
        public void Constructor_WithMessageAndDetails_ShouldSetBothProperties()
        {
            // Arrange
            var message = "Validation failed";
            var details = "The container ISO code is invalid";

            // Act
            var exception = new BusinessRuleValidationException(message, details);

            // Assert
            exception.Message.Should().Be(message);
            exception.Details.Should().Be(details);
        }

        [Fact]
        public void Constructor_WithEmptyMessage_ShouldAcceptEmptyString()
        {
            // Arrange
            var message = "";

            // Act
            var exception = new BusinessRuleValidationException(message);

            // Assert
            exception.Message.Should().Be(message);
            exception.Details.Should().Be(message);
        }

        [Fact]
        public void Constructor_WithEmptyDetails_ShouldAcceptEmptyString()
        {
            // Arrange
            var message = "Validation failed";
            var details = "";

            // Act
            var exception = new BusinessRuleValidationException(message, details);

            // Assert
            exception.Message.Should().Be(message);
            exception.Details.Should().Be(details);
        }

        #endregion

        #region Property Tests

        [Fact]
        public void Details_AfterConstructionWithOneParameter_ShouldEqualMessage()
        {
            // Arrange
            var message = "Business rule violation";
            var exception = new BusinessRuleValidationException(message);

            // Act
            var details = exception.Details;

            // Assert
            details.Should().Be(message);
        }

        [Fact]
        public void Details_AfterConstructionWithTwoParameters_ShouldNotEqualMessage()
        {
            // Arrange
            var message = "Validation failed";
            var details = "Specific violation details";
            var exception = new BusinessRuleValidationException(message, details);

            // Act
            var actualDetails = exception.Details;

            // Assert
            actualDetails.Should().Be(details);
            actualDetails.Should().NotBe(message);
        }

        [Fact]
        public void Message_ShouldBeInheritedFromException()
        {
            // Arrange
            var message = "Custom error message";
            var exception = new BusinessRuleValidationException(message);

            // Act & Assert
            exception.Should().BeAssignableTo<Exception>();
            exception.Message.Should().Be(message);
        }

        #endregion

        #region Exception Behavior Tests

        [Fact]
        public void Exception_ShouldBeThrowable()
        {
            // Arrange
            var message = "Test exception";

            // Act
            Action act = () => throw new BusinessRuleValidationException(message);

            // Assert
            act.Should().Throw<BusinessRuleValidationException>()
                .WithMessage(message);
        }

        [Fact]
        public void Exception_ShouldBeCatchableAsException()
        {
            // Arrange
            var message = "Test exception";
            Exception? caughtException = null;

            // Act
            try
            {
                throw new BusinessRuleValidationException(message);
            }
            catch (Exception ex)
            {
                caughtException = ex;
            }

            // Assert
            caughtException.Should().NotBeNull();
            caughtException.Should().BeOfType<BusinessRuleValidationException>();
            caughtException!.Message.Should().Be(message);
        }

        [Fact]
        public void Exception_WithDetails_ShouldPreserveDetails()
        {
            // Arrange
            var message = "Validation error";
            var details = "Container dimensions exceed maximum allowed";
            BusinessRuleValidationException? caughtException = null;

            // Act
            try
            {
                throw new BusinessRuleValidationException(message, details);
            }
            catch (BusinessRuleValidationException ex)
            {
                caughtException = ex;
            }

            // Assert
            caughtException.Should().NotBeNull();
            caughtException!.Message.Should().Be(message);
            caughtException.Details.Should().Be(details);
        }

        #endregion

        #region Edge Cases

        [Fact]
        public void Constructor_WithVeryLongMessage_ShouldHandleCorrectly()
        {
            // Arrange
            var message = new string('A', 10000);

            // Act
            var exception = new BusinessRuleValidationException(message);

            // Assert
            exception.Message.Should().Be(message);
            exception.Details.Should().Be(message);
        }

        [Fact]
        public void Constructor_WithVeryLongDetails_ShouldHandleCorrectly()
        {
            // Arrange
            var message = "Error";
            var details = new string('B', 10000);

            // Act
            var exception = new BusinessRuleValidationException(message, details);

            // Assert
            exception.Message.Should().Be(message);
            exception.Details.Should().Be(details);
        }

        [Fact]
        public void Constructor_WithSpecialCharacters_ShouldPreserveCharacters()
        {
            // Arrange
            var message = "Error: \"Invalid\" <value> & 'special' chars\n\t";
            var details = "Details with ñ, ü, ç, and émojis 🚢";

            // Act
            var exception = new BusinessRuleValidationException(message, details);

            // Assert
            exception.Message.Should().Be(message);
            exception.Details.Should().Be(details);
        }

        [Theory]
        [InlineData("Simple error")]
        [InlineData("Error with numbers: 123456")]
        [InlineData("Error with punctuation: !@#$%^&*()")]
        [InlineData("Multi\nLine\nError\nMessage")]
        public void Constructor_WithVariousMessages_ShouldWorkCorrectly(string message)
        {
            // Act
            var exception = new BusinessRuleValidationException(message);

            // Assert
            exception.Message.Should().Be(message);
            exception.Details.Should().Be(message);
        }

        [Fact]
        public void Constructor_WithWhitespaceMessage_ShouldPreserveWhitespace()
        {
            // Arrange
            var message = "   Error with spaces   ";
            var details = "\tDetails with tabs\t";

            // Act
            var exception = new BusinessRuleValidationException(message, details);

            // Assert
            exception.Message.Should().Be(message);
            exception.Details.Should().Be(details);
        }

        #endregion

        #region Real-World Scenarios

        [Fact]
        public void Exception_WithDomainValidationMessage_ShouldBeUsefulForLogging()
        {
            // Arrange
            var message = "Container validation failed";
            var details = "ISO code MSCU1234564 has invalid check digit. Expected: 5, Got: 4";

            // Act
            var exception = new BusinessRuleValidationException(message, details);

            // Assert
            exception.Message.Should().Contain("validation");
            exception.Details.Should().Contain("ISO code");
            exception.Details.Should().Contain("check digit");
        }

        [Fact]
        public void Exception_WithBusinessRuleViolation_ShouldCommunicateClearly()
        {
            // Arrange
            var message = "Dock capacity exceeded";
            var details = "Cannot assign vessel with draft 15.5m to dock with maximum draft 12.0m";

            // Act
            var exception = new BusinessRuleValidationException(message, details);

            // Assert
            exception.Message.Should().Be(message);
            exception.Details.Should().Contain("15.5m");
            exception.Details.Should().Contain("12.0m");
        }

        #endregion
    }
}
