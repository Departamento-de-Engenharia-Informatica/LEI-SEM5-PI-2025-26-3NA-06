const { TYPES } = require("tedious");
const database = require("../config/database");
const logger = require("../utils/logger");
const Incident = require("../domain/Incident/Incident");

class IncidentRepository {
  /**
   * Ensure Incidents table exists
   */
  async ensureTableExists() {
    const createTableSQL = `
      IF NOT EXISTS (SELECT * FROM sys.tables WHERE name = 'Incidents')
      BEGIN
        CREATE TABLE Incidents (
          Id UNIQUEIDENTIFIER PRIMARY KEY,
          IncidentTypeId UNIQUEIDENTIFIER NOT NULL,
          Date DATE NOT NULL,
          StartTime DATETIME2 NOT NULL,
          EndTime DATETIME2 NULL,
          Description NVARCHAR(MAX) NOT NULL,
          AffectsAllVVEs BIT NOT NULL DEFAULT 0,
          AffectedVVEIds NVARCHAR(MAX),
          IsActive BIT NOT NULL DEFAULT 1,
          CreatedAt DATETIME2 NOT NULL DEFAULT GETUTCDATE(),
          UpdatedAt DATETIME2 NULL,
          
          CONSTRAINT FK_Incident_IncidentType FOREIGN KEY (IncidentTypeId) 
            REFERENCES IncidentTypes(Id)
        );
        
        CREATE INDEX IX_Incident_Date ON Incidents(Date);
        CREATE INDEX IX_Incident_IncidentTypeId ON Incidents(IncidentTypeId);
        CREATE INDEX IX_Incident_IsActive ON Incidents(IsActive);
      END
    `;

    try {
      await database.executeQuery(createTableSQL);
      logger.info("Incidents table ready");
    } catch (error) {
      logger.error("Error ensuring Incidents table exists:", error);
      throw error;
    }
  }

  /**
   * Create a new incident
   */
  async createAsync(incident) {
    const query = `
      INSERT INTO Incidents (
        Id, IncidentTypeId, Date, StartTime, EndTime, Description,
        AffectsAllVVEs, AffectedVVEIds, IsActive, CreatedAt, UpdatedAt
      )
      VALUES (
        @id, @incidentTypeId, @date, @startTime, @endTime, @description,
        @affectsAllVVEs, @affectedVVEIds, @isActive, @createdAt, @updatedAt
      )
    `;

    const dbData = incident.toDatabase();
    const params = [
      { name: "id", type: TYPES.UniqueIdentifier, value: dbData.Id },
      {
        name: "incidentTypeId",
        type: TYPES.UniqueIdentifier,
        value: dbData.IncidentTypeId,
      },
      { name: "date", type: TYPES.Date, value: dbData.Date },
      { name: "startTime", type: TYPES.DateTime2, value: dbData.StartTime },
      { name: "endTime", type: TYPES.DateTime2, value: dbData.EndTime },
      { name: "description", type: TYPES.NVarChar, value: dbData.Description },
      { name: "affectsAllVVEs", type: TYPES.Bit, value: dbData.AffectsAllVVEs },
      {
        name: "affectedVVEIds",
        type: TYPES.NVarChar,
        value: dbData.AffectedVVEIds,
      },
      { name: "isActive", type: TYPES.Bit, value: dbData.IsActive },
      { name: "createdAt", type: TYPES.DateTime2, value: dbData.CreatedAt },
      { name: "updatedAt", type: TYPES.DateTime2, value: dbData.UpdatedAt },
    ];

    try {
      await database.executeQuery(query, params);
      return incident;
    } catch (error) {
      logger.error(`Error creating incident:`, error);
      throw error;
    }
  }

  /**
   * Get incident by ID with incident type details
   */
  async getByIdAsync(id) {
    const query = `
      SELECT 
        i.*,
        it.Code as IncidentTypeCode,
        it.Name as IncidentTypeName,
        it.Severity as IncidentTypeSeverity
      FROM Incidents i
      LEFT JOIN IncidentTypes it ON i.IncidentTypeId = it.Id
      WHERE i.Id = @id
    `;
    const params = [{ name: "id", type: TYPES.UniqueIdentifier, value: id }];

    try {
      const results = await database.executeQuery(query, params);
      if (results.length === 0) {
        return null;
      }
      const incident = Incident.fromDatabase(results[0]);
      // Add incident type details
      incident.incidentTypeCode = results[0].IncidentTypeCode;
      incident.incidentTypeName = results[0].IncidentTypeName;
      incident.incidentTypeSeverity = results[0].IncidentTypeSeverity;
      return incident;
    } catch (error) {
      logger.error(`Error fetching incident by ID ${id}:`, error);
      throw error;
    }
  }

  /**
   * Get all incidents with filtering
   */
  async getAllAsync({ date, status, incidentTypeId } = {}) {
    let whereClauses = ["i.IsActive = 1"];
    let params = [];

    if (date) {
      whereClauses.push("i.Date = @date");
      params.push({ name: "date", type: TYPES.Date, value: date });
    }

    if (incidentTypeId) {
      whereClauses.push("i.IncidentTypeId = @incidentTypeId");
      params.push({
        name: "incidentTypeId",
        type: TYPES.UniqueIdentifier,
        value: incidentTypeId,
      });
    }

    // Status filter: 'active' or 'inactive' based on current time
    if (status === "active") {
      whereClauses.push(
        "(i.EndTime IS NULL OR (GETDATE() >= i.StartTime AND GETDATE() <= i.EndTime))"
      );
    } else if (status === "inactive") {
      whereClauses.push("i.EndTime IS NOT NULL AND GETDATE() > i.EndTime");
    }

    const query = `
      SELECT 
        i.*,
        it.Code as IncidentTypeCode,
        it.Name as IncidentTypeName,
        it.Severity as IncidentTypeSeverity
      FROM Incidents i
      LEFT JOIN IncidentTypes it ON i.IncidentTypeId = it.Id
      WHERE ${whereClauses.join(" AND ")}
      ORDER BY i.Date DESC, i.StartTime DESC
    `;

    try {
      const results = await database.executeQuery(query, params);
      return results.map((row) => {
        const incident = Incident.fromDatabase(row);
        incident.incidentTypeCode = row.IncidentTypeCode;
        incident.incidentTypeName = row.IncidentTypeName;
        incident.incidentTypeSeverity = row.IncidentTypeSeverity;
        return incident;
      });
    } catch (error) {
      logger.error("Error fetching all incidents:", error);
      throw error;
    }
  }

  /**
   * Update incident
   */
  async updateAsync(incident) {
    const query = `
      UPDATE Incidents
      SET 
        StartTime = @startTime,
        EndTime = @endTime,
        Description = @description,
        AffectsAllVVEs = @affectsAllVVEs,
        AffectedVVEIds = @affectedVVEIds,
        UpdatedAt = @updatedAt
      WHERE Id = @id
    `;

    const dbData = incident.toDatabase();
    const params = [
      { name: "id", type: TYPES.UniqueIdentifier, value: dbData.Id },
      { name: "startTime", type: TYPES.DateTime2, value: dbData.StartTime },
      { name: "endTime", type: TYPES.DateTime2, value: dbData.EndTime },
      { name: "description", type: TYPES.NVarChar, value: dbData.Description },
      { name: "affectsAllVVEs", type: TYPES.Bit, value: dbData.AffectsAllVVEs },
      {
        name: "affectedVVEIds",
        type: TYPES.NVarChar,
        value: dbData.AffectedVVEIds,
      },
      { name: "updatedAt", type: TYPES.DateTime2, value: new Date() },
    ];

    try {
      await database.executeQuery(query, params);
      return incident;
    } catch (error) {
      logger.error(`Error updating incident ${incident.id}:`, error);
      throw error;
    }
  }

  /**
   * Delete incident (soft delete)
   */
  async deleteAsync(id) {
    const query = `
      UPDATE Incidents
      SET IsActive = 0, UpdatedAt = @updatedAt
      WHERE Id = @id
    `;

    const params = [
      { name: "id", type: TYPES.UniqueIdentifier, value: id },
      { name: "updatedAt", type: TYPES.DateTime2, value: new Date() },
    ];

    try {
      await database.executeQuery(query, params);
      return await this.getByIdAsync(id);
    } catch (error) {
      logger.error(`Error deleting incident ${id}:`, error);
      throw error;
    }
  }

  /**
   * Get today's active incidents
   */
  async getTodaysActiveAsync() {
    const query = `
      SELECT 
        i.*,
        it.Code as IncidentTypeCode,
        it.Name as IncidentTypeName,
        it.Severity as IncidentTypeSeverity
      FROM Incidents i
      LEFT JOIN IncidentTypes it ON i.IncidentTypeId = it.Id
      WHERE i.IsActive = 1
        AND i.Date = CAST(GETDATE() AS DATE)
        AND (i.EndTime IS NULL OR (GETDATE() >= i.StartTime AND GETDATE() <= i.EndTime))
      ORDER BY it.Severity DESC, i.StartTime DESC
    `;

    try {
      const results = await database.executeQuery(query);
      return results.map((row) => {
        const incident = Incident.fromDatabase(row);
        incident.incidentTypeCode = row.IncidentTypeCode;
        incident.incidentTypeName = row.IncidentTypeName;
        incident.incidentTypeSeverity = row.IncidentTypeSeverity;
        return incident;
      });
    } catch (error) {
      logger.error("Error fetching today's active incidents:", error);
      throw error;
    }
  }
}

module.exports = new IncidentRepository();
