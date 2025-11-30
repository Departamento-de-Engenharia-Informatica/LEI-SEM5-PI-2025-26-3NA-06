# US 4.1.11 - Mark VVE as Completed

## Descrição

As a Logistics Operator, I want to mark a VVE as completed by recording the vessel's unberth time and port departure time, so that the visit lifecycle is correctly closed and operational statistics can be derived.

## Critérios de Aceitação

- The SPA must provide an option to mark a VVE as "Completed."
- To complete a VVE, the following information must be recorded:
  - Actual unberth time (when the vessel leaves the dock).
  - Actual port departure time (when the vessel exits port limits).
- Completion is only allowed if all associated cargo operations are recorded as finished.
- Once completed, the VVE becomes read-only, except for authorized corrections by admin users.
- The action must be logged (timestamp, user, any changes made) for audit purposes.
