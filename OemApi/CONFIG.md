# OEM API Configuration Guide

## Overview

OEM (Operations & Execution Management) API is an independent microservice for managing Operation Plans and execution workflows.

## Port

- **Development**: `http://localhost:5003`

## Database Setup

### Azure SQL Database

1. Create a new Azure SQL Database named `OemDb`
2. Update `appsettings.json` with your connection string:

```json
"ConnectionStrings": {
  "OemDatabase": "Server=YOUR_AZURE_SERVER.database.windows.net;Database=OemDb;User Id=YOUR_USER;Password=YOUR_PASSWORD;TrustServerCertificate=True;"
}
```

### Run Migrations (when entities are added)

```bash
cd OemApi
dotnet ef migrations add InitialCreate
dotnet ef database update
```

## Authentication Configuration

### Google OAuth (Same as Backend/SchedulingApi)

Update `appsettings.json` with your Google OAuth credentials:

```json
"JwtSettings": {
  "Issuer": "https://accounts.google.com",
  "Audience": "YOUR_GOOGLE_CLIENT_ID.apps.googleusercontent.com",
  "SecretKey": "YOUR_GOOGLE_CLIENT_SECRET_OR_KEY"
}
```

**Important**: Use the **same** JwtSettings as your Backend and AuthApi to ensure token compatibility.

## Authorization

### Roles

- **LogisticOperator**: Can access OEM endpoints
- **PortAuthority**: (Future) Limited access to certain endpoints

### Policies

- `LogisticOperator`: Required for all OEM endpoints in MVP

## CORS

Configured to allow requests from Frontend SPA:

- Origin: `http://localhost:4200`

## Endpoints

### Health Check

- **GET** `/api/oem/health`
- **Auth**: Required (Bearer token)
- **Role**: LogisticOperator
- **Response**:

```json
{
  "service": "OEM",
  "status": "ok",
  "utcNow": "2025-12-23T18:00:00Z",
  "user": {
    "id": "...",
    "username": "...",
    "email": "...",
    "roles": ["LogisticOperator"]
  }
}
```

## Running the Service

```bash
cd OemApi
dotnet run
```

Access Swagger UI: http://localhost:5003/swagger

## Architecture

```
OemApi/
├── Controllers/         # API endpoints
├── Application/
│   └── DTOs/           # Data Transfer Objects
├── Domain/             # Domain entities (future)
├── Infrastructure/     # DbContext, repositories (future)
└── Program.cs          # Startup configuration
```

## Next Steps

1. Configure Azure SQL Database connection string
2. Copy JwtSettings from Backend's appsettings.json
3. Run the service
4. Test health endpoint from Frontend
5. Implement Operation Plan persistence (next sprint)
