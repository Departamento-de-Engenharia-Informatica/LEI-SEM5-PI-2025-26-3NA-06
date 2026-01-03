using FluentAssertions;
using ProjArqsi.Domain.DockAggregate;
using Xunit;

namespace ProjArqsi.Tests._1_ValueObjectTests.Dock
{
    public class AllowedVesselTypesTests
    {
        #region Valid Creation Tests

        [Fact]
        public void CreateAllowedVesselTypes_WithValidGuidList_ShouldSucceed()
        {
            // Arrange
            var vesselTypeIds = new List<Guid>
            {
                Guid.NewGuid(),
                Guid.NewGuid(),
                Guid.NewGuid()
            };

            // Act
            var allowedTypes = new AllowedVesselTypes(vesselTypeIds);

            // Assert
            allowedTypes.Should().NotBeNull();
            allowedTypes.VesselTypeIds.Should().HaveCount(3);
            allowedTypes.VesselTypeIds.Should().BeEquivalentTo(vesselTypeIds);
        }

        [Fact]
        public void CreateAllowedVesselTypes_WithEmptyList_ShouldSucceed()
        {
            // Arrange
            var emptyList = new List<Guid>();

            // Act
            var allowedTypes = new AllowedVesselTypes(emptyList);

            // Assert
            allowedTypes.Should().NotBeNull();
            allowedTypes.VesselTypeIds.Should().BeEmpty();
        }

        [Fact]
        public void CreateAllowedVesselTypes_WithNull_ShouldCreateEmptyList()
        {
            // Act
            var allowedTypes = new AllowedVesselTypes(null);

            // Assert
            allowedTypes.Should().NotBeNull();
            allowedTypes.VesselTypeIds.Should().NotBeNull();
            allowedTypes.VesselTypeIds.Should().BeEmpty();
        }

        [Fact]
        public void CreateAllowedVesselTypes_WithDefaultConstructor_ShouldCreateEmptyList()
        {
            // Act
            var allowedTypes = new AllowedVesselTypes();

            // Assert
            allowedTypes.Should().NotBeNull();
            allowedTypes.VesselTypeIds.Should().NotBeNull();
            allowedTypes.VesselTypeIds.Should().BeEmpty();
        }

        [Fact]
        public void CreateAllowedVesselTypes_WithSingleGuid_ShouldSucceed()
        {
            // Arrange
            var singleGuid = Guid.NewGuid();
            var vesselTypeIds = new List<Guid> { singleGuid };

            // Act
            var allowedTypes = new AllowedVesselTypes(vesselTypeIds);

            // Assert
            allowedTypes.VesselTypeIds.Should().ContainSingle();
            allowedTypes.VesselTypeIds.First().Should().Be(singleGuid);
        }

        [Fact]
        public void CreateAllowedVesselTypes_WithManyGuids_ShouldSucceed()
        {
            // Arrange
            var vesselTypeIds = Enumerable.Range(0, 50)
                .Select(_ => Guid.NewGuid())
                .ToList();

            // Act
            var allowedTypes = new AllowedVesselTypes(vesselTypeIds);

            // Assert
            allowedTypes.VesselTypeIds.Should().HaveCount(50);
            allowedTypes.VesselTypeIds.Should().BeEquivalentTo(vesselTypeIds);
        }

        [Fact]
        public void CreateAllowedVesselTypes_WithDuplicateGuids_ShouldAllowDuplicates()
        {
            // Arrange
            var guid1 = Guid.NewGuid();
            var vesselTypeIds = new List<Guid> { guid1, guid1, guid1 };

            // Act
            var allowedTypes = new AllowedVesselTypes(vesselTypeIds);

            // Assert
            allowedTypes.VesselTypeIds.Should().HaveCount(3);
            allowedTypes.VesselTypeIds.Should().OnlyContain(g => g == guid1);
        }

        #endregion

        #region Equality Tests

        [Fact]
        public void AllowedVesselTypes_WithSameReference_ShouldBeEqual()
        {
            // Arrange
            var list = new List<Guid> { Guid.NewGuid(), Guid.NewGuid() };
            var allowedTypes1 = new AllowedVesselTypes(list);
            var allowedTypes2 = allowedTypes1;

            // Act & Assert
            allowedTypes1.Should().Be(allowedTypes2);
            allowedTypes1.GetHashCode().Should().Be(allowedTypes2.GetHashCode());
        }

        [Fact]
        public void AllowedVesselTypes_WithDifferentListInstances_ShouldNotBeEqual()
        {
            // Arrange - Even with same GUIDs, different list instances are not equal
            var guid1 = Guid.NewGuid();
            var guid2 = Guid.NewGuid();
            var list1 = new List<Guid> { guid1, guid2 };
            var list2 = new List<Guid> { guid1, guid2 };

            var allowedTypes1 = new AllowedVesselTypes(list1);
            var allowedTypes2 = new AllowedVesselTypes(list2);

            // Act & Assert
            // Note: List uses reference equality in ValueObject, so different instances are not equal
            allowedTypes1.Should().NotBe(allowedTypes2);
        }

        [Fact]
        public void AllowedVesselTypes_WithDifferentGuidLists_ShouldNotBeEqual()
        {
            // Arrange
            var list1 = new List<Guid> { Guid.NewGuid(), Guid.NewGuid() };
            var list2 = new List<Guid> { Guid.NewGuid(), Guid.NewGuid() };

            var allowedTypes1 = new AllowedVesselTypes(list1);
            var allowedTypes2 = new AllowedVesselTypes(list2);

            // Act & Assert
            allowedTypes1.Should().NotBe(allowedTypes2);
        }

        [Fact]
        public void AllowedVesselTypes_BothCreatedWithNull_ShouldHaveEmptyLists()
        {
            // Arrange
            var allowedTypes1 = new AllowedVesselTypes(null);
            var allowedTypes2 = new AllowedVesselTypes(null);

            // Act & Assert
            // Both have empty lists but different instances
            allowedTypes1.VesselTypeIds.Should().BeEmpty();
            allowedTypes2.VesselTypeIds.Should().BeEmpty();
            allowedTypes1.Should().NotBe(allowedTypes2);
        }

        [Fact]
        public void AllowedVesselTypes_ComparedToNull_ShouldNotBeEqual()
        {
            // Arrange
            var allowedTypes = new AllowedVesselTypes(new List<Guid> { Guid.NewGuid() });

            // Act & Assert
            allowedTypes.Should().NotBe(null);
        }

        [Fact]
        public void AllowedVesselTypes_WithDifferentOrder_ShouldNotBeEqual()
        {
            // Arrange
            var guid1 = Guid.NewGuid();
            var guid2 = Guid.NewGuid();
            var list1 = new List<Guid> { guid1, guid2 };
            var list2 = new List<Guid> { guid2, guid1 };

            var allowedTypes1 = new AllowedVesselTypes(list1);
            var allowedTypes2 = new AllowedVesselTypes(list2);

            // Act & Assert
            allowedTypes1.Should().NotBe(allowedTypes2);
        }

        #endregion

        #region Edge Case Tests

        [Fact]
        public void CreateAllowedVesselTypes_WithEmptyGuid_ShouldSucceed()
        {
            // Arrange
            var vesselTypeIds = new List<Guid> { Guid.Empty };

            // Act
            var allowedTypes = new AllowedVesselTypes(vesselTypeIds);

            // Assert
            allowedTypes.VesselTypeIds.Should().ContainSingle();
            allowedTypes.VesselTypeIds.First().Should().Be(Guid.Empty);
        }

        [Fact]
        public void CreateAllowedVesselTypes_WithMixOfEmptyAndValidGuids_ShouldSucceed()
        {
            // Arrange
            var vesselTypeIds = new List<Guid>
            {
                Guid.Empty,
                Guid.NewGuid(),
                Guid.Empty,
                Guid.NewGuid()
            };

            // Act
            var allowedTypes = new AllowedVesselTypes(vesselTypeIds);

            // Assert
            allowedTypes.VesselTypeIds.Should().HaveCount(4);
            allowedTypes.VesselTypeIds.Should().Contain(Guid.Empty);
        }

        [Fact]
        public void AllowedVesselTypes_ShouldPreserveListOrder()
        {
            // Arrange
            var guid1 = Guid.NewGuid();
            var guid2 = Guid.NewGuid();
            var guid3 = Guid.NewGuid();
            var vesselTypeIds = new List<Guid> { guid1, guid2, guid3 };

            // Act
            var allowedTypes = new AllowedVesselTypes(vesselTypeIds);

            // Assert
            allowedTypes.VesselTypeIds[0].Should().Be(guid1);
            allowedTypes.VesselTypeIds[1].Should().Be(guid2);
            allowedTypes.VesselTypeIds[2].Should().Be(guid3);
        }

        [Fact]
        public void AllowedVesselTypes_ListShouldNotBeNull()
        {
            // Act
            var allowedTypes = new AllowedVesselTypes(null);

            // Assert
            allowedTypes.VesselTypeIds.Should().NotBeNull();
        }

        [Fact]
        public void CreateAllowedVesselTypes_WithLargeNumberOfGuids_ShouldSucceed()
        {
            // Arrange - 1000 vessel types
            var vesselTypeIds = Enumerable.Range(0, 1000)
                .Select(_ => Guid.NewGuid())
                .ToList();

            // Act
            var allowedTypes = new AllowedVesselTypes(vesselTypeIds);

            // Assert
            allowedTypes.VesselTypeIds.Should().HaveCount(1000);
        }

        #endregion
    }
}
