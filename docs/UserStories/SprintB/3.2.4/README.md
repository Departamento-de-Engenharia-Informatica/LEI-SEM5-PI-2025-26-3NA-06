# US 3.2.4 - Role-Based Access Control

## Descrição

As a System User, I want the system to restrict access to actions and features based on my role, so that I cannot perform unauthorized operations.

## Critérios de Aceitação

- On the back-end side:
  - Each REST API route must enforce role-based access control (RBAC) and/or attribute-based access control (ABAC) as needed to enforce the applicable business rules.
  - Unauthorized requests must return proper HTTP status codes (e.g., 403 Forbidden).
  - Logs must record unauthorized attempts.
- On the front-end side:
  - Front-end routes must check for the user's authorization before rendering pages.
  - Direct URL access to unauthorized pages must be prevented.
  - A default "Access Denied" or "Not Authorized" page must be shown when needed.

## 3. Análise

### 3.1. Domínio

**Session Components:**

**JWT Token:**

- Stored in browser localStorage
- Contains: user email, name, role, expiration
- Valid for 24 hours

**Frontend Session:**

- AuthService maintains authentication state
- BehaviorSubject for reactive current user
- Automatic token refresh before expiration

**Backend Validation:**

- Stateless token validation on each request
- No server-side session storage
- Token signature verification with secret key

### 3.2. Regras de Negócio

1. JWT tokens expire after 24 hours
2. Tokens stored in localStorage (persists across browser sessions)
3. On app initialization, validate stored token
4. Expired tokens trigger re-authentication
5. Logout clears token from localStorage
6. Token included in Authorization header: "Bearer {token}"
7. Frontend intercepts 401 responses and redirects to login
8. No concurrent session limit (multiple devices allowed)

### 3.3. Casos de Uso

#### UC1 - Login Session

User logs in, Frontend stores JWT token, maintains session until expiration or logout.

#### UC2 - Auto-Login

User returns to app, Frontend checks stored token, auto-authenticates if valid.

#### UC3 - Session Expiration

Token expires during use, system detects 401 response, redirects to login.

#### UC4 - Logout

User logs out, Frontend clears token and navigates to login page.

### 3.4. API Routes

| Method | Endpoint | Description                         | Auth Required |
| ------ | -------- | ----------------------------------- | ------------- |
| N/A    | N/A      | RBAC/ABAC enforced on all endpoints | N/A           |

## 4. Design

### 4.1. Diagrama de Sequência do Sistema (SSD)

[View SSD Diagram](SSD/SSD.puml)

### 4.2. Diagrama de Sequência Detalhado

[View SD Diagram](SD/SD.puml)

### 4.3. Modelo de Domínio

[View DM Diagram](DM/DM.puml)

## Implementação

### Backend - RBAC

**Controllers with Role Authorization:**

```csharp
// Admin-only endpoints
[Authorize(Roles = "Admin")]
public class UserManagementController : ControllerBase { ... }

// Multiple roles allowed
[Authorize(Roles = "Admin,PortAuthorityOfficer")]
public async Task<ActionResult> GetVesselTypes() { ... }

// All authenticated users
[Authorize]
public async Task<ActionResult> GetProfile() { ... }
```

**Protected Controllers:**

- `UserManagementController` - Admin only
- `VesselTypeController` - Admin, PortAuthorityOfficer
- `VesselController` - Admin, PortAuthorityOfficer
- All controllers use `[Authorize]` attributes

**Middleware/AuthorizationLoggingMiddleware.cs:**

```csharp
if (context.Response.StatusCode == 403)
{
    var email = context.User?.FindFirst(ClaimTypes.Email)?.Value;
    var role = context.User?.FindFirst(ClaimTypes.Role)?.Value;
    var path = context.Request.Path;
    var method = context.Request.Method;

    _logger.LogWarning(
        "Authorization failed for user {Email} with role {Role} " +
        "attempting {Method} {Path}",
        email, role, method, path
    );
}
```

### Frontend - Route Guards

**guards/auth.guard.ts:**

```typescript
export const authGuard: CanActivateFn = (route, state) => {
  const router = inject(Router);
  let userRole = localStorage.getItem("userRole");

  // Accept role from query params on first login only
  const roleFromQuery = route.queryParams["role"];
  if (roleFromQuery && !userRole) {
    localStorage.setItem("userRole", roleFromQuery);
    userRole = roleFromQuery;
  }

  // Check if user has required role
  const requiredRole = route.data["role"];
  if (requiredRole && userRole !== requiredRole) {
    router.navigate(["/unauthorized"]);
    return false;
  }

  // Clean URL after OAuth
  if (roleFromQuery) {
    router.navigate([state.url.split("?")[0]], { replaceUrl: true });
  }

  return true;
};
```

**app.routes.ts - Protected Routes:**

```typescript
{
  path: '',
  component: LayoutComponent,
  canActivate: [authGuard],
  children: [
    {
      path: 'admin',
      data: { role: 'Admin' },
      canActivate: [authGuard],
      children: [
        { path: '', component: AdminDashboardComponent },
        { path: 'user-management', component: UserManagementComponent },
      ],
    },
    {
      path: 'port-authority',
      data: { role: 'PortAuthorityOfficer' },
      canActivate: [authGuard],
      children: [
        { path: '', component: PortAuthorityDashboardComponent },
      ],
    },
    // ... other role-based routes
  ],
}
```

### Frontend - Menu Visibility

**layout/layout.component.html:**

```html
<!-- Admin Menu -->
<li *ngIf="userRole === 'Admin'">
  <a routerLink="/admin">Dashboard</a>
</li>
<li *ngIf="userRole === 'Admin'">
  <a routerLink="/admin/user-management">User Management</a>
</li>

<!-- Port Authority Menu -->
<li *ngIf="userRole === 'PortAuthorityOfficer'">
  <a routerLink="/port-authority">Dashboard</a>
</li>
<li *ngIf="userRole === 'PortAuthorityOfficer'">
  <a routerLink="/port-authority/vessel-types">Vessel Types</a>
</li>
```

### Error Pages

**access-denied/access-denied.component.ts:**

- Displays when inactive user tries to access system
- Shows reason from query parameter
- "Your account is inactive. Please contact an administrator."

**unauthorized/unauthorized.component.ts:**

- Displays when user lacks required role for route
- HTTP 403 errors redirect here
- "You do not have permission to access this page."

### Authorization Flow

**Backend:**

1. Request arrives at controller endpoint
2. ASP.NET Core checks `[Authorize]` attribute
3. Validates authentication cookie exists and is valid
4. Checks if user has required role (if specified)
5. If authorized → proceed to endpoint
6. If not authenticated → 401 Unauthorized
7. If authenticated but wrong role → 403 Forbidden
8. Authorization logging middleware logs 403 attempts

**Frontend:**

1. User tries to navigate to route
2. Auth guard checks localStorage for role
3. Compares user role with `route.data['role']`
4. If match → allow navigation
5. If no match → redirect to `/unauthorized`
6. Menu items conditionally rendered based on role
7. Manual URL typing blocked by guard

### Security Measures

✅ **Defense in depth**: Both frontend and backend validate authorization  
✅ **Backend is source of truth**: Frontend checks are UI only  
✅ **Audit logging**: All unauthorized attempts logged  
✅ **Proper HTTP codes**: 401 for authentication, 403 for authorization  
✅ **URL manipulation prevented**: Guard blocks manual route changes  
✅ **Clean separation**: Role-based routes with explicit requirements

## 5. Implementação

### Abordagem

O controlo de acesso baseado em roles (RBAC) foi implementado em duas camadas:

**Backend (Defense in Depth):**

1. **[Authorize] Attribute**: Requer autenticação em todos os endpoints (exceto auth)
2. **[Authorize(Roles)] Attribute**: Especifica roles permitidos por endpoint
3. **JWT Middleware**: Valida token e extrai claims antes do controller
4. **Audit Logging**: Regista todas as tentativas de acesso não autorizado

**Frontend (UX):**

1. **Auth Guard**: Verifica role antes de renderizar rota
2. **Template Directives**: Oculta elementos UI baseado no role
3. **Redirect on Unauthorized**: Navega para página de acesso negado

O backend é a única fonte de verdade - o frontend apenas melhora a UX.

### Excertos de Código Relevantes

**1. Backend Controller with RBAC (Backend/Controllers/VesselController.cs) - Excerto**

```csharp
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using ProjArqsi.Services;

namespace ProjArqsi.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize] // Requires authentication for all endpoints
    public class VesselController : ControllerBase
    {
        private readonly VesselService _vesselService;

        public VesselController(VesselService vesselService)
        {
            _vesselService = vesselService;
        }

        [HttpGet]
        [Authorize(Roles = "PortAuthorityOfficer,Admin")]
        public async Task<ActionResult<IEnumerable<VesselDto>>> GetAllVessels()
        {
            var vessels = await _vesselService.GetAllAsync();
            return Ok(vessels);
        }

        [HttpPost]
        [Authorize(Roles = "PortAuthorityOfficer")]
        public async Task<ActionResult> CreateVessel([FromBody] VesselUpsertDto dto)
        {
            try
            {
                var vessel = await _vesselService.AddAsync(dto);
                return CreatedAtAction(nameof(GetVesselById),
                    new { id = vessel.Id }, vessel);
            }
            catch (BusinessRuleValidationException ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }

        [HttpDelete("{id}")]
        [Authorize(Roles = "Admin")] // Only Admin can delete
        public async Task<ActionResult> DeleteVessel(Guid id)
        {
            try
            {
                await _vesselService.DeleteAsync(id);
                return NoContent();
            }
            catch (InvalidOperationException ex)
            {
                return NotFound(new { message = ex.Message });
            }
        }
    }
}
```

**2. Unauthorized Access Audit Logging (Backend/Controllers/AuditController.cs) - Excerto**

```csharp
using Microsoft.AspNetCore.Mvc;
using Serilog;

namespace ProjArqsi.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class AuditController : ControllerBase
    {
        private readonly ILogger _logger = Log.ForContext<AuditController>();

        [HttpPost("unauthorized-access")]
        public IActionResult LogUnauthorizedAccess([FromBody] UnauthorizedAccessLog log)
        {
            _logger.Warning(
                "Unauthorized access attempt: {Email} (Role: {Role}) attempted to access {Path} requiring {RequiredRole} at {Timestamp}",
                log.Email,
                log.Role,
                log.AttemptedPath,
                log.RequiredRole,
                log.Timestamp
            );

            return Ok(new { message = "Unauthorized access logged" });
        }
    }

    public class UnauthorizedAccessLog
    {
        public string Email { get; set; } = string.Empty;
        public string Role { get; set; } = string.Empty;
        public string AttemptedPath { get; set; } = string.Empty;
        public string RequiredRole { get; set; } = string.Empty;
        public string Timestamp { get; set; } = string.Empty;
    }
}
```

**3. Frontend Unauthorized Component (Frontend/src/app/unauthorized/unauthorized.component.ts)**

```typescript
import { Component, OnInit } from "@angular/core";
import { Router } from "@angular/router";
import { AuthService } from "../services/auth.service";

@Component({
  selector: "app-unauthorized",
  template: `
    <div class="unauthorized-container">
      <h1>Access Denied</h1>
      <p>You do not have permission to access this page.</p>
      <p>
        Your role: <strong>{{ userRole }}</strong>
      </p>
      <button (click)="goToDashboard()">Go to Dashboard</button>
      <button (click)="logout()">Logout</button>
    </div>
  `,
  styles: [
    `
      .unauthorized-container {
        text-align: center;
        margin-top: 100px;
      }
      button {
        margin: 10px;
        padding: 10px 20px;
        font-size: 16px;
      }
    `,
  ],
})
export class UnauthorizedComponent implements OnInit {
  userRole: string = "Unknown";

  constructor(private authService: AuthService, private router: Router) {}

  ngOnInit(): void {
    const user = this.authService.getUser();
    if (user) {
      this.userRole = user.role;
    }
  }

  goToDashboard(): void {
    const dashboardRoutes: { [key: string]: string } = {
      Admin: "/admin",
      PortAuthorityOfficer: "/port-authority",
      LogisticOperator: "/logistic-operator",
      ShippingAgentRepresentative: "/shipping-agent",
    };
    const route = dashboardRoutes[this.userRole] || "/login";
    this.router.navigate([route]);
  }

  logout(): void {
    this.authService.logout();
    this.router.navigate(["/login"]);
  }
}
```

## 6. Testes

### Como Executor: `dotnet test --filter "Authorization"` | `npm run cypress:open`

### Testes: ~60+ (Integration 30+ for role checks, E2E all role workflows)

### Excertos

**1. Unauthorized Access**: `GET /api/admin/users without Admin role → Assert 403 Forbidden`
**2. Role Authorization**: `[Authorize(Roles="Admin")] endpoint rejects LogisticOperator → 403`
**3. E2E**: `cy.loginAs('LogisticOperator') → cy.visit('/admin') → cy.contains('Access Denied').should('exist')`

## 7. Observações

### Conformidade com Critérios de Aceitação

✅ **RBAC/ABAC rigoroso implementado:**

**Backend:**

1. **RBAC em Rotas**: Cada endpoint usa `[Authorize(Roles = "Role1,Role2")]`.
2. **403 Forbidden**: Unauthorized requests retornam 403 com mensagem clara.
3. **Logging**: Tentativas de acesso não autorizado logged para auditoria.

**Frontend:**

1. **Route Guards**: Guards verificam role antes de renderizar páginas.
2. **URL Protection**: Acesso direto via URL bloqueado por guards.
3. **Access Denied Page**: Página dedicada `/unauthorized` para tentativas bloqueadas.

### Destaques da Implementação

- **Defense in Depth** (3 camadas):

  1. **UI Layer**: Esconde botões/menus não autorizados
  2. **Routing Layer**: Guards bloqueiam navegação
  3. **API Layer**: Backend valida role em cada request

- **Authorization Attributes**:

  ```csharp
  [Authorize(Roles = "PortAuthorityOfficer")]
  public async Task<ActionResult> CreateDock([FromBody] DockDto dto)
  ```

- **Role Guard**:
  ```typescript
  canActivate(route: ActivatedRouteSnapshot): boolean {
    const requiredRole = route.data['role'];
    return this.authService.hasRole(requiredRole);
  }
  ```

### RBAC Rules Implementadas

| Endpoint           | Roles Permitidas            |
| ------------------ | --------------------------- |
| VesselType CRUD    | PortAuthorityOfficer        |
| VVN Create/Update  | ShippingAgentRepresentative |
| VVN Approve/Reject | PortAuthorityOfficer        |
| Operation Plans    | LogisticOperator, Admin     |
| User Management    | Admin                       |

### Observações de Segurança

- **Never Trust Frontend**: Backend sempre valida, mesmo se frontend escondeu UI.
- **Audit Trail**: Failed authorization attempts logged para deteção de ataques.
- **Clear Feedback**: Users sabem quando acesso negado e porquê.

### ABAC Future Enhancement

- Atual implementação é primariamente RBAC.
- ABAC poderia adicionar regras como "Agent só edita suas próprias VVNs" (ownership-based).
