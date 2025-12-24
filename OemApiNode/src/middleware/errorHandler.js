const logger = require("../utils/logger");

const errorHandler = (err, req, res, next) => {
  logger.error("Error:", err);

  const status = err.status || err.statusCode || 500;
  const message = err.message || "Internal Server Error";

  res.status(status).json({
    error: {
      message,
      status,
      timestamp: new Date().toISOString(),
      path: req.path,
    },
  });
};

module.exports = { errorHandler };
