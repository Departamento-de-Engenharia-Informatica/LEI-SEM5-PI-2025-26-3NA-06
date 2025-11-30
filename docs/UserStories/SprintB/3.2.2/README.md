# US 3.2.2 - Load Internal Authorization Role

## Descrição

As a System User, I want the system to automatically load my internal authorization role after authentication, so that I gain access only to my permitted features.

## Critérios de Aceitação

- After IAM login, the SPA must call a backend endpoint to retrieve the user's assigned role and render the respective menu options.
- If the user has no assigned role or it is inactive, access must be denied with an appropriate message.
