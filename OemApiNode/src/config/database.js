const { Connection, Request, TYPES } = require("tedious");
const logger = require("../utils/logger");

const connectionConfig = {
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
    this.connectionPool = [];
    this.maxPoolSize = 10;
    this.minPoolSize = 2;
  }

  async connect() {
    // Pre-create minimum connections
    logger.info("Initializing database connection pool...");
    const promises = [];
    for (let i = 0; i < this.minPoolSize; i++) {
      promises.push(this.createConnection());
    }

    try {
      await Promise.all(promises);
      logger.info(
        `✓ Connected to Azure SQL Database (Pool: ${this.minPoolSize} connections)`
      );
    } catch (error) {
      logger.error("Database connection failed:", error);
      throw error;
    }
  }

  createConnection() {
    return new Promise((resolve, reject) => {
      const connection = new Connection(connectionConfig);

      connection.on("connect", (err) => {
        if (err) {
          reject(err);
        } else {
          connection.inUse = false;
          this.connectionPool.push(connection);
          resolve(connection);
        }
      });

      connection.on("error", (err) => {
        logger.error("Connection error:", err);
      });

      connection.connect();
    });
  }

  async getConnection() {
    // Find available connection
    let connection = this.connectionPool.find(
      (conn) => !conn.inUse && conn.state.name === "LoggedIn"
    );

    if (!connection) {
      // Create new connection if pool not full
      if (this.connectionPool.length < this.maxPoolSize) {
        connection = await this.createConnection();
      } else {
        // Wait for available connection
        await new Promise((resolve) => setTimeout(resolve, 100));
        return this.getConnection();
      }
    }

    connection.inUse = true;
    return connection;
  }

  releaseConnection(connection) {
    if (connection) {
      connection.inUse = false;
    }
  }

  async executeQuery(query, params = []) {
    const connection = await this.getConnection();

    return new Promise((resolve, reject) => {
      const results = [];
      const request = new Request(query, (err, rowCount) => {
        this.releaseConnection(connection);

        if (err) {
          const errorMessage =
            err.message ||
            (err.errors && err.errors.map((e) => e.message).join("; ")) ||
            err.toString();
          logger.error("Query execution error:", {
            message: errorMessage,
            fullError: err,
          });
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

      connection.execSql(request);
    });
  }

  async close() {
    for (const connection of this.connectionPool) {
      if (connection) {
        connection.close();
      }
    }
    this.connectionPool = [];
    logger.info("Database connection pool closed");
  }
}

module.exports = new Database();
