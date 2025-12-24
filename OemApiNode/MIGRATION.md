# OEM Module Migration Summary

## ✅ Migration Completed: .NET → Node.js

### Background

The OEM (Operations & Execution Management) module was initially implemented in .NET C# (port 5003) but **Sprint C requirements specify Node.js** to demonstrate polyglot architecture.

### What Was Done

#### 1. Created Complete Node.js/Express Project

**Location:** `OemApiNode/` (parallel to existing `OemApi/`)

**Tech Stack:**

- Node.js + Express.js (REST API framework)
- Tedious (Azure SQL Server driver for Node.js)
- JSON Web Token (JWT authentication)
- Swagger/OpenAPI (API documentation)
- Winston (logging)

#### 2. Implemented Core Infrastructure

✅ **JWT Authentication Middleware**

- Compatible with existing C# services (same signing key)
- Extracts user claims (id, email, roles)
- Role-based authorization (LogisticOperator, PortAuthority)

✅ **Azure SQL Database Connection**

- Uses Tedious library for SQL Server
- Configured for `projArqsiDataBaseOEM`
- Azure Active Directory authentication

✅ **Error Handling & Logging**

- Centralized error handler middleware
- Winston logger with file + console output
- Logs stored in `OemApiNode/logs/`

✅ **Swagger Documentation**

- Available at http://localhost:5004/swagger
- JWT bearer authentication in Swagger UI
- All endpoints documented

#### 3. Implemented Endpoints (MVP + Scaffolding)

**Fully Functional:**

- ✅ `GET /api/oem/health` - Health check with user authentication

**Scaffolded (501 Not Implemented):**

- 🔄 Operation Plans CRUD (US 4.1.2, 4.1.3, 4.1.4)
- 🔄 Vessel Visit Executions (US 4.1.8, 4.1.9)
- 🔄 Incidents CRUD (US 4.1.13)
- 🔄 Incident Types catalog (US 4.1.12)
- 🔄 Complementary Tasks CRUD
- 🔄 Task Categories catalog (US 4.1.14)

All routes are wired up with proper authentication/authorization but return 501 responses until business logic is implemented.

#### 4. Updated Frontend Integration

✅ Changed `oem.service.ts` to use **port 5004** (Node.js) instead of 5003 (C#)

### Port Allocations

- **5001** - AuthApi (.NET)
- **5002** - SchedulingApi (.NET)
- **5003** - OemApi (.NET) - **DEPRECATED** (keep for reference)
- **5004** - OemApiNode (Node.js) - **NEW ACTIVE OEM**

### Architecture

```
┌─────────────────────────────────────────────────────────────┐
│                  Frontend (Angular, port 4200)              │
└────────────┬────────────┬────────────┬──────────────────────┘
             │            │            │
             ▼            ▼            ▼
      ┌──────────┐  ┌──────────┐  ┌─────────────┐
      │ AuthApi  │  │Scheduling│  │ OemApiNode  │
      │ .NET     │  │  .NET    │  │  Node.js    │ ◄── NEW!
      │ :5001    │  │  :5002   │  │  :5004      │
      └────┬─────┘  └────┬─────┘  └──────┬──────┘
           │             │                │
           └─────────────┴────────────────┘
                         ▼
              ┌──────────────────────┐
              │ Azure SQL Database   │
              │ projArqsiDataBaseOEM │
              └──────────────────────┘
```

### Running the Services

#### Start Node.js OEM API

```bash
cd OemApiNode
npm install
npm start
```

Server starts at http://localhost:5004

#### Test Health Endpoint

```bash
curl -H "Authorization: Bearer YOUR_JWT_TOKEN" \
     http://localhost:5004/api/oem/health
```

Or use Frontend's OEM test page:

- Login as LogisticOperator
- Navigate to `/logistic-operator/oem-test`
- Click "Test OEM Health"

### What's Next

#### Phase 1: Database Schema

- [ ] Create database tables:
  - `OperationPlans` (date, algorithm, author, vvns, status)
  - `VesselVisitExecutions` (vvnId, plannedDock, actualDock, berthTime, operations)
  - `Incidents` (type, severity, timestamp, vveId, description, resolved)
  - `IncidentTypes` (code, name, parentId, description)
  - `ComplementaryTasks` (categoryId, vveId, description, duration)
  - `TaskCategories` (code, name, description)
- [ ] Create migration script or Entity Framework equivalent

#### Phase 2: Implement Business Logic

- [ ] Operation Plans CRUD with validation
- [ ] VVE tracking and updates
- [ ] Incident management with severity levels
- [ ] Hierarchical incident type catalog
- [ ] Task categories management

#### Phase 3: Integration

- [ ] Scheduling API sends Operation Plans to OEM after generation
- [ ] OEM provides operation execution feedback to Scheduling
- [ ] Audit logging for all modifications

#### Phase 4: Advanced Features

- [ ] Search and filtering
- [ ] Pagination
- [ ] Conflict detection
- [ ] Notifications for incidents
- [ ] Reports and analytics

### Configuration Files

#### Environment Variables (.env)

```env
NODE_ENV=development
PORT=5004
DB_SERVER=projetoarqsibd.database.windows.net
DB_DATABASE=projArqsiDataBaseOEM
JWT_SECRET=QN7G8hgedJZEL3V4TUt6AzlPncYswvoIKaDpy5jSMk1XufHb09iRmWCqFOrB2x
JWT_ISSUER=ProjArqsi.AuthApi
JWT_AUDIENCE=ProjArqsi.Api
CORS_ORIGIN=http://localhost:4200
```

#### Dependencies (package.json)

- `express` - Web framework
- `tedious` - SQL Server driver
- `jsonwebtoken` - JWT verification
- `cors` - CORS middleware
- `swagger-jsdoc`, `swagger-ui-express` - API docs
- `winston` - Logging
- `joi` - Validation

### Key Files Created

**Configuration:**

- `src/server.js` - Express app setup
- `src/config/swagger.js` - OpenAPI specification
- `src/config/database.js` - Azure SQL connection

**Middleware:**

- `src/middleware/auth.js` - JWT verification + RBAC
- `src/middleware/errorHandler.js` - Centralized error handling

**Routes (7 modules):**

- `src/routes/health.js`
- `src/routes/operationPlans.js`
- `src/routes/vesselVisitExecutions.js`
- `src/routes/incidents.js`
- `src/routes/incidentTypes.js`
- `src/routes/complementaryTasks.js`
- `src/routes/taskCategories.js`

**Controllers:**

- `src/controllers/operationPlanController.js` (placeholder)

**Utils:**

- `src/utils/logger.js`

### Comparison: .NET vs Node.js OEM

| Feature   | .NET OEM (5003)                               | Node.js OEM (5004)        |
| --------- | --------------------------------------------- | ------------------------- |
| Language  | C#                                            | JavaScript (Node.js)      |
| Framework | ASP.NET Core                                  | Express.js                |
| Database  | EF Core                                       | Tedious (raw SQL)         |
| JWT Auth  | Microsoft.AspNetCore.Authentication.JwtBearer | jsonwebtoken + middleware |
| Swagger   | Swashbuckle                                   | swagger-jsdoc             |
| Logging   | ILogger                                       | Winston                   |
| Status    | ⚠️ Deprecated                                 | ✅ Active                 |

### Why Node.js?

According to Sprint C (US 4.1.1), OEM should demonstrate:

- **Polyglot architecture** (different language from other modules)
- **Microservices independence** (separate runtime, deployment)
- **REST-based integration** (no direct database coupling)

Node.js provides:

- Different tech stack from .NET Backend
- Lightweight, fast startup
- Rich npm ecosystem
- Asynchronous I/O (good for API gateways)

### Notes for Team

1. **Don't delete .NET OEM yet** - Keep `OemApi/` as reference during migration
2. **Frontend now points to Node.js** - Port 5004 instead of 5003
3. **Same JWT tokens work** - Authentication is compatible across services
4. **Same database** - Both versions can coexist during transition
5. **Swagger docs available** - http://localhost:5004/swagger

---

**Current Status:** 🟢 MVP Running - Health endpoint functional, full implementation in progress

**Last Updated:** 2025-12-23
