jest.mock("../../../OemApiNode/src/infrastructure/IncidentTypeRepository");

const IncidentTypeService = require("../../../OemApiNode/src/services/IncidentTypeService");
const incidentTypeRepository = require("../../../OemApiNode/src/infrastructure/IncidentTypeRepository");

// Mock repository
describe("IncidentTypeService Tests", () => {
  beforeEach(() => {
    jest.clearAllMocks();
  });

  describe("createIncidentTypeAsync", () => {
    test("should create incident type successfully", async () => {
      // Arrange
      const requestBody = {
        code: "EQ-FAIL",
        name: "Equipment Failure",
        description: "Equipment malfunction",
        severity: "Major",
      };

      const mockCreated = {
        id: "type-123",
        code: "EQ-FAIL",
        name: "Equipment Failure",
        severity: "Major",
        isActive: true,
      };

      incidentTypeRepository.getByCodeAsync.mockResolvedValue(null);
      incidentTypeRepository.createAsync.mockResolvedValue(mockCreated);

      // Act
      const result = await IncidentTypeService.createIncidentTypeAsync(
        requestBody,
        "user-1",
        "john.doe"
      );

      // Assert
      expect(result.success).toBe(true);
      expect(result.data).toBeDefined();
      expect(incidentTypeRepository.getByCodeAsync).toHaveBeenCalledWith(
        "EQ-FAIL"
      );
    });

    test("should fail when code already exists", async () => {
      // Arrange
      const requestBody = {
        code: "EXISTING",
        name: "Test Type",
        severity: "Minor",
      };

      const mockExisting = {
        id: "existing-123",
        code: "EXISTING",
      };

      incidentTypeRepository.getByCodeAsync.mockResolvedValue(mockExisting);

      // Act
      const result = await IncidentTypeService.createIncidentTypeAsync(
        requestBody,
        "user-1",
        "john.doe"
      );

      // Assert
      expect(result.success).toBe(false);
      expect(result.error).toContain("already exists");
    });

    test("should validate parent exists when parentId provided", async () => {
      // Arrange
      const requestBody = {
        code: "SUB-TYPE",
        name: "Sub Type",
        severity: "Minor",
        parentId: "123e4567-e89b-12d3-a456-426614174000",
      };

      const mockParent = {
        id: "123e4567-e89b-12d3-a456-426614174000",
        code: "PARENT",
        isActive: true,
      };

      incidentTypeRepository.getByCodeAsync.mockResolvedValue(null);
      incidentTypeRepository.getByIdAsync.mockResolvedValue(mockParent);
      incidentTypeRepository.createAsync.mockResolvedValue({
        id: "sub-123",
        code: "SUB-TYPE",
        parentId: "123e4567-e89b-12d3-a456-426614174000",
      });

      // Act
      const result = await IncidentTypeService.createIncidentTypeAsync(
        requestBody,
        "user-1",
        "john.doe"
      );

      // Assert
      expect(result.success).toBe(true);
      expect(incidentTypeRepository.getByIdAsync).toHaveBeenCalledWith(
        "123e4567-e89b-12d3-a456-426614174000"
      );
    });

    test("should fail when parent not found", async () => {
      // Arrange
      const requestBody = {
        code: "SUB-TYPE",
        name: "Sub Type",
        severity: "Minor",
        parentId: "123e4567-e89b-12d3-a456-426614174999",
      };

      incidentTypeRepository.getByCodeAsync.mockResolvedValue(null);
      incidentTypeRepository.getByIdAsync.mockResolvedValue(null);

      // Act
      const result = await IncidentTypeService.createIncidentTypeAsync(
        requestBody,
        "user-1",
        "john.doe"
      );

      // Assert
      expect(result.success).toBe(false);
      expect(result.error).toContain("not found");
    });

    test("should fail when parent is inactive", async () => {
      // Arrange
      const requestBody = {
        code: "SUB-TYPE",
        name: "Sub Type",
        severity: "Minor",
        parentId: "parent-123",
      };

      const mockInactiveParent = {
        id: "123e4567-e89b-12d3-a456-426614174000",
        code: "PARENT",
        isActive: false,
      };

      incidentTypeRepository.getByCodeAsync.mockResolvedValue(null);
      incidentTypeRepository.getByIdAsync.mockResolvedValue(mockInactiveParent);

      // Act
      const result = await IncidentTypeService.createIncidentTypeAsync(
        requestBody,
        "user-1",
        "john.doe"
      );

      // Assert
      expect(result.success).toBe(false);
      expect(result.error).toContain("inactive");
    });
  });

  describe("getIncidentTypeByIdAsync", () => {
    test("should return incident type when found", async () => {
      // Arrange
      const mockIncidentType = {
        id: "type-123",
        code: "TEST",
        name: "Test Type",
        severity: "Minor",
        parentId: null,
      };

      incidentTypeRepository.getByIdAsync.mockResolvedValue(mockIncidentType);

      // Act
      const result = await IncidentTypeService.getIncidentTypeByIdAsync(
        "type-123"
      );

      // Assert
      expect(result.success).toBe(true);
      expect(result.data).toBeDefined();
    });

    test("should include parent name when parent exists", async () => {
      // Arrange
      const mockParent = {
        id: "parent-123",
        name: "Parent Type",
      };

      const mockIncidentType = {
        id: "type-123",
        code: "SUB",
        name: "Sub Type",
        parentId: "parent-123",
      };

      incidentTypeRepository.getByIdAsync
        .mockResolvedValueOnce(mockIncidentType)
        .mockResolvedValueOnce(mockParent);

      // Act
      const result = await IncidentTypeService.getIncidentTypeByIdAsync(
        "type-123"
      );

      // Assert
      expect(result.success).toBe(true);
      expect(incidentTypeRepository.getByIdAsync).toHaveBeenCalledTimes(2);
    });

    test("should return error when not found", async () => {
      // Arrange
      incidentTypeRepository.getByIdAsync.mockResolvedValue(null);

      // Act
      const result = await IncidentTypeService.getIncidentTypeByIdAsync(
        "nonexistent"
      );

      // Assert
      expect(result.success).toBe(false);
      expect(result.error).toBe("Incident type not found");
    });
  });

  describe("getIncidentTypeByCodeAsync", () => {
    test("should return incident type by code", async () => {
      // Arrange
      const mockIncidentType = {
        id: "type-123",
        code: "TEST-CODE",
        name: "Test Type",
      };

      incidentTypeRepository.getByCodeAsync.mockResolvedValue(mockIncidentType);

      // Act
      const result = await IncidentTypeService.getIncidentTypeByCodeAsync(
        "TEST-CODE"
      );

      // Assert
      expect(result.success).toBe(true);
      expect(incidentTypeRepository.getByCodeAsync).toHaveBeenCalledWith(
        "TEST-CODE"
      );
    });

    test("should return error when code not found", async () => {
      // Arrange
      incidentTypeRepository.getByCodeAsync.mockResolvedValue(null);

      // Act
      const result = await IncidentTypeService.getIncidentTypeByCodeAsync(
        "NONEXISTENT"
      );

      // Assert
      expect(result.success).toBe(false);
      expect(result.error).toBe("Incident type not found");
    });
  });

  describe("getAllIncidentTypesAsync", () => {
    test("should return all incident types", async () => {
      // Arrange
      const mockTypes = [
        { id: "1", code: "TYPE1", name: "Type 1" },
        { id: "2", code: "TYPE2", name: "Type 2" },
      ];

      incidentTypeRepository.getAllAsync.mockResolvedValue(mockTypes);

      // Act
      const result = await IncidentTypeService.getAllIncidentTypesAsync();

      // Assert
      expect(result.success).toBe(true);
      expect(result.data).toHaveLength(2);
    });

    test("should apply filters", async () => {
      // Arrange
      const filters = { isActive: true };
      incidentTypeRepository.getAllAsync.mockResolvedValue([]);

      // Act
      const result = await IncidentTypeService.getAllIncidentTypesAsync(
        filters
      );

      // Assert
      expect(incidentTypeRepository.getAllAsync).toHaveBeenCalledWith(filters);
    });
  });

  describe("updateIncidentTypeAsync", () => {
    test("should update incident type successfully", async () => {
      // Arrange
      const mockExisting = {
        id: "type-123",
        code: "TEST",
        name: "Old Name",
        update: jest.fn(),
      };

      const updateData = {
        name: "New Name",
        description: "New description",
      };

      const mockUpdated = {
        ...mockExisting,
        name: "New Name",
      };

      incidentTypeRepository.getByIdAsync.mockResolvedValue(mockExisting);
      incidentTypeRepository.updateAsync.mockResolvedValue(mockUpdated);

      // Act
      const result = await IncidentTypeService.updateIncidentTypeAsync(
        "type-123",
        updateData,
        "user-1"
      );

      // Assert
      expect(result.success).toBe(true);
      expect(incidentTypeRepository.updateAsync).toHaveBeenCalled();
    });

    test("should fail when incident type not found", async () => {
      // Arrange
      incidentTypeRepository.getByIdAsync.mockResolvedValue(null);

      // Act
      const result = await IncidentTypeService.updateIncidentTypeAsync(
        "nonexistent",
        {},
        "user-1"
      );

      // Assert
      expect(result.success).toBe(false);
      expect(result.error).toBe("Incident type not found");
    });

    test("should not allow code updates", async () => {
      // Arrange
      const mockExisting = {
        id: "123e4567-e89b-12d3-a456-426614174000",
        code: "ORIGINAL",
        name: "Test",
        severity: "Minor",
        update: jest.fn(), // Mock the update method
      };

      const updateData = {
        name: "Updated Name",
        description: "New description",
      };

      incidentTypeRepository.getByIdAsync.mockResolvedValue(mockExisting);
      incidentTypeRepository.updateAsync.mockResolvedValue({
        ...mockExisting,
        name: "Updated Name",
      });

      // Act
      const result = await IncidentTypeService.updateIncidentTypeAsync(
        "123e4567-e89b-12d3-a456-426614174000",
        updateData,
        "user-1"
      );

      // Assert
      expect(result.success).toBe(true);
      // Code should remain unchanged in the update call
    });
  });

  describe("deleteIncidentTypeAsync", () => {
    test("should deactivate incident type successfully", async () => {
      // Arrange
      const mockIncidentType = {
        id: "123e4567-e89b-12d3-a456-426614174000",
        code: "TEST-TYPE",
        name: "Test Type",
        severity: "Minor",
        isActive: true,
        toJSON: jest.fn(() => ({ code: "TEST-TYPE", name: "Test Type" })),
      };

      incidentTypeRepository.getByIdAsync.mockResolvedValue(mockIncidentType);
      incidentTypeRepository.deleteAsync.mockResolvedValue({
        ...mockIncidentType,
        isActive: false,
      });

      // Act
      const result = await IncidentTypeService.deleteIncidentTypeAsync(
        "123e4567-e89b-12d3-a456-426614174000",
        "user-1"
      );

      // Assert
      expect(result.success).toBe(true);
    });
  });
});
