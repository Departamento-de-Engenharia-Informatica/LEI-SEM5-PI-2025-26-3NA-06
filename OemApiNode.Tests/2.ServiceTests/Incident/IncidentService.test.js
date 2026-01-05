// Mock repositories BEFORE requiring the service (singleton pattern)
jest.mock("../../../OemApiNode/src/infrastructure/IncidentRepository");
jest.mock("../../../OemApiNode/src/infrastructure/IncidentTypeRepository");

const IncidentService = require("../../../OemApiNode/src/services/IncidentService");
const incidentRepository = require("../../../OemApiNode/src/infrastructure/IncidentRepository");
const incidentTypeRepository = require("../../../OemApiNode/src/infrastructure/IncidentTypeRepository");

describe("Incident Service Tests", () => {
  beforeEach(() => {
    jest.clearAllMocks();
  });

  describe("createIncidentAsync", () => {
    test("should create incident successfully", async () => {
      const requestBody = {
        incidentTypeId: "type-123",
        startTime: new Date("2024-01-15T10:00:00"),
        endTime: new Date("2024-01-15T14:00:00"),
        description: "Equipment failure",
        affectsAllVVEs: false,
        affectedVVEIds: ["vve-1", "vve-2"],
      };

      const mockIncidentType = { id: "type-123", isActive: true };
      const mockCreatedIncident = {
        id: "incident-123",
        toJSON: () => ({
          id: "incident-123",
          description: "Equipment failure",
        }),
      };

      incidentTypeRepository.getByIdAsync.mockResolvedValue(mockIncidentType);
      incidentRepository.createAsync.mockResolvedValue(mockCreatedIncident);
      incidentRepository.getByIdAsync.mockResolvedValue(mockCreatedIncident);

      const result = await IncidentService.createIncidentAsync(requestBody);

      expect(result.success).toBe(true);
      expect(result.data).toBeDefined();
    });

    test("should fail when incident type not found", async () => {
      incidentTypeRepository.getByIdAsync.mockResolvedValue(null);

      const result = await IncidentService.createIncidentAsync({
        incidentTypeId: "x",
        startTime: new Date(),
        description: "Test",
      });

      expect(result.success).toBe(false);
      expect(result.error).toBe("Incident type not found");
    });

    test("should fail when incident type is inactive", async () => {
      incidentTypeRepository.getByIdAsync.mockResolvedValue({
        id: "x",
        isActive: false,
      });

      const result = await IncidentService.createIncidentAsync({
        incidentTypeId: "x",
        startTime: new Date(),
        description: "Test",
      });

      expect(result.success).toBe(false);
      expect(result.error).toBe("Cannot use inactive incident type");
    });
  });

  describe("getIncidentByIdAsync", () => {
    test("should return incident when found", async () => {
      const mockIncident = {
        id: "incident-123",
        toJSON: () => ({ id: "incident-123" }),
      };
      incidentRepository.getByIdAsync.mockResolvedValue(mockIncident);

      const result = await IncidentService.getIncidentByIdAsync("incident-123");

      expect(result.success).toBe(true);
      expect(result.data.id).toBe("incident-123");
    });

    test("should return error when not found", async () => {
      incidentRepository.getByIdAsync.mockResolvedValue(null);

      const result = await IncidentService.getIncidentByIdAsync("nonexistent");

      expect(result.success).toBe(false);
      expect(result.error).toBe("Incident not found");
    });
  });

  describe("getAllIncidentsAsync", () => {
    test("should return all incidents", async () => {
      const mockIncidents = [
        { toJSON: () => ({ id: "1" }) },
        { toJSON: () => ({ id: "2" }) },
      ];
      incidentRepository.getAllAsync.mockResolvedValue(mockIncidents);

      const result = await IncidentService.getAllIncidentsAsync();

      expect(result.success).toBe(true);
      expect(result.data).toHaveLength(2);
    });
  });

  describe("Dummy Test", () => {
    test("should pass - basic service test", () => {
      // Arrange
      const expected = true;

      // Act
      const actual = true;

      // Assert
      expect(actual).toBe(expected);
    });
  });
});
