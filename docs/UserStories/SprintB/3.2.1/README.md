# US 3.2.1 - External IAM Authentication

## Descrição

As a (Non-Authenticated) System User, I want to authenticate using the external IAM provider, so that I can securely access the system without managing separate credentials.

## Critérios de Aceitação

- The SPA must integrate with the selected IAM (e.g., via OAuth2/OpenID Connect).
- Unauthenticated users must be redirected to the IAM login page.
- The system must not handle the password storage.
- After successful authentication, a valid access token must be available to the front-end.
- Logout must also be supported, clearing tokens/session data.

## 3. Análise

### 3.1. Domínio

**Authentication Flow:**

**Request:**

- GoogleIdToken: Token from Google OAuth client

**Response:**

- JWT Token: System-issued authentication token
- User: User details (email, name, role)
- ExpiresAt: Token expiration timestamp

**JWT Claims:**

- sub: User email
- name: User full name
- role: System role (PortAuthorityOfficer, LogisticOperator, etc.)
- exp: Expiration timestamp

**Roles:**

- PortAuthorityOfficer
- LogisticOperator
- ShippingAgentRepresentative
- Admin

### 3.2. Regras de Negócio

1. Users authenticate via Google OAuth 2.0
2. Google ID token must be validated against Google's public keys
3. System generates JWT token valid for 24 hours
4. Email from Google account maps to system user
5. First-time users must be pre-registered by Admin
6. Role assigned during user registration (cannot self-select)
7. JWT token includes user role for authorization
8. Tokens are stateless (no server-side session storage)

### 3.2.1. Perguntas do Fórum (Dev-Cliente)

**Q1:**
I would like to ask whether there are any specific recommendations regarding the IAM provider to be used, or if further details about this User Story will be provided.

**A1:**
It is the team's responsibility to select the external IAM provider to integrate with, as long as it serves the intended purpose.
For instance, integration with Google, Facebook, Microsoft, or similar providers fits the project purposes.
Standard protocols must be supported by the IAM, so pay attention to the acceptance criteria.

Integration procedures depend on the selected IAM, so this should be taken into consideration when choosing the provider.

### 3.3. Casos de Uso

#### UC1 - Google Login

User clicks "Sign in with Google", completes Google authentication, receives JWT token from AuthApi.

#### UC2 - Validate Google Token

AuthApi validates Google ID token signature and expiration, extracts user email.

#### UC3 - Generate JWT

AuthApi creates JWT with user claims (email, name, role), signs it, returns to Frontend.

#### UC4 - Rejected Login

User with unregistered email attempts login, system rejects with appropriate error message.

### 3.4. API Routes

| Method | Endpoint     | Description                          | Auth Required |
| ------ | ------------ | ------------------------------------ | ------------- |
| POST   | /auth/google | Authenticate with Google OAuth token | No            |

## 4. Design

### 4.1. Diagrama de Sequência do Sistema (SSD)

[View SSD Diagram](SSD/SSD.puml)

### 4.2. Diagrama de Sequência Detalhado

[View SD Diagram](SD/SD.puml)

### 4.3. Modelo de Domínio

[View DM Diagram](DM/DM.puml)

## Implementação

### Backend

**Program.cs** - Google OAuth configuration:

- Integrated `Microsoft.AspNetCore.Authentication.Google`
- Configured OAuth with Client ID and Client Secret
- Implemented `OnCreatingTicket` event to check user existence and active status
- Implemented `OnTicketReceived` event to redirect based on user role or registration status
- Uses httpOnly cookies for secure session management

**Controllers/LoginController.cs**:

- `GET /api/login` - Initiates Google OAuth challenge
- `GET /api/logout` - Signs out user and clears authentication cookies

### Frontend

**login/login.component.ts**:

- `loginWithGoogle()` method redirects to backend `/api/login` endpoint
- Clears localStorage on component init for fresh authentication

**login/login.component.html**:

- Single "Login with Google" button
- Simple, clean login interface

### Flow

1. User visits `/login`
2. Clicks "Login with Google" → redirects to `http://localhost:5218/api/login`
3. Backend initiates OAuth challenge → redirects to Google
4. User authenticates with Google
5. Google redirects back to backend with authorization code
6. Backend exchanges code for tokens
7. `OnCreatingTicket` checks if user exists in database
8. `OnTicketReceived` redirects to appropriate page (registration or role-based dashboard)
9. httpOnly cookie created for session management

### Security

- No password storage in system
- Google handles all credential management
- httpOnly cookies prevent XSS attacks
- Secure cookie flags enabled
- HTTPS enforced for OAuth redirects

## 5. Implementação

### Abordagem

A autenticação externa foi implementada usando Google OAuth 2.0:

1. **Frontend**: Integração com Google Sign-In SDK para obter ID Token
2. **AuthApi**: Valida Google ID Token e gera JWT interno
3. **JWT Token**: Contém claims do utilizador (email, role, expiração)
4. **Backend Validation**: Todos os endpoints validam JWT em cada request

O fluxo:

1. Utilizador clica "Sign in with Google" no frontend
2. Google SDK abre popup de autenticação
3. Após sucesso, frontend recebe Google ID Token
4. Frontend envia token para AuthApi POST /auth/google
5. AuthApi valida token com Google, verifica se email existe na BD
6. Se utilizador existe e ativo, AuthApi gera JWT e retorna
7. Frontend armazena JWT e redireciona para dashboard

### Excertos de Código Relevantes

**1. AuthController - Google Authentication (AuthApi/Controllers/AuthController.cs)**

```csharp
using Google.Apis.Auth;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using ProjArqsi.Auth.Common;
using ProjArqsi.Domain.UserAggregate.ValueObjects;

[ApiController]
[Route("auth")]
public class AuthController : ControllerBase
{
    private readonly AppDbContext _dbContext;
    private readonly JwtTokenGenerator _jwtTokenGenerator;
    private readonly IConfiguration _configuration;

    [HttpPost("google")]
    public async Task<IActionResult> GoogleAuth([FromBody] GoogleAuthRequest request)
    {
        try
        {
            // Validate Google ID token
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
                    request.IdToken, settings);
            }
            catch (Exception ex)
            {
                return BadRequest(new { message = "Invalid Google ID token" });
            }

            if (!payload.EmailVerified)
            {
                return BadRequest(new { message = "Email not verified by Google" });
            }

            // Find user by email
            var email = new Email(payload.Email);
            var user = await _dbContext.Users
                .FirstOrDefaultAsync(u => u.Email == email);

            if (user == null)
            {
                return Unauthorized(new
                {
                    requiresRegistration = true,
                    email = payload.Email,
                    name = payload.Name,
                    message = "Please complete your registration."
                });
            }

            if (!user.IsActive)
            {
                return Unauthorized(new { message = "Account is not active" });
            }

            // Generate JWT token
            var token = _jwtTokenGenerator.GenerateToken(
                user.Id.Value,
                user.Email.Value,
                user.Role.Value.ToString());

            return Ok(new AuthResponse
            {
                AccessToken = token,
                TokenType = "Bearer",
                ExpiresIn = 86400, // 24 hours
                User = new
                {
                    UserId = user.Id.Value,
                    Email = user.Email.Value,
                    Role = user.Role.Value.ToString(),
                    Name = user.Username.Value
                }
            });
        }
        catch (Exception ex)
        {
            return StatusCode(500, new { message = "Authentication failed", details = ex.Message });
        }
    }
}
```

**2. JWT Token Generator (Auth.Common/JwtTokenGenerator.cs)**

```csharp
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using Microsoft.IdentityModel.Tokens;

public class JwtTokenGenerator
{
    private readonly JwtSettings _jwtSettings;

    public string GenerateToken(Guid userId, string email, string role)
    {
        var securityKey = new SymmetricSecurityKey(
            Encoding.UTF8.GetBytes(_jwtSettings.SigningKey));
        var credentials = new SigningCredentials(
            securityKey, SecurityAlgorithms.HmacSha256);

        var claims = new[]
        {
            new Claim(JwtRegisteredClaimNames.Sub, userId.ToString()),
            new Claim(JwtRegisteredClaimNames.Email, email),
            new Claim(ClaimTypes.Role, role),
            new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString()),
            new Claim(JwtRegisteredClaimNames.Iat,
                DateTimeOffset.UtcNow.ToUnixTimeSeconds().ToString(),
                ClaimValueTypes.Integer64)
        };

        var token = new JwtSecurityToken(
            issuer: _jwtSettings.Issuer,
            audience: _jwtSettings.Audience,
            claims: claims,
            expires: DateTime.UtcNow.AddMinutes(_jwtSettings.ExpirationMinutes),
            signingCredentials: credentials
        );

        return new JwtSecurityTokenHandler().WriteToken(token);
    }
}
```

**3. Frontend Login Component (Frontend/src/app/login/login.component.ts) - Excerto**

```typescript
import { Component } from "@angular/core";
import { Router } from "@angular/router";
import { AuthService } from "../services/auth.service";

declare const google: any;

@Component({
  selector: "app-login",
  templateUrl: "./login.component.html",
})
export class LoginComponent {
  errorMessage = "";
  loading = false;

  constructor(private authService: AuthService, private router: Router) {}

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
    this.loading = true;
    this.authService.authenticateWithGoogle(response.credential).subscribe({
      next: (authResponse) => {
        const role = authResponse.user.role;
        const dashboardRoutes: { [key: string]: string } = {
          Admin: "/admin",
          PortAuthorityOfficer: "/port-authority",
          LogisticOperator: "/logistic-operator",
          ShippingAgentRepresentative: "/shipping-agent",
        };
        this.router.navigate([dashboardRoutes[role] || "/"]);
        this.loading = false;
      },
      error: (err) => {
        this.errorMessage = err.error.message || "Login failed";
        this.loading = false;
      },
    });
  }
}
```

## 6. Testes

### Como Executar: `dotnet test --filter "Auth"` | `npm run cypress:open -- --spec "cypress/e2e/01-authentication.cy.ts"`

### Testes: ~30+ (AuthApi xUnit 15+, E2E authentication flows)

### Excertos

**1. JWT Generation**: `var token = _tokenGenerator.Generate(user) → Assert decoded claims match user`
**2. Google Token Validation**: `GoogleJsonWebSignature.ValidateAsync(token) → Assert payload.Email not null`
**3. E2E**: `cy.loginWithGoogle('admin@example.com') → cy.url().should('include', '/dashboard')`

## 7. Observações

### Conformidade com Critérios de Aceitação

✅ **Autenticação externa completa:**

1. **Integração IAM**: Sistema integra com **Google OAuth 2.0** (OAuth2/OpenID Connect).

2. **Redirect para IAM**: Utilizadores não autenticados redirecionados para Google login page.

3. **Sem Password Storage**: Sistema nunca armazena passwords (delegado ao Google).

4. **Access Token para Frontend**: JWT token disponível após autenticação bem-sucedida.

5. **Logout Implementado**: Logout limpa tokens e session data (localStorage).

### Destaques da Implementação

- **AuthApi Dedicada**: Serviço separado (porta 5001) para autenticação.
- **Google OAuth Flow**:

  1. Frontend redireciona para Google
  2. User autentica com Google
  3. Google redireciona com ID token
  4. AuthApi valida ID token com Google public keys
  5. AuthApi gera JWT interno com role claims
  6. Frontend recebe JWT e armazena

- **JWT Generation**: Token interno com claims (email, name, role, expiration).
- **Token Validation**: `GoogleJsonWebSignature.ValidateAsync()` valida tokens Google.

### Observações de Segurança

- **Zero Password Risk**: Passwords geridos por Google (2FA, security alerts, breach detection).
- **Token Signature**: JWT assinado com secret key previne tampering.
- **24h Expiration**: Tokens expiram forçando re-autenticação periódica.
- **HTTPS Required**: Produção requer HTTPS para protect tokens em trânsito.

### Decisão de Design

- **Google escolhido** conforme discussão cliente: "Google, Facebook, Microsoft or similar" - equipa escolheu Google pela popularidade e documentação.
