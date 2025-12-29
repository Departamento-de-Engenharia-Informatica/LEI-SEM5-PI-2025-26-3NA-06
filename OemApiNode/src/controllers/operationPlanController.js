const logger = require("../utils/logger");
const operationPlanService = require("../services/OperationPlanService");
const backendApiClient = require("../services/BackendApiClient");

exports.create = async (req, res, next) => {
  try {
    const userId = req.user.sub || req.user.id;
    const username =
      req.user.name || req.user.email || req.user.preferred_username || userId;

    const result = await operationPlanService.createOperationPlanAsync(
      req.body,
      userId,
      username
    );

    if (result.success) {
      res.status(201).json(result);
    } else {
      res.status(400).json(result);
    }
  } catch (error) {
    logger.error("Error in create operation plan controller:", error);
    next(error);
  }
};

exports.replace = async (req, res, next) => {
  try {
    const userId = req.user.sub || req.user.id;
    const username =
      req.user.name || req.user.email || req.user.preferred_username || userId;

    const result = await operationPlanService.replaceOperationPlanByDateAsync(
      req.body,
      userId,
      username
    );

    if (result.success) {
      res.status(200).json(result);
    } else {
      res.status(400).json(result);
    }
  } catch (error) {
    logger.error("Error in replace operation plan controller:", error);
    next(error);
  }
};

exports.search = async (req, res, next) => {
  try {
    const { startDate, endDate, vesselIMO, skip, take } = req.query;

    const filters = {
      startDate,
      endDate,
      vesselIMO,
      skip: skip ? parseInt(skip) : 0,
      take: take ? parseInt(take) : 50,
    };

    const result = await operationPlanService.searchOperationPlansAsync(
      filters
    );

    if (result.success) {
      res.status(200).json(result);
    } else {
      res.status(400).json(result);
    }
  } catch (error) {
    logger.error("Error in search operation plans controller:", error);
    next(error);
  }
};

exports.getById = async (req, res, next) => {
  try {
    const { id } = req.params;
    const result = await operationPlanService.getOperationPlanByIdAsync(id);

    if (result.success) {
      res.status(200).json(result);
    } else {
      res.status(404).json(result);
    }
  } catch (error) {
    logger.error("Error in get operation plan by ID controller:", error);
    next(error);
  }
};

exports.getByDate = async (req, res, next) => {
  try {
    const { date } = req.params;
    const result = await operationPlanService.getOperationPlanByDateAsync(date);

    if (result.success) {
      res.status(200).json(result);
    } else {
      res.status(404).json(result);
    }
  } catch (error) {
    logger.error("Error in get operation plan by date controller:", error);
    next(error);
  }
};

exports.update = async (req, res, next) => {
  try {
    const { id } = req.params;
    const userId = req.user.sub || req.user.id;

    const result = await operationPlanService.updateOperationPlanAsync(
      id,
      req.body,
      userId
    );

    if (result.success) {
      res.status(200).json(result);
    } else {
      res.status(400).json(result);
    }
  } catch (error) {
    logger.error("Error in update operation plan controller:", error);
    next(error);
  }
};

exports.delete = async (req, res, next) => {
  try {
    const { date } = req.params;
    const userId = req.user.sub || req.user.id;

    const result = await operationPlanService.deleteOperationPlanAsync(
      date,
      userId
    );

    if (result.success) {
      res.status(204).send();
    } else {
      res.status(400).json(result);
    }
  } catch (error) {
    logger.error("Error in delete operation plan controller:", error);
    next(error);
  }
};

exports.validateFeasibility = async (req, res, next) => {
  try {
    const result = await operationPlanService.validateFeasibilityAsync(
      req.body
    );

    if (result.success) {
      res.status(200).json(result);
    } else {
      res.status(400).json(result);
    }
  } catch (error) {
    logger.error("Error in validate feasibility controller:", error);
    next(error);
  }
};

exports.getCargoManifests = async (req, res, next) => {
  try {
    const { vvnId } = req.params;
    const token = req.headers.authorization?.split(" ")[1];

    if (!token) {
      return res.status(401).json({
        success: false,
        error: "Authorization token required",
      });
    }

    const manifests = await backendApiClient.getCargoManifestsAsync(
      vvnId,
      token
    );

    if (!manifests) {
      return res.status(404).json({
        success: false,
        error: "Cargo manifests not found for this VVN",
      });
    }

    res.status(200).json({
      success: true,
      data: manifests,
    });
  } catch (error) {
    logger.error("Error in get cargo manifests controller:", error);
    next(error);
  }
};
