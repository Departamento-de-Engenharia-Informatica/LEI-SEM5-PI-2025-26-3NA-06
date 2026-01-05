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

_A desenvolver: Identificar as entidades, agregados e value objects do domínio relacionados com esta US._

### 3.2. Regras de Negócio

_A desenvolver: Documentar as regras de negócio específicas desta funcionalidade._

### 3.3. Casos de Uso

_A desenvolver: Descrever os principais casos de uso e seus fluxos._

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

## Perguntas do Fórum (Dev-Cliente)

**Q1:**
I would like to ask whether there are any specific recommendations regarding the IAM provider to be used, or if further details about this User Story will be provided.

**A1:**
It is the team's responsibility to select the external IAM provider to integrate with, as long as it serves the intended purpose.
For instance, integration with Google, Facebook, Microsoft, or similar providers fits the project purposes.
Standard protocols must be supported by the IAM, so pay attention to the acceptance criteria.

Integration procedures depend on the selected IAM, so this should be taken into consideration when choosing the provider.
