# US 3.4.1 - Dedicated Planning Module

## Descrição

As a Project Manager, I want the team to develop a dedicated back-end module that provides planning and scheduling algorithms through a REST-based API, consuming information from the existing back-end modules, so that operational plans can be computed dynamically and consistently without duplicating data.

## Critérios de Aceitação

- The module must expose its algorithms / functionalities through a REST-based API.
- The module must consume existing data from other back-end services via their exposed APIs (e.g., staff, resources).
- The module must not persist operational data — it only computes and returns scheduling results upon request.
- Input and output payloads must follow JSON format and use consistent identifiers with other modules (e.g., resource IDs).
- The module API must be properly documented (e.g. via OpenAPI/Swagger) and accessible.
