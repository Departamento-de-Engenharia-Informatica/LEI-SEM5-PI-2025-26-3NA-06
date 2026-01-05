const fs = require("fs");
const path = require("path");

// Fix OperationPlanService tests
const opTestPath =
  "./2.ServiceTests/OperationPlan/OperationPlanService.test.js";
let opContent = fs.readFileSync(opTestPath, "utf8");

// Replace the entire file with fixed version
opContent = `jest.mock("../../../OemApiNode/src/infrastructure/OperationPlanRepository");

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
`;

fs.writeFileSync(opTestPath, opContent, "utf8");
console.log("Fixed OperationPlanService.test.js");

// Fix VesselVisitExecutionService tests
const vveTestPath =
  "./2.ServiceTests/VesselVisitExecution/VesselVisitExecutionService.test.js";
let vveContent = fs.readFileSync(vveTestPath, "utf8");

vveContent = `jest.mock("../../../OemApiNode/src/infrastructure/VesselVisitExecutionRepository");

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
`;

fs.writeFileSync(vveTestPath, vveContent, "utf8");
console.log("Fixed VesselVisitExecutionService.test.js");

// Fix IncidentTypeService remaining issues
const itTestPath = "./2.ServiceTests/IncidentType/IncidentTypeService.test.js";
let itContent = fs.readFileSync(itTestPath, "utf8");

// Fix the createAsync mock to return proper parent ID
itContent = itContent.replace(
  /id: "sub-123",\s+code: "SUB-TYPE",\s+parentId: "parent-123",/g,
  'id: "sub-123",\n        code: "SUB-TYPE",\n        parentId: "123e4567-e89b-12d3-a456-426614174000",'
);

// Fix the assertion
itContent = itContent.replace(
  /expect\(incidentTypeRepository\.getByIdAsync\)\.toHaveBeenCalledWith\(\s*"parent-123"\s*\);/g,
  'expect(incidentTypeRepository.getByIdAsync).toHaveBeenCalledWith("123e4567-e89b-12d3-a456-426614174000");'
);

// Fix parent not found test
itContent = itContent.replace(
  /parentId: "nonexistent"/g,
  'parentId: "123e4567-e89b-12d3-a456-426614174999"'
);

// Fix parent inactive test
itContent = itContent.replace(
  /id: "parent-123",\s+isActive: false/g,
  'id: "123e4567-e89b-12d3-a456-426614174000",\n        code: "PARENT",\n        isActive: false'
);

fs.writeFileSync(itTestPath, itContent, "utf8");
console.log("Fixed IncidentTypeService.test.js");

console.log("All test files fixed!");
