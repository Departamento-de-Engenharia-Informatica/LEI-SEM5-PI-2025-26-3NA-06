// Mock the service before requiring the controller
jest.mock("../../../OemApiNode/src/services/OperationPlanService");
jest.mock("../../../OemApiNode/src/services/BackendApiClient");

const operationPlanController = require("../../../OemApiNode/src/controllers/operationPlanController");
const operationPlanService = require("../../../OemApiNode/src/services/OperationPlanService");

describe("OperationPlan Controller Tests", () => {
  let req, res, next;

  beforeEach(() => {
    req = {
      body: {},
      params: {},
      query: {},
      user: { sub: "user-123", name: "Test User" },
    };
    res = {
      status: jest.fn().mockReturnThis(),
      json: jest.fn(),
    };
    next = jest.fn();
    jest.clearAllMocks();
  });

  describe("create", () => {
    test("should return 201 when operation plan created successfully", async () => {
      const mockResult = {
        success: true,
        data: { id: "plan-123", planDate: "2024-01-15" },
      };

      operationPlanService.createOperationPlanAsync.mockResolvedValue(
        mockResult
      );

      req.body = { planDate: "2024-01-15", assignments: [] };

      await operationPlanController.create(req, res, next);

      expect(
        operationPlanService.createOperationPlanAsync
      ).toHaveBeenCalledWith(req.body, "user-123", "Test User");
      expect(res.status).toHaveBeenCalledWith(201);
      expect(res.json).toHaveBeenCalledWith(mockResult);
      expect(next).not.toHaveBeenCalled();
    });

    test("should return 400 when creation fails", async () => {
      const mockResult = {
        success: false,
        error: "Validation failed",
      };

      operationPlanService.createOperationPlanAsync.mockResolvedValue(
        mockResult
      );

      await operationPlanController.create(req, res, next);

      expect(res.status).toHaveBeenCalledWith(400);
      expect(res.json).toHaveBeenCalledWith(mockResult);
    });

    test("should call next with error when exception occurs", async () => {
      const error = new Error("Service error");
      operationPlanService.createOperationPlanAsync.mockRejectedValue(error);

      await operationPlanController.create(req, res, next);

      expect(next).toHaveBeenCalledWith(error);
    });
  });

  describe("replace", () => {
    test("should return 200 when operation plan replaced successfully", async () => {
      const mockResult = {
        success: true,
        data: { id: "plan-123", planDate: "2024-01-15" },
      };

      operationPlanService.replaceOperationPlanByDateAsync.mockResolvedValue(
        mockResult
      );

      req.body = { planDate: "2024-01-15", assignments: [] };

      await operationPlanController.replace(req, res, next);

      expect(res.status).toHaveBeenCalledWith(200);
      expect(res.json).toHaveBeenCalledWith(mockResult);
    });

    test("should return 400 when replacement fails", async () => {
      const mockResult = { success: false, error: "Not found" };

      operationPlanService.replaceOperationPlanByDateAsync.mockResolvedValue(
        mockResult
      );

      await operationPlanController.replace(req, res, next);

      expect(res.status).toHaveBeenCalledWith(400);
      expect(res.json).toHaveBeenCalledWith(mockResult);
    });
  });

  describe("search", () => {
    test("should return 200 with search results", async () => {
      const mockResult = {
        success: true,
        data: [{ id: "plan-1" }, { id: "plan-2" }],
      };

      operationPlanService.searchOperationPlansAsync.mockResolvedValue(
        mockResult
      );

      req.query = {
        startDate: "2024-01-01",
        endDate: "2024-01-31",
        skip: "0",
        take: "10",
      };

      await operationPlanController.search(req, res, next);

      expect(
        operationPlanService.searchOperationPlansAsync
      ).toHaveBeenCalledWith({
        startDate: "2024-01-01",
        endDate: "2024-01-31",
        vesselIMO: undefined,
        skip: 0,
        take: 10,
      });
      expect(res.status).toHaveBeenCalledWith(200);
      expect(res.json).toHaveBeenCalledWith(mockResult);
    });

    test("should use default skip and take values", async () => {
      const mockResult = { success: true, data: [] };

      operationPlanService.searchOperationPlansAsync.mockResolvedValue(
        mockResult
      );

      req.query = {};

      await operationPlanController.search(req, res, next);

      expect(
        operationPlanService.searchOperationPlansAsync
      ).toHaveBeenCalledWith({
        startDate: undefined,
        endDate: undefined,
        vesselIMO: undefined,
        skip: 0,
        take: 50,
      });
    });
  });

  describe("getById", () => {
    test("should return 200 when operation plan found", async () => {
      const mockResult = {
        success: true,
        data: { id: "plan-123", planDate: "2024-01-15" },
      };

      operationPlanService.getOperationPlanByIdAsync.mockResolvedValue(
        mockResult
      );

      req.params.id = "plan-123";

      await operationPlanController.getById(req, res, next);

      expect(
        operationPlanService.getOperationPlanByIdAsync
      ).toHaveBeenCalledWith("plan-123");
      expect(res.status).toHaveBeenCalledWith(200);
      expect(res.json).toHaveBeenCalledWith(mockResult);
    });

    test("should return 404 when operation plan not found", async () => {
      const mockResult = {
        success: false,
        error: "Operation plan not found",
      };

      operationPlanService.getOperationPlanByIdAsync.mockResolvedValue(
        mockResult
      );

      req.params.id = "nonexistent";

      await operationPlanController.getById(req, res, next);

      expect(res.status).toHaveBeenCalledWith(404);
      expect(res.json).toHaveBeenCalledWith(mockResult);
    });
  });

  describe("getByDate", () => {
    test("should return 200 when operation plan found by date", async () => {
      const mockResult = {
        success: true,
        data: { id: "plan-123", planDate: "2024-01-15" },
      };

      operationPlanService.getOperationPlanByDateAsync.mockResolvedValue(
        mockResult
      );

      req.params.date = "2024-01-15";

      await operationPlanController.getByDate(req, res, next);

      expect(
        operationPlanService.getOperationPlanByDateAsync
      ).toHaveBeenCalledWith("2024-01-15");
      expect(res.status).toHaveBeenCalledWith(200);
    });

    test("should return 404 when no plan for date", async () => {
      const mockResult = { success: false, error: "Not found" };

      operationPlanService.getOperationPlanByDateAsync.mockResolvedValue(
        mockResult
      );

      req.params.date = "2024-01-15";

      await operationPlanController.getByDate(req, res, next);

      expect(res.status).toHaveBeenCalledWith(404);
    });
  });

  describe("update", () => {
    test("should return 200 when update succeeds", async () => {
      const mockResult = {
        success: true,
        data: { id: "plan-123", status: "InProgress" },
      };

      operationPlanService.updateOperationPlanAsync.mockResolvedValue(
        mockResult
      );

      req.params.id = "plan-123";
      req.body = { status: "InProgress" };

      await operationPlanController.update(req, res, next);

      expect(
        operationPlanService.updateOperationPlanAsync
      ).toHaveBeenCalledWith("plan-123", req.body, "user-123");
      expect(res.status).toHaveBeenCalledWith(200);
    });

    test("should return 400 when update fails", async () => {
      const mockResult = { success: false, error: "Validation error" };

      operationPlanService.updateOperationPlanAsync.mockResolvedValue(
        mockResult
      );

      await operationPlanController.update(req, res, next);

      expect(res.status).toHaveBeenCalledWith(400);
    });
  });

  describe("updateStatus", () => {
    test("should return 200 when status updated", async () => {
      const mockResult = {
        success: true,
        data: { id: "plan-123", status: "Completed" },
      };

      operationPlanService.updateOperationPlanStatusAsync.mockResolvedValue(
        mockResult
      );

      req.params.id = "plan-123";
      req.body = { status: "Completed" };

      await operationPlanController.updateStatus(req, res, next);

      expect(res.status).toHaveBeenCalledWith(200);
    });
  });

  describe("delete", () => {
    test("should return 204 when delete succeeds", async () => {
      const mockResult = {
        success: true,
        message: "Operation plan deleted",
      };

      operationPlanService.deleteOperationPlanAsync.mockResolvedValue(
        mockResult
      );

      req.params.date = "2024-01-15";

      await operationPlanController.delete(req, res, next);

      expect(
        operationPlanService.deleteOperationPlanAsync
      ).toHaveBeenCalledWith("2024-01-15", "user-123");
      expect(res.status).toHaveBeenCalledWith(204);
    });

    test("should return 400 when delete fails", async () => {
      const mockResult = { success: false, error: "Cannot delete" };

      operationPlanService.deleteOperationPlanAsync.mockResolvedValue(
        mockResult
      );

      await operationPlanController.delete(req, res, next);

      expect(res.status).toHaveBeenCalledWith(400);
    });
  });

  describe("validateFeasibility", () => {
    test("should return 200 with feasibility results", async () => {
      const mockResult = {
        success: true,
        data: { isFeasible: true, warnings: [] },
      };

      operationPlanService.validateFeasibilityAsync.mockResolvedValue(
        mockResult
      );

      req.body = { planDate: "2024-01-15", assignments: [] };

      await operationPlanController.validateFeasibility(req, res, next);

      expect(
        operationPlanService.validateFeasibilityAsync
      ).toHaveBeenCalledWith(req.body);
      expect(res.status).toHaveBeenCalledWith(200);
    });
  });

  // Note: getCargoManifests calls backendApiClient directly, not operationPlanService
});
