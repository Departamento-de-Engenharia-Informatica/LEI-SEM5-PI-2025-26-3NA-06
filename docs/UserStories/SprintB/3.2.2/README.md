# US 3.2.2 - Load Internal Authorization Role

## Descrição

As a System User, I want the system to automatically load my internal authorization role after authentication, so that I gain access only to my permitted features.

## Critérios de Aceitação

- After IAM login, the SPA must call a backend endpoint to retrieve the user's assigned role and render the respective menu options.
- If the user has no assigned role or it is inactive, access must be denied with an appropriate message.

## Implementação

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
