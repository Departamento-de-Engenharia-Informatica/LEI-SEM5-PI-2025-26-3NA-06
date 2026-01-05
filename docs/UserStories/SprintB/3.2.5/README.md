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

_A desenvolver: Identificar as entidades, agregados e value objects do domínio relacionados com esta US._

### 3.2. Regras de Negócio

_A desenvolver: Documentar as regras de negócio específicas desta funcionalidade._

### 3.3. Casos de Uso

_A desenvolver: Descrever os principais casos de uso e seus fluxos._

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
