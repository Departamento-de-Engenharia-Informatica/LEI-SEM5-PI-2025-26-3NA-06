namespace Backend.Tests.AggregateTests.Container
{
    public class ContainerTests
    {
        #region Constructor Tests

        [Fact]
        public void Constructor_WithValidParameters_ShouldCreateContainer()
        {
            // Arrange
            var isoCode = new IsoCode("CSQU3054383");
            var cargoType = new CargoType("Refrigerated");
            var description = new ContainerDescription("Fresh produce from Brazil");
            var isHazardous = false;

            // Act
            var container = new ProjArqsi.Domain.ContainerAggregate.Container(
                isoCode,
                isHazardous,
                cargoType,
                description);

            // Assert
            container.Should().NotBeNull();
            container.Id.Should().NotBeNull();
            container.IsoCode.Should().Be(isoCode);
            container.IsHazardous.Should().Be(isHazardous);
            container.CargoType.Should().Be(cargoType);
            container.Description.Should().Be(description);
        }

        [Fact]
        public void Constructor_ShouldGenerateUniqueId()
        {
            // Arrange
            var isoCode = new IsoCode("CSQU3054383");
            var cargoType = new CargoType("General");
            var description = new ContainerDescription("Test cargo");

            // Act
            var container1 = new ProjArqsi.Domain.ContainerAggregate.Container(
                isoCode, false, cargoType, description);
            var container2 = new ProjArqsi.Domain.ContainerAggregate.Container(
                isoCode, false, cargoType, description);

            // Assert
            container1.Id.Should().NotBe(container2.Id);
        }

        [Fact]
        public void Constructor_WithHazardousCargo_ShouldSetIsHazardousToTrue()
        {
            // Arrange
            var isoCode = new IsoCode("CSQU3054383");
            var cargoType = new CargoType("Chemicals");
            var description = new ContainerDescription("Hazardous chemicals - Class 3");
            var isHazardous = true;

            // Act
            var container = new ProjArqsi.Domain.ContainerAggregate.Container(
                isoCode,
                isHazardous,
                cargoType,
                description);

            // Assert
            container.IsHazardous.Should().BeTrue();
        }

        [Fact]
        public void Constructor_WithNonHazardousCargo_ShouldSetIsHazardousToFalse()
        {
            // Arrange
            var isoCode = new IsoCode("CSQU3054383");
            var cargoType = new CargoType("Textiles");
            var description = new ContainerDescription("Cotton fabrics");
            var isHazardous = false;

            // Act
            var container = new ProjArqsi.Domain.ContainerAggregate.Container(
                isoCode,
                isHazardous,
                cargoType,
                description);

            // Assert
            container.IsHazardous.Should().BeFalse();
        }

        [Fact]
        public void Constructor_WithNullIsoCode_ShouldThrowBusinessRuleValidationException()
        {
            // Arrange
            IsoCode? nullIsoCode = null;
            var cargoType = new CargoType("General");
            var description = new ContainerDescription("Test cargo");

            // Act
            Action act = () => new ProjArqsi.Domain.ContainerAggregate.Container(
                nullIsoCode!,
                false,
                cargoType,
                description);

            // Assert
            act.Should().Throw<BusinessRuleValidationException>()
                .WithMessage("*ISO Code*");
        }

        [Fact]
        public void Constructor_WithNullCargoType_ShouldThrowBusinessRuleValidationException()
        {
            // Arrange
            var isoCode = new IsoCode("CSQU3054383");
            CargoType? nullCargoType = null;
            var description = new ContainerDescription("Test cargo");

            // Act
            Action act = () => new ProjArqsi.Domain.ContainerAggregate.Container(
                isoCode,
                false,
                nullCargoType!,
                description);

            // Assert
            act.Should().Throw<BusinessRuleValidationException>()
                .WithMessage("*Cargo type*");
        }

        [Fact]
        public void Constructor_WithNullDescription_ShouldThrowBusinessRuleValidationException()
        {
            // Arrange
            var isoCode = new IsoCode("CSQU3054383");
            var cargoType = new CargoType("General");
            ContainerDescription? nullDescription = null;

            // Act
            Action act = () => new ProjArqsi.Domain.ContainerAggregate.Container(
                isoCode,
                false,
                cargoType,
                nullDescription!);

            // Assert
            act.Should().Throw<BusinessRuleValidationException>()
                .WithMessage("*Description*");
        }

        #endregion

        #region UpdateCargoInformation Tests

        [Fact]
        public void UpdateCargoInformation_WithValidParameters_ShouldUpdateAllFields()
        {
            // Arrange
            var container = CreateDefaultContainer();
            var newCargoType = new CargoType("Perishables");
            var newDescription = new ContainerDescription("Fresh vegetables from Asia");
            var newIsHazardous = true;

            // Act
            container.UpdateCargoInformation(newCargoType, newDescription, newIsHazardous);

            // Assert
            container.CargoType.Should().Be(newCargoType);
            container.Description.Should().Be(newDescription);
            container.IsHazardous.Should().Be(newIsHazardous);
        }

        [Fact]
        public void UpdateCargoInformation_WithNewCargoType_ShouldUpdateCargoType()
        {
            // Arrange
            var container = CreateDefaultContainer();
            var originalCargoType = container.CargoType;
            var newCargoType = new CargoType("Electronics");

            // Act
            container.UpdateCargoInformation(
                newCargoType,
                container.Description,
                container.IsHazardous);

            // Assert
            container.CargoType.Should().Be(newCargoType);
            container.CargoType.Should().NotBe(originalCargoType);
        }

        [Fact]
        public void UpdateCargoInformation_WithNewDescription_ShouldUpdateDescription()
        {
            // Arrange
            var container = CreateDefaultContainer();
            var originalDescription = container.Description;
            var newDescription = new ContainerDescription("Updated cargo description");

            // Act
            container.UpdateCargoInformation(
                container.CargoType,
                newDescription,
                container.IsHazardous);

            // Assert
            container.Description.Should().Be(newDescription);
            container.Description.Should().NotBe(originalDescription);
        }

        [Fact]
        public void UpdateCargoInformation_ChangingHazardousStatus_ShouldUpdateIsHazardous()
        {
            // Arrange
            var container = CreateDefaultContainer();
            var originalStatus = container.IsHazardous;

            // Act
            container.UpdateCargoInformation(
                container.CargoType,
                container.Description,
                !originalStatus);

            // Assert
            container.IsHazardous.Should().Be(!originalStatus);
        }

        [Fact]
        public void UpdateCargoInformation_ToHazardous_ShouldSetIsHazardousToTrue()
        {
            // Arrange
            var container = new ProjArqsi.Domain.ContainerAggregate.Container(
                new IsoCode("CSQU3054383"),
                false,
                new CargoType("General"),
                new ContainerDescription("Non-hazardous cargo"));

            var hazardousCargoType = new CargoType("Flammable Liquids");
            var hazardousDescription = new ContainerDescription("Gasoline - UN 1203");

            // Act
            container.UpdateCargoInformation(hazardousCargoType, hazardousDescription, true);

            // Assert
            container.IsHazardous.Should().BeTrue();
            container.CargoType.Should().Be(hazardousCargoType);
            container.Description.Should().Be(hazardousDescription);
        }

        [Fact]
        public void UpdateCargoInformation_ToNonHazardous_ShouldSetIsHazardousToFalse()
        {
            // Arrange
            var container = new ProjArqsi.Domain.ContainerAggregate.Container(
                new IsoCode("CSQU3054383"),
                true,
                new CargoType("Chemicals"),
                new ContainerDescription("Hazardous cargo"));

            var safeCargoType = new CargoType("Textiles");
            var safeDescription = new ContainerDescription("Cotton fabrics");

            // Act
            container.UpdateCargoInformation(safeCargoType, safeDescription, false);

            // Assert
            container.IsHazardous.Should().BeFalse();
            container.CargoType.Should().Be(safeCargoType);
            container.Description.Should().Be(safeDescription);
        }

        [Fact]
        public void UpdateCargoInformation_WithNullCargoType_ShouldThrowBusinessRuleValidationException()
        {
            // Arrange
            var container = CreateDefaultContainer();
            CargoType? nullCargoType = null;

            // Act
            Action act = () => container.UpdateCargoInformation(
                nullCargoType!,
                container.Description,
                container.IsHazardous);

            // Assert
            act.Should().Throw<BusinessRuleValidationException>()
                .WithMessage("*Cargo type*");
        }

        [Fact]
        public void UpdateCargoInformation_WithNullDescription_ShouldThrowBusinessRuleValidationException()
        {
            // Arrange
            var container = CreateDefaultContainer();
            ContainerDescription? nullDescription = null;

            // Act
            Action act = () => container.UpdateCargoInformation(
                container.CargoType,
                nullDescription!,
                container.IsHazardous);

            // Assert
            act.Should().Throw<BusinessRuleValidationException>()
                .WithMessage("*Description*");
        }

        [Fact]
        public void UpdateCargoInformation_MultipleTimes_ShouldAlwaysUseLatestValues()
        {
            // Arrange
            var container = CreateDefaultContainer();

            var cargoType1 = new CargoType("Type1");
            var description1 = new ContainerDescription("Description1");
            var cargoType2 = new CargoType("Type2");
            var description2 = new ContainerDescription("Description2");
            var cargoType3 = new CargoType("Type3");
            var description3 = new ContainerDescription("Description3");

            // Act
            container.UpdateCargoInformation(cargoType1, description1, false);
            container.UpdateCargoInformation(cargoType2, description2, true);
            container.UpdateCargoInformation(cargoType3, description3, false);

            // Assert
            container.CargoType.Should().Be(cargoType3);
            container.Description.Should().Be(description3);
            container.IsHazardous.Should().BeFalse();
        }

        [Fact]
        public void UpdateCargoInformation_ShouldNotChangeContainerId()
        {
            // Arrange
            var container = CreateDefaultContainer();
            var originalId = container.Id;

            // Act
            container.UpdateCargoInformation(
                new CargoType("New Type"),
                new ContainerDescription("New Description"),
                true);

            // Assert
            container.Id.Should().Be(originalId);
        }

        [Fact]
        public void UpdateCargoInformation_ShouldNotChangeIsoCode()
        {
            // Arrange
            var container = CreateDefaultContainer();
            var originalIsoCode = container.IsoCode;

            // Act
            container.UpdateCargoInformation(
                new CargoType("New Type"),
                new ContainerDescription("New Description"),
                true);

            // Assert
            container.IsoCode.Should().Be(originalIsoCode);
        }

        #endregion

        #region ToString Tests

        [Fact]
        public void ToString_ShouldReturnFormattedString()
        {
            // Arrange
            var container = CreateDefaultContainer();

            // Act
            var result = container.ToString();

            // Assert
            result.Should().NotBeNullOrEmpty();
            result.Should().Contain("Container");
            result.Should().Contain(container.CargoType.Type);
            result.Should().Contain(container.Description.Text);
        }

        [Fact]
        public void ToString_WithHazardousCargo_ShouldIndicateHazmat()
        {
            // Arrange
            var container = new ProjArqsi.Domain.ContainerAggregate.Container(
                new IsoCode("CSQU3054383"),
                true,
                new CargoType("Chemicals"),
                new ContainerDescription("Dangerous goods"));

            // Act
            var result = container.ToString();

            // Assert
            result.Should().Contain("Hazmat");
            result.Should().Contain("True");
        }

        [Fact]
        public void ToString_WithNonHazardousCargo_ShouldIndicateNonHazmat()
        {
            // Arrange
            var container = new ProjArqsi.Domain.ContainerAggregate.Container(
                new IsoCode("CSQU3054383"),
                false,
                new CargoType("Textiles"),
                new ContainerDescription("Safe cargo"));

            // Act
            var result = container.ToString();

            // Assert
            result.Should().Contain("Hazmat");
            result.Should().Contain("False");
        }

        #endregion

        #region Real-World Scenarios

        [Fact]
        public void Container_RefrigeratedFoodShipment_ShouldBeCreatedAndTracked()
        {
            // Arrange - Container loaded with fresh produce
            var isoCode = new IsoCode("ABCU1234569"); // 45ft refrigerated high cube
            var cargoType = new CargoType("Perishables");
            var description = new ContainerDescription("Fresh bananas from Ecuador, keep at 13°C");

            // Act
            var container = new ProjArqsi.Domain.ContainerAggregate.Container(
                isoCode, false, cargoType, description);

            // Assert
            container.Should().NotBeNull();
            container.IsHazardous.Should().BeFalse();
            container.IsoCode.Value.Should().Be("ABCU1234569");
        }

        [Fact]
        public void Container_HazardousChemicalShipment_ShouldBeMarkedAsHazardous()
        {
            // Arrange - Container with dangerous chemicals
            var isoCode = new IsoCode("CSQU3054383");
            var cargoType = new CargoType("Chemicals - Class 3");
            var description = new ContainerDescription("Flammable liquids, UN 1203, Gasoline");

            // Act
            var container = new ProjArqsi.Domain.ContainerAggregate.Container(
                isoCode, true, cargoType, description);

            // Assert
            container.IsHazardous.Should().BeTrue();
            container.Description.Text.Should().Contain("UN 1203");
        }

        [Fact]
        public void Container_GeneralCargoUpgrade_ShouldUpdateToRefrigerated()
        {
            // Arrange - Original general cargo
            var container = new ProjArqsi.Domain.ContainerAggregate.Container(
                new IsoCode("CSQU3054383"),
                false,
                new CargoType("General"),
                new ContainerDescription("Dry goods"));

            // Act - Upgrade to refrigerated cargo
            container.UpdateCargoInformation(
                new CargoType("Refrigerated"),
                new ContainerDescription("Fresh meat, maintain at -18°C"),
                false);

            // Assert
            container.CargoType.Type.Should().Be("Refrigerated");
            container.Description.Text.Should().Contain("-18°C");
        }

        [Fact]
        public void Container_ElectronicsShipment_ShouldBeNonHazardous()
        {
            // Arrange
            var isoCode = new IsoCode("CSQU3054383");
            var cargoType = new CargoType("Electronics");
            var description = new ContainerDescription("Laptop computers, fragile, keep dry");

            // Act
            var container = new ProjArqsi.Domain.ContainerAggregate.Container(
                isoCode, false, cargoType, description);

            // Assert
            container.IsHazardous.Should().BeFalse();
            container.CargoType.Type.Should().Be("Electronics");
        }

        [Fact]
        public void Container_MultipleCargoChanges_DuringVoyage_ShouldTrackLatestInformation()
        {
            // Arrange - Container starts with general cargo
            var container = new ProjArqsi.Domain.ContainerAggregate.Container(
                new IsoCode("CSQU3054383"),
                false,
                new CargoType("General"),
                new ContainerDescription("Mixed goods"));

            // Act - Cargo is inspected and reclassified multiple times
            container.UpdateCargoInformation(
                new CargoType("Electronics"),
                new ContainerDescription("Consumer electronics from China"),
                false);

            container.UpdateCargoInformation(
                new CargoType("Electronics"),
                new ContainerDescription("Consumer electronics - batteries removed, compliant"),
                false);

            // Assert
            container.CargoType.Type.Should().Be("Electronics");
            container.Description.Text.Should().Contain("batteries removed");
            container.IsHazardous.Should().BeFalse();
        }

        [Fact]
        public void Container_ReclassifiedAsHazardous_AfterInspection_ShouldUpdateStatus()
        {
            // Arrange - Container initially declared as non-hazardous
            var container = new ProjArqsi.Domain.ContainerAggregate.Container(
                new IsoCode("CSQU3054383"),
                false,
                new CargoType("Chemicals"),
                new ContainerDescription("Industrial cleaning supplies"));

            // Act - After inspection, found to contain hazardous materials
            container.UpdateCargoInformation(
                new CargoType("Chemicals - Class 8"),
                new ContainerDescription("Corrosive substances - UN 1789, Hydrochloric acid"),
                true);

            // Assert
            container.IsHazardous.Should().BeTrue();
            container.Description.Text.Should().Contain("UN 1789");
            container.CargoType.Type.Should().Contain("Class 8");
        }

        [Fact]
        public void Container_HighValueCargo_ShouldMaintainDetailedDescription()
        {
            // Arrange
            var isoCode = new IsoCode("CSQU3054383");
            var cargoType = new CargoType("High Value");
            var description = new ContainerDescription(
                "Medical equipment: 20x MRI scanners, value $15M, requires climate control and security");

            // Act
            var container = new ProjArqsi.Domain.ContainerAggregate.Container(
                isoCode, false, cargoType, description);

            // Assert
            container.Description.Text.Should().Contain("MRI scanners");
            container.Description.Text.Should().Contain("$15M");
            container.Description.Text.Should().Contain("security");
        }

        [Fact]
        public void Container_AutoPartsShipment_ShouldAllowDescriptionUpdate()
        {
            // Arrange
            var container = new ProjArqsi.Domain.ContainerAggregate.Container(
                new IsoCode("CSQU3054383"),
                false,
                new CargoType("Automotive"),
                new ContainerDescription("Car parts - unspecified"));

            // Act - Detailed manifest becomes available
            container.UpdateCargoInformation(
                new CargoType("Automotive"),
                new ContainerDescription("Car parts: 500 brake pads, 200 air filters, 100 headlights"),
                false);

            // Assert
            container.Description.Text.Should().Contain("brake pads");
            container.Description.Text.Should().Contain("air filters");
            container.Description.Text.Should().Contain("headlights");
        }

        #endregion

        #region Identity and Equality Tests

        [Fact]
        public void Container_WithSameId_ShouldBeConsideredEqual()
        {
            // Arrange
            var container = CreateDefaultContainer();
            var containerId = container.Id;

            // Create another container reference with same ID (simulating retrieval from DB)
            // Note: In real scenario, this would come from repository
            var sameContainer = container;

            // Act & Assert
            sameContainer.Id.Should().Be(containerId);
            ReferenceEquals(container, sameContainer).Should().BeTrue();
        }

        [Fact]
        public void Container_WithDifferentIds_ShouldNotBeEqual()
        {
            // Arrange
            var container1 = CreateDefaultContainer();
            var container2 = CreateDefaultContainer();

            // Act & Assert
            container1.Id.Should().NotBe(container2.Id);
        }

        [Fact]
        public void Container_IdShouldBeImmutable()
        {
            // Arrange
            var container = CreateDefaultContainer();
            var originalId = container.Id;

            // Act - Perform various operations
            container.UpdateCargoInformation(
                new CargoType("New Type"),
                new ContainerDescription("New Description"),
                true);

            // Assert
            container.Id.Should().Be(originalId);
        }

        [Fact]
        public void Container_IsoCodeShouldBeImmutableAfterCreation()
        {
            // Arrange
            var originalIsoCode = new IsoCode("CSQU3054383");
            var container = new ProjArqsi.Domain.ContainerAggregate.Container(
                originalIsoCode,
                false,
                new CargoType("General"),
                new ContainerDescription("Test"));

            // Act - Try to update cargo information
            container.UpdateCargoInformation(
                new CargoType("New Type"),
                new ContainerDescription("New Description"),
                false);

            // Assert
            container.IsoCode.Should().Be(originalIsoCode);
        }

        #endregion

        #region Edge Cases

        [Fact]
        public void Container_WithEmptyString_InCargoType_ShouldBeHandledByValueObject()
        {
            // Arrange & Act
            Action act = () => new ProjArqsi.Domain.ContainerAggregate.Container(
                new IsoCode("CSQU3054383"),
                false,
                new CargoType(""),
                new ContainerDescription("Test"));

            // Assert - CargoType validation should handle this
            act.Should().Throw<BusinessRuleValidationException>();
        }

        [Fact]
        public void Container_WithEmptyString_InDescription_ShouldBeHandledByValueObject()
        {
            // Arrange & Act
            Action act = () => new ProjArqsi.Domain.ContainerAggregate.Container(
                new IsoCode("CSQU3054383"),
                false,
                new CargoType("General"),
                new ContainerDescription(""));

            // Assert - ContainerDescription validation should handle this
            act.Should().Throw<BusinessRuleValidationException>();
        }

        [Fact]
        public void Container_WithVeryLongDescription_ShouldBeAccepted()
        {
            // Arrange
            var longDescription = new string('A', 500); // 500 characters
            var description = new ContainerDescription(longDescription);

            // Act
            var container = new ProjArqsi.Domain.ContainerAggregate.Container(
                new IsoCode("CSQU3054383"),
                false,
                new CargoType("General"),
                description);

            // Assert
            container.Description.Text.Should().HaveLength(500);
        }

        #endregion

        #region Helper Methods

        private ProjArqsi.Domain.ContainerAggregate.Container CreateDefaultContainer()
        {
            return new ProjArqsi.Domain.ContainerAggregate.Container(
                new IsoCode("CSQU3054383"),
                false,
                new CargoType("General"),
                new ContainerDescription("Test cargo"));
        }

        #endregion
    }
}
