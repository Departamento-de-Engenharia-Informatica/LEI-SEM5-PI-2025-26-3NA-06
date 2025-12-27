/**
 * Assignment Value Object
 * Represents a vessel assignment to a dock
 */
class Assignment {
  constructor({
    vvnId,
    dockId,
    dockName = null,
    eta,
    etd,
    estimatedTeu = 0,
    vesselName = null,
    vesselImo = null,
  }) {
    this.vvnId = vvnId;
    this.dockId = dockId;
    this.dockName = dockName;
    this.eta = new Date(eta);
    this.etd = new Date(etd);
    this.estimatedTeu = estimatedTeu;
    this.vesselName = vesselName;
    this.vesselImo = vesselImo;

    this.validate();
  }

  validate() {
    const errors = [];

    if (!this.vvnId) {
      errors.push("vvnId is required");
    }

    if (!this.dockId) {
      errors.push("dockId is required");
    }

    if (!(this.eta instanceof Date) || isNaN(this.eta)) {
      errors.push("Invalid eta date");
    }

    if (!(this.etd instanceof Date) || isNaN(this.etd)) {
      errors.push("Invalid etd date");
    }

    if (this.eta >= this.etd) {
      errors.push("eta must be before etd");
    }

    if (errors.length > 0) {
      throw new Error(`Assignment validation failed: ${errors.join(", ")}`);
    }
  }

  /**
   * Check if this assignment overlaps with another
   */
  overlapsWith(other) {
    // Only check if they're on the same dock
    if (this.dockId !== other.dockId) {
      return false;
    }

    // Check for time overlap: a.eta < b.etd && b.eta < a.etd
    return this.eta < other.etd && other.eta < this.etd;
  }

  /**
   * Convert to plain object for JSON serialization
   */
  toJSON() {
    return {
      vvnId: this.vvnId,
      dockId: this.dockId,
      dockName: this.dockName,
      eta: this.eta.toISOString(),
      etd: this.etd.toISOString(),
      estimatedTeu: this.estimatedTeu,
      vesselName: this.vesselName,
      vesselImo: this.vesselImo,
    };
  }
}

module.exports = Assignment;
