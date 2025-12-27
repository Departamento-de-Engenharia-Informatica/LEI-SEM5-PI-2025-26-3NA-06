const OperationPlan = require("../domain/OperationPlan/OperationPlan");
const Assignment = require("../domain/OperationPlan/Assignment");
const UpsertOperationPlanDto = require("./UpsertOperationPlanDto");
const OperationPlanResponseDto = require("./OperationPlanResponseDto");

/**
 * Mapper between DTOs and Domain entities
 */
class OperationPlanMapper {
  /**
   * Convert UpsertOperationPlanDto to OperationPlan domain entity
   */
  static toDomain(dto) {
    const assignments = dto.assignments.map(
      (a) =>
        new Assignment({
          vvnId: a.vvnId,
          dockId: a.dockId,
          eta: a.eta,
          etd: a.etd,
          estimatedTeu: a.estimatedTeu || 0,
        })
    );

    // Use the isFeasible and warnings from the DTO (already calculated by Schedule module)
    return new OperationPlan({
      planDate: dto.planDate,
      isFeasible: dto.isFeasible,
      warnings: dto.warnings,
      assignments,
    });
  }

  /**
   * Convert OperationPlan domain entity to OperationPlanResponseDto
   */
  static toResponseDto(operationPlan) {
    return OperationPlanResponseDto.fromDomain(operationPlan);
  }

  /**
   * Convert database row to OperationPlan domain entity
   */
  static fromDatabase(row) {
    return OperationPlan.fromDatabase(row);
  }

  /**
   * Convert request body to UpsertOperationPlanDto
   */
  static fromRequest(requestBody) {
    return UpsertOperationPlanDto.fromRequest(requestBody);
  }
}

module.exports = OperationPlanMapper;
