const swaggerJsdoc = require("swagger-jsdoc");

const options = {
  definition: {
    openapi: "3.0.0",
    info: {
      title: "OEM API - Operations & Execution Management",
      version: "1.0.0",
      description:
        "API for managing port operation plans, vessel visit executions, incidents, and complementary tasks",
      contact: {
        name: "ProjArqsi Team",
      },
    },
    servers: [
      {
        url: "http://localhost:5004",
        description: "Development server",
      },
    ],
    components: {
      securitySchemes: {
        bearerAuth: {
          type: "http",
          scheme: "bearer",
          bearerFormat: "JWT",
          description: "Enter JWT token obtained from AuthApi",
        },
      },
    },
    security: [
      {
        bearerAuth: [],
      },
    ],
  },
  apis: ["./src/routes/*.js", "./src/models/*.js"],
};

const swaggerSpec = swaggerJsdoc(options);

module.exports = swaggerSpec;
