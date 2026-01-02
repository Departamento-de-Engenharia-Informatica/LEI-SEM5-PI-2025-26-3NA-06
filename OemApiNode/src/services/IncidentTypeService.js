const incidentTypeRepository = require("../infrastructure/IncidentTypeRepository");
const IncidentTypeMapper = require("../dtos/IncidentType/IncidentTypeMapper");
const logger = require("../utils/logger");

class IncidentTypeService {
  /**
   * Initialize database (ensure table exists)
   */
  async initializeAsync() {
    await incidentTypeRepository.ensureTableExists();
  }

  /**
   * Create a new incident type
   */
  async createIncidentTypeAsync(requestBody, userId, username) {
    try {
      // Convert request to DTO
      const dto = IncidentTypeMapper.fromCreateRequest(requestBody);
      dto.validate();

      // Check if code already exists
      const existingByCode = await incidentTypeRepository.getByCodeAsync(
        dto.code.trim()
      );
      if (existingByCode) {
        throw new Error(`Incident type with code '${dto.code}' already exists`);
      }

      // Check for circular reference if parent is specified
      if (dto.parentId) {
        const parent = await incidentTypeRepository.getByIdAsync(dto.parentId);
        if (!parent) {
          throw new Error(`Parent incident type '${dto.parentId}' not found`);
        }
        if (!parent.isActive) {
          throw new Error("Cannot use inactive incident type as parent");
        }
      }

      // Convert DTO to domain entity
      const incidentType = IncidentTypeMapper.toDomain(dto);

      // Save to database
      const created = await incidentTypeRepository.createAsync(incidentType);

      logger.info(
        `Incident type created: ${created.code} by user ${username || userId}`
      );

      return {
        success: true,
        data: IncidentTypeMapper.toResponseDto(created),
      };
    } catch (error) {
      logger.error("Error creating incident type:", error);
      return {
        success: false,
        error: error.message || "Failed to create incident type",
      };
    }
  }

  /**
   * Get incident type by ID
   */
  async getIncidentTypeByIdAsync(id) {
    try {
      const incidentType = await incidentTypeRepository.getByIdAsync(id);

      if (!incidentType) {
        return {
          success: false,
          error: "Incident type not found",
        };
      }

      // Fetch parent name if exists
      let parentName = null;
      if (incidentType.parentId) {
        const parent = await incidentTypeRepository.getByIdAsync(
          incidentType.parentId
        );
        if (parent) {
          parentName = parent.name;
        }
      }

      return {
        success: true,
        data: IncidentTypeMapper.toResponseDto(incidentType, { parentName }),
      };
    } catch (error) {
      logger.error(`Error fetching incident type ${id}:`, error);
      return {
        success: false,
        error: error.message || "Failed to fetch incident type",
      };
    }
  }

  /**
   * Get incident type by code
   */
  async getIncidentTypeByCodeAsync(code) {
    try {
      const incidentType = await incidentTypeRepository.getByCodeAsync(code);

      if (!incidentType) {
        return {
          success: false,
          error: "Incident type not found",
        };
      }

      // Fetch parent name if exists
      let parentName = null;
      if (incidentType.parentId) {
        const parent = await incidentTypeRepository.getByIdAsync(
          incidentType.parentId
        );
        if (parent) {
          parentName = parent.name;
        }
      }

      return {
        success: true,
        data: IncidentTypeMapper.toResponseDto(incidentType, { parentName }),
      };
    } catch (error) {
      logger.error(`Error fetching incident type by code ${code}:`, error);
      return {
        success: false,
        error: error.message || "Failed to fetch incident type",
      };
    }
  }

  /**
   * Get all incident types (flat list)
   * Parent names are now fetched via JOIN in repository
   * @param {boolean|string} filter - false: active only, true: all, 'inactive': inactive only
   */
  async getAllIncidentTypesAsync(filter = false) {
    try {
      const incidentTypes = await incidentTypeRepository.getAllAsync(filter);

      // Map to response DTOs with parent names already included from JOIN
      const responseDtos = incidentTypes.map((type) =>
        IncidentTypeMapper.toResponseDto(type, { parentName: type.parentName })
      );

      return {
        success: true,
        data: responseDtos,
      };
    } catch (error) {
      logger.error("Error fetching all incident types:", error);
      return {
        success: false,
        error: error.message || "Failed to fetch incident types",
      };
    }
  }

  /**
   * Get incident types as hierarchical tree
   */
  async getIncidentTypesHierarchyAsync() {
    try {
      const incidentTypes = await incidentTypeRepository.getAllAsync(false);
      const hierarchy = IncidentTypeMapper.buildHierarchy(incidentTypes);

      return {
        success: true,
        data: hierarchy,
      };
    } catch (error) {
      logger.error("Error building incident types hierarchy:", error);
      return {
        success: false,
        error: error.message || "Failed to build hierarchy",
      };
    }
  }

  /**
   * Update incident type (code is immutable)
   */
  async updateIncidentTypeAsync(id, requestBody, userId) {
    try {
      // Fetch existing incident type
      const existing = await incidentTypeRepository.getByIdAsync(id);
      if (!existing) {
        return {
          success: false,
          error: "Incident type not found",
        };
      }

      // Convert request to DTO
      const dto = IncidentTypeMapper.fromUpdateRequest(requestBody);
      dto.validate();

      // Check if trying to change code (not allowed)
      if (requestBody.code && requestBody.code !== existing.code) {
        return {
          success: false,
          error: "Code cannot be modified after creation",
        };
      }

      // Check if DTO has any updates
      if (!dto.hasUpdates()) {
        return {
          success: false,
          error: "No fields to update",
        };
      }

      // Check for circular reference if parent is being updated
      if (dto.parentId !== undefined && dto.parentId !== null) {
        if (dto.parentId === existing.id) {
          return {
            success: false,
            error: "Incident type cannot be its own parent",
          };
        }

        const wouldBeCircular =
          await incidentTypeRepository.wouldCreateCircularReference(
            existing.id,
            dto.parentId
          );
        if (wouldBeCircular) {
          return {
            success: false,
            error: "Cannot create circular reference in hierarchy",
          };
        }

        // Check if parent exists and is active
        const parent = await incidentTypeRepository.getByIdAsync(dto.parentId);
        if (!parent) {
          return {
            success: false,
            error: `Parent incident type '${dto.parentId}' not found`,
          };
        }
        if (!parent.isActive) {
          return {
            success: false,
            error: "Cannot use inactive incident type as parent",
          };
        }
      }

      // Update the domain entity
      existing.update({
        name: dto.name,
        description: dto.description,
        severity: dto.severity,
        parentId: dto.parentId,
        isActive: dto.isActive,
      });

      // Save to database
      const updated = await incidentTypeRepository.updateAsync(existing);

      logger.info(`Incident type updated: ${updated.code} by user ${userId}`);

      return {
        success: true,
        data: IncidentTypeMapper.toResponseDto(updated),
      };
    } catch (error) {
      logger.error(`Error updating incident type ${id}:`, error);
      return {
        success: false,
        error: error.message || "Failed to update incident type",
      };
    }
  }

  /**
   * Delete incident type (soft delete)
   */
  async deleteIncidentTypeAsync(id, userId) {
    try {
      // Fetch existing incident type
      const existing = await incidentTypeRepository.getByIdAsync(id);
      if (!existing) {
        return {
          success: false,
          error: "Incident type not found",
        };
      }

      // Check if incident type has children
      const hasChildren = await incidentTypeRepository.hasChildrenAsync(id);
      if (hasChildren) {
        return {
          success: false,
          error:
            "Cannot delete incident type that has active children. Please delete or reassign children first.",
        };
      }

      // TODO: When Incidents module is implemented, check if this type is used
      // const isUsedInIncidents = await incidentRepository.isTypeUsedAsync(id);
      // if (isUsedInIncidents) {
      //   return {
      //     success: false,
      //     error: "Cannot delete incident type that is used in incident records"
      //   };
      // }

      // Soft delete
      const deleted = await incidentTypeRepository.deleteAsync(id);

      logger.info(`Incident type deleted: ${deleted.code} by user ${userId}`);

      return {
        success: true,
        message: "Incident type successfully deactivated",
        data: IncidentTypeMapper.toResponseDto(deleted),
      };
    } catch (error) {
      logger.error(`Error deleting incident type ${id}:`, error);
      return {
        success: false,
        error: error.message || "Failed to delete incident type",
      };
    }
  }

  /**
   * Activate incident type (set isActive to true)
   */
  async activateIncidentTypeAsync(id, userId) {
    try {
      // Fetch existing incident type
      const existing = await incidentTypeRepository.getByIdAsync(id);
      if (!existing) {
        return {
          success: false,
          error: "Incident type not found",
        };
      }

      if (existing.isActive) {
        return {
          success: false,
          error: "Incident type is already active",
        };
      }

      // Check if parent exists and is active (if has parent)
      if (existing.parentId) {
        const parent = await incidentTypeRepository.getByIdAsync(
          existing.parentId
        );
        if (!parent) {
          return {
            success: false,
            error: `Parent incident type not found`,
          };
        }
        if (!parent.isActive) {
          return {
            success: false,
            error:
              "Cannot activate incident type with inactive parent. Please activate parent first.",
          };
        }
      }

      // Activate
      existing.update({ isActive: true });
      const activated = await incidentTypeRepository.updateAsync(existing);

      logger.info(
        `Incident type activated: ${activated.code} by user ${userId}`
      );

      return {
        success: true,
        message: "Incident type successfully activated",
        data: IncidentTypeMapper.toResponseDto(activated),
      };
    } catch (error) {
      logger.error(`Error activating incident type ${id}:`, error);
      return {
        success: false,
        error: error.message || "Failed to activate incident type",
      };
    }
  }
}

module.exports = new IncidentTypeService();
