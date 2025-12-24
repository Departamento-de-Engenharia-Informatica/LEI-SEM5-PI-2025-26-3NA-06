# OEM API Node.js - Setup & Configuration

## Overview

Operations & Execution Management (OEM) API built with Node.js/Express. This module manages:

- **Operation Plans** - Scheduling results and planned cargo operations
- **Vessel Visit Executions (VVE)** - Actual vessel operations and execution tracking
- **Incidents & Incident Types** - Operational disruptions and their classification
- **Complementary Tasks & Categories** - Non-cargo activities during vessel visits

## Architecture

Based on Sprint C requirements (US 4.1.1 through US 4.1.14), this module:

- Exposes REST API with Swagger documentation
- Implements JWT authentication (compatible with existing C# services)
- Uses Azure SQL Database (same as other modules)
- Follows RBAC/ABAC authorization patterns
- Communicates with other modules exclusively via REST

## Quick Start

### 1. Install Dependencies

```bash
cd OemApiNode
npm install
```

### 2. Configure Environment

Copy `.env.example` to `.env` and update:

```env
PORT=5004
DB_SERVER=projetoarqsibd.database.windows.net
DB_DATABASE=projArqsiDataBaseOEM
JWT_SECRET=QN7G8hgedJZEL3V4TUt6AzlPncYswvoIKaDpy5jSMk1XufHb09iRmWCqFOrB2x
```

### 3. Run the Server

```bash
# Development with auto-restart
npm run dev

# Production
npm start
```

Server will start on **http://localhost:5004**

### 4. Access Swagger Documentation

http://localhost:5004/swagger

## API Endpoints

### Health Check

- **GET** `/api/oem/health` - Test authentication and service status
- Requires: Bearer token, LogisticOperator role

### Operation Plans (US 4.1.2, 4.1.3, 4.1.4)

- **POST** `/api/oem/operation-plans` - Create Operation Plan
- **GET** `/api/oem/operation-plans` - Search plans by date/vessel
- **GET** `/api/oem/operation-plans/:id` - Get specific plan
- **PUT** `/api/oem/operation-plans/:id` - Update plan
- **DELETE** `/api/oem/operation-plans/:id` - Delete plan

### Vessel Visit Executions (US 4.1.8, 4.1.9)

- **POST** `/api/oem/vessel-visit-executions` - Create VVE
- **GET** `/api/oem/vessel-visit-executions` - List VVEs
- **PUT** `/api/oem/vessel-visit-executions/:id/berth` - Update berth time/dock
- **PUT** `/api/oem/vessel-visit-executions/:id/operations` - Update executed operations

### Incidents (US 4.1.13)

- **POST** `/api/oem/incidents` - Record incident
- **GET** `/api/oem/incidents` - List/filter incidents
- **GET** `/api/oem/incidents/:id` - Get incident details
- **PUT** `/api/oem/incidents/:id` - Update/resolve incident
- **DELETE** `/api/oem/incidents/:id` - Delete incident

### Incident Types (US 4.1.12)

- **POST** `/api/oem/incident-types` - Create incident type
- **GET** `/api/oem/incident-types` - List types hierarchy
- **PUT** `/api/oem/incident-types/:id` - Update type
- **DELETE** `/api/oem/incident-types/:id` - Delete type

### Task Categories (US 4.1.14)

- **POST** `/api/oem/task-categories` - Create category
- **GET** `/api/oem/task-categories` - List categories
- **PUT** `/api/oem/task-categories/:id` - Update category
- **DELETE** `/api/oem/task-categories/:id` - Delete category

## Authentication & Authorization

### JWT Token

Same token from AuthApi (port 5001). Include in requests:

```
Authorization: Bearer <your-jwt-token>
```

### Role Requirements

- **LogisticOperator**: Most OEM operations (plans, VVEs, incidents)
- **PortAuthority**: Manage catalogs (Incident Types, Task Categories)

## Database

Uses Azure SQL Database `projArqsiDataBaseOEM` (same server as other modules).

Connection via Azure Active Directory Default authentication (no SQL password needed).

## Project Structure

```
OemApiNode/
├── src/
│   ├── server.js                    # Entry point
│   ├── config/
│   │   ├── swagger.js              # Swagger/OpenAPI config
│   │   └── database.js             # Azure SQL connection
│   ├── middleware/
│   │   ├── auth.js                 # JWT authentication
│   │   └── errorHandler.js        # Error handling
│   ├── routes/
│   │   ├── health.js               # Health check
│   │   ├── operationPlans.js       # Operation Plans CRUD
│   │   ├── vesselVisitExecutions.js
│   │   ├── incidents.js
│   │   ├── incidentTypes.js
│   │   ├── complementaryTasks.js
│   │   └── taskCategories.js
│   ├── controllers/
│   │   └── operationPlanController.js
│   ├── models/                     # (To be added)
│   ├── services/                   # (To be added)
│   └── utils/
│       └── logger.js               # Winston logging
├── logs/                           # Log files
├── package.json
├── .env
└── README.md
```

## Current Status

### ✅ Implemented

- Project structure and configuration
- JWT authentication middleware
- Role-based authorization
- Swagger/OpenAPI documentation
- Health check endpoint (fully functional)
- Route scaffolding for all endpoints
- Azure SQL Database connection setup
- Logging infrastructure

### 🔄 In Progress

- Database models and schemas
- Full CRUD implementations for all entities
- Business logic validation
- Database migrations

### 📋 Next Steps

1. Define database schema (tables for OperationPlans, VVEs, Incidents, etc.)
2. Implement full CRUD logic in controllers
3. Add data validation (Joi schemas)
4. Implement business rules (conflict detection, auditability)
5. Add unit tests
6. Update Frontend to consume Node.js API (port 5004 instead of 5003)

## Testing

### Test Health Endpoint

```bash
curl -H "Authorization: Bearer YOUR_JWT_TOKEN" \
     http://localhost:5004/api/oem/health
```

Expected response:

```json
{
  "service": "OEM",
  "status": "ok",
  "utcNow": "2025-12-23T19:30:00.000Z",
  "user": {
    "id": "...",
    "username": "...",
    "email": "...",
    "roles": ["LogisticOperator"]
  }
}
```

## Frontend Integration

Update Frontend OEM service to use port **5004**:

```typescript
// src/app/services/oem.service.ts
private oemApiUrl = 'http://localhost:5004/api/oem';
```

## Logs

- `logs/combined.log` - All logs
- `logs/error.log` - Error logs only
- Console - Colored output with timestamps

## Notes

- Port 5003 was used by C# OEM API (keep it for now)
- Port 5004 is for Node.js OEM API (new)
- Both versions use same database: `projArqsiDataBaseOEM`
- JWT settings must match C# services for token compatibility

---

**Status**: 🟡 MVP - Health endpoint working, full CRUD implementation in progress
