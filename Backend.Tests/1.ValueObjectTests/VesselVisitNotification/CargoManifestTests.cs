using FluentAssertions;
using ProjArqsi.Domain.ContainerAggregate;
using ProjArqsi.Domain.StorageAreaAggregate;
using ProjArqsi.Domain.VesselVisitNotificationAggregate;
using ProjArqsi.Domain.Shared;

namespace Backend.Tests.ValueObjectTests.VesselVisitNotification
{
    public class CargoManifestTests
    {
        #region Valid CargoManifest Tests

        [Fact]
        public void Constructor_WithValidManifestType_ShouldCreateCargoManifest()
        {
            // Arrange
            var manifestType = ManifestType.Load;

            // Act
            var cargoManifest = new CargoManifest(manifestType);

            // Assert
            cargoManifest.Should().NotBeNull();
            cargoManifest.ManifestType.Should().Be(manifestType);
            cargoManifest.Entries.Should().BeEmpty();
        }

        [Fact]
        public void Constructor_WithLoadType_ShouldCreateLoadManifest()
        {
            // Act
            var cargoManifest = new CargoManifest(ManifestType.Load);

            // Assert
            cargoManifest.ManifestType.Value.Should().Be(ManifestTypeEnum.Load);
        }

        [Fact]
        public void Constructor_WithUnloadType_ShouldCreateUnloadManifest()
        {
            // Act
            var cargoManifest = new CargoManifest(ManifestType.Unload);

            // Assert
            cargoManifest.ManifestType.Value.Should().Be(ManifestTypeEnum.Unload);
        }

        #endregion

        #region Invalid CargoManifest Tests

        [Fact]
        public void Constructor_WithNullManifestType_ShouldThrowBusinessRuleValidationException()
        {
            // Act
            Action act = () => new CargoManifest(null!);

            // Assert
            act.Should().Throw<BusinessRuleValidationException>()
                .WithMessage("Manifest type is required.");
        }

        #endregion

        #region AddEntry Tests - Valid

        [Fact]
        public void AddEntry_WithValidLoadEntry_ShouldAddToManifest()
        {
            // Arrange
            var manifest = new CargoManifest(ManifestType.Load);
            var entry = ManifestEntry.CreateLoadEntry(
                new ContainerId(Guid.NewGuid()),
                new StorageAreaId(Guid.NewGuid()));

            // Act
            manifest.AddEntry(entry);

            // Assert
            manifest.Entries.Should().HaveCount(1);
            manifest.Entries.First().Should().Be(entry);
        }

        [Fact]
        public void AddEntry_WithValidUnloadEntry_ShouldAddToManifest()
        {
            // Arrange
            var manifest = new CargoManifest(ManifestType.Unload);
            var entry = ManifestEntry.CreateUnloadEntry(
                new ContainerId(Guid.NewGuid()),
                new StorageAreaId(Guid.NewGuid()));

            // Act
            manifest.AddEntry(entry);

            // Assert
            manifest.Entries.Should().HaveCount(1);
        }

        [Fact]
        public void AddEntry_MultipleEntries_ShouldAddAll()
        {
            // Arrange
            var manifest = new CargoManifest(ManifestType.Load);
            var sourceArea = new StorageAreaId(Guid.NewGuid());
            var entry1 = ManifestEntry.CreateLoadEntry(new ContainerId(Guid.NewGuid()), sourceArea);
            var entry2 = ManifestEntry.CreateLoadEntry(new ContainerId(Guid.NewGuid()), sourceArea);
            var entry3 = ManifestEntry.CreateLoadEntry(new ContainerId(Guid.NewGuid()), sourceArea);

            // Act
            manifest.AddEntry(entry1);
            manifest.AddEntry(entry2);
            manifest.AddEntry(entry3);

            // Assert
            manifest.Entries.Should().HaveCount(3);
        }

        #endregion

        #region AddEntry Tests - Invalid

        [Fact]
        public void AddEntry_WithNullEntry_ShouldThrowBusinessRuleValidationException()
        {
            // Arrange
            var manifest = new CargoManifest(ManifestType.Load);

            // Act
            Action act = () => manifest.AddEntry(null!);

            // Assert
            act.Should().Throw<BusinessRuleValidationException>()
                .WithMessage("Manifest entry cannot be null.");
        }

        [Fact]
        public void AddEntry_UnloadEntryToLoadManifest_ShouldThrowBusinessRuleValidationException()
        {
            // Arrange
            var manifest = new CargoManifest(ManifestType.Load);
            var unloadEntry = ManifestEntry.CreateUnloadEntry(
                new ContainerId(Guid.NewGuid()),
                new StorageAreaId(Guid.NewGuid()));

            // Act
            Action act = () => manifest.AddEntry(unloadEntry);

            // Assert
            act.Should().Throw<BusinessRuleValidationException>();
        }

        [Fact]
        public void AddEntry_LoadEntryToUnloadManifest_ShouldThrowBusinessRuleValidationException()
        {
            // Arrange
            var manifest = new CargoManifest(ManifestType.Unload);
            var loadEntry = ManifestEntry.CreateLoadEntry(
                new ContainerId(Guid.NewGuid()),
                new StorageAreaId(Guid.NewGuid()));

            // Act
            Action act = () => manifest.AddEntry(loadEntry);

            // Assert
            act.Should().Throw<BusinessRuleValidationException>();
        }

        [Fact]
        public void AddEntry_DuplicateContainer_ShouldThrowBusinessRuleValidationException()
        {
            // Arrange
            var manifest = new CargoManifest(ManifestType.Load);
            var containerId = new ContainerId(Guid.NewGuid());
            var sourceArea = new StorageAreaId(Guid.NewGuid());
            var entry1 = ManifestEntry.CreateLoadEntry(containerId, sourceArea);
            var entry2 = ManifestEntry.CreateLoadEntry(containerId, sourceArea);

            // Act
            manifest.AddEntry(entry1);
            Action act = () => manifest.AddEntry(entry2);

            // Assert
            act.Should().Throw<BusinessRuleValidationException>()
                .WithMessage($"Container {containerId} is already in this manifest.");
        }

        #endregion

        #region RemoveEntry Tests

        [Fact]
        public void RemoveEntry_WithExistingEntry_ShouldRemoveFromManifest()
        {
            // Arrange
            var manifest = new CargoManifest(ManifestType.Load);
            var entry = ManifestEntry.CreateLoadEntry(
                new ContainerId(Guid.NewGuid()),
                new StorageAreaId(Guid.NewGuid()));
            manifest.AddEntry(entry);

            // Act
            manifest.RemoveEntry(entry);

            // Assert
            manifest.Entries.Should().BeEmpty();
        }

        [Fact]
        public void RemoveEntry_MultipleTimesOnSameEntry_ShouldNotThrow()
        {
            // Arrange
            var manifest = new CargoManifest(ManifestType.Load);
            var entry = ManifestEntry.CreateLoadEntry(
                new ContainerId(Guid.NewGuid()),
                new StorageAreaId(Guid.NewGuid()));
            manifest.AddEntry(entry);

            // Act
            manifest.RemoveEntry(entry);
            Action act = () => manifest.RemoveEntry(entry);

            // Assert
            act.Should().NotThrow();
            manifest.Entries.Should().BeEmpty();
        }

        #endregion

        #region ClearEntries Tests

        [Fact]
        public void ClearEntries_WithMultipleEntries_ShouldRemoveAll()
        {
            // Arrange
            var manifest = new CargoManifest(ManifestType.Load);
            var sourceArea = new StorageAreaId(Guid.NewGuid());
            manifest.AddEntry(ManifestEntry.CreateLoadEntry(new ContainerId(Guid.NewGuid()), sourceArea));
            manifest.AddEntry(ManifestEntry.CreateLoadEntry(new ContainerId(Guid.NewGuid()), sourceArea));
            manifest.AddEntry(ManifestEntry.CreateLoadEntry(new ContainerId(Guid.NewGuid()), sourceArea));

            // Act
            manifest.ClearEntries();

            // Assert
            manifest.Entries.Should().BeEmpty();
        }

        [Fact]
        public void ClearEntries_OnEmptyManifest_ShouldNotThrow()
        {
            // Arrange
            var manifest = new CargoManifest(ManifestType.Load);

            // Act
            Action act = () => manifest.ClearEntries();

            // Assert
            act.Should().NotThrow();
            manifest.Entries.Should().BeEmpty();
        }

        #endregion

        #region ValidateConsistency Tests

        [Fact]
        public void ValidateConsistency_WithValidEntries_ShouldNotThrow()
        {
            // Arrange
            var manifest = new CargoManifest(ManifestType.Load);
            var sourceArea = new StorageAreaId(Guid.NewGuid());
            manifest.AddEntry(ManifestEntry.CreateLoadEntry(new ContainerId(Guid.NewGuid()), sourceArea));
            manifest.AddEntry(ManifestEntry.CreateLoadEntry(new ContainerId(Guid.NewGuid()), sourceArea));

            // Act
            Action act = () => manifest.ValidateConsistency();

            // Assert
            act.Should().NotThrow();
        }

        [Fact]
        public void ValidateConsistency_WithEmptyManifest_ShouldNotThrow()
        {
            // Arrange
            var manifest = new CargoManifest(ManifestType.Unload);

            // Act
            Action act = () => manifest.ValidateConsistency();

            // Assert
            act.Should().NotThrow();
        }

        #endregion

        #region CalculateEstimatedTeu Tests

        [Fact]
        public void CalculateEstimatedTeu_WithNoEntries_ShouldReturnZero()
        {
            // Arrange
            var manifest = new CargoManifest(ManifestType.Load);

            // Act
            var teu = manifest.CalculateEstimatedTeu();

            // Assert
            teu.Should().Be(0);
        }

        [Fact]
        public void CalculateEstimatedTeu_WithOneEntry_ShouldReturnOne()
        {
            // Arrange
            var manifest = new CargoManifest(ManifestType.Load);
            manifest.AddEntry(ManifestEntry.CreateLoadEntry(
                new ContainerId(Guid.NewGuid()),
                new StorageAreaId(Guid.NewGuid())));

            // Act
            var teu = manifest.CalculateEstimatedTeu();

            // Assert
            teu.Should().Be(1);
        }

        [Fact]
        public void CalculateEstimatedTeu_WithMultipleEntries_ShouldReturnCount()
        {
            // Arrange
            var manifest = new CargoManifest(ManifestType.Load);
            var sourceArea = new StorageAreaId(Guid.NewGuid());
            manifest.AddEntry(ManifestEntry.CreateLoadEntry(new ContainerId(Guid.NewGuid()), sourceArea));
            manifest.AddEntry(ManifestEntry.CreateLoadEntry(new ContainerId(Guid.NewGuid()), sourceArea));
            manifest.AddEntry(ManifestEntry.CreateLoadEntry(new ContainerId(Guid.NewGuid()), sourceArea));
            manifest.AddEntry(ManifestEntry.CreateLoadEntry(new ContainerId(Guid.NewGuid()), sourceArea));
            manifest.AddEntry(ManifestEntry.CreateLoadEntry(new ContainerId(Guid.NewGuid()), sourceArea));

            // Act
            var teu = manifest.CalculateEstimatedTeu();

            // Assert
            teu.Should().Be(5);
        }

        #endregion

        #region Edge Cases

        [Fact]
        public void Entries_ShouldBeReadOnly()
        {
            // Arrange
            var manifest = new CargoManifest(ManifestType.Load);

            // Act
            var entries = manifest.Entries;

            // Assert
            entries.Should().BeAssignableTo<IReadOnlyCollection<ManifestEntry>>();
        }

        [Fact]
        public void AddEntry_AfterClear_ShouldAddSuccessfully()
        {
            // Arrange
            var manifest = new CargoManifest(ManifestType.Load);
            var sourceArea = new StorageAreaId(Guid.NewGuid());
            manifest.AddEntry(ManifestEntry.CreateLoadEntry(new ContainerId(Guid.NewGuid()), sourceArea));
            manifest.ClearEntries();

            // Act
            manifest.AddEntry(ManifestEntry.CreateLoadEntry(new ContainerId(Guid.NewGuid()), sourceArea));

            // Assert
            manifest.Entries.Should().HaveCount(1);
        }

        [Fact]
        public void AddEntry_SameContainerAfterRemoval_ShouldAddSuccessfully()
        {
            // Arrange
            var manifest = new CargoManifest(ManifestType.Load);
            var containerId = new ContainerId(Guid.NewGuid());
            var sourceArea = new StorageAreaId(Guid.NewGuid());
            var entry1 = ManifestEntry.CreateLoadEntry(containerId, sourceArea);
            manifest.AddEntry(entry1);
            manifest.RemoveEntry(entry1);

            // Act
            var entry2 = ManifestEntry.CreateLoadEntry(containerId, sourceArea);
            manifest.AddEntry(entry2);

            // Assert
            manifest.Entries.Should().HaveCount(1);
        }

        #endregion

        #region Real-World Scenarios

        [Fact]
        public void CargoManifest_LoadingVesselWithMultipleContainers_ShouldCreateValidManifest()
        {
            // Arrange - Vessel loading 10 containers from yard
            var manifest = new CargoManifest(ManifestType.Load);
            var yardArea = new StorageAreaId(Guid.NewGuid());

            // Act - Add 10 containers to loading manifest
            for (int i = 0; i < 10; i++)
            {
                manifest.AddEntry(ManifestEntry.CreateLoadEntry(
                    new ContainerId(Guid.NewGuid()),
                    yardArea));
            }

            // Assert
            manifest.Entries.Should().HaveCount(10);
            manifest.CalculateEstimatedTeu().Should().Be(10);
        }

        [Fact]
        public void CargoManifest_UnloadingVesselToMultipleAreas_ShouldCreateValidManifest()
        {
            // Arrange - Vessel unloading containers to different storage areas
            var manifest = new CargoManifest(ManifestType.Unload);
            var warehouseA = new StorageAreaId(Guid.NewGuid());
            var warehouseB = new StorageAreaId(Guid.NewGuid());
            var yardC = new StorageAreaId(Guid.NewGuid());

            // Act
            manifest.AddEntry(ManifestEntry.CreateUnloadEntry(new ContainerId(Guid.NewGuid()), warehouseA));
            manifest.AddEntry(ManifestEntry.CreateUnloadEntry(new ContainerId(Guid.NewGuid()), warehouseA));
            manifest.AddEntry(ManifestEntry.CreateUnloadEntry(new ContainerId(Guid.NewGuid()), warehouseB));
            manifest.AddEntry(ManifestEntry.CreateUnloadEntry(new ContainerId(Guid.NewGuid()), warehouseB));
            manifest.AddEntry(ManifestEntry.CreateUnloadEntry(new ContainerId(Guid.NewGuid()), yardC));

            // Assert
            manifest.Entries.Should().HaveCount(5);
            manifest.Entries.Select(e => e.TargetStorageAreaId).Distinct().Should().HaveCount(3);
        }

        [Fact]
        public void CargoManifest_ModifyingManifestBeforeSubmission_ShouldAllowChanges()
        {
            // Arrange - User creating manifest and making changes
            var manifest = new CargoManifest(ManifestType.Load);
            var sourceArea = new StorageAreaId(Guid.NewGuid());
            var entry1 = ManifestEntry.CreateLoadEntry(new ContainerId(Guid.NewGuid()), sourceArea);
            var entry2 = ManifestEntry.CreateLoadEntry(new ContainerId(Guid.NewGuid()), sourceArea);

            // Act - Add, then realize mistake and remove one
            manifest.AddEntry(entry1);
            manifest.AddEntry(entry2);
            manifest.RemoveEntry(entry1);

            // Assert
            manifest.Entries.Should().HaveCount(1);
            manifest.Entries.Should().Contain(entry2);
        }

        [Fact]
        public void CargoManifest_EmptyManifest_ShouldBeValidForOptionalCargo()
        {
            // Arrange - VVN with no cargo operations (just vessel visit)
            var loadManifest = new CargoManifest(ManifestType.Load);
            var unloadManifest = new CargoManifest(ManifestType.Unload);

            // Assert - Both manifests can be empty
            loadManifest.Entries.Should().BeEmpty();
            unloadManifest.Entries.Should().BeEmpty();
            loadManifest.CalculateEstimatedTeu().Should().Be(0);
            unloadManifest.CalculateEstimatedTeu().Should().Be(0);
        }

        [Fact]
        public void CargoManifest_LargeVesselWithManyContainers_ShouldHandleScale()
        {
            // Arrange - Large container vessel with 100+ containers
            var manifest = new CargoManifest(ManifestType.Unload);
            var warehouseArea = new StorageAreaId(Guid.NewGuid());

            // Act - Add 150 containers
            for (int i = 0; i < 150; i++)
            {
                manifest.AddEntry(ManifestEntry.CreateUnloadEntry(
                    new ContainerId(Guid.NewGuid()),
                    warehouseArea));
            }

            // Assert
            manifest.Entries.Should().HaveCount(150);
            manifest.CalculateEstimatedTeu().Should().Be(150);
        }

        [Fact]
        public void ValidateConsistency_BeforeVVNSubmission_ShouldValidateAllEntries()
        {
            // Arrange - VVN about to be submitted
            var manifest = new CargoManifest(ManifestType.Load);
            var sourceArea = new StorageAreaId(Guid.NewGuid());
            manifest.AddEntry(ManifestEntry.CreateLoadEntry(new ContainerId(Guid.NewGuid()), sourceArea));
            manifest.AddEntry(ManifestEntry.CreateLoadEntry(new ContainerId(Guid.NewGuid()), sourceArea));
            manifest.AddEntry(ManifestEntry.CreateLoadEntry(new ContainerId(Guid.NewGuid()), sourceArea));

            // Act
            Action act = () => manifest.ValidateConsistency();

            // Assert - Should validate without errors
            act.Should().NotThrow();
        }

        [Fact]
        public void CargoManifest_LoadFromMultipleSources_ShouldCreateValidManifest()
        {
            // Arrange - Loading containers from different yards
            var manifest = new CargoManifest(ManifestType.Load);
            var yardA = new StorageAreaId(Guid.NewGuid());
            var yardB = new StorageAreaId(Guid.NewGuid());
            var warehouseC = new StorageAreaId(Guid.NewGuid());

            // Act
            manifest.AddEntry(ManifestEntry.CreateLoadEntry(new ContainerId(Guid.NewGuid()), yardA));
            manifest.AddEntry(ManifestEntry.CreateLoadEntry(new ContainerId(Guid.NewGuid()), yardA));
            manifest.AddEntry(ManifestEntry.CreateLoadEntry(new ContainerId(Guid.NewGuid()), yardB));
            manifest.AddEntry(ManifestEntry.CreateLoadEntry(new ContainerId(Guid.NewGuid()), warehouseC));

            // Assert
            manifest.Entries.Should().HaveCount(4);
            manifest.Entries.Select(e => e.SourceStorageAreaId).Distinct().Should().HaveCount(3);
        }

        #endregion
    }
}
