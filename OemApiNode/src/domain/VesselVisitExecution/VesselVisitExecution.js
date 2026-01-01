const { v4: uuidv4, validate: uuidValidate } = require("uuid");

/**
 * Vessel Visit Execution (VVE) Domain Entity
 * Represents the actual runtime execution of a Vessel Visit Notification (VVN)
 * Used to track real operations vs planned operations
 */
class VesselVisitExecution {
  constructor({
    id = null,
    vvnId,
    operationPlanId = null,
    vveDate,
    plannedDockId,
    plannedArrivalTime,
    plannedDepartureTime,
    actualDockId = null,
    actualArrivalTime = null,
    actualDepartureTime = null,
    status = "NotStarted",
    createdAt = null,
    updatedAt = null,
  }) {
    this.id = id || uuidv4();
    this.vvnId = vvnId;
    this.operationPlanId = operationPlanId;
    this.vveDate = this.normalizeDate(vveDate);
    this.plannedDockId = plannedDockId;
    this.plannedArrivalTime = plannedArrivalTime;
    this.plannedDepartureTime = plannedDepartureTime;
    this.actualDockId = actualDockId;
    this.actualArrivalTime = actualArrivalTime;
    this.actualDepartureTime = actualDepartureTime;
    this.status = this.validateStatus(status);
    this.createdAt = createdAt || new Date();
    this.updatedAt = updatedAt;

    this.validate();
  }

  /**
   * Normalize date to YYYY-MM-DD string format
   */
  normalizeDate(date) {
    if (!date) {
      return null;
    }

    if (typeof date === "string" && /^\d{4}-\d{2}-\d{2}$/.test(date)) {
      return date;
    }

    const d = new Date(date);
    if (isNaN(d.getTime())) {
      return date;
    }

    const year = d.getFullYear();
    const month = String(d.getMonth() + 1).padStart(2, "0");
    const day = String(d.getDate()).padStart(2, "0");
    return `${year}-${month}-${day}`;
  }

  /**
   * Validate VVE required fields
   */
  validate() {
    const errors = [];

    if (!uuidValidate(this.id)) {
      errors.push("Invalid ID format (must be UUID)");
    }

    if (!this.vvnId || !uuidValidate(this.vvnId)) {
      errors.push("Invalid vvnId (must be a valid UUID)");
    }

    if (!this.vveDate) {
      errors.push("vveDate is required");
    }

    if (!this.plannedDockId || !uuidValidate(this.plannedDockId)) {
      errors.push("Invalid plannedDockId (must be a valid UUID)");
    }

    if (!this.plannedArrivalTime) {
      errors.push("plannedArrivalTime is required");
    }

    if (!this.plannedDepartureTime) {
      errors.push("plannedDepartureTime is required");
    }

    if (errors.length > 0) {
      throw new Error(`VVE validation failed: ${errors.join(", ")}`);
    }
  }

  /**
   * Validate and normalize status
   * Valid states: NotStarted, InProgress, Delayed, Completed
   */
  validateStatus(status) {
    const validStatuses = ["NotStarted", "InProgress", "Delayed", "Completed"];
    const normalizedStatus = status || "NotStarted";

    if (!validStatuses.includes(normalizedStatus)) {
      throw new Error(
        `Invalid status '${normalizedStatus}'. Must be one of: ${validStatuses.join(
          ", "
        )}`
      );
    }

    return normalizedStatus;
  }

  /**
   * Validate state transition
   * NotStarted -> InProgress, Delayed
   * InProgress -> Delayed, Completed
   * Delayed -> InProgress, Completed
   * Completed -> (terminal state)
   */
  canTransitionTo(newStatus) {
    const validTransitions = {
      NotStarted: ["InProgress", "Delayed"],
      InProgress: ["Delayed", "Completed"],
      Delayed: ["InProgress", "Completed"],
      Completed: [],
    };

    const allowedTransitions = validTransitions[this.status] || [];
    return allowedTransitions.includes(newStatus);
  }

  /**
   * Transition to a new status with validation
   */
  transitionTo(newStatus) {
    if (!this.canTransitionTo(newStatus)) {
      const validTransitions = {
        NotStarted: ["InProgress", "Delayed"],
        InProgress: ["Delayed", "Completed"],
        Delayed: ["InProgress", "Completed"],
        Completed: [],
      };
      const allowed = validTransitions[this.status] || [];
      throw new Error(
        `Cannot transition from '${
          this.status
        }' to '${newStatus}'. Valid transitions: ${
          allowed.join(", ") || "none"
        }`
      );
    }
    this.status = this.validateStatus(newStatus);
    this.updatedAt = new Date();
  }

  /**
   * Check if actual dock differs from planned dock (warning condition)
   */
  isDockChanged() {
    return (
      this.actualDockId &&
      this.plannedDockId &&
      this.actualDockId !== this.plannedDockId
    );
  }

  /**
   * Check if vessel is delayed (actual arrival > planned arrival)
   */
  isDelayed() {
    if (!this.actualArrivalTime || !this.plannedArrivalTime) {
      return false;
    }
    return new Date(this.actualArrivalTime) > new Date(this.plannedArrivalTime);
  }

  /**
   * Convert to database format
   */
  toDatabase() {
    return {
      Id: this.id,
      VvnId: this.vvnId,
      OperationPlanId: this.operationPlanId,
      VVEDate: this.vveDate,
      PlannedDockId: this.plannedDockId,
      PlannedArrivalTime: this.plannedArrivalTime,
      PlannedDepartureTime: this.plannedDepartureTime,
      ActualDockId: this.actualDockId,
      ActualArrivalTime: this.actualArrivalTime,
      ActualDepartureTime: this.actualDepartureTime,
      Status: this.status,
      CreatedAt: this.createdAt,
      UpdatedAt: this.updatedAt,
    };
  }

  /**
   * Create from database row
   */
  static fromDatabase(row) {
    return new VesselVisitExecution({
      id: row.Id,
      vvnId: row.VvnId,
      operationPlanId: row.OperationPlanId,
      vveDate: row.VVEDate,
      plannedDockId: row.PlannedDockId,
      plannedArrivalTime: row.PlannedArrivalTime,
      plannedDepartureTime: row.PlannedDepartureTime,
      actualDockId: row.ActualDockId,
      actualArrivalTime: row.ActualArrivalTime,
      actualDepartureTime: row.ActualDepartureTime,
      status: row.Status || "NotStarted",
      createdAt: row.CreatedAt,
      updatedAt: row.UpdatedAt,
    });
  }
}

module.exports = VesselVisitExecution;
