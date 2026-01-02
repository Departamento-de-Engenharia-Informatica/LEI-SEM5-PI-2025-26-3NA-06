/**
 * DTO for updating an existing Incident Type
 * Code is immutable and cannot be updated
 */
class UpdateIncidentTypeDto {
  constructor({ name, description, severity, parentId, isActive }) {
    this.name = name;
    this.description = description;
    this.severity = severity;
    this.parentId = parentId;
    this.isActive = isActive;
  }

  /**
   * Create from request body
   */
  static fromRequest(body) {
    return new UpdateIncidentTypeDto({
      name: body.name,
      description: body.description,
      severity: body.severity,
      parentId: body.parentId,
      isActive: body.isActive,
    });
  }

  /**
   * Validate DTO
   */
  validate() {
    const errors = [];

    if (this.name !== undefined) {
      if (typeof this.name !== "string" || !this.name.trim()) {
        errors.push("Name must be a non-empty string");
      } else if (this.name.trim().length > 255) {
        errors.push("Name must not exceed 255 characters");
      }
    }

    if (this.severity !== undefined) {
      if (!["Minor", "Major", "Critical"].includes(this.severity)) {
        errors.push("Severity must be one of: Minor, Major, Critical");
      }
    }

    if (errors.length > 0) {
      throw new Error(`Validation failed: ${errors.join("; ")}`);
    }
  }

  /**
   * Check if DTO has any updates
   */
  hasUpdates() {
    return (
      this.name !== undefined ||
      this.description !== undefined ||
      this.severity !== undefined ||
      this.parentId !== undefined ||
      this.isActive !== undefined
    );
  }
}

module.exports = UpdateIncidentTypeDto;
