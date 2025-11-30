# US 4.1.13 - Record and Manage Incidents

## Descrição

As a Logistics Operator, I want to record and manage incidents that affect the execution of port operations, so that delays and operational disruptions can be accurately tracked, scoped, and analyzed.

## Critérios de Aceitação

- CRUD operations for incidents must be available via the REST API.
- The SPA must allow:
  - Filtering and listing incidents by vessel, date range, severity, or status (active/resolved).
  - Quickly associating or detaching affected VVEs.
  - Highlighting active incidents that are currently impacting operations.
- Each incident record must include: a unique generated ID, a reference to its Incident Type, start and end timestamps (allowing ongoing incidents to be marked as active), a severity level (e.g., minor, major, critical), a free-text description, a responsible user (the creator).
- An incident may affect: (i) all ongoing VVEs; (ii) specific VVEs (manually selected); or (iii) upcoming VVEs (planned for later execution on the same day or period).
- When an incident is marked as resolved (i.e., end time is set), its duration must be computed automatically.
