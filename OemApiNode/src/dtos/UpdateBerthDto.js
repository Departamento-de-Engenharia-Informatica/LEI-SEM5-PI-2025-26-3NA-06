/**
 * DTO for updating VVE berth information
 */
class UpdateBerthDto {
  constructor({
    actualArrivalTime = null,
    actualDockId = null,
    actualDepartureTime = null,
  }) {
    this.actualArrivalTime = actualArrivalTime;
    this.actualDockId = actualDockId;
    this.actualDepartureTime = actualDepartureTime;
  }

  /**
   * Validate DTO
   */
  validate() {
    const errors = [];

    // At least one field must be provided
    if (
      !this.actualArrivalTime &&
      !this.actualDockId &&
      !this.actualDepartureTime
    ) {
      errors.push(
        "At least one of actualArrivalTime, actualDockId, or actualDepartureTime must be provided"
      );
    }

    if (errors.length > 0) {
      throw new Error(`DTO validation failed: ${errors.join(", ")}`);
    }

    return true;
  }

  /**
   * Create from request body
   */
  static fromRequest(requestBody) {
    return new UpdateBerthDto({
      actualArrivalTime: requestBody.actualArrivalTime,
      actualDockId: requestBody.actualDockId,
      actualDepartureTime: requestBody.actualDepartureTime,
    });
  }
}

module.exports = UpdateBerthDto;
