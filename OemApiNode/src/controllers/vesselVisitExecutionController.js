const logger = require("../utils/logger");
const vveService = require("../services/VesselVisitExecutionService");
const VesselVisitExecutionResponseDto = require("../dtos/VesselVisitExecutionResponseDto");
const CreateVesselVisitExecutionDto = require("../dtos/CreateVesselVisitExecutionDto");
const UpdateBerthDto = require("../dtos/UpdateBerthDto");

/**
 * Prepare today's VVEs
 * Creates NotStarted placeholders for all VVNs in today's operation plan
 */
exports.prepareTodaysVVEs = async (req, res, next) => {
  try {
    const userId = req.user.sub || req.user.id;

    const result = await vveService.prepareTodaysVVEsAsync(userId);

    if (result.success) {
      res.status(201).json(result);
    } else {
      res.status(400).json(result);
    }
  } catch (error) {
    logger.error("Error in prepare today's VVEs controller:", error);
    next(error);
  }
};

/**
 * Create a new VVE manually
 */
exports.create = async (req, res, next) => {
  try {
    const userId = req.user.sub || req.user.id;

    const dto = CreateVesselVisitExecutionDto.fromRequest(req.body);
    dto.validate();

    const result = await vveService.createVVEAsync(dto, userId);

    if (result.success) {
      res.status(201).json({
        success: true,
        data: VesselVisitExecutionResponseDto.fromDomain(result.data),
        message: result.message,
      });
    } else {
      res.status(400).json(result);
    }
  } catch (error) {
    logger.error("Error in create VVE controller:", error);
    next(error);
  }
};

/**
 * Get VVE by ID
 */
exports.getById = async (req, res, next) => {
  try {
    const { id } = req.params;
    const result = await vveService.getVVEByIdAsync(id);

    if (result.success) {
      res.status(200).json({
        success: true,
        data: VesselVisitExecutionResponseDto.fromDomain(result.data),
      });
    } else {
      res.status(404).json(result);
    }
  } catch (error) {
    logger.error("Error in get VVE by ID controller:", error);
    next(error);
  }
};

/**
 * Get VVE by VVN ID
 */
exports.getByVvnId = async (req, res, next) => {
  try {
    const { vvnId } = req.params;
    const result = await vveService.getVVEByVvnIdAsync(vvnId);

    if (result.success) {
      res.status(200).json({
        success: true,
        data: VesselVisitExecutionResponseDto.fromDomain(result.data),
      });
    } else {
      res.status(404).json(result);
    }
  } catch (error) {
    logger.error("Error in get VVE by VVN ID controller:", error);
    next(error);
  }
};

/**
 * Get VVEs by date
 */
exports.getByDate = async (req, res, next) => {
  try {
    const { date } = req.params;
    const result = await vveService.getVVEsByDateAsync(date);

    if (result.success) {
      res.status(200).json({
        success: true,
        data: result.data.map((vve) =>
          VesselVisitExecutionResponseDto.fromDomain(vve)
        ),
        total: result.total,
      });
    } else {
      res.status(400).json(result);
    }
  } catch (error) {
    logger.error("Error in get VVEs by date controller:", error);
    next(error);
  }
};

/**
 * Search VVEs with filters
 */
exports.search = async (req, res, next) => {
  try {
    const { startDate, endDate, status, skip, take } = req.query;

    const filters = {
      startDate,
      endDate,
      status,
      skip: skip ? parseInt(skip) : 0,
      take: take ? parseInt(take) : 50,
    };

    const result = await vveService.searchVVEsAsync(filters);

    if (result.success) {
      res.status(200).json({
        success: true,
        data: result.data.map((vve) =>
          VesselVisitExecutionResponseDto.fromDomain(vve)
        ),
        total: result.total,
      });
    } else {
      res.status(400).json(result);
    }
  } catch (error) {
    logger.error("Error in search VVEs controller:", error);
    next(error);
  }
};

/**
 * Update VVE berth time and dock (US 4.1.8)
 */
exports.updateBerth = async (req, res, next) => {
  try {
    const { id } = req.params;
    const userId = req.user.sub || req.user.id;

    const dto = UpdateBerthDto.fromRequest(req.body);
    dto.validate();

    const result = await vveService.updateBerthAsync(id, dto, userId);

    if (result.success) {
      res.status(200).json({
        success: true,
        data: VesselVisitExecutionResponseDto.fromDomain(result.data),
        warnings: result.warnings || [],
        message: result.message,
      });
    } else {
      const statusCode = result.error.includes("not found") ? 404 : 400;
      res.status(statusCode).json(result);
    }
  } catch (error) {
    logger.error("Error in update VVE berth controller:", error);
    next(error);
  }
};

/**
 * Update VVE status
 */
exports.updateStatus = async (req, res, next) => {
  try {
    const { id } = req.params;
    const { status } = req.body;
    const userId = req.user.sub || req.user.id;

    if (!status) {
      return res.status(400).json({
        success: false,
        error: "Status is required in request body",
      });
    }

    const result = await vveService.updateStatusAsync(id, status, userId);

    if (result.success) {
      res.status(200).json({
        success: true,
        data: VesselVisitExecutionResponseDto.fromDomain(result.data),
        message: result.message,
      });
    } else {
      const statusCode = result.error.includes("not found") ? 404 : 400;
      res.status(statusCode).json(result);
    }
  } catch (error) {
    logger.error("Error in update VVE status controller:", error);
    next(error);
  }
};

/**
 * Delete VVE
 */
exports.delete = async (req, res, next) => {
  try {
    const { id } = req.params;
    const userId = req.user.sub || req.user.id;

    const result = await vveService.deleteVVEAsync(id, userId);

    if (result.success) {
      res.status(204).send();
    } else {
      const statusCode = result.error.includes("not found") ? 404 : 400;
      res.status(statusCode).json(result);
    }
  } catch (error) {
    logger.error("Error in delete VVE controller:", error);
    next(error);
  }
};
