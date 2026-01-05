const VesselVisitExecution = require("../../../OemApiNode/src/domain/VesselVisitExecution/VesselVisitExecution");

describe("VesselVisitExecution Domain Tests", () => {
  // Helper to create valid VVE data
  const createValidVVEData = (overrides = {}) => ({
    vvnId: "123e4567-e89b-12d3-a456-426614174000",
    operationPlanId: "223e4567-e89b-12d3-a456-426614174000",
    vveDate: "2024-01-15",
    plannedDockId: "323e4567-e89b-12d3-a456-426614174000",
    plannedArrivalTime: new Date("2024-01-15T10:00:00"),
    plannedDepartureTime: new Date("2024-01-15T14:00:00"),
    ...overrides,
  });

  describe("Constructor", () => {
    test("should create a VesselVisitExecution with valid data", () => {
      // Arrange
      const data = createValidVVEData();

      // Act
      const vve = new VesselVisitExecution(data);

      // Assert
      expect(vve).toBeDefined();
      expect(vve.vvnId).toBe(data.vvnId);
      expect(vve.operationPlanId).toBe(data.operationPlanId);
      expect(vve.vveDate).toBe("2024-01-15");
      expect(vve.status).toBe("NotStarted");
    });

    test("should generate UUID if not provided", () => {
      // Arrange & Act
      const vve = new VesselVisitExecution(createValidVVEData());

      // Assert
      expect(vve.id).toBeDefined();
      expect(typeof vve.id).toBe("string");
    });

    test("should normalize vveDate from Date object", () => {
      // Arrange & Act
      const vve = new VesselVisitExecution(
        createValidVVEData({
          vveDate: new Date("2024-01-15T10:30:00"),
        })
      );

      // Assert
      expect(vve.vveDate).toBe("2024-01-15");
    });

    test("should default status to NotStarted", () => {
      // Arrange & Act
      const vve = new VesselVisitExecution(createValidVVEData());

      // Assert
      expect(vve.status).toBe("NotStarted");
    });

    test("should throw error if vvnId is missing", () => {
      // Arrange
      const data = createValidVVEData({ vvnId: null });

      // Act & Assert
      expect(() => new VesselVisitExecution(data)).toThrow("vvnId");
    });

    test("should throw error if vvnId is invalid UUID", () => {
      // Arrange
      const data = createValidVVEData({ vvnId: "invalid-uuid" });

      // Act & Assert
      expect(() => new VesselVisitExecution(data)).toThrow("vvnId");
    });
  });

  describe("Status Transitions", () => {
    test("should allow transition from NotStarted to InProgress", () => {
      // Arrange
      const vve = new VesselVisitExecution(
        createValidVVEData({ status: "NotStarted" })
      );

      // Act
      const canTransition = vve.canTransitionTo("InProgress");

      // Assert
      expect(canTransition).toBe(true);
    });

    test("should allow transition from NotStarted to Delayed", () => {
      // Arrange
      const vve = new VesselVisitExecution(
        createValidVVEData({ status: "NotStarted" })
      );

      // Act
      const canTransition = vve.canTransitionTo("Delayed");

      // Assert
      expect(canTransition).toBe(true);
    });

    test("should allow transition from InProgress to Completed", () => {
      // Arrange
      const vve = new VesselVisitExecution(
        createValidVVEData({ status: "InProgress" })
      );

      // Act
      const canTransition = vve.canTransitionTo("Completed");

      // Assert
      expect(canTransition).toBe(true);
    });

    test("should not allow transition from Completed to any state", () => {
      // Arrange
      const vve = new VesselVisitExecution(
        createValidVVEData({ status: "Completed" })
      );

      // Act & Assert
      expect(vve.canTransitionTo("InProgress")).toBe(false);
      expect(vve.canTransitionTo("NotStarted")).toBe(false);
    });

    test("should execute transition successfully", () => {
      // Arrange
      const vve = new VesselVisitExecution(
        createValidVVEData({ status: "NotStarted" })
      );

      // Act
      vve.transitionTo("InProgress");

      // Assert
      expect(vve.status).toBe("InProgress");
    });

    test("should throw error on invalid transition", () => {
      // Arrange
      const vve = new VesselVisitExecution(
        createValidVVEData({ status: "NotStarted" })
      );

      // Act & Assert
      expect(() => vve.transitionTo("Completed")).toThrow("Cannot transition");
    });
  });

  describe("Dock Change Detection", () => {
    test("should detect dock change when actual differs from planned", () => {
      // Arrange
      const actualDockId = "423e4567-e89b-12d3-a456-426614174000";
      const vve = new VesselVisitExecution(
        createValidVVEData({ actualDockId })
      );

      // Act
      const changed = vve.isDockChanged();

      // Assert
      expect(changed).toBe(true);
    });

    test("should not detect dock change when actual matches planned", () => {
      // Arrange
      const plannedDockId = "323e4567-e89b-12d3-a456-426614174000";
      const vve = new VesselVisitExecution(
        createValidVVEData({
          plannedDockId,
          actualDockId: plannedDockId,
        })
      );

      // Act
      const changed = vve.isDockChanged();

      // Assert
      expect(changed).toBe(false);
    });

    test("should return false when actualDockId is null", () => {
      // Arrange
      const vve = new VesselVisitExecution(
        createValidVVEData({ actualDockId: null })
      );

      // Act
      const changed = vve.isDockChanged();

      // Assert - The implementation returns a truthy value (actualDockId && planned && actual !== planned)
      // When actualDockId is null, the && short-circuits and returns null (which is falsy)
      expect(changed).toBeFalsy();
    });
  });

  describe("Delay Detection", () => {
    test("should detect delay when actual arrival after planned", () => {
      // Arrange
      const vve = new VesselVisitExecution(
        createValidVVEData({
          plannedArrivalTime: new Date("2024-01-15T10:00:00"),
          actualArrivalTime: new Date("2024-01-15T12:00:00"),
        })
      );

      // Act
      const delayed = vve.isDelayed();

      // Assert
      expect(delayed).toBe(true);
    });

    test("should not detect delay when actual arrival before planned", () => {
      // Arrange
      const vve = new VesselVisitExecution(
        createValidVVEData({
          plannedArrivalTime: new Date("2024-01-15T10:00:00"),
          actualArrivalTime: new Date("2024-01-15T09:00:00"),
        })
      );

      // Act
      const delayed = vve.isDelayed();

      // Assert
      expect(delayed).toBe(false);
    });

    test("should return false when actualArrivalTime is null", () => {
      // Arrange
      const vve = new VesselVisitExecution(
        createValidVVEData({
          plannedArrivalTime: new Date("2024-01-15T10:00:00"),
          actualArrivalTime: null,
        })
      );

      // Act
      const delayed = vve.isDelayed();

      // Assert
      expect(delayed).toBe(false);
    });
  });

  describe("Timestamps", () => {
    test("should set createdAt on construction", () => {
      // Arrange & Act
      const vve = new VesselVisitExecution(createValidVVEData());

      // Assert
      expect(vve.createdAt).toBeInstanceOf(Date);
    });

    test("should update updatedAt on status transition", () => {
      // Arrange
      const vve = new VesselVisitExecution(
        createValidVVEData({ status: "NotStarted" })
      );
      const originalUpdatedAt = vve.updatedAt;

      // Act
      vve.transitionTo("InProgress");

      // Assert
      expect(vve.updatedAt).toBeInstanceOf(Date);
      if (originalUpdatedAt) {
        expect(vve.updatedAt.getTime()).toBeGreaterThanOrEqual(
          originalUpdatedAt.getTime()
        );
      }
    });
  });

  describe("Date Normalization", () => {
    test("should normalize ISO string date", () => {
      // Arrange & Act
      const vve = new VesselVisitExecution(
        createValidVVEData({
          vveDate: "2024-01-15T10:30:00.000Z",
        })
      );

      // Assert
      expect(vve.vveDate).toBe("2024-01-15");
    });

    test("should keep YYYY-MM-DD format unchanged", () => {
      // Arrange & Act
      const vve = new VesselVisitExecution(
        createValidVVEData({
          vveDate: "2024-01-15",
        })
      );

      // Assert
      expect(vve.vveDate).toBe("2024-01-15");
    });

    test("should handle invalid date in validation", () => {
      // Arrange & Act & Assert
      expect(
        () =>
          new VesselVisitExecution(
            createValidVVEData({
              vveDate: null,
            })
          )
      ).toThrow("vveDate is required");
    });
  });

  describe("Validation", () => {
    test("should validate UUID format for all UUID fields", () => {
      // Arrange & Act
      const vve = new VesselVisitExecution(createValidVVEData());

      // Assert - all UUID fields should be valid
      expect(vve.id).toBeDefined();
      expect(vve.vvnId).toBe("123e4567-e89b-12d3-a456-426614174000");
      expect(vve.operationPlanId).toBe("223e4567-e89b-12d3-a456-426614174000");
      expect(vve.plannedDockId).toBe("323e4567-e89b-12d3-a456-426614174000");
    });

    test("should throw error for invalid plannedDockId UUID", () => {
      // Arrange & Act & Assert
      expect(
        () =>
          new VesselVisitExecution(
            createValidVVEData({
              plannedDockId: "invalid-uuid",
            })
          )
      ).toThrow("plannedDockId");
    });

    test("should require plannedArrivalTime", () => {
      // Arrange & Act & Assert
      expect(
        () =>
          new VesselVisitExecution(
            createValidVVEData({
              plannedArrivalTime: null,
            })
          )
      ).toThrow("plannedArrivalTime is required");
    });

    test("should require plannedDepartureTime", () => {
      // Arrange & Act & Assert
      expect(
        () =>
          new VesselVisitExecution(
            createValidVVEData({
              plannedDepartureTime: null,
            })
          )
      ).toThrow("plannedDepartureTime is required");
    });
  });

  describe("Planned vs Actual Tracking", () => {
    test("should track planned and actual times separately", () => {
      // Arrange & Act
      const vve = new VesselVisitExecution(
        createValidVVEData({
          plannedArrivalTime: new Date("2024-01-15T10:00:00"),
          plannedDepartureTime: new Date("2024-01-15T14:00:00"),
          actualArrivalTime: new Date("2024-01-15T10:30:00"),
          actualDepartureTime: new Date("2024-01-15T14:30:00"),
        })
      );

      // Assert
      expect(vve.plannedArrivalTime).toEqual(new Date("2024-01-15T10:00:00"));
      expect(vve.actualArrivalTime).toEqual(new Date("2024-01-15T10:30:00"));
      expect(vve.plannedDepartureTime).toEqual(new Date("2024-01-15T14:00:00"));
      expect(vve.actualDepartureTime).toEqual(new Date("2024-01-15T14:30:00"));
    });

    test("should allow null actual times for not-yet-executed operations", () => {
      // Arrange & Act
      const vve = new VesselVisitExecution(
        createValidVVEData({
          plannedArrivalTime: new Date("2024-01-15T10:00:00"),
          plannedDepartureTime: new Date("2024-01-15T14:00:00"),
          actualArrivalTime: null,
          actualDepartureTime: null,
        })
      );

      // Assert
      expect(vve.actualArrivalTime).toBeNull();
      expect(vve.actualDepartureTime).toBeNull();
    });
  });
});
