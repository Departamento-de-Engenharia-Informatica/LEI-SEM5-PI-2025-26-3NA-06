# US 4.1.15 - Record and Manage Complementary Tasks

## Descrição

As a Logistics Operator, I want to record and manage Complementary Tasks performed during vessel visits, so that non-cargo activities (e.g., inspections, cleaning, maintenance) can be tracked and correlated with operational efficiency.

## Critérios de Aceitação

- A Complementary Task may either:
  - Run in parallel with ongoing cargo operations (e.g., inspection), or
  - Temporarily suspend execution of ongoing cargo operations until it is concluded (e.g., maintenance, safety procedure).
- CRUD operations for Complementary Tasks must be provided via the REST API.
- The SPA must provide:
  - A simple form to log or edit Complementary Tasks,
  - Filtering and listing Complementary Tasks by vessel, date range, or status.
  - Highlighting on going tasks that are currently impacting operations.
- Each task record must include: a unique generated ID, a reference to its Category, the responsible team or service, the start and end timestamps (time window), the completion status (e.g. ongoing, completed), the VVE on which the task is being performed.
