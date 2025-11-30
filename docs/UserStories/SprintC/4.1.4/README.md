# US 4.1.4 - Manually Update Operation Plan

## Descrição

As a Logistics Operator, I want to manually update the Operation Plan of a given VVN, so that last-minute adjustments (e.g., resource or timing changes) can be made when needed.

## Critérios de Aceitação

- The REST API must provide update endpoints.
- The SPA must allow editing key plan fields (e.g., crane assignment, start/end time, staff).
- Changes must be validated and logged (date, author, reason for change).
- The system must alert the user if the updated plans introduce possible inconsistencies with related VVNs and resource availability (e.g., cranes or staff).
