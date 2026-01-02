const vveRepository = require("../infrastructure/VesselVisitExecutionRepository");
const operationPlanRepository = require("../infrastructure/OperationPlanRepository");
const VesselVisitExecution = require("../domain/VesselVisitExecution/VesselVisitExecution");
const backendApiClient = require("./BackendApiClient");
const logger = require("../utils/logger");

class VesselVisitExecutionService {
  /**
   * Initialize database (ensure table exists)
   */
  async initializeAsync() {
    await vveRepository.ensureTableExists();
  }

  /**
   * Prepare today's VVEs
   * Creates NotStarted VVE placeholders for all VVNs in today's OperationPlan
   * Business Rule: Only for planDate = today
   */
  async prepareTodaysVVEsAsync(userId) {
    try {
      const today = new Date();
      const todayStr = today.toISOString().split("T")[0]; // YYYY-MM-DD

      logger.info(`Preparing VVEs for date: ${todayStr}`);

      // Get today's operation plan
      const operationPlan = await operationPlanRepository.getByDateAsync(
        todayStr
      );

      if (!operationPlan) {
        return {
          success: false,
          error: `No operation plan found for today (${todayStr})`,
        };
      }

      if (
        !operationPlan.assignments ||
        operationPlan.assignments.length === 0
      ) {
        return {
          success: true,
          message: "No assignments in today's operation plan",
          created: 0,
          skipped: 0,
          errors: 0,
        };
      }

      // Get VVN details from Backend API for each assignment
      const vvesToCreate = [];
      const skippedVvns = [];

      for (const assignment of operationPlan.assignments) {
        try {
          // Check if VVE already exists
          const existingVve = await vveRepository.getByVvnIdAsync(
            assignment.vvnId
          );
          if (existingVve) {
            logger.info(
              `VVE already exists for VVN ${assignment.vvnId}, skipping`
            );
            skippedVvns.push({
              vvnId: assignment.vvnId,
              reason: "VVE already exists",
            });
            continue;
          }

          // Create VVE with NotStarted status
          const vve = new VesselVisitExecution({
            vvnId: assignment.vvnId,
            operationPlanId: operationPlan.id,
            vveDate: todayStr,
            plannedDockId: assignment.dockId,
            plannedArrivalTime: assignment.eta,
            plannedDepartureTime: assignment.etd,
            status: "NotStarted",
            // Actual fields remain null
            actualDockId: null,
            actualArrivalTime: null,
            actualDepartureTime: null,
          });

          vvesToCreate.push(vve);
        } catch (error) {
          logger.error(
            `Error preparing VVE for VVN ${assignment.vvnId}:`,
            error
          );
          skippedVvns.push({
            vvnId: assignment.vvnId,
            reason: error.message,
          });
        }
      }

      // Bulk create VVEs
      const createResults = await vveRepository.bulkCreateAsync(vvesToCreate);

      const created = createResults.filter((r) => r.success).length;
      const errors = createResults.filter((r) => !r.success).length;

      logger.info(
        `VVE preparation complete: ${created} created, ${skippedVvns.length} skipped, ${errors} errors`
      );

      return {
        success: true,
        message: `Prepared ${created} VVEs for today (${todayStr})`,
        operationPlanId: operationPlan.id,
        date: todayStr,
        created,
        skipped: skippedVvns.length,
        errors,
        details: {
          createdVves: createResults
            .filter((r) => r.success)
            .map((r) => ({
              id: r.vve.id,
              vvnId: r.vve.vvnId,
              status: r.vve.status,
            })),
          skippedVvns,
          failedVves: createResults.filter((r) => !r.success),
        },
      };
    } catch (error) {
      logger.error("Error preparing today's VVEs:", error);
      return {
        success: false,
        error: error.message || error.toString(),
      };
    }
  }

  /**
   * Create a single VVE manually
   */
  async createVVEAsync(vveData, userId) {
    try {
      // Check if VVE already exists
      const existingVve = await vveRepository.getByVvnIdAsync(vveData.vvnId);
      if (existingVve) {
        throw new Error(`VVE already exists for VVN ${vveData.vvnId}`);
      }

      const vve = new VesselVisitExecution(vveData);
      const savedVve = await vveRepository.createAsync(vve);

      logger.info(
        `VVE created manually: ${savedVve.id} for VVN ${savedVve.vvnId}`
      );

      return {
        success: true,
        data: savedVve,
        message: "VVE created successfully",
      };
    } catch (error) {
      logger.error("Error creating VVE:", error);
      return {
        success: false,
        error: error.message || error.toString(),
      };
    }
  }

  /**
   * Get VVE by ID
   */
  async getVVEByIdAsync(id) {
    try {
      const vve = await vveRepository.getByIdAsync(id);

      if (!vve) {
        return {
          success: false,
          error: "VVE not found",
        };
      }

      return {
        success: true,
        data: vve,
      };
    } catch (error) {
      logger.error(`Error fetching VVE ${id}:`, error);
      return {
        success: false,
        error: error.message,
      };
    }
  }

  /**
   * Get VVE by VVN ID
   */
  async getVVEByVvnIdAsync(vvnId) {
    try {
      const vve = await vveRepository.getByVvnIdAsync(vvnId);

      if (!vve) {
        return {
          success: false,
          error: "VVE not found for this VVN",
        };
      }

      return {
        success: true,
        data: vve,
      };
    } catch (error) {
      logger.error(`Error fetching VVE for VVN ${vvnId}:`, error);
      return {
        success: false,
        error: error.message,
      };
    }
  }

  /**
   * Get VVEs by date
   */
  async getVVEsByDateAsync(date) {
    try {
      const vves = await vveRepository.getByDateAsync(date);

      return {
        success: true,
        data: vves,
        total: vves.length,
      };
    } catch (error) {
      logger.error(`Error fetching VVEs for date ${date}:`, error);
      return {
        success: false,
        error: error.message,
      };
    }
  }

  /**
   * Search VVEs with filters
   */
  async searchVVEsAsync(filters) {
    try {
      const vves = await vveRepository.searchAsync(filters);

      return {
        success: true,
        data: vves,
        total: vves.length,
      };
    } catch (error) {
      logger.error("Error searching VVEs:", error);
      return {
        success: false,
        error: error.message,
      };
    }
  }

  /**
   * Update VVE with berth time and dock (US 4.1.8)
   */
  async updateBerthAsync(id, berthData, userId) {
    try {
      const vve = await vveRepository.getByIdAsync(id);

      if (!vve) {
        return {
          success: false,
          error: "VVE not found",
        };
      }

      // Update berth information
      if (berthData.actualArrivalTime) {
        vve.actualArrivalTime = berthData.actualArrivalTime;
      }
      if (berthData.actualDockId) {
        vve.actualDockId = berthData.actualDockId;
      }
      if (berthData.actualDepartureTime) {
        vve.actualDepartureTime = berthData.actualDepartureTime;
      }

      // Auto-transition to InProgress if coming from NotStarted
      if (vve.status === "NotStarted" && vve.actualArrivalTime) {
        vve.transitionTo("InProgress");
      }

      // Auto-transition from Delayed to InProgress if arrival time is corrected to on-time
      if (
        vve.status === "Delayed" &&
        vve.actualArrivalTime &&
        !vve.isDelayed()
      ) {
        vve.transitionTo("InProgress");
      }

      // Check for dock change warning
      const warnings = [];
      if (vve.isDockChanged()) {
        warnings.push(
          `Warning: Actual dock (${vve.actualDockId}) differs from planned dock (${vve.plannedDockId})`
        );
      }

      // Check for delay
      if (vve.isDelayed()) {
        warnings.push(
          `Warning: Vessel arrived late (planned: ${vve.plannedArrivalTime}, actual: ${vve.actualArrivalTime})`
        );
      }

      vve.updatedAt = new Date();
      const updatedVve = await vveRepository.updateAsync(vve);

      // Update operation plan status based on VVEs
      const operationPlanService = require("./OperationPlanService");
      await operationPlanService.updateStatusFromVVEsAsync(vve.operationPlanId);

      logger.info(`VVE berth updated: ${id}`);

      return {
        success: true,
        data: updatedVve,
        warnings,
        message: "Berth information updated successfully",
      };
    } catch (error) {
      logger.error(`Error updating VVE berth ${id}:`, error);
      return {
        success: false,
        error: error.message,
      };
    }
  }

  /**
   * Update VVE status
   */
  async updateStatusAsync(id, newStatus, userId) {
    try {
      const vve = await vveRepository.getByIdAsync(id);

      if (!vve) {
        return {
          success: false,
          error: "VVE not found",
        };
      }

      // Validate and perform transition
      if (!vve.canTransitionTo(newStatus)) {
        const validTransitions = {
          NotStarted: ["InProgress", "Delayed"],
          InProgress: ["Delayed", "Completed"],
          Delayed: ["InProgress", "Completed"],
          Completed: [],
        };
        const allowed = validTransitions[vve.status] || [];
        return {
          success: false,
          error: `Cannot transition from '${
            vve.status
          }' to '${newStatus}'. Valid transitions: ${
            allowed.join(", ") || "none"
          }`,
        };
      }

      vve.transitionTo(newStatus);

      const updatedVve = await vveRepository.updateAsync(vve);

      // Update operation plan status based on VVEs
      const operationPlanService = require("./OperationPlanService");
      await operationPlanService.updateStatusFromVVEsAsync(vve.operationPlanId);

      logger.info(`VVE ${id} status updated to ${newStatus}`);

      return {
        success: true,
        data: updatedVve,
        message: `VVE status updated to ${newStatus}`,
      };
    } catch (error) {
      logger.error(`Error updating VVE status ${id}:`, error);
      return {
        success: false,
        error: error.message,
      };
    }
  }

  /**
   * Delete VVE
   */
  async deleteVVEAsync(id, userId) {
    try {
      const vve = await vveRepository.getByIdAsync(id);

      if (!vve) {
        return {
          success: false,
          error: "VVE not found",
        };
      }

      await vveRepository.deleteAsync(id);

      logger.info(`VVE deleted: ${id}`);

      return {
        success: true,
        message: "VVE deleted successfully",
      };
    } catch (error) {
      logger.error(`Error deleting VVE ${id}:`, error);
      return {
        success: false,
        error: error.message,
      };
    }
  }
}

module.exports = new VesselVisitExecutionService();
