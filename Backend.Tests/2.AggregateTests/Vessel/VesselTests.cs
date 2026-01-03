using FluentAssertions;
using ProjArqsi.Domain.Shared;
using ProjArqsi.Domain.VesselAggregate;
using ProjArqsi.Domain.VesselTypeAggregate;
using Xunit;

namespace Backend.Tests.AggregateTests.Vessel
{
    public class VesselTests
    {
        #region Constructor Tests

        [Fact]
        public void Constructor_WithValidParameters_ShouldCreateVessel()
        {
            // Arrange
            var imo = new IMOnumber("9074729");  // Valid check digit
            var vesselTypeId = new VesselTypeId(Guid.NewGuid());
            var vesselName = new VesselName("MSC OSCAR");
            var capacity = new Capacity(19224);
            var rows = new Rows(24);
            var bays = new Bays(23);
            var tiers = new Tiers(10);
            var length = new Length(395.4);

            // Act
            var vessel = new ProjArqsi.Domain.VesselAggregate.Vessel(
                imo, vesselTypeId, vesselName, capacity, rows, bays, tiers, length);

            // Assert
            vessel.Should().NotBeNull();
            vessel.Id.Should().NotBeNull();
            vessel.IMO.Should().Be(imo);
            vessel.VesselTypeId.Should().Be(vesselTypeId);
            vessel.VesselName.Should().Be(vesselName);
            vessel.Capacity.Should().Be(capacity);
            vessel.Rows.Should().Be(rows);
            vessel.Bays.Should().Be(bays);
            vessel.Tiers.Should().Be(tiers);
            vessel.Length.Should().Be(length);
        }

        [Fact]
        public void Constructor_ShouldGenerateUniqueIds()
        {
            // Arrange
            var imo = new IMOnumber("9305623");  // Valid check digit
            var vesselTypeId = new VesselTypeId(Guid.NewGuid());
            var vesselName = new VesselName("EMMA MAERSK");
            var capacity = new Capacity(15000);
            var rows = new Rows(22);
            var bays = new Bays(20);
            var tiers = new Tiers(9);
            var length = new Length(397.7);

            // Act
            var vessel1 = new ProjArqsi.Domain.VesselAggregate.Vessel(
                imo, vesselTypeId, vesselName, capacity, rows, bays, tiers, length);
            var vessel2 = new ProjArqsi.Domain.VesselAggregate.Vessel(
                imo, vesselTypeId, vesselName, capacity, rows, bays, tiers, length);

            // Assert
            vessel1.Id.Should().NotBe(vessel2.Id);
        }

        [Fact]
        public void Constructor_WithNullIMO_ShouldThrowException()
        {
            // Arrange
            var vesselTypeId = new VesselTypeId(Guid.NewGuid());
            var vesselName = new VesselName("TEST VESSEL");
            var capacity = new Capacity(10000);
            var rows = new Rows(20);
            var bays = new Bays(18);
            var tiers = new Tiers(8);
            var length = new Length(300);

            // Act & Assert
            var act = () => new ProjArqsi.Domain.VesselAggregate.Vessel(
                null!, vesselTypeId, vesselName, capacity, rows, bays, tiers, length);

            act.Should().Throw<ProjArqsi.Domain.Shared.BusinessRuleValidationException>()
                .WithMessage("*IMO number*");
        }

        [Fact]
        public void Constructor_WithNullVesselTypeId_ShouldThrowException()
        {
            // Arrange
            var imo = new IMOnumber("9366213");  // Valid check digit
            var vesselName = new VesselName("TEST VESSEL");
            var capacity = new Capacity(10000);
            var rows = new Rows(20);
            var bays = new Bays(18);
            var tiers = new Tiers(8);
            var length = new Length(300);

            // Act & Assert
            var act = () => new ProjArqsi.Domain.VesselAggregate.Vessel(
                imo, null!, vesselName, capacity, rows, bays, tiers, length);

            act.Should().Throw<ProjArqsi.Domain.Shared.BusinessRuleValidationException>()
                .WithMessage("*vesselTypeId*");
        }

        [Fact]
        public void Constructor_WithNullVesselName_ShouldThrowException()
        {
            // Arrange
            var imo = new IMOnumber("9432892");  // Valid check digit
            var vesselTypeId = new VesselTypeId(Guid.NewGuid());
            var capacity = new Capacity(10000);
            var rows = new Rows(20);
            var bays = new Bays(18);
            var tiers = new Tiers(8);
            var length = new Length(300);

            // Act & Assert
            var act = () => new ProjArqsi.Domain.VesselAggregate.Vessel(
                imo, vesselTypeId, null!, capacity, rows, bays, tiers, length);

            act.Should().Throw<ProjArqsi.Domain.Shared.BusinessRuleValidationException>()
                .WithMessage("*Vessel name*");
        }

        [Fact]
        public void Constructor_WithNullCapacity_ShouldThrowException()
        {
            // Arrange
            var imo = new IMOnumber("9542738");  // Valid check digit
            var vesselTypeId = new VesselTypeId(Guid.NewGuid());
            var vesselName = new VesselName("TEST VESSEL");
            var rows = new Rows(20);
            var bays = new Bays(18);
            var tiers = new Tiers(8);
            var length = new Length(300);

            // Act & Assert
            var act = () => new ProjArqsi.Domain.VesselAggregate.Vessel(
                imo, vesselTypeId, vesselName, null!, rows, bays, tiers, length);

            act.Should().Throw<ProjArqsi.Domain.Shared.BusinessRuleValidationException>()
                .WithMessage("*capacity*");
        }

        [Fact]
        public void Constructor_WithNullRows_ShouldThrowException()
        {
            // Arrange
            var imo = new IMOnumber("9612507");  // Valid check digit
            var vesselTypeId = new VesselTypeId(Guid.NewGuid());
            var vesselName = new VesselName("TEST VESSEL");
            var capacity = new Capacity(10000);
            var bays = new Bays(18);
            var tiers = new Tiers(8);
            var length = new Length(300);

            // Act & Assert
            var act = () => new ProjArqsi.Domain.VesselAggregate.Vessel(
                imo, vesselTypeId, vesselName, capacity, null!, bays, tiers, length);

            act.Should().Throw<ProjArqsi.Domain.Shared.BusinessRuleValidationException>()
                .WithMessage("*rows*");
        }

        [Fact]
        public void Constructor_WithNullBays_ShouldThrowException()
        {
            // Arrange
            var imo = new IMOnumber("9802140");  // Valid check digit
            var vesselTypeId = new VesselTypeId(Guid.NewGuid());
            var vesselName = new VesselName("TEST VESSEL");
            var capacity = new Capacity(10000);
            var rows = new Rows(20);
            var tiers = new Tiers(8);
            var length = new Length(300);

            // Act & Assert
            var act = () => new ProjArqsi.Domain.VesselAggregate.Vessel(
                imo, vesselTypeId, vesselName, capacity, rows, null!, tiers, length);

            act.Should().Throw<ProjArqsi.Domain.Shared.BusinessRuleValidationException>()
                .WithMessage("*bays*");
        }

        [Fact]
        public void Constructor_WithNullTiers_ShouldThrowException()
        {
            // Arrange
            var imo = new IMOnumber("9234329");  // Valid check digit
            var vesselTypeId = new VesselTypeId(Guid.NewGuid());
            var vesselName = new VesselName("TEST VESSEL");
            var capacity = new Capacity(10000);
            var rows = new Rows(20);
            var bays = new Bays(18);
            var length = new Length(300);

            // Act & Assert
            var act = () => new ProjArqsi.Domain.VesselAggregate.Vessel(
                imo, vesselTypeId, vesselName, capacity, rows, bays, null!, length);

            act.Should().Throw<ProjArqsi.Domain.Shared.BusinessRuleValidationException>()
                .WithMessage("*tiers*");
        }

        [Fact]
        public void Constructor_WithNullLength_ShouldThrowException()
        {
            // Arrange
            var imo = new IMOnumber("9074729");  // Valid check digit
            var vesselTypeId = new VesselTypeId(Guid.NewGuid());
            var vesselName = new VesselName("TEST VESSEL");
            var capacity = new Capacity(10000);
            var rows = new Rows(20);
            var bays = new Bays(18);
            var tiers = new Tiers(8);

            // Act & Assert
            var act = () => new ProjArqsi.Domain.VesselAggregate.Vessel(
                imo, vesselTypeId, vesselName, capacity, rows, bays, tiers, null!);

            act.Should().Throw<ProjArqsi.Domain.Shared.BusinessRuleValidationException>()
                .WithMessage("*length*");
        }

        #endregion

        #region UpdateDetails Tests

        [Fact]
        public void UpdateDetails_WithValidParameters_ShouldUpdateProperties()
        {
            // Arrange
            var vessel = CreateTestVessel();
            var newVesselName = new VesselName("UPDATED VESSEL");
            var newCapacity = new Capacity(20000);
            var newRows = new Rows(25);
            var newBays = new Bays(22);
            var newTiers = new Tiers(11);
            var newLength = new Length(400);

            // Act
            vessel.UpdateDetails(newVesselName, newCapacity, newRows, newBays, newTiers, newLength);

            // Assert
            vessel.VesselName.Should().Be(newVesselName);
            vessel.Capacity.Should().Be(newCapacity);
            vessel.Rows.Should().Be(newRows);
            vessel.Bays.Should().Be(newBays);
            vessel.Tiers.Should().Be(newTiers);
            vessel.Length.Should().Be(newLength);
        }

        [Fact]
        public void UpdateDetails_ShouldPreserveImmutableProperties()
        {
            // Arrange
            var vessel = CreateTestVessel();
            var originalId = vessel.Id;
            var originalIMO = vessel.IMO;
            var originalVesselTypeId = vessel.VesselTypeId;

            // Act
            vessel.UpdateDetails(
                new VesselName("NEW NAME"),
                new Capacity(18000),
                new Rows(23),
                new Bays(21),
                new Tiers(10),
                new Length(380));

            // Assert
            vessel.Id.Should().Be(originalId);
            vessel.IMO.Should().Be(originalIMO);
            vessel.VesselTypeId.Should().Be(originalVesselTypeId);
        }

        [Fact]
        public void UpdateDetails_WithNullVesselName_ShouldThrowException()
        {
            // Arrange
            var vessel = CreateTestVessel();

            // Act & Assert
            var act = () => vessel.UpdateDetails(
                null!,
                new Capacity(18000),
                new Rows(23),
                new Bays(21),
                new Tiers(10),
                new Length(380));

            act.Should().Throw<ProjArqsi.Domain.Shared.BusinessRuleValidationException>()
                .WithMessage("*Vessel name*");
        }

        [Fact]
        public void UpdateDetails_WithNullCapacity_ShouldThrowException()
        {
            // Arrange
            var vessel = CreateTestVessel();

            // Act & Assert
            var act = () => vessel.UpdateDetails(
                new VesselName("NEW NAME"),
                null!,
                new Rows(23),
                new Bays(21),
                new Tiers(10),
                new Length(380));

            act.Should().Throw<ProjArqsi.Domain.Shared.BusinessRuleValidationException>()
                .WithMessage("*capacity*");
        }

        [Fact]
        public void UpdateDetails_WithNullRows_ShouldThrowException()
        {
            // Arrange
            var vessel = CreateTestVessel();

            // Act & Assert
            var act = () => vessel.UpdateDetails(
                new VesselName("NEW NAME"),
                new Capacity(18000),
                null!,
                new Bays(21),
                new Tiers(10),
                new Length(380));

            act.Should().Throw<ProjArqsi.Domain.Shared.BusinessRuleValidationException>()
                .WithMessage("*rows*");
        }

        [Fact]
        public void UpdateDetails_WithNullBays_ShouldThrowException()
        {
            // Arrange
            var vessel = CreateTestVessel();

            // Act & Assert
            var act = () => vessel.UpdateDetails(
                new VesselName("NEW NAME"),
                new Capacity(18000),
                new Rows(23),
                null!,
                new Tiers(10),
                new Length(380));

            act.Should().Throw<ProjArqsi.Domain.Shared.BusinessRuleValidationException>()
                .WithMessage("*bays*");
        }

        [Fact]
        public void UpdateDetails_WithNullTiers_ShouldThrowException()
        {
            // Arrange
            var vessel = CreateTestVessel();

            // Act & Assert
            var act = () => vessel.UpdateDetails(
                new VesselName("NEW NAME"),
                new Capacity(18000),
                new Rows(23),
                new Bays(21),
                null!,
                new Length(380));

            act.Should().Throw<ProjArqsi.Domain.Shared.BusinessRuleValidationException>()
                .WithMessage("*tiers*");
        }

        [Fact]
        public void UpdateDetails_WithNullLength_ShouldThrowException()
        {
            // Arrange
            var vessel = CreateTestVessel();

            // Act & Assert
            var act = () => vessel.UpdateDetails(
                new VesselName("NEW NAME"),
                new Capacity(18000),
                new Rows(23),
                new Bays(21),
                new Tiers(10),
                null!);

            act.Should().Throw<ProjArqsi.Domain.Shared.BusinessRuleValidationException>()
                .WithMessage("*length*");
        }

        [Fact]
        public void UpdateDetails_CalledMultipleTimes_ShouldApplyLatestValues()
        {
            // Arrange
            var vessel = CreateTestVessel();

            // Act
            vessel.UpdateDetails(
                new VesselName("UPDATE 1"),
                new Capacity(16000),
                new Rows(21),
                new Bays(19),
                new Tiers(9),
                new Length(350));

            vessel.UpdateDetails(
                new VesselName("UPDATE 2"),
                new Capacity(18000),
                new Rows(23),
                new Bays(21),
                new Tiers(10),
                new Length(370));

            // Assert
            vessel.VesselName.Name.Should().Be("UPDATE 2");
            vessel.Capacity.Value.Should().Be(18000);
            vessel.Rows.Value.Should().Be(23);
        }

        #endregion

        #region Real-World Scenarios

        [Fact]
        public void Scenario_UltraLargeContainerVessel_ULCV()
        {
            // Arrange - ULCV like MSC IRINA with >24,000 TEU
            var imo = new IMOnumber("9305623");  // Valid check digit
            var vesselTypeId = new VesselTypeId(Guid.NewGuid());
            var vesselName = new VesselName("MSC IRINA");
            var capacity = new Capacity(24346);
            var rows = new Rows(25);
            var bays = new Bays(24);
            var tiers = new Tiers(11);
            var length = new Length(399.9);

            // Act
            var vessel = new ProjArqsi.Domain.VesselAggregate.Vessel(
                imo, vesselTypeId, vesselName, capacity, rows, bays, tiers, length);

            // Assert
            vessel.Capacity.Value.Should().BeGreaterThan(20000);
            vessel.Length.Value.Should().BeGreaterThan(390);
            vessel.VesselName.Name.Should().Contain("MSC");
        }

        [Fact]
        public void Scenario_FeederShip_SmallerCapacity()
        {
            // Arrange - Small feeder vessel
            var imo = new IMOnumber("9366213");  // Valid check digit
            var vesselTypeId = new VesselTypeId(Guid.NewGuid());
            var vesselName = new VesselName("NORDIC FEEDER");
            var capacity = new Capacity(850);
            var rows = new Rows(13);
            var bays = new Bays(10);
            var tiers = new Tiers(5);
            var length = new Length(120);

            // Act
            var vessel = new ProjArqsi.Domain.VesselAggregate.Vessel(
                imo, vesselTypeId, vesselName, capacity, rows, bays, tiers, length);

            // Assert
            vessel.Capacity.Value.Should().BeLessThan(1500);
            vessel.Length.Value.Should().BeLessThan(150);
        }

        [Fact]
        public void Scenario_VesselCapacityUpgrade_AfterModifications()
        {
            // Arrange - Vessel upgraded with more capacity
            var vessel = CreateTestVessel();
            var originalCapacity = vessel.Capacity.Value;

            // Act - Simulate retrofitting with increased capacity
            vessel.UpdateDetails(
                vessel.VesselName,
                new Capacity(originalCapacity + 2000),
                new Rows(vessel.Rows.Value + 2),
                vessel.Bays,
                vessel.Tiers,
                vessel.Length);

            // Assert
            vessel.Capacity.Value.Should().BeGreaterThan(originalCapacity);
            vessel.Rows.Value.Should().BeGreaterThan(22);
        }

        [Fact]
        public void Scenario_VesselRenaming_AfterOwnershipChange()
        {
            // Arrange
            var vessel = CreateTestVessel();
            var originalName = vessel.VesselName.Name;

            // Act - Ship sold and renamed
            vessel.UpdateDetails(
                new VesselName("NEW OWNER LINE"),
                vessel.Capacity,
                vessel.Rows,
                vessel.Bays,
                vessel.Tiers,
                vessel.Length);

            // Assert
            vessel.VesselName.Name.Should().NotBe(originalName);
            vessel.VesselName.Name.Should().Be("NEW OWNER LINE");
        }

        [Fact]
        public void Scenario_PanamaxVessel_WithSpecificDimensions()
        {
            // Arrange - Panamax vessel that fits Panama Canal locks
            var imo = new IMOnumber("9432892");  // Valid check digit
            var vesselTypeId = new VesselTypeId(Guid.NewGuid());
            var vesselName = new VesselName("PANAMAX EXPRESS");
            var capacity = new Capacity(5000);
            var rows = new Rows(17);
            var bays = new Bays(17);
            var tiers = new Tiers(7);
            var length = new Length(294);  // Under 295m limit

            // Act
            var vessel = new ProjArqsi.Domain.VesselAggregate.Vessel(
                imo, vesselTypeId, vesselName, capacity, rows, bays, tiers, length);

            // Assert
            vessel.Length.Value.Should().BeLessThan(295);
            vessel.Capacity.Value.Should().BeInRange(4000, 5500);
        }

        [Fact]
        public void Scenario_NewPanamaxVessel_LargerDimensions()
        {
            // Arrange - New Panamax (Post-Panamax) for expanded canal
            var imo = new IMOnumber("9542738");  // Valid check digit
            var vesselTypeId = new VesselTypeId(Guid.NewGuid());
            var vesselName = new VesselName("NEO PANAMAX");
            var capacity = new Capacity(13000);
            var rows = new Rows(20);
            var bays = new Bays(20);
            var tiers = new Tiers(9);
            var length = new Length(366);  // Larger than old Panamax

            // Act
            var vessel = new ProjArqsi.Domain.VesselAggregate.Vessel(
                imo, vesselTypeId, vesselName, capacity, rows, bays, tiers, length);

            // Assert
            vessel.Length.Value.Should().BeGreaterThan(295);
            vessel.Length.Value.Should().BeLessOrEqualTo(366);
            vessel.Capacity.Value.Should().BeInRange(10000, 14500);
        }

        [Fact]
        public void Scenario_VesselModernization_UpdateMultipleAttributes()
        {
            // Arrange
            var vessel = CreateTestVessel();

            // Act - Modernization increases capacity and adds tiers
            vessel.UpdateDetails(
                new VesselName("MODERNIZED VESSEL"),
                new Capacity(17000),
                new Rows(24),
                new Bays(21),
                new Tiers(10),
                new Length(370));

            // Assert
            vessel.Capacity.Value.Should().BeGreaterThan(15000);
            vessel.Tiers.Value.Should().Be(10);
            vessel.VesselName.Name.Should().Contain("MODERNIZED");
        }

        [Fact]
        public void Scenario_TripleEClassVessel_ShouldRepresentLargestCapacity()
        {
            // Arrange - Maersk Triple E class vessel
            var imo = new IMOnumber("9612507");  // Valid check digit
            var vesselTypeId = new VesselTypeId(Guid.NewGuid());
            var vesselName = new VesselName("MAERSK TRIPLE E");
            var capacity = new Capacity(18270);
            var rows = new Rows(23);
            var bays = new Bays(23);
            var tiers = new Tiers(10);
            var length = new Length(399);

            // Act
            var vessel = new ProjArqsi.Domain.VesselAggregate.Vessel(
                imo, vesselTypeId, vesselName, capacity, rows, bays, tiers, length);

            // Assert
            vessel.Capacity.Value.Should().BeGreaterThan(18000);
            vessel.Length.Value.Should().BeApproximately(399, 1);
            vessel.VesselName.Name.Should().Contain("MAERSK");
        }

        #endregion

        #region Identity and Equality Tests

        [Fact]
        public void Vessels_WithSameId_ShouldBeEqual()
        {
            // Arrange
            var vessel1 = CreateTestVessel();

            // Act - Compare vessel with itself
            var result = vessel1.Equals(vessel1);

            // Assert
            result.Should().BeTrue();
            vessel1.GetHashCode().Should().Be(vessel1.GetHashCode());
        }

        [Fact]
        public void Vessels_WithDifferentIds_ShouldNotBeEqual()
        {
            // Arrange
            var vessel1 = CreateTestVessel();
            var vessel2 = CreateTestVessel();

            // Act & Assert
            vessel1.Should().NotBe(vessel2);
        }

        [Fact]
        public void VesselId_ShouldBeImmutable()
        {
            // Arrange
            var vessel = CreateTestVessel();
            var originalId = vessel.Id;

            // Act - Update details
            vessel.UpdateDetails(
                new VesselName("NEW NAME"),
                new Capacity(18000),
                new Rows(23),
                new Bays(21),
                new Tiers(10),
                new Length(380));

            // Assert
            vessel.Id.Should().Be(originalId);
        }

        [Fact]
        public void VesselIMO_ShouldBeImmutable()
        {
            // Arrange
            var vessel = CreateTestVessel();
            var originalIMO = vessel.IMO;

            // Act - Update details
            vessel.UpdateDetails(
                new VesselName("NEW NAME"),
                new Capacity(18000),
                new Rows(23),
                new Bays(21),
                new Tiers(10),
                new Length(380));

            // Assert
            vessel.IMO.Should().Be(originalIMO);
        }

        #endregion

        #region Edge Cases

        [Fact]
        public void EdgeCase_MinimumViableVesselConfiguration()
        {
            // Arrange - Smallest realistic container vessel
            var imo = new IMOnumber("9802140");  // Valid check digit
            var vesselTypeId = new VesselTypeId(Guid.NewGuid());
            var vesselName = new VesselName("MINI");
            var capacity = new Capacity(100);
            var rows = new Rows(5);
            var bays = new Bays(5);
            var tiers = new Tiers(3);
            var length = new Length(50);

            // Act
            var vessel = new ProjArqsi.Domain.VesselAggregate.Vessel(
                imo, vesselTypeId, vesselName, capacity, rows, bays, tiers, length);

            // Assert
            vessel.Should().NotBeNull();
            vessel.Capacity.Value.Should().Be(100);
        }

        [Fact]
        public void EdgeCase_MaximumDimensions_ShouldHandleLargeValues()
        {
            // Arrange - Hypothetical future mega-vessel
            var imo = new IMOnumber("9234329");  // Valid check digit
            var vesselTypeId = new VesselTypeId(Guid.NewGuid());
            var vesselName = new VesselName("MEGAMAX");
            var capacity = new Capacity(30000);
            var rows = new Rows(30);
            var bays = new Bays(30);
            var tiers = new Tiers(15);
            var length = new Length(450);

            // Act
            var vessel = new ProjArqsi.Domain.VesselAggregate.Vessel(
                imo, vesselTypeId, vesselName, capacity, rows, bays, tiers, length);

            // Assert
            vessel.Should().NotBeNull();
            vessel.Capacity.Value.Should().Be(30000);
        }

        [Fact]
        public void EdgeCase_VesselWithVeryLongName()
        {
            // Arrange
            var vessel = CreateTestVessel();
            var longName = new VesselName("VERY LONG VESSEL NAME THAT EXCEEDS NORMAL EXPECTATIONS BUT IS STILL VALID");

            // Act
            vessel.UpdateDetails(
                longName,
                vessel.Capacity,
                vessel.Rows,
                vessel.Bays,
                vessel.Tiers,
                vessel.Length);

            // Assert
            vessel.VesselName.Should().Be(longName);
        }

        #endregion

        #region Multiple Update Tests

        [Fact]
        public void MultipleUpdates_ShouldApplyAllChangesSequentially()
        {
            // Arrange
            var vessel = CreateTestVessel();
            var originalName = vessel.VesselName.Name;

            // Act - Perform 10 sequential updates
            for (int i = 1; i <= 10; i++)
            {
                vessel.UpdateDetails(
                    new VesselName($"UPDATE {i}"),
                    new Capacity(15000 + (i * 500)),
                    new Rows(22 + i),
                    vessel.Bays,
                    vessel.Tiers,
                    new Length(365 + i));
            }

            // Assert
            vessel.VesselName.Name.Should().Be("UPDATE 10");
            vessel.Capacity.Value.Should().Be(20000);
            vessel.Rows.Value.Should().Be(32);
        }

        #endregion

        #region Helper Methods

        private ProjArqsi.Domain.VesselAggregate.Vessel CreateTestVessel()
        {
            return new ProjArqsi.Domain.VesselAggregate.Vessel(
                new IMOnumber("9074729"),  // Valid check digit
                new VesselTypeId(Guid.NewGuid()),
                new VesselName("TEST VESSEL"),
                new Capacity(15000),
                new Rows(22),
                new Bays(20),
                new Tiers(9),
                new Length(365)
            );
        }

        #endregion
    }
}
