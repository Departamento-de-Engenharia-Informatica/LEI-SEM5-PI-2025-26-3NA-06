# OEM API - Setup Instructions

## ✅ Completed Tasks

### Backend (OEM API)

- ✅ Created new project: `ProjArqsi.OemApi`
- ✅ Configured layered architecture (Controllers/Application/Infrastructure)
- ✅ Set up Swagger/OpenAPI on port 5003
- ✅ Configured JWT authentication (shared with Backend/SchedulingApi)
- ✅ Configured CORS for SPA communication
- ✅ Implemented role-based authorization (LogisticOperator policy)
- ✅ Created `OemHealthController` with `/api/oem/health` endpoint
- ✅ Created `OemDbContext` for future database entities
- ✅ Project builds successfully

### Frontend

- ✅ Created `OemService` in `services/oem.service.ts`
- ✅ Created OEM Test page component (`logistic-operator/oem-test/`)
- ✅ Added route: `/logistic-operator/oem-test`
- ✅ Added navigation link in Logistic Operator Dashboard
- ✅ Implemented health check UI with user info display

## 🚀 Quick Start

### 1. Configure OEM API

Copy JWT settings from your Backend's `appsettings.Development.json`:

```bash
cd OemApi
# Edit appsettings.Development.json
```

Update the `SigningKey` (must match Backend/AuthApi):

```json
{
  "JwtSettings": {
    "SigningKey": "YOUR_SAME_SIGNING_KEY_FROM_BACKEND"
  }
}
```

### 2. Run OEM API

```bash
cd OemApi
dotnet run
```

The service will start on **http://localhost:5003**

Access Swagger: http://localhost:5003/swagger

### 3. Test the Integration

1. Start all services:

   - AuthApi (port 5001)
   - Backend/CoreApi (port 5218)
   - SchedulingApi (port 5002)
   - **OemApi (port 5003)** ← New!
   - Frontend (port 4200)

2. Login to the Frontend as **LogisticOperator**

3. Navigate to **Logistic Operator Dashboard** → **🔌 OEM Test**

4. Click "Test Connection"

5. You should see:
   - ✅ Connection Successful
   - Service: OEM
   - Your user details (ID, email, roles)
   - Server time

## 📂 Project Structure

```
OemApi/
├── Controllers/
│   └── OemHealthController.cs       # Health check endpoint
├── Application/
│   └── DTOs/
│       └── OemHealthResponseDto.cs  # Response models
├── Infrastructure/
│   └── OemDbContext.cs              # EF Core DbContext (empty for now)
├── Properties/
│   └── launchSettings.json          # Port 5003 configuration
├── Program.cs                        # Startup & configuration
├── appsettings.json                 # Configuration (needs SigningKey)
├── appsettings.Development.json
├── CONFIG.md                        # Detailed configuration guide
└── ProjArqsi.OemApi.csproj

Frontend/src/app/
├── services/
│   └── oem.service.ts               # OEM API client service
└── logistic-operator/
    └── oem-test/                    # Test page component
        ├── oem-test.ts
        ├── oem-test.html
        └── oem-test.css
```

## 🔐 Authentication Flow

1. Frontend obtains JWT token from AuthApi (Google OAuth)
2. Frontend includes token in Authorization header: `Bearer {token}`
3. OEM API validates token using shared JwtSettings
4. OEM API checks for **LogisticOperator** role
5. If authorized, returns health status with user info

## 🎯 API Endpoints

### Health Check

- **Endpoint**: `GET /api/oem/health`
- **Auth**: Required (Bearer token)
- **Role**: LogisticOperator
- **Response**:

```json
{
  "service": "OEM",
  "status": "ok",
  "utcNow": "2025-12-23T18:30:00Z",
  "user": {
    "id": "google|123456789",
    "username": "John Doe",
    "email": "john@example.com",
    "roles": ["LogisticOperator"]
  }
}
```

## 🔧 Troubleshooting

### Error: "Failed to connect to OEM API"

- Ensure OEM API is running: `cd OemApi && dotnet run`
- Check console shows: "Now listening on: http://localhost:5003"

### Error: 401 Unauthorized

- Verify JWT token is valid (try logging out and back in)
- Check `SigningKey` matches between Backend and OemApi
- Verify token includes LogisticOperator role

### Error: 403 Forbidden

- User doesn't have LogisticOperator role
- Check user roles in AuthApi database

### CORS Error

- Verify OemApi Program.cs has CORS policy for `http://localhost:4200`
- Check browser console for specific CORS error message

## 📊 Database Setup (Future)

For now, `OemDbContext` is empty. When implementing Operation Plan persistence:

1. Add entities to `Domain/` folder
2. Add DbSet properties to `OemDbContext`
3. Create entity configurations
4. Run migrations:

```bash
cd OemApi
dotnet ef migrations add AddOperationPlanEntity
dotnet ef database update
```

5. Update Azure connection string in `appsettings.json`

## 🎨 Frontend Navigation

- Dashboard: `/logistic-operator/dashboard`
- Daily Schedule: `/logistic-operator/daily-schedule`
- **OEM Test**: `/logistic-operator/oem-test` ← New!

## ✨ Next Steps

1. ✅ MVP Complete - Health check working
2. 🔄 Implement Operation Plan persistence (save to database)
3. 🔄 Add endpoint to create/store Operation Plans
4. 🔄 Add endpoint to retrieve Operation Plans by date
5. 🔄 Add operation execution tracking
6. 🔄 Add analytics and reporting

## 🚦 Testing Checklist

- [ ] OEM API builds without errors
- [ ] OEM API starts on port 5003
- [ ] Swagger UI accessible at http://localhost:5003/swagger
- [ ] Frontend OEM Test page loads
- [ ] "Test Connection" button works
- [ ] Health response shows correct user info
- [ ] LogisticOperator role is enforced (test with different role should fail)
- [ ] Browser console shows no CORS errors

## 📝 Configuration Files to Update

**Critical**: Update these before running:

1. `OemApi/appsettings.Development.json` - Add SigningKey (copy from Backend)
2. `OemApi/appsettings.json` - Update Azure DB connection string (when ready)

**Already Configured**:

- ✅ Port 5003
- ✅ CORS for http://localhost:4200
- ✅ JWT authentication
- ✅ LogisticOperator role policy
- ✅ Swagger/OpenAPI

---

**Status**: 🟢 MVP Ready - Health endpoint functional, awaiting JWT configuration
