using FluentAssertions;
using ProjArqsi.Domain.VesselAggregate;
using Xunit;

namespace ProjArqsi.Tests.ValueObjectTests.Vessel
{
    public class IMOnumberTests
    {
        #region Valid IMOnumber Tests

        [Fact]
        public void Constructor_WithValidIMO_ShouldCreateIMOnumber()
        {
            // Arrange & Act
            var imo = new IMOnumber("9074729");

            // Assert
            imo.Number.Should().Be("9074729");
        }

        [Theory]
        [InlineData("9074729")] // Valid IMO: (9*7 + 0*6 + 7*5 + 4*4 + 7*3 + 2*2) % 10 = 9
        [InlineData("8814275")] // Valid IMO: (8*7 + 8*6 + 1*5 + 4*4 + 2*3 + 7*2) % 10 = 5
        [InlineData("9305611")] // Valid IMO: (9*7 + 3*6 + 0*5 + 5*4 + 6*3 + 1*2) % 10 = 1
        [InlineData("9195195")] // Valid IMO: (9*7 + 1*6 + 9*5 + 5*4 + 1*3 + 9*2) % 10 = 5
        [InlineData("9000003")] // Valid IMO: (9*7 + 0*6 + 0*5 + 0*4 + 0*3 + 0*2) % 10 = 3
        public void Constructor_WithMultipleValidIMOs_ShouldCreateIMOnumber(string validIMO)
        {
            // Arrange & Act
            var imo = new IMOnumber(validIMO);

            // Assert
            imo.Number.Should().Be(validIMO);
        }

        [Fact]
        public void Constructor_WithValidIMO_9074729_ShouldPassCheckDigitValidation()
        {
            // Arrange: IMO 9074729
            // Check digit calculation: (9*7 + 0*6 + 7*5 + 4*4 + 7*3 + 2*2) % 10 = (63 + 0 + 35 + 16 + 21 + 4) % 10 = 139 % 10 = 9
            var imoNumber = "9074729";

            // Act
            var imo = new IMOnumber(imoNumber);

            // Assert
            imo.Number.Should().Be("9074729");
        }

        [Fact]
        public void Constructor_WithValidIMO_9305611_ShouldPassCheckDigitValidation()
        {
            // Arrange: IMO 9305611
            // Check digit calculation: (9*7 + 3*6 + 0*5 + 5*4 + 6*3 + 1*2) % 10 = (63 + 18 + 0 + 20 + 18 + 2) % 10 = 121 % 10 = 1
            var imoNumber = "9305611";

            // Act
            var imo = new IMOnumber(imoNumber);

            // Assert
            imo.Number.Should().Be("9305611");
        }

        #endregion

        #region Invalid IMOnumber Tests

        [Fact]
        public void Constructor_WithNull_ShouldThrowException()
        {
            // Arrange & Act
            Action act = () => new IMOnumber(null);

            // Assert
            act.Should().Throw<Exception>(); // Can be either NullReferenceException or InvalidOperationException
        }

        [Fact]
        public void Constructor_WithEmptyString_ShouldThrowInvalidOperationException()
        {
            // Arrange & Act
            Action act = () => new IMOnumber("");

            // Assert
            act.Should().Throw<InvalidOperationException>()
                .WithMessage("IMO number cannot be empty");
        }

        [Fact]
        public void Constructor_WithWhitespace_ShouldThrowInvalidOperationException()
        {
            // Arrange & Act
            Action act = () => new IMOnumber("   ");

            // Assert
            act.Should().Throw<InvalidOperationException>()
                .WithMessage("IMO number cannot be empty");
        }

        [Theory]
        [InlineData("123456")] // Too short
        [InlineData("12345678")] // Too long
        [InlineData("12345")] // Too short
        public void Constructor_WithWrongLength_ShouldThrowInvalidOperationException(string invalidIMO)
        {
            // Arrange & Act
            Action act = () => new IMOnumber(invalidIMO);

            // Assert
            act.Should().Throw<InvalidOperationException>()
                .WithMessage("IMO number must be exactly 7 digits");
        }

        [Theory]
        [InlineData("ABC1234")]
        [InlineData("1234ABC")]
        [InlineData("123X567")]
        public void Constructor_WithNonNumericCharacters_ShouldThrowInvalidOperationException(string invalidIMO)
        {
            // Arrange & Act
            Action act = () => new IMOnumber(invalidIMO);

            // Assert
            act.Should().Throw<InvalidOperationException>()
                .WithMessage("IMO number must contain only numeric digits");
        }

        [Fact]
        public void Constructor_WithInvalidCheckDigit_ShouldThrowInvalidOperationException()
        {
            // Arrange: IMO 9074728 (last digit should be 9, not 8)
            // Check digit calculation: (9*7 + 0*6 + 7*5 + 4*4 + 7*3 + 2*2) % 10 = 139 % 10 = 9
            var invalidIMO = "9074728";

            // Act
            Action act = () => new IMOnumber(invalidIMO);

            // Assert
            act.Should().Throw<InvalidOperationException>()
                .WithMessage("Invalid IMO check digit: expected 9, but got 8");
        }

        [Theory]
        [InlineData("9074720")] // Expected check digit: 9, got: 0
        [InlineData("9074721")] // Expected check digit: 9, got: 1
        [InlineData("9074725")] // Expected check digit: 9, got: 5
        public void Constructor_WithMultipleInvalidCheckDigits_ShouldThrowInvalidOperationException(string invalidIMO)
        {
            // Arrange & Act
            Action act = () => new IMOnumber(invalidIMO);

            // Assert
            act.Should().Throw<InvalidOperationException>()
                .WithMessage("Invalid IMO check digit*");
        }

        #endregion

        #region Equality Tests

        [Fact]
        public void Equals_WithSameIMONumber_ShouldReturnTrue()
        {
            // Arrange
            var imo1 = new IMOnumber("9074729");
            var imo2 = new IMOnumber("9074729");

            // Act & Assert
            imo1.Equals(imo2).Should().BeTrue();
        }

        [Fact]
        public void Equals_WithDifferentIMONumbers_ShouldReturnFalse()
        {
            // Arrange
            var imo1 = new IMOnumber("9074729");
            var imo2 = new IMOnumber("9195195");

            // Act & Assert
            imo1.Equals(imo2).Should().BeFalse();
        }

        [Fact]
        public void GetHashCode_WithSameIMONumber_ShouldReturnSameHashCode()
        {
            // Arrange
            var imo1 = new IMOnumber("9074729");
            var imo2 = new IMOnumber("9074729");

            // Act & Assert
            imo1.GetHashCode().Should().Be(imo2.GetHashCode());
        }

        [Fact]
        public void GetHashCode_WithDifferentIMONumbers_ShouldReturnDifferentHashCodes()
        {
            // Arrange
            var imo1 = new IMOnumber("9074729");
            var imo2 = new IMOnumber("9195195");

            // Act & Assert
            imo1.GetHashCode().Should().NotBe(imo2.GetHashCode());
        }

        #endregion

        #region Edge Cases

        [Fact]
        public void Constructor_WithAllZerosExceptCheckDigit_ShouldValidateCorrectly()
        {
            // Arrange: IMO 0000003 (all zeros except check digit)
            // Check digit calculation: (0*7 + 0*6 + 0*5 + 0*4 + 0*3 + 0*2) % 10 = 0 % 10 = 0
            // So valid would be 0000000
            var validIMO = "0000000";

            // Act
            var imo = new IMOnumber(validIMO);

            // Assert
            imo.Number.Should().Be("0000000");
        }

        [Fact]
        public void Constructor_WithAllNines_ShouldValidateCheckDigit()
        {
            // Arrange: Calculate proper check digit for 999999X
            // Check digit calculation: (9*7 + 9*6 + 9*5 + 9*4 + 9*3 + 9*2) % 10 = (63 + 54 + 45 + 36 + 27 + 18) % 10 = 243 % 10 = 3
            var validIMO = "9999993";

            // Act
            var imo = new IMOnumber(validIMO);

            // Assert
            imo.Number.Should().Be("9999993");
        }

        [Fact]
        public void Number_Property_ShouldBeReadOnly()
        {
            // Arrange
            var imo = new IMOnumber("9074729");

            // Act - The Number property should not have a public setter
            var propertyInfo = typeof(IMOnumber).GetProperty(nameof(IMOnumber.Number));

            // Assert
            propertyInfo.Should().NotBeNull();
            propertyInfo.SetMethod.Should().NotBeNull();
            propertyInfo.SetMethod.IsPublic.Should().BeFalse();
        }

        #endregion

        #region Real-World IMO Numbers

        [Theory]
        [InlineData("9074729")] // Large container vessel
        [InlineData("9195195")] // Large container vessel
        [InlineData("8814275")] // Older vessel
        public void Constructor_WithRealWorldIMONumbers_ShouldCreateValidIMOnumber(string realIMO)
        {
            // Arrange & Act
            var imo = new IMOnumber(realIMO);

            // Assert
            imo.Number.Should().Be(realIMO);
            imo.Number.Length.Should().Be(7);
        }

        [Fact]
        public void Constructor_WithIMO_9074729_ShouldRepresentValidVessel()
        {
            // Arrange - This represents a valid IMO number format
            var imoNumber = "9074729";

            // Act
            var imo = new IMOnumber(imoNumber);

            // Assert
            imo.Number.Should().Be("9074729");
            imo.AsString().Should().Be("9074729");
        }

        #endregion
    }
}
