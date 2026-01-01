/**
 * DTO for creating VVE
 */
class CreateVesselVisitExecutionDto {
  constructor({
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
  }) {
    this.vvnId = vvnId;
    this.operationPlanId = operationPlanId;
    this.vveDate = vveDate;
    this.plannedDockId = plannedDockId;
    this.plannedArrivalTime = plannedArrivalTime;
    this.plannedDepartureTime = plannedDepartureTime;
    this.actualDockId = actualDockId;
    this.actualArrivalTime = actualArrivalTime;
    this.actualDepartureTime = actualDepartureTime;
    this.status = status;
  }

  /**
   * Validate DTO
   */
  validate() {
    const errors = [];

    if (!this.vvnId) {
      errors.push("vvnId is required");
    }

    if (!this.vveDate) {
      errors.push("vveDate is required");
    }

    if (!this.plannedDockId) {
      errors.push("plannedDockId is required");
    }

    if (!this.plannedArrivalTime) {
      errors.push("plannedArrivalTime is required");
    }

    if (!this.plannedDepartureTime) {
      errors.push("plannedDepartureTime is required");
    }

    if (errors.length > 0) {
      throw new Error(`DTO validation failed: ${errors.join(", ")}`);
    }

    return true;
  }

  /**
   * Create from request body
   */
  static fromRequest(requestBody) {
    return new CreateVesselVisitExecutionDto({
      vvnId: requestBody.vvnId,
      operationPlanId: requestBody.operationPlanId,
      vveDate: requestBody.vveDate,
      plannedDockId: requestBody.plannedDockId,
      plannedArrivalTime: requestBody.plannedArrivalTime,
      plannedDepartureTime: requestBody.plannedDepartureTime,
      actualDockId: requestBody.actualDockId,
      actualArrivalTime: requestBody.actualArrivalTime,
      actualDepartureTime: requestBody.actualDepartureTime,
      status: requestBody.status || "NotStarted",
    });
  }
}

module.exports = CreateVesselVisitExecutionDto;
