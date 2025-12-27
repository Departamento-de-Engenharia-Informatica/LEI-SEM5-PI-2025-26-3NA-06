const axios = require("axios");
const logger = require("../utils/logger");

/**
 * Client for calling the Backend API to fetch vessel and dock information
 */
class BackendApiClient {
  constructor() {
    this.baseUrl = process.env.BACKEND_API_URL || "http://localhost:5044";
    this.timeout = 10000; // 10 seconds
  }

  /**
   * Fetch vessel information by IMO
   * @param {string} imo - Vessel IMO number
   * @returns {Promise<{name: string, imo: string} | null>}
   */
  async getVesselByImoAsync(imo) {
    try {
      const response = await axios.get(
        `${this.baseUrl}/api/Vessel/imo/${imo}`,
        {
          timeout: this.timeout,
          headers: {
            Accept: "application/json",
          },
        }
      );

      if (response.data) {
        return {
          name: response.data.name,
          imo: response.data.imo,
        };
      }

      return null;
    } catch (error) {
      if (error.response?.status === 404) {
        logger.warn(`Vessel not found for IMO: ${imo}`);
        return null;
      }
      logger.error(`Error fetching vessel by IMO ${imo}:`, error.message);
      return null;
    }
  }

  /**
   * Fetch dock information by ID
   * @param {string} dockId - Dock GUID
   * @returns {Promise<{id: string, name: string} | null>}
   */
  async getDockByIdAsync(dockId) {
    try {
      const response = await axios.get(`${this.baseUrl}/api/Dock/${dockId}`, {
        timeout: this.timeout,
        headers: {
          Accept: "application/json",
        },
      });

      if (response.data) {
        return {
          id: response.data.id,
          name: response.data.name,
        };
      }

      return null;
    } catch (error) {
      if (error.response?.status === 404) {
        logger.warn(`Dock not found for ID: ${dockId}`);
        return null;
      }
      logger.error(`Error fetching dock by ID ${dockId}:`, error.message);
      return null;
    }
  }

  /**
   * Fetch VesselVisitNotification by ID
   * @param {string} vvnId - VVN GUID
   * @returns {Promise<{vesselName: string, vesselImo: string} | null>}
   */
  async getVVNByIdAsync(vvnId) {
    try {
      const response = await axios.get(
        `${this.baseUrl}/api/VesselVisitNotification/${vvnId}`,
        {
          timeout: this.timeout,
          headers: {
            Accept: "application/json",
          },
        }
      );

      if (response.data && response.data.vessel) {
        return {
          vesselName: response.data.vessel.name,
          vesselImo: response.data.vessel.imo,
        };
      }

      return null;
    } catch (error) {
      if (error.response?.status === 404) {
        logger.warn(`VVN not found for ID: ${vvnId}`);
        return null;
      }
      logger.error(`Error fetching VVN by ID ${vvnId}:`, error.message);
      return null;
    }
  }

  /**
   * Enrich assignments with vessel and dock information from Backend
   * @param {Array} assignments - Array of Assignment objects
   * @returns {Promise<Array>} - Enriched assignments
   */
  async enrichAssignmentsAsync(assignments) {
    if (!assignments || assignments.length === 0) {
      return assignments;
    }

    logger.info(
      `Enriching ${assignments.length} assignments with Backend data...`
    );

    const enrichedAssignments = [];

    for (const assignment of assignments) {
      const enriched = { ...assignment };

      // Enrich vessel information if missing
      if (!enriched.vesselName || !enriched.vesselImo) {
        const vvnData = await this.getVVNByIdAsync(enriched.vvnId);
        if (vvnData) {
          enriched.vesselName = vvnData.vesselName;
          enriched.vesselImo = vvnData.vesselImo;
          logger.info(
            `Enriched vessel info for VVN ${enriched.vvnId}: ${vvnData.vesselName} (${vvnData.vesselImo})`
          );
        }
      }

      // Enrich dock name if missing
      if (!enriched.dockName) {
        const dockData = await this.getDockByIdAsync(enriched.dockId);
        if (dockData) {
          enriched.dockName = dockData.name;
          logger.info(
            `Enriched dock info for ${enriched.dockId}: ${dockData.name}`
          );
        }
      }

      enrichedAssignments.push(enriched);
    }

    logger.info(
      `Enrichment complete. Processed ${enrichedAssignments.length} assignments.`
    );

    return enrichedAssignments;
  }
}

module.exports = new BackendApiClient();
