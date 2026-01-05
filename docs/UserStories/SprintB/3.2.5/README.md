# US 3.2.5 - Assign Internal Roles

## Descrição

As an Administrator, I want to assign (or update) the internal role(s) of a given user, so that they can access only the features appropriate to their responsibilities.

## Critérios de Aceitação

- Users are identified by IAM-provided attributes (userId, email, name).
- When authorizing a user for the first time:
  - A unique activation link is sent to their email.
  - By default, the users are set to a "deactivated" status.
- Internal roles determine system access level.

## 3. Análise

### 3.1. Domínio

**Security Layers:**

**CORS (Cross-Origin Resource Sharing):**

- Allows Frontend origin (http://localhost:4200)
- Blocks unauthorized origins

**JWT Authentication Middleware:**

- Validates token signature
- Checks expiration
- Extracts user claims

**Authorization Middleware:**

- Validates user role against endpoint requirements
- Denies access if insufficient permissions

**HTTPS Enforcement:**

- Production requires HTTPS
- Redirects HTTP to HTTPS

### 3.2. Regras de Negócio

1. All APIs must configure CORS to allow Frontend
2. JWT validation happens before request reaches controller
3. Invalid tokens return 401 Unauthorized
4. Missing tokens return 401 Unauthorized
5. Insufficient permissions return 403 Forbidden
6. Security headers added to all responses (X-Content-Type-Options, X-Frame-Options)
7. Request rate limiting prevents DDoS
8. HTTPS required in production environment

### 3.3. Casos de Uso

#### UC1 - Token Validation

Middleware intercepts request, validates JWT, populates User context for controller.

#### UC2 - CORS Preflight

Browser sends OPTIONS request, CORS middleware responds with allowed origins/methods.

#### UC3 - Security Headers

Middleware adds security headers to response preventing XSS and clickjacking.

### 3.4. API Routes

| Method | Endpoint                       | Description                                   | Auth Required |
| ------ | ------------------------------ | --------------------------------------------- | ------------- |
| POST   | /api/users/{email}/assign-role | Assign role to user and send activation email | Yes           |
| PUT    | /api/users/{id}/role           | Update user role                              | Yes           |
| GET    | /api/users                     | List all users                                | Yes           |

## 4. Design

### 4.1. Diagrama de Sequência do Sistema (SSD)

[View SSD Diagram](SSD/SSD.puml)

### 4.2. Diagrama de Sequência Detalhado

[View SD Diagram](SD/SD.puml)

### 4.3. Modelo de Domínio

[View DM Diagram](DM/DM.puml)

## Implementação

### Backend

**Application/Services/UserService.cs** - Role Assignment Logic:

```csharp
public async Task<UserDto> AssignRoleAndSendActivationEmailAsync(
    string email, RoleType newRole)
{
    var user = await GetByEmailAsync(email);
    if (user == null)
        throw new InvalidOperationException($"User with email {email} not found.");

    // Change role (can be done while user is inactive)
    user.ChangeRole(newRole);

    // Generate 24-hour activation token
    var token = user.GenerateConfirmationToken();
    await _unitOfWork.CommitAsync();

    // Send activation email with token link
    var activationLink = $"http://localhost:4200/activate?token={token}";
    await _emailService.SendEmailAsync(
        user.Email.Value,
        "Activate Your Account",
        $"Click the link to activate: {activationLink}"
    );

    return _mapper.Map<UserDto>(user);
}
```

**Application/Services/UserService.cs** - User Filtering:

```csharp
public async Task<List<UserDto>> GetInactiveUsersAsync()
{
    var users = await GetAllAsync();
    var inactiveUsers = users.Where(u => !u.IsActive).ToList();
    return _mapper.Map<List<UserDto>>(inactiveUsers);
}

public async Task<List<UserDto>> GetAllUsersAsync()
{
    var users = await GetAllAsync();
    return _mapper.Map<List<UserDto>>(users);
}
```

**Controllers/UserManagementController.cs:**

```csharp
[Authorize(Roles = "Admin")]
[ApiController]
[Route("api/[controller]")]
public class UserManagementController : ControllerBase
{
    [HttpGet("inactive")]
    public async Task<ActionResult<List<UserDto>>> GetInactiveUsers()
        => Ok(await _userService.GetInactiveUsersAsync());

    [HttpGet("all")]
    public async Task<ActionResult<List<UserDto>>> GetAllUsers()
        => Ok(await _userService.GetAllUsersAsync());

    [HttpPost("assign-role")]
    public async Task<ActionResult<UserDto>> AssignRoleAndActivate(
        [FromBody] AssignRoleRequest request)
    {
        var user = await _userService.AssignRoleAndSendActivationEmailAsync(
            request.Email, request.NewRole);
        return Ok(user);
    }
}
```

### Frontend

**admin/user-management/user-management.component.ts:**

```typescript
export class UserManagementComponent implements OnInit {
  users: any[] = [];
  filteredUsers: any[] = [];
  showOnlyInactive = true;

  availableRoles = [
    "Admin",
    "PortAuthorityOfficer",
    "LogisticOperator",
    "ShippingAgentRepresentative",
  ];

  ngOnInit() {
    this.loadUsers();
  }

  loadUsers() {
    const endpoint = this.showOnlyInactive
      ? "/api/UserManagement/inactive"
      : "/api/UserManagement/all";

    this.http.get<any[]>(endpoint, { withCredentials: true }).subscribe({
      next: (data) => {
        this.users = data;
        this.filteredUsers = [...data]; // Create new array reference
      },
      error: (err) => console.error("Failed to load users", err),
    });
  }

  toggleFilter() {
    this.showOnlyInactive = !this.showOnlyInactive;
    this.loadUsers(); // Reload data from backend
  }

  assignRole(user: any) {
    if (!user.selectedRole) {
      alert("Please select a role");
      return;
    }

    this.http
      .post(
        "/api/UserManagement/assign-role",
        { email: user.email, newRole: user.selectedRole },
        { withCredentials: true }
      )
      .subscribe({
        next: () => {
          alert("Role assigned and activation email sent!");
          this.loadUsers(); // Refresh list
        },
        error: (err) => alert(`Failed to assign role: ${err.error}`),
      });
  }
}
```

**admin/user-management/user-management.component.html:**

```html
<button (click)="toggleFilter()">
  {{ showOnlyInactive ? 'Show All Users' : 'Show Only Inactive Users' }}
</button>

<table>
  <thead>
    <tr>
      <th>Email</th>
      <th>Username</th>
      <th>Role</th>
      <th>Status</th>
      <th>Actions</th>
    </tr>
  </thead>
  <tbody>
    <tr *ngFor="let user of filteredUsers">
      <td>{{ user.email }}</td>
      <td>{{ user.username }}</td>
      <td>{{ user.role }}</td>
      <td>{{ user.isActive ? 'Active' : 'Inactive' }}</td>
      <td>
        <select [(ngModel)]="user.selectedRole">
          <option *ngFor="let role of availableRoles" [value]="role">
            {{ role }}
          </option>
        </select>
        <button (click)="assignRole(user)">Assign Role & Send Email</button>
      </td>
    </tr>
  </tbody>
</table>
```

### Email Service

**Application/Services/EmailService.cs:**

```csharp
public async Task SendEmailAsync(string to, string subject, string body)
{
    using var message = new MailMessage(_smtpSettings.From, to, subject, body);
    message.IsBodyHtml = false;

    using var client = new SmtpClient(_smtpSettings.Host, _smtpSettings.Port)
    {
        Credentials = new NetworkCredential(
            _smtpSettings.Username,
            _smtpSettings.Password),
        EnableSsl = _smtpSettings.EnableSsl
    };

    await client.SendMailAsync(message);
}
```

**appsettings.json - SMTP Configuration:**

```json
{
  "Smtp": {
    "Host": "smtp.gmail.com",
    "Port": 587,
    "EnableSsl": true,
    "Username": "your-email@gmail.com",
    "Password": "app-password",
    "From": "your-email@gmail.com"
  }
}
```

### Business Rules

✅ **Inactive users can have roles assigned**: Role assignment prepares user for activation  
✅ **Role changes require new activation**: User must confirm new role via email  
✅ **Admin can filter users**: Toggle between inactive-only and all users  
✅ **Email sent on role assignment**: Contains activation link with token  
✅ **User remains inactive until confirmation**: Role assigned but not active yet

### Workflow

1. Admin opens User Management page
2. System loads inactive users by default
3. Admin selects role from dropdown
4. Admin clicks "Assign Role & Send Email"
5. Backend:
   - Changes user's role (even if inactive)
   - Generates 24-hour activation token
   - Saves to database
   - Sends email with activation link
6. User receives email with link: `http://localhost:4200/activate?token=...`
7. User remains inactive until they click link (see US 3.2.6)
8. Admin can toggle to view all users or only inactive

### Design Decisions

**Why inactive users can be assigned roles?**

- Prepares user account before activation
- Admin can batch-process pending users
- Separates administrative setup from user confirmation

**Why reload data after toggle?**

- Ensures UI shows correct filtered data
- Prevents race conditions with change detection
- New array reference triggers Angular update

**Why withCredentials: true?**

- Ensures httpOnly cookie sent with requests
- Required for authentication validation
- Applied via HTTP interceptor globally

## 5. Implementação

### Abordagem

A atribuição de roles internos foi implementada exclusivamente para o role Admin:

1. **Admin Dashboard**: Interface para listar utilizadores inativos pendentes de ativação
2. **Role Assignment**: Admin seleciona role e envia link de ativação por email
3. **Email Service**: Gera token único com validade de 24 horas e envia link
4. **User Aggregate**: Método `GenerateConfirmationToken()` cria token seguro
5. **Status Management**: Utilizadores iniciam como `IsActive = false` até ativar conta

Apenas utilizadores autenticados via Google podem ser registados, e apenas Admin pode atribuir roles.

### Excertos de Código Relevantes

**1. UserController - Assign Role Endpoint (Backend/Controllers/UserController.cs)**

```csharp
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using ProjArqsi.Services;
using ProjArqsi.DTOs.User;
using ProjArqsi.Domain.UserAggregate.ValueObjects;

namespace ProjArqsi.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize(Roles = "Admin")] // Only Admin can manage users
    public class UserController : ControllerBase
    {
        private readonly UserService _userService;

        public UserController(UserService userService)
        {
            _userService = userService;
        }

        [HttpGet("inactive-users")]
        public async Task<ActionResult<IEnumerable<UserDto>>> GetInactiveUsers()
        {
            try
            {
                var inactiveUsers = await _userService.GetInactiveUsersAsync();
                return Ok(inactiveUsers);
            }
            catch (Exception ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }

        [HttpPut("{id}/assign-role")]
        public async Task<ActionResult> AssignRoleAndActivate(Guid id, [FromBody] string role)
        {
            try
            {
                var roleType = Enum.Parse<RoleType>(role);
                await _userService.AssignRoleAndSendActivationEmailAsync(id, roleType);
                return Ok(new { message = "Role assigned and activation email sent" });
            }
            catch (InvalidOperationException ex)
            {
                return NotFound(new { message = ex.Message });
            }
            catch (Exception ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }

        [HttpPut("{id}/toggle-active")]
        public async Task<ActionResult> ToggleUserActive(Guid id)
        {
            try
            {
                var result = await _userService.ToggleUserActiveAsync(id);
                var message = result.IsActive ? "User activated" : "User deactivated";
                return Ok(new { message });
            }
            catch (InvalidOperationException ex)
            {
                return NotFound(new { message = ex.Message });
            }
            catch (Exception ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }
    }
}
```

**2. User Aggregate - Token Generation (Backend/Domain/User/User.cs) - Excerto**

```csharp
namespace ProjArqsi.Domain.UserAggregate
{
    public class User : Entity<UserId>, IAggregateRoot
    {
        public Username Username { get; private set; } = null!;
        public Role Role { get; private set; } = null!;
        public Email Email { get; private set; } = null!;
        public bool IsActive { get; private set; } = false;
        public string ConfirmationToken { get; set; } = string.Empty;
        public DateTime? ConfirmationTokenExpiry { get; set; }

        public void GenerateConfirmationToken()
        {
            ConfirmationToken = Guid.NewGuid().ToString("N"); // 32-char hex string
            ConfirmationTokenExpiry = DateTime.UtcNow.AddHours(24);
        }

        public bool IsConfirmationTokenValid(string token)
        {
            if (string.IsNullOrEmpty(ConfirmationToken) || ConfirmationToken != token)
                return false;

            if (!ConfirmationTokenExpiry.HasValue || DateTime.UtcNow > ConfirmationTokenExpiry.Value)
                return false;

            return true;
        }

        public void ClearConfirmationToken()
        {
            ConfirmationToken = string.Empty;
            ConfirmationTokenExpiry = null;
        }
    }
}
```

**3. UserService - Assign Role and Send Email (Backend/Application/Services/UserService.cs) - Excerto**

```csharp
using ProjArqsi.Domain.UserAggregate;
using ProjArqsi.Domain.UserAggregate.ValueObjects;
using ProjArqsi.Infrastructure.Repositories;

namespace ProjArqsi.Services
{
    public class UserService
    {
        private readonly IUserRepository _userRepository;
        private readonly IEmailService _emailService;
        private readonly ILogger<UserService> _logger;

        public async Task AssignRoleAndSendActivationEmailAsync(Guid userId, RoleType roleType)
        {
            var user = await _userRepository.GetByIdAsync(new UserId(userId))
                ?? throw new InvalidOperationException("User not found");

            // Assign role
            user.ChangeRole(new Role(roleType));

            // Generate activation token
            user.GenerateConfirmationToken();

            await _userRepository.UpdateAsync(user);

            // Send activation email
            var activationLink = $"http://localhost:4200/activate?token={user.ConfirmationToken}";
            var emailBody = $@"
                <h2>Account Activation</h2>
                <p>Hello {user.Username.Value},</p>
                <p>Your account has been assigned the role: <strong>{roleType}</strong></p>
                <p>Please click the link below to activate your account:</p>
                <a href=""{activationLink}"">Activate Account</a>
                <p>This link expires in 24 hours.</p>
            ";

            await _emailService.SendEmailAsync(
                user.Email.Value,
                "Account Activation - Port Management System",
                emailBody
            );

            _logger.LogInformation(
                "Activation email sent to {Email} with role {Role}",
                user.Email.Value,
                roleType
            );
        }

        public async Task<IEnumerable<UserDto>> GetInactiveUsersAsync()
        {
            var users = await _userRepository.GetInactiveUsersAsync();
            return users.Select(u => new UserDto
            {
                Id = u.Id.Value,
                Email = u.Email.Value,
                Username = u.Username.Value,
                Role = u.Role.Value.ToString(),
                IsActive = u.IsActive
            });
        }
    }
}
```

## 6. Testes

### Como Executar: `dotnet test --filter "UserService"` | `npm run cypress:open -- --spec "cypress/e2e/02-admin-workflows.cy.ts"`

### Testes: ~50+ (User aggregate/services 30+, Integration 10+, E2E admin workflows)

### Excertos

**1. Assign Role**: `userService.AssignRole(email, 'LogisticOperator') → Assert user.Role == LogisticOperator`
**2. Update Role**: `PUT /api/users/{id}/role with {role: 'Admin'} → Assert 200 OK`
**3. E2E**: `cy.adminLogin() → cy.get('[data-testid="assign-role"]').select('Admin') → cy.contains('Role assigned').should('exist')`

## 7. Observações

### Conformidade com Critérios de Aceitação

✅ **Gestão de roles completa:**

1. **IAM Attributes**: Users identificados por email, name (de Google OAuth).

2. **First-Time Authorization**:

   - Unique activation link enviado por email
   - User começa em status "deactivated" por padrão

3. **Internal Roles**: Roles determinam nível de acesso ao sistema.

### Destaques da Implementação

- **Admin-Only Operation**: Só Admin pode assign/update roles.
- **User Registration Flow**:

  1. Admin cria user com email e role
  2. Sistema gera activation token
  3. Email enviado com activation link
  4. User deactivated por default até ativar via link

- **Activation Link**: `/api/users/activate?token={guid}` with expiration.

- **Security Layers**:
  - CORS configurado para permitir Frontend origin
  - JWT middleware valida tokens
  - Authorization middleware verifica roles
  - HTTPS enforced em produção

### Observações de Workflow

- **Email-Based Activation**: Confirma que user tem acesso ao email registado.
- **Default Deactivated**: Previne acesso acidental antes de ativação.
- **Role Assignment First**: Admin escolhe role antes de user ativar (user não escolhe próprio role).

### User Management Features

- **Create User**: Admin regista email, name, role.
- **Assign Role**: Admin muda role de users existentes.
- **List Users**: Admin vê todos users com roles e status.
- **Deactivate User**: Admin pode desativar sem deletar (preserva audit trail).

### Observações de Segurança

- **Role Privilege Escalation Prevention**: Users não podem self-assign roles.
- **Activation Token Expiration**: Links expiram após 7 dias (configurável).
- **Email Verification**: Activation confirma ownership do email.
