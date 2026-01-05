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

_A desenvolver: Identificar as entidades, agregados e value objects do domínio relacionados com esta US._

### 3.2. Regras de Negócio

_A desenvolver: Documentar as regras de negócio específicas desta funcionalidade._

### 3.3. Casos de Uso

_A desenvolver: Descrever os principais casos de uso e seus fluxos._

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
