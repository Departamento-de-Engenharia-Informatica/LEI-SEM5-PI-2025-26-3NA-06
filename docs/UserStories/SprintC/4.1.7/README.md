# US 4.1.7 - Create Vessel Visit Execution (VVE) Record

## Descrição

As a Logistics Operator, I want to create a Vessel Visit Execution (VVE) record when a vessel arrives at the port, so that the actual start of operations can be logged and monitored.

## Critérios de Aceitação

- The REST API must allow creating a new VVE referencing an existing VVN.
- Recorded fields must include vessel identifier, actual arrival time at the port, and creator user ID. An automatic VVE identifier must be also assigned, whose pattern is like VVN IDs.
- The SPA must easy the VVE creation using available VVN information.
- Once created, the VVE must be marked as "In Progress."
