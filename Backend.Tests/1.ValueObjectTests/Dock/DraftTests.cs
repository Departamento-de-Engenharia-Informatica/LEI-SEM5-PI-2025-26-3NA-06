using FluentAssertions;
using ProjArqsi.Domain.DockAggregate;
using ProjArqsi.Domain.Shared;
using Xunit;

namespace ProjArqsi.Tests._1_ValueObjectTests.Dock
{
    public class DraftTests
    {
        #region Valid Creation Tests

        [Theory]
        [InlineData(1.0)]
        [InlineData(8.5)]
        [InlineData(15.0)]
        [InlineData(20.75)]
        [InlineData(0.1)]
        public void CreateDraft_WithValidDraft_ShouldSucceed(double validDraft)
        {
            // Act
            var draft = new Draft(validDraft);

            // Assert
            draft.Should().NotBeNull();
            draft.Value.Should().Be(validDraft);
        }

        [Fact]
        public void CreateDraft_WithVerySmallPositiveValue_ShouldSucceed()
        {
            // Arrange
            var verySmall = 0.001;

            // Act
            var draft = new Draft(verySmall);

            // Assert
            draft.Value.Should().Be(0.001);
        }

        [Fact]
        public void CreateDraft_WithLargeValue_ShouldSucceed()
        {
            // Arrange
            var largeValue = 50.0;

            // Act
            var draft = new Draft(largeValue);

            // Assert
            draft.Value.Should().Be(50.0);
        }

        [Fact]
        public void CreateDraft_WithPreciseDecimal_ShouldPreservePrecision()
        {
            // Arrange
            var preciseValue = 12.456789;

            // Act
            var draft = new Draft(preciseValue);

            // Assert
            draft.Value.Should().BeApproximately(12.456789, 0.000001);
        }

        #endregion

        #region Invalid Creation Tests

        [Fact]
        public void CreateDraft_WithZero_ShouldThrowBusinessRuleValidationException()
        {
            // Arrange
            var zeroDraft = 0.0;

            // Act
            Action act = () => new Draft(zeroDraft);

            // Assert
            act.Should().Throw<BusinessRuleValidationException>()
                .WithMessage("*must be greater than 0*");
        }

        [Theory]
        [InlineData(-1.0)]
        [InlineData(-8.5)]
        [InlineData(-0.1)]
        public void CreateDraft_WithNegativeValue_ShouldThrowBusinessRuleValidationException(double negativeDraft)
        {
            // Act
            Action act = () => new Draft(negativeDraft);

            // Assert
            act.Should().Throw<BusinessRuleValidationException>()
                .WithMessage("*must be greater than 0*");
        }

        [Fact]
        public void CreateDraft_WithVeryLargeNegative_ShouldThrowBusinessRuleValidationException()
        {
            // Arrange
            var veryLargeNegative = -9999.99;

            // Act
            Action act = () => new Draft(veryLargeNegative);

            // Assert
            act.Should().Throw<BusinessRuleValidationException>()
                .WithMessage("*must be greater than 0*");
        }

        #endregion

        #region Equality Tests

        [Fact]
        public void Draft_WithSameValue_ShouldBeEqual()
        {
            // Arrange
            var draft1 = new Draft(12.5);
            var draft2 = new Draft(12.5);

            // Act & Assert
            draft1.Should().Be(draft2);
            draft1.GetHashCode().Should().Be(draft2.GetHashCode());
        }

        [Fact]
        public void Draft_WithDifferentValues_ShouldNotBeEqual()
        {
            // Arrange
            var draft1 = new Draft(12.5);
            var draft2 = new Draft(15.5);

            // Act & Assert
            draft1.Should().NotBe(draft2);
            draft1.GetHashCode().Should().NotBe(draft2.GetHashCode());
        }

        [Fact]
        public void Draft_ComparedToNull_ShouldNotBeEqual()
        {
            // Arrange
            var draft = new Draft(12.0);

            // Act & Assert
            draft.Should().NotBe(null);
        }

        [Fact]
        public void Draft_WithSlightlyDifferentValues_ShouldNotBeEqual()
        {
            // Arrange
            var draft1 = new Draft(12.001);
            var draft2 = new Draft(12.002);

            // Act & Assert
            draft1.Should().NotBe(draft2);
        }

        #endregion

        #region Edge Case Tests

        [Fact]
        public void CreateDraft_WithMinimumPositiveValue_ShouldSucceed()
        {
            // Arrange
            var minPositive = double.Epsilon;

            // Act
            var draft = new Draft(minPositive);

            // Assert
            draft.Value.Should().Be(double.Epsilon);
        }

        [Fact]
        public void CreateDraft_WithTypicalContainerShipDraft_ShouldSucceed()
        {
            // Arrange - Typical container ship draft in meters
            var typicalDraft = 14.0;

            // Act
            var draft = new Draft(typicalDraft);

            // Assert
            draft.Value.Should().Be(14.0);
        }

        [Fact]
        public void CreateDraft_WithSmallVesselDraft_ShouldSucceed()
        {
            // Arrange - Small vessel draft
            var smallDraft = 3.5;

            // Act
            var draft = new Draft(smallDraft);

            // Assert
            draft.Value.Should().Be(3.5);
        }

        [Fact]
        public void CreateDraft_WithLargeVesselDraft_ShouldSucceed()
        {
            // Arrange - Large vessel draft
            var largeDraft = 22.0;

            // Act
            var draft = new Draft(largeDraft);

            // Assert
            draft.Value.Should().Be(22.0);
        }

        [Fact]
        public void CreateDraft_WithOneDecimalPlace_ShouldPreservePrecision()
        {
            // Arrange
            var oneDecimal = 10.5;

            // Act
            var draft = new Draft(oneDecimal);

            // Assert
            draft.Value.Should().Be(10.5);
        }

        #endregion
    }
}
