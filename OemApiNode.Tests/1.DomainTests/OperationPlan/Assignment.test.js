const Assignment = require("../../../OemApiNode/src/domain/OperationPlan/Assignment");

describe("Assignment Value Object Tests", () => {
  describe("Constructor", () => {
    test("should create an Assignment with valid data", () => {
      // Arrange
      const data = {
        vvnId: "123e4567-e89b-12d3-a456-426614174000",
        dockId: "223e4567-e89b-12d3-a456-426614174000",
        dockName: "Dock A",
        eta: new Date("2024-01-15T10:00:00"),
        etd: new Date("2024-01-15T14:00:00"),
        estimatedTeu: 1500,
        vesselName: "Ever Given",
        vesselImo: "IMO1234567",
      };

      // Act
      const assignment = new Assignment(data);

      // Assert
      expect(assignment.vvnId).toBe(data.vvnId);
      expect(assignment.dockId).toBe(data.dockId);
      expect(assignment.dockName).toBe("Dock A");
      expect(assignment.eta).toEqual(new Date("2024-01-15T10:00:00"));
      expect(assignment.etd).toEqual(new Date("2024-01-15T14:00:00"));
      expect(assignment.estimatedTeu).toBe(1500);
    });

    test("should throw error if vvnId is missing", () => {
      // Arrange & Act & Assert
      expect(
        () =>
          new Assignment({
            dockId: "223e4567-e89b-12d3-a456-426614174000",
            eta: new Date(),
            etd: new Date(Date.now() + 3600000),
          })
      ).toThrow("vvnId is required");
    });

    test("should throw error if dockId is missing", () => {
      // Arrange & Act & Assert
      expect(
        () =>
          new Assignment({
            vvnId: "123e4567-e89b-12d3-a456-426614174000",
            eta: new Date(),
            etd: new Date(Date.now() + 3600000),
          })
      ).toThrow("dockId is required");
    });

    test("should throw error if eta is after etd", () => {
      // Arrange & Act & Assert
      expect(
        () =>
          new Assignment({
            vvnId: "123e4567-e89b-12d3-a456-426614174000",
            dockId: "223e4567-e89b-12d3-a456-426614174000",
            eta: new Date("2024-01-15T14:00:00"),
            etd: new Date("2024-01-15T10:00:00"),
          })
      ).toThrow("eta must be before etd");
    });

    test("should throw error if eta is invalid date", () => {
      // Arrange & Act & Assert
      expect(
        () =>
          new Assignment({
            vvnId: "123e4567-e89b-12d3-a456-426614174000",
            dockId: "223e4567-e89b-12d3-a456-426614174000",
            eta: "invalid-date",
            etd: new Date(),
          })
      ).toThrow("Invalid eta date");
    });
  });

  describe("Overlap Detection", () => {
    test("should detect overlap on same dock with overlapping times", () => {
      // Arrange
      const assignment1 = new Assignment({
        vvnId: "vvn1",
        dockId: "dock-a",
        eta: new Date("2024-01-15T10:00:00"),
        etd: new Date("2024-01-15T14:00:00"),
      });

      const assignment2 = new Assignment({
        vvnId: "vvn2",
        dockId: "dock-a",
        eta: new Date("2024-01-15T12:00:00"),
        etd: new Date("2024-01-15T16:00:00"),
      });

      // Act
      const overlaps = assignment1.overlapsWith(assignment2);

      // Assert
      expect(overlaps).toBe(true);
    });

    test("should not detect overlap on different docks", () => {
      // Arrange
      const assignment1 = new Assignment({
        vvnId: "vvn1",
        dockId: "dock-a",
        eta: new Date("2024-01-15T10:00:00"),
        etd: new Date("2024-01-15T14:00:00"),
      });

      const assignment2 = new Assignment({
        vvnId: "vvn2",
        dockId: "dock-b",
        eta: new Date("2024-01-15T12:00:00"),
        etd: new Date("2024-01-15T16:00:00"),
      });

      // Act
      const overlaps = assignment1.overlapsWith(assignment2);

      // Assert
      expect(overlaps).toBe(false);
    });

    test("should not detect overlap with consecutive times", () => {
      // Arrange
      const assignment1 = new Assignment({
        vvnId: "vvn1",
        dockId: "dock-a",
        eta: new Date("2024-01-15T10:00:00"),
        etd: new Date("2024-01-15T14:00:00"),
      });

      const assignment2 = new Assignment({
        vvnId: "vvn2",
        dockId: "dock-a",
        eta: new Date("2024-01-15T14:00:00"),
        etd: new Date("2024-01-15T18:00:00"),
      });

      // Act
      const overlaps = assignment1.overlapsWith(assignment2);

      // Assert
      expect(overlaps).toBe(false);
    });
  });

  describe("JSON Serialization", () => {
    test("should convert to JSON correctly", () => {
      // Arrange
      const assignment = new Assignment({
        vvnId: "vvn1",
        dockId: "dock-a",
        dockName: "Terminal A",
        eta: new Date("2024-01-15T10:00:00"),
        etd: new Date("2024-01-15T14:00:00"),
        estimatedTeu: 1500,
        vesselName: "Ever Given",
        vesselImo: "IMO1234567",
      });

      // Act
      const json = assignment.toJSON();

      // Assert
      expect(json.vvnId).toBe("vvn1");
      expect(json.dockId).toBe("dock-a");
      expect(json.eta).toBe(new Date("2024-01-15T10:00:00").toISOString());
      expect(json.etd).toBe(new Date("2024-01-15T14:00:00").toISOString());
    });
  });

  describe("Default Values", () => {
    test("should default estimatedTeu to 0", () => {
      // Arrange & Act
      const assignment = new Assignment({
        vvnId: "vvn1",
        dockId: "dock-a",
        eta: new Date(),
        etd: new Date(Date.now() + 3600000),
      });

      // Assert
      expect(assignment.estimatedTeu).toBe(0);
    });

    test("should accept null optional fields", () => {
      // Arrange & Act
      const assignment = new Assignment({
        vvnId: "vvn1",
        dockId: "dock-a",
        eta: new Date(),
        etd: new Date(Date.now() + 3600000),
        dockName: null,
        vesselName: null,
        vesselImo: null,
      });

      // Assert
      expect(assignment.dockName).toBeNull();
      expect(assignment.vesselName).toBeNull();
      expect(assignment.vesselImo).toBeNull();
    });
  });
});
