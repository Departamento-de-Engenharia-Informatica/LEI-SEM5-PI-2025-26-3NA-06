using FluentAssertions;
using ProjArqsi.Domain.Shared;
using Xunit;

namespace ProjArqsi.Tests.ValueObjectTests.Shared
{
    // Concrete implementation for testing
    public class TestEntityId : EntityId
    {
        public TestEntityId(Guid value) : base(value) { }
        public TestEntityId(string value) : base(value) { }
    }

    public class AnotherTestEntityId : EntityId
    {
        public AnotherTestEntityId(Guid value) : base(value) { }
    }

    public class EntityIdTests
    {
        #region Constructor Tests

        [Fact]
        public void Constructor_WithGuid_ShouldCreateEntityId()
        {
            // Arrange
            var guid = Guid.NewGuid();

            // Act
            var entityId = new TestEntityId(guid);

            // Assert
            entityId.Should().NotBeNull();
            entityId.Value.Should().Be(guid.ToString());
        }

        [Fact]
        public void Constructor_WithString_ShouldCreateEntityId()
        {
            // Arrange
            var guid = Guid.NewGuid();
            var guidString = guid.ToString();

            // Act
            var entityId = new TestEntityId(guidString);

            // Assert
            entityId.Should().NotBeNull();
            entityId.Value.Should().Be(guidString);
        }

        [Fact]
        public void Constructor_WithValidGuidString_ShouldParseCorrectly()
        {
            // Arrange
            var guidString = "a1b2c3d4-e5f6-7890-abcd-ef1234567890";

            // Act
            var entityId = new TestEntityId(guidString);

            // Assert
            entityId.Value.Should().Be(guidString);
        }

        [Fact]
        public void Constructor_WithInvalidGuidString_ShouldThrowException()
        {
            // Arrange
            var invalidString = "not-a-guid";

            // Act
            Action act = () => new TestEntityId(invalidString);

            // Assert
            act.Should().Throw<FormatException>();
        }

        #endregion

        #region AsString Tests

        [Fact]
        public void AsString_WithGuidValue_ShouldReturnStringRepresentation()
        {
            // Arrange
            var guid = Guid.NewGuid();
            var entityId = new TestEntityId(guid);

            // Act
            var result = entityId.AsString();

            // Assert
            result.Should().Be(guid.ToString());
        }

        [Fact]
        public void AsString_WithStringValue_ShouldReturnSameString()
        {
            // Arrange
            var guidString = Guid.NewGuid().ToString();
            var entityId = new TestEntityId(guidString);

            // Act
            var result = entityId.AsString();

            // Assert
            result.Should().Be(guidString);
        }

        #endregion

        #region Value Property Tests

        [Fact]
        public void Value_WithGuidConstructor_ShouldReturnStringValue()
        {
            // Arrange
            var guid = Guid.NewGuid();
            var entityId = new TestEntityId(guid);

            // Act
            var value = entityId.Value;

            // Assert
            value.Should().Be(guid.ToString());
        }

        [Fact]
        public void Value_WithStringConstructor_ShouldReturnStringValue()
        {
            // Arrange
            var guidString = Guid.NewGuid().ToString();
            var entityId = new TestEntityId(guidString);

            // Act
            var value = entityId.Value;

            // Assert
            value.Should().Be(guidString);
        }

        #endregion

        #region Equality Tests

        [Fact]
        public void Equals_WithSameValue_ShouldReturnTrue()
        {
            // Arrange
            var guid = Guid.NewGuid();
            var entityId1 = new TestEntityId(guid);
            var entityId2 = new TestEntityId(guid);

            // Act
            var result = entityId1.Equals(entityId2);

            // Assert
            result.Should().BeTrue();
        }

        [Fact]
        public void Equals_WithDifferentValue_ShouldReturnFalse()
        {
            // Arrange
            var entityId1 = new TestEntityId(Guid.NewGuid());
            var entityId2 = new TestEntityId(Guid.NewGuid());

            // Act
            var result = entityId1.Equals(entityId2);

            // Assert
            result.Should().BeFalse();
        }

        [Fact]
        public void Equals_WithNull_ShouldReturnFalse()
        {
            // Arrange
            var entityId = new TestEntityId(Guid.NewGuid());

            // Act
            var result = entityId.Equals((EntityId?)null);

            // Assert
            result.Should().BeFalse();
        }

        [Fact]
        public void Equals_WithNullObject_ShouldReturnFalse()
        {
            // Arrange
            var entityId = new TestEntityId(Guid.NewGuid());

            // Act
            var result = entityId.Equals((object?)null);

            // Assert
            result.Should().BeFalse();
        }

        [Fact]
        public void Equals_WithDifferentType_ShouldReturnFalse()
        {
            // Arrange
            var guid = Guid.NewGuid();
            var entityId1 = new TestEntityId(guid);
            var entityId2 = new AnotherTestEntityId(guid);

            // Act
            var result = entityId1.Equals(entityId2);

            // Assert
            result.Should().BeFalse();
        }

        [Fact]
        public void Equals_WithSameReference_ShouldReturnTrue()
        {
            // Arrange
            var entityId = new TestEntityId(Guid.NewGuid());

            // Act
            var result = entityId.Equals(entityId);

            // Assert
            result.Should().BeTrue();
        }

        [Fact]
        public void Equals_WithStringAndGuidConstructors_ShouldReturnTrue()
        {
            // Arrange
            var guid = Guid.NewGuid();
            var entityId1 = new TestEntityId(guid);
            var entityId2 = new TestEntityId(guid.ToString());

            // Act
            var result = entityId1.Equals(entityId2);

            // Assert
            result.Should().BeTrue();
        }

        #endregion

        #region Equality Operator Tests

        [Fact]
        public void EqualityOperator_WithSameValue_ShouldReturnTrue()
        {
            // Arrange
            var guid = Guid.NewGuid();
            var entityId1 = new TestEntityId(guid);
            var entityId2 = new TestEntityId(guid);

            // Act
            var result = entityId1 == entityId2;

            // Assert
            result.Should().BeTrue();
        }

        [Fact]
        public void EqualityOperator_WithDifferentValue_ShouldReturnFalse()
        {
            // Arrange
            var entityId1 = new TestEntityId(Guid.NewGuid());
            var entityId2 = new TestEntityId(Guid.NewGuid());

            // Act
            var result = entityId1 == entityId2;

            // Assert
            result.Should().BeFalse();
        }

        [Fact]
        public void EqualityOperator_WithBothNull_ShouldReturnTrue()
        {
            // Arrange
            TestEntityId? entityId1 = null;
            TestEntityId? entityId2 = null;

            // Act
            var result = entityId1 == entityId2;

            // Assert
            result.Should().BeTrue();
        }

        [Fact]
        public void EqualityOperator_WithOneNull_ShouldReturnFalse()
        {
            // Arrange
            var entityId1 = new TestEntityId(Guid.NewGuid());
            TestEntityId? entityId2 = null;

            // Act
            var result = entityId1 == entityId2;

            // Assert
            result.Should().BeFalse();
        }

        [Fact]
        public void InequalityOperator_WithDifferentValue_ShouldReturnTrue()
        {
            // Arrange
            var entityId1 = new TestEntityId(Guid.NewGuid());
            var entityId2 = new TestEntityId(Guid.NewGuid());

            // Act
            var result = entityId1 != entityId2;

            // Assert
            result.Should().BeTrue();
        }

        [Fact]
        public void InequalityOperator_WithSameValue_ShouldReturnFalse()
        {
            // Arrange
            var guid = Guid.NewGuid();
            var entityId1 = new TestEntityId(guid);
            var entityId2 = new TestEntityId(guid);

            // Act
            var result = entityId1 != entityId2;

            // Assert
            result.Should().BeFalse();
        }

        [Fact]
        public void InequalityOperator_WithBothNull_ShouldReturnFalse()
        {
            // Arrange
            TestEntityId? entityId1 = null;
            TestEntityId? entityId2 = null;

            // Act
            var result = entityId1 != entityId2;

            // Assert
            result.Should().BeFalse();
        }

        #endregion

        #region GetHashCode Tests

        [Fact]
        public void GetHashCode_WithSameValue_ShouldReturnSameHashCode()
        {
            // Arrange
            var guid = Guid.NewGuid();
            var entityId1 = new TestEntityId(guid);
            var entityId2 = new TestEntityId(guid);

            // Act
            var hash1 = entityId1.GetHashCode();
            var hash2 = entityId2.GetHashCode();

            // Assert
            hash1.Should().Be(hash2);
        }

        [Fact]
        public void GetHashCode_WithDifferentValue_ShouldReturnDifferentHashCode()
        {
            // Arrange
            var entityId1 = new TestEntityId(Guid.NewGuid());
            var entityId2 = new TestEntityId(Guid.NewGuid());

            // Act
            var hash1 = entityId1.GetHashCode();
            var hash2 = entityId2.GetHashCode();

            // Assert
            hash1.Should().NotBe(hash2);
        }

        [Fact]
        public void GetHashCode_WithStringAndGuidConstructors_ShouldReturnSameHashCode()
        {
            // Arrange
            var guid = Guid.NewGuid();
            var entityId1 = new TestEntityId(guid);
            var entityId2 = new TestEntityId(guid.ToString());

            // Act
            var hash1 = entityId1.GetHashCode();
            var hash2 = entityId2.GetHashCode();

            // Assert
            hash1.Should().Be(hash2);
        }

        #endregion

        #region CompareTo Tests

        [Fact]
        public void CompareTo_WithNull_ShouldReturnNegativeOne()
        {
            // Arrange
            var entityId = new TestEntityId(Guid.NewGuid());

            // Act
            var result = entityId.CompareTo(null);

            // Assert
            result.Should().Be(-1);
        }

        [Fact]
        public void CompareTo_WithSameValue_ShouldReturnZero()
        {
            // Arrange
            var guid = Guid.NewGuid();
            var entityId1 = new TestEntityId(guid);
            var entityId2 = new TestEntityId(guid);

            // Act
            var result = entityId1.CompareTo(entityId2);

            // Assert
            result.Should().Be(0);
        }

        [Fact]
        public void CompareTo_WithLexicographicallyGreaterValue_ShouldReturnPositive()
        {
            // Arrange
            var entityId1 = new TestEntityId("b0000000-0000-0000-0000-000000000000");
            var entityId2 = new TestEntityId("a0000000-0000-0000-0000-000000000000");

            // Act
            var result = entityId1.CompareTo(entityId2);

            // Assert
            result.Should().BePositive();
        }

        [Fact]
        public void CompareTo_WithLexicographicallySmallerValue_ShouldReturnNegative()
        {
            // Arrange
            var entityId1 = new TestEntityId("a0000000-0000-0000-0000-000000000000");
            var entityId2 = new TestEntityId("b0000000-0000-0000-0000-000000000000");

            // Act
            var result = entityId1.CompareTo(entityId2);

            // Assert
            result.Should().BeNegative();
        }

        #endregion

        #region Edge Cases

        [Fact]
        public void Equals_WithSameValueDifferentTypes_ShouldReturnFalse()
        {
            // Arrange
            var guid = Guid.NewGuid();
            var entityId1 = new TestEntityId(guid);
            var entityId2 = new AnotherTestEntityId(guid);

            // Act
            var result = entityId1.Equals(entityId2);

            // Assert
            result.Should().BeFalse("different EntityId types should not be equal even with same value");
        }

        [Fact]
        public void Equals_WithArbitraryObject_ShouldReturnFalse()
        {
            // Arrange
            var entityId = new TestEntityId(Guid.NewGuid());
            var arbitraryObject = new { Id = "test" };

            // Act
            var result = entityId.Equals(arbitraryObject);

            // Assert
            result.Should().BeFalse();
        }

        [Fact]
        public void Value_AfterConstruction_ShouldBeConsistent()
        {
            // Arrange
            var guid = Guid.NewGuid();
            var entityId = new TestEntityId(guid);

            // Act
            var value1 = entityId.Value;
            var value2 = entityId.Value;

            // Assert
            value1.Should().Be(value2);
        }

        [Fact]
        public void AsString_CalledMultipleTimes_ShouldReturnSameValue()
        {
            // Arrange
            var guid = Guid.NewGuid();
            var entityId = new TestEntityId(guid);

            // Act
            var result1 = entityId.AsString();
            var result2 = entityId.AsString();

            // Assert
            result1.Should().Be(result2);
        }

        #endregion
    }
}
