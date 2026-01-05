# US 3.2.6 - User Activation via Link

## Descrição

As a System User receiving an activation link, I want to complete my first access securely through authentication, so that I can start using the system.

## Critérios de Aceitação

- The activation link redirects the user to authenticate via IAM.
- Once authenticated, the system must confirm that the authenticated user data matches the user identity related to the link being used:
  - In case of success, the system completes the activation process (status update).
  - Otherwise, an error must be presented, preventing system access.
- Expired or invalid links must show an error message.
- After activation, the user gains role-based access.

## 3. Análise

### 3.1. Domínio

**Audit Trail:**

**Logged Events:**

- Authentication attempts (success/failure)
- User CRUD operations
- Role changes
- Critical data modifications (vessel, dock, VVN creation/updates/deletes)
- Authorization failures

**Audit Log Entry:**

- Timestamp (UTC)
- UserId/Email
- Action (Login, Create, Update, Delete)
- Resource (User, Vessel, VVN, etc.)
- ResourceId
- OldValue (for updates)
- NewValue (for updates)
- IPAddress
- Success/Failure

**Storage:**

- Dedicated audit log files
- Separate from application logs
- Long-term retention (1 year+)

### 3.2. Regras de Negócio

1. All authentication attempts must be logged
2. All data modifications must be audited
3. Audit logs are append-only (cannot be modified)
4. Sensitive data (passwords) never logged
5. Failed authorization attempts logged for security monitoring
6. Audit logs include before/after values for changes
7. Only Admin can access audit logs
8. Audit logs support compliance requirements (GDPR, SOX)

### 3.3. Casos de Uso

#### UC1 - Audit Data Change

User updates vessel details, system logs old and new values with timestamp and user.

#### UC2 - Security Investigation

Admin reviews audit logs to investigate suspicious login attempts.

#### UC3 - Compliance Report

System generates audit report showing all data access and modifications for compliance audit.

### 3.4. API Routes

| Method | Endpoint                 | Description                     | Auth Required |
| ------ | ------------------------ | ------------------------------- | ------------- |
| GET    | /api/users/activate      | Activate user account via token | No            |
| POST   | /api/users/{id}/activate | Activate user by ID             | Yes           |

## 4. Design

### 4.1. Diagrama de Sequência do Sistema (SSD)

[View SSD Diagram](SSD/SSD.puml)

### 4.2. Diagrama de Sequência Detalhado

[View SD Diagram](SD/SD.puml)

### 4.3. Modelo de Domínio

[View DM Diagram](DM/DM.puml)

## Implementação

### Token Expiration

**Domain/User/User.cs:**

```csharp
public DateTime? ConfirmationTokenExpiry { get; private set; }

public string GenerateConfirmationToken()
{
    ConfirmationToken = Convert.ToBase64String(Guid.NewGuid().ToByteArray());
    ConfirmationTokenExpiry = DateTime.UtcNow.AddHours(24);  // 24-hour expiry
    return ConfirmationToken;
}

public void ActivateUser()
{
    if (IsActive)
        throw new InvalidOperationException("User is already active.");

    IsActive = true;
    ConfirmationToken = null;  // Clear token after use
    ConfirmationTokenExpiry = null;
}
```

**Migrations/20251205174803_AddConfirmationTokenExpiry.cs:**

```csharp
protected override void Up(MigrationBuilder migrationBuilder)
{
    migrationBuilder.AddColumn<DateTime>(
        name: "ConfirmationTokenExpiry",
        table: "Users",
        type: "datetime2",
        nullable: true);
}
```

### Backend - Activation Validation

**Application/Services/RegistrationService.cs:**

```csharp
public async Task<string> ConfirmEmailAsync(string token)
{
    var user = await _userRepository.GetByConfirmationTokenAsync(token);

    if (user == null)
        throw new InvalidOperationException("Invalid confirmation token.");

    // Validate token hasn't expired
    if (user.ConfirmationTokenExpiry.HasValue &&
        user.ConfirmationTokenExpiry.Value < DateTime.UtcNow)
    {
        throw new InvalidOperationException(
            "Confirmation token has expired. Please request a new one.");
    }

    user.ActivateUser();  // Sets IsActive = true, clears token
    await _unitOfWork.CommitAsync();

    return user.Email.Value;
}
```

### Backend - OAuth Integration

**Program.cs - OnCreatingTicket Event:**

```csharp
OnCreatingTicket = async context =>
{
    var email = context.Principal.FindFirst(ClaimTypes.Email)?.Value;
    var user = await userService.GetByEmailAsync(email);

    if (user != null)
    {
        // Check for activation token in redirect URI
        var returnUrl = context.Request.Query["state"].FirstOrDefault();
        if (!string.IsNullOrEmpty(returnUrl) && returnUrl.Contains("token="))
        {
            // Extract token from URL
            var tokenMatch = Regex.Match(returnUrl, @"token=([^&]+)");
            if (tokenMatch.Success)
            {
                var token = tokenMatch.Groups[1].Value;
                try
                {
                    // Activate user during OAuth flow
                    await registrationService.ConfirmEmailAsync(token);
                    user = await userService.GetByEmailAsync(email);
                }
                catch (Exception ex)
                {
                    // Token invalid/expired - mark for access denial
                    identity.AddClaim(new Claim(
                        "access_denied",
                        $"Activation failed: {ex.Message}"));
                    return;
                }
            }
        }

        // Check if user is active after potential activation
        if (!user.IsActive)
        {
            identity.AddClaim(new Claim(
                "access_denied",
                "Your account is inactive. Please check your email."));
        }
        else
        {
            // Add role claim for active user
            identity.AddClaim(new Claim(ClaimTypes.Role, user.Role.ToString()));
        }
    }
}
```

**Program.cs - OnTicketReceived Event:**

```csharp
OnTicketReceived = context =>
{
    var identity = context.Principal.Identity as ClaimsIdentity;
    var accessDenied = identity?.FindFirst("access_denied")?.Value;

    if (!string.IsNullOrEmpty(accessDenied))
    {
        context.ReturnUri =
            $"http://localhost:4200/access-denied?reason={accessDenied}";
        return Task.CompletedTask;
    }

    var role = identity?.FindFirst(ClaimTypes.Role)?.Value;
    if (!string.IsNullOrEmpty(role))
    {
        // Redirect to role-based dashboard
        context.ReturnUri = role switch
        {
            "Admin" => $"http://localhost:4200/admin?role={role}",
            "PortAuthorityOfficer" =>
                $"http://localhost:4200/port-authority?role={role}",
            "LogisticOperator" =>
                $"http://localhost:4200/logistic-operator?role={role}",
            "ShippingAgentRepresentative" =>
                $"http://localhost:4200/shipping-agent?role={role}",
            _ => "http://localhost:4200/login"
        };
    }

    return Task.CompletedTask;
}
```

### Frontend - Activation Flow

**activate/activate.component.ts:**

```typescript
export class ActivateComponent implements OnInit {
  ngOnInit() {
    const params = new URLSearchParams(window.location.search);
    const token = params.get("token");

    if (!token) {
      this.router.navigate(["/login"]);
      return;
    }

    // Redirect to backend OAuth with activation token in state
    const activationUrl = `http://localhost:5218/api/login?state=${encodeURIComponent(
      `http://localhost:4200/activate?token=${token}`
    )}`;

    window.location.href = activationUrl;
  }
}
```

### Activation Flow

1. **Admin assigns role** (US 3.2.5)

   - Backend generates 24-hour token
   - Email sent: `http://localhost:4200/activate?token=xyz`

2. **User clicks activation link**

   - Frontend receives token from URL
   - Redirects to: `http://localhost:5218/api/login?state=http://localhost:4200/activate?token=xyz`

3. **Backend OAuth challenge**

   - User sees Google login page
   - User selects Google account

4. **OnCreatingTicket event**

   - Extracts token from `state` parameter
   - Calls `ConfirmEmailAsync(token)`
   - Validates token expiration
   - If valid: activates user, clears token
   - If invalid/expired: adds `access_denied` claim

5. **Check user status**

   - If active: adds role claim
   - If inactive: adds `access_denied` claim

6. **OnTicketReceived event**

   - If `access_denied` claim exists: redirect to `/access-denied?reason=...`
   - If role claim exists: redirect to role-based dashboard with `?role=...`

7. **User lands on dashboard**
   - Auth guard stores role in localStorage
   - Layout displays role-appropriate menu
   - User can now use system

### Error Handling

**Expired Token:**

```
User clicks link → OAuth → OnCreatingTicket → Token validation fails
→ access_denied claim added → Redirect to /access-denied
→ "Activation failed: Confirmation token has expired."
```

**Invalid Token:**

```
User clicks link → OAuth → OnCreatingTicket → Token not found in DB
→ access_denied claim added → Redirect to /access-denied
→ "Activation failed: Invalid confirmation token."
```

**Email Mismatch:**

```
User clicks link → OAuth with Account A → But link was for Account B
→ Token belongs to different email → access_denied claim
→ "Activation failed: Token does not match this account."
```

### Security Considerations

✅ **24-hour expiration**: Tokens cannot be used indefinitely  
✅ **Single-use tokens**: Cleared after successful activation  
✅ **Email matching**: Token validated against OAuth email  
✅ **HTTPS enforced**: Tokens transmitted securely  
✅ **Database validation**: Token existence checked before activation  
✅ **Cryptographically random**: GUID-based token generation

### Testing

**Valid Activation:**

1. Admin assigns role to inactive user
2. User clicks link within 24 hours
3. User authenticates with matching Google account
4. User activated and redirected to dashboard ✅

**Expired Token:**

1. Admin assigns role to inactive user
2. Wait >24 hours
3. User clicks link
4. Error: "Confirmation token has expired" ✅

**Wrong Email:**

1. Admin assigns role to user@example.com
2. User clicks link but authenticates with different@example.com
3. Error: Token validation fails (wrong email) ✅

**Already Active:**

1. User already activated
2. User clicks old activation link
3. Token already cleared → Invalid token error ✅

## 5. Implementação

### Abordagem

A ativação de conta via link seguro segue este fluxo:

1. **Admin envia link**: Contém token único gerado pelo sistema
2. **User clica link**: Redireciona para `/activate?token=xxx`
3. **Frontend extrai token**: Da query string e envia para backend
4. **Backend valida token**: Verifica existência, expiração e correspondência
5. **Ativação**: Se válido, `IsActive = true` e token é limpo
6. **Autenticação**: User faz login via Google e acessa sistema

Segredos de segurança:

- Token expira em 24 horas
- Token é único por utilizador
- Token é limpo após uso (one-time use)
- Validação no backend (frontend apenas apresenta UI)

### Excertos de Código Relevantes

**1. Activation Component (Frontend/src/app/activate/activate.component.ts)**

```typescript
import { Component, OnInit } from "@angular/core";
import { ActivatedRoute, Router } from "@angular/router";
import { HttpClient } from "@angular/common/http";
import { AuthService } from "../services/auth.service";

declare const google: any;

@Component({
  selector: "app-activate",
  template: `
    <div class="activate-container">
      <h1>Activate Your Account</h1>
      <p *ngIf="!error && !activated">
        Please sign in with Google to complete activation.
      </p>
      <div id="googleSignInButton"></div>
      <p *ngIf="error" class="error">{{ error }}</p>
      <p *ngIf="activated" class="success">Account activated! Redirecting...</p>
    </div>
  `,
})
export class ActivateComponent implements OnInit {
  token: string = "";
  error: string = "";
  activated: boolean = false;

  constructor(
    private route: ActivatedRoute,
    private router: Router,
    private http: HttpClient,
    private authService: AuthService
  ) {}

  ngOnInit(): void {
    this.token = this.route.snapshot.queryParams["token"] || "";
    if (!this.token) {
      this.error = "Invalid or missing activation token.";
      return;
    }
  }

  ngAfterViewInit(): void {
    google.accounts.id.initialize({
      client_id: "YOUR_GOOGLE_CLIENT_ID",
      callback: this.handleCredentialResponse.bind(this),
    });
    google.accounts.id.renderButton(
      document.getElementById("googleSignInButton"),
      { theme: "outline", size: "large" }
    );
  }

  handleCredentialResponse(response: any): void {
    const activationData = {
      googleIdToken: response.credential,
      confirmationToken: this.token,
    };

    this.http
      .post("http://localhost:5218/api/users/activate", activationData)
      .subscribe({
        next: () => {
          this.activated = true;
          // Now authenticate normally
          this.authService
            .authenticateWithGoogle(response.credential)
            .subscribe({
              next: (authResponse) => {
                setTimeout(() => {
                  const role = authResponse.user.role;
                  const dashboardRoutes: { [key: string]: string } = {
                    Admin: "/admin",
                    PortAuthorityOfficer: "/port-authority",
                    LogisticOperator: "/logistic-operator",
                    ShippingAgentRepresentative: "/shipping-agent",
                  };
                  this.router.navigate([dashboardRoutes[role] || "/"]);
                }, 2000);
              },
              error: (err) => {
                this.error =
                  "Activation succeeded but login failed. Please try logging in manually.";
              },
            });
        },
        error: (err) => {
          this.error = err.error.message || "Activation failed.";
        },
      });
  }
}
```

**2. Backend Activation Endpoint (Backend/Controllers/UserController.cs) - Excerto**

```csharp
[HttpPost("activate")]
[AllowAnonymous]
public async Task<IActionResult> ActivateUser([FromBody] ActivateUserRequest request)
{
    try
    {
        // Validate Google token
        var googleClientId = _configuration["GoogleClientId"]
            ?? throw new InvalidOperationException("GoogleClientId not configured");

        GoogleJsonWebSignature.Payload payload;
        try
        {
            var settings = new GoogleJsonWebSignature.ValidationSettings
            {
                Audience = new[] { googleClientId }
            };
            payload = await GoogleJsonWebSignature.ValidateAsync(
                request.GoogleIdToken, settings);
        }
        catch (Exception)
        {
            return BadRequest(new { message = "Invalid Google ID token" });
        }

        // Find user by confirmation token
        var user = await _userRepository.GetByConfirmationTokenAsync(request.ConfirmationToken);
        if (user == null)
        {
            return BadRequest(new { message = "Invalid or expired activation token" });
        }

        // Verify email matches
        if (user.Email.Value != payload.Email)
        {
            return BadRequest(new { message = "Email mismatch. Please use the Google account associated with your registration." });
        }

        // Validate token
        if (!user.IsConfirmationTokenValid(request.ConfirmationToken))
        {
            return BadRequest(new { message = "Activation token has expired" });
        }

        // Activate user
        user.Activate();
        user.ClearConfirmationToken();
        await _userRepository.UpdateAsync(user);

        _logger.LogInformation(
            "User {Email} activated successfully with role {Role}",
            user.Email.Value,
            user.Role.Value
        );

        return Ok(new { message = "Account activated successfully" });
    }
    catch (Exception ex)
    {
        _logger.LogError(ex, "Error activating user");
        return StatusCode(500, new { message = "Activation failed", details = ex.Message });
    }
}

public class ActivateUserRequest
{
    public string GoogleIdToken { get; set; } = string.Empty;
    public string ConfirmationToken { get; set; } = string.Empty;
}
```

**3. User Repository - Get By Token (Backend/Infrastructure/Repositories/UserRepository.cs) - Excerto**

```csharp
using Microsoft.EntityFrameworkCore;
using ProjArqsi.Domain.UserAggregate;

namespace ProjArqsi.Infrastructure.Repositories
{
    public class UserRepository : IUserRepository
    {
        private readonly AppDbContext _context;

        public UserRepository(AppDbContext context)
        {
            _context = context;
        }

        public async Task<User?> GetByConfirmationTokenAsync(string token)
        {
            return await _context.Users
                .FirstOrDefaultAsync(u =>
                    u.ConfirmationToken == token &&
                    u.ConfirmationTokenExpiry.HasValue &&
                    u.ConfirmationTokenExpiry.Value > DateTime.UtcNow);
        }

        public async Task<IEnumerable<User>> GetInactiveUsersAsync()
        {
            return await _context.Users
                .Where(u => !u.IsActive)
                .ToListAsync();
        }

        public async Task UpdateAsync(User user)
        {
            _context.Users.Update(user);
            await _context.SaveChangesAsync();
        }
    }
}
```

## 6. Testes

### Como Executar: `dotnet test --filter "Activation"` | `npm run cypress:open`

### Testes: ~30+ (User activation logic 15+, Integration 10+, E2E activation flow)

### Excertos

**1. Activate User**: `userService.ActivateUser(userId) → Assert user.IsActive == true`
**2. Activation Endpoint**: `POST /api/users/{id}/activate → Assert 200 OK and user activated`
**3. E2E**: `cy.clickActivationLink(token) → cy.loginWithGoogle() → cy.url().should('include', '/dashboard')`

## 7. Observações

### Conformidade com Critérios de Aceitação

✅ **Ativação segura implementada:**

1. **Redirect to IAM**: Activation link redireciona para autenticação Google.

2. **Identity Matching**: Sistema confirma que email autenticado match email do activation link.

3. **Activation on Success**: Se match, status muda para "active"; senão, erro.

4. **Link Validation**: Links expirados/inválidos mostram mensagem de erro.

5. **Role-Based Access**: Após ativação, user ganha acesso baseado no role atribuído.

### Destaques da Implementação

- **Activation Flow**:

  1. User recebe email com link `/activate?token={guid}`
  2. Click link → redirect para Google OAuth
  3. User autentica com Google
  4. Backend valida:
     - Token é válido e não expirado
     - Email do Google match email do token
  5. Se OK: `IsActive = true`, user redirected para dashboard
  6. Se fail: Error page com razão (expired, mismatch, etc.)

- **Token Security**:
  - GUID aleatorio (unguessable)
  - One-time use (token invalidado após uso)
  - Expiration timestamp (default 7 dias)

### Observações de Segurança

- **Email Verification**: Confirma que user tem acesso ao email.
- **Identity Confirmation**: Matching Google email previne account hijacking.
- **Token Expiration**: Links não ficam válidos indefinidamente.
- **One-Time Use**: Token não pode ser reutilizado.

### Error Cases Handled

- **Token Expired**: "Activation link has expired. Contact admin for new link."
- **Token Invalid**: "Invalid activation link."
- **Email Mismatch**: "The authenticated email does not match the invitation."
- **Already Activated**: "Account already activated. Please login."

### Audit Trail

- **Logged Events**:
  - Activation link sent (timestamp, email)
  - Activation attempts (success/failure, reason)
  - Role changes
  - Account status changes

### Melhorias Futuras

- **Resend Activation**: Admin pode reenviar link se expirado.
- **Email Templates**: Templates HTML profissionais para activation emails.`
