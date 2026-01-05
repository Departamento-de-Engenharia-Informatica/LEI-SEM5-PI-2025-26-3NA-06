# US 4.1.14 - Manage Complementary Task Categories

## Descrição

As a Port Operations Supervisor, I want to manage the catalog of Complementary Task Categories so that non-cargo-related activities are consistently classified and can be properly recorded during vessel visits.

## Critérios de Aceitação

- CRUD operations for Complementary Task Categories must be available via the REST API.
- The SPA must allow users to view, search, and manage these categories efficiently.
- Each category must include a unique code (e.g., CTC001), a name (e.g., Security Check, Hull Maintenance), and a brief description of the task type or context.
- Categories may optionally define a default duration or expected impact (e.g., typically 1h delay).
- Examples of possible categories:
  - Safety and Security: Onboard Security Check, Customs Inspection
  - Maintenance: Hull Repair, Equipment Calibration
  - Cleaning and Housekeeping: Deck Cleaning, Waste Removal

## Perguntas do Fórum (Dev-Cliente)

**Q1:**
Na user story 4.1.14 – Complementary Task Categories, os exemplos apresentados (por exemplo, Safety and Security: Onboard Security Check, Customs Inspection) parecem sugerir uma estrutura hierárquica semelhante à definida para os Incident Types na user story 4.1.12.
A dúvida é: as complementary task categories também devem ter hierarquia, à semelhança dos Incident Types?

**A1:**
Tens razão. Não é necessário providenciarem essa hierarquia, contudo, seria algo desejável.

---

**Q2:**
When updating a complementary task category, which fields can be edited?
Can the name, description, and duration be changed?
I assume the ID (unique code) cannot be changed.

**A2:**
All fields, excepting the unique code, may be updated.
