# US 4.1.9 - Update VVE with Executed Operations

## Descrição

As a Logistics Operator, I want to update an in progress VVE with executed operations, so that the system reflects real execution progress and performance.

## Critérios de Aceitação

- Executed operations (mainly) derive from planned operations, which may be used to easy recording execution of operations.
- The SPA must allow the operator to confirm or modify start/end times and resource usage.
- The corresponding planned operations must be marked as "started," "completed," or "delayed."
- Execution updates must be stored with timestamps and operator ID.
- Completion status must synchronize with the corresponding Operation Plan for comparison.
