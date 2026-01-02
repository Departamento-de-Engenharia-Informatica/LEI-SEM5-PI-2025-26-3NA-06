const { v4: uuidv4, validate: uuidValidate } = require("uuid");
const Assignment = require("./Assignment");

/**
 * Operation Plan Domain Entity
 * Represents a daily scheduling plan for vessel operations
 */
class OperationPlan {
  constructor({
    id = null,
    planDate,
    status = "NotStarted",
    isFeasible = true,
    warnings = [],
    assignments = [],
    algorithm = "FIFO",
    creationDate = null,
    author = null,
  }) {
    this.id = id || uuidv4();
    // Normalize planDate to YYYY-MM-DD string format
    this.planDate = this.normalizePlanDate(planDate);
    this.status = this.validateStatus(status);
    this.isFeasible = isFeasible;
    this.warnings = Array.isArray(warnings) ? warnings : [];
    this.assignments = assignments.map((a) =>
      a instanceof Assignment ? a : new Assignment(a)
    );
    this.algorithm = algorithm || "FIFO";
    this.creationDate = creationDate || new Date();
    this.author = author;

    this.validate();
  }

  /**
   * Normalize planDate to YYYY-MM-DD string format
   */
  normalizePlanDate(planDate) {
    if (!planDate) {
      return null;
    }

    // If already a string in YYYY-MM-DD format, return as is
    if (typeof planDate === "string" && /^\d{4}-\d{2}-\d{2}$/.test(planDate)) {
      return planDate;
    }

    // If it's a Date object or other format, convert to YYYY-MM-DD
    const date = new Date(planDate);
    if (isNaN(date.getTime())) {
      return planDate; // Return as is, validation will catch it
    }

    // Format as YYYY-MM-DD
    const year = date.getFullYear();
    const month = String(date.getMonth() + 1).padStart(2, "0");
    const day = String(date.getDate()).padStart(2, "0");
    return `${year}-${month}-${day}`;
  }

  /**
   * Validate and normalize status
   * Valid states: NotStarted, InProgress, Finished
   */
  validateStatus(status) {
    const validStatuses = ["NotStarted", "InProgress", "Finished"];
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
   * Pending -> InProgress, Cancelled
   * InProgress -> Completed, Cancelled
   * Completed -> (terminal state)
   * Cancelled -> (terminal state)
   */
  canTransitionTo(newStatus) {
    const validTransitions = {
      Pending: ["InProgress", "Cancelled"],
      InProgress: ["Completed", "Cancelled"],
      Completed: [],
      Cancelled: [],
    };

    const allowedTransitions = validTransitions[this.status] || [];
    return allowedTransitions.includes(newStatus);
  }

  /**
   * Transition to a new status with validation
   */
  transitionTo(newStatus) {
    if (!this.canTransitionTo(newStatus)) {
      throw new Error(
        `Cannot transition from '${
          this.status
        }' to '${newStatus}'. Valid transitions: ${this.canTransitionTo.toString()}`
      );
    }
    this.status = this.validateStatus(newStatus);
  }

  validate() {
    const errors = [];

    if (!this.planDate) {
      errors.push("planDate is required");
    }

    if (!uuidValidate(this.id)) {
      errors.push("Invalid ID format (must be UUID)");
    }

    if (errors.length > 0) {
      throw new Error(`Operation Plan validation failed: ${errors.join(", ")}`);
    }
  }

  /**
   * Business rule: Check if the operation plan is feasible
   * A plan is feasible if no time conflicts exist between assignments on the same dock
   */
  checkFeasibility() {
    if (!this.assignments || this.assignments.length === 0) {
      this.isFeasible = true;
      this.warnings = [];
      return { isFeasible: true, warnings: [] };
    }

    const conflicts = this.detectConflicts();
    this.isFeasible = conflicts.length === 0;

    if (!this.isFeasible) {
      this.warnings = conflicts;
    } else {
      this.warnings = [];
    }

    return { isFeasible: this.isFeasible, warnings: this.warnings };
  }

  /**
   * Detect conflicts between assignments on the same dock
   */
  detectConflicts() {
    const conflicts = [];

    // Group assignments by dock
    const assignmentsByDock = new Map();
    for (const assignment of this.assignments) {
      if (!assignmentsByDock.has(assignment.dockId)) {
        assignmentsByDock.set(assignment.dockId, []);
      }
      assignmentsByDock.get(assignment.dockId).push(assignment);
    }

    // Check for conflicts within each dock
    for (const [dockId, dockAssignments] of assignmentsByDock) {
      for (let i = 0; i < dockAssignments.length; i++) {
        for (let j = i + 1; j < dockAssignments.length; j++) {
          const a1 = dockAssignments[i];
          const a2 = dockAssignments[j];

          if (a1.overlapsWith(a2)) {
            const vessel1 = a1.vesselName
              ? `${a1.vesselName} (${a1.vesselImo || "Unknown IMO"})`
              : a1.vesselImo || "Unknown Vessel";
            const vessel2 = a2.vesselName
              ? `${a2.vesselName} (${a2.vesselImo || "Unknown IMO"})`
              : a2.vesselImo || "Unknown Vessel";

            const formatTime = (date) => {
              const d = new Date(date);
              return `${d.getHours().toString().padStart(2, "0")}:${d
                .getMinutes()
                .toString()
                .padStart(2, "0")}`;
            };

            conflicts.push(
              `[Dock ${dockId.substring(0, 8)}...]: VVN from ${formatTime(
                a1.eta
              )} to ${formatTime(
                a1.etd
              )} (${vessel1}) has conflict with VVN from ${formatTime(
                a2.eta
              )} to ${formatTime(a2.etd)} (${vessel2})`
            );
          }
        }
      }
    }

    return conflicts;
  }

  /**
   * Get total number of vessel assignments
   */
  getTotalAssignments() {
    return this.assignments.length;
  }

  /**
   * Calculate status based on VVE statuses
   * NotStarted - if no VVE has started
   * InProgress - if at least 1 VVE has started (InProgress, Delayed, or Completed)
   * Finished - if all VVEs are Completed
   */
  static calculateStatusFromVVEs(vves) {
    if (!vves || vves.length === 0) {
      return "NotStarted";
    }

    const startedStatuses = ["InProgress", "Delayed", "Completed"];
    const startedCount = vves.filter((vve) =>
      startedStatuses.includes(vve.status)
    ).length;
    const completedCount = vves.filter(
      (vve) => vve.status === "Completed"
    ).length;

    // All VVEs completed
    if (completedCount === vves.length) {
      return "Finished";
    }

    // At least one VVE started
    if (startedCount > 0) {
      return "InProgress";
    }

    // No VVEs started yet
    return "NotStarted";
  }

  /**
   * Convert to database format
   */
  toDatabase() {
    return {
      Id: this.id,
      PlanDate: this.planDate,
      Status: this.status,
      IsFeasible: this.isFeasible,
      Warnings: JSON.stringify(this.warnings),
      Assignments: JSON.stringify(this.assignments.map((a) => a.toJSON())),
      Algorithm: this.algorithm,
      CreationDate: this.creationDate,
      Author: this.author,
    };
  }

  /**
   * Create from database row
   */
  static fromDatabase(row) {
    const assignmentsData = JSON.parse(row.Assignments || "[]");
    const Assignment = require("./Assignment");

    // Reconstruct Assignment objects to ensure all properties are properly set
    const assignments = assignmentsData.map(
      (a) =>
        new Assignment({
          vvnId: a.vvnId,
          dockId: a.dockId,
          dockName: a.dockName || null,
          eta: a.eta,
          etd: a.etd,
          estimatedTeu: a.estimatedTeu || 0,
          vesselName: a.vesselName || null,
          vesselImo: a.vesselImo || null,
        })
    );

    return new OperationPlan({
      id: row.Id,
      planDate: row.PlanDate,
      status:
        row.Status === "Pending" ? "NotStarted" : row.Status || "NotStarted",
      isFeasible: row.IsFeasible,
      warnings: JSON.parse(row.Warnings || "[]"),
      assignments: assignments,
      algorithm: row.Algorithm,
      creationDate: row.CreationDate,
      author: row.Author,
    });
  }
}

module.exports = OperationPlan;
