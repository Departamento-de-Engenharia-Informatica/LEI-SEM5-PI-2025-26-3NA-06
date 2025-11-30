# US 3.2.3 - Maintain Secure Session

## Descrição

As a System User, I want my authenticated session to be maintained securely, so that I don't need to re-login frequently while using the SPA.

## Critérios de Aceitação

- Access tokens must be securely stored.
- Token expiration must be handled (e.g., silent refresh or forced re-login when invalid).
- The SPA must try to avoid unauthorized API calls by, for instance, attaching the user access token to requests.
- Back-end module(s) must also validate tokens on each request.
