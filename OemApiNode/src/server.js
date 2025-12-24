const express = require("express");
const cors = require("cors");
const dotenv = require("dotenv");
const swaggerUi = require("swagger-ui-express");
const swaggerSpec = require("./config/swagger");
const logger = require("./utils/logger");
const { errorHandler } = require("./middleware/errorHandler");

// Load environment variables
dotenv.config();

const app = express();
const PORT = process.env.PORT || 5004;

// Middleware
app.use(
  cors({
    origin: process.env.CORS_ORIGIN || "http://localhost:4200",
    credentials: true,
  })
);
app.use(express.json());
app.use(express.urlencoded({ extended: true }));

// Swagger Documentation
app.use("/swagger", swaggerUi.serve, swaggerUi.setup(swaggerSpec));

// Routes
app.use("/api/oem/health", require("./routes/health"));
app.use("/api/oem/operation-plans", require("./routes/operationPlans"));
app.use(
  "/api/oem/vessel-visit-executions",
  require("./routes/vesselVisitExecutions")
);
app.use("/api/oem/incidents", require("./routes/incidents"));
app.use("/api/oem/incident-types", require("./routes/incidentTypes"));
app.use("/api/oem/complementary-tasks", require("./routes/complementaryTasks"));
app.use("/api/oem/task-categories", require("./routes/taskCategories"));

// Error handling middleware
app.use(errorHandler);

// Start server
app.listen(PORT, () => {
  logger.info(`✓ OEM API (Node.js) is ready`);
  logger.info(`  Server: http://localhost:${PORT}`);
  logger.info(`  Swagger: http://localhost:${PORT}/swagger`);
  logger.info(`  Health: http://localhost:${PORT}/api/oem/health`);
});

module.exports = app;
