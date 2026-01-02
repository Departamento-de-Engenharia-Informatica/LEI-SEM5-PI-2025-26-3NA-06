const database = require("../config/database");
const { TYPES } = require("tedious");
const VesselVisitExecution = require("../domain/VesselVisitExecution/VesselVisitExecution");
const logger = require("../utils/logger");

/**
 * Parse and validate date for SQL Server
 */
function parseAndValidateDate(dateValue) {
  if (!dateValue) {
    throw new Error("Date value is required");
  }

  let date;
  if (typeof dateValue === "string") {
    date = new Date(dateValue);
  } else if (dateValue instanceof Date) {
    date = dateValue;
  } else {
    throw new Error(`Invalid date type: ${typeof dateValue}`);
  }

  if (isNaN(date.getTime())) {
    throw new Error(`Invalid date value: ${dateValue}`);
  }

  return date;
}

/**
 * Repository for Vessel Visit Execution (VVE)
 */
class VesselVisitExecutionRepository {
  /**
   * Create VVE table if not exists
   */
  async ensureTableExists() {
    const createTableSQL = `
      IF NOT EXISTS (SELECT * FROM sys.tables WHERE name = 'VesselVisitExecutions')
      BEGIN
        CREATE TABLE VesselVisitExecutions (
          Id UNIQUEIDENTIFIER PRIMARY KEY,
          VvnId UNIQUEIDENTIFIER NOT NULL,
          OperationPlanId UNIQUEIDENTIFIER NULL,
          VVEDate DATE NOT NULL,
          PlannedDockId UNIQUEIDENTIFIER NOT NULL,
          PlannedArrivalTime DATETIME2 NOT NULL,
          PlannedDepartureTime DATETIME2 NOT NULL,
          ActualDockId UNIQUEIDENTIFIER NULL,
          ActualArrivalTime DATETIME2 NULL,
          ActualDepartureTime DATETIME2 NULL,
          Status NVARCHAR(50) NOT NULL DEFAULT 'NotStarted',
          CreatedAt DATETIME2 NOT NULL DEFAULT GETUTCDATE(),
          UpdatedAt DATETIME2 NULL,
          CONSTRAINT UQ_VVE_VvnId UNIQUE (VvnId)
        );
        CREATE INDEX IX_VVE_VVEDate ON VesselVisitExecutions(VVEDate);
        CREATE INDEX IX_VVE_Status ON VesselVisitExecutions(Status);
        CREATE INDEX IX_VVE_VvnId ON VesselVisitExecutions(VvnId);
        CREATE INDEX IX_VVE_OperationPlanId ON VesselVisitExecutions(OperationPlanId);
      END
    `;

    // Migration: Add missing columns if table already exists
    const migrationSQL = `
      IF EXISTS (SELECT * FROM sys.tables WHERE name = 'VesselVisitExecutions')
      BEGIN
        -- Add Status column if it doesn't exist
        IF NOT EXISTS (SELECT * FROM sys.columns WHERE object_id = OBJECT_ID('VesselVisitExecutions') AND name = 'Status')
        BEGIN
          ALTER TABLE VesselVisitExecutions ADD Status NVARCHAR(50) NOT NULL DEFAULT 'NotStarted';
          CREATE INDEX IX_VVE_Status ON VesselVisitExecutions(Status);
        END

        -- Add UpdatedAt column if it doesn't exist
        IF NOT EXISTS (SELECT * FROM sys.columns WHERE object_id = OBJECT_ID('VesselVisitExecutions') AND name = 'UpdatedAt')
        BEGIN
          ALTER TABLE VesselVisitExecutions ADD UpdatedAt DATETIME2 NULL;
        END
      END
    `;

    try {
      await database.executeQuery(createTableSQL);
      await database.executeQuery(migrationSQL);
      logger.info("VesselVisitExecutions table ready (schema ensured)");
    } catch (error) {
      logger.error(
        "Failed to ensure VesselVisitExecutions table exists:",
        error
      );
      throw error;
    }
  }

  /**
   * Create a new VVE
   */
  async createAsync(vve) {
    const query = `
      INSERT INTO VesselVisitExecutions 
        (Id, VvnId, OperationPlanId, VVEDate, PlannedDockId, PlannedArrivalTime, PlannedDepartureTime,
         ActualDockId, ActualArrivalTime, ActualDepartureTime, Status, CreatedAt, UpdatedAt)
      VALUES 
        (@id, @vvnId, @operationPlanId, @vveDate, @plannedDockId, @plannedArrivalTime, @plannedDepartureTime,
         @actualDockId, @actualArrivalTime, @actualDepartureTime, @status, @createdAt, @updatedAt);
      
      SELECT * FROM VesselVisitExecutions WHERE Id = @id;
    `;

    const dbData = vve.toDatabase();

    const params = [
      { name: "id", type: TYPES.UniqueIdentifier, value: dbData.Id },
      { name: "vvnId", type: TYPES.UniqueIdentifier, value: dbData.VvnId },
      {
        name: "operationPlanId",
        type: TYPES.UniqueIdentifier,
        value: dbData.OperationPlanId,
      },
      {
        name: "vveDate",
        type: TYPES.Date,
        value: parseAndValidateDate(dbData.VVEDate),
      },
      {
        name: "plannedDockId",
        type: TYPES.UniqueIdentifier,
        value: dbData.PlannedDockId,
      },
      {
        name: "plannedArrivalTime",
        type: TYPES.DateTime2,
        value: new Date(dbData.PlannedArrivalTime),
      },
      {
        name: "plannedDepartureTime",
        type: TYPES.DateTime2,
        value: new Date(dbData.PlannedDepartureTime),
      },
      {
        name: "actualDockId",
        type: TYPES.UniqueIdentifier,
        value: dbData.ActualDockId,
      },
      {
        name: "actualArrivalTime",
        type: TYPES.DateTime2,
        value: dbData.ActualArrivalTime
          ? new Date(dbData.ActualArrivalTime)
          : null,
      },
      {
        name: "actualDepartureTime",
        type: TYPES.DateTime2,
        value: dbData.ActualDepartureTime
          ? new Date(dbData.ActualDepartureTime)
          : null,
      },
      { name: "status", type: TYPES.NVarChar, value: dbData.Status },
      {
        name: "createdAt",
        type: TYPES.DateTime2,
        value: new Date(dbData.CreatedAt),
      },
      {
        name: "updatedAt",
        type: TYPES.DateTime2,
        value: dbData.UpdatedAt ? new Date(dbData.UpdatedAt) : null,
      },
    ];

    try {
      const results = await database.executeQuery(query, params);
      if (results.length > 0) {
        logger.info(`VVE created: ${dbData.Id} for VVN ${dbData.VvnId}`);
        return VesselVisitExecution.fromDatabase(results[0]);
      }
      throw new Error("Failed to create VVE");
    } catch (error) {
      const errorMsg = error.message || error.toString();
      if (
        errorMsg.includes("unique") ||
        errorMsg.includes("duplicate") ||
        errorMsg.includes("UNIQUE")
      ) {
        logger.error(`Duplicate VVE for VVN ${dbData.VvnId}`);
        throw new Error(`A VVE for VVN ${dbData.VvnId} already exists`);
      }
      logger.error("Error creating VVE:", error);
      throw error;
    }
  }

  /**
   * Get VVE by ID
   */
  async getByIdAsync(id) {
    const query = "SELECT * FROM VesselVisitExecutions WHERE Id = @id";
    const params = [{ name: "id", type: TYPES.UniqueIdentifier, value: id }];

    try {
      const results = await database.executeQuery(query, params);
      if (results.length === 0) {
        return null;
      }
      return VesselVisitExecution.fromDatabase(results[0]);
    } catch (error) {
      logger.error(`Error fetching VVE ${id}:`, error);
      throw error;
    }
  }

  /**
   * Get VVE by VVN ID
   */
  async getByVvnIdAsync(vvnId) {
    const query = "SELECT * FROM VesselVisitExecutions WHERE VvnId = @vvnId";
    const params = [
      { name: "vvnId", type: TYPES.UniqueIdentifier, value: vvnId },
    ];

    try {
      const results = await database.executeQuery(query, params);
      if (results.length === 0) {
        return null;
      }
      return VesselVisitExecution.fromDatabase(results[0]);
    } catch (error) {
      logger.error(`Error fetching VVE for VVN ${vvnId}:`, error);
      throw error;
    }
  }

  /**
   * Get VVEs by date
   */
  async getByDateAsync(vveDate) {
    const query =
      "SELECT * FROM VesselVisitExecutions WHERE VVEDate = @vveDate ORDER BY PlannedArrivalTime";
    const params = [
      {
        name: "vveDate",
        type: TYPES.Date,
        value: parseAndValidateDate(vveDate),
      },
    ];

    try {
      const results = await database.executeQuery(query, params);
      return results.map((row) => VesselVisitExecution.fromDatabase(row));
    } catch (error) {
      logger.error(`Error fetching VVEs for date ${vveDate}:`, error);
      throw error;
    }
  }

  /**
   * Get VVEs by operation plan ID
   */
  async getByOperationPlanIdAsync(operationPlanId) {
    const query =
      "SELECT * FROM VesselVisitExecutions WHERE OperationPlanId = @operationPlanId ORDER BY PlannedArrivalTime";
    const params = [
      {
        name: "operationPlanId",
        type: TYPES.UniqueIdentifier,
        value: operationPlanId,
      },
    ];

    try {
      const results = await database.executeQuery(query, params);
      return results.map((row) => VesselVisitExecution.fromDatabase(row));
    } catch (error) {
      logger.error(
        `Error fetching VVEs for operation plan ${operationPlanId}:`,
        error
      );
      throw error;
    }
  }

  /**
   * Search VVEs with filters
   */
  async searchAsync({ startDate, endDate, status, skip = 0, take = 50 }) {
    let query = "SELECT * FROM VesselVisitExecutions WHERE 1=1";
    const params = [];

    if (startDate) {
      query += " AND VVEDate >= @startDate";
      params.push({
        name: "startDate",
        type: TYPES.Date,
        value: parseAndValidateDate(startDate),
      });
    }

    if (endDate) {
      query += " AND VVEDate <= @endDate";
      params.push({
        name: "endDate",
        type: TYPES.Date,
        value: parseAndValidateDate(endDate),
      });
    }

    if (status) {
      query += " AND Status = @status";
      params.push({
        name: "status",
        type: TYPES.NVarChar,
        value: status,
      });
    }

    query +=
      " ORDER BY VVEDate DESC, PlannedArrivalTime DESC OFFSET @skip ROWS FETCH NEXT @take ROWS ONLY";
    params.push({ name: "skip", type: TYPES.Int, value: skip });
    params.push({ name: "take", type: TYPES.Int, value: take });

    try {
      const results = await database.executeQuery(query, params);
      return results.map((row) => VesselVisitExecution.fromDatabase(row));
    } catch (error) {
      logger.error("Error searching VVEs:", error);
      throw error;
    }
  }

  /**
   * Update VVE
   */
  async updateAsync(vve) {
    const query = `
      UPDATE VesselVisitExecutions
      SET 
        ActualDockId = @actualDockId,
        ActualArrivalTime = @actualArrivalTime,
        ActualDepartureTime = @actualDepartureTime,
        Status = @status,
        UpdatedAt = @updatedAt
      WHERE Id = @id;
      
      SELECT * FROM VesselVisitExecutions WHERE Id = @id;
    `;

    const dbData = vve.toDatabase();
    const params = [
      { name: "id", type: TYPES.UniqueIdentifier, value: dbData.Id },
      {
        name: "actualDockId",
        type: TYPES.UniqueIdentifier,
        value: dbData.ActualDockId,
      },
      {
        name: "actualArrivalTime",
        type: TYPES.DateTime2,
        value: dbData.ActualArrivalTime
          ? new Date(dbData.ActualArrivalTime)
          : null,
      },
      {
        name: "actualDepartureTime",
        type: TYPES.DateTime2,
        value: dbData.ActualDepartureTime
          ? new Date(dbData.ActualDepartureTime)
          : null,
      },
      { name: "status", type: TYPES.NVarChar, value: dbData.Status },
      { name: "updatedAt", type: TYPES.DateTime2, value: new Date() },
    ];

    try {
      const results = await database.executeQuery(query, params);
      if (results.length > 0) {
        logger.info(`VVE updated: ${dbData.Id}`);
        return VesselVisitExecution.fromDatabase(results[0]);
      }
      return null;
    } catch (error) {
      logger.error(`Error updating VVE ${dbData.Id}:`, error);
      throw error;
    }
  }

  /**
   * Delete VVE
   */
  async deleteAsync(id) {
    const query = "DELETE FROM VesselVisitExecutions WHERE Id = @id";
    const params = [{ name: "id", type: TYPES.UniqueIdentifier, value: id }];

    try {
      await database.executeQuery(query, params);
      logger.info(`VVE deleted: ${id}`);
      return true;
    } catch (error) {
      logger.error(`Error deleting VVE ${id}:`, error);
      throw error;
    }
  }

  /**
   * Check if VVE exists for a VVN
   */
  async existsByVvnIdAsync(vvnId) {
    const query =
      "SELECT COUNT(*) as count FROM VesselVisitExecutions WHERE VvnId = @vvnId";
    const params = [
      { name: "vvnId", type: TYPES.UniqueIdentifier, value: vvnId },
    ];

    try {
      const results = await database.executeQuery(query, params);
      return results[0].count > 0;
    } catch (error) {
      logger.error(`Error checking VVE existence for VVN ${vvnId}:`, error);
      throw error;
    }
  }

  /**
   * Delete all VVEs associated with an operation plan
   */
  async deleteByOperationPlanIdAsync(operationPlanId) {
    const query =
      "DELETE FROM VesselVisitExecutions WHERE OperationPlanId = @operationPlanId";
    const params = [
      {
        name: "operationPlanId",
        type: TYPES.UniqueIdentifier,
        value: operationPlanId,
      },
    ];

    try {
      await database.executeQuery(query, params);
      logger.info(`VVEs deleted for operation plan ${operationPlanId}`);
      // Note: executeQuery doesn't return rowsAffected, but the deletion is executed
      return true;
    } catch (error) {
      logger.error(
        `Error deleting VVEs for operation plan ${operationPlanId}:`,
        error
      );
      throw error;
    }
  }

  /**
   * Bulk create VVEs (for preparing today's executions)
   */
  async bulkCreateAsync(vves) {
    const results = [];
    for (const vve of vves) {
      try {
        const created = await this.createAsync(vve);
        results.push({ success: true, vve: created });
      } catch (error) {
        results.push({
          success: false,
          vvnId: vve.vvnId,
          error: error.message,
        });
      }
    }
    return results;
  }
}

module.exports = new VesselVisitExecutionRepository();
