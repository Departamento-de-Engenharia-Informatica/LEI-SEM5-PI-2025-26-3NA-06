# US 4.1.12 - Manage Incident Types Catalog

## Descrição

As a Port Authority Officer, I want to manage the catalog of Incident Types so that the classification of operational disruptions remains standardized, hierarchical, and clearly distinct from complementary tasks.

## Critérios de Aceitação

- The system must support hierarchical structuring of incident types (e.g., Fog is a subtype of Environmental Conditions), allowing grouping and filtering by parent type.
- CRUD operations for Incident Types must be available via the REST API.
- The SPA must provide an intuitive interface for listing, filtering, and managing these hierarchy of types.
- Each Incident Type must include a unique code (e.g., T-INC001), a name (e.g., Equipment Failure), a detailed description, a severity classification (e.g., Minor, Major, Critical).
- Examples of possible types:
  - Environmental Conditions: Fog, Strong Winds, Heavy Rain
  - Operational Failures: Crane Malfunction, Power Outage
  - Safety/Security Events: Security Alert

## 3. Análise

### 3.1. Domínio

_A desenvolver: Identificar as entidades, agregados e value objects do domínio relacionados com esta US._

### 3.2. Regras de Negócio

_A desenvolver: Documentar as regras de negócio específicas desta funcionalidade._

### 3.3. Casos de Uso

_A desenvolver: Descrever os principais casos de uso e seus fluxos._

## Perguntas do Fórum (Dev-Cliente)

**Q1:**
US 4.1.12 refers that:
Each Incident Type must include a unique code (e.g., T-INC001), a name (e.g., Equipment Failure), a detailed description, and a severity classification (e.g., Minor, Major, Critical).

US 4.1.13 refers that:
Each Incident Record must include a unique generated ID, a reference to its Incident Type, start and end timestamps (allowing ongoing incidents to be marked as active), a severity level (e.g., minor, major, critical), a free-text description, and a responsible user (the creator).

Considering that the Incident Record inherits all the characteristics of its Incident Type, what is the role and the practical value of including the severity directly in the Incident Record (in addition to referencing the Type)?

Is the objective of this duplication to allow the severity of a specific occurrence (the Record) to be adjusted (e.g., from Major to Critical or to Minor) by the responsible user, independently of the Type's standard classification, thus reflecting the real and contextualized impact of the incident at a given moment? Or is there another primary reason for not depending exclusively on the severity defined in the Type?

**A1:**
You got it right.
The purpose of this duplication is precisely this – to allow the severity of a specific occurrence (the Record) to be adjusted by the responsible user, independently of what its type states, thus reflecting the real and contextualized impact of the incident at a given moment.
Thus, the severity information derived from the type serves as a predefined value that can be changed by the responsible user.

---

**Q2:**
Is it possible for a Port Authority Officer to update any field after creating an Incident Type?

**A2:**
Yes, excepting the unique code.

---

**Q3:**
Como dito no enunciado, um Incident pode afetar várias VVEs e uma Complementary Task pertence a um VVE.
É possível um VVE sofrer mais do que um Incident em simultâneo?
E um VVE pode ter várias Complementary Tasks associadas para realizar?

**A3:**
Sim, é possível uma VVE ser afetada por mais do que um incidente, podendo ou não ser em simultâneo.

Sim, é possível que, no âmbito de uma única VVE, sejam executadas várias tarefas complementares.

---

**Q4:**
Hi. We would like to request clarification regarding the Incident Type Catalog Management User Story, specifically concerning the required hierarchical structure:

The system must support hierarchical structuring of incident types (e.g., Fog is a subtype of Environmental Conditions), allowing grouping and filtering by parent type. Our question is about the depth of this hierarchy:

Is the structure limited to a single level of nesting (only "Parents" and "Children")?

Example: Operational Failures (Parent) → Crane Malfunction (Child).

Are multiple levels of nesting permitted (Parents, Children, and Grandchildren/Deeper Subtypes)?

Example: Operational Failures (Parent) → Equipment Failures (Child) → Crane Malfunction (Grandchild).

Considering the need for "hierarchical structuring," our assumption is that multiple levels are allowed. Confirmation of this possibility is crucial for us to correctly define the data model and the REST API interface for this functionality.

**A4:**
The depth of the hierarchy should not be limited by the system.
It is likely that in some cases there will be only one level, in others two levels, and in others three or four levels, all simultaneously.
It is up to the users to decide how they intend to define this catalog.

---

**Q5:**
No enunciado é mencionado que tanto os Incident Types como as Complementary Task Categories possuem "unique codes", e que os Incidents e as Complementary Tasks têm um "unique generated ID".
Existe algum formato específico para esses códigos? Se sim, quais são as regras? Se possível, gostaríamos também de ver alguns exemplos.

**A5:**
Relativamente aos "unique codes" dos Incident Types e das Complementary Task Categories, estes são códigos alfanuméricos (máx. 10 caracteres) introduzidos manualmente pelos utilizadores. Alguns exemplos já constam nas respetivas User Stories.

Quanto aos "unique generated IDs", estes devem seguir a seguinte estrutura, usando um separador (por exemplo, "-"):

Um prefixo constante pré-definido por configuração (por exemplo, "CT" ou "INC");

O ano de ocorrência (por exemplo, 2025, 2026, etc.);

Um número sequencial formatado com zeros à esquerda (por exemplo, "00001", "00456").

Exemplos completos:

CT-2025-00456; …; CT-2025-07654; …; CT-2026-00001;

INC-2025-00066; …; INC-2025-01001; …; INC-2026-00004.
