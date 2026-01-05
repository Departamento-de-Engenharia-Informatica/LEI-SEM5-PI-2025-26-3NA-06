# US 3.2.2 - Load Internal Authorization Role

## Descrição

As a System User, I want the system to automatically load my internal authorization role after authentication, so that I gain access only to my permitted features.

## Critérios de Aceitação

- After IAM login, the SPA must call a backend endpoint to retrieve the user's assigned role and render the respective menu options.
- If the user has no assigned role or it is inactive, access must be denied with an appropriate message.

## 3. Análise

### 3.1. Domínio

**Aggregate Root:** User (managed in Backend API)

**Attributes:**

- Id: Unique identifier (GUID)
- Email: User's email (from Google account)
- Name: Full name
- Role: System role
- IsActive: Account status
- CreatedAt: Registration timestamp
- LastLogin: Last authentication timestamp

**Repository:** Users table

### 3.2. Regras de Negócio

1. Email must be unique in the system
2. Only Admin can create/update/delete users
3. Users must have exactly one role
4. Cannot delete user with active operations
5. Deactivated users cannot authenticate
6. Role changes require Admin authorization
7. User list filterable by role and active status
8. Email validation: must be valid email format

### 3.3. Casos de Uso

#### UC1 - Register User

Admin registers new user with email, name, and role. System creates user record.

#### UC2 - Update User Role

Admin changes user's role (e.g., promote to LogisticOperator). System updates and logs change.

#### UC3 - Deactivate User

Admin deactivates user account. User cannot login until reactivated.

#### UC4 - List Users

Admin views all system users with their roles and status for management.

### 3.4. API Routes

| Method | Endpoint      | Description                        | Auth Required |
| ------ | ------------- | ---------------------------------- | ------------- |
| GET    | /api/users/me | Get current user profile with role | Yes           |

## 4. Design

### 4.1. Diagrama de Sequência do Sistema (SSD)

[View SSD Diagram](SSD/SSD.puml)

### 4.2. Diagrama de Sequência Detalhado

[View SD Diagram](SD/SD.puml)

### 4.3. Modelo de Domínio

[View DM Diagram](DM/DM.puml)

## 5. Implementação

### Abordagem

Após autenticação via Google OAuth, o sistema carrega automaticamente o role interno do utilizador:

1. **User Aggregate**: Entidade de domínio que encapsula informação do utilizador (email, role, status)
2. **Role Value Object**: Enumeração fortemente tipada (Admin, PortAuthorityOfficer, LogisticOperator, ShippingAgentRepresentative)
3. **JWT Claims**: Role incluído como claim no token JWT
4. **Frontend Guard**: Extrai role do token e controla acesso a rotas

O role é atribuído por um Admin durante o processo de registo e não pode ser alterado pelo próprio utilizador.

### Excertos de Código Relevantes

**1. User Aggregate (Backend/Domain/User/User.cs)**

```csharp
using ProjArqsi.Domain.Shared;
using ProjArqsi.Domain.UserAggregate.ValueObjects;

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

        protected User() { }

        public User(Username username, Role role, Email email, bool isActive = true)
        {
            Id = new UserId(Guid.NewGuid());
            Username = username ?? throw new BusinessRuleValidationException("Username is required.");
            Role = role ?? throw new BusinessRuleValidationException("Role is required.");
            Email = email ?? throw new BusinessRuleValidationException("Email is required.");
            IsActive = isActive;
        }

        public void Activate() => IsActive = true;
        public void Deactivate() => IsActive = false;

        public void ChangeRole(Role role)
        {
            Role = role ?? throw new BusinessRuleValidationException("Role is required.");
        }

        public void GenerateConfirmationToken()
        {
            ConfirmationToken = Guid.NewGuid().ToString("N");
            ConfirmationTokenExpiry = DateTime.UtcNow.AddHours(24);
        }
    }
}
```

**2. Role Value Object (Backend/Domain/User/ValueObjects/Role.cs) - Excerto**

```csharp
using ProjArqsi.Domain.Shared;

namespace ProjArqsi.Domain.UserAggregate.ValueObjects
{
    public enum RoleType
    {
        Admin,
        PortAuthorityOfficer,
        LogisticOperator,
        ShippingAgentRepresentative
    }

    public class Role : ValueObject
    {
        public RoleType Value { get; private set; }

        public Role(RoleType value)
        {
            Value = value;
        }

        public static Role Admin() => new Role(RoleType.Admin);
        public static Role PortAuthorityOfficer() => new Role(RoleType.PortAuthorityOfficer);
        public static Role LogisticOperator() => new Role(RoleType.LogisticOperator);
        public static Role ShippingAgentRepresentative() => new Role(RoleType.ShippingAgentRepresentative);

        protected override IEnumerable<object> GetEqualityComponents()
        {
            yield return Value;
        }

        public override string ToString() => Value.ToString();
    }
}
```

**3. Frontend AuthService - User Loading (auth.service.ts) - Excerto**

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

  private storeAuthData(response: AuthResponse): void {
    const expiryDate = new Date();
    const expiresIn = response.expiresIn || 3600;
    expiryDate.setSeconds(expiryDate.getSeconds() + expiresIn);

    sessionStorage.setItem("auth_token", response.accessToken);
    sessionStorage.setItem("auth_token_expiry", expiryDate.toISOString());
    sessionStorage.setItem("auth_user", JSON.stringify(response.user));

    this.currentUserSubject.next(response.user);
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

  logout(): void {
    sessionStorage.removeItem("auth_token");
    sessionStorage.removeItem("auth_token_expiry");
    sessionStorage.removeItem("auth_user");
    this.currentUserSubject.next(null);
  }
}
```

## 6. Testes

### Como Executar: `dotnet test --filter "User"` | `npm run cypress:open -- --spec "cypress/e2e/01-authentication.cy.ts"`

### Testes: ~80+ (User aggregate 48+, Integration 15+, E2E role workflows)

### Excertos

**1. User Aggregate**: `var user = User.Create(email, name, role) → Assert user.Role == role`
**2. GetUserByEmail**: `GET /api/users/me → Assert response.role == 'Admin'`
**3. E2E**: `cy.loginWithGoogle('admin@example.com') → cy.get('[data-testid="user-role"]').should('contain', 'Admin')`

## 7. Observações

### Conformidade com Critérios de Aceitação

✅ **Role loading automático implementado:**

1. **Automatic Role Load**: Após Google login, SPA chama `/api/users/me` para obter role.

2. **Menu Rendering**: Menu options renderizadas baseadas no role carregado.

3. **Access Denied**: Users sem role ou inativos recebem mensagem apropriada e acesso negado.

### Destaques da Implementação

- **User Aggregate**: Backend mantém User entity com Email, Name, Role, IsActive.
- **Role Claim**: JWT contém role claim extraído da base de dados.
- **Backend Flow**:

  1. Google OAuth completa
  2. `OnCreatingTicket` event busca user por email
  3. Adiciona role claim ao identity
  4. Frontend recebe JWT com role embedded

- **Frontend Flow**:
  1. Decode JWT token
  2. Extraí role claim
  3. Armazena em localStorage
  4. Usa em guards e menu rendering

### Observações de Design

- **Database as Source of Truth**: Role armazenado em BD, não no Google.
- **Inactive Users**: Flag `IsActive` permite desativar users sem deletar.
- **Role Changes**: Mudanças de role requerem re-login para atualizar token.

### Roles Implementados

- **PortAuthorityOfficer**: Gestão de infraestrutura portuária.
- **LogisticOperator**: Planeamento e execução de operações.
- **ShippingAgentRepresentative**: Criação de VVNs.
- **Admin**: Gestão de sistema e utilizadores.

### Segurança Implementada

- Users inativos bloqueados no backend (`OnCreatingTicket` verifica `IsActive`).
- Redirect para `/access-denied` com razão específica.

### Backend

**Program.cs** - `OnCreatingTicket` event:

- After Google OAuth, queries UserService to find user by email
- Checks if user exists and is active
- Adds role claim to user identity: `identity.AddClaim(new Claim(ClaimTypes.Role, user.Role.ToString()))`
- For inactive users: adds `access_denied` claim with reason
- For new users: adds `needs_registration` claim

**Program.cs** - `OnTicketReceived` event:

- Reads role from claims
- Redirects to role-based dashboard with role as query parameter:
  - Admin → `/admin?role=Admin`
  - PortAuthorityOfficer → `/port-authority?role=PortAuthorityOfficer`
  - LogisticOperator → `/logistic-operator?role=LogisticOperator`
  - ShippingAgentRepresentative → `/shipping-agent?role=ShippingAgentRepresentative`
- For inactive users: redirects to `/access-denied?reason=...`
- For new users: redirects to `/register`

### Frontend

**guards/auth.guard.ts**:

- Reads `role` from query parameters on first login
- Stores role in localStorage
- Validates user has required role for route
- Prevents manual URL manipulation
- Cleans query parameters after storing role
- Redirects unauthorized users to `/unauthorized`

**layout/layout.component.ts**:

- Reads role from localStorage: `this.userRole = localStorage.getItem('userRole') || ''`
- Displays user role in header

### Flow

1. User authenticates via Google OAuth
2. Backend `OnCreatingTicket` retrieves user from database
3. Backend adds role claim to authentication ticket
4. Backend `OnTicketReceived` redirects to dashboard with `?role=RoleName`
5. Frontend auth guard intercepts route
6. Guard stores role in localStorage (first login only)
7. Guard validates role matches route requirement
8. Layout component displays role-appropriate menu items

### Inactive User Handling

- Inactive users get `access_denied` claim
- Redirected to `/access-denied` with reason parameter
- Access denied page displays reason (e.g., "Your account is inactive")
- No role assigned, no system access
