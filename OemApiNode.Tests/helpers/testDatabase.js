const Database = require("better-sqlite3");
const logger = require("../../OemApiNode/src/utils/logger");

class TestDatabase {
  constructor() {
    this.db = null;
  }

  initialize() {
    // Create in-memory database
    this.db = new Database(":memory:");
    logger.info("✓ Test in-memory database initialized");

    // Create schema
    this.createSchema();
  }

  createSchema() {
    // Create OperationPlans table
    this.db.exec(`
      CREATE TABLE IF NOT EXISTS OperationPlans (
        id TEXT PRIMARY KEY,
        planDate TEXT NOT NULL,
        status TEXT NOT NULL,
        algorithm TEXT NOT NULL,
        isFeasible INTEGER NOT NULL,
        creationDate TEXT NOT NULL,
        author TEXT
      )
    `);

    // Create Assignments table
    this.db.exec(`
      CREATE TABLE IF NOT EXISTS Assignments (
        id TEXT PRIMARY KEY,
        operationPlanId TEXT NOT NULL,
        vvnId TEXT NOT NULL,
        dockId TEXT NOT NULL,
        eta TEXT NOT NULL,
        etd TEXT NOT NULL,
        estimatedTeu INTEGER,
        FOREIGN KEY (operationPlanId) REFERENCES OperationPlans(id)
      )
    `);

    // Create VesselVisitExecutions table
    this.db.exec(`
      CREATE TABLE IF NOT EXISTS VesselVisitExecutions (
        id TEXT PRIMARY KEY,
        vvnId TEXT NOT NULL,
        vveDate TEXT NOT NULL,
        status TEXT NOT NULL,
        plannedDockId TEXT NOT NULL,
        plannedArrivalTime TEXT NOT NULL,
        plannedDepartureTime TEXT NOT NULL,
        actualDockId TEXT,
        actualArrivalTime TEXT,
        actualDepartureTime TEXT,
        createdAt TEXT NOT NULL,
        updatedAt TEXT NOT NULL
      )
    `);

    // Create IncidentTypes table
    this.db.exec(`
      CREATE TABLE IF NOT EXISTS IncidentTypes (
        id TEXT PRIMARY KEY,
        code TEXT NOT NULL UNIQUE,
        name TEXT NOT NULL,
        description TEXT,
        severity TEXT NOT NULL,
        parentId TEXT,
        isActive INTEGER NOT NULL DEFAULT 1,
        createdAt TEXT NOT NULL,
        updatedAt TEXT NOT NULL
      )
    `);

    // Create Incidents table
    this.db.exec(`
      CREATE TABLE IF NOT EXISTS Incidents (
        id TEXT PRIMARY KEY,
        incidentTypeId TEXT NOT NULL,
        description TEXT NOT NULL,
        startTime TEXT NOT NULL,
        endTime TEXT,
        affectsAllVVEs INTEGER NOT NULL DEFAULT 0,
        isActive INTEGER NOT NULL DEFAULT 1,
        createdAt TEXT NOT NULL,
        updatedAt TEXT NOT NULL,
        FOREIGN KEY (incidentTypeId) REFERENCES IncidentTypes(id)
      )
    `);

    // Create IncidentVVEs junction table
    this.db.exec(`
      CREATE TABLE IF NOT EXISTS IncidentVVEs (
        incidentId TEXT NOT NULL,
        vveId TEXT NOT NULL,
        PRIMARY KEY (incidentId, vveId),
        FOREIGN KEY (incidentId) REFERENCES Incidents(id),
        FOREIGN KEY (vveId) REFERENCES VesselVisitExecutions(id)
      )
    `);

    logger.info("✓ Test database schema created");
  }

  // Adapter method to work with tedious-style queries
  async executeQuery(query, params = []) {
    return new Promise((resolve, reject) => {
      try {
        // Convert tedious parameters to SQLite format
        let sqliteQuery = query;
        const sqliteParams = {};

        // Replace @paramName with :paramName for SQLite
        params.forEach((param) => {
          const paramName = param.name;
          sqliteQuery = sqliteQuery.replace(
            new RegExp(`@${paramName}`, "g"),
            `:${paramName}`
          );
          sqliteParams[paramName] = param.value;
        });

        // Execute query
        if (sqliteQuery.trim().toUpperCase().startsWith("SELECT")) {
          const stmt = this.db.prepare(sqliteQuery);
          const results = stmt.all(sqliteParams);
          resolve(results);
        } else {
          const stmt = this.db.prepare(sqliteQuery);
          const info = stmt.run(sqliteParams);
          resolve([{ rowsAffected: info.changes }]);
        }
      } catch (error) {
        logger.error("Test database query error:", error);
        reject(error);
      }
    });
  }

  // Helper method to clear all tables
  clearAllTables() {
    this.db.exec(`DELETE FROM IncidentVVEs`);
    this.db.exec(`DELETE FROM Incidents`);
    this.db.exec(`DELETE FROM IncidentTypes`);
    this.db.exec(`DELETE FROM Assignments`);
    this.db.exec(`DELETE FROM VesselVisitExecutions`);
    this.db.exec(`DELETE FROM OperationPlans`);
  }

  // Seed test data
  seedTestData() {
    // Add sample operation plan
    const operationPlanId = "550e8400-e29b-41d4-a716-446655440000";
    this.db
      .prepare(
        `
      INSERT INTO OperationPlans (id, planDate, status, algorithm, isFeasible, creationDate, author)
      VALUES (?, ?, ?, ?, ?, ?, ?)
    `
      )
      .run(
        operationPlanId,
        "2024-01-15",
        "NotStarted",
        "FIFO",
        1,
        new Date().toISOString(),
        "test-user"
      );

    // Add sample VVE
    const vveId = "660e8400-e29b-41d4-a716-446655440000";
    this.db
      .prepare(
        `
      INSERT INTO VesselVisitExecutions (
        id, vvnId, vveDate, status, 
        plannedDockId, plannedArrivalTime, plannedDepartureTime,
        createdAt, updatedAt
      ) VALUES (?, ?, ?, ?, ?, ?, ?, ?, ?)
    `
      )
      .run(
        vveId,
        "123e4567-e89b-12d3-a456-426614174000",
        "2024-01-15",
        "NotStarted",
        "223e4567-e89b-12d3-a456-426614174000",
        "2024-01-15T08:00:00Z",
        "2024-01-15T18:00:00Z",
        new Date().toISOString(),
        new Date().toISOString()
      );

    logger.info("✓ Test data seeded");
  }

  close() {
    if (this.db) {
      this.db.close();
      this.db = null;
      logger.info("✓ Test database closed");
    }
  }

  // Direct SQL execution for testing
  exec(sql) {
    return this.db.exec(sql);
  }

  prepare(sql) {
    return this.db.prepare(sql);
  }
}

module.exports = new TestDatabase();
