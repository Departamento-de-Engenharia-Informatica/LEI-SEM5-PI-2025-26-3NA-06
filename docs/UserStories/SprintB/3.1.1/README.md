# US 3.1.1 - Set Up SPA Framework

## Descrição

As a Project Manager, I want the team to set up the SPA using a modern framework, so that future features can be developed in a maintainable way.

## Critérios de Aceitação

- SPA must be built using a framework such as Angular, React or Vue.
- A modular folder structure (e.g., components, services, pages, routing) is required.
- The SPA must adopt a well-founded HTTP client (e.g., Axios/Fetch) for REST API consumption.

## 3. Análise

### 3.1. Domínio

**Componentes Infraestruturais:**

Esta US estabelece a infraestrutura base do sistema:

**Backend (.NET Core):**

- SQL Server Database com Entity Framework Core
- RESTful API com controllers e services
- DDD architecture: Domain, Application, Infrastructure layers
- Authentication/Authorization com JWT

**AuthApi (.NET Core):**

- Google OAuth 2.0 integration
- JWT token generation
- User session management

**SchedulingApi (.NET Core):**

- Scheduling algorithm implementation
- Integration with Backend for VVN data

**OemApiNode (Node.js):**

- Express.js REST API
- SQL Server integration
- Operation execution management

**Frontend (Angular):**

- SPA with routing
- Services for API communication
- Guards for authentication

### 3.2. Regras de Negócio

1. All APIs must use JWT authentication
2. CORS configured to allow Frontend access
3. Environment-specific configurations (Development, Testing, Production)
4. Centralized logging for all services
5. Database migrations managed via EF Core
6. Health checks for service monitoring
7. Error handling with consistent response formats

### 3.3. Casos de Uso

#### UC1 - System Initialization

Developers set up all services, databases, and configurations for first deployment.

#### UC2 - Service Health Check

Monitoring system verifies all services are running and database connections are active.

#### UC3 - Environment Configuration

Administrators configure environment-specific settings (DB connections, API URLs, secrets).

### 3.4. API Routes

| Method | Endpoint | Description                        | Auth Required |
| ------ | -------- | ---------------------------------- | ------------- |
| N/A    | N/A      | This is an infrastructure setup US | N/A           |

## 4. Design

### 4.1. Diagrama de Sequência do Sistema (SSD)

[View SSD Diagram](SSD/SSD.puml)

### 4.2. Diagrama de Sequência Detalhado

[View SD Diagram](SD/SD.puml)

### 4.3. Modelo de Domínio

[View DM Diagram](DM/DM.puml)

## 5. Implementação

### Abordagem

A implementação desta User Story envolveu a configuração da infraestrutura completa do sistema, incluindo:

1. **Backend (.NET Core)**: API RESTful com arquitetura DDD, autenticação JWT e Entity Framework Core
2. **AuthApi (.NET Core)**: Serviço dedicado para autenticação via Google OAuth
3. **SchedulingApi (.NET Core)**: Serviço de geração de agendamentos
4. **OemApiNode (Node.js)**: API para gestão de execução de operações
5. **Frontend (Angular)**: SPA com routing, services e guards

A estrutura modular permite que cada serviço opere independentemente enquanto mantém comunicação via REST.

### Excertos de Código Relevantes

**1. Angular App Configuration (app.config.ts)**

```typescript
import { ApplicationConfig } from "@angular/core";
import { provideRouter } from "@angular/router";
import { provideHttpClient, withInterceptors } from "@angular/common/http";
import { routes } from "./app.routes";
import { authInterceptor } from "./interceptors/auth.interceptor";

export const appConfig: ApplicationConfig = {
  providers: [
    provideRouter(routes),
    provideHttpClient(withInterceptors([authInterceptor])),
  ],
};
```

**2. Backend Program.cs - API Configuration**

```csharp
var builder = WebApplication.CreateBuilder(args);

// Add services
builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

// Database
builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));

// JWT Authentication
builder.Services.AddJwtAuthentication(builder.Configuration);

// CORS
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowFrontend", policy =>
    {
        policy.WithOrigins("http://localhost:4200")
              .AllowAnyMethod()
              .AllowAnyHeader();
    });
});

var app = builder.Build();
app.UseCors("AllowFrontend");
app.UseAuthentication();
app.UseAuthorization();
app.MapControllers();
app.Run();
```

**3. OemApiNode Server Setup (src/app.js)**

```javascript
const express = require("express");
const cors = require("cors");
const { connectDatabase } = require("./config/database");
const logger = require("./utils/logger");

const app = express();

// Middleware
app.use(cors({ origin: "http://localhost:4200" }));
app.use(express.json());

// Routes
app.use("/api/operation-plans", require("./routes/operationPlanRoutes"));
app.use("/api/incidents", require("./routes/incidentRoutes"));

// Database connection
connectDatabase();

const PORT = process.env.PORT || 5004;
app.listen(PORT, () => {
  logger.info(`OEM API Node running on port ${PORT}`);
});

module.exports = app;
```

## 6. Testes

### Como Executar: `npm run test -- --testPathPattern="app.component"` | `npm run cypress:open`

### Testes: ~50+ (Unit 10+, E2E 5 files)

### Excertos

**1. App Component**: `expect(app.title).toBe('port-management-system')`
**2. Routing**: `expect(router.config.length).toBeGreaterThan(0)`
**3. E2E**: `cy.visit('/') → cy.get('app-root').should('exist')`

## 7. Observações

### Conformidade com Critérios de Aceitação

✅ **Todos os critérios infraestruturais implementados:**

1. **Modern Framework**: Sistema usa **Angular** (versão moderna) como SPA framework.

2. **Estrutura Modular**: Organização clara em `components/`, `services/`, `pages/`, `guards/`, `interceptors/`.

3. **HTTP Client**: Angular `HttpClient` para consumo de REST APIs (TypeScript typed).

### Destaques da Implementação

- **Arquitetura Multi-Serviço**: Frontend integra com 4 APIs (Backend, AuthApi, SchedulingApi, OemApiNode).
- **TypeScript**: Type safety em toda a aplicação (interfaces, DTOs, services).
- **Routing**: Angular Router com lazy loading para performance.
- **RxJS**: Programação reativa para gestão de estado e comunicação assíncrona.

### Arquitetura Implementada

- **Services Layer**: Abstrai comunicação com APIs (VesselService, VVNService, AuthService, etc.).
- **Guards**: Proteção de rotas com AuthGuard e RoleGuard.
- **Interceptors**: JWT token injection automático, error handling global.
- **Shared Module**: Componentes reutilizáveis (ex: loading spinner, error messages).

### Observações Técnicas

- Setup permite desenvolvimento escalável para 28 User Stories.
- Environment configurations (Development, Testing, Production).
- Build otimizado com AOT compilation e tree-shaking.
