/**
 * DTO for creating a new Incident Type
 */
class CreateIncidentTypeDto {
  constructor({ code, name, description, severity, parentId }) {
    this.code = code;
    this.name = name;
    this.description = description;
    this.severity = severity;
    this.parentId = parentId || null;
  }

  /**
   * Create from request body
   */
  static fromRequest(body) {
    return new CreateIncidentTypeDto({
      code: body.code,
      name: body.name,
      description: body.description,
      severity: body.severity,
      parentId: body.parentId,
    });
  }

  /**
   * Validate DTO
   */
  validate() {
    const errors = [];

    if (!this.code || typeof this.code !== "string" || !this.code.trim()) {
      errors.push("Code is required");
    } else if (this.code.trim().length > 10) {
      errors.push("Code must not exceed 10 characters");
    } else if (!/^[A-Za-z0-9\-]+$/.test(this.code.trim())) {
      errors.push("Code must be alphanumeric (letters, numbers, hyphens only)");
    }

    if (!this.name || typeof this.name !== "string" || !this.name.trim()) {
      errors.push("Name is required");
    } else if (this.name.trim().length > 255) {
      errors.push("Name must not exceed 255 characters");
    }

    if (
      !this.severity ||
      !["Minor", "Major", "Critical"].includes(this.severity)
    ) {
      errors.push("Severity must be one of: Minor, Major, Critical");
    }

    if (errors.length > 0) {
      throw new Error(`Validation failed: ${errors.join("; ")}`);
    }
  }
}

module.exports = CreateIncidentTypeDto;
