jest.mock("../../../OemApiNode/src/infrastructure/OperationPlanRepository");

const OperationPlanService = require("../../../OemApiNode/src/services/OperationPlanService");
const operationPlanRepository = require("../../../OemApiNode/src/infrastructure/OperationPlanRepository");

describe("OperationPlan Service Tests", () => {
  beforeEach(() => {
    jest.clearAllMocks();
  });

  describe("createOperationPlanAsync", () => {
    test("should create operation plan successfully", async () => {
      const eta = new Date("2024-01-15T10:00:00Z");
      const etd = new Date("2024-01-15T14:00:00Z");
      const requestBody = {
        planDate: "2024-01-15",
        assignments: [
          {
            vvnId: "123e4567-e89b-12d3-a456-426614174000",
            dockId: "223e4567-e89b-12d3-a456-426614174000",
            eta,
            etd,
          },
        ],
        algorithm: "FIFO",
      };

      const mockCreated = {
        id: "323e4567-e89b-12d3-a456-426614174000",
        planDate: "2024-01-15",
        isFeasible: true,
        warnings: [],
        assignments: requestBody.assignments,
      };

      operationPlanRepository.getByDateAsync.mockResolvedValue(null);
      operationPlanRepository.createAsync.mockResolvedValue(mockCreated);

      const result = await OperationPlanService.createOperationPlanAsync(
        requestBody,
        "user-1"
      );

      expect(result.success).toBe(true);
      expect(result.data).toBeDefined();
    });

    test("should detect feasibility conflicts", async () => {
      const requestBody = {
        planDate: "2024-01-15",
        assignments: [
          {
            vvnId: "123e4567-e89b-12d3-a456-426614174000",
            dockId: "323e4567-e89b-12d3-a456-426614174000",
            eta: new Date("2024-01-15T10:00:00Z"),
            etd: new Date("2024-01-15T14:00:00Z"),
          },
          {
            vvnId: "223e4567-e89b-12d3-a456-426614174000",
            dockId: "323e4567-e89b-12d3-a456-426614174000",
            eta: new Date("2024-01-15T12:00:00Z"),
            etd: new Date("2024-01-15T16:00:00Z"),
          },
        ],
      };

      const mockCreated = {
        id: "423e4567-e89b-12d3-a456-426614174000",
        planDate: "2024-01-15",
        isFeasible: false,
        warnings: ["Dock conflict detected"],
        assignments: requestBody.assignments,
      };

      operationPlanRepository.getByDateAsync.mockResolvedValue(null);
      operationPlanRepository.createAsync.mockResolvedValue(mockCreated);

      const result = await OperationPlanService.createOperationPlanAsync(
        requestBody,
        "user-1"
      );

      expect(result.success).toBe(true);
      expect(result.data.isFeasible).toBe(false);
    });
  });

  describe("getOperationPlanByIdAsync", () => {
    test("should return plan when found", async () => {
      const mockPlan = {
        id: "123e4567-e89b-12d3-a456-426614174000",
        planDate: "2024-01-15",
        isFeasible: true,
        warnings: [],
        assignments: [],
      };

      operationPlanRepository.getByIdAsync.mockResolvedValue(mockPlan);

      const result = await OperationPlanService.getOperationPlanByIdAsync(
        "123e4567-e89b-12d3-a456-426614174000"
      );

      expect(result.success).toBe(true);
      expect(result.data).toBeDefined();
    });

    test("should return error when not found", async () => {
      operationPlanRepository.getByIdAsync.mockResolvedValue(null);

      const result = await OperationPlanService.getOperationPlanByIdAsync(
        "nonexistent"
      );

      expect(result.success).toBe(false);
      expect(result.error).toBe("Operation plan not found");
    });
  });

  describe("getOperationPlanByDateAsync", () => {
    test("should return plan for specific date", async () => {
      const mockPlan = {
        id: "123e4567-e89b-12d3-a456-426614174000",
        planDate: "2024-01-15",
        isFeasible: true,
        warnings: [],
        assignments: [],
      };

      operationPlanRepository.getByDateAsync.mockResolvedValue(mockPlan);

      const result = await OperationPlanService.getOperationPlanByDateAsync(
        "2024-01-15"
      );

      expect(result.success).toBe(true);
      expect(result.data.planDate).toBe("2024-01-15");
    });
  });

  // Note: updateOperationPlanAsync exists but requires complex DTO validation
  describe("updateOperationPlanAsync", () => {
    test("should update plan successfully", async () => {
      const mockExisting = {
        id: "123e4567-e89b-12d3-a456-426614174000",
        planDate: "2024-01-15",
        status: "NotStarted",
        isFeasible: true,
        warnings: [],
        assignments: [],
        validate: jest.fn(), // Mock the validate method
        checkFeasibility: jest.fn(), // Mock checkFeasibility method
      };

      const mockUpdated = {
        ...mockExisting,
        status: "InProgress",
      };

      operationPlanRepository.getByIdAsync.mockResolvedValue(mockExisting);
      operationPlanRepository.updateAsync.mockResolvedValue(mockUpdated);

      const result = await OperationPlanService.updateOperationPlanAsync(
        "123e4567-e89b-12d3-a456-426614174000",
        { planDate: "2024-01-15", assignments: [], status: "InProgress" }
      );

      expect(result.success).toBe(true);
    });
  });
});
