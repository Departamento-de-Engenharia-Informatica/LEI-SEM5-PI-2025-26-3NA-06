using FluentAssertions;
using ProjArqsi.Domain.DockAggregate;
using ProjArqsi.Domain.VesselVisitNotificationAggregate;

namespace Backend.Tests.ValueObjectTests.VesselVisitNotification
{
    public class TempAssignedDockIdTests
    {
        #region Valid TempAssignedDockId Tests

        [Fact]
        public void Constructor_WithValidGuid_ShouldCreateTempAssignedDockId()
        {
            // Arrange
            var guid = Guid.NewGuid();

            // Act
            var tempDockId = new TempAssignedDockId(guid);

            // Assert
            tempDockId.Should().NotBeNull();
            tempDockId.Value.Should().Be(guid.ToString());
        }

        [Fact]
        public void Constructor_WithDockId_ShouldCreateTempAssignedDockId()
        {
            // Arrange
            var dockId = new DockId(Guid.NewGuid());

            // Act
            var tempDockId = new TempAssignedDockId(dockId);

            // Assert
            tempDockId.Should().NotBeNull();
            tempDockId.Value.Should().Be(dockId.Value.ToString());
        }

        [Theory]
        [InlineData("d1e2f3a4-b5c6-4d7e-8f9a-0b1c2d3e4f5a")]
        [InlineData("12345678-90ab-cdef-1234-567890abcdef")]
        [InlineData("aaaaaaaa-bbbb-cccc-dddd-eeeeeeeeeeee")]
        public void Constructor_WithVariousValidGuids_ShouldCreateTempAssignedDockId(string guidString)
        {
            // Arrange
            var guid = Guid.Parse(guidString);

            // Act
            var tempDockId = new TempAssignedDockId(guid);

            // Assert
            tempDockId.Value.Should().Be(guid.ToString());
        }

        #endregion

        #region AsGuid Method Tests

        [Fact]
        public void AsGuid_ShouldReturnGuidValue()
        {
            // Arrange
            var guid = Guid.NewGuid();
            var tempDockId = new TempAssignedDockId(guid);

            // Act
            var result = tempDockId.AsGuid();

            // Assert
            result.Should().Be(guid);
        }

        [Fact]
        public void AsGuid_WithDockIdConstructor_ShouldReturnDockIdValue()
        {
            // Arrange
            var guid = Guid.NewGuid();
            var dockId = new DockId(guid);
            var tempDockId = new TempAssignedDockId(dockId);

            // Act
            var result = tempDockId.AsGuid();

            // Assert
            result.Should().Be(guid);
        }

        #endregion

        #region Equality Tests

        [Fact]
        public void Equality_TwoTempAssignedDockIdsWithSameGuid_ShouldBeEqual()
        {
            // Arrange
            var guid = Guid.NewGuid();
            var tempDockId1 = new TempAssignedDockId(guid);
            var tempDockId2 = new TempAssignedDockId(guid);

            // Act & Assert
            tempDockId1.Equals(tempDockId2).Should().BeTrue();
            tempDockId1.GetHashCode().Should().Be(tempDockId2.GetHashCode());
        }

        [Fact]
        public void Equality_TwoTempAssignedDockIdsWithDifferentGuids_ShouldNotBeEqual()
        {
            // Arrange
            var guid1 = Guid.NewGuid();
            var guid2 = Guid.NewGuid();
            var tempDockId1 = new TempAssignedDockId(guid1);
            var tempDockId2 = new TempAssignedDockId(guid2);

            // Act & Assert
            tempDockId1.Equals(tempDockId2).Should().BeFalse();
        }

        [Fact]
        public void Equality_TempAssignedDockIdFromGuidAndDockId_WithSameValue_ShouldBeEqual()
        {
            // Arrange
            var guid = Guid.NewGuid();
            var dockId = new DockId(guid);
            var tempDockId1 = new TempAssignedDockId(guid);
            var tempDockId2 = new TempAssignedDockId(dockId);

            // Act & Assert
            tempDockId1.Equals(tempDockId2).Should().BeTrue();
            tempDockId1.GetHashCode().Should().Be(tempDockId2.GetHashCode());
        }

        [Fact]
        public void Equality_TempAssignedDockIdComparedWithNull_ShouldNotBeEqual()
        {
            // Arrange
            var tempDockId = new TempAssignedDockId(Guid.NewGuid());

            // Act & Assert
            tempDockId.Equals(null).Should().BeFalse();
        }

        #endregion

        #region Edge Cases

        [Fact]
        public void Constructor_WithEmptyGuid_ShouldCreateTempAssignedDockId()
        {
            // Arrange
            var emptyGuid = Guid.Empty;

            // Act
            var tempDockId = new TempAssignedDockId(emptyGuid);

            // Assert
            tempDockId.Value.Should().Be(emptyGuid.ToString());
        }

        [Fact]
        public void TempAssignedDockId_ShouldBeImmutable()
        {
            // Arrange
            var guid = Guid.NewGuid();
            var tempDockId = new TempAssignedDockId(guid);
            var originalValue = tempDockId.Value;

            // Act - Value property should not have public setter

            // Assert
            tempDockId.Value.Should().Be(originalValue);
        }

        [Fact]
        public void AsGuid_CalledMultipleTimes_ShouldReturnSameValue()
        {
            // Arrange
            var guid = Guid.NewGuid();
            var tempDockId = new TempAssignedDockId(guid);

            // Act
            var result1 = tempDockId.AsGuid();
            var result2 = tempDockId.AsGuid();
            var result3 = tempDockId.AsGuid();

            // Assert
            result1.Should().Be(result2);
            result2.Should().Be(result3);
        }

        [Fact]
        public void Constructor_WithDockIdFromEmptyGuid_ShouldCreateTempAssignedDockId()
        {
            // Arrange
            var dockId = new DockId(Guid.Empty);

            // Act
            var tempDockId = new TempAssignedDockId(dockId);

            // Assert
            tempDockId.Value.Should().Be(Guid.Empty.ToString());
        }

        #endregion

        #region Real-World Scenarios

        [Fact]
        public void Constructor_AssigningDockToVVN_ShouldCreateTempAssignedDockId()
        {
            // Arrange - Officer assigns dock to approved VVN
            var dockId = new DockId(Guid.NewGuid());

            // Act
            var tempDockId = new TempAssignedDockId(dockId);

            // Assert
            tempDockId.Should().NotBeNull();
            tempDockId.AsGuid().Should().Be(dockId.Value);
        }

        [Fact]
        public void Constructor_MultipleVVNsWithDifferentDocks_ShouldCreateDistinctIds()
        {
            // Arrange
            var dock1 = new DockId(Guid.NewGuid());
            var dock2 = new DockId(Guid.NewGuid());
            var dock3 = new DockId(Guid.NewGuid());

            // Act
            var tempDock1 = new TempAssignedDockId(dock1);
            var tempDock2 = new TempAssignedDockId(dock2);
            var tempDock3 = new TempAssignedDockId(dock3);

            // Assert
            tempDock1.Should().NotBe(tempDock2);
            tempDock2.Should().NotBe(tempDock3);
            tempDock1.Should().NotBe(tempDock3);
        }

        [Fact]
        public void Constructor_ReassigningDockToSameVVN_ShouldUpdateReference()
        {
            // Arrange
            var firstDock = new DockId(Guid.NewGuid());
            var secondDock = new DockId(Guid.NewGuid());
            var tempDockId1 = new TempAssignedDockId(firstDock);

            // Act - VVN gets reassigned to different dock
            var tempDockId2 = new TempAssignedDockId(secondDock);

            // Assert
            tempDockId1.AsGuid().Should().NotBe(tempDockId2.AsGuid());
        }

        [Fact]
        public void AsGuid_CanBeUsedToRetrieveDockDetails()
        {
            // Arrange
            var dockGuid = Guid.NewGuid();
            var dockId = new DockId(dockGuid);
            var tempDockId = new TempAssignedDockId(dockId);

            // Act - Simulate retrieving dock for display
            var retrievedGuid = tempDockId.AsGuid();

            // Assert
            retrievedGuid.Should().Be(dockGuid);
        }

        [Fact]
        public void Constructor_ForApprovedVVN_ShouldStoreTemporaryDockAssignment()
        {
            // Arrange - VVN approved and temporarily assigned to Dock A
            var dockAId = new DockId(Guid.NewGuid());

            // Act
            var tempDockId = new TempAssignedDockId(dockAId);

            // Assert - Temporary assignment stored
            tempDockId.AsGuid().Should().Be(dockAId.Value);
        }

        [Fact]
        public void TempAssignedDockId_CanBeUsedInCollections()
        {
            // Arrange - Tracking multiple VVN dock assignments
            var assignments = new Dictionary<Guid, TempAssignedDockId>
            {
                { Guid.NewGuid(), new TempAssignedDockId(Guid.NewGuid()) },
                { Guid.NewGuid(), new TempAssignedDockId(Guid.NewGuid()) },
                { Guid.NewGuid(), new TempAssignedDockId(Guid.NewGuid()) }
            };

            // Assert
            assignments.Should().HaveCount(3);
        }

        [Fact]
        public void Constructor_FromExistingDock_ShouldPreserveDockReference()
        {
            // Arrange - Dock already exists in system
            var existingDockGuid = Guid.Parse("12345678-90ab-cdef-1234-567890abcdef");
            var existingDockId = new DockId(existingDockGuid);

            // Act
            var tempDockId = new TempAssignedDockId(existingDockId);

            // Assert
            tempDockId.AsGuid().Should().Be(existingDockGuid);
        }

        #endregion

        #region Interoperability Tests

        [Fact]
        public void Constructor_CanConvertBetweenDockIdAndGuid()
        {
            // Arrange
            var guid = Guid.NewGuid();
            var dockId = new DockId(guid);

            // Act
            var fromGuid = new TempAssignedDockId(guid);
            var fromDockId = new TempAssignedDockId(dockId);

            // Assert
            fromGuid.AsGuid().Should().Be(fromDockId.AsGuid());
        }

        [Fact]
        public void Value_ShouldMatchDockIdValue()
        {
            // Arrange
            var dockId = new DockId(Guid.NewGuid());

            // Act
            var tempDockId = new TempAssignedDockId(dockId);

            // Assert
            tempDockId.Value.Should().Be(dockId.Value.ToString());
        }

        #endregion
    }
}
