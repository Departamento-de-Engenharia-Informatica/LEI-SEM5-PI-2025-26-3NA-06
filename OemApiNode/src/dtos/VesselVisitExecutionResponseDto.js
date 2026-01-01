/**
 * DTO for VVE response
 */
class VesselVisitExecutionResponseDto {
  constructor({
    id,
    vvnId,
    operationPlanId,
    vveDate,
    plannedDockId,
    plannedArrivalTime,
    plannedDepartureTime,
    actualDockId,
    actualArrivalTime,
    actualDepartureTime,
    status,
    createdAt,
    updatedAt,
  }) {
    this.id = id;
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
    this.createdAt = createdAt;
    this.updatedAt = updatedAt;
  }

  /**
   * Create from domain entity
   */
  static fromDomain(vve) {
    return new VesselVisitExecutionResponseDto({
      id: vve.id,
      vvnId: vve.vvnId,
      operationPlanId: vve.operationPlanId,
      vveDate: vve.vveDate,
      plannedDockId: vve.plannedDockId,
      plannedArrivalTime:
        vve.plannedArrivalTime instanceof Date
          ? vve.plannedArrivalTime.toISOString()
          : vve.plannedArrivalTime,
      plannedDepartureTime:
        vve.plannedDepartureTime instanceof Date
          ? vve.plannedDepartureTime.toISOString()
          : vve.plannedDepartureTime,
      actualDockId: vve.actualDockId,
      actualArrivalTime: vve.actualArrivalTime
        ? vve.actualArrivalTime instanceof Date
          ? vve.actualArrivalTime.toISOString()
          : vve.actualArrivalTime
        : null,
      actualDepartureTime: vve.actualDepartureTime
        ? vve.actualDepartureTime instanceof Date
          ? vve.actualDepartureTime.toISOString()
          : vve.actualDepartureTime
        : null,
      status: vve.status,
      createdAt:
        vve.createdAt instanceof Date
          ? vve.createdAt.toISOString()
          : vve.createdAt,
      updatedAt: vve.updatedAt
        ? vve.updatedAt instanceof Date
          ? vve.updatedAt.toISOString()
          : vve.updatedAt
        : null,
    });
  }
}

module.exports = VesselVisitExecutionResponseDto;
