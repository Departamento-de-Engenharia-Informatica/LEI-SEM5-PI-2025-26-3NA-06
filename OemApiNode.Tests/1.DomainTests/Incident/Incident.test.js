const Incident = require("../../../OemApiNode/src/domain/Incident/Incident");

describe("Incident Domain Tests", () => {
  describe("Constructor", () => {
    test("should create an Incident with valid data", () => {
      // Arrange
      const incidentData = {
        incidentTypeId: "123e4567-e89b-12d3-a456-426614174001",
        date: new Date("2024-01-15"),
        startTime: new Date("2024-01-15T10:00:00"),
        endTime: new Date("2024-01-15T12:00:00"),
        description: "Equipment malfunction in crane 3",
        affectsAllVVEs: false,
        affectedVVEIds: ["vve-123", "vve-456"],
      };

      // Act
      const incident = new Incident(incidentData);

      // Assert
      expect(incident).toBeDefined();
      expect(incident.incidentTypeId).toBe(incidentData.incidentTypeId);
      expect(incident.description).toBe(incidentData.description);
      expect(incident.affectsAllVVEs).toBe(false);
      expect(incident.affectedVVEIds).toEqual(["vve-123", "vve-456"]);
    });

    test("should have an ID after creation", () => {
      // Arrange
      const incidentData = {
        incidentTypeId: "123e4567-e89b-12d3-a456-426614174001",
        startTime: new Date(),
        description: "Test incident",
      };

      // Act
      const incident = new Incident(incidentData);

      // Assert
      expect(incident.id).toBeDefined();
      expect(typeof incident.id).toBe("string");
    });

    test("should have timestamps after creation", () => {
      // Arrange
      const incidentData = {
        incidentTypeId: "123e4567-e89b-12d3-a456-426614174001",
        startTime: new Date(),
        description: "Test incident",
      };

      // Act
      const incident = new Incident(incidentData);

      // Assert
      expect(incident.createdAt).toBeDefined();
      expect(incident.createdAt).toBeInstanceOf(Date);
    });

    test("should default isActive to true", () => {
      // Arrange
      const incidentData = {
        incidentTypeId: "123e4567-e89b-12d3-a456-426614174001",
        startTime: new Date(),
        description: "Test incident",
        affectsAllVVEs: true,
      };

      // Act
      const incident = new Incident(incidentData);

      // Assert
      expect(incident.isActive).toBe(true);
    });
  });

  describe("Validation", () => {
    test("should throw error when description is empty", () => {
      // Arrange
      const incident = new Incident({
        incidentTypeId: "123e4567-e89b-12d3-a456-426614174001",
        startTime: new Date(),
        description: "",
        affectsAllVVEs: true,
      });

      // Act & Assert
      expect(() => incident.validate()).toThrow("Description is required");
    });

    test("should throw error when incidentTypeId is missing", () => {
      // Arrange
      const incident = new Incident({
        startTime: new Date(),
        description: "Test incident",
        affectsAllVVEs: true,
      });

      // Act & Assert
      expect(() => incident.validate()).toThrow("Incident Type is required");
    });

    test("should throw error when startTime is missing", () => {
      // Arrange
      const incident = new Incident({
        incidentTypeId: "123e4567-e89b-12d3-a456-426614174001",
        description: "Test incident",
        affectsAllVVEs: true,
      });

      // Act & Assert
      expect(() => incident.validate()).toThrow("Start time is required");
    });

    test("should throw error when endTime is before startTime", () => {
      // Arrange
      const incident = new Incident({
        incidentTypeId: "123e4567-e89b-12d3-a456-426614174001",
        startTime: new Date("2024-01-15T12:00:00"),
        endTime: new Date("2024-01-15T10:00:00"),
        description: "Test incident",
        affectsAllVVEs: true,
      });

      // Act & Assert
      expect(() => incident.validate()).toThrow(
        "End time cannot be before start time"
      );
    });

    test("should throw error when no VVEs selected and affectsAllVVEs is false", () => {
      // Arrange
      const incident = new Incident({
        incidentTypeId: "123e4567-e89b-12d3-a456-426614174001",
        startTime: new Date(),
        description: "Test incident",
        affectsAllVVEs: false,
        affectedVVEIds: [],
      });

      // Act & Assert
      expect(() => incident.validate()).toThrow(
        "Must select affected VVEs or check 'Affects All VVEs'"
      );
    });

    test("should pass validation with valid data", () => {
      // Arrange
      const incident = new Incident({
        incidentTypeId: "123e4567-e89b-12d3-a456-426614174001",
        startTime: new Date(),
        description: "Test incident",
        affectsAllVVEs: true,
      });

      // Act & Assert
      expect(() => incident.validate()).not.toThrow();
    });
  });

  describe("Business Logic", () => {
    test("getDurationMinutes should return null when no end time", () => {
      // Arrange
      const incident = new Incident({
        incidentTypeId: "123e4567-e89b-12d3-a456-426614174001",
        startTime: new Date(),
        description: "Test incident",
        affectsAllVVEs: true,
      });

      // Act
      const duration = incident.getDurationMinutes();

      // Assert
      expect(duration).toBeNull();
    });

    test("getDurationMinutes should calculate duration correctly", () => {
      // Arrange
      const startTime = new Date("2024-01-15T10:00:00");
      const endTime = new Date("2024-01-15T12:30:00");

      const incident = new Incident({
        incidentTypeId: "123e4567-e89b-12d3-a456-426614174001",
        startTime,
        endTime,
        description: "Test incident",
        affectsAllVVEs: true,
      });

      // Act
      const duration = incident.getDurationMinutes();

      // Assert
      expect(duration).toBe(150); // 2.5 hours = 150 minutes
    });

    test("update should modify incident properties", () => {
      // Arrange
      const incident = new Incident({
        incidentTypeId: "123e4567-e89b-12d3-a456-426614174001",
        startTime: new Date(),
        description: "Original description",
        affectsAllVVEs: false,
        affectedVVEIds: ["vve-1"],
      });

      // Act
      incident.update({
        description: "Updated description",
        affectedVVEIds: ["vve-1", "vve-2"],
      });

      // Assert
      expect(incident.description).toBe("Updated description");
      expect(incident.affectedVVEIds).toEqual(["vve-1", "vve-2"]);
    });
  });

  describe("Dummy Test", () => {
    test("should pass - basic test to verify Jest is working", () => {
      // Arrange
      const expected = true;

      // Act
      const actual = true;

      // Assert
      expect(actual).toBe(expected);
    });

    test("should pass - math operations work correctly", () => {
      // Arrange
      const a = 1;
      const b = 2;

      // Act
      const result = a + b;

      // Assert
      expect(result).toBe(3);
    });
  });
});
