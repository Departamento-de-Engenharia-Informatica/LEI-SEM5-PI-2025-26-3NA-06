/**
 * DTO for Incident Type response
 */
class IncidentTypeResponseDto {
  constructor({
    id,
    code,
    name,
    description,
    severity,
    parentId,
    isActive,
    createdAt,
    updatedAt,
    children,
    parentName,
  }) {
    this.id = id;
    this.code = code;
    this.name = name;
    this.description = description;
    this.severity = severity;
    this.parentId = parentId;
    this.isActive = isActive;
    this.createdAt = createdAt;
    this.updatedAt = updatedAt;
    this.children = children || [];
    this.parentName = parentName || null;
  }

  /**
   * Create from domain entity
   */
  static fromDomain(incidentType, options = {}) {
    return new IncidentTypeResponseDto({
      id: incidentType.id,
      code: incidentType.code,
      name: incidentType.name,
      description: incidentType.description,
      severity: incidentType.severity,
      parentId: incidentType.parentId,
      isActive: incidentType.isActive,
      createdAt:
        incidentType.createdAt instanceof Date
          ? incidentType.createdAt.toISOString()
          : incidentType.createdAt,
      updatedAt:
        incidentType.updatedAt instanceof Date
          ? incidentType.updatedAt.toISOString()
          : incidentType.updatedAt,
      children: options.children || [],
      parentName: options.parentName || null,
    });
  }
}

module.exports = IncidentTypeResponseDto;
