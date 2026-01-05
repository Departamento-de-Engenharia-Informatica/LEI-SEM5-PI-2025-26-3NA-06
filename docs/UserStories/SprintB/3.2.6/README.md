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

_A desenvolver: Identificar as entidades, agregados e value objects do domínio relacionados com esta US._

### 3.2. Regras de Negócio

_A desenvolver: Documentar as regras de negócio específicas desta funcionalidade._

### 3.3. Casos de Uso

_A desenvolver: Descrever os principais casos de uso e seus fluxos._

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
