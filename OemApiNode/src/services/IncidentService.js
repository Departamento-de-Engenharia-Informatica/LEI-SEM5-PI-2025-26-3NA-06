const incidentRepository = require("../infrastructure/IncidentRepository");
const incidentTypeRepository = require("../infrastructure/IncidentTypeRepository");
const Incident = require("../domain/Incident/Incident");
const logger = require("../utils/logger");

class IncidentService {
  /**
   * Initialize database
   */
  async initializeAsync() {
    await incidentRepository.ensureTableExists();
  }

  /**
   * Create a new incident
   */
  async createIncidentAsync(requestBody) {
    try {
      // Validate incident type exists
      const incidentType = await incidentTypeRepository.getByIdAsync(
        requestBody.incidentTypeId
      );
      if (!incidentType) {
        return {
          success: false,
          error: "Incident type not found",
        };
      }

      if (!incidentType.isActive) {
        return {
          success: false,
          error: "Cannot use inactive incident type",
        };
      }

      // Create incident
      const incident = new Incident({
        incidentTypeId: requestBody.incidentTypeId,
        date: new Date(), // Always today
        startTime: new Date(requestBody.startTime),
        endTime: requestBody.endTime ? new Date(requestBody.endTime) : null,
        description: requestBody.description,
        affectsAllVVEs: requestBody.affectsAllVVEs || false,
        affectedVVEIds: requestBody.affectedVVEIds || [],
      });

      // Validate
      incident.validate();

      // Save to database
      const created = await incidentRepository.createAsync(incident);

      // Fetch full details including incident type
      const fullIncident = await incidentRepository.getByIdAsync(created.id);

      logger.info(`Incident created: ${fullIncident.id}`);

      return {
        success: true,
        data: fullIncident.toJSON(),
      };
    } catch (error) {
      logger.error("Error creating incident:", error);
      return {
        success: false,
        error: error.message || "Failed to create incident",
      };
    }
  }

  /**
   * Get incident by ID
   */
  async getIncidentByIdAsync(id) {
    try {
      const incident = await incidentRepository.getByIdAsync(id);

      if (!incident) {
        return {
          success: false,
          error: "Incident not found",
        };
      }

      return {
        success: true,
        data: incident.toJSON(),
      };
    } catch (error) {
      logger.error(`Error fetching incident by ID ${id}:`, error);
      return {
        success: false,
        error: error.message || "Failed to fetch incident",
      };
    }
  }

  /**
   * Get all incidents with filtering
   */
  async getAllIncidentsAsync(filters = {}) {
    try {
      const incidents = await incidentRepository.getAllAsync(filters);

      return {
        success: true,
        data: incidents.map((i) => i.toJSON()),
      };
    } catch (error) {
      logger.error("Error fetching all incidents:", error);
      return {
        success: false,
        error: error.message || "Failed to fetch incidents",
      };
    }
  }

  /**
   * Get today's active incidents
   */
  async getTodaysActiveIncidentsAsync() {
    try {
      const incidents = await incidentRepository.getTodaysActiveAsync();

      return {
        success: true,
        data: incidents.map((i) => i.toJSON()),
      };
    } catch (error) {
      logger.error("Error fetching today's active incidents:", error);
      return {
        success: false,
        error: error.message || "Failed to fetch active incidents",
      };
    }
  }

  /**
   * Update incident
   */
  async updateIncidentAsync(id, requestBody) {
    try {
      // Fetch existing incident
      const existing = await incidentRepository.getByIdAsync(id);
      if (!existing) {
        return {
          success: false,
          error: "Incident not found",
        };
      }

      // Update fields
      existing.update({
        startTime: requestBody.startTime
          ? new Date(requestBody.startTime)
          : undefined,
        endTime: requestBody.endTime
          ? new Date(requestBody.endTime)
          : undefined,
        description: requestBody.description,
        affectsAllVVEs: requestBody.affectsAllVVEs,
        affectedVVEIds: requestBody.affectedVVEIds,
      });

      // Validate
      existing.validate();

      // Save to database
      const updated = await incidentRepository.updateAsync(existing);

      // Fetch full details
      const fullIncident = await incidentRepository.getByIdAsync(updated.id);

      logger.info(`Incident updated: ${fullIncident.id}`);

      return {
        success: true,
        data: fullIncident.toJSON(),
      };
    } catch (error) {
      logger.error(`Error updating incident ${id}:`, error);
      return {
        success: false,
        error: error.message || "Failed to update incident",
      };
    }
  }

  /**
   * Delete incident (soft delete)
   */
  async deleteIncidentAsync(id) {
    try {
      const existing = await incidentRepository.getByIdAsync(id);
      if (!existing) {
        return {
          success: false,
          error: "Incident not found",
        };
      }

      const deleted = await incidentRepository.deleteAsync(id);

      logger.info(`Incident deleted: ${deleted.id}`);

      return {
        success: true,
        message: "Incident successfully deleted",
        data: deleted.toJSON(),
      };
    } catch (error) {
      logger.error(`Error deleting incident ${id}:`, error);
      return {
        success: false,
        error: error.message || "Failed to delete incident",
      };
    }
  }
}

module.exports = new IncidentService();
