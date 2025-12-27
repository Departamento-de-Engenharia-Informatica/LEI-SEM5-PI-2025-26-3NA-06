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
    isFeasible = true,
    warnings = [],
    assignments = [],
    algorithm = "FIFO",
    creationDate = null,
    author = null,
  }) {
    this.id = id || uuidv4();
    this.planDate = planDate;
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
            conflicts.push(
              `VVN ${a1.vvnId.substring(
                0,
                8
              )}... conflicts on Dock ${dockId.substring(
                0,
                8
              )}... with VVN ${a2.vvnId.substring(0, 8)}...`
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
   * Convert to database format
   */
  toDatabase() {
    return {
      Id: this.id,
      PlanDate: this.planDate,
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
    const assignments = JSON.parse(row.Assignments || "[]");

    return new OperationPlan({
      id: row.Id,
      planDate: row.PlanDate,
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
