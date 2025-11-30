# US 4.1.8 - Update VVE with Berth Time and Dock

## Descrição

As a Logistics Operator, I want to update an in progress VVE with the actual berth time and dock used, so that discrepancies from the planned dock assignment are recorded.

## Critérios de Aceitação

- The REST API must support update of berth time and dock ID.
- If the assigned dock differs from the planned one, a warning or note must be automatically added.
- All updates must be timestamped and logged for auditability.
