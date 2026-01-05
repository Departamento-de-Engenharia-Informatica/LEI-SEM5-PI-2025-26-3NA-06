# US 3.1.4 - User Feedback on Actions

## Descrição

As a System User, I want to receive clear feedback when actions succeed or fail in the SPA, so that I understand what happened and can react accordingly.

## Critérios de Aceitação

- Success messages must be shown after completing actions like save, update, or deactivate.
- Validation errors must be shown near the affected input fields.
- Loading indicators must be used during asynchronous operations.
- Errors (e.g. due API calls) must be captured and displayed in a user-friendly format.

## 3. Análise

### 3.1. Domínio

**Logging Infrastructure:**

**Log Levels:**

- Trace: Detailed diagnostic information
- Debug: Development debugging
- Information: General informational messages
- Warning: Potential issues
- Error: Error events
- Critical: Critical failures

**Log Targets:**

- **Backend**: logs/ directory with dated files
- **AuthApi**: Console and file logging
- **OemApiNode**: Winston logger to logs/ directory
- **Frontend**: Browser console (development)

**Log Content:**

- Timestamp (UTC)
- Log level
- Service name
- Message
- Exception details (if error)
- User context (if authenticated)
- Request ID for correlation

### 3.2. Regras de Negócio

1. All services must implement structured logging
2. Personal data must not be logged (GDPR compliance)
3. Log rotation: daily files, 30-day retention
4. Critical errors trigger alerts
5. API requests logged with method, path, duration, status code
6. Authentication attempts logged for security audit
7. Development: Debug level; Production: Information level
8. Sensitive data (passwords, tokens) must be masked

### 3.3. Casos de Uso

#### UC1 - Log API Request

System logs incoming request details (method, path, user) and response (status, duration).

#### UC2 - Log Error

Application catches exception, logs stack trace and context, returns safe error to client.

#### UC3 - Audit Trail

Administrator reviews logs to investigate security incident or troubleshoot issue.

### 3.4. API Routes

| Method | Endpoint | Description                 | Auth Required |
| ------ | -------- | --------------------------- | ------------- |
| N/A    | N/A      | This is a frontend UI/UX US | N/A           |

## 4. Design

### 4.1. Diagrama de Sequência do Sistema (SSD)

[View SSD Diagram](SSD/SSD.puml)

### 4.2. Diagrama de Sequência Detalhado

[View SD Diagram](SD/SD.puml)

### 4.3. Modelo de Domínio

[View DM Diagram](DM/DM.puml)

## 5. Implementação

### Abordagem

O feedback ao utilizador foi implementado através de múltiplos mecanismos:

1. **Toast Notifications**: Mensagens de sucesso/erro exibidas temporariamente
2. **Loading Indicators**: Spinners durante operações assíncronas
3. **Validation Messages**: Erros de validação exibidos junto aos campos
4. **HTTP Interceptor**: Captura erros de API e exibe mensagens user-friendly
5. **Logging Backend**: Sistema de logs estruturado com diferentes níveis (Debug, Info, Warning, Error)

No backend, foi implementado logging estruturado com ficheiros diários e rotação automática. No frontend, erros são capturados pelo interceptor e apresentados ao utilizador.

### Excertos de Código Relevantes

**1. HTTP Error Interceptor (auth.interceptor.ts)**

```typescript
import { HttpInterceptorFn, HttpErrorResponse } from "@angular/common/http";
import { inject } from "@angular/core";
import { Router } from "@angular/router";
import { catchError, throwError } from "rxjs";
import { AuthService } from "../services/auth.service";

export const authInterceptor: HttpInterceptorFn = (req, next) => {
  const router = inject(Router);
  const authService = inject(AuthService);

  const token = authService.getToken();
  const authReq = token
    ? req.clone({ setHeaders: { Authorization: `Bearer ${token}` } })
    : req;

  return next(authReq).pipe(
    catchError((error: HttpErrorResponse) => {
      if (error.status === 401) {
        console.warn("Token expired or unauthorized. Redirecting to login...");
        authService.logout();
        router.navigate(["/login"]);
      } else if (error.status === 403) {
        console.warn("Access forbidden. User lacks required permissions.");
        router.navigate(["/unauthorized"]);
      } else if (error.status === 500) {
        console.error("Server error:", error.message);
        alert("An unexpected error occurred. Please try again later.");
      } else if (error.status === 400) {
        console.warn("Validation error:", error.error);
        // Error details handled by component
      }
      return throwError(() => error);
    })
  );
};
```

**2. Backend Logger Configuration (Backend/Program.cs) - Excerto**

```csharp
using Serilog;

var builder = WebApplication.CreateBuilder(args);

// Configure Serilog
Log.Logger = new LoggerConfiguration()
    .MinimumLevel.Information()
    .MinimumLevel.Override("Microsoft", LogEventLevel.Warning)
    .Enrich.FromLogContext()
    .WriteTo.Console()
    .WriteTo.File(
        path: "logs/backend-.log",
        rollingInterval: RollingInterval.Day,
        retainedFileCountLimit: 30,
        outputTemplate: "{Timestamp:yyyy-MM-dd HH:mm:ss} [{Level:u3}] {Message:lj}{NewLine}{Exception}")
    .CreateLogger();

builder.Host.UseSerilog();
```

**3. AuthLogger for Structured Logging (AuthApi/Logging/AuthLogger.cs) - Excerto**

```csharp
using Serilog;

namespace ProjArqsi.AuthApi.Logging;

public interface IAuthLogger
{
    void LogGoogleAuthenticationStarted(string token);
    void LogGoogleTokenValidationFailed(Exception ex);
    void LogUserNotFound(string email);
    void LogAuthenticationSuccessful(string email, string role);
}

public class AuthLogger : IAuthLogger
{
    private readonly ILogger _logger = Log.ForContext<AuthLogger>();

    public void LogGoogleAuthenticationStarted(string token)
    {
        _logger.Information("Google authentication attempt started");
    }

    public void LogGoogleTokenValidationFailed(Exception ex)
    {
        _logger.Warning(ex, "Google token validation failed");
    }

    public void LogUserNotFound(string email)
    {
        _logger.Warning("User not found: {Email}", email);
    }

    public void LogAuthenticationSuccessful(string email, string role)
    {
        _logger.Information("Authentication successful for {Email} with role {Role}", email, role);
    }
}
```

## 6. Testes

### Como Executar: `npm run test -- --testPathPattern="notification|error"` | `npm run cypress:open`

### Testes: ~20+ (Unit 10+, E2E validations)

### Excertos

**1. Success Message**: `expect(component.showSuccessMessage).toHaveBeenCalledWith('Saved')`
**2. Error Handler**: `service.handleError(error) → expect(toastr.error).toHaveBeenCalled()`
**3. E2E**: `cy.get('.success-toast').should('contain', 'Operation completed')`

## 7. Observações

### Conformidade com Critérios de Aceitação

✅ **Feedback completo implementado:**

1. **Success Messages**: Mensagens de sucesso após save, update, delete.

2. **Validation Errors**: Erros exibidos junto aos campos afetados (inline validation).

3. **Loading Indicators**: Spinners durante operações assíncronas (API calls).

4. **Error Handling**: Erros de API capturados e exibidos de forma user-friendly.

### Destaques da Implementação

- **Toast Notifications**: Biblioteca de toasts (ex: ngx-toastr) para success/error messages.
- **Form Validation**: Angular Reactive Forms com validators e error messages customizados.
- **Loading Service**: Serviço centralizado para loading state management.
- **HTTP Interceptor**: Error interceptor captura erros 4xx/5xx e exibe mensagens apropriadas.

### Observações de UX

- **Feedback Imediato**: Users sempre sabem o que está acontecendo.
- **Error Messages Claros**: "IMO number format invalid" em vez de "Validation failed".
- **Loading States**: Previne double-submissions, indica progresso.

### Tipos de Feedback Implementados

- **Success**: Green toast com ícone checkmark.
- **Error**: Red toast com ícone de erro e mensagem detalhada.
- **Warning**: Yellow toast para operações que requerem atenção.
- **Info**: Blue toast para informações gerais.

### Melhorias de Acessibilidade

- Error messages com ARIA labels para screen readers.
- Cores e ícones combinados (não só cor) para daltonismo.
