const { v4: uuidv4, validate: uuidValidate } = require("uuid");

/**
 * Incident Type Domain Entity
 * Represents a classification type for operational disruptions
 * Supports unlimited hierarchical depth
 */
class IncidentType {
  constructor({
    id,
    code,
    name,
    description,
    severity,
    parentId = null,
    isActive = true,
    createdAt,
    updatedAt = null,
  }) {
    // Generate ID if not provided
    this.id = id || uuidv4();

    // Validate and set ID format
    if (!uuidValidate(this.id)) {
      throw new Error("Invalid incident type ID format");
    }

    // Validate and set code (immutable, alphanumeric, max 10 chars)
    this.validateCode(code);
    this.code = code;

    // Validate and set name
    this.validateName(name);
    this.name = name;

    // Description is optional
    this.description = description || null;

    // Validate and set severity
    this.validateSeverity(severity);
    this.severity = severity;

    // Parent ID for hierarchy (optional, validates if provided)
    if (parentId) {
      if (!uuidValidate(parentId)) {
        throw new Error("Invalid parent ID format");
      }
      if (parentId === this.id) {
        throw new Error("Incident type cannot be its own parent");
      }
    }
    this.parentId = parentId;

    // Active status (for soft delete)
    this.isActive = isActive !== false;

    // Timestamps
    this.createdAt = createdAt || new Date();
    this.updatedAt = updatedAt;
  }

  /**
   * Validate code: alphanumeric, max 10 chars, required
   */
  validateCode(code) {
    if (!code || typeof code !== "string") {
      throw new Error("Code is required and must be a string");
    }

    const trimmedCode = code.trim();

    if (trimmedCode.length === 0 || trimmedCode.length > 10) {
      throw new Error("Code must be between 1 and 10 characters");
    }

    // Alphanumeric with hyphens allowed
    if (!/^[A-Za-z0-9\-]+$/.test(trimmedCode)) {
      throw new Error(
        "Code must be alphanumeric (letters, numbers, hyphens only)"
      );
    }
  }

  /**
   * Validate name: required, non-empty
   */
  validateName(name) {
    if (!name || typeof name !== "string" || name.trim().length === 0) {
      throw new Error("Name is required and must be a non-empty string");
    }

    if (name.trim().length > 255) {
      throw new Error("Name must not exceed 255 characters");
    }
  }

  /**
   * Validate severity: must be one of the predefined values
   */
  validateSeverity(severity) {
    const validSeverities = ["Minor", "Major", "Critical"];

    if (!severity || !validSeverities.includes(severity)) {
      throw new Error(`Severity must be one of: ${validSeverities.join(", ")}`);
    }
  }

  /**
   * Update incident type fields (code is immutable)
   */
  update({ name, description, severity, parentId, isActive }) {
    // Update name if provided
    if (name !== undefined) {
      this.validateName(name);
      this.name = name;
    }

    // Update description if provided
    if (description !== undefined) {
      this.description = description;
    }

    // Update severity if provided
    if (severity !== undefined) {
      this.validateSeverity(severity);
      this.severity = severity;
    }

    // Update parent ID if provided
    if (parentId !== undefined) {
      if (parentId) {
        if (!uuidValidate(parentId)) {
          throw new Error("Invalid parent ID format");
        }
        if (parentId === this.id) {
          throw new Error("Incident type cannot be its own parent");
        }
      }
      this.parentId = parentId;
    }

    // Update active status if provided
    if (isActive !== undefined) {
      this.isActive = isActive !== false;
    }

    // Update timestamp
    this.updatedAt = new Date();
  }

  /**
   * Deactivate incident type (soft delete)
   */
  deactivate() {
    this.isActive = false;
    this.updatedAt = new Date();
  }

  /**
   * Convert to database format
   */
  toDatabase() {
    return {
      Id: this.id,
      Code: this.code,
      Name: this.name,
      Description: this.description,
      Severity: this.severity,
      ParentId: this.parentId,
      IsActive: this.isActive ? 1 : 0,
      CreatedAt: this.createdAt,
      UpdatedAt: this.updatedAt,
    };
  }

  /**
   * Create from database row
   */
  static fromDatabase(row) {
    return new IncidentType({
      id: row.Id,
      code: row.Code,
      name: row.Name,
      description: row.Description,
      severity: row.Severity,
      parentId: row.ParentId,
      isActive: row.IsActive === 1 || row.IsActive === true,
      createdAt: row.CreatedAt,
      updatedAt: row.UpdatedAt,
    });
  }

  /**
   * Convert to JSON for API responses
   */
  toJSON() {
    return {
      id: this.id,
      code: this.code,
      name: this.name,
      description: this.description,
      severity: this.severity,
      parentId: this.parentId,
      isActive: this.isActive,
      createdAt: this.createdAt,
      updatedAt: this.updatedAt,
    };
  }
}

module.exports = IncidentType;
