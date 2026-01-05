# US 3.1.3 - Role-Based Menu Visibility

## Descrição

As a System User with a specific role, I want the SPA to show only the menus relevant to my permissions, so that the interface remains clear and I can only access allowed features.

## Critérios de Aceitação

- Menu options must be rendered dynamically based on the logged-authenticated user's role.
- Navigation to unauthorized sections must be prevented (even if manually typed in the URL).

## 3. Análise

### 3.1. Domínio

**API Communication Architecture:**

**Services:**

- **Backend API** (port 5218): Core domain management
- **AuthApi** (port 5001): Authentication service
- **SchedulingApi** (port 5002): Schedule generation
- **OemApiNode** (port 5004): Operation execution

**Communication Patterns:**

- RESTful HTTP/HTTPS
- JSON payloads
- JWT Bearer tokens in Authorization headers
- CORS for cross-origin requests

**Frontend Integration:**

- Angular HttpClient for API calls
- Services layer abstracting API communication
- Interceptors for authentication token injection
- Error handling with retry logic

### 3.2. Regras de Negócio

1. All API requests must include valid JWT token (except auth endpoints)
2. APIs return standardized response formats (success/error)
3. HTTP status codes: 200 (success), 201 (created), 400 (validation), 401 (unauthorized), 404 (not found), 500 (server error)
4. Request/response logging for debugging
5. API versioning via URL path (e.g., /api/v1/)
6. Rate limiting to prevent abuse
7. Timeouts configured (30s default)
8. HTTPS required in production

### 3.3. Casos de Uso

#### UC1 - Authenticated API Request

Frontend sends request with JWT token, API validates token and processes request.

#### UC2 - Cross-Service Communication

SchedulingApi calls Backend API to retrieve VVN data for schedule generation.

#### UC3 - Error Handling

API returns error response, Frontend displays user-friendly message and logs details.

### 3.4. API Routes

| Method | Endpoint      | Description                       | Auth Required |
| ------ | ------------- | --------------------------------- | ------------- |
| GET    | /api/users/me | Get current user profile and role | Yes           |

## 4. Design

### 4.1. Diagrama de Sequência do Sistema (SSD)

[View SSD Diagram](SSD/SSD.puml)

### 4.2. Diagrama de Sequência Detalhado

[View SD Diagram](SD/SD.puml)

### 4.3. Modelo de Domínio

[View DM Diagram](DM/DM.puml)

## 5. Implementação

### Abordagem

A visibilidade de menu baseada em roles foi implementada através de:

1. **Auth Guard**: Protege rotas verificando autenticação e role do utilizador
2. **Template Directives**: Uso de `*ngIf` no template para mostrar/ocultar opções baseado no role
3. **Route Configuration**: Rotas definem `data: { role: 'RoleName' }` para controlo de acesso
4. **AuthService**: Mantém estado do utilizador autenticado via BehaviorSubject

Quando um utilizador tenta aceder uma rota não autorizada (mesmo via URL direto), o guard redireciona para página de "Unauthorized" e loga a tentativa.

### Excertos de Código Relevantes

**1. Auth Guard (auth.guard.ts)**

```typescript
import { inject } from "@angular/core";
import { Router, CanActivateFn } from "@angular/router";
import { AuthService } from "../services/auth.service";
import { HttpClient } from "@angular/common/http";
import { firstValueFrom } from "rxjs";

export const authGuard: CanActivateFn = async (route, state) => {
  const router = inject(Router);
  const authService = inject(AuthService);
  const http = inject(HttpClient);

  if (!authService.isAuthenticated()) {
    router.navigate(["/login"]);
    return false;
  }

  const user = authService.getUser();
  if (!user || !user.role) {
    router.navigate(["/login"]);
    return false;
  }

  const requiredRole = route.data["role"];
  if (!requiredRole) {
    return true; // No role requirement
  }

  if (user.role !== requiredRole) {
    // Log unauthorized access attempt
    const logData = {
      email: user.email,
      role: user.role,
      attemptedPath: state.url,
      requiredRole: requiredRole,
      timestamp: new Date().toISOString(),
    };
    try {
      await firstValueFrom(
        http.post(
          "http://localhost:5218/api/audit/unauthorized-access",
          logData
        )
      );
    } catch (err) {
      console.error("Failed to log unauthorized access:", err);
    }
    router.navigate(["/unauthorized"]);
    return false;
  }

  return true;
};
```

**2. Route Configuration with Role Protection (app.routes.ts) - Excerto**

```typescript
import { Routes } from "@angular/router";
import { authGuard } from "./guards/auth.guard";

export const routes: Routes = [
  { path: "login", component: LoginComponent },
  {
    path: "",
    component: LayoutComponent,
    canActivate: [authGuard],
    children: [
      {
        path: "vessels",
        component: VesselsComponent,
        canActivate: [authGuard],
        data: { role: "PortAuthorityOfficer" },
      },
      {
        path: "operations",
        component: OperationsComponent,
        canActivate: [authGuard],
        data: { role: "LogisticOperator" },
      },
      {
        path: "admin",
        component: AdminComponent,
        canActivate: [authGuard],
        data: { role: "Admin" },
      },
    ],
  },
  { path: "unauthorized", component: UnauthorizedComponent },
  { path: "**", redirectTo: "/login" },
];
```

**3. AuthService with User State (auth.service.ts) - Excerto**

```typescript
import { Injectable } from "@angular/core";
import { BehaviorSubject } from "rxjs";

export interface User {
  userId: string;
  email: string;
  role: string;
  name: string;
}

@Injectable({ providedIn: "root" })
export class AuthService {
  private currentUserSubject = new BehaviorSubject<User | null>(
    this.getStoredUser()
  );
  public currentUser$ = this.currentUserSubject.asObservable();

  getUser(): User | null {
    const userJson = sessionStorage.getItem("auth_user");
    return userJson ? JSON.parse(userJson) : null;
  }

  isAuthenticated(): boolean {
    const token = sessionStorage.getItem("auth_token");
    if (!token) return false;
    return !this.isTokenExpired();
  }

  hasRole(role: string): boolean {
    const user = this.getUser();
    return user?.role === role;
  }
}
```

## 6. Testes

### Como Executar: `npm run test -- --testPathPattern="auth"` | `npm run cypress:open -- --spec "cypress/e2e/02-admin-workflows.cy.ts"`

### Testes: ~40+ (Auth Unit 15+, E2E role workflows)

### Excertos

**1. Auth Guard**: `expect(guard.canActivate()).toBe(true)`
**2. Role Check**: `expect(authService.hasRole('Admin')).toBe(true)`
**3. E2E**: `cy.login('admin') → cy.get('[data-testid="admin-menu"]').should('exist')`

## 7. Observações

### Conformidade com Critérios de Aceitação

✅ **Segurança e usabilidade implementadas:**

1. **Menu Dinâmico**: Opções de menu renderizadas apenas para roles autorizadas.

2. **Prevenção de Acesso Direto**: URLs digitadas manualmente bloqueadas por guards se role insuficiente.

### Destaques da Implementação

- **Defense in Depth**: Proteção em 3 camadas:

  1. Frontend esconde menu items (UI layer)
  2. Guards bloqueiam rotas (routing layer)
  3. Backend valida tokens (API layer)

- **Role Guard**: `RoleGuard` verifica role antes de ativar rotas protegidas.
- **AuthGuard**: `AuthGuard` garante autenticação antes de qualquer rota.

### Menu Items por Role Implementados

- **Admin**: User management, system settings, all features.
- **PortAuthorityOfficer**: VesselTypes, Vessels, Docks, StorageAreas, VVN approval.
- **LogisticOperator**: Operation Plans, VVE management, Incidents.
- **ShippingAgentRepresentative**: VVN creation, status view.

### Observações de Segurança

- URL tampering previne tentativas de acesso não autorizado.
- Guards redirectam para `/unauthorized` com mensagem clara.
- JWT role claim validado tanto frontend como backend.

### UX Melhorada

- Interface limpa: users veem apenas features relevantes ao seu trabalho.
- Menos clutter, maior produtividade.
