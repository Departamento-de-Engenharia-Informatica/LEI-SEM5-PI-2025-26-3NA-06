using FluentAssertions;
using ProjArqsi.Domain.StorageAreaAggregate;
using Xunit;

namespace ProjArqsi.Tests.ValueObjectTests.StorageArea
{
    public class AreaTypeTests
    {
        #region Valid Area Type Tests

        [Fact]
        public void Constructor_WithYardType_ShouldCreateAreaType()
        {
            // Act
            var areaType = new AreaType(AreaTypeEnum.Yard);

            // Assert
            areaType.Should().NotBeNull();
            areaType.Value.Should().Be(AreaTypeEnum.Yard);
        }

        [Fact]
        public void Constructor_WithWarehouseType_ShouldCreateAreaType()
        {
            // Act
            var areaType = new AreaType(AreaTypeEnum.Warehouse);

            // Assert
            areaType.Should().NotBeNull();
            areaType.Value.Should().Be(AreaTypeEnum.Warehouse);
        }

        [Theory]
        [InlineData(AreaTypeEnum.Yard)]
        [InlineData(AreaTypeEnum.Warehouse)]
        public void Constructor_WithAllValidTypes_ShouldCreateAreaType(AreaTypeEnum type)
        {
            // Act
            var areaType = new AreaType(type);

            // Assert
            areaType.Value.Should().Be(type);
        }

        #endregion

        #region Static Factory Tests

        [Fact]
        public void AreaTypes_Yard_ShouldReturnYardType()
        {
            // Act
            var areaType = AreaTypes.Yard;

            // Assert
            areaType.Should().NotBeNull();
            areaType.Value.Should().Be(AreaTypeEnum.Yard);
        }

        [Fact]
        public void AreaTypes_Warehouse_ShouldReturnWarehouseType()
        {
            // Act
            var areaType = AreaTypes.Warehouse;

            // Assert
            areaType.Should().NotBeNull();
            areaType.Value.Should().Be(AreaTypeEnum.Warehouse);
        }

        [Fact]
        public void AreaTypes_Yard_CalledMultipleTimes_ShouldReturnEqualObjects()
        {
            // Act
            var type1 = AreaTypes.Yard;
            var type2 = AreaTypes.Yard;

            // Assert
            type1.Should().Be(type2);
        }

        [Fact]
        public void AreaTypes_Warehouse_CalledMultipleTimes_ShouldReturnEqualObjects()
        {
            // Act
            var type1 = AreaTypes.Warehouse;
            var type2 = AreaTypes.Warehouse;

            // Assert
            type1.Should().Be(type2);
        }

        #endregion

        #region ToString Tests

        [Fact]
        public void ToString_WithYardType_ShouldReturnYard()
        {
            // Arrange
            var areaType = new AreaType(AreaTypeEnum.Yard);

            // Act
            var result = areaType.ToString();

            // Assert
            result.Should().Be("Yard");
        }

        [Fact]
        public void ToString_WithWarehouseType_ShouldReturnWarehouse()
        {
            // Arrange
            var areaType = new AreaType(AreaTypeEnum.Warehouse);

            // Act
            var result = areaType.ToString();

            // Assert
            result.Should().Be("Warehouse");
        }

        [Theory]
        [InlineData(AreaTypeEnum.Yard, "Yard")]
        [InlineData(AreaTypeEnum.Warehouse, "Warehouse")]
        public void ToString_WithAllTypes_ShouldReturnCorrectString(AreaTypeEnum type, string expected)
        {
            // Arrange
            var areaType = new AreaType(type);

            // Act
            var result = areaType.ToString();

            // Assert
            result.Should().Be(expected);
        }

        #endregion

        #region Equality Tests

        [Fact]
        public void Equals_WithSameYardType_ShouldReturnTrue()
        {
            // Arrange
            var type1 = new AreaType(AreaTypeEnum.Yard);
            var type2 = new AreaType(AreaTypeEnum.Yard);

            // Act
            var result = type1.Equals(type2);

            // Assert
            result.Should().BeTrue();
        }

        [Fact]
        public void Equals_WithSameWarehouseType_ShouldReturnTrue()
        {
            // Arrange
            var type1 = new AreaType(AreaTypeEnum.Warehouse);
            var type2 = new AreaType(AreaTypeEnum.Warehouse);

            // Act
            var result = type1.Equals(type2);

            // Assert
            result.Should().BeTrue();
        }

        [Fact]
        public void Equals_WithDifferentTypes_ShouldReturnFalse()
        {
            // Arrange
            var type1 = new AreaType(AreaTypeEnum.Yard);
            var type2 = new AreaType(AreaTypeEnum.Warehouse);

            // Act
            var result = type1.Equals(type2);

            // Assert
            result.Should().BeFalse();
        }

        [Fact]
        public void Equals_UsingStaticFactories_ShouldWork()
        {
            // Arrange
            var type1 = AreaTypes.Yard;
            var type2 = new AreaType(AreaTypeEnum.Yard);

            // Act
            var result = type1.Equals(type2);

            // Assert
            result.Should().BeTrue();
        }

        [Fact]
        public void GetHashCode_WithSameType_ShouldReturnSameHashCode()
        {
            // Arrange
            var type1 = new AreaType(AreaTypeEnum.Yard);
            var type2 = new AreaType(AreaTypeEnum.Yard);

            // Act
            var hash1 = type1.GetHashCode();
            var hash2 = type2.GetHashCode();

            // Assert
            hash1.Should().Be(hash2);
        }

        [Fact]
        public void GetHashCode_WithDifferentTypes_ShouldReturnDifferentHashCode()
        {
            // Arrange
            var type1 = new AreaType(AreaTypeEnum.Yard);
            var type2 = new AreaType(AreaTypeEnum.Warehouse);

            // Act
            var hash1 = type1.GetHashCode();
            var hash2 = type2.GetHashCode();

            // Assert
            hash1.Should().NotBe(hash2);
        }

        #endregion

        #region Enum Tests

        [Fact]
        public void AreaTypeEnum_ShouldHaveYardAsZero()
        {
            // Assert
            ((int)AreaTypeEnum.Yard).Should().Be(0);
        }

        [Fact]
        public void AreaTypeEnum_ShouldHaveWarehouseAsOne()
        {
            // Assert
            ((int)AreaTypeEnum.Warehouse).Should().Be(1);
        }

        [Fact]
        public void AreaTypeEnum_ShouldHaveExactlyTwoValues()
        {
            // Act
            var values = Enum.GetValues(typeof(AreaTypeEnum));

            // Assert
            values.Length.Should().Be(2);
        }

        [Fact]
        public void AreaTypeEnum_AllValues_ShouldBeUsableInConstructor()
        {
            // Arrange
            var allValues = Enum.GetValues(typeof(AreaTypeEnum)).Cast<AreaTypeEnum>();

            // Act & Assert
            foreach (var value in allValues)
            {
                var areaType = new AreaType(value);
                areaType.Value.Should().Be(value);
            }
        }

        #endregion

        #region Edge Cases

        [Fact]
        public void Constructor_WithInvalidEnumValue_ShouldStillCreateObject()
        {
            // Arrange - Cast an invalid int to enum
            var invalidEnum = (AreaTypeEnum)999;

            // Act
            var areaType = new AreaType(invalidEnum);

            // Assert
            areaType.Value.Should().Be(invalidEnum);
        }

        [Fact]
        public void Value_AfterConstruction_ShouldBeImmutable()
        {
            // Arrange
            var areaType = new AreaType(AreaTypeEnum.Yard);

            // Act
            var retrievedValue = areaType.Value;

            // Assert
            retrievedValue.Should().Be(AreaTypeEnum.Yard);
        }

        [Fact]
        public void Equals_WithNull_ShouldReturnFalse()
        {
            // Arrange
            var areaType = new AreaType(AreaTypeEnum.Yard);

            // Act
            var result = areaType.Equals(null);

            // Assert
            result.Should().BeFalse();
        }

        [Fact]
        public void Equals_WithArbitraryObject_ShouldReturnFalse()
        {
            // Arrange
            var areaType = new AreaType(AreaTypeEnum.Yard);
            var arbitraryObject = new { Type = "Yard" };

            // Act
            var result = areaType.Equals(arbitraryObject);

            // Assert
            result.Should().BeFalse();
        }

        #endregion
    }
}
