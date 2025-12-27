const { Connection, Request, TYPES } = require("tedious");
const logger = require("../utils/logger");

const config = {
  server: process.env.DB_SERVER,
  authentication: {
    type: process.env.DB_AUTHENTICATION || "azure-active-directory-default",
    options: {},
  },
  options: {
    database: process.env.DB_DATABASE,
    encrypt: process.env.DB_ENCRYPT === "true",
    trustServerCertificate: process.env.DB_TRUST_SERVER_CERTIFICATE === "true",
    port: parseInt(process.env.DB_PORT) || 1433,
    connectTimeout: 30000,
    requestTimeout: 30000,
  },
};

class Database {
  constructor() {
    this.connection = null;
  }

  async connect() {
    return new Promise((resolve, reject) => {
      this.connection = new Connection(config);

      this.connection.on("connect", (err) => {
        if (err) {
          logger.error("Database connection failed:", err);
          reject(err);
        } else {
          logger.info("✓ Connected to Azure SQL Database");
          resolve();
        }
      });

      this.connection.connect();
    });
  }

  async executeQuery(query, params = []) {
    return new Promise((resolve, reject) => {
      const results = [];
      const request = new Request(query, (err, rowCount) => {
        if (err) {
          const errorMessage = err.message || (err.errors && err.errors.map(e => e.message).join('; ')) || err.toString();
          logger.error("Query execution error:", { message: errorMessage, fullError: err });
          reject(new Error(errorMessage));
        } else {
          resolve(results);
        }
      });

      // Add parameters
      params.forEach((param) => {
        request.addParameter(param.name, param.type, param.value);
      });

      request.on("row", (columns) => {
        const row = {};
        columns.forEach((column) => {
          row[column.metadata.colName] = column.value;
        });
        results.push(row);
      });

      this.connection.execSql(request);
    });
  }

  async close() {
    if (this.connection) {
      this.connection.close();
      logger.info("Database connection closed");
    }
  }
}

module.exports = new Database();
