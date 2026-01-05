const IncidentType = require("../../../OemApiNode/src/domain/IncidentType/IncidentType");

describe("IncidentType Domain Tests", () => {
  describe("Constructor", () => {
    test("should create an IncidentType with valid data", () => {
      // Arrange
      const data = {
        code: "EQ-FAIL",
        name: "Equipment Failure",
        description: "Equipment malfunction or breakdown",
        severity: "Major",
      };

      // Act
      const incidentType = new IncidentType(data);

      // Assert
      expect(incidentType).toBeDefined();
      expect(incidentType.code).toBe("EQ-FAIL");
      expect(incidentType.name).toBe("Equipment Failure");
      expect(incidentType.severity).toBe("Major");
      expect(incidentType.isActive).toBe(true);
    });

    test("should generate UUID if not provided", () => {
      // Arrange & Act
      const incidentType = new IncidentType({
        code: "TEST",
        name: "Test Type",
        severity: "Minor",
      });

      // Assert
      expect(incidentType.id).toBeDefined();
      expect(typeof incidentType.id).toBe("string");
    });

    test("should accept parentId for hierarchical structure", () => {
      // Arrange
      const parentId = "123e4567-e89b-12d3-a456-426614174000";

      // Act
      const incidentType = new IncidentType({
        code: "SUB-TYPE",
        name: "Sub Type",
        severity: "Minor",
        parentId,
      });

      // Assert
      expect(incidentType.parentId).toBe(parentId);
    });

    test("should throw error if code is missing", () => {
      // Arrange & Act & Assert
      expect(
        () =>
          new IncidentType({
            name: "Test Type",
            severity: "Minor",
          })
      ).toThrow("Code is required");
    });

    test("should throw error if code exceeds 10 characters", () => {
      // Arrange & Act & Assert
      expect(
        () =>
          new IncidentType({
            code: "VERY-LONG-CODE",
            name: "Test Type",
            severity: "Minor",
          })
      ).toThrow("Code must be between 1 and 10 characters");
    });

    test("should throw error if code contains invalid characters", () => {
      // Arrange & Act & Assert
      expect(
        () =>
          new IncidentType({
            code: "TEST@CODE",
            name: "Test Type",
            severity: "Minor",
          })
      ).toThrow("Code must be alphanumeric");
    });
  });

  describe("Validation", () => {
    test("should throw error if name is empty", () => {
      // Arrange & Act & Assert
      expect(
        () =>
          new IncidentType({
            code: "TEST",
            name: "",
            severity: "Minor",
          })
      ).toThrow("Name is required");
    });

    test("should throw error if name exceeds 255 characters", () => {
      // Arrange
      const longName = "A".repeat(256);

      // Act & Assert
      expect(
        () =>
          new IncidentType({
            code: "TEST",
            name: longName,
            severity: "Minor",
          })
      ).toThrow("Name must not exceed 255 characters");
    });

    test("should throw error if severity is invalid", () => {
      // Arrange & Act & Assert
      expect(
        () =>
          new IncidentType({
            code: "TEST",
            name: "Test Type",
            severity: "SuperCritical",
          })
      ).toThrow("Severity must be one of: Minor, Major, Critical");
    });

    test("should accept valid severity values", () => {
      // Arrange & Act
      const minor = new IncidentType({
        code: "T1",
        name: "Type 1",
        severity: "Minor",
      });
      const major = new IncidentType({
        code: "T2",
        name: "Type 2",
        severity: "Major",
      });
      const critical = new IncidentType({
        code: "T3",
        name: "Type 3",
        severity: "Critical",
      });

      // Assert
      expect(minor.severity).toBe("Minor");
      expect(major.severity).toBe("Major");
      expect(critical.severity).toBe("Critical");
    });

    test("should throw error if incidentType is its own parent", () => {
      // Arrange
      const id = "123e4567-e89b-12d3-a456-426614174000";

      // Act & Assert
      expect(
        () =>
          new IncidentType({
            id,
            code: "TEST",
            name: "Test Type",
            severity: "Minor",
            parentId: id,
          })
      ).toThrow("Incident type cannot be its own parent");
    });

    test("should throw error if parentId is invalid UUID", () => {
      // Arrange & Act & Assert
      expect(
        () =>
          new IncidentType({
            code: "TEST",
            name: "Test Type",
            severity: "Minor",
            parentId: "invalid-uuid",
          })
      ).toThrow("Invalid parent ID format");
    });
  });

  describe("Update", () => {
    test("should update name", () => {
      // Arrange
      const incidentType = new IncidentType({
        code: "TEST",
        name: "Original Name",
        severity: "Minor",
      });

      // Act
      incidentType.update({ name: "Updated Name" });

      // Assert
      expect(incidentType.name).toBe("Updated Name");
      expect(incidentType.updatedAt).toBeInstanceOf(Date);
    });

    test("should update description", () => {
      // Arrange
      const incidentType = new IncidentType({
        code: "TEST",
        name: "Test Type",
        severity: "Minor",
      });

      // Act
      incidentType.update({ description: "New description" });

      // Assert
      expect(incidentType.description).toBe("New description");
    });

    test("should update severity", () => {
      // Arrange
      const incidentType = new IncidentType({
        code: "TEST",
        name: "Test Type",
        severity: "Minor",
      });

      // Act
      incidentType.update({ severity: "Critical" });

      // Assert
      expect(incidentType.severity).toBe("Critical");
    });

    test("should not allow code update", () => {
      // Arrange
      const incidentType = new IncidentType({
        code: "TEST",
        name: "Test Type",
        severity: "Minor",
      });
      const originalCode = incidentType.code;

      // Act
      incidentType.update({ code: "NEWCODE" });

      // Assert
      expect(incidentType.code).toBe(originalCode);
    });

    test("should update isActive status", () => {
      // Arrange
      const incidentType = new IncidentType({
        code: "TEST",
        name: "Test Type",
        severity: "Minor",
      });

      // Act
      incidentType.update({ isActive: false });

      // Assert
      expect(incidentType.isActive).toBe(false);
    });
  });

  describe("Timestamps", () => {
    test("should set createdAt on construction", () => {
      // Arrange & Act
      const incidentType = new IncidentType({
        code: "TEST",
        name: "Test Type",
        severity: "Minor",
      });

      // Assert
      expect(incidentType.createdAt).toBeInstanceOf(Date);
    });

    test("should set updatedAt on update", () => {
      // Arrange
      const incidentType = new IncidentType({
        code: "TEST",
        name: "Test Type",
        severity: "Minor",
      });
      const originalUpdatedAt = incidentType.updatedAt;

      // Act
      incidentType.update({ name: "Updated Name" });

      // Assert
      expect(incidentType.updatedAt).toBeInstanceOf(Date);
      expect(incidentType.updatedAt).not.toBe(originalUpdatedAt);
    });
  });

  describe("Hierarchical Structure", () => {
    test("should allow null parentId for root types", () => {
      // Arrange & Act
      const rootType = new IncidentType({
        code: "ROOT",
        name: "Root Type",
        severity: "Major",
        parentId: null,
      });

      // Assert
      expect(rootType.parentId).toBeNull();
    });

    test("should allow valid parentId for sub-types", () => {
      // Arrange
      const parentId = "123e4567-e89b-12d3-a456-426614174000";

      // Act
      const subType = new IncidentType({
        code: "SUB",
        name: "Sub Type",
        severity: "Minor",
        parentId,
      });

      // Assert
      expect(subType.parentId).toBe(parentId);
    });
  });
});
