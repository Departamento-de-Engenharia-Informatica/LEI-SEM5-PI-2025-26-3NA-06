const operationPlanRepository = require("../infrastructure/OperationPlanRepository");
const vesselVisitExecutionRepository = require("../infrastructure/VesselVisitExecutionRepository");
const OperationPlanMapper = require("../dtos/OperationPlanMapper");
const UpsertOperationPlanDto = require("../dtos/UpsertOperationPlanDto");
const backendApiClient = require("./BackendApiClient");
const logger = require("../utils/logger");

class OperationPlanService {
  /**
   * Initialize database (ensure table exists)
   */
  async initializeAsync() {
    await operationPlanRepository.ensureTableExists();
  }

  /**
   * Create a new operation plan
   * Business logic: validates feasibility before saving
   */
  async createOperationPlanAsync(requestBody, userId, username) {
    try {
      // Convert request to DTO
      const dto = OperationPlanMapper.fromRequest(requestBody);
      dto.validate();

      // Check if plan already exists for this date
      const existingPlan = await operationPlanRepository.getByDateAsync(
        dto.planDate
      );
      if (existingPlan) {
        throw new Error(
          `Operation plan already exists for date ${dto.planDate}`
        );
      }

      // Convert DTO to domain entity (uses isFeasible and warnings from Schedule module)
      const operationPlan = OperationPlanMapper.toDomain(dto);

      // Set audit metadata
      operationPlan.algorithm = "FIFO";
      operationPlan.creationDate = new Date();
      operationPlan.author = username || "Unknown";

      logger.info(
        `Saving operation plan for ${dto.planDate}: isFeasible=${operationPlan.isFeasible}, warnings=${operationPlan.warnings.length}, author=${operationPlan.author}`
      );

      // Persist to database (allow saving even with warnings/conflicts)
      const savedPlan = await operationPlanRepository.createAsync(
        operationPlan
      );

      logger.info(`Operation plan created successfully for ${dto.planDate}`);

      // Convert to response DTO
      const responseDto = OperationPlanMapper.toResponseDto(savedPlan);

      return {
        success: true,
        data: responseDto,
        message: "Operation plan created successfully",
      };
    } catch (error) {
      logger.error("Error creating operation plan:", error);
      return {
        success: false,
        error: error.message || error.toString() || "Unknown error occurred",
      };
    }
  }

  /**
   * Get operation plan by ID
   */
  async getOperationPlanByIdAsync(id) {
    try {
      const plan = await operationPlanRepository.getByIdAsync(id);

      if (!plan) {
        return {
          success: false,
          error: "Operation plan not found",
        };
      }

      // Enrich assignments with vessel and dock information from Backend
      plan.assignments = await backendApiClient.enrichAssignmentsAsync(
        plan.assignments
      );

      const responseDto = OperationPlanMapper.toResponseDto(plan);

      return {
        success: true,
        data: responseDto,
      };
    } catch (error) {
      logger.error(`Error fetching operation plan ${id}:`, error);
      return {
        success: false,
        error: error.message,
      };
    }
  }

  /**
   * Get operation plan by date
   */
  async getOperationPlanByDateAsync(planDate) {
    try {
      const plan = await operationPlanRepository.getByDateAsync(planDate);

      if (!plan) {
        return {
          success: false,
          error: "Operation plan not found for this date",
        };
      }

      // Enrich assignments with vessel and dock information from Backend
      plan.assignments = await backendApiClient.enrichAssignmentsAsync(
        plan.assignments
      );

      const responseDto = OperationPlanMapper.toResponseDto(plan);

      return {
        success: true,
        data: responseDto,
      };
    } catch (error) {
      logger.error(
        `Error fetching operation plan for date ${planDate}:`,
        error
      );
      return {
        success: false,
        error: error.message,
      };
    }
  }

  /**
   * Search operation plans with filters
   */
  async searchOperationPlansAsync(filters) {
    try {
      const plans = await operationPlanRepository.searchAsync(filters);

      // Enrich all plans with vessel and dock information from Backend
      for (const plan of plans) {
        plan.assignments = await backendApiClient.enrichAssignmentsAsync(
          plan.assignments
        );
      }

      const responseDtos = plans.map((p) =>
        OperationPlanMapper.toResponseDto(p)
      );

      return {
        success: true,
        data: responseDtos,
        total: responseDtos.length,
      };
    } catch (error) {
      logger.error("Error searching operation plans:", error);
      return {
        success: false,
        error: error.message,
      };
    }
  }

  /**
   * Replace operation plan by date (delete existing and create new one)
   */
  async replaceOperationPlanByDateAsync(requestBody, userId, username) {
    try {
      // Convert request to DTO
      const dto = OperationPlanMapper.fromRequest(requestBody);
      dto.validate();

      // Check if plan exists for this date
      const existingPlan = await operationPlanRepository.getByDateAsync(
        dto.planDate
      );

      if (existingPlan) {
        // Delete all associated VVEs first
        try {
          await vesselVisitExecutionRepository.deleteByOperationPlanIdAsync(
            existingPlan.id
          );
          logger.info(
            `Deleted VVEs associated with operation plan ${existingPlan.id} before replacement`
          );
        } catch (vveError) {
          logger.error(
            "Error deleting associated VVEs during replacement:",
            vveError
          );
          // Continue with operation plan replacement even if VVE deletion fails
        }

        // Delete existing plan
        await operationPlanRepository.deleteAsync(existingPlan.planDate);
        logger.info(
          `Deleted existing operation plan for ${dto.planDate} before replacement`
        );
      }

      // Convert DTO to domain entity
      const operationPlan = OperationPlanMapper.toDomain(dto);

      // Set audit metadata
      operationPlan.algorithm = "FIFO";
      operationPlan.creationDate = new Date();
      operationPlan.author = username || "Unknown";

      logger.info(
        `Replacing operation plan for ${dto.planDate}: isFeasible=${operationPlan.isFeasible}, warnings=${operationPlan.warnings.length}, author=${operationPlan.author}`
      );

      // Persist to database
      const savedPlan = await operationPlanRepository.createAsync(
        operationPlan
      );

      logger.info(`Operation plan replaced successfully for ${dto.planDate}`);

      // Convert to response DTO
      const responseDto = OperationPlanMapper.toResponseDto(savedPlan);

      return {
        success: true,
        data: responseDto,
        message: "Operation plan and associated VVEs replaced successfully",
      };
    } catch (error) {
      logger.error("Error replacing operation plan:", error);
      return {
        success: false,
        error: error.message || error.toString() || "Unknown error occurred",
      };
    }
  }

  /**
   * Update operation plan
   */
  async updateOperationPlanAsync(id, requestBody, userId) {
    try {
      const existingPlan = await operationPlanRepository.getByIdAsync(id);

      if (!existingPlan) {
        return {
          success: false,
          error: "Operation plan not found",
        };
      }

      // Convert request to DTO
      const dto = OperationPlanMapper.fromRequest(requestBody);
      dto.validate();

      // Update fields
      if (dto.assignments) {
        existingPlan.assignments = dto.assignments.map((a) => {
          const Assignment = require("../domain/OperationPlan/Assignment");
          return new Assignment(a);
        });
      }
      if (dto.isFeasible !== undefined) {
        existingPlan.isFeasible = dto.isFeasible;
      }
      if (dto.warnings) {
        existingPlan.warnings = dto.warnings;
      }

      // Validate
      existingPlan.validate();

      // Check feasibility
      const feasibility = existingPlan.checkFeasibility();

      // Update in database
      const updatedPlan = await operationPlanRepository.updateAsync(
        existingPlan
      );

      logger.info(`Operation plan ${id} updated`);

      const responseDto = OperationPlanMapper.toResponseDto(updatedPlan);

      return {
        success: true,
        data: responseDto,
        message: "Operation plan updated successfully",
      };
    } catch (error) {
      logger.error(`Error updating operation plan ${id}:`, error);
      return {
        success: false,
        error: error.message,
      };
    }
  }

  /**
   * Delete operation plan
   */
  async deleteOperationPlanAsync(planDate, userId) {
    try {
      const plan = await operationPlanRepository.getByDateAsync(planDate);

      if (!plan) {
        return {
          success: false,
          error: "Operation plan not found",
        };
      }

      // Delete all associated VVEs first (cascading delete)
      try {
        await vesselVisitExecutionRepository.deleteByOperationPlanIdAsync(
          plan.id
        );
        logger.info(`Deleted VVEs associated with operation plan ${plan.id}`);
      } catch (vveError) {
        logger.error("Error deleting associated VVEs:", vveError);
        // Continue with operation plan deletion even if VVE deletion fails
      }

      // Delete the operation plan
      await operationPlanRepository.deleteAsync(planDate);

      logger.info(`Operation plan for date ${planDate} deleted`);

      return {
        success: true,
        message: "Operation plan and associated VVEs deleted successfully",
      };
    } catch (error) {
      logger.error(
        `Error deleting operation plan for date ${planDate}:`,
        error
      );
      return {
        success: false,
        error: error.message,
      };
    }
  }

  /**
   * Validate if a plan is feasible (without saving)
   */
  async validateFeasibilityAsync(requestBody) {
    try {
      const dto = OperationPlanMapper.fromRequest(requestBody);
      dto.validate();

      const operationPlan = OperationPlanMapper.toDomain(dto);

      const feasibility = operationPlan.checkFeasibility();

      return {
        success: true,
        data: {
          isFeasible: feasibility.isFeasible,
          warnings: feasibility.warnings,
          totalAssignments: operationPlan.getTotalAssignments(),
        },
      };
    } catch (error) {
      logger.error("Error validating feasibility:", error);
      return {
        success: false,
        error: error.message,
      };
    }
  }

  /**
   * Update operation plan status with validation
   * Valid transitions:
   * - Pending -> InProgress, Cancelled
   * - InProgress -> Completed, Cancelled
   * - Completed -> (terminal state)
   * - Cancelled -> (terminal state)
   */
  async updateOperationPlanStatusAsync(id, newStatus, userId) {
    try {
      const plan = await operationPlanRepository.getByIdAsync(id);

      if (!plan) {
        return {
          success: false,
          error: "Operation plan not found",
        };
      }

      // Validate state transition
      if (!plan.canTransitionTo(newStatus)) {
        const validTransitions = {
          Pending: ["InProgress", "Cancelled"],
          InProgress: ["Completed", "Cancelled"],
          Completed: [],
          Cancelled: [],
        };
        const allowed = validTransitions[plan.status] || [];
        return {
          success: false,
          error: `Cannot transition from '${
            plan.status
          }' to '${newStatus}'. Valid transitions: ${
            allowed.join(", ") || "none"
          }`,
        };
      }

      // Perform transition
      plan.transitionTo(newStatus);

      // Update in database
      const updatedPlan = await operationPlanRepository.updateAsync(plan);

      logger.info(
        `Operation plan ${id} status updated from ${plan.status} to ${newStatus}`
      );

      const responseDto = OperationPlanMapper.toResponseDto(updatedPlan);

      return {
        success: true,
        data: responseDto,
        message: `Operation plan status updated to ${newStatus}`,
      };
    } catch (error) {
      logger.error(`Error updating operation plan status ${id}:`, error);
      return {
        success: false,
        error: error.message,
      };
    }
  }

  /**
   * Update OperationPlan status based on its VVEs
   * Called when VVEs are updated
   */
  async updateStatusFromVVEsAsync(operationPlanId) {
    try {
      const plan = await operationPlanRepository.getByIdAsync(operationPlanId);

      if (!plan) {
        logger.warn(
          `Operation plan ${operationPlanId} not found for status update`
        );
        return {
          success: false,
          error: "Operation plan not found",
        };
      }

      // Get all VVEs for this operation plan
      const vves =
        await vesselVisitExecutionRepository.getByOperationPlanIdAsync(
          operationPlanId
        );

      // Calculate new status from VVEs
      const OperationPlan = require("../domain/OperationPlan/OperationPlan");
      const newStatus = OperationPlan.calculateStatusFromVVEs(vves);

      // Only update if status changed
      if (plan.status !== newStatus) {
        plan.status = newStatus;
        await operationPlanRepository.updateAsync(plan);
        logger.info(
          `Operation plan ${operationPlanId} status updated to ${newStatus} based on VVEs`
        );
      }

      return {
        success: true,
        status: newStatus,
      };
    } catch (error) {
      logger.error(`Error updating operation plan status from VVEs:`, error);
      return {
        success: false,
        error: error.message,
      };
    }
  }
}

module.exports = new OperationPlanService();
