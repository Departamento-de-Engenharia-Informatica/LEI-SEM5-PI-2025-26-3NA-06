jest.mock("../../../OemApiNode/src/infrastructure/VesselVisitExecutionRepository");

const VesselVisitExecutionService = require("../../../OemApiNode/src/services/VesselVisitExecutionService");
const vveRepository = require("../../../OemApiNode/src/infrastructure/VesselVisitExecutionRepository");

describe("VesselVisitExecution Service Tests", () => {
  beforeEach(() => {
    jest.clearAllMocks();
  });

  describe("createVVEAsync", () => {
    test("should create VVE successfully", async () => {
      const requestBody = {
        vvnId: "123e4567-e89b-12d3-a456-426614174000",
        operationPlanId: "223e4567-e89b-12d3-a456-426614174000",
        vveDate: "2024-01-15",
        plannedDockId: "323e4567-e89b-12d3-a456-426614174000",
        plannedArrivalTime: new Date("2024-01-15T10:00:00Z"),
        plannedDepartureTime: new Date("2024-01-15T14:00:00Z"),
      };

      const mockCreated = {
        id: "423e4567-e89b-12d3-a456-426614174000",
        ...requestBody,
        status: "NotStarted",
      };

      vveRepository.createAsync.mockResolvedValue(mockCreated);

      const result = await VesselVisitExecutionService.createVVEAsync(
        requestBody,
        "user-1",
        "testuser"
      );

      expect(result.success).toBe(true);
      expect(result.data).toBeDefined();
    });

    test("should validate required fields", async () => {
      const requestBody = {
        vvnId: "123e4567-e89b-12d3-a456-426614174000",
        // Missing required fields
      };

      const result = await VesselVisitExecutionService.createVVEAsync(
        requestBody,
        "user-1",
        "testuser"
      );

      expect(result.success).toBe(false);
      expect(result.error).toBeDefined();
    });
  });

  describe("getVVEByIdAsync", () => {
    test("should return VVE when found", async () => {
      const mockVVE = {
        id: "123e4567-e89b-12d3-a456-426614174000",
        vvnId: "223e4567-e89b-12d3-a456-426614174000",
        vveDate: "2024-01-15",
        status: "NotStarted",
      };

      vveRepository.getByIdAsync.mockResolvedValue(mockVVE);

      const result = await VesselVisitExecutionService.getVVEByIdAsync(
        "123e4567-e89b-12d3-a456-426614174000"
      );

      expect(result.success).toBe(true);
      expect(result.data).toBeDefined();
    });

    test("should return error when not found", async () => {
      vveRepository.getByIdAsync.mockResolvedValue(null);

      const result = await VesselVisitExecutionService.getVVEByIdAsync(
        "nonexistent"
      );

      expect(result.success).toBe(false);
      expect(result.error).toBe("VVE not found");
    });
  });

  // Note: Service only has getVVEByIdAsync, getVVEByVvnIdAsync, getVVEsByDateAsync
  // Methods like getAllVVEsAsync, updateVVEAsync, transitionStatusAsync don't exist
});
