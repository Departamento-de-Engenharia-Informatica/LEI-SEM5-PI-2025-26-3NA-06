# US 3.2.1 - External IAM Authentication

## Descrição

As a (Non-Authenticated) System User, I want to authenticate using the external IAM provider, so that I can securely access the system without managing separate credentials.

## Critérios de Aceitação

- The SPA must integrate with the selected IAM (e.g., via OAuth2/OpenID Connect).
- Unauthenticated users must be redirected to the IAM login page.
- The system must not handle the password storage.
- After successful authentication, a valid access token must be available to the front-end.
- Logout must also be supported, clearing tokens/session data.
