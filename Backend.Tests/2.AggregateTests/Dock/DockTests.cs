using Xunit;
using FluentAssertions;
using ProjArqsi.Domain.DockAggregate;
using ProjArqsi.Domain.Shared;

namespace Backend.Tests.AggregateTests.Dock
{
    public class DockTests
    {
        #region Constructor Tests

        [Fact]
        public void Constructor_WithValidParameters_ShouldCreateDock()
        {
            // Arrange
            var dockName = new DockName("Dock Alpha");
            var location = new Location("Port of Leixões - Section A");
            var length = new DockLength(250.5);
            var depth = new Depth(12.5);
            var maxDraft = new Draft(10.5);
            var allowedTypes = new AllowedVesselTypes(new List<Guid> { Guid.NewGuid(), Guid.NewGuid() });

            // Act
            var dock = new ProjArqsi.Domain.DockAggregate.Dock(dockName, location, length, depth, maxDraft, allowedTypes);

            // Assert
            dock.Should().NotBeNull();
            dock.Id.Should().NotBeNull();
            dock.DockName.Should().Be(dockName);
            dock.Location.Should().Be(location);
            dock.Length.Should().Be(length);
            dock.Depth.Should().Be(depth);
            dock.MaxDraft.Should().Be(maxDraft);
            dock.AllowedVesselTypes.Should().Be(allowedTypes);
        }

        [Fact]
        public void Constructor_ShouldGenerateUniqueIds()
        {
            // Arrange
            var dockName = new DockName("Dock Beta");
            var location = new Location("Port of Leixões - Section B");
            var length = new DockLength(180.0);
            var depth = new Depth(10.0);
            var maxDraft = new Draft(8.5);
            var allowedTypes = new AllowedVesselTypes(new List<Guid> { Guid.NewGuid() });

            // Act
            var dock1 = new ProjArqsi.Domain.DockAggregate.Dock(dockName, location, length, depth, maxDraft, allowedTypes);
            var dock2 = new ProjArqsi.Domain.DockAggregate.Dock(dockName, location, length, depth, maxDraft, allowedTypes);

            // Assert
            dock1.Id.Should().NotBe(dock2.Id);
        }

        [Fact]
        public void Constructor_WithNullDockName_ShouldThrowBusinessRuleValidationException()
        {
            // Arrange
            var location = new Location("Port of Leixões");
            var length = new DockLength(200.0);
            var depth = new Depth(10.0);
            var maxDraft = new Draft(8.0);
            var allowedTypes = new AllowedVesselTypes(new List<Guid> { Guid.NewGuid() });

            // Act
            Action act = () => new ProjArqsi.Domain.DockAggregate.Dock(null!, location, length, depth, maxDraft, allowedTypes);

            // Assert
            act.Should().Throw<BusinessRuleValidationException>()
                .WithMessage("Dock name is required.");
        }

        [Fact]
        public void Constructor_WithNullLocation_ShouldThrowBusinessRuleValidationException()
        {
            // Arrange
            var dockName = new DockName("Dock Gamma");
            var length = new DockLength(200.0);
            var depth = new Depth(10.0);
            var maxDraft = new Draft(8.0);
            var allowedTypes = new AllowedVesselTypes(new List<Guid> { Guid.NewGuid() });

            // Act
            Action act = () => new ProjArqsi.Domain.DockAggregate.Dock(dockName, null!, length, depth, maxDraft, allowedTypes);

            // Assert
            act.Should().Throw<BusinessRuleValidationException>()
                .WithMessage("Location is required.");
        }

        [Fact]
        public void Constructor_WithNullLength_ShouldThrowBusinessRuleValidationException()
        {
            // Arrange
            var dockName = new DockName("Dock Delta");
            var location = new Location("Port of Leixões");
            var depth = new Depth(10.0);
            var maxDraft = new Draft(8.0);
            var allowedTypes = new AllowedVesselTypes(new List<Guid> { Guid.NewGuid() });

            // Act
            Action act = () => new ProjArqsi.Domain.DockAggregate.Dock(dockName, location, null!, depth, maxDraft, allowedTypes);

            // Assert
            act.Should().Throw<BusinessRuleValidationException>()
                .WithMessage("Length is required.");
        }

        [Fact]
        public void Constructor_WithNullDepth_ShouldThrowBusinessRuleValidationException()
        {
            // Arrange
            var dockName = new DockName("Dock Epsilon");
            var location = new Location("Port of Leixões");
            var length = new DockLength(200.0);
            var maxDraft = new Draft(8.0);
            var allowedTypes = new AllowedVesselTypes(new List<Guid> { Guid.NewGuid() });

            // Act
            Action act = () => new ProjArqsi.Domain.DockAggregate.Dock(dockName, location, length, null!, maxDraft, allowedTypes);

            // Assert
            act.Should().Throw<BusinessRuleValidationException>()
                .WithMessage("Depth is required.");
        }

        [Fact]
        public void Constructor_WithNullMaxDraft_ShouldThrowBusinessRuleValidationException()
        {
            // Arrange
            var dockName = new DockName("Dock Zeta");
            var location = new Location("Port of Leixões");
            var length = new DockLength(200.0);
            var depth = new Depth(10.0);
            var allowedTypes = new AllowedVesselTypes(new List<Guid> { Guid.NewGuid() });

            // Act
            Action act = () => new ProjArqsi.Domain.DockAggregate.Dock(dockName, location, length, depth, null!, allowedTypes);

            // Assert
            act.Should().Throw<BusinessRuleValidationException>()
                .WithMessage("Max draft is required.");
        }

        [Fact]
        public void Constructor_WithNullAllowedVesselTypes_ShouldThrowBusinessRuleValidationException()
        {
            // Arrange
            var dockName = new DockName("Dock Eta");
            var location = new Location("Port of Leixões");
            var length = new DockLength(200.0);
            var depth = new Depth(10.0);
            var maxDraft = new Draft(8.0);

            // Act
            Action act = () => new ProjArqsi.Domain.DockAggregate.Dock(dockName, location, length, depth, maxDraft, null!);

            // Assert
            act.Should().Throw<BusinessRuleValidationException>()
                .WithMessage("Allowed vessel types are required.");
        }

        #endregion

        #region UpdateDetails Tests

        [Fact]
        public void UpdateDetails_WithValidParameters_ShouldUpdateAllProperties()
        {
            // Arrange
            var dock = CreateValidDock();
            var newDockName = new DockName("Updated Dock Name");
            var newLocation = new Location("New Port Location");
            var newLength = new DockLength(300.0);
            var newDepth = new Depth(15.0);
            var newMaxDraft = new Draft(12.0);
            var newAllowedTypes = new AllowedVesselTypes(new List<Guid> { Guid.NewGuid(), Guid.NewGuid(), Guid.NewGuid() });

            // Act
            dock.UpdateDetails(newDockName, newLocation, newLength, newDepth, newMaxDraft, newAllowedTypes);

            // Assert
            dock.DockName.Should().Be(newDockName);
            dock.Location.Should().Be(newLocation);
            dock.Length.Should().Be(newLength);
            dock.Depth.Should().Be(newDepth);
            dock.MaxDraft.Should().Be(newMaxDraft);
            dock.AllowedVesselTypes.Should().Be(newAllowedTypes);
        }

        [Fact]
        public void UpdateDetails_ShouldPreserveId()
        {
            // Arrange
            var dock = CreateValidDock();
            var originalId = dock.Id;
            var newDockName = new DockName("Updated Dock");
            var newLocation = new Location("Updated Location");
            var newLength = new DockLength(280.0);
            var newDepth = new Depth(13.0);
            var newMaxDraft = new Draft(11.0);
            var newAllowedTypes = new AllowedVesselTypes(new List<Guid> { Guid.NewGuid() });

            // Act
            dock.UpdateDetails(newDockName, newLocation, newLength, newDepth, newMaxDraft, newAllowedTypes);

            // Assert
            dock.Id.Should().Be(originalId);
        }

        [Fact]
        public void UpdateDetails_WithNullDockName_ShouldThrowBusinessRuleValidationException()
        {
            // Arrange
            var dock = CreateValidDock();
            var newLocation = new Location("Updated Location");
            var newLength = new DockLength(250.0);
            var newDepth = new Depth(12.0);
            var newMaxDraft = new Draft(10.0);
            var newAllowedTypes = new AllowedVesselTypes(new List<Guid> { Guid.NewGuid() });

            // Act
            Action act = () => dock.UpdateDetails(null!, newLocation, newLength, newDepth, newMaxDraft, newAllowedTypes);

            // Assert
            act.Should().Throw<BusinessRuleValidationException>()
                .WithMessage("Dock name is required.");
        }

        [Fact]
        public void UpdateDetails_WithNullLocation_ShouldThrowBusinessRuleValidationException()
        {
            // Arrange
            var dock = CreateValidDock();
            var newDockName = new DockName("Updated Dock");
            var newLength = new DockLength(250.0);
            var newDepth = new Depth(12.0);
            var newMaxDraft = new Draft(10.0);
            var newAllowedTypes = new AllowedVesselTypes(new List<Guid> { Guid.NewGuid() });

            // Act
            Action act = () => dock.UpdateDetails(newDockName, null!, newLength, newDepth, newMaxDraft, newAllowedTypes);

            // Assert
            act.Should().Throw<BusinessRuleValidationException>()
                .WithMessage("Location is required.");
        }

        [Fact]
        public void UpdateDetails_WithNullLength_ShouldThrowBusinessRuleValidationException()
        {
            // Arrange
            var dock = CreateValidDock();
            var newDockName = new DockName("Updated Dock");
            var newLocation = new Location("Updated Location");
            var newDepth = new Depth(12.0);
            var newMaxDraft = new Draft(10.0);
            var newAllowedTypes = new AllowedVesselTypes(new List<Guid> { Guid.NewGuid() });

            // Act
            Action act = () => dock.UpdateDetails(newDockName, newLocation, null!, newDepth, newMaxDraft, newAllowedTypes);

            // Assert
            act.Should().Throw<BusinessRuleValidationException>()
                .WithMessage("Length is required.");
        }

        [Fact]
        public void UpdateDetails_WithNullDepth_ShouldThrowBusinessRuleValidationException()
        {
            // Arrange
            var dock = CreateValidDock();
            var newDockName = new DockName("Updated Dock");
            var newLocation = new Location("Updated Location");
            var newLength = new DockLength(250.0);
            var newMaxDraft = new Draft(10.0);
            var newAllowedTypes = new AllowedVesselTypes(new List<Guid> { Guid.NewGuid() });

            // Act
            Action act = () => dock.UpdateDetails(newDockName, newLocation, newLength, null!, newMaxDraft, newAllowedTypes);

            // Assert
            act.Should().Throw<BusinessRuleValidationException>()
                .WithMessage("Depth is required.");
        }

        [Fact]
        public void UpdateDetails_WithNullMaxDraft_ShouldThrowBusinessRuleValidationException()
        {
            // Arrange
            var dock = CreateValidDock();
            var newDockName = new DockName("Updated Dock");
            var newLocation = new Location("Updated Location");
            var newLength = new DockLength(250.0);
            var newDepth = new Depth(12.0);
            var newAllowedTypes = new AllowedVesselTypes(new List<Guid> { Guid.NewGuid() });

            // Act
            Action act = () => dock.UpdateDetails(newDockName, newLocation, newLength, newDepth, null!, newAllowedTypes);

            // Assert
            act.Should().Throw<BusinessRuleValidationException>()
                .WithMessage("Max draft is required.");
        }

        [Fact]
        public void UpdateDetails_WithNullAllowedVesselTypes_ShouldThrowBusinessRuleValidationException()
        {
            // Arrange
            var dock = CreateValidDock();
            var newDockName = new DockName("Updated Dock");
            var newLocation = new Location("Updated Location");
            var newLength = new DockLength(250.0);
            var newDepth = new Depth(12.0);
            var newMaxDraft = new Draft(10.0);

            // Act
            Action act = () => dock.UpdateDetails(newDockName, newLocation, newLength, newDepth, newMaxDraft, null!);

            // Assert
            act.Should().Throw<BusinessRuleValidationException>()
                .WithMessage("Allowed vessel types are required.");
        }

        [Fact]
        public void UpdateDetails_CalledMultipleTimes_ShouldAlwaysUpdateToLatestValues()
        {
            // Arrange
            var dock = CreateValidDock();

            var update1Name = new DockName("First Update");
            var update1Location = new Location("First Location");
            var update1Length = new DockLength(220.0);
            var update1Depth = new Depth(11.0);
            var update1Draft = new Draft(9.0);
            var update1Types = new AllowedVesselTypes(new List<Guid> { Guid.NewGuid() });

            var update2Name = new DockName("Second Update");
            var update2Location = new Location("Second Location");
            var update2Length = new DockLength(240.0);
            var update2Depth = new Depth(12.5);
            var update2Draft = new Draft(10.5);
            var update2Types = new AllowedVesselTypes(new List<Guid> { Guid.NewGuid(), Guid.NewGuid() });

            // Act
            dock.UpdateDetails(update1Name, update1Location, update1Length, update1Depth, update1Draft, update1Types);
            dock.UpdateDetails(update2Name, update2Location, update2Length, update2Depth, update2Draft, update2Types);

            // Assert
            dock.DockName.Should().Be(update2Name);
            dock.Location.Should().Be(update2Location);
            dock.Length.Should().Be(update2Length);
            dock.Depth.Should().Be(update2Depth);
            dock.MaxDraft.Should().Be(update2Draft);
            dock.AllowedVesselTypes.Should().Be(update2Types);
        }

        #endregion

        #region Real-World Scenarios

        [Fact]
        public void Scenario_SmallContainerDock_ShouldBeCreatedSuccessfully()
        {
            // Arrange - Small dock for feeder vessels
            var dockName = new DockName("Feeder Terminal 1");
            var location = new Location("Port of Leixões - South Basin");
            var length = new DockLength(150.0); // 150 meters
            var depth = new Depth(8.0); // 8 meters depth
            var maxDraft = new Draft(7.0); // 7 meters max draft
            var feederVesselTypeId = Guid.NewGuid();
            var allowedTypes = new AllowedVesselTypes(new List<Guid> { feederVesselTypeId });

            // Act
            var dock = new ProjArqsi.Domain.DockAggregate.Dock(dockName, location, length, depth, maxDraft, allowedTypes);

            // Assert
            dock.Should().NotBeNull();
            dock.DockName.Value.Should().Be("Feeder Terminal 1");
            dock.Length.Value.Should().Be(150.0);
            dock.Depth.Value.Should().Be(8.0);
            dock.MaxDraft.Value.Should().Be(7.0);
            dock.AllowedVesselTypes.VesselTypeIds.Should().HaveCount(1);
            dock.Location.Description.Should().Contain("South Basin");
        }

        [Fact]
        public void Scenario_LargeContainerDock_ForUltraLargeContainerVessels()
        {
            // Arrange - Large dock for ULCV (Ultra Large Container Vessels)
            var dockName = new DockName("Deep Water Terminal");
            var location = new Location("Port of Leixões - North Basin - Berth 1");
            var length = new DockLength(400.0); // 400 meters - can accommodate 20,000+ TEU vessels
            var depth = new Depth(18.0); // 18 meters depth
            var maxDraft = new Draft(16.0); // 16 meters max draft
            var ulcvTypeId = Guid.NewGuid();
            var neopanamaxTypeId = Guid.NewGuid();
            var allowedTypes = new AllowedVesselTypes(new List<Guid> { ulcvTypeId, neopanamaxTypeId });

            // Act
            var dock = new ProjArqsi.Domain.DockAggregate.Dock(dockName, location, length, depth, maxDraft, allowedTypes);

            // Assert
            dock.DockName.Value.Should().Be("Deep Water Terminal");
            dock.Length.Value.Should().Be(400.0);
            dock.Depth.Value.Should().Be(18.0);
            dock.MaxDraft.Value.Should().Be(16.0);
            dock.AllowedVesselTypes.VesselTypeIds.Should().HaveCount(2);
            dock.Location.Description.Should().Contain("North Basin");
        }

        [Fact]
        public void Scenario_DockUpgrade_IncreaseDepthAndAllowedVesselTypes()
        {
            // Arrange - Upgrade existing dock to accommodate larger vessels
            var dock = new ProjArqsi.Domain.DockAggregate.Dock(
                new DockName("Terminal 3"),
                new Location("Port of Leixões - Central Basin"),
                new DockLength(250.0),
                new Depth(10.0), // Original: 10 meters
                new Draft(9.0), // Original: 9 meters
                new AllowedVesselTypes(new List<Guid> { Guid.NewGuid() }) // Only 1 type originally
            );

            // Act - Dredging operation increases depth, allowing larger vessels
            dock.UpdateDetails(
                new DockName("Terminal 3 - Upgraded"),
                new Location("Port of Leixões - Central Basin"),
                new DockLength(250.0), // Same length
                new Depth(14.0), // Increased depth after dredging
                new Draft(12.5), // Increased max draft
                new AllowedVesselTypes(new List<Guid> { 
                    Guid.NewGuid(), 
                    Guid.NewGuid(), 
                    Guid.NewGuid() 
                }) // Now accepts 3 vessel types
            );

            // Assert
            dock.DockName.Value.Should().Be("Terminal 3 - Upgraded");
            dock.Depth.Value.Should().Be(14.0);
            dock.MaxDraft.Value.Should().Be(12.5);
            dock.AllowedVesselTypes.VesselTypeIds.Should().HaveCount(3);
        }

        [Fact]
        public void Scenario_MultiPurposeTerminal_AcceptsMultipleVesselTypes()
        {
            // Arrange - Multi-purpose terminal
            var vesselType1 = Guid.NewGuid();
            var vesselType2 = Guid.NewGuid();
            var vesselType3 = Guid.NewGuid();
            var vesselType4 = Guid.NewGuid();

            // Act
            var dock = new ProjArqsi.Domain.DockAggregate.Dock(
                new DockName("Multi-Purpose Terminal"),
                new Location("Port of Leixões - East Basin"),
                new DockLength(300.0),
                new Depth(12.0),
                new Draft(11.0),
                new AllowedVesselTypes(new List<Guid> { vesselType1, vesselType2, vesselType3, vesselType4 })
            );

            // Assert
            dock.AllowedVesselTypes.VesselTypeIds.Should().HaveCount(4);
            dock.AllowedVesselTypes.VesselTypeIds.Should().Contain(vesselType1);
            dock.AllowedVesselTypes.VesselTypeIds.Should().Contain(vesselType2);
            dock.AllowedVesselTypes.VesselTypeIds.Should().Contain(vesselType3);
            dock.AllowedVesselTypes.VesselTypeIds.Should().Contain(vesselType4);
        }

        [Fact]
        public void Scenario_DockRelocation_UpdateLocationOnly()
        {
            // Arrange - Dock information needs location update (administrative change)
            var dock = new ProjArqsi.Domain.DockAggregate.Dock(
                new DockName("Terminal 5"),
                new Location("Berth A1"),
                new DockLength(200.0),
                new Depth(10.0),
                new Draft(9.0),
                new AllowedVesselTypes(new List<Guid> { Guid.NewGuid() })
            );

            // Act - Update location nomenclature
            dock.UpdateDetails(
                dock.DockName, // Keep same name
                new Location("Port of Leixões - North Basin - Berth A1 (Renamed)"), // More detailed location
                dock.Length, // Keep same physical dimensions
                dock.Depth,
                dock.MaxDraft,
                dock.AllowedVesselTypes
            );

            // Assert
            dock.Location.Description.Should().Be("Port of Leixões - North Basin - Berth A1 (Renamed)");
            dock.DockName.Value.Should().Be("Terminal 5");
        }

        [Fact]
        public void Scenario_RestrictVesselTypes_SecurityReasons()
        {
            // Arrange - Dock initially accepts multiple vessel types
            var typeA = Guid.NewGuid();
            var typeB = Guid.NewGuid();
            var typeC = Guid.NewGuid();
            var dock = new ProjArqsi.Domain.DockAggregate.Dock(
                new DockName("Security Terminal"),
                new Location("Port of Leixões - Restricted Zone"),
                new DockLength(220.0),
                new Depth(11.0),
                new Draft(10.0),
                new AllowedVesselTypes(new List<Guid> { typeA, typeB, typeC })
            );

            // Act - Restrict to only one specific vessel type for security
            dock.UpdateDetails(
                dock.DockName,
                dock.Location,
                dock.Length,
                dock.Depth,
                dock.MaxDraft,
                new AllowedVesselTypes(new List<Guid> { typeA }) // Only one type allowed now
            );

            // Assert
            dock.AllowedVesselTypes.VesselTypeIds.Should().HaveCount(1);
            dock.AllowedVesselTypes.VesselTypeIds.Should().Contain(typeA);
            dock.AllowedVesselTypes.VesselTypeIds.Should().NotContain(typeB);
            dock.AllowedVesselTypes.VesselTypeIds.Should().NotContain(typeC);
        }

        [Fact]
        public void Scenario_DockMaintenance_TemporaryRestrictions()
        {
            // Arrange - Dock undergoing maintenance, reduce allowed draft
            var dock = new ProjArqsi.Domain.DockAggregate.Dock(
                new DockName("Maintenance Terminal"),
                new Location("Port of Leixões - West Basin"),
                new DockLength(280.0),
                new Depth(15.0),
                new Draft(13.0), // Normal max draft
                new AllowedVesselTypes(new List<Guid> { Guid.NewGuid(), Guid.NewGuid() })
            );

            // Act - Reduce max draft during maintenance
            dock.UpdateDetails(
                new DockName("Maintenance Terminal - Under Maintenance"),
                dock.Location,
                dock.Length,
                dock.Depth,
                new Draft(8.0), // Reduced draft during maintenance
                new AllowedVesselTypes(new List<Guid> { Guid.NewGuid() }) // Only small vessels
            );

            // Assert
            dock.DockName.Value.Should().Contain("Under Maintenance");
            dock.MaxDraft.Value.Should().Be(8.0);
            dock.AllowedVesselTypes.VesselTypeIds.Should().HaveCount(1);
        }

        [Fact]
        public void Scenario_RoRoDock_SpecializedForRollOnRollOffVessels()
        {
            // Arrange - RoRo (Roll-on/Roll-off) specialized dock
            var roroVesselType = Guid.NewGuid();

            // Act
            var dock = new ProjArqsi.Domain.DockAggregate.Dock(
                new DockName("RoRo Terminal"),
                new Location("Port of Leixões - RoRo Basin"),
                new DockLength(180.0), // Shorter length suitable for RoRo vessels
                new Depth(9.0),
                new Draft(7.5),
                new AllowedVesselTypes(new List<Guid> { roroVesselType })
            );

            // Assert
            dock.DockName.Value.Should().Be("RoRo Terminal");
            dock.Length.Value.Should().Be(180.0);
            dock.AllowedVesselTypes.VesselTypeIds.Should().HaveCount(1);
        }

        #endregion

        #region Identity and Equality Tests

        [Fact]
        public void Dock_WithSameId_ShouldBeConsideredEqual()
        {
            // Arrange
            var dockId = new DockId(Guid.NewGuid());
            var dock1 = CreateValidDock();
            var dock2 = CreateValidDock();

            // Use reflection to set same ID (for testing purposes)
            var idProperty = typeof(ProjArqsi.Domain.DockAggregate.Dock).GetProperty("Id");
            idProperty!.SetValue(dock1, dockId);
            idProperty!.SetValue(dock2, dockId);

            // Act & Assert
            dock1.Id.Should().Be(dock2.Id);
        }

        [Fact]
        public void Dock_WithDifferentIds_ShouldNotBeEqual()
        {
            // Arrange & Act
            var dock1 = CreateValidDock();
            var dock2 = CreateValidDock();

            // Assert
            dock1.Id.Should().NotBe(dock2.Id);
        }

        [Fact]
        public void Dock_IdShouldBeImmutable()
        {
            // Arrange
            var dock = CreateValidDock();
            var originalId = dock.Id;

            // Act - Update all other properties
            dock.UpdateDetails(
                new DockName("New Name"),
                new Location("New Location"),
                new DockLength(999.0),
                new Depth(99.0),
                new Draft(88.0),
                new AllowedVesselTypes(new List<Guid> { Guid.NewGuid() })
            );

            // Assert
            dock.Id.Should().Be(originalId);
        }

        [Fact]
        public void Dock_AfterMultipleUpdates_IdShouldRemainUnchanged()
        {
            // Arrange
            var dock = CreateValidDock();
            var originalId = dock.Id;

            // Act - Multiple updates
            for (int i = 0; i < 5; i++)
            {
                dock.UpdateDetails(
                    new DockName($"Update {i}"),
                    new Location($"Location {i}"),
                    new DockLength(200.0 + i),
                    new Depth(10.0 + i),
                    new Draft(9.0 + i),
                    new AllowedVesselTypes(new List<Guid> { Guid.NewGuid() })
                );
            }

            // Assert
            dock.Id.Should().Be(originalId);
        }

        #endregion

        #region Edge Cases

        [Fact]
        public void Dock_WithVeryLongLocationDescription_ShouldBeCreated()
        {
            // Arrange
            var longLocation = new string('A', 500); // 500 character location

            // Act
            var dock = new ProjArqsi.Domain.DockAggregate.Dock(
                new DockName("Edge Case Dock"),
                new Location(longLocation),
                new DockLength(200.0),
                new Depth(10.0),
                new Draft(9.0),
                new AllowedVesselTypes(new List<Guid> { Guid.NewGuid() })
            );

            // Assert
            dock.Location.Description.Length.Should().Be(500);
        }

        [Fact]
        public void Dock_WithMinimumViableValues_ShouldBeCreated()
        {
            // Arrange & Act
            var dock = new ProjArqsi.Domain.DockAggregate.Dock(
                new DockName("A"), // Minimum name
                new Location("B"), // Minimum location
                new DockLength(0.1), // Minimum length
                new Depth(0.1), // Minimum depth
                new Draft(0.1), // Minimum draft
                new AllowedVesselTypes(new List<Guid> { Guid.NewGuid() })
            );

            // Assert
            dock.Should().NotBeNull();
            dock.DockName.Value.Should().Be("A");
            dock.Location.Description.Should().Be("B");
        }

        #endregion

        #region Helper Methods

        private static ProjArqsi.Domain.DockAggregate.Dock CreateValidDock()
        {
            return new ProjArqsi.Domain.DockAggregate.Dock(
                new DockName("Test Dock"),
                new Location("Test Location"),
                new DockLength(200.0),
                new Depth(10.0),
                new Draft(9.0),
                new AllowedVesselTypes(new List<Guid> { Guid.NewGuid() })
            );
        }

        #endregion
    }
}
