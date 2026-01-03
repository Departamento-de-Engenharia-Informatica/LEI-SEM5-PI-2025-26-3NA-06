using FluentAssertions;
using ProjArqsi.Domain.ContainerAggregate;
using ProjArqsi.Domain.StorageAreaAggregate;
using ProjArqsi.Domain.VesselVisitNotificationAggregate;
using ProjArqsi.Domain.Shared;

namespace Backend.Tests.ValueObjectTests.VesselVisitNotification
{
    public class ManifestEntryTests
    {
        #region CreateLoadEntry Tests

        [Fact]
        public void CreateLoadEntry_WithValidParameters_ShouldCreateManifestEntry()
        {
            // Arrange
            var containerId = new ContainerId(Guid.NewGuid());
            var storageAreaId = new StorageAreaId(Guid.NewGuid());

            // Act
            var entry = ManifestEntry.CreateLoadEntry(containerId, storageAreaId);

            // Assert
            entry.Should().NotBeNull();
            entry.ContainerId.Should().Be(containerId);
            entry.SourceStorageAreaId.Should().Be(storageAreaId);
            entry.TargetStorageAreaId.Should().BeNull();
        }

        [Fact]
        public void CreateLoadEntry_ShouldSetSourceStorageAreaAndNullTarget()
        {
            // Arrange
            var containerId = new ContainerId(Guid.NewGuid());
            var sourceArea = new StorageAreaId(Guid.NewGuid());

            // Act
            var entry = ManifestEntry.CreateLoadEntry(containerId, sourceArea);

            // Assert
            entry.SourceStorageAreaId.Should().NotBeNull();
            entry.TargetStorageAreaId.Should().BeNull();
        }

        [Fact]
        public void CreateLoadEntry_WithNullContainerId_ShouldThrowBusinessRuleValidationException()
        {
            // Arrange
            var storageAreaId = new StorageAreaId(Guid.NewGuid());

            // Act
            Action act = () => ManifestEntry.CreateLoadEntry(null!, storageAreaId);

            // Assert
            act.Should().Throw<BusinessRuleValidationException>()
                .WithMessage("Container ID is required.");
        }

        [Fact]
        public void CreateLoadEntry_WithNullSourceStorageAreaId_ShouldThrowBusinessRuleValidationException()
        {
            // Arrange
            var containerId = new ContainerId(Guid.NewGuid());

            // Act
            Action act = () => ManifestEntry.CreateLoadEntry(containerId, null!);

            // Assert
            act.Should().Throw<BusinessRuleValidationException>()
                .WithMessage("Source storage area is required for loading operations.");
        }

        #endregion

        #region CreateUnloadEntry Tests

        [Fact]
        public void CreateUnloadEntry_WithValidParameters_ShouldCreateManifestEntry()
        {
            // Arrange
            var containerId = new ContainerId(Guid.NewGuid());
            var storageAreaId = new StorageAreaId(Guid.NewGuid());

            // Act
            var entry = ManifestEntry.CreateUnloadEntry(containerId, storageAreaId);

            // Assert
            entry.Should().NotBeNull();
            entry.ContainerId.Should().Be(containerId);
            entry.SourceStorageAreaId.Should().BeNull();
            entry.TargetStorageAreaId.Should().Be(storageAreaId);
        }

        [Fact]
        public void CreateUnloadEntry_ShouldSetTargetStorageAreaAndNullSource()
        {
            // Arrange
            var containerId = new ContainerId(Guid.NewGuid());
            var targetArea = new StorageAreaId(Guid.NewGuid());

            // Act
            var entry = ManifestEntry.CreateUnloadEntry(containerId, targetArea);

            // Assert
            entry.SourceStorageAreaId.Should().BeNull();
            entry.TargetStorageAreaId.Should().NotBeNull();
        }

        [Fact]
        public void CreateUnloadEntry_WithNullContainerId_ShouldThrowBusinessRuleValidationException()
        {
            // Arrange
            var storageAreaId = new StorageAreaId(Guid.NewGuid());

            // Act
            Action act = () => ManifestEntry.CreateUnloadEntry(null!, storageAreaId);

            // Assert
            act.Should().Throw<BusinessRuleValidationException>()
                .WithMessage("Container ID is required.");
        }

        [Fact]
        public void CreateUnloadEntry_WithNullTargetStorageAreaId_ShouldThrowBusinessRuleValidationException()
        {
            // Arrange
            var containerId = new ContainerId(Guid.NewGuid());

            // Act
            Action act = () => ManifestEntry.CreateUnloadEntry(containerId, null!);

            // Assert
            act.Should().Throw<BusinessRuleValidationException>()
                .WithMessage("Target storage area is required for unloading operations.");
        }

        #endregion

        #region ValidateForManifestType Tests - Load

        [Fact]
        public void ValidateForManifestType_LoadEntryWithLoadManifest_ShouldNotThrow()
        {
            // Arrange
            var containerId = new ContainerId(Guid.NewGuid());
            var sourceArea = new StorageAreaId(Guid.NewGuid());
            var entry = ManifestEntry.CreateLoadEntry(containerId, sourceArea);
            var manifestType = ManifestType.Load;

            // Act
            Action act = () => entry.ValidateForManifestType(manifestType);

            // Assert
            act.Should().NotThrow();
        }

        [Fact]
        public void ValidateForManifestType_LoadEntryWithUnloadManifest_ShouldThrowBusinessRuleValidationException()
        {
            // Arrange
            var containerId = new ContainerId(Guid.NewGuid());
            var sourceArea = new StorageAreaId(Guid.NewGuid());
            var entry = ManifestEntry.CreateLoadEntry(containerId, sourceArea);
            var manifestType = ManifestType.Unload;

            // Act
            Action act = () => entry.ValidateForManifestType(manifestType);

            // Assert
            act.Should().Throw<BusinessRuleValidationException>()
                .WithMessage("Unload manifest entries must have a target storage area.");
        }

        #endregion

        #region ValidateForManifestType Tests - Unload

        [Fact]
        public void ValidateForManifestType_UnloadEntryWithUnloadManifest_ShouldNotThrow()
        {
            // Arrange
            var containerId = new ContainerId(Guid.NewGuid());
            var targetArea = new StorageAreaId(Guid.NewGuid());
            var entry = ManifestEntry.CreateUnloadEntry(containerId, targetArea);
            var manifestType = ManifestType.Unload;

            // Act
            Action act = () => entry.ValidateForManifestType(manifestType);

            // Assert
            act.Should().NotThrow();
        }

        [Fact]
        public void ValidateForManifestType_UnloadEntryWithLoadManifest_ShouldThrowBusinessRuleValidationException()
        {
            // Arrange
            var containerId = new ContainerId(Guid.NewGuid());
            var targetArea = new StorageAreaId(Guid.NewGuid());
            var entry = ManifestEntry.CreateUnloadEntry(containerId, targetArea);
            var manifestType = ManifestType.Load;

            // Act
            Action act = () => entry.ValidateForManifestType(manifestType);

            // Assert
            act.Should().Throw<BusinessRuleValidationException>()
                .WithMessage("Load manifest entries must have a source storage area.");
        }

        #endregion

        #region Edge Cases

        [Fact]
        public void CreateLoadEntry_MultipleEntriesWithDifferentContainers_ShouldCreateDistinctEntries()
        {
            // Arrange
            var container1 = new ContainerId(Guid.NewGuid());
            var container2 = new ContainerId(Guid.NewGuid());
            var sourceArea = new StorageAreaId(Guid.NewGuid());

            // Act
            var entry1 = ManifestEntry.CreateLoadEntry(container1, sourceArea);
            var entry2 = ManifestEntry.CreateLoadEntry(container2, sourceArea);

            // Assert
            entry1.ContainerId.Should().NotBe(entry2.ContainerId);
            entry1.SourceStorageAreaId.Should().Be(entry2.SourceStorageAreaId);
        }

        [Fact]
        public void CreateUnloadEntry_MultipleEntriesWithDifferentStorageAreas_ShouldCreateDistinctEntries()
        {
            // Arrange
            var containerId = new ContainerId(Guid.NewGuid());
            var targetArea1 = new StorageAreaId(Guid.NewGuid());
            var targetArea2 = new StorageAreaId(Guid.NewGuid());

            // Act
            var entry1 = ManifestEntry.CreateUnloadEntry(containerId, targetArea1);
            var entry2 = ManifestEntry.CreateUnloadEntry(containerId, targetArea2);

            // Assert
            entry1.TargetStorageAreaId.Should().NotBe(entry2.TargetStorageAreaId);
        }

        #endregion

        #region Real-World Scenarios

        [Fact]
        public void CreateLoadEntry_LoadingContainerFromYard_ShouldCreateValidEntry()
        {
            // Arrange - Container in yard needs to be loaded onto vessel
            var containerId = new ContainerId(Guid.NewGuid());
            var yardArea = new StorageAreaId(Guid.NewGuid());

            // Act
            var loadEntry = ManifestEntry.CreateLoadEntry(containerId, yardArea);

            // Assert
            loadEntry.ContainerId.Should().Be(containerId);
            loadEntry.SourceStorageAreaId.Should().Be(yardArea);
            loadEntry.TargetStorageAreaId.Should().BeNull(); // Target is vessel
        }

        [Fact]
        public void CreateUnloadEntry_UnloadingContainerToWarehouse_ShouldCreateValidEntry()
        {
            // Arrange - Container on vessel needs to be unloaded to warehouse
            var containerId = new ContainerId(Guid.NewGuid());
            var warehouseArea = new StorageAreaId(Guid.NewGuid());

            // Act
            var unloadEntry = ManifestEntry.CreateUnloadEntry(containerId, warehouseArea);

            // Assert
            unloadEntry.ContainerId.Should().Be(containerId);
            unloadEntry.SourceStorageAreaId.Should().BeNull(); // Source is vessel
            unloadEntry.TargetStorageAreaId.Should().Be(warehouseArea);
        }

        [Fact]
        public void CreateLoadEntry_MultipleContainersFromSameYard_ShouldCreateMultipleEntries()
        {
            // Arrange - Loading multiple containers from the same storage area
            var container1 = new ContainerId(Guid.NewGuid());
            var container2 = new ContainerId(Guid.NewGuid());
            var container3 = new ContainerId(Guid.NewGuid());
            var yardArea = new StorageAreaId(Guid.NewGuid());

            // Act
            var entries = new List<ManifestEntry>
            {
                ManifestEntry.CreateLoadEntry(container1, yardArea),
                ManifestEntry.CreateLoadEntry(container2, yardArea),
                ManifestEntry.CreateLoadEntry(container3, yardArea)
            };

            // Assert
            entries.Should().HaveCount(3);
            entries.Select(e => e.SourceStorageAreaId).Should().OnlyContain(id => id == yardArea);
            entries.Select(e => e.ContainerId).Should().OnlyHaveUniqueItems();
        }

        [Fact]
        public void CreateUnloadEntry_ContainersToMultipleStorageAreas_ShouldCreateValidEntries()
        {
            // Arrange - Unloading containers to different storage areas
            var container1 = new ContainerId(Guid.NewGuid());
            var container2 = new ContainerId(Guid.NewGuid());
            var warehouseA = new StorageAreaId(Guid.NewGuid());
            var yardB = new StorageAreaId(Guid.NewGuid());

            // Act
            var entry1 = ManifestEntry.CreateUnloadEntry(container1, warehouseA);
            var entry2 = ManifestEntry.CreateUnloadEntry(container2, yardB);

            // Assert
            entry1.TargetStorageAreaId.Should().Be(warehouseA);
            entry2.TargetStorageAreaId.Should().Be(yardB);
        }

        [Fact]
        public void ValidateForManifestType_LoadManifestWithProperEntries_ShouldValidateSuccessfully()
        {
            // Arrange - Loading manifest with multiple load entries
            var loadManifest = ManifestType.Load;
            var entries = new List<ManifestEntry>
            {
                ManifestEntry.CreateLoadEntry(new ContainerId(Guid.NewGuid()), new StorageAreaId(Guid.NewGuid())),
                ManifestEntry.CreateLoadEntry(new ContainerId(Guid.NewGuid()), new StorageAreaId(Guid.NewGuid())),
                ManifestEntry.CreateLoadEntry(new ContainerId(Guid.NewGuid()), new StorageAreaId(Guid.NewGuid()))
            };

            // Act & Assert - All entries should validate
            foreach (var entry in entries)
            {
                entry.Invoking(e => e.ValidateForManifestType(loadManifest))
                    .Should().NotThrow();
            }
        }

        [Fact]
        public void ValidateForManifestType_UnloadManifestWithProperEntries_ShouldValidateSuccessfully()
        {
            // Arrange - Unloading manifest with multiple unload entries
            var unloadManifest = ManifestType.Unload;
            var entries = new List<ManifestEntry>
            {
                ManifestEntry.CreateUnloadEntry(new ContainerId(Guid.NewGuid()), new StorageAreaId(Guid.NewGuid())),
                ManifestEntry.CreateUnloadEntry(new ContainerId(Guid.NewGuid()), new StorageAreaId(Guid.NewGuid())),
                ManifestEntry.CreateUnloadEntry(new ContainerId(Guid.NewGuid()), new StorageAreaId(Guid.NewGuid()))
            };

            // Act & Assert - All entries should validate
            foreach (var entry in entries)
            {
                entry.Invoking(e => e.ValidateForManifestType(unloadManifest))
                    .Should().NotThrow();
            }
        }

        [Fact]
        public void CreateLoadEntry_ForExportContainer_ShouldRepresentContainerMovement()
        {
            // Arrange - Export container: Storage Area -> Vessel
            var exportContainer = new ContainerId(Guid.NewGuid());
            var exportYard = new StorageAreaId(Guid.NewGuid());

            // Act
            var entry = ManifestEntry.CreateLoadEntry(exportContainer, exportYard);

            // Assert
            entry.SourceStorageAreaId.Should().NotBeNull();
            entry.TargetStorageAreaId.Should().BeNull(); // Implicit: vessel
        }

        [Fact]
        public void CreateUnloadEntry_ForImportContainer_ShouldRepresentContainerMovement()
        {
            // Arrange - Import container: Vessel -> Storage Area
            var importContainer = new ContainerId(Guid.NewGuid());
            var importWarehouse = new StorageAreaId(Guid.NewGuid());

            // Act
            var entry = ManifestEntry.CreateUnloadEntry(importContainer, importWarehouse);

            // Assert
            entry.SourceStorageAreaId.Should().BeNull(); // Implicit: vessel
            entry.TargetStorageAreaId.Should().NotBeNull();
        }

        [Fact]
        public void ManifestEntry_LoadAndUnloadSameContainer_ShouldBeDifferentEntries()
        {
            // Arrange - Same container appears in both manifests (edge case, should be validated elsewhere)
            var containerId = new ContainerId(Guid.NewGuid());
            var sourceArea = new StorageAreaId(Guid.NewGuid());
            var targetArea = new StorageAreaId(Guid.NewGuid());

            // Act
            var loadEntry = ManifestEntry.CreateLoadEntry(containerId, sourceArea);
            var unloadEntry = ManifestEntry.CreateUnloadEntry(containerId, targetArea);

            // Assert
            loadEntry.ContainerId.Should().Be(unloadEntry.ContainerId);
            loadEntry.SourceStorageAreaId.Should().NotBeNull();
            loadEntry.TargetStorageAreaId.Should().BeNull();
            unloadEntry.SourceStorageAreaId.Should().BeNull();
            unloadEntry.TargetStorageAreaId.Should().NotBeNull();
        }

        #endregion

        #region Validation Error Messages Tests

        [Fact]
        public void ValidateForManifestType_LoadManifestWithMissingSource_ShouldProvideSpecificErrorMessage()
        {
            // Arrange - Create unload entry and try to validate for load manifest
            var entry = ManifestEntry.CreateUnloadEntry(
                new ContainerId(Guid.NewGuid()), 
                new StorageAreaId(Guid.NewGuid()));
            var loadManifest = ManifestType.Load;

            // Act
            Action act = () => entry.ValidateForManifestType(loadManifest);

            // Assert
            act.Should().Throw<BusinessRuleValidationException>()
                .WithMessage("*must have a source storage area*");
        }

        [Fact]
        public void ValidateForManifestType_UnloadManifestWithMissingTarget_ShouldProvideSpecificErrorMessage()
        {
            // Arrange - Create load entry and try to validate for unload manifest
            var entry = ManifestEntry.CreateLoadEntry(
                new ContainerId(Guid.NewGuid()), 
                new StorageAreaId(Guid.NewGuid()));
            var unloadManifest = ManifestType.Unload;

            // Act
            Action act = () => entry.ValidateForManifestType(unloadManifest);

            // Assert
            act.Should().Throw<BusinessRuleValidationException>()
                .WithMessage("*must have a target storage area*");
        }

        #endregion
    }
}
