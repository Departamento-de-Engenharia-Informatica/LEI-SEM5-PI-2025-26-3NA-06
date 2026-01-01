const { Request, TYPES } = require("tedious");
const database = require("../config/database");
const logger = require("../utils/logger");
const OperationPlan = require("../domain/OperationPlan/OperationPlan");

/**
 * Parse and validate date string to Date object
 * @param {string|Date} dateInput - Date string or Date object
 * @returns {Date} Valid Date object
 * @throws {Error} If date is invalid
 */
function parseAndValidateDate(dateInput) {
  if (dateInput instanceof Date) {
    if (isNaN(dateInput.getTime())) {
      throw new Error("Invalid date object");
    }
    return dateInput;
  }

  // Handle string date input in YYYY-MM-DD format
  if (typeof dateInput === "string") {
    // If it's in YYYY-MM-DD format, parse it explicitly to avoid timezone issues
    const match = dateInput.match(/^(\d{4})-(\d{2})-(\d{2})/);
    if (match) {
      const year = parseInt(match[1], 10);
      const month = parseInt(match[2], 10) - 1; // JS months are 0-indexed
      const day = parseInt(match[3], 10);
      const parsed = new Date(year, month, day);

      if (isNaN(parsed.getTime())) {
        throw new Error(
          `Validation failed for parameter 'planDate'. Invalid date: ${dateInput}`
        );
      }
      return parsed;
    }
  }

  // Fallback to standard Date parsing
  const parsed = new Date(dateInput);
  if (isNaN(parsed.getTime())) {
    throw new Error(
      `Validation failed for parameter 'planDate'. Invalid date: ${dateInput}`
    );
  }
  return parsed;
}

class OperationPlanRepository {
  /**
   * Create operation plan table if not exists
   */
  async ensureTableExists() {
    // Only create table if it does not exist, and add missing columns if needed
    const createTableSQL = `
      IF NOT EXISTS (SELECT * FROM sys.tables WHERE name = 'OperationPlans')
      BEGIN
        CREATE TABLE OperationPlans (
          Id UNIQUEIDENTIFIER PRIMARY KEY,
          PlanDate DATE NOT NULL,
          Status NVARCHAR(50) NOT NULL DEFAULT 'Pending',
          IsFeasible BIT NOT NULL DEFAULT 1,
          Warnings NVARCHAR(MAX),
          Assignments NVARCHAR(MAX) NOT NULL,
          Algorithm NVARCHAR(50) NOT NULL DEFAULT 'FIFO',
          CreationDate DATETIME2 NOT NULL DEFAULT GETUTCDATE(),
          Author NVARCHAR(255),
          CONSTRAINT UQ_OperationPlan_PlanDate UNIQUE (PlanDate)
        );
        CREATE INDEX IX_OperationPlan_PlanDate ON OperationPlans(PlanDate);
        CREATE INDEX IX_OperationPlan_Status ON OperationPlans(Status);
      END
    `;

    // Migration: Add missing columns if table already exists
    const migrationSQL = `
      IF EXISTS (SELECT * FROM sys.tables WHERE name = 'OperationPlans')
      BEGIN
        -- Add PlanDate column if it doesn't exist
        IF NOT EXISTS (SELECT * FROM sys.columns WHERE object_id = OBJECT_ID('OperationPlans') AND name = 'PlanDate')
        BEGIN
          ALTER TABLE OperationPlans ADD PlanDate DATE NOT NULL DEFAULT GETDATE();
          ALTER TABLE OperationPlans ADD CONSTRAINT UQ_OperationPlan_PlanDate UNIQUE (PlanDate);
          CREATE INDEX IX_OperationPlan_PlanDate ON OperationPlans(PlanDate);
        END

        -- Add IsFeasible column if it doesn't exist
        IF NOT EXISTS (SELECT * FROM sys.columns WHERE object_id = OBJECT_ID('OperationPlans') AND name = 'IsFeasible')
        BEGIN
          ALTER TABLE OperationPlans ADD IsFeasible BIT NOT NULL DEFAULT 1;
        END

        -- Add Warnings column if it doesn't exist
        IF NOT EXISTS (SELECT * FROM sys.columns WHERE object_id = OBJECT_ID('OperationPlans') AND name = 'Warnings')
        BEGIN
          ALTER TABLE OperationPlans ADD Warnings NVARCHAR(MAX);
        END

        -- Add Assignments column if it doesn't exist
        IF NOT EXISTS (SELECT * FROM sys.columns WHERE object_id = OBJECT_ID('OperationPlans') AND name = 'Assignments')
        BEGIN
          ALTER TABLE OperationPlans ADD Assignments NVARCHAR(MAX) NOT NULL DEFAULT '[]';
        END

        -- Add Algorithm column if it doesn't exist
        IF NOT EXISTS (SELECT * FROM sys.columns WHERE object_id = OBJECT_ID('OperationPlans') AND name = 'Algorithm')
        BEGIN
          ALTER TABLE OperationPlans ADD Algorithm NVARCHAR(50) NOT NULL DEFAULT 'FIFO';
        END

        -- Add CreationDate column if it doesn't exist
        IF NOT EXISTS (SELECT * FROM sys.columns WHERE object_id = OBJECT_ID('OperationPlans') AND name = 'CreationDate')
        BEGIN
          ALTER TABLE OperationPlans ADD CreationDate DATETIME2 NOT NULL DEFAULT GETUTCDATE();
        END

        -- Add Author column if it doesn't exist
        IF NOT EXISTS (SELECT * FROM sys.columns WHERE object_id = OBJECT_ID('OperationPlans') AND name = 'Author')
        BEGIN
          ALTER TABLE OperationPlans ADD Author NVARCHAR(255);
        END

        -- Add Status column if it doesn't exist
        IF NOT EXISTS (SELECT * FROM sys.columns WHERE object_id = OBJECT_ID('OperationPlans') AND name = 'Status')
        BEGIN
          ALTER TABLE OperationPlans ADD Status NVARCHAR(50) NOT NULL DEFAULT 'Pending';
          CREATE INDEX IX_OperationPlan_Status ON OperationPlans(Status);
        END
      END
    `;

    try {
      await database.executeQuery(createTableSQL);
      await database.executeQuery(migrationSQL);
      logger.info("OperationPlans table ready (schema ensured, no drop)");
    } catch (error) {
      logger.error("Failed to ensure OperationPlans table exists:", error);
      throw error;
    }
  }

  /**
   * Create a new operation plan
   */
  async createAsync(operationPlan) {
    const query = `
      INSERT INTO OperationPlans 
        (Id, PlanDate, Status, IsFeasible, Warnings, Assignments, Algorithm, CreationDate, Author)
      VALUES 
        (@id, @planDate, @status, @isFeasible, @warnings, @assignments, @algorithm, @creationDate, @author);
      
      SELECT * FROM OperationPlans WHERE Id = @id;
    `;

    const dbData = operationPlan.toDatabase();

    // Log the date being processed for debugging
    logger.info(
      `Creating operation plan - Raw PlanDate: ${
        dbData.PlanDate
      }, Type: ${typeof dbData.PlanDate}`
    );

    const parsedDate = parseAndValidateDate(dbData.PlanDate);
    logger.info(
      `Parsed date for SQL: ${parsedDate.toISOString()}, ${parsedDate}`
    );

    const params = [
      { name: "id", type: TYPES.UniqueIdentifier, value: dbData.Id },
      {
        name: "planDate",
        type: TYPES.Date,
        value: parsedDate,
      },
      {
        name: "status",
        type: TYPES.NVarChar,
        value: dbData.Status || "Pending",
      },
      { name: "isFeasible", type: TYPES.Bit, value: dbData.IsFeasible ? 1 : 0 },
      { name: "warnings", type: TYPES.NVarChar, value: dbData.Warnings },
      {
        name: "assignments",
        type: TYPES.NVarChar,
        value: dbData.Assignments,
      },
      { name: "algorithm", type: TYPES.NVarChar, value: dbData.Algorithm },
      {
        name: "creationDate",
        type: TYPES.DateTime2,
        value: new Date(dbData.CreationDate),
      },
      { name: "author", type: TYPES.NVarChar, value: dbData.Author },
    ];

    try {
      const results = await database.executeQuery(query, params);
      if (results.length > 0) {
        logger.info(
          `Operation plan created: ${dbData.Id} for date ${dbData.PlanDate}`
        );
        return OperationPlan.fromDatabase(results[0]);
      }
      throw new Error("Failed to create operation plan");
    } catch (error) {
      // Check for unique constraint violation
      const errorMsg = error.message || error.toString();
      if (
        errorMsg.includes("unique") ||
        errorMsg.includes("duplicate") ||
        errorMsg.includes("UNIQUE")
      ) {
        logger.error(`Duplicate operation plan for date ${dbData.PlanDate}`);
        throw new Error(
          `An operation plan for date ${dbData.PlanDate} already exists`
        );
      }
      logger.error("Error creating operation plan:", error);
      throw error;
    }
  }

  /**
   * Get operation plan by ID
   */
  async getByIdAsync(id) {
    const query = "SELECT * FROM OperationPlans WHERE Id = @id";
    const params = [{ name: "id", type: TYPES.UniqueIdentifier, value: id }];

    try {
      const results = await database.executeQuery(query, params);
      if (results.length === 0) {
        return null;
      }
      return OperationPlan.fromDatabase(results[0]);
    } catch (error) {
      logger.error(`Error fetching operation plan ${id}:`, error);
      throw error;
    }
  }

  /**
   * Get operation plan by date
   */
  async getByDateAsync(planDate) {
    const query = "SELECT * FROM OperationPlans WHERE PlanDate = @planDate";
    const params = [
      {
        name: "planDate",
        type: TYPES.Date,
        value: parseAndValidateDate(planDate),
      },
    ];

    try {
      const results = await database.executeQuery(query, params);
      if (results.length === 0) {
        return null;
      }
      return OperationPlan.fromDatabase(results[0]);
    } catch (error) {
      logger.error(
        `Error fetching operation plan for date ${planDate}:`,
        error
      );
      throw error;
    }
  }

  /**
   * Search operation plans with filters
   */
  async searchAsync({ startDate, endDate, vesselIMO, skip = 0, take = 50 }) {
    let query = "SELECT * FROM OperationPlans WHERE 1=1";
    const params = [];

    if (startDate) {
      query += " AND PlanDate >= @startDate";
      params.push({
        name: "startDate",
        type: TYPES.Date,
        value: parseAndValidateDate(startDate),
      });
    }

    if (endDate) {
      query += " AND PlanDate <= @endDate";
      params.push({
        name: "endDate",
        type: TYPES.Date,
        value: parseAndValidateDate(endDate),
      });
    }

    if (vesselIMO) {
      // Search in Assignments JSON for vessel IMO or vessel ID
      query += " AND (Assignments LIKE @vesselIMO OR Warnings LIKE @vesselIMO)";
      params.push({
        name: "vesselIMO",
        type: TYPES.NVarChar,
        value: `%${vesselIMO}%`,
      });
    }

    query +=
      " ORDER BY PlanDate DESC OFFSET @skip ROWS FETCH NEXT @take ROWS ONLY";
    params.push({ name: "skip", type: TYPES.Int, value: skip });
    params.push({ name: "take", type: TYPES.Int, value: take });

    try {
      const results = await database.executeQuery(query, params);
      return results.map((row) => OperationPlan.fromDatabase(row));
    } catch (error) {
      logger.error("Error searching operation plans:", error);
      throw error;
    }
  }

  /**
   * Update operation plan
   */
  async updateAsync(operationPlan) {
    const query = `
      UPDATE OperationPlans
      SET 
        Status = @status,
        IsFeasible = @isFeasible,
        Warnings = @warnings,
        Assignments = @assignments
      WHERE PlanDate = @planDate;
      
      SELECT * FROM OperationPlans WHERE PlanDate = @planDate;
    `;

    const dbData = operationPlan.toDatabase();
    const params = [
      { name: "planDate", type: TYPES.Date, value: new Date(dbData.PlanDate) },
      {
        name: "status",
        type: TYPES.NVarChar,
        value: dbData.Status || "Pending",
      },
      { name: "isFeasible", type: TYPES.Bit, value: dbData.IsFeasible ? 1 : 0 },
      {
        name: "warnings",
        type: TYPES.NVarChar,
        value: dbData.Warnings,
      },
      {
        name: "assignments",
        type: TYPES.NVarChar,
        value: dbData.Assignments,
      },
    ];

    try {
      const results = await database.executeQuery(query, params);
      if (results.length > 0) {
        logger.info(`Operation plan updated: ${dbData.Id}`);
        return OperationPlan.fromDatabase(results[0]);
      }
      return null;
    } catch (error) {
      logger.error(
        `Error updating operation plan for date ${dbData.PlanDate}:`,
        error
      );
      throw error;
    }
  }

  /**
   * Delete operation plan by date
   */
  async deleteAsync(planDate) {
    const query = "DELETE FROM OperationPlans WHERE PlanDate = @planDate";
    const params = [
      { name: "planDate", type: TYPES.Date, value: new Date(planDate) },
    ];

    try {
      await database.executeQuery(query, params);
      logger.info(`Operation plan deleted for date: ${planDate}`);
      return true;
    } catch (error) {
      logger.error(
        `Error deleting operation plan for date ${planDate}:`,
        error
      );
      throw error;
    }
  }

  /**
   * Check if operation plan exists for a specific date
   */
  async existsByDateAsync(planDate) {
    const query =
      "SELECT COUNT(*) as count FROM OperationPlans WHERE PlanDate = @planDate";
    const params = [
      { name: "planDate", type: TYPES.Date, value: new Date(planDate) },
    ];

    try {
      const results = await database.executeQuery(query, params);
      return results[0].count > 0;
    } catch (error) {
      logger.error(
        `Error checking if plan exists for date ${planDate}:`,
        error
      );
      throw error;
    }
  }
}

module.exports = new OperationPlanRepository();
