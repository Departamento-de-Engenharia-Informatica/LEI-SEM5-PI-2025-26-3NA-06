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
