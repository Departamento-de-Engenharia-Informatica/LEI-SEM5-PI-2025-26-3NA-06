const IncidentType = require("../../domain/IncidentType/IncidentType");
const CreateIncidentTypeDto = require("./CreateIncidentTypeDto");
const UpdateIncidentTypeDto = require("./UpdateIncidentTypeDto");
const IncidentTypeResponseDto = require("./IncidentTypeResponseDto");

/**
 * Mapper between DTOs and Domain entities for Incident Types
 */
class IncidentTypeMapper {
  /**
   * Convert CreateIncidentTypeDto to IncidentType domain entity
   */
  static toDomain(dto) {
    return new IncidentType({
      code: dto.code.trim(),
      name: dto.name.trim(),
      description: dto.description ? dto.description.trim() : null,
      severity: dto.severity,
      parentId: dto.parentId || null,
    });
  }

  /**
   * Convert IncidentType domain entity to IncidentTypeResponseDto
   */
  static toResponseDto(incidentType, options = {}) {
    return IncidentTypeResponseDto.fromDomain(incidentType, options);
  }

  /**
   * Convert array of IncidentTypes to response DTOs
   */
  static toResponseDtoList(incidentTypes, options = {}) {
    return incidentTypes.map((type) => this.toResponseDto(type, options));
  }

  /**
   * Create CreateIncidentTypeDto from request body
   */
  static fromCreateRequest(body) {
    return CreateIncidentTypeDto.fromRequest(body);
  }

  /**
   * Create UpdateIncidentTypeDto from request body
   */
  static fromUpdateRequest(body) {
    return UpdateIncidentTypeDto.fromRequest(body);
  }

  /**
   * Build hierarchical tree structure from flat list
   */
  static buildHierarchy(incidentTypes) {
    // Create a map of all types by ID
    const typeMap = new Map();
    incidentTypes.forEach((type) => {
      typeMap.set(type.id, {
        ...this.toResponseDto(type),
        children: [],
      });
    });

    // Build hierarchy
    const rootTypes = [];
    typeMap.forEach((type) => {
      if (type.parentId) {
        const parent = typeMap.get(type.parentId);
        if (parent) {
          parent.children.push(type);
        } else {
          // Parent not found (might be inactive), treat as root
          rootTypes.push(type);
        }
      } else {
        rootTypes.push(type);
      }
    });

    return rootTypes;
  }
}

module.exports = IncidentTypeMapper;
