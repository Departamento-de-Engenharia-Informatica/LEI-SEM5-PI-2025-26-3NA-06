const { v4: uuidv4 } = require("uuid");

/**
 * Incident Domain Entity
 * Represents an incident affecting port operations
 */
class Incident {
  constructor({
    id,
    incidentTypeId,
    date,
    startTime,
    endTime,
    description,
    affectsAllVVEs,
    affectedVVEIds,
    isActive,
    createdAt,
    updatedAt,
  }) {
    this.id = id || uuidv4();
    this.incidentTypeId = incidentTypeId;
    this.date = date || new Date();
    this.startTime = startTime;
    this.endTime = endTime || null;
    this.description = description;
    this.affectsAllVVEs = affectsAllVVEs !== undefined ? affectsAllVVEs : false;
    this.affectedVVEIds = affectedVVEIds || [];
    this.isActive = isActive !== undefined ? isActive : true;
    this.createdAt = createdAt || new Date();
    this.updatedAt = updatedAt || null;
  }

  /**
   * Validate incident data
   */
  validate() {
    if (!this.incidentTypeId) {
      throw new Error("Incident Type is required");
    }

    if (!this.startTime) {
      throw new Error("Start time is required");
    }

    if (!this.description || this.description.trim().length === 0) {
      throw new Error("Description is required");
    }

    if (this.description.length > 2000) {
      throw new Error("Description cannot exceed 2000 characters");
    }

    if (this.endTime && this.endTime < this.startTime) {
      throw new Error("End time cannot be before start time");
    }

    if (
      !this.affectsAllVVEs &&
      (!this.affectedVVEIds || this.affectedVVEIds.length === 0)
    ) {
      throw new Error("Must select affected VVEs or check 'Affects All VVEs'");
    }
  }

  /**
   * Check if incident is currently active
   */
  isCurrentlyActive() {
    const now = new Date();

    // If no end time, it's active
    if (!this.endTime) {
      return true;
    }

    // If current time is between start and end, it's active
    return now >= this.startTime && now <= this.endTime;
  }

  /**
   * Get duration in minutes
   */
  getDurationMinutes() {
    if (!this.endTime) {
      return null;
    }
    return Math.floor((this.endTime - this.startTime) / 60000);
  }

  /**
   * Update incident
   */
  update({ startTime, endTime, description, affectsAllVVEs, affectedVVEIds }) {
    if (startTime !== undefined) this.startTime = startTime;
    if (endTime !== undefined) this.endTime = endTime;
    if (description !== undefined) this.description = description;
    if (affectsAllVVEs !== undefined) this.affectsAllVVEs = affectsAllVVEs;
    if (affectedVVEIds !== undefined) this.affectedVVEIds = affectedVVEIds;

    this.updatedAt = new Date();
  }

  /**
   * Convert to database format
   */
  toDatabase() {
    return {
      Id: this.id,
      IncidentTypeId: this.incidentTypeId,
      Date: this.date,
      StartTime: this.startTime,
      EndTime: this.endTime,
      Description: this.description,
      AffectsAllVVEs: this.affectsAllVVEs,
      AffectedVVEIds: this.affectedVVEIds.join(","), // Store as comma-separated
      IsActive: this.isActive,
      CreatedAt: this.createdAt,
      UpdatedAt: this.updatedAt,
    };
  }

  /**
   * Create from database row
   */
  static fromDatabase(row) {
    return new Incident({
      id: row.Id,
      incidentTypeId: row.IncidentTypeId,
      date: row.Date,
      startTime: row.StartTime,
      endTime: row.EndTime,
      description: row.Description,
      affectsAllVVEs: row.AffectsAllVVEs,
      affectedVVEIds: row.AffectedVVEIds
        ? row.AffectedVVEIds.split(",").filter((id) => id)
        : [],
      isActive: row.IsActive,
      createdAt: row.CreatedAt,
      updatedAt: row.UpdatedAt,
    });
  }

  /**
   * Convert to JSON response
   */
  toJSON() {
    return {
      id: this.id,
      incidentTypeId: this.incidentTypeId,
      incidentTypeCode: this.incidentTypeCode,
      incidentTypeName: this.incidentTypeName,
      severity: this.incidentTypeSeverity,
      date: this.date,
      startTime: this.startTime,
      endTime: this.endTime,
      description: this.description,
      affectsAllVVEs: this.affectsAllVVEs,
      affectedVVEIds: this.affectedVVEIds,
      isActive: this.isCurrentlyActive(),
      status: this.isCurrentlyActive() ? "Active" : "Inactive",
      durationMinutes: this.getDurationMinutes(),
      createdAt: this.createdAt,
      updatedAt: this.updatedAt,
    };
  }
}

module.exports = Incident;
