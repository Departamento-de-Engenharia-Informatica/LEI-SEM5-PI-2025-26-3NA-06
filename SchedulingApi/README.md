# ProjArqsi.SchedulingApi

## Overview

The Scheduling API is an independent backend service responsible for computing operation schedules on-demand. This service operates in a **stateless** manner - it generates schedules dynamically without persisting results to a database.

## Architecture

- **Framework**: ASP.NET Core Web API (.NET 8)
- **Authentication**: JWT Bearer tokens (issued by AuthApi)
- **Authorization**: Role-based (only LogisticsOperator can generate schedules)
- **Integration**: Communicates with Core API via REST (HttpClient) - NO direct database access
- **API Documentation**: Swagger/OpenAPI available at `/swagger`

## Running the Service

### Prerequisites

- .NET 8 SDK
- AuthApi and Core API must be running for full functionality

### Start the service

```bash
cd SchedulingApi
dotnet run
```

The service will start on: `http://localhost:5002`

Access Swagger UI: `http://localhost:5002/swagger`

## Configuration

Configuration is stored in `appsettings.json` and `appsettings.Development.json`.

### Key Settings

- **JwtSettings**: JWT validation configuration (must match AuthApi tokens)

  - `Issuer`: ProjArqsi.AuthApi
  - `Audience`: ProjArqsi.Api
  - `SigningKey`: Shared secret key (min 32 characters)

- **CoreApiSettings**: Configuration for Core API integration

  - `BaseUrl`: Base URL of the Core API (e.g., https://localhost:7123)

- **CORS**: Configured to allow Angular frontend (http://localhost:4200)

## Endpoints

### Health Check

```
GET /api/Scheduling/health
```

No authentication required. Returns service status.

### Generate Daily Schedule (Primary Endpoint)

```
POST /api/Scheduling/daily?date=YYYY-MM-DD
Authorization: Bearer {token}
```

**Query Parameters**:

- `date` (required): Target date in YYYY-MM-DD format

**Response** - `DailyScheduleResultDto`:

```json
{
  "date": "2025-12-23",
  "isFeasible": true,
  "warnings": [
    "VVN 123e4567-e89b-12d3-a456-426614174000: No departure date specified, using default"
  ],
  "assignments": [
    {
      "vvnId": "123e4567-e89b-12d3-a456-426614174000",
      "vesselId": "987fcdeb-51a2-43d1-b789-123456789abc",
      "vesselImo": "9863382",
      "vesselName": "MSC Oscar",
      "dockId": "456e7890-e12b-34d5-c678-901234567def",
      "dockName": "Dock A",
      "eta": "2025-12-23T08:00:00",
      "etd": "2025-12-23T14:00:00",
      "estimatedTeu": 150
    }
  ]
}
```

**DailyScheduleResultDto Fields**:

- `date` (string): Target date
- `isFeasible` (bool): Whether the schedule is feasible
- `warnings` (string[]): List of warnings about incomplete data or conflicts
- `assignments` (DockAssignmentDto[]): List of dock assignments

**DockAssignmentDto Fields**:

- `vvnId` (Guid): Vessel Visit Notification ID
- `vesselId` (Guid): Vessel ID
- `vesselImo` (string, optional): IMO number for UI display
- `vesselName` (string, optional): Vessel name for UI display
- `dockId` (Guid): Assigned dock ID
- `dockName` (string, optional): Dock name for UI display
- `eta` (DateTime): Estimated Time of Arrival
- `etd` (DateTime): Estimated Time of Departure
- `estimatedTeu` (int): Estimated TEU based on cargo manifests (0 if unavailable)

**Authorization**: Requires JWT token with `LogisticsOperator` role.

**Notes**:

- Returns simple dock assignments without detailed crane/truck sequencing
- Assignments are sorted by ETA
- Estimated TEU is calculated from cargo manifests (sum of container quantities)
- Warnings are added for missing data (no dock, no vessel details, no ETD)

### Generate Schedule (Legacy)

```
POST /api/Scheduling/generate
Authorization: Bearer {token}
```

**Request Body**:

```json
{
  "targetDate": "2025-12-23",
  "algorithm": "FCFS"
}
```

**Response**:

```json
{
  "targetDate": "2025-12-23T00:00:00",
  "algorithm": "FCFS",
  "generatedAt": "2025-12-22T10:30:00Z",
  "operationPlans": [
    {
      "vesselVisitNotificationId": "guid",
      "vesselName": "MSC Oscar",
      "vesselImo": "9863382",
      "assignedDock": "Dock A",
      "plannedStartTime": "2025-12-23T08:00:00",
      "plannedEndTime": "2025-12-23T12:00:00",
      "cargoOperations": []
    }
  ]
}
```

**Authorization**: Requires JWT token with `LogisticsOperator` role.

## Integration with Core API

The Scheduling API integrates with the Core API using HttpClient to fetch:

- Approved Vessel Visit Notifications (VVNs) for a given date
- Dock information
- Vessel details

All requests to Core API include the JWT token from the current user for authentication.

## Security

- **JWT Authentication**: All scheduling endpoints require valid JWT tokens
- **Role-Based Authorization**: Only users with `LogisticsOperator` role can generate schedules
- **Token Forwarding**: User tokens are forwarded to Core API for authorization

## Future Development

- Implement multiple scheduling algorithms (FCFS, Priority-based, Genetic Algorithm, etc.)
- Add support for resource constraints (cranes, staff, storage areas)
- Implement schedule optimization based on vessel priorities
- Add caching for frequently accessed Core API data
