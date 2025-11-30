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
