/**
 * DTO for Operation Plan response
 */
class OperationPlanResponseDto {
  constructor({
    id,
    planDate,
    isFeasible,
    warnings,
    assignments,
    algorithm,
    creationDate,
    author,
  }) {
    this.id = id;
    this.planDate = planDate;
    this.isFeasible = isFeasible;
    this.warnings = warnings;
    this.assignments = assignments;
    this.algorithm = algorithm;
    this.creationDate = creationDate;
    this.author = author;
  }

  /**
   * Create from domain entity
   */
  static fromDomain(operationPlan) {
    return new OperationPlanResponseDto({
      id: operationPlan.id,
      planDate: operationPlan.planDate,
      isFeasible: operationPlan.isFeasible,
      warnings: operationPlan.warnings,
      assignments: operationPlan.assignments.map((a) => ({
        vvnId: a.vvnId,
        dockId: a.dockId,
        eta: a.eta instanceof Date ? a.eta.toISOString() : a.eta,
        etd: a.etd instanceof Date ? a.etd.toISOString() : a.etd,
        estimatedTeu: a.estimatedTeu,
      })),
      algorithm: operationPlan.algorithm,
      creationDate: operationPlan.creationDate,
      author: operationPlan.author,
    });
  }
}

module.exports = OperationPlanResponseDto;
