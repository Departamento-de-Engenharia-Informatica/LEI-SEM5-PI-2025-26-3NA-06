using FluentAssertions;
using ProjArqsi.Domain.Shared;
using Xunit;

namespace ProjArqsi.Tests.ValueObjectTests.Shared
{
    // Concrete implementations for testing
    public class TestValueObject : ValueObject
    {
        public string Property1 { get; }
        public int Property2 { get; }

        public TestValueObject(string property1, int property2)
        {
            Property1 = property1;
            Property2 = property2;
        }

        protected override IEnumerable<object> GetEqualityComponents()
        {
            yield return Property1;
            yield return Property2;
        }
    }

    public class SinglePropertyValueObject : ValueObject
    {
        public string Value { get; }

        public SinglePropertyValueObject(string value)
        {
            Value = value;
        }

        protected override IEnumerable<object> GetEqualityComponents()
        {
            yield return Value;
        }
    }

    public class ComplexValueObject : ValueObject
    {
        public string StringProperty { get; }
        public int IntProperty { get; }
        public bool BoolProperty { get; }
        public double DoubleProperty { get; }

        public ComplexValueObject(string stringProperty, int intProperty, bool boolProperty, double doubleProperty)
        {
            StringProperty = stringProperty;
            IntProperty = intProperty;
            BoolProperty = boolProperty;
            DoubleProperty = doubleProperty;
        }

        protected override IEnumerable<object> GetEqualityComponents()
        {
            yield return StringProperty;
            yield return IntProperty;
            yield return BoolProperty;
            yield return DoubleProperty;
        }
    }

    public class EmptyValueObject : ValueObject
    {
        protected override IEnumerable<object> GetEqualityComponents()
        {
            yield break;
        }
    }

    public class ValueObjectTests
    {
        #region Equals Tests

        [Fact]
        public void Equals_WithSameValues_ShouldReturnTrue()
        {
            // Arrange
            var vo1 = new TestValueObject("test", 42);
            var vo2 = new TestValueObject("test", 42);

            // Act
            var result = vo1.Equals(vo2);

            // Assert
            result.Should().BeTrue();
        }

        [Fact]
        public void Equals_WithDifferentFirstProperty_ShouldReturnFalse()
        {
            // Arrange
            var vo1 = new TestValueObject("test1", 42);
            var vo2 = new TestValueObject("test2", 42);

            // Act
            var result = vo1.Equals(vo2);

            // Assert
            result.Should().BeFalse();
        }

        [Fact]
        public void Equals_WithDifferentSecondProperty_ShouldReturnFalse()
        {
            // Arrange
            var vo1 = new TestValueObject("test", 42);
            var vo2 = new TestValueObject("test", 43);

            // Act
            var result = vo1.Equals(vo2);

            // Assert
            result.Should().BeFalse();
        }

        [Fact]
        public void Equals_WithNull_ShouldReturnFalse()
        {
            // Arrange
            var vo = new TestValueObject("test", 42);

            // Act
            var result = vo.Equals(null);

            // Assert
            result.Should().BeFalse();
        }

        [Fact]
        public void Equals_WithDifferentType_ShouldReturnFalse()
        {
            // Arrange
            var vo1 = new TestValueObject("test", 42);
            var vo2 = new SinglePropertyValueObject("test");

            // Act
            var result = vo1.Equals(vo2);

            // Assert
            result.Should().BeFalse();
        }

        [Fact]
        public void Equals_WithSameReference_ShouldReturnTrue()
        {
            // Arrange
            var vo = new TestValueObject("test", 42);

            // Act
            var result = vo.Equals(vo);

            // Assert
            result.Should().BeTrue();
        }

        [Fact]
        public void Equals_WithComplexObject_ShouldCompareAllProperties()
        {
            // Arrange
            var vo1 = new ComplexValueObject("test", 42, true, 3.14);
            var vo2 = new ComplexValueObject("test", 42, true, 3.14);

            // Act
            var result = vo1.Equals(vo2);

            // Assert
            result.Should().BeTrue();
        }

        [Fact]
        public void Equals_WithComplexObjectDifferentLastProperty_ShouldReturnFalse()
        {
            // Arrange
            var vo1 = new ComplexValueObject("test", 42, true, 3.14);
            var vo2 = new ComplexValueObject("test", 42, true, 3.15);

            // Act
            var result = vo1.Equals(vo2);

            // Assert
            result.Should().BeFalse();
        }

        [Fact]
        public void Equals_WithSingleProperty_ShouldWork()
        {
            // Arrange
            var vo1 = new SinglePropertyValueObject("test");
            var vo2 = new SinglePropertyValueObject("test");

            // Act
            var result = vo1.Equals(vo2);

            // Assert
            result.Should().BeTrue();
        }

        [Fact]
        public void Equals_WithEmptyValueObjects_ShouldReturnTrue()
        {
            // Arrange
            var vo1 = new EmptyValueObject();
            var vo2 = new EmptyValueObject();

            // Act
            var result = vo1.Equals(vo2);

            // Assert
            result.Should().BeTrue();
        }

        #endregion

        #region GetHashCode Tests

        [Fact]
        public void GetHashCode_WithSameValues_ShouldReturnSameHashCode()
        {
            // Arrange
            var vo1 = new TestValueObject("test", 42);
            var vo2 = new TestValueObject("test", 42);

            // Act
            var hash1 = vo1.GetHashCode();
            var hash2 = vo2.GetHashCode();

            // Assert
            hash1.Should().Be(hash2);
        }

        [Fact]
        public void GetHashCode_WithDifferentValues_ShouldReturnDifferentHashCode()
        {
            // Arrange
            var vo1 = new TestValueObject("test1", 42);
            var vo2 = new TestValueObject("test2", 42);

            // Act
            var hash1 = vo1.GetHashCode();
            var hash2 = vo2.GetHashCode();

            // Assert
            hash1.Should().NotBe(hash2);
        }

        [Fact]
        public void GetHashCode_CalledMultipleTimes_ShouldReturnSameValue()
        {
            // Arrange
            var vo = new TestValueObject("test", 42);

            // Act
            var hash1 = vo.GetHashCode();
            var hash2 = vo.GetHashCode();

            // Assert
            hash1.Should().Be(hash2);
        }

        [Fact]
        public void GetHashCode_WithComplexObject_ShouldBeConsistent()
        {
            // Arrange
            var vo1 = new ComplexValueObject("test", 42, true, 3.14);
            var vo2 = new ComplexValueObject("test", 42, true, 3.14);

            // Act
            var hash1 = vo1.GetHashCode();
            var hash2 = vo2.GetHashCode();

            // Assert
            hash1.Should().Be(hash2);
        }

        [Fact]
        public void GetHashCode_WithSingleProperty_ShouldWork()
        {
            // Arrange
            var vo1 = new SinglePropertyValueObject("test");
            var vo2 = new SinglePropertyValueObject("test");

            // Act
            var hash1 = vo1.GetHashCode();
            var hash2 = vo2.GetHashCode();

            // Assert
            hash1.Should().Be(hash2);
        }

        [Fact]
        public void GetHashCode_WithEmptyValueObjects_ShouldReturnSameHashCode()
        {
            // Arrange
            var vo1 = new EmptyValueObject();
            var vo2 = new EmptyValueObject();

            // Act
            var hash1 = vo1.GetHashCode();
            var hash2 = vo2.GetHashCode();

            // Assert
            hash1.Should().Be(hash2);
        }

        [Fact]
        public void GetHashCode_WithDifferentPropertiesOrder_ShouldReturnDifferentHashCode()
        {
            // Arrange
            var vo1 = new TestValueObject("A", 1);
            var vo2 = new TestValueObject("B", 2);

            // Act
            var hash1 = vo1.GetHashCode();
            var hash2 = vo2.GetHashCode();

            // Assert
            hash1.Should().NotBe(hash2);
        }

        #endregion

        #region Edge Cases

        [Fact]
        public void Equals_WithArbitraryObject_ShouldReturnFalse()
        {
            // Arrange
            var vo = new TestValueObject("test", 42);
            var arbitraryObject = new { Property1 = "test", Property2 = 42 };

            // Act
            var result = vo.Equals(arbitraryObject);

            // Assert
            result.Should().BeFalse();
        }

        [Fact]
        public void Equals_WithStringObject_ShouldReturnFalse()
        {
            // Arrange
            var vo = new SinglePropertyValueObject("test");
            var stringObject = "test";

            // Act
            var result = vo.Equals(stringObject);

            // Assert
            result.Should().BeFalse();
        }

        [Fact]
        public void GetHashCode_WithZeroValues_ShouldWork()
        {
            // Arrange
            var vo1 = new TestValueObject("", 0);
            var vo2 = new TestValueObject("", 0);

            // Act
            var hash1 = vo1.GetHashCode();
            var hash2 = vo2.GetHashCode();

            // Assert
            hash1.Should().Be(hash2);
        }

        [Fact]
        public void GetHashCode_WithNegativeValues_ShouldWork()
        {
            // Arrange
            var vo1 = new TestValueObject("test", -42);
            var vo2 = new TestValueObject("test", -42);

            // Act
            var hash1 = vo1.GetHashCode();
            var hash2 = vo2.GetHashCode();

            // Assert
            hash1.Should().Be(hash2);
        }

        [Fact]
        public void Equals_WithDifferentBooleanValue_ShouldReturnFalse()
        {
            // Arrange
            var vo1 = new ComplexValueObject("test", 42, true, 3.14);
            var vo2 = new ComplexValueObject("test", 42, false, 3.14);

            // Act
            var result = vo1.Equals(vo2);

            // Assert
            result.Should().BeFalse();
        }

        [Fact]
        public void Equals_WithVeryCloseDoubleValues_ShouldReturnFalse()
        {
            // Arrange
            var vo1 = new ComplexValueObject("test", 42, true, 3.14159265358979);
            var vo2 = new ComplexValueObject("test", 42, true, 3.14159265358978);

            // Act
            var result = vo1.Equals(vo2);

            // Assert
            result.Should().BeFalse("even tiny differences in equality components should make objects unequal");
        }

        [Theory]
        [InlineData("")]
        [InlineData(" ")]
        [InlineData("a")]
        [InlineData("A very long string value for testing purposes")]
        public void Equals_WithVariousStringValues_ShouldWorkCorrectly(string value)
        {
            // Arrange
            var vo1 = new SinglePropertyValueObject(value);
            var vo2 = new SinglePropertyValueObject(value);

            // Act
            var result = vo1.Equals(vo2);

            // Assert
            result.Should().BeTrue();
        }

        [Theory]
        [InlineData(0)]
        [InlineData(1)]
        [InlineData(-1)]
        [InlineData(int.MaxValue)]
        [InlineData(int.MinValue)]
        public void Equals_WithVariousIntegerValues_ShouldWorkCorrectly(int value)
        {
            // Arrange
            var vo1 = new TestValueObject("test", value);
            var vo2 = new TestValueObject("test", value);

            // Act
            var result = vo1.Equals(vo2);

            // Assert
            result.Should().BeTrue();
        }

        #endregion

        #region HashCode Distribution Tests

        [Fact]
        public void GetHashCode_WithManyDifferentObjects_ShouldProduceDifferentHashes()
        {
            // Arrange
            var hashCodes = new HashSet<int>();
            var objects = new List<TestValueObject>();

            for (int i = 0; i < 100; i++)
            {
                objects.Add(new TestValueObject($"test{i}", i));
            }

            // Act
            foreach (var obj in objects)
            {
                hashCodes.Add(obj.GetHashCode());
            }

            // Assert
            hashCodes.Count.Should().BeGreaterThan(90, "hash codes should be well distributed");
        }

        [Fact]
        public void GetHashCode_WithSimilarValues_ShouldBeDifferent()
        {
            // Arrange
            var vo1 = new TestValueObject("test1", 42);
            var vo2 = new TestValueObject("test2", 42);
            var vo3 = new TestValueObject("test1", 43);

            // Act
            var hash1 = vo1.GetHashCode();
            var hash2 = vo2.GetHashCode();
            var hash3 = vo3.GetHashCode();

            // Assert
            hash1.Should().NotBe(hash2);
            hash1.Should().NotBe(hash3);
            hash2.Should().NotBe(hash3);
        }

        #endregion
    }
}
