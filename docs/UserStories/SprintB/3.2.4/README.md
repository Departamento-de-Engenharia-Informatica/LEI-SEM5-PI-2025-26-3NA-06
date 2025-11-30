# US 3.2.4 - Role-Based Access Control

## Descrição

As a System User, I want the system to restrict access to actions and features based on my role, so that I cannot perform unauthorized operations.

## Critérios de Aceitação

- On the back-end side:
  - Each REST API route must enforce role-based access control (RBAC) and/or attribute-based access control (ABAC) as needed to enforce the applicable business rules.
  - Unauthorized requests must return proper HTTP status codes (e.g., 403 Forbidden).
  - Logs must record unauthorized attempts.
- On the front-end side:
  - Front-end routes must check for the user's authorization before rendering pages.
  - Direct URL access to unauthorized pages must be prevented.
  - A default "Access Denied" or "Not Authorized" page must be shown when needed.
