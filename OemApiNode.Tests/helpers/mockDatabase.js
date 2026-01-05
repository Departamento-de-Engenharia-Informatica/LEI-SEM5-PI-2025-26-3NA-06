const logger = require("../../OemApiNode/src/utils/logger");

/**
 * Mock Database for Integration Tests
 * Simulates database behavior without executing SQL
 */
class MockDatabase {
  constructor() {
    this.tables = {
      OperationPlans: new Map(),
      Assignments: new Map(),
      VesselVisitExecutions: new Map(),
      IncidentTypes: new Map(),
      Incidents: new Map(),
      IncidentVVEs: new Map(),
    };
  }

  initialize() {
    logger.info("✓ Mock database initialized");
    this.clear();
    this.seed();
  }

  clear() {
    Object.keys(this.tables).forEach((table) => {
      this.tables[table].clear();
    });
  }

  seed() {
    // Seed Operation Plan
    const operationPlanId = "550e8400-e29b-41d4-a716-446655440000";
    this.tables.OperationPlans.set(operationPlanId, {
      id: operationPlanId,
      planDate: "2024-01-15",
      status: "NotStarted",
      algorithm: "FIFO",
      isFeasible: 1,
      creationDate: new Date().toISOString(),
      author: "test-user",
    });

    // Seed VVE
    const vveId = "660e8400-e29b-41d4-a716-446655440000";
    this.tables.VesselVisitExecutions.set(vveId, {
      id: vveId,
      vvnId: "123e4567-e89b-12d3-a456-426614174000",
      operationPlanId: null,
      vveDate: "2024-01-15",
      status: "NotStarted",
      plannedDockId: "223e4567-e89b-12d3-a456-426614174000",
      plannedArrivalTime: "2024-01-15T08:00:00Z",
      plannedDepartureTime: "2024-01-15T18:00:00Z",
      actualDockId: null,
      actualArrivalTime: null,
      actualDepartureTime: null,
      createdAt: new Date().toISOString(),
      updatedAt: new Date().toISOString(),
    });

    logger.info("✓ Mock test data seeded");
  }

  async executeQuery(query, params = []) {
    return new Promise((resolve) => {
      try {
        // Parse parameters into object
        const paramObj = {};
        params.forEach((param) => {
          paramObj[param.name] = param.value;
        });

        // Split by semicolon to handle multi-statement queries (e.g., INSERT; SELECT)
        const statements = query
          .split(";")
          .map((s) => s.trim())
          .filter((s) => s.length > 0);

        if (statements.length > 1) {
          // Execute all statements, return the result of the last one
          let lastResult = [];
          for (const statement of statements) {
            const sql = statement.toUpperCase();
            if (sql.startsWith("SELECT")) {
              lastResult = this.handleSelect(sql, paramObj, statement);
            } else if (sql.startsWith("INSERT")) {
              this.handleInsert(sql, paramObj, statement);
              lastResult = [{ rowsAffected: 1 }];
            } else if (sql.startsWith("UPDATE")) {
              this.handleUpdate(sql, paramObj, statement);
              lastResult = [{ rowsAffected: 1 }];
            } else if (sql.startsWith("DELETE")) {
              this.handleDelete(sql, paramObj, statement);
              lastResult = [{ rowsAffected: 1 }];
            }
          }
          resolve(lastResult);
        } else {
          // Single statement
          const sql = query.trim().toUpperCase();

          // Handle SELECT queries
          if (sql.startsWith("SELECT")) {
            const results = this.handleSelect(sql, paramObj, query);
            resolve(results);
          }
          // Handle INSERT queries
          else if (sql.startsWith("INSERT")) {
            this.handleInsert(sql, paramObj, query);
            resolve([{ rowsAffected: 1 }]);
          }
          // Handle UPDATE queries
          else if (sql.startsWith("UPDATE")) {
            this.handleUpdate(sql, paramObj, query);
            resolve([{ rowsAffected: 1 }]);
          }
          // Handle DELETE queries
          else if (sql.startsWith("DELETE")) {
            this.handleDelete(sql, paramObj, query);
            resolve([{ rowsAffected: 1 }]);
          } else {
            resolve([]);
          }
        }
      } catch (error) {
        logger.error("Mock database query error:", error);
        resolve([]);
      }
    });
  }

  handleSelect(sqlUpper, params, originalQuery) {
    // Operation Plans
    if (sqlUpper.includes("OPERATIONPLANS")) {
      if (
        sqlUpper.includes("WHERE") &&
        (sqlUpper.includes("PLANDATE") || params.planDate)
      ) {
        const planDate = params.planDate;
        // Normalize dates for comparison (handle both Date objects and strings)
        const normalizedSearchDate =
          planDate instanceof Date
            ? planDate.toISOString().split("T")[0]
            : typeof planDate === "string"
            ? planDate.split("T")[0]
            : planDate;

        const result = Array.from(this.tables.OperationPlans.values())
          .filter((p) => {
            const normalizedPlanDate =
              p.planDate instanceof Date
                ? p.planDate.toISOString().split("T")[0]
                : typeof p.planDate === "string"
                ? p.planDate.split("T")[0]
                : p.planDate;
            return normalizedPlanDate === normalizedSearchDate;
          })
          .map((p) => ({
            ...p,
            PlanDate: p.planDate,
            Id: p.id,
            Status: p.status,
            Algorithm: p.algorithm,
            IsFeasible: p.isFeasible,
            CreationDate: p.creationDate,
            Author: p.author,
          }));
        return result;
      }
      if (params.id) {
        const result = this.tables.OperationPlans.get(params.id);
        if (!result) return [];
        return [
          {
            ...result,
            PlanDate: result.planDate,
            Id: result.id,
            Status: result.status,
            Algorithm: result.algorithm,
            IsFeasible: result.isFeasible,
            CreationDate: result.creationDate,
            Author: result.author,
          },
        ];
      }
      // Search/List all
      return Array.from(this.tables.OperationPlans.values()).map((p) => ({
        ...p,
        PlanDate: p.planDate,
        Id: p.id,
        Status: p.status,
        Algorithm: p.algorithm,
        IsFeasible: p.isFeasible,
        CreationDate: p.creationDate,
        Author: p.author,
      }));
    }

    // VesselVisitExecutions
    if (sqlUpper.includes("VESSELVISITEXECUTIONS")) {
      if (params.id) {
        const result = this.tables.VesselVisitExecutions.get(params.id);
        if (!result) return [];
        return [
          {
            ...result,
            Id: result.id,
            VvnId: result.vvnId,
            OperationPlanId: result.operationPlanId,
            VVEDate: result.vveDate,
            Status: result.status,
            PlannedDockId: result.plannedDockId,
            PlannedArrivalTime: result.plannedArrivalTime,
            PlannedDepartureTime: result.plannedDepartureTime,
            ActualDockId: result.actualDockId,
            ActualArrivalTime: result.actualArrivalTime,
            ActualDepartureTime: result.actualDepartureTime,
            CreatedAt: result.createdAt,
            UpdatedAt: result.updatedAt,
          },
        ];
      }
      if (params.vvnId) {
        const result = Array.from(this.tables.VesselVisitExecutions.values())
          .filter((v) => v.vvnId === params.vvnId)
          .map((v) => ({
            ...v,
            Id: v.id,
            VvnId: v.vvnId,
            OperationPlanId: v.operationPlanId,
            VVEDate: v.vveDate,
            Status: v.status,
            PlannedDockId: v.plannedDockId,
            PlannedArrivalTime: v.plannedArrivalTime,
            PlannedDepartureTime: v.plannedDepartureTime,
            ActualDockId: v.actualDockId,
            ActualArrivalTime: v.actualArrivalTime,
            ActualDepartureTime: v.actualDepartureTime,
            CreatedAt: v.createdAt,
            UpdatedAt: v.updatedAt,
          }));
        return result;
      }
      if (params.vveDate) {
        const result = Array.from(this.tables.VesselVisitExecutions.values())
          .filter((v) => v.vveDate === params.vveDate)
          .map((v) => ({
            ...v,
            Id: v.id,
            VvnId: v.vvnId,
            OperationPlanId: v.operationPlanId,
            VVEDate: v.vveDate,
            Status: v.status,
            PlannedDockId: v.plannedDockId,
            PlannedArrivalTime: v.plannedArrivalTime,
            PlannedDepartureTime: v.plannedDepartureTime,
            ActualDockId: v.actualDockId,
            ActualArrivalTime: v.actualArrivalTime,
            ActualDepartureTime: v.actualDepartureTime,
            CreatedAt: v.createdAt,
            UpdatedAt: v.updatedAt,
          }));
        return result;
      }
      // Search/List all
      return Array.from(this.tables.VesselVisitExecutions.values()).map(
        (v) => ({
          ...v,
          Id: v.id,
          VvnId: v.vvnId,
          OperationPlanId: v.operationPlanId,
          VVEDate: v.vveDate,
          Status: v.status,
          PlannedDockId: v.plannedDockId,
          PlannedArrivalTime: v.plannedArrivalTime,
          PlannedDepartureTime: v.plannedDepartureTime,
          ActualDockId: v.actualDockId,
          ActualArrivalTime: v.actualArrivalTime,
          ActualDepartureTime: v.actualDepartureTime,
          CreatedAt: v.createdAt,
          UpdatedAt: v.updatedAt,
        })
      );
    }

    // Assignments
    if (sqlUpper.includes("ASSIGNMENTS")) {
      if (params.operationPlanId) {
        const result = Array.from(this.tables.Assignments.values()).filter(
          (a) => a.operationPlanId === params.operationPlanId
        );
        return result;
      }
      return Array.from(this.tables.Assignments.values());
    }

    return [];
  }

  handleInsert(sqlUpper, params, originalQuery) {
    // Operation Plan
    if (sqlUpper.includes("OPERATIONPLANS")) {
      this.tables.OperationPlans.set(params.id, {
        id: params.id,
        planDate: params.planDate,
        status: params.status || "NotStarted",
        algorithm: params.algorithm || "FIFO",
        isFeasible: params.isFeasible ?? 1,
        creationDate: params.creationDate || new Date().toISOString(),
        author: params.author || "unknown",
      });
    }

    // VVE
    if (sqlUpper.includes("VESSELVISITEXECUTIONS")) {
      this.tables.VesselVisitExecutions.set(params.id, {
        id: params.id,
        vvnId: params.vvnId,
        operationPlanId: params.operationPlanId || null,
        vveDate: params.vveDate,
        status: params.status || "NotStarted",
        plannedDockId: params.plannedDockId,
        plannedArrivalTime: params.plannedArrivalTime,
        plannedDepartureTime: params.plannedDepartureTime,
        actualDockId: params.actualDockId || null,
        actualArrivalTime: params.actualArrivalTime || null,
        actualDepartureTime: params.actualDepartureTime || null,
        createdAt: params.createdAt || new Date().toISOString(),
        updatedAt: params.updatedAt || new Date().toISOString(),
      });
    }

    // Assignment
    if (sqlUpper.includes("ASSIGNMENTS")) {
      const id = params.id || `${params.operationPlanId}-${Date.now()}`;
      this.tables.Assignments.set(id, {
        id,
        operationPlanId: params.operationPlanId,
        vvnId: params.vvnId,
        dockId: params.dockId,
        eta: params.eta,
        etd: params.etd,
        estimatedTeu: params.estimatedTeu || 0,
      });
    }
  }

  handleUpdate(sqlUpper, params, originalQuery) {
    // Operation Plan
    if (sqlUpper.includes("OPERATIONPLANS")) {
      if (params.id) {
        const existing = this.tables.OperationPlans.get(params.id);
        if (existing) {
          this.tables.OperationPlans.set(params.id, {
            ...existing,
            ...params,
          });
        }
      } else if (params.planDate) {
        // Update by date - normalize dates for comparison
        const normalizedSearchDate =
          params.planDate instanceof Date
            ? params.planDate.toISOString().split("T")[0]
            : typeof params.planDate === "string"
            ? params.planDate.split("T")[0]
            : params.planDate;

        Array.from(this.tables.OperationPlans.entries()).forEach(
          ([key, plan]) => {
            const normalizedPlanDate =
              plan.planDate instanceof Date
                ? plan.planDate.toISOString().split("T")[0]
                : typeof plan.planDate === "string"
                ? plan.planDate.split("T")[0]
                : plan.planDate;
            if (normalizedPlanDate === normalizedSearchDate) {
              this.tables.OperationPlans.set(key, {
                ...plan,
                ...params,
              });
            }
          }
        );
      }
    }

    // VVE
    if (sqlUpper.includes("VESSELVISITEXECUTIONS")) {
      if (params.id) {
        const existing = this.tables.VesselVisitExecutions.get(params.id);
        if (existing) {
          this.tables.VesselVisitExecutions.set(params.id, {
            ...existing,
            ...params,
            updatedAt: new Date().toISOString(),
          });
        }
      }
    }
  }

  handleDelete(sqlUpper, params, originalQuery) {
    // Operation Plan
    if (sqlUpper.includes("OPERATIONPLANS")) {
      if (params.planDate) {
        // The DELETE might receive a Date object created with new Date(str) which is UTC,
        // while SELECT uses parseAndValidateDate which creates local dates.
        // We need to handle both cases by checking if the dates represent the same calendar day
        let searchDates = [];

        if (params.planDate instanceof Date) {
          // Get the date string in both UTC and local timezone
          const utcDate = params.planDate.toISOString().split("T")[0];
          searchDates.push(utcDate);

          // Also check one day before and after to handle timezone edge cases
          const dayBefore = new Date(params.planDate);
          dayBefore.setDate(dayBefore.getDate() - 1);
          searchDates.push(dayBefore.toISOString().split("T")[0]);

          const dayAfter = new Date(params.planDate);
          dayAfter.setDate(dayAfter.getDate() + 1);
          searchDates.push(dayAfter.toISOString().split("T")[0]);
        } else if (typeof params.planDate === "string") {
          searchDates.push(params.planDate.split("T")[0]);
        }

        Array.from(this.tables.OperationPlans.entries()).forEach(
          ([key, plan]) => {
            const normalizedPlanDate =
              plan.planDate instanceof Date
                ? plan.planDate.toISOString().split("T")[0]
                : typeof plan.planDate === "string"
                ? plan.planDate.split("T")[0]
                : plan.planDate;
            if (searchDates.includes(normalizedPlanDate)) {
              this.tables.OperationPlans.delete(key);
            }
          }
        );
      } else if (params.id) {
        this.tables.OperationPlans.delete(params.id);
      }
    }

    // VVE
    if (sqlUpper.includes("VESSELVISITEXECUTIONS")) {
      if (params.id) {
        this.tables.VesselVisitExecutions.delete(params.id);
      }
    }

    // Assignments
    if (sqlUpper.includes("ASSIGNMENTS")) {
      if (params.operationPlanId) {
        Array.from(this.tables.Assignments.entries()).forEach(
          ([key, assignment]) => {
            if (assignment.operationPlanId === params.operationPlanId) {
              this.tables.Assignments.delete(key);
            }
          }
        );
      }
    }
  }

  close() {
    this.clear();
    logger.info("✓ Mock database closed");
  }
}

module.exports = new MockDatabase();
