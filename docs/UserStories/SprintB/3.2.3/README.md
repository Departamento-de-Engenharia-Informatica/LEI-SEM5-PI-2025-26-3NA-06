# US 3.2.3 - Maintain Secure Session

## Descrição

As a System User, I want my authenticated session to be maintained securely, so that I don't need to re-login frequently while using the SPA.

## Critérios de Aceitação

- Access tokens must be securely stored.
- Token expiration must be handled (e.g., silent refresh or forced re-login when invalid).
- The SPA must try to avoid unauthorized API calls by, for instance, attaching the user access token to requests.
- Back-end module(s) must also validate tokens on each request.

## 3. Análise

### 3.1. Domínio

**Authorization Model:**

**Roles:**

- **PortAuthorityOfficer**: Port infrastructure management
- **LogisticOperator**: Operations planning and execution
- **ShippingAgentRepresentative**: Vessel visit notifications
- **Admin**: System administration

**Authorization Attributes:**

- [Authorize]: Requires authentication
- [Authorize(Roles = "Role1,Role2")]: Requires specific role(s)

**Frontend Guards:**

- AuthGuard: Checks authentication
- RoleGuard: Checks role permissions

### 3.2. Regras de Negócio

1. Every API endpoint (except auth) requires authentication
2. Endpoints specify allowed roles via [Authorize(Roles)] attribute
3. JWT token role claim determines user permissions
4. Frontend hides UI elements user cannot access
5. Backend validates roles on every request (defense in depth)
6. Role hierarchy: Admin > all other roles
7. Unauthorized access returns 403 Forbidden
8. Token expiration returns 401 Unauthorized

### 3.3. Casos de Uso

#### UC1 - Authorized Access

User with correct role accesses protected endpoint successfully.

#### UC2 - Unauthorized Access

User attempts to access endpoint without required role, system returns 403 error.

#### UC3 - Token Validation

API validates JWT signature, expiration, and role claim before processing request.

### 3.4. API Routes

| Method | Endpoint | Description                    | Auth Required |
| ------ | -------- | ------------------------------ | ------------- |
| N/A    | N/A      | Session management via cookies | N/A           |

## 4. Design

### 4.1. Diagrama de Sequência do Sistema (SSD)

[View SSD Diagram](SSD/SSD.puml)

### 4.2. Diagrama de Sequência Detalhado

[View SD Diagram](SD/SD.puml)

### 4.3. Modelo de Domínio

[View DM Diagram](DM/DM.puml)

## Implementação

### Backend

**Program.cs** - Cookie Authentication:

```csharp
builder.Services.AddAuthentication(options =>
{
    options.DefaultAuthenticateScheme = CookieAuthenticationDefaults.AuthenticationScheme;
    options.DefaultSignInScheme = CookieAuthenticationDefaults.AuthenticationScheme;
    options.DefaultChallengeScheme = GoogleDefaults.AuthenticationScheme;
})
.AddCookie(options =>
{
    options.Cookie.HttpOnly = true;  // Prevents JavaScript access (XSS protection)
    options.Cookie.SecurePolicy = CookieSecurePolicy.Always;  // HTTPS only
    options.Cookie.SameSite = SameSiteMode.Lax;
    options.ExpireTimeSpan = TimeSpan.FromHours(24);  // 24-hour session
})
.AddGoogle(options => { ... });
```

**Controllers** - Authorization attributes:

- All protected endpoints use `[Authorize]` or `[Authorize(Roles = "...")]`
- ASP.NET Core validates authentication cookie on every request
- Returns 401 Unauthorized if cookie invalid/expired
- Returns 403 Forbidden if user lacks required role

**Middleware/AuthorizationLoggingMiddleware.cs**:

- Logs all 403 Forbidden attempts
- Records: user email, role, requested path, HTTP method
- Helps audit unauthorized access attempts

### Frontend

**interceptors/auth.interceptor.ts**:

```typescript
export const authInterceptor: HttpInterceptorFn = (req, next) => {
  // Attach credentials (cookies) to every request
  const authReq = req.clone({ withCredentials: true });

  return next(authReq).pipe(
    catchError((error: HttpErrorResponse) => {
      if (error.status === 401) {
        // Session expired - redirect to login
        localStorage.removeItem("userRole");
        router.navigate(["/login"]);
      } else if (error.status === 403) {
        // Forbidden - insufficient permissions
        router.navigate(["/unauthorized"]);
      }
      return throwError(() => error);
    })
  );
};
```

**app.config.ts**:

- Interceptor registered globally: `provideHttpClient(withInterceptors([authInterceptor]))`
- All HTTP requests automatically include credentials

### Session Management

**Secure Storage:**

- Authentication: httpOnly cookies (not accessible to JavaScript)
- Role: localStorage (UI display only, not for security decisions)

**Token Validation:**

- Every API request includes httpOnly cookie
- Backend validates cookie on each request
- Invalid/expired cookie → 401 response → redirect to login

**Session Expiry:**

- Cookie expires after 24 hours of inactivity
- No silent refresh implemented (acceptable for Sprint B)
- User must re-authenticate after expiry
- On 401: localStorage cleared, redirect to `/login`

### Security Benefits

✅ **httpOnly cookies**: Protected from XSS attacks  
✅ **Secure flag**: Cookies only sent over HTTPS  
✅ **SameSite=Lax**: CSRF protection  
✅ **Automatic credential attachment**: Interceptor adds cookies to all requests  
✅ **Centralized error handling**: Interceptor handles 401/403 globally  
✅ **No token in localStorage**: Cannot be stolen by malicious scripts

## 5. Implementação

### Abordagem

A sessão segura foi implementada usando JWT armazenado em `sessionStorage` (limpa quando o browser fecha):

1. **Token Storage**: sessionStorage ao invés de localStorage para maior segurança
2. **Token Validation**: Backend valida assinatura, expiração e claims em cada request
3. **HTTP Interceptor**: Adiciona token automaticamente ao header Authorization
4. **Expiration Handling**: Frontend verifica expiração antes de cada chamada
5. **Auto-Logout**: Redireciona para login se token expirar ou for inválido

O token JWT tem validade de 24 horas e contém claims do utilizador. Não há refresh token implementado no Sprint B.

### Excertos de Código Relevantes

**1. Auth Interceptor (Frontend/src/app/interceptors/auth.interceptor.ts)**

```typescript
import { HttpInterceptorFn, HttpErrorResponse } from "@angular/common/http";
import { inject } from "@angular/core";
import { Router } from "@angular/router";
import { catchError, throwError } from "rxjs";
import { AuthService } from "../services/auth.service";

export const authInterceptor: HttpInterceptorFn = (req, next) => {
  const router = inject(Router);
  const authService = inject(AuthService);

  // Get the token from AuthService
  const token = authService.getToken();

  // Clone the request and add Authorization header if token exists
  const authReq = token
    ? req.clone({
        setHeaders: {
          Authorization: `Bearer ${token}`,
        },
      })
    : req;

  return next(authReq).pipe(
    catchError((error: HttpErrorResponse) => {
      if (error.status === 401) {
        // Unauthorized - token expired or invalid
        console.warn("Token expired or unauthorized. Redirecting to login...");
        authService.logout();
        router.navigate(["/login"]);
      } else if (error.status === 403) {
        // Forbidden - user doesn't have permission
        console.warn("Access forbidden. User lacks required permissions.");
        router.navigate(["/unauthorized"]);
      }
      return throwError(() => error);
    })
  );
};
```

**2. JWT Authentication Configuration (Backend/Auth.Common/JwtAuthenticationExtensions.cs)**

```csharp
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.IdentityModel.Tokens;
using System.Text;

namespace ProjArqsi.Auth.Common;

public static class JwtAuthenticationExtensions
{
    public static IServiceCollection AddJwtAuthentication(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        var jwtSettings = configuration.GetSection("JwtSettings").Get<JwtSettings>()
            ?? throw new InvalidOperationException("JwtSettings not configured");

        services.AddSingleton(jwtSettings);
        services.AddSingleton<JwtTokenGenerator>();

        services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
            .AddJwtBearer(options =>
            {
                options.TokenValidationParameters = new TokenValidationParameters
                {
                    ValidateIssuer = true,
                    ValidateAudience = true,
                    ValidateLifetime = true,
                    ValidateIssuerSigningKey = true,
                    ValidIssuer = jwtSettings.Issuer,
                    ValidAudience = jwtSettings.Audience,
                    IssuerSigningKey = new SymmetricSecurityKey(
                        Encoding.UTF8.GetBytes(jwtSettings.SigningKey)),
                    ClockSkew = TimeSpan.Zero // No tolerance for expired tokens
                };
            });

        return services;
    }
}
```

**3. Token Expiration Check (Frontend/src/app/services/auth.service.ts) - Excerto**

```typescript
export class AuthService {
  private tokenKey = "auth_token";
  private tokenExpiryKey = "auth_token_expiry";

  getToken(): string | null {
    const token = sessionStorage.getItem(this.tokenKey);
    if (!token) return null;

    const expired = this.isTokenExpired();
    if (token && !expired) {
      return token;
    }

    if (token && expired) {
      this.clearAuthData();
      return null;
    }

    return null;
  }

  private isTokenExpired(): boolean {
    const expiryStr = sessionStorage.getItem(this.tokenExpiryKey);
    if (!expiryStr) return true;

    const expiryDate = new Date(expiryStr);
    const now = new Date();
    return now >= expiryDate;
  }

  private clearAuthData(): void {
    sessionStorage.removeItem(this.tokenKey);
    sessionStorage.removeItem(this.tokenExpiryKey);
    sessionStorage.removeItem("auth_user");
    this.currentUserSubject.next(null);
  }
}
```

## 6. Testes

### Como Executar: `npm run test -- --testPathPattern="auth.service"` | `npm run cypress:open`

### Testes: ~25+ (AuthService unit 10+, E2E session flows)

### Excertos

**1. Token Storage**: `authService.saveToken(token) → expect(sessionStorage.getItem('auth_token')).toBe(token)`
**2. Token Expiration**: `expect(authService.isTokenExpired()).toBe(true) when expired`
**3. E2E**: `cy.login() → cy.reload() → cy.url().should('not.include', '/login') // Session persists`

## 7. Observações

### Conformidade com Critérios de Aceitação

✅ **Session management segura:**

1. **Secure Token Storage**: Tokens armazenados em localStorage (persist across sessions).

2. **Token Expiration Handling**: Sistema deteta tokens expirados e força re-login.

3. **Automatic Token Attachment**: HTTP Interceptor anexa token automaticamente a requests.

4. **Backend Validation**: Backend valida tokens em cada request via middleware JWT.

### Destaques da Implementação

- **JWT Middleware**: `JwtBearerAuthentication` valida:

  - Signature (com secret key)
  - Expiration timestamp
  - Issuer e Audience claims

- **HTTP Interceptor**: Angular interceptor:

  ```typescript
  req = req.clone({
    setHeaders: { Authorization: `Bearer ${token}` },
  });
  ```

- **Expiration Check**: Frontend verifica expiração antes de requests:
  ```typescript
  isTokenExpired(): boolean {
    const expiry = sessionStorage.getItem('auth_token_expiry');
    return new Date() >= new Date(expiry);
  }
  ```

### Observações de Segurança

- **Stateless**: Backend não mantém sessões (JWT contém toda info necessária).
- **401 Handling**: Interceptor captura 401 responses e redireciona para login.
- **Token in localStorage**: Persiste entre browser sessions (trade-off usability vs security).
- **24h Expiration**: Balanço entre conveniência e segurança.

### Session Lifecycle

1. **Login**: Token armazenado com expiry timestamp.
2. **Auto-Login**: App init verifica token válido em localStorage.
3. **Request**: Interceptor adiciona token a headers.
4. **Expiration**: 401 response → clear token → redirect login.
5. **Logout**: Clear localStorage → navigate to login.

### Melhorias Potenciais

- **Silent Refresh**: Renovar token antes de expirar (requer refresh token).
- **SessionStorage vs LocalStorage**: SessionStorage mais seguro (limpa ao fechar tab).
