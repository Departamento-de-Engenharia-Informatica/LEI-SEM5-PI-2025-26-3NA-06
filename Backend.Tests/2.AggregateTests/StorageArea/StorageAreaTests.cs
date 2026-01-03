using Xunit;
using FluentAssertions;
using ProjArqsi.Domain.StorageAreaAggregate;
using ProjArqsi.Domain.StorageArea.ValueObjects;
using ProjArqsi.Domain.DockAggregate;
using ProjArqsi.Domain.Shared;

namespace Backend.Tests.AggregateTests.StorageArea
{
    public class StorageAreaTests
    {
        #region Constructor Tests

        [Fact]
        public void Constructor_WithValidParameters_ShouldCreateStorageArea()
        {
            // Arrange
            var name = new AreaName("Container Yard A");
            var type = new AreaType(AreaTypeEnum.Yard);
            var location = new Location("Port of Leixões - North Terminal");
            var maxCapacity = new MaxCapacity(500);
            var servesEntirePort = true;
            var servedDocks = new ServedDocks(new List<DockId> { new DockId(Guid.NewGuid()) });

            // Act
            var storageArea = new ProjArqsi.Domain.StorageAreaAggregate.StorageArea(
                name, type, location, maxCapacity, servesEntirePort, servedDocks);

            // Assert
            storageArea.Should().NotBeNull();
            storageArea.Id.Should().NotBeNull();
            storageArea.Name.Should().Be(name);
            storageArea.AreaType.Should().Be(type);
            storageArea.Location.Should().Be(location);
            storageArea.MaxCapacity.Should().Be(maxCapacity);
            storageArea.ServesEntirePort.Should().Be(servesEntirePort);
            storageArea.ServedDocks.Should().Be(servedDocks);
            storageArea.CurrentContainers.Should().NotBeNull();
        }

        [Fact]
        public void Constructor_ShouldGenerateUniqueIds()
        {
            // Arrange
            var name = new AreaName("Yard B");
            var type = new AreaType(AreaTypeEnum.Warehouse);
            var location = new Location("Port of Leixões");
            var maxCapacity = new MaxCapacity(300);
            var servedDocks = new ServedDocks(new List<DockId> { new DockId(Guid.NewGuid()) });

            // Act
            var area1 = new ProjArqsi.Domain.StorageAreaAggregate.StorageArea(
                name, type, location, maxCapacity, false, servedDocks);
            var area2 = new ProjArqsi.Domain.StorageAreaAggregate.StorageArea(
                name, type, location, maxCapacity, false, servedDocks);

            // Assert
            area1.Id.Should().NotBe(area2.Id);
        }

        [Fact]
        public void Constructor_ShouldInitializeCurrentContainersAsEmpty()
        {
            // Arrange
            var name = new AreaName("Empty Yard");
            var type = new AreaType(AreaTypeEnum.Yard);
            var location = new Location("Port");
            var maxCapacity = new MaxCapacity(100);
            var servedDocks = new ServedDocks(new List<DockId> { new DockId(Guid.NewGuid()) });

            // Act
            var storageArea = new ProjArqsi.Domain.StorageAreaAggregate.StorageArea(
                name, type, location, maxCapacity, true, servedDocks);

            // Assert
            storageArea.CurrentContainers.Should().NotBeNull();
            storageArea.CurrentContainers.Value.Should().BeEmpty();
        }

        [Fact]
        public void Constructor_WithServesEntirePortTrue_ShouldSetPropertyCorrectly()
        {
            // Arrange & Act
            var storageArea = CreateValidStorageArea(servesEntirePort: true);

            // Assert
            storageArea.ServesEntirePort.Should().BeTrue();
        }

        [Fact]
        public void Constructor_WithServesEntirePortFalse_ShouldSetPropertyCorrectly()
        {
            // Arrange & Act
            var storageArea = CreateValidStorageArea(servesEntirePort: false);

            // Assert
            storageArea.ServesEntirePort.Should().BeFalse();
        }

        [Fact]
        public void Constructor_WithNullName_ShouldThrowBusinessRuleValidationException()
        {
            // Arrange
            var type = new AreaType(AreaTypeEnum.Yard);
            var location = new Location("Port");
            var maxCapacity = new MaxCapacity(200);
            var servedDocks = new ServedDocks(new List<DockId> { new DockId(Guid.NewGuid()) });

            // Act
            Action act = () => new ProjArqsi.Domain.StorageAreaAggregate.StorageArea(
                null!, type, location, maxCapacity, true, servedDocks);

            // Assert
            act.Should().Throw<BusinessRuleValidationException>()
                .WithMessage("Area name is required.");
        }

        [Fact]
        public void Constructor_WithNullAreaType_ShouldThrowBusinessRuleValidationException()
        {
            // Arrange
            var name = new AreaName("Yard C");
            var location = new Location("Port");
            var maxCapacity = new MaxCapacity(200);
            var servedDocks = new ServedDocks(new List<DockId> { new DockId(Guid.NewGuid()) });

            // Act
            Action act = () => new ProjArqsi.Domain.StorageAreaAggregate.StorageArea(
                name, null!, location, maxCapacity, true, servedDocks);

            // Assert
            act.Should().Throw<BusinessRuleValidationException>()
                .WithMessage("Area type is required.");
        }

        [Fact]
        public void Constructor_WithNullLocation_ShouldThrowBusinessRuleValidationException()
        {
            // Arrange
            var name = new AreaName("Yard D");
            var type = new AreaType(AreaTypeEnum.Warehouse);
            var maxCapacity = new MaxCapacity(200);
            var servedDocks = new ServedDocks(new List<DockId> { new DockId(Guid.NewGuid()) });

            // Act
            Action act = () => new ProjArqsi.Domain.StorageAreaAggregate.StorageArea(
                name, type, null!, maxCapacity, true, servedDocks);

            // Assert
            act.Should().Throw<BusinessRuleValidationException>()
                .WithMessage("Location is required.");
        }

        [Fact]
        public void Constructor_WithNullMaxCapacity_ShouldThrowBusinessRuleValidationException()
        {
            // Arrange
            var name = new AreaName("Yard E");
            var type = new AreaType(AreaTypeEnum.Yard);
            var location = new Location("Port");
            var servedDocks = new ServedDocks(new List<DockId> { new DockId(Guid.NewGuid()) });

            // Act
            Action act = () => new ProjArqsi.Domain.StorageAreaAggregate.StorageArea(
                name, type, location, null!, true, servedDocks);

            // Assert
            act.Should().Throw<BusinessRuleValidationException>()
                .WithMessage("Max capacity is required.");
        }

        [Fact]
        public void Constructor_WithNullServedDocks_ShouldThrowBusinessRuleValidationException()
        {
            // Arrange
            var name = new AreaName("Yard F");
            var type = new AreaType(AreaTypeEnum.Yard);
            var location = new Location("Port");
            var maxCapacity = new MaxCapacity(200);

            // Act
            Action act = () => new ProjArqsi.Domain.StorageAreaAggregate.StorageArea(
                name, type, location, maxCapacity, true, null!);

            // Assert
            act.Should().Throw<BusinessRuleValidationException>()
                .WithMessage("Served docks are required.");
        }

        #endregion

        #region UpdateDetails Tests

        [Fact]
        public void UpdateDetails_WithValidParameters_ShouldUpdateAllProperties()
        {
            // Arrange
            var storageArea = CreateValidStorageArea();
            var newName = new AreaName("Updated Yard Name");
            var newType = new AreaType(AreaTypeEnum.Warehouse);
            var newLocation = new Location("New Location");
            var newMaxCapacity = new MaxCapacity(1000);
            var newServedDocks = new ServedDocks(new List<DockId> 
            { 
                new DockId(Guid.NewGuid()), 
                new DockId(Guid.NewGuid()) 
            });

            // Act
            storageArea.UpdateDetails(newName, newType, newLocation, newMaxCapacity, false, newServedDocks);

            // Assert
            storageArea.Name.Should().Be(newName);
            storageArea.AreaType.Should().Be(newType);
            storageArea.Location.Should().Be(newLocation);
            storageArea.MaxCapacity.Should().Be(newMaxCapacity);
            storageArea.ServesEntirePort.Should().BeFalse();
            storageArea.ServedDocks.Should().Be(newServedDocks);
        }

        [Fact]
        public void UpdateDetails_ShouldPreserveId()
        {
            // Arrange
            var storageArea = CreateValidStorageArea();
            var originalId = storageArea.Id;
            var newName = new AreaName("Updated");
            var newType = new AreaType(AreaTypeEnum.Warehouse);
            var newLocation = new Location("Updated Location");
            var newMaxCapacity = new MaxCapacity(800);
            var newServedDocks = new ServedDocks(new List<DockId> { new DockId(Guid.NewGuid()) });

            // Act
            storageArea.UpdateDetails(newName, newType, newLocation, newMaxCapacity, true, newServedDocks);

            // Assert
            storageArea.Id.Should().Be(originalId);
        }

        [Fact]
        public void UpdateDetails_ShouldPreserveCurrentContainers()
        {
            // Arrange
            var storageArea = CreateValidStorageArea();
            var originalContainers = storageArea.CurrentContainers;
            var newName = new AreaName("Updated");
            var newType = new AreaType(AreaTypeEnum.Yard);
            var newLocation = new Location("Location");
            var newMaxCapacity = new MaxCapacity(600);
            var newServedDocks = new ServedDocks(new List<DockId> { new DockId(Guid.NewGuid()) });

            // Act
            storageArea.UpdateDetails(newName, newType, newLocation, newMaxCapacity, false, newServedDocks);

            // Assert
            storageArea.CurrentContainers.Should().Be(originalContainers);
        }

        [Fact]
        public void UpdateDetails_CanChangeServesEntirePortFromTrueToFalse()
        {
            // Arrange
            var storageArea = CreateValidStorageArea(servesEntirePort: true);
            var newName = new AreaName("Specialized Yard");
            var newType = new AreaType(AreaTypeEnum.Warehouse);
            var newLocation = new Location("Location");
            var newMaxCapacity = new MaxCapacity(400);
            var newServedDocks = new ServedDocks(new List<DockId> { new DockId(Guid.NewGuid()) });

            // Act
            storageArea.UpdateDetails(newName, newType, newLocation, newMaxCapacity, false, newServedDocks);

            // Assert
            storageArea.ServesEntirePort.Should().BeFalse();
        }

        [Fact]
        public void UpdateDetails_CanChangeServesEntirePortFromFalseToTrue()
        {
            // Arrange
            var storageArea = CreateValidStorageArea(servesEntirePort: false);
            var newName = new AreaName("General Yard");
            var newType = new AreaType(AreaTypeEnum.Yard);
            var newLocation = new Location("Location");
            var newMaxCapacity = new MaxCapacity(400);
            var newServedDocks = new ServedDocks(new List<DockId> { new DockId(Guid.NewGuid()) });

            // Act
            storageArea.UpdateDetails(newName, newType, newLocation, newMaxCapacity, true, newServedDocks);

            // Assert
            storageArea.ServesEntirePort.Should().BeTrue();
        }

        [Fact]
        public void UpdateDetails_WithNullName_ShouldThrowBusinessRuleValidationException()
        {
            // Arrange
            var storageArea = CreateValidStorageArea();
            var newType = new AreaType(AreaTypeEnum.Yard);
            var newLocation = new Location("Location");
            var newMaxCapacity = new MaxCapacity(300);
            var newServedDocks = new ServedDocks(new List<DockId> { new DockId(Guid.NewGuid()) });

            // Act
            Action act = () => storageArea.UpdateDetails(
                null!, newType, newLocation, newMaxCapacity, true, newServedDocks);

            // Assert
            act.Should().Throw<BusinessRuleValidationException>()
                .WithMessage("Area name is required.");
        }

        [Fact]
        public void UpdateDetails_WithNullAreaType_ShouldThrowBusinessRuleValidationException()
        {
            // Arrange
            var storageArea = CreateValidStorageArea();
            var newName = new AreaName("Name");
            var newLocation = new Location("Location");
            var newMaxCapacity = new MaxCapacity(300);
            var newServedDocks = new ServedDocks(new List<DockId> { new DockId(Guid.NewGuid()) });

            // Act
            Action act = () => storageArea.UpdateDetails(
                newName, null!, newLocation, newMaxCapacity, true, newServedDocks);

            // Assert
            act.Should().Throw<BusinessRuleValidationException>()
                .WithMessage("Area type is required.");
        }

        [Fact]
        public void UpdateDetails_WithNullLocation_ShouldThrowBusinessRuleValidationException()
        {
            // Arrange
            var storageArea = CreateValidStorageArea();
            var newName = new AreaName("Name");
            var newType = new AreaType(AreaTypeEnum.Warehouse);
            var newMaxCapacity = new MaxCapacity(300);
            var newServedDocks = new ServedDocks(new List<DockId> { new DockId(Guid.NewGuid()) });

            // Act
            Action act = () => storageArea.UpdateDetails(
                newName, newType, null!, newMaxCapacity, true, newServedDocks);

            // Assert
            act.Should().Throw<BusinessRuleValidationException>()
                .WithMessage("Location is required.");
        }

        [Fact]
        public void UpdateDetails_WithNullMaxCapacity_ShouldThrowBusinessRuleValidationException()
        {
            // Arrange
            var storageArea = CreateValidStorageArea();
            var newName = new AreaName("Name");
            var newType = new AreaType(AreaTypeEnum.Yard);
            var newLocation = new Location("Location");
            var newServedDocks = new ServedDocks(new List<DockId> { new DockId(Guid.NewGuid()) });

            // Act
            Action act = () => storageArea.UpdateDetails(
                newName, newType, newLocation, null!, true, newServedDocks);

            // Assert
            act.Should().Throw<BusinessRuleValidationException>()
                .WithMessage("Max capacity is required.");
        }

        [Fact]
        public void UpdateDetails_WithNullServedDocks_ShouldThrowBusinessRuleValidationException()
        {
            // Arrange
            var storageArea = CreateValidStorageArea();
            var newName = new AreaName("Name");
            var newType = new AreaType(AreaTypeEnum.Warehouse);
            var newLocation = new Location("Location");
            var newMaxCapacity = new MaxCapacity(300);

            // Act
            Action act = () => storageArea.UpdateDetails(
                newName, newType, newLocation, newMaxCapacity, true, null!);

            // Assert
            act.Should().Throw<BusinessRuleValidationException>()
                .WithMessage("Served docks are required.");
        }

        [Fact]
        public void UpdateDetails_CalledMultipleTimes_ShouldAlwaysUpdateToLatestValues()
        {
            // Arrange
            var storageArea = CreateValidStorageArea();

            var update1Name = new AreaName("First Update");
            var update1Type = new AreaType(AreaTypeEnum.Yard);
            var update1Location = new Location("Location 1");
            var update1Capacity = new MaxCapacity(250);
            var update1Docks = new ServedDocks(new List<DockId> { new DockId(Guid.NewGuid()) });

            var update2Name = new AreaName("Second Update");
            var update2Type = new AreaType(AreaTypeEnum.Warehouse);
            var update2Location = new Location("Location 2");
            var update2Capacity = new MaxCapacity(750);
            var update2Docks = new ServedDocks(new List<DockId> 
            { 
                new DockId(Guid.NewGuid()), 
                new DockId(Guid.NewGuid()) 
            });

            // Act
            storageArea.UpdateDetails(update1Name, update1Type, update1Location, update1Capacity, false, update1Docks);
            storageArea.UpdateDetails(update2Name, update2Type, update2Location, update2Capacity, true, update2Docks);

            // Assert
            storageArea.Name.Should().Be(update2Name);
            storageArea.AreaType.Should().Be(update2Type);
            storageArea.Location.Should().Be(update2Location);
            storageArea.MaxCapacity.Should().Be(update2Capacity);
            storageArea.ServesEntirePort.Should().BeTrue();
            storageArea.ServedDocks.Should().Be(update2Docks);
        }

        #endregion

        #region Real-World Scenarios

        [Fact]
        public void Scenario_GeneralPurposeYard_ServingEntirePort()
        {
            // Arrange - Large general-purpose yard serving all docks
            var name = new AreaName("Main Container Yard");
            var type = new AreaType(AreaTypeEnum.Yard);
            var location = new Location("Port of Leixões - Central Area");
            var maxCapacity = new MaxCapacity(2000); // 2000 containers
            var dock1 = new DockId(Guid.NewGuid());
            var dock2 = new DockId(Guid.NewGuid());
            var dock3 = new DockId(Guid.NewGuid());
            var servedDocks = new ServedDocks(new List<DockId> { dock1, dock2, dock3 });

            // Act
            var storageArea = new ProjArqsi.Domain.StorageAreaAggregate.StorageArea(
                name, type, location, maxCapacity, true, servedDocks);

            // Assert
            storageArea.Name.Value.Should().Be("Main Container Yard");
            storageArea.AreaType.Value.Should().Be(AreaTypeEnum.Yard);
            storageArea.MaxCapacity.Value.Should().Be(2000);
            storageArea.ServesEntirePort.Should().BeTrue();
            storageArea.ServedDocks.Value.Should().HaveCount(3);
            storageArea.CurrentContainers.Value.Should().BeEmpty();
        }

        [Fact]
        public void Scenario_RefrigeratedContainerYard_SpecializedStorage()
        {
            // Arrange - Specialized refrigerated container storage
            var name = new AreaName("Reefer Yard");
            var type = new AreaType(AreaTypeEnum.Warehouse);
            var location = new Location("Port of Leixões - South Terminal - Reefer Zone");
            var maxCapacity = new MaxCapacity(200); // Smaller capacity for specialized storage
            var reeferDock = new DockId(Guid.NewGuid());
            var servedDocks = new ServedDocks(new List<DockId> { reeferDock });

            // Act
            var storageArea = new ProjArqsi.Domain.StorageAreaAggregate.StorageArea(
                name, type, location, maxCapacity, false, servedDocks);

            // Assert
            storageArea.Name.Value.Should().Be("Reefer Yard");
            storageArea.AreaType.Value.Should().Be(AreaTypeEnum.Warehouse);
            storageArea.ServesEntirePort.Should().BeFalse();
            storageArea.ServedDocks.Value.Should().HaveCount(1);
        }

        [Fact]
        public void Scenario_HazardousCargoArea_RestrictedAccess()
        {
            // Arrange - Hazardous cargo storage with strict controls
            var name = new AreaName("Hazmat Storage Zone");
            var type = new AreaType(AreaTypeEnum.Warehouse);
            var location = new Location("Port of Leixões - Restricted Zone Alpha");
            var maxCapacity = new MaxCapacity(50); // Very limited capacity
            var hazmatDock = new DockId(Guid.NewGuid());
            var servedDocks = new ServedDocks(new List<DockId> { hazmatDock });

            // Act
            var storageArea = new ProjArqsi.Domain.StorageAreaAggregate.StorageArea(
                name, type, location, maxCapacity, false, servedDocks);

            // Assert
            storageArea.Name.Value.Should().Contain("Hazmat");
            storageArea.MaxCapacity.Value.Should().Be(50);
            storageArea.ServesEntirePort.Should().BeFalse();
        }

        [Fact]
        public void Scenario_TemporaryStorageExpansion_IncreaseCapacity()
        {
            // Arrange - Port experiencing high demand, needs to expand storage
            var storageArea = new ProjArqsi.Domain.StorageAreaAggregate.StorageArea(
                new AreaName("Overflow Yard A"),
                new AreaType(AreaTypeEnum.Yard),
                new Location("Port of Leixões - East Extension"),
                new MaxCapacity(300),
                true,
                new ServedDocks(new List<DockId> { new DockId(Guid.NewGuid()) })
            );

            // Act - Expand capacity to handle peak season
            storageArea.UpdateDetails(
                new AreaName("Overflow Yard A - Expanded"),
                new AreaType(AreaTypeEnum.Yard),
                new Location("Port of Leixões - East Extension"),
                new MaxCapacity(600), // Doubled capacity
                true,
                storageArea.ServedDocks
            );

            // Assert
            storageArea.Name.Value.Should().Contain("Expanded");
            storageArea.MaxCapacity.Value.Should().Be(600);
        }

        [Fact]
        public void Scenario_ConvertGeneralToSpecialized_UpdateServesEntirePort()
        {
            // Arrange - Converting general yard to specialized use
            var storageArea = new ProjArqsi.Domain.StorageAreaAggregate.StorageArea(
                new AreaName("Yard 5"),
                new AreaType(AreaTypeEnum.Yard),
                new Location("Port of Leixões - West Basin"),
                new MaxCapacity(500),
                true, // Originally serves entire port
                new ServedDocks(new List<DockId> 
                { 
                    new DockId(Guid.NewGuid()), 
                    new DockId(Guid.NewGuid()) 
                })
            );

            // Act - Convert to specialized automotive cargo storage
            var automotiveDock = new DockId(Guid.NewGuid());
            storageArea.UpdateDetails(
                new AreaName("Automotive Cargo Yard"),
                new AreaType(AreaTypeEnum.Yard),
                storageArea.Location,
                new MaxCapacity(300),
                false, // Now specialized, not serving entire port
                new ServedDocks(new List<DockId> { automotiveDock })
            );

            // Assert
            storageArea.Name.Value.Should().Be("Automotive Cargo Yard");
            storageArea.AreaType.Value.Should().Be(AreaTypeEnum.Yard);
            storageArea.ServesEntirePort.Should().BeFalse();
            storageArea.ServedDocks.Value.Should().HaveCount(1);
        }

        [Fact]
        public void Scenario_EmptyYardRelocation_UpdateLocationOnly()
        {
            // Arrange - Administrative change to yard location designation
            var storageArea = new ProjArqsi.Domain.StorageAreaAggregate.StorageArea(
                new AreaName("Terminal 3 Yard"),
                new AreaType(AreaTypeEnum.Yard),
                new Location("Zone B"),
                new MaxCapacity(400),
                true,
                new ServedDocks(new List<DockId> { new DockId(Guid.NewGuid()) })
            );

            // Act - Update location description after port reorganization
            storageArea.UpdateDetails(
                storageArea.Name,
                storageArea.AreaType,
                new Location("Port of Leixões - Terminal 3 - Zone B (Reorganized)"),
                storageArea.MaxCapacity,
                storageArea.ServesEntirePort,
                storageArea.ServedDocks
            );

            // Assert
            storageArea.Location.Description.Should().Contain("Reorganized");
            storageArea.Name.Value.Should().Be("Terminal 3 Yard");
        }

        [Fact]
        public void Scenario_CoveredStorage_WeatherProtection()
        {
            // Arrange - Covered storage for weather-sensitive cargo
            var name = new AreaName("Covered Warehouse 1");
            var type = new AreaType(AreaTypeEnum.Warehouse);
            var location = new Location("Port of Leixões - Covered Facilities Section");
            var maxCapacity = new MaxCapacity(150); // Lower capacity due to building constraints
            var servedDocks = new ServedDocks(new List<DockId> 
            { 
                new DockId(Guid.NewGuid()), 
                new DockId(Guid.NewGuid()) 
            });

            // Act
            var storageArea = new ProjArqsi.Domain.StorageAreaAggregate.StorageArea(
                name, type, location, maxCapacity, false, servedDocks);

            // Assert
            storageArea.Name.Value.Should().Contain("Covered");
            storageArea.AreaType.Value.Should().Be(AreaTypeEnum.Warehouse);
            storageArea.MaxCapacity.Value.Should().Be(150);
        }

        [Fact]
        public void Scenario_TransshipmentHub_HighTurnoverYard()
        {
            // Arrange - High-throughput transshipment yard
            var name = new AreaName("Transshipment Hub");
            var type = new AreaType(AreaTypeEnum.Yard);
            var location = new Location("Port of Leixões - Central Transshipment Area");
            var maxCapacity = new MaxCapacity(1500);
            var servedDocks = new ServedDocks(new List<DockId> 
            { 
                new DockId(Guid.NewGuid()), 
                new DockId(Guid.NewGuid()),
                new DockId(Guid.NewGuid()),
                new DockId(Guid.NewGuid())
            });

            // Act
            var storageArea = new ProjArqsi.Domain.StorageAreaAggregate.StorageArea(
                name, type, location, maxCapacity, true, servedDocks);

            // Assert
            storageArea.ServedDocks.Value.Should().HaveCount(4);
            storageArea.ServesEntirePort.Should().BeTrue();
            storageArea.MaxCapacity.Value.Should().Be(1500);
        }

        #endregion

        #region Identity and Equality Tests

        [Fact]
        public void StorageArea_WithSameId_ShouldBeConsideredEqual()
        {
            // Arrange
            var areaId = new StorageAreaId(Guid.NewGuid());
            var area1 = CreateValidStorageArea();
            var area2 = CreateValidStorageArea();

            // Use reflection to set same ID
            var idProperty = typeof(ProjArqsi.Domain.StorageAreaAggregate.StorageArea).GetProperty("Id");
            idProperty!.SetValue(area1, areaId);
            idProperty!.SetValue(area2, areaId);

            // Act & Assert
            area1.Id.Should().Be(area2.Id);
        }

        [Fact]
        public void StorageArea_WithDifferentIds_ShouldNotBeEqual()
        {
            // Arrange & Act
            var area1 = CreateValidStorageArea();
            var area2 = CreateValidStorageArea();

            // Assert
            area1.Id.Should().NotBe(area2.Id);
        }

        [Fact]
        public void StorageArea_IdShouldBeImmutable()
        {
            // Arrange
            var storageArea = CreateValidStorageArea();
            var originalId = storageArea.Id;

            // Act - Update all properties
            storageArea.UpdateDetails(
                new AreaName("New Name"),
                new AreaType(AreaTypeEnum.Warehouse),
                new Location("New Location"),
                new MaxCapacity(999),
                false,
                new ServedDocks(new List<DockId> { new DockId(Guid.NewGuid()) })
            );

            // Assert
            storageArea.Id.Should().Be(originalId);
        }

        [Fact]
        public void StorageArea_AfterMultipleUpdates_IdShouldRemainUnchanged()
        {
            // Arrange
            var storageArea = CreateValidStorageArea();
            var originalId = storageArea.Id;

            // Act - Multiple updates
            for (int i = 0; i < 5; i++)
            {
                storageArea.UpdateDetails(
                    new AreaName($"Update {i}"),
                    new AreaType(i % 2 == 0 ? AreaTypeEnum.Yard : AreaTypeEnum.Warehouse),
                    new Location($"Location {i}"),
                    new MaxCapacity(100 + i * 50),
                    i % 2 == 0,
                    new ServedDocks(new List<DockId> { new DockId(Guid.NewGuid()) })
                );
            }

            // Assert
            storageArea.Id.Should().Be(originalId);
        }

        #endregion

        #region Edge Cases

        [Fact]
        public void StorageArea_WithVeryHighCapacity_ShouldBeCreated()
        {
            // Arrange & Act
            var storageArea = new ProjArqsi.Domain.StorageAreaAggregate.StorageArea(
                new AreaName("Mega Yard"),
                new AreaType(AreaTypeEnum.Yard),
                new Location("Port"),
                new MaxCapacity(10000), // Very high capacity
                true,
                new ServedDocks(new List<DockId> { new DockId(Guid.NewGuid()) })
            );

            // Assert
            storageArea.MaxCapacity.Value.Should().Be(10000);
        }

        [Fact]
        public void StorageArea_WithSingleDock_ShouldBeCreated()
        {
            // Arrange & Act
            var storageArea = new ProjArqsi.Domain.StorageAreaAggregate.StorageArea(
                new AreaName("Single Dock Yard"),
                new AreaType(AreaTypeEnum.Warehouse),
                new Location("Port"),
                new MaxCapacity(100),
                false,
                new ServedDocks(new List<DockId> { new DockId(Guid.NewGuid()) })
            );

            // Assert
            storageArea.ServedDocks.Value.Should().HaveCount(1);
        }

        [Fact]
        public void StorageArea_WithManyDocks_ShouldBeCreated()
        {
            // Arrange
            var manyDocks = Enumerable.Range(0, 10)
                .Select(_ => new DockId(Guid.NewGuid()))
                .ToList();

            // Act
            var storageArea = new ProjArqsi.Domain.StorageAreaAggregate.StorageArea(
                new AreaName("Multi-Dock Yard"),
                new AreaType(AreaTypeEnum.Yard),
                new Location("Port"),
                new MaxCapacity(500),
                true,
                new ServedDocks(manyDocks)
            );

            // Assert
            storageArea.ServedDocks.Value.Should().HaveCount(10);
        }

        [Fact]
        public void StorageArea_WithVeryLongNameAndDescription_ShouldBeCreated()
        {
            // Arrange
            var longName = new string('A', 100); // Maximum allowed length
            var longLocation = new string('B', 500);

            // Act
            var storageArea = new ProjArqsi.Domain.StorageAreaAggregate.StorageArea(
                new AreaName(longName),
                new AreaType(AreaTypeEnum.Warehouse),
                new Location(longLocation),
                new MaxCapacity(300),
                true,
                new ServedDocks(new List<DockId> { new DockId(Guid.NewGuid()) })
            );

            // Assert
            storageArea.Name.Value.Length.Should().Be(100);
            storageArea.Location.Description.Length.Should().Be(500);
        }

        #endregion

        #region Helper Methods

        private static ProjArqsi.Domain.StorageAreaAggregate.StorageArea CreateValidStorageArea(bool servesEntirePort = true)
        {
            return new ProjArqsi.Domain.StorageAreaAggregate.StorageArea(
                new AreaName("Test Storage Area"),
                new AreaType(AreaTypeEnum.Yard),
                new Location("Test Location"),
                new MaxCapacity(500),
                servesEntirePort,
                new ServedDocks(new List<DockId> { new DockId(Guid.NewGuid()) })
            );
        }

        #endregion
    }
}
