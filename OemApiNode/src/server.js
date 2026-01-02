// Load environment variables FIRST
const dotenv = require("dotenv");
dotenv.config();

const express = require("express");
const cors = require("cors");
const swaggerUi = require("swagger-ui-express");
const swaggerSpec = require("./config/swagger");
const logger = require("./utils/logger");
const { errorHandler } = require("./middleware/errorHandler");
const database = require("./config/database");
const operationPlanService = require("./services/OperationPlanService");
const vveService = require("./services/VesselVisitExecutionService");
const incidentTypeService = require("./services/IncidentTypeService");
const incidentService = require("./services/IncidentService");

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

// Start server with database initialization
async function startServer() {
  try {
    // Connect to database
    await database.connect();

    // Initialize services (create tables if needed)
    await operationPlanService.initializeAsync();
    await vveService.initializeAsync();
    await incidentTypeService.initializeAsync();
    await incidentService.initializeAsync();

    app.listen(PORT, () => {
      logger.info(`✓ OEM API (Node.js) is ready`);
      logger.info(`  Server: http://localhost:${PORT}`);
      logger.info(`  Swagger: http://localhost:${PORT}/swagger`);
      logger.info(`  Health: http://localhost:${PORT}/api/oem/health`);
    });
  } catch (error) {
    logger.error("Failed to start server:", error);
    process.exit(1);
  }
}

startServer();

module.exports = app;
