const { TYPES } = require("tedious");
const database = require("../config/database");
const logger = require("../utils/logger");
const IncidentType = require("../domain/IncidentType/IncidentType");

/**
 * Repository for Incident Type CRUD operations
 */
class IncidentTypeRepository {
  /**
   * Create incident types table if not exists
   */
  async ensureTableExists() {
    const createTableSQL = `
      IF NOT EXISTS (SELECT * FROM sys.tables WHERE name = 'IncidentTypes')
      BEGIN
        CREATE TABLE IncidentTypes (
          Id UNIQUEIDENTIFIER PRIMARY KEY,
          Code NVARCHAR(10) NOT NULL,
          Name NVARCHAR(255) NOT NULL,
          Description NVARCHAR(MAX),
          Severity NVARCHAR(50) NOT NULL,
          ParentId UNIQUEIDENTIFIER NULL,
          IsActive BIT NOT NULL DEFAULT 1,
          CreatedAt DATETIME2 NOT NULL DEFAULT GETUTCDATE(),
          UpdatedAt DATETIME2 NULL,
          CONSTRAINT UQ_IncidentType_Code UNIQUE (Code),
          CONSTRAINT FK_IncidentType_Parent FOREIGN KEY (ParentId) 
            REFERENCES IncidentTypes(Id),
          CONSTRAINT CK_IncidentType_Severity 
            CHECK (Severity IN ('Minor', 'Major', 'Critical'))
        );
        CREATE INDEX IX_IncidentType_Code ON IncidentTypes(Code);
        CREATE INDEX IX_IncidentType_ParentId ON IncidentTypes(ParentId);
        CREATE INDEX IX_IncidentType_IsActive ON IncidentTypes(IsActive);
        CREATE INDEX IX_IncidentType_Severity ON IncidentTypes(Severity);
      END
    `;

    try {
      await database.executeQuery(createTableSQL);
      logger.info("IncidentTypes table ready");
    } catch (error) {
      logger.error("Failed to ensure IncidentTypes table exists:", error);
      throw error;
    }
  }

  /**
   * Create a new incident type
   */
  async createAsync(incidentType) {
    const query = `
      INSERT INTO IncidentTypes 
        (Id, Code, Name, Description, Severity, ParentId, IsActive, CreatedAt, UpdatedAt)
      VALUES 
        (@id, @code, @name, @description, @severity, @parentId, @isActive, @createdAt, @updatedAt);
      
      SELECT * FROM IncidentTypes WHERE Id = @id;
    `;

    const dbData = incidentType.toDatabase();

    const params = [
      { name: "id", type: TYPES.UniqueIdentifier, value: dbData.Id },
      { name: "code", type: TYPES.NVarChar, value: dbData.Code },
      { name: "name", type: TYPES.NVarChar, value: dbData.Name },
      {
        name: "description",
        type: TYPES.NVarChar,
        value: dbData.Description,
      },
      { name: "severity", type: TYPES.NVarChar, value: dbData.Severity },
      {
        name: "parentId",
        type: TYPES.UniqueIdentifier,
        value: dbData.ParentId,
      },
      { name: "isActive", type: TYPES.Bit, value: dbData.IsActive },
      { name: "createdAt", type: TYPES.DateTime2, value: dbData.CreatedAt },
      { name: "updatedAt", type: TYPES.DateTime2, value: dbData.UpdatedAt },
    ];

    try {
      const results = await database.executeQuery(query, params);
      if (results.length > 0) {
        logger.info(`Incident type created: ${dbData.Code}`);
        return IncidentType.fromDatabase(results[0]);
      }
      throw new Error("Failed to create incident type");
    } catch (error) {
      const errorMsg = error.message || error.toString();
      if (
        errorMsg.includes("unique") ||
        errorMsg.includes("duplicate") ||
        errorMsg.includes("UNIQUE")
      ) {
        throw new Error(
          `Incident type with code '${dbData.Code}' already exists`
        );
      }
      logger.error("Error creating incident type:", error);
      throw error;
    }
  }

  /**
   * Get incident type by ID
   */
  async getByIdAsync(id) {
    const query = "SELECT * FROM IncidentTypes WHERE Id = @id";
    const params = [{ name: "id", type: TYPES.UniqueIdentifier, value: id }];

    try {
      const results = await database.executeQuery(query, params);
      if (results.length === 0) {
        return null;
      }
      return IncidentType.fromDatabase(results[0]);
    } catch (error) {
      logger.error(`Error fetching incident type by ID ${id}:`, error);
      throw error;
    }
  }

  /**
   * Get incident type by code
   */
  async getByCodeAsync(code) {
    const query = "SELECT * FROM IncidentTypes WHERE Code = @code";
    const params = [{ name: "code", type: TYPES.NVarChar, value: code }];

    try {
      const results = await database.executeQuery(query, params);
      if (results.length === 0) {
        return null;
      }
      return IncidentType.fromDatabase(results[0]);
    } catch (error) {
      logger.error(`Error fetching incident type by code ${code}:`, error);
      throw error;
    }
  }

  /**
   * Get all incident types with filtering options
   * Includes parent name via LEFT JOIN
   * @param {boolean|string} filter - false: active only, true: all, 'inactive': inactive only
   */
  async getAllAsync(filter = false) {
    let whereClause = "";

    if (filter === false) {
      // Active only
      whereClause = "WHERE it.IsActive = 1";
    } else if (filter === "inactive") {
      // Inactive only
      whereClause = "WHERE it.IsActive = 0";
    }
    // else if filter === true: no WHERE clause, get all

    const query = `SELECT 
        it.*, 
        parent.Name as ParentName
       FROM IncidentTypes it
       LEFT JOIN IncidentTypes parent ON it.ParentId = parent.Id
       ${whereClause}
       ORDER BY it.Code`;

    try {
      const results = await database.executeQuery(query);
      return results.map((row) => {
        const incidentType = IncidentType.fromDatabase(row);
        // Add parentName as additional property
        incidentType.parentName = row.ParentName || null;
        return incidentType;
      });
    } catch (error) {
      logger.error("Error fetching all incident types:", error);
      throw error;
    }
  }

  /**
   * Get children of a specific parent (for hierarchy)
   */
  async getChildrenAsync(parentId) {
    const query = `
      SELECT * FROM IncidentTypes 
      WHERE ParentId = @parentId AND IsActive = 1 
      ORDER BY Code
    `;
    const params = [
      { name: "parentId", type: TYPES.UniqueIdentifier, value: parentId },
    ];

    try {
      const results = await database.executeQuery(query, params);
      return results.map((row) => IncidentType.fromDatabase(row));
    } catch (error) {
      logger.error(`Error fetching children for parent ${parentId}:`, error);
      throw error;
    }
  }

  /**
   * Get root level incident types (no parent)
   */
  async getRootTypesAsync() {
    const query = `
      SELECT * FROM IncidentTypes 
      WHERE ParentId IS NULL AND IsActive = 1 
      ORDER BY Code
    `;

    try {
      const results = await database.executeQuery(query);
      return results.map((row) => IncidentType.fromDatabase(row));
    } catch (error) {
      logger.error("Error fetching root incident types:", error);
      throw error;
    }
  }

  /**
   * Update incident type (code is immutable)
   */
  async updateAsync(incidentType) {
    const query = `
      UPDATE IncidentTypes 
      SET 
        Name = @name,
        Description = @description,
        Severity = @severity,
        ParentId = @parentId,
        IsActive = @isActive,
        UpdatedAt = @updatedAt
      WHERE Id = @id;
      
      SELECT * FROM IncidentTypes WHERE Id = @id;
    `;

    const dbData = incidentType.toDatabase();

    const params = [
      { name: "id", type: TYPES.UniqueIdentifier, value: dbData.Id },
      { name: "name", type: TYPES.NVarChar, value: dbData.Name },
      {
        name: "description",
        type: TYPES.NVarChar,
        value: dbData.Description,
      },
      { name: "severity", type: TYPES.NVarChar, value: dbData.Severity },
      {
        name: "parentId",
        type: TYPES.UniqueIdentifier,
        value: dbData.ParentId,
      },
      { name: "isActive", type: TYPES.Bit, value: dbData.IsActive },
      { name: "updatedAt", type: TYPES.DateTime2, value: new Date() },
    ];

    try {
      const results = await database.executeQuery(query, params);
      if (results.length > 0) {
        logger.info(`Incident type updated: ${dbData.Code}`);
        return IncidentType.fromDatabase(results[0]);
      }
      return null;
    } catch (error) {
      logger.error(`Error updating incident type ${dbData.Id}:`, error);
      throw error;
    }
  }

  /**
   * Soft delete incident type (set IsActive = false)
   */
  async deleteAsync(id) {
    const query = `
      UPDATE IncidentTypes 
      SET IsActive = 0, UpdatedAt = @updatedAt 
      WHERE Id = @id;
      
      SELECT * FROM IncidentTypes WHERE Id = @id;
    `;

    const params = [
      { name: "id", type: TYPES.UniqueIdentifier, value: id },
      { name: "updatedAt", type: TYPES.DateTime2, value: new Date() },
    ];

    try {
      const results = await database.executeQuery(query, params);
      if (results.length > 0) {
        logger.info(`Incident type soft deleted: ${id}`);
        return IncidentType.fromDatabase(results[0]);
      }
      return null;
    } catch (error) {
      logger.error(`Error deleting incident type ${id}:`, error);
      throw error;
    }
  }

  /**
   * Check if incident type has children (for deletion validation)
   */
  async hasChildrenAsync(id) {
    const query = `
      SELECT COUNT(*) as Count 
      FROM IncidentTypes 
      WHERE ParentId = @id AND IsActive = 1
    `;
    const params = [{ name: "id", type: TYPES.UniqueIdentifier, value: id }];

    try {
      const results = await database.executeQuery(query, params);
      return results[0].Count > 0;
    } catch (error) {
      logger.error(`Error checking children for incident type ${id}:`, error);
      throw error;
    }
  }

  /**
   * Check if parent-child relationship would create a circular reference
   */
  async wouldCreateCircularReference(childId, parentId) {
    if (!parentId) return false;

    // Get all ancestors of the potential parent
    const query = `
      WITH IncidentTypeHierarchy AS (
        SELECT Id, ParentId, 0 as Level
        FROM IncidentTypes
        WHERE Id = @parentId
        
        UNION ALL
        
        SELECT it.Id, it.ParentId, h.Level + 1
        FROM IncidentTypes it
        INNER JOIN IncidentTypeHierarchy h ON it.Id = h.ParentId
        WHERE h.Level < 10  -- Prevent infinite loops
      )
      SELECT Id FROM IncidentTypeHierarchy WHERE Id = @childId
    `;

    const params = [
      { name: "parentId", type: TYPES.UniqueIdentifier, value: parentId },
      { name: "childId", type: TYPES.UniqueIdentifier, value: childId },
    ];

    try {
      const results = await database.executeQuery(query, params);
      // If childId is found in parent's ancestors, it would create a circle
      return results.length > 0;
    } catch (error) {
      logger.error("Error checking circular reference:", error);
      throw error;
    }
  }
}

module.exports = new IncidentTypeRepository();
