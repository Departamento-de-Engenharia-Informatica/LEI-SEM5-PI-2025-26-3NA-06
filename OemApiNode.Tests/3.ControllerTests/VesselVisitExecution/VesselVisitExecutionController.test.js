// Mock the service before requiring the controller
jest.mock("../../../OemApiNode/src/services/VesselVisitExecutionService");

const vveController = require("../../../OemApiNode/src/controllers/vesselVisitExecutionController");
const vveService = require("../../../OemApiNode/src/services/VesselVisitExecutionService");

describe("VesselVisitExecution Controller Tests", () => {
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

  describe("prepareTodaysVVEs", () => {
    test("should return 201 when VVEs prepared successfully", async () => {
      const mockResult = {
        success: true,
        created: 5,
        skipped: 2,
        errors: 0,
      };

      vveService.prepareTodaysVVEsAsync.mockResolvedValue(mockResult);

      await vveController.prepareTodaysVVEs(req, res, next);

      expect(vveService.prepareTodaysVVEsAsync).toHaveBeenCalledWith(
        "user-123"
      );
      expect(res.status).toHaveBeenCalledWith(201);
      expect(res.json).toHaveBeenCalledWith(mockResult);
      expect(next).not.toHaveBeenCalled();
    });

    test("should return 400 when preparation fails", async () => {
      const mockResult = {
        success: false,
        error: "No operation plan found",
      };

      vveService.prepareTodaysVVEsAsync.mockResolvedValue(mockResult);

      await vveController.prepareTodaysVVEs(req, res, next);

      expect(res.status).toHaveBeenCalledWith(400);
      expect(res.json).toHaveBeenCalledWith(mockResult);
    });

    test("should call next with error when exception occurs", async () => {
      const error = new Error("Service error");
      vveService.prepareTodaysVVEsAsync.mockRejectedValue(error);

      await vveController.prepareTodaysVVEs(req, res, next);

      expect(next).toHaveBeenCalledWith(error);
    });
  });

  describe("create", () => {
    test("should return 201 when VVE created successfully", async () => {
      const mockVVE = {
        id: "vve-123",
        vvnId: "vvn-123",
        vveDate: "2024-01-15",
        status: "NotStarted",
        toJSON: () => ({
          id: "vve-123",
          vvnId: "vvn-123",
          vveDate: "2024-01-15",
        }),
      };

      const mockResult = {
        success: true,
        data: mockVVE,
        message: "VVE created",
      };

      vveService.createVVEAsync.mockResolvedValue(mockResult);

      req.body = {
        vvnId: "123e4567-e89b-12d3-a456-426614174000",
        operationPlanId: "223e4567-e89b-12d3-a456-426614174000",
        vveDate: "2024-01-15",
        plannedDockId: "323e4567-e89b-12d3-a456-426614174000",
        plannedArrivalTime: "2024-01-15T10:00:00Z",
        plannedDepartureTime: "2024-01-15T14:00:00Z",
      };

      await vveController.create(req, res, next);

      expect(res.status).toHaveBeenCalledWith(201);
      expect(res.json).toHaveBeenCalled();
    });

    test("should return 400 when creation fails", async () => {
      // This test triggers DTO validation error in controller
      req.body = { vvnId: "invalid" }; // Missing required fields

      await vveController.create(req, res, next);

      // DTO validation throws before service is called
      expect(next).toHaveBeenCalled();
    });
  });

  describe("getById", () => {
    test("should return 200 when VVE found", async () => {
      const mockVVE = {
        id: "vve-123",
        vvnId: "vvn-123",
        toJSON: () => ({ id: "vve-123", vvnId: "vvn-123" }),
      };

      const mockResult = {
        success: true,
        data: mockVVE,
      };

      vveService.getVVEByIdAsync.mockResolvedValue(mockResult);

      req.params.id = "vve-123";

      await vveController.getById(req, res, next);

      expect(vveService.getVVEByIdAsync).toHaveBeenCalledWith("vve-123");
      expect(res.status).toHaveBeenCalledWith(200);
      expect(res.json).toHaveBeenCalled();
    });

    test("should return 404 when VVE not found", async () => {
      const mockResult = {
        success: false,
        error: "VVE not found",
      };

      vveService.getVVEByIdAsync.mockResolvedValue(mockResult);

      req.params.id = "nonexistent";

      await vveController.getById(req, res, next);

      expect(res.status).toHaveBeenCalledWith(404);
      expect(res.json).toHaveBeenCalledWith(mockResult);
    });
  });

  describe("getByVvnId", () => {
    test("should return 200 when VVE found by VVN ID", async () => {
      const mockResult = {
        success: true,
        data: { id: "vve-123", vvnId: "vvn-123" },
      };

      vveService.getVVEByVvnIdAsync.mockResolvedValue(mockResult);

      req.params.vvnId = "vvn-123";

      await vveController.getByVvnId(req, res, next);

      expect(vveService.getVVEByVvnIdAsync).toHaveBeenCalledWith("vvn-123");
      expect(res.status).toHaveBeenCalledWith(200);
    });

    test("should return 404 when VVE not found by VVN ID", async () => {
      const mockResult = { success: false, error: "Not found" };

      vveService.getVVEByVvnIdAsync.mockResolvedValue(mockResult);

      await vveController.getByVvnId(req, res, next);

      expect(res.status).toHaveBeenCalledWith(404);
    });
  });

  describe("getByDate", () => {
    test("should return 200 with VVEs for date", async () => {
      const mockResult = {
        success: true,
        data: [
          { id: "vve-1", vveDate: "2024-01-15" },
          { id: "vve-2", vveDate: "2024-01-15" },
        ],
      };

      vveService.getVVEsByDateAsync.mockResolvedValue(mockResult);

      req.params.date = "2024-01-15";

      await vveController.getByDate(req, res, next);

      expect(vveService.getVVEsByDateAsync).toHaveBeenCalledWith("2024-01-15");
      expect(res.status).toHaveBeenCalledWith(200);
    });

    test("should return 400 when fetching by date fails", async () => {
      const mockResult = { success: false, error: "Invalid date" };

      vveService.getVVEsByDateAsync.mockResolvedValue(mockResult);

      await vveController.getByDate(req, res, next);

      expect(res.status).toHaveBeenCalledWith(400);
    });
  });

  describe("search", () => {
    test("should return 200 with search results", async () => {
      const mockResult = {
        success: true,
        data: [{ id: "vve-1" }, { id: "vve-2" }],
        total: 2,
      };

      vveService.searchVVEsAsync.mockResolvedValue(mockResult);

      req.query = {
        startDate: "2024-01-01",
        endDate: "2024-01-31",
        status: "InProgress",
        skip: "0",
        take: "10",
      };

      await vveController.search(req, res, next);

      expect(vveService.searchVVEsAsync).toHaveBeenCalledWith({
        startDate: "2024-01-01",
        endDate: "2024-01-31",
        status: "InProgress",
        vvnId: undefined,
        dockId: undefined,
        skip: 0,
        take: 10,
      });
      expect(res.status).toHaveBeenCalledWith(200);
    });

    test("should use default pagination values", async () => {
      const mockResult = { success: true, data: [] };

      vveService.searchVVEsAsync.mockResolvedValue(mockResult);

      req.query = {};

      await vveController.search(req, res, next);

      expect(vveService.searchVVEsAsync).toHaveBeenCalledWith({
        startDate: undefined,
        endDate: undefined,
        status: undefined,
        vvnId: undefined,
        dockId: undefined,
        skip: 0,
        take: 50,
      });
    });
  });

  describe("updateBerth", () => {
    test("should return 200 when berth updated successfully", async () => {
      const mockResult = {
        success: true,
        data: { id: "vve-123", actualDockId: "dock-456", toJSON: () => ({}) },
      };

      vveService.updateBerthAsync.mockResolvedValue(mockResult);

      req.params.id = "vve-123";
      req.body = {
        actualDockId: "dock-456",
        actualArrivalTime: "2024-01-15T10:00:00Z",
      };

      await vveController.updateBerth(req, res, next);

      expect(res.status).toHaveBeenCalledWith(200);
    });

    test("should call next when DTO validation fails", async () => {
      req.params.id = "vve-123";
      req.body = {}; // Missing required fields

      await vveController.updateBerth(req, res, next);

      // DTO validation throws error
      expect(next).toHaveBeenCalled();
    });
  });

  describe("updateStatus", () => {
    test("should return 200 when status updated", async () => {
      const mockResult = {
        success: true,
        data: { id: "vve-123", status: "InProgress", toJSON: () => ({}) },
      };

      vveService.updateStatusAsync.mockResolvedValue(mockResult);

      req.params.id = "vve-123";
      req.body = { status: "InProgress" };

      await vveController.updateStatus(req, res, next);

      expect(vveService.updateStatusAsync).toHaveBeenCalledWith(
        "vve-123",
        "InProgress",
        "user-123"
      );
      expect(res.status).toHaveBeenCalledWith(200);
    });

    test("should return 400 when status not provided", async () => {
      req.params.id = "vve-123";
      req.body = {}; // Missing status

      await vveController.updateStatus(req, res, next);

      expect(res.status).toHaveBeenCalledWith(400);
      expect(res.json).toHaveBeenCalledWith(
        expect.objectContaining({
          success: false,
          error: "Status is required in request body",
        })
      );
    });

    test("should return 400 when status update fails", async () => {
      const mockResult = { success: false, error: "Invalid transition" };

      vveService.updateStatusAsync.mockResolvedValue(mockResult);

      req.params.id = "vve-123";
      req.body = { status: "Completed" };

      await vveController.updateStatus(req, res, next);

      expect(res.status).toHaveBeenCalledWith(400);
    });
  });

  describe("delete", () => {
    test("should return 204 when VVE deleted", async () => {
      const mockResult = {
        success: true,
        message: "VVE deleted",
      };

      vveService.deleteVVEAsync.mockResolvedValue(mockResult);

      req.params.id = "vve-123";

      await vveController.delete(req, res, next);

      expect(vveService.deleteVVEAsync).toHaveBeenCalledWith(
        "vve-123",
        "user-123"
      );
      expect(res.status).toHaveBeenCalledWith(204);
    });

    test("should return 400 when delete fails", async () => {
      const mockResult = { success: false, error: "Cannot delete" };

      vveService.deleteVVEAsync.mockResolvedValue(mockResult);

      await vveController.delete(req, res, next);

      expect(res.status).toHaveBeenCalledWith(400);
    });
  });
});
