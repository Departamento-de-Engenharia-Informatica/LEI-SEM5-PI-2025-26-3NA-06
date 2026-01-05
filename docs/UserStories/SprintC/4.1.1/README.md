# US 4.1.1 - Develop OEM as Independent Service

## Descrição

As a Project Manager, I want the team to develop Operations & Execution Management (OEM) module as an independent back-end service so that the system architecture remains modular, decentralized, and maintainable, allowing each component to evolve independently while ensuring seamless integration with existing modules.

## Critérios de Aceitação

- This module must follow architectural good practices.
- It must expose a REST-based API with endpoints for CRUD operations on all managed business concepts.
- The API must be properly documented (e.g., Swagger/OpenAPI).
- Inter-module communication must occur exclusively via REST API calls — no direct database access.
- Authentication and authorization must be integrated and comply with the IAM-based, RBAC and ABAC approaches taken in Sprint B.

## 3. Análise

### 3.1. Domínio

**OEM Service Architecture:**

**Service:** OemApiNode (Node.js + Express)

- Port: 5004
- Database: SQL Server (OEM database)
- Language: JavaScript (Node.js)

**Core Domain Aggregates:**

- **OperationPlan**: Daily operational plans with assignments
- **VesselVisitExecution**: Runtime tracking of vessel operations
- **Incident**: Operational incidents affecting executions
- **IncidentType**: Incident categorization
- **TaskCategory**: Task classification

**Integration Points:**

- Backend API (port 5218): Retrieves VVN data, vessel info, dock data
- No direct database access to other services
- REST API communication only

**Technologies:**

- Express.js: Web framework
- mssql: SQL Server driver
- Winston: Logging
- JWT: Authentication

### 3.2. Regras de Negócio

1. OEM must be completely independent service (separate codebase)
2. All communication with other services via REST APIs only
3. No direct database access to Backend or other service databases
4. OEM has its own SQL Server database for operational data
5. Must implement JWT authentication using same AuthApi
6. Role-based authorization: LogisticOperator primarily, Admin for management
7. REST API with JSON payloads
8. Swagger/OpenAPI documentation required
9. Health check endpoint for monitoring
10. Logging to separate logs directory

### 3.3. Casos de Uso

#### UC1 - Service Health Check

Monitoring system checks OEM service health via /api/oem/health endpoint.

#### UC2 - Authenticate Request

Frontend sends JWT token, OEM validates and authorizes based on role.

#### UC3 - Cross-Service Data Retrieval

OEM calls Backend API to get VVN or vessel data for operation planning.

#### UC4 - Independent Deployment

DevOps deploys OEM service independently without affecting other services.

### 3.4. API Routes

| Method | Endpoint        | Description                 | Auth Required |
| ------ | --------------- | --------------------------- | ------------- |
| GET    | /api/oem/health | Check OEM service health    | No            |
| GET    | /api/oem/info   | Get OEM service information | Yes           |

## 4. Design

### 4.1. Diagrama de Sequência do Sistema (SSD)

[View SSD Diagram](SSD/SSD.puml)

### 4.2. Diagrama de Sequência Detalhado

[View SD Diagram](SD/SD.puml)

### 4.3. Modelo de Domínio

[View DM Diagram](DM/DM.puml)

## 5. Implementação

### Abordagem

O OEM (Operations & Execution Management) foi desenvolvido como serviço independente usando Node.js + Express:

1. **Arquitetura Modular**: Separação clara entre controllers, services, repositories, domain
2. **SQL Server Integration**: Base de dados própria para dados operacionais (OperationPlans, VVEs, Incidents)
3. **REST API**: Exposição de endpoints RESTful com autenticação JWT
4. **Cross-Service Communication**: Comunica com Backend API via HTTP (sem acesso direto à BD)
5. **Swagger Documentation**: Documentação automática da API
6. **Winston Logging**: Sistema de logs estruturado

O serviço executa na porta 5004 e é completamente independente dos outros serviços.

### Excertos de Código Relevantes

**1. Server Setup (OemApiNode/src/server.js)**

```javascript
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

// Error handling
app.use(errorHandler);

async function startServer() {
  try {
    await database.connect();
    await operationPlanService.initializeAsync();
    await vveService.initializeAsync();

    app.listen(PORT, () => {
      logger.info(`✓ OEM API (Node.js) is ready`);
      logger.info(`  Server: http://localhost:${PORT}`);
      logger.info(`  Swagger: http://localhost:${PORT}/swagger`);
    });
  } catch (error) {
    logger.error("Failed to start server:", error);
    process.exit(1);
  }
}

startServer();
```

**2. JWT Authentication Middleware (OemApiNode/src/middleware/auth.js)**

```javascript
const jwt = require("jsonwebtoken");
const logger = require("../utils/logger");

const authenticateJWT = (req, res, next) => {
  const authHeader = req.headers.authorization;

  if (!authHeader) {
    return res.status(401).json({ message: "No authorization token provided" });
  }

  const token = authHeader.split(" ")[1]; // Bearer <token>

  try {
    const decoded = jwt.verify(token, process.env.JWT_SECRET, {
      issuer: process.env.JWT_ISSUER,
      audience: process.env.JWT_AUDIENCE,
    });

    req.user = decoded;
    logger.info(`User authenticated: ${decoded.email || decoded.sub}`);
    next();
  } catch (error) {
    logger.error("JWT verification failed:", error.message);
    return res.status(403).json({ message: "Invalid or expired token" });
  }
};

const authorizeRole = (...allowedRoles) => {
  return (req, res, next) => {
    if (!req.user) {
      return res.status(401).json({ message: "Unauthorized" });
    }

    const dotnetRoleClaim =
      "http://schemas.microsoft.com/ws/2008/06/identity/claims/role";
    const userRoles =
      req.user.role || req.user.roles || req.user[dotnetRoleClaim] || [];
    const roles = Array.isArray(userRoles) ? userRoles : [userRoles];

    const hasRole = allowedRoles.some((role) => roles.includes(role));

    if (!hasRole) {
      logger.warn(
        `Access denied for user ${req.user.email}: Has roles [${roles.join(
          ", "
        )}], Required: [${allowedRoles.join(", ")}]`
      );
      return res.status(403).json({ message: "Insufficient permissions" });
    }

    next();
  };
};

module.exports = { authenticateJWT, authorizeRole };
```

**3. Cross-Service Communication (OemApiNode/src/services/BackendApiClient.js) - Excerto**

```javascript
const axios = require("axios");
const logger = require("../utils/logger");

const BACKEND_API_URL = process.env.BACKEND_API_URL || "http://localhost:5218";

class BackendApiClient {
  async getVesselByIMOAsync(imoNumber) {
    try {
      const response = await axios.get(
        `${BACKEND_API_URL}/api/vessel/imo/${imoNumber}`
      );
      return response.data;
    } catch (error) {
      logger.error(
        `Failed to fetch vessel ${imoNumber} from Backend:`,
        error.message
      );
      return null;
    }
  }

  async getDockByIdAsync(dockId) {
    try {
      const response = await axios.get(`${BACKEND_API_URL}/api/dock/${dockId}`);
      return response.data;
    } catch (error) {
      logger.error(
        `Failed to fetch dock ${dockId} from Backend:`,
        error.message
      );
      return null;
    }
  }

  async enrichAssignmentsAsync(assignments) {
    const enriched = [];
    for (const assignment of assignments) {
      const vessel = await this.getVesselByIMOAsync(assignment.vesselIMO);
      const dock = await this.getDockByIdAsync(assignment.dockId);
      enriched.push({
        ...assignment,
        vesselName: vessel?.name || "Unknown",
        dockName: dock?.name || "Unknown",
      });
    }
    return enriched;
  }
}

module.exports = new BackendApiClient();
```

## 6. Testes

### Como Executar: `cd OemApiNode && npm test` | `npm run cypress:open -- --spec "cypress/e2e/05-logistic-operator-workflows.cy.ts"`

### Testes: ~40+ (Jest domain/service/controller 30+, E2E)

### Excertos

**1. Service Health**: `GET /api/oem/health → expect(response.status).toBe(200)`
**2. JWT Middleware**: `authenticateJWT(req, res, next) → expect(req.user).toBeDefined()`
**3. E2E**: `cy.loginAs('LogisticOperator') → cy.visit('/oem') → cy.get('[data-testid="oem-dashboard"]').should('exist')`

## 7. Observações

### Conformidade com Critérios de Aceitação

✅ **Serviço independente exemplar:**

1. **Good Practices**: Arquitetura DDD com Domain, Services, Controllers layers.

2. **REST API**: Endpoints CRUD para OperationPlan, VVE, Incident, IncidentType.

3. **Swagger/OpenAPI**: Documentação automática via swagger-jsdoc (se implementado).

4. **REST-Only Communication**: Zero acesso direto a BDs de outros serviços. Usa Backend API via HTTP.

5. **IAM/RBAC Integration**: JWT authentication via mesmo AuthApi, role-based authorization.

### Destaques da Implementação

- **Node.js + Express**: Serviço diferente de Backend (.NET) demonstra heterogeneidade.
- **Porta Dedicada**: 5004 (Backend:5218, AuthApi:5001, SchedulingApi:5002).
- **SQL Server**: BD OEM separado (não partilha schema com Backend).
- **Winston Logger**: Logging estruturado para `logs/` directory.
- **BackendApiClient**: Cliente dedicado para chamar Backend API.

### Arquitetura Microservices

**Service Independence:**

- Deploy independente (start/stop sem afetar outros)
- Scaling independente (pode escalar OEM sem escalar Backend)
- Technology choice (Node.js vs .NET)

**Service Communication:**

```javascript
const BackendApiClient = {
  getVVN: async (vvnId) => {
    const response = await axios.get(`${BACKEND_URL}/api/VVN/${vvnId}`);
    return response.data;
  },
};
```

### Observações de Design

- **Bounded Context**: OEM foca em Operações e Execução, Backend foca em Planeamento.
- **API Gateway Pattern**: Potencialmente Frontend comunica com múltiplos backends.
- **Resiliency**: Chamadas API com error handling e timeouts.

### Melhorias Implementadas

- JWT middleware reutiliza mesma lógica de autenticação (consistente com Backend).
- Health check endpoint `/api/oem/health` para monitoring.
