/**
 * DTO for creating or updating an Operation Plan
 * Simplified structure received from frontend
 */
class UpsertOperationPlanDto {
  constructor({
    planDate,
    status = "Pending",
    isFeasible = true,
    warnings = [],
    assignments = [],
  }) {
    this.planDate = planDate;
    this.status = status;
    this.isFeasible = isFeasible;
    this.warnings = warnings;
    this.assignments = assignments;
  }

  /**
   * Validate DTO
   */
  validate() {
    const errors = [];

    if (!this.planDate) {
      errors.push("planDate is required");
    }

    if (!Array.isArray(this.assignments)) {
      errors.push("assignments must be an array");
    }

    if (!Array.isArray(this.warnings)) {
      errors.push("warnings must be an array");
    }

    // Validate each assignment
    for (const assignment of this.assignments) {
      if (!assignment.vvnId) {
        errors.push(`Assignment missing vvnId`);
      }
      if (!assignment.dockId) {
        errors.push(`Assignment missing dockId`);
      }
      if (!assignment.eta) {
        errors.push(`Assignment missing eta`);
      }
      if (!assignment.etd) {
        errors.push(`Assignment missing etd`);
      }
    }

    if (errors.length > 0) {
      throw new Error(`DTO validation failed: ${errors.join(", ")}`);
    }

    return true;
  }

  /**
   * Create from request body (handles frontend format with dockSchedules or vesselVisitNotifications)
   */
  static fromRequest(requestBody) {
    let assignments = [];

    // Handle dockSchedules format (from SchedulingApi)
    if (requestBody.dockSchedules && Array.isArray(requestBody.dockSchedules)) {
      assignments = requestBody.dockSchedules.flatMap((dockSchedule) =>
        dockSchedule.assignments.map((a) => ({
          vvnId: a.vvnId,
          dockId: a.dockId || dockSchedule.dockId,
          dockName: a.dockName || dockSchedule.dockName,
          eta: a.eta,
          etd: a.etd,
          estimatedTeu: a.estimatedTeu || 0,
          vesselName: a.vesselName,
          vesselImo: a.vesselImo,
        }))
      );
    }

    // Handle vesselVisitNotifications format (from legacy frontend)
    if (
      requestBody.vesselVisitNotifications &&
      Array.isArray(requestBody.vesselVisitNotifications)
    ) {
      assignments = requestBody.vesselVisitNotifications.map((vvn) => ({
        vvnId: vvn.id || vvn.vvnId,
        dockId: vvn.assignedDockId || vvn.dockId,
        dockName: vvn.dockName,
        eta: vvn.eta,
        etd: vvn.etd,
        estimatedTeu: vvn.estimatedTeu || 0,
        vesselName: vvn.vesselName,
        vesselImo: vvn.vesselImo,
      }));
    }

    // Handle direct assignments format
    if (requestBody.assignments && Array.isArray(requestBody.assignments)) {
      assignments = requestBody.assignments;
    }

    return new UpsertOperationPlanDto({
      planDate: requestBody.planDate || requestBody.date,
      isFeasible:
        requestBody.isFeasible !== undefined ? requestBody.isFeasible : true,
      warnings: requestBody.warnings || [],
      assignments,
    });
  }
}

module.exports = UpsertOperationPlanDto;
