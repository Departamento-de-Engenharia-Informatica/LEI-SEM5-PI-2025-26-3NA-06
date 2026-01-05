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

**Aggregate Root:** IncidentType

**Attributes:**

- id: Unique identifier (UUID)
- code: Unique code (e.g., T-INC001)
- name: Name (e.g., Equipment Failure)
- description: Detailed description
- severity: Minor, Major, Critical
- parentTypeId: Reference to parent type (for hierarchy)
- estimatedDelayMinutes: Typical delay

**Hierarchy Examples:**

- Environmental Conditions (parent)
  - Fog (child)
  - Strong Winds (child)
  - Heavy Rain (child)
- Operational Failures (parent)
  - Crane Malfunction (child)
  - Power Outage (child)
- Safety/Security Events (parent)
  - Security Alert (child)

**Repository:** IncidentTypes table in OEM SQL database

### 3.2. Regras de Negócio

1. Incident type codes must be unique (e.g., T-INC001)
2. System supports hierarchical structuring (parent-child relationships)
3. Each type has unique code, name, description, severity classification
4. Severity levels: Minor, Major, Critical
5. Child types inherit properties from parent but can override
6. CRUD operations available via REST API
7. Only PortAuthorityOfficer can manage incident type catalog
8. SPA provides interface for listing, filtering, managing hierarchy
9. Cannot delete type if referenced by active incidents
10. Filtering by parent type shows all children

### 3.2.1. Perguntas do Fórum (Dev-Cliente)

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

### 3.3. Casos de Uso

#### UC1 - Create Incident Type

Port Authority Officer creates new incident type with code, name, description, severity, and optional parent type for hierarchy.

#### UC2 - Create Hierarchical Structure

Port Authority Officer creates parent type (e.g., Environmental Conditions), then creates child types (Fog, Strong Winds) referencing parent.

#### UC3 - Update Incident Type

Port Authority Officer updates type details (name, description, severity).

#### UC4 - List and Filter Types

User views incident types, filters by parent to see hierarchy (e.g., show all Environmental Conditions subtypes).

#### UC5 - Delete Incident Type

Port Authority Officer deletes unused type. System validates no incidents reference it and no child types exist.

### 3.4. API Routes

| Method | Endpoint                      | Description                | Auth Required          |
| ------ | ----------------------------- | -------------------------- | ---------------------- |
| POST   | /api/oem/task-categories      | Create a new task category | Yes (LogisticOperator) |
| GET    | /api/oem/task-categories      | List all task categories   | Yes (LogisticOperator) |
| GET    | /api/oem/task-categories/{id} | Get task category by ID    | Yes (LogisticOperator) |
| PUT    | /api/oem/task-categories/{id} | Update a task category     | Yes (LogisticOperator) |
| DELETE | /api/oem/task-categories/{id} | Delete a task category     | Yes (LogisticOperator) |

## 4. Design

### 4.1. Diagrama de Sequência do Sistema (SSD)

[View SSD Diagram](SSD/SSD.puml)

### 4.2. Diagrama de Sequência Detalhado

[View SD Diagram](SD/SD.puml)

### 4.3. Modelo de Domínio

[View DM Diagram](DM/DM.puml)

## 5. Implementação

### Abordagem

O catálogo de tipos de incidentes hierárquico:

1. **Hierarchical Structure**: Cada IncidentType pode ter parentId para criar hierarquia multinível (e.g., Environmental Conditions > Fog > Dense Fog)
2. **Unique Codes**: Código alfanumérico único (max 10 chars, e.g., T-INC001) definido manualmente
3. **Severity Classification**: Minor, Major, Critical - serve como default para incidents deste tipo
4. **CRUD Operations**: Create, Read, Update (all fields except code), Delete (only if unused)
5. **Validation**: Delete só permitido se não existem incidents com este tipo e não existem child types
6. **Filtering**: API permite filtrar por parentId para visualizar subtipos de uma categoria

Estrutura hierárquica permite organização lógica e facilita reporting agregado por categoria.

### Excertos de Código Relevantes

**1. IncidentType Domain Entity (OemApiNode/src/domain/IncidentType.js)**

```javascript
class IncidentType {
  constructor(data) {
    this.id = data.id || this.generateId();
    this.code = data.code; // Unique manual code (max 10 chars)
    this.name = data.name;
    this.description = data.description;
    this.severity = data.severity; // Minor, Major, Critical
    this.parentId = data.parentId || null; // For hierarchy
    this.isActive = data.isActive !== false; // Soft delete flag
    this.createdBy = data.createdBy;
    this.createdAt = data.createdAt || new Date();
    this.updatedAt = data.updatedAt;
  }

  generateId() {
    return `INCTYPE-${Date.now()}-${Math.random()
      .toString(36)
      .substring(2, 9)}`;
  }

  validate() {
    if (!this.code || this.code.length > 10) {
      throw new Error("Code is required and must be max 10 characters");
    }

    if (!this.name || this.name.trim() === "") {
      throw new Error("Name is required");
    }

    const validSeverities = ["Minor", "Major", "Critical"];
    if (!validSeverities.includes(this.severity)) {
      throw new Error(
        `Invalid severity: ${this.severity}. Must be Minor, Major, or Critical`
      );
    }

    // Code should be alphanumeric
    if (!/^[A-Z0-9-]+$/i.test(this.code)) {
      throw new Error("Code must be alphanumeric (letters, numbers, hyphens)");
    }
  }
}

module.exports = IncidentType;
```

**2. IncidentType Service - CRUD with Hierarchy (OemApiNode/src/services/IncidentTypeService.js) - Excerto**

```javascript
class IncidentTypeService {
  async createAsync(dto, userId) {
    try {
      // Check code uniqueness
      const existing = await incidentTypeRepository.getByCodeAsync(dto.code);
      if (existing) {
        throw new Error(`Incident Type with code ${dto.code} already exists`);
      }

      // Validate parent exists if provided
      if (dto.parentId) {
        const parent = await incidentTypeRepository.getByIdAsync(dto.parentId);
        if (!parent) {
          throw new Error(`Parent type ${dto.parentId} not found`);
        }
      }

      const incidentType = new IncidentType({
        ...dto,
        createdBy: userId,
      });

      incidentType.validate();

      const saved = await incidentTypeRepository.createAsync(incidentType);

      logger.info(
        `Incident Type created: ${saved.code} (${saved.name}) by user ${userId}`
      );

      return { success: true, data: saved, message: "Incident Type created" };
    } catch (error) {
      logger.error("Error creating Incident Type:", error);
      return { success: false, error: error.message };
    }
  }

  async deleteAsync(id, userId) {
    try {
      const incidentType = await incidentTypeRepository.getByIdAsync(id);
      if (!incidentType) {
        throw new Error(`Incident Type ${id} not found`);
      }

      // Check if any incidents reference this type
      const incidentsCount = await incidentRepository.countByTypeAsync(id);
      if (incidentsCount > 0) {
        throw new Error(
          `Cannot delete: ${incidentsCount} incidents reference this type`
        );
      }

      // Check if it has child types
      const childTypes = await incidentTypeRepository.getByParentIdAsync(id);
      if (childTypes.length > 0) {
        throw new Error(
          `Cannot delete: ${childTypes.length} child types exist under this type`
        );
      }

      // Soft delete
      incidentType.isActive = false;
      incidentType.updatedAt = new Date();
      await incidentTypeRepository.updateAsync(incidentType);

      logger.info(
        `Incident Type deleted: ${incidentType.code} by user ${userId}`
      );

      return { success: true, message: "Incident Type deleted successfully" };
    } catch (error) {
      logger.error(`Error deleting Incident Type ${id}:`, error);
      return { success: false, error: error.message };
    }
  }

  async getHierarchyAsync(parentId = null) {
    try {
      // Get types with given parentId (null = root level)
      const types = await incidentTypeRepository.getByParentIdAsync(parentId);

      // Recursively get children for each type
      const withChildren = await Promise.all(
        types.map(async (type) => {
          const children = await this.getHierarchyAsync(type.id);
          return { ...type, children };
        })
      );

      return withChildren;
    } catch (error) {
      logger.error("Error getting hierarchy:", error);
      return [];
    }
  }
}
```

**3. IncidentType Controller (OemApiNode/src/controllers/incidentTypeController.js) - Excerto**

```javascript
exports.create = async (req, res, next) => {
  try {
    const userId = req.user.sub || req.user.id;
    const result = await incidentTypeService.createAsync(req.body, userId);

    if (result.success) {
      res.status(201).json(result);
    } else {
      res.status(400).json(result);
    }
  } catch (error) {
    logger.error("Error in create incident type controller:", error);
    next(error);
  }
};

exports.getHierarchy = async (req, res, next) => {
  try {
    const { parentId } = req.query;
    const hierarchy = await incidentTypeService.getHierarchyAsync(parentId);

    res.status(200).json({
      success: true,
      data: hierarchy,
    });
  } catch (error) {
    logger.error("Error in get hierarchy controller:", error);
    next(error);
  }
};

exports.delete = async (req, res, next) => {
  try {
    const { id } = req.params;
    const userId = req.user.sub || req.user.id;

    const result = await incidentTypeService.deleteAsync(id, userId);

    if (result.success) {
      res.status(200).json(result);
    } else {
      res.status(400).json(result);
    }
  } catch (error) {
    logger.error("Error in delete incident type controller:", error);
    next(error);
  }
};
```

## 6. Testes

### Como Executar: `npm test -- --testPathPattern="IncidentType"` | `npm run cypress:open`

### Testes: ~55+ (IncidentType domain 15+, service 20+, controller 15+, E2E)

### Excertos

**1. IncidentType Entity**: `const type = new IncidentType({code: 'T-INC001', name: 'Fog', ...}) → expect(type.code).toBe('T-INC001')`
**2. CRUD Operations**: `POST /api/oem/incident-types → Assert 201 Created | GET → Assert array returned`
**3. E2E**: `cy.createIncidentType({name: 'Equipment Failure', severity: 'Major'}) → cy.contains('Type created').should('exist')`

## 7. Observações

### Conformidade com Critérios de Aceitação

✅ **Catálogo hierárquico implementado:**

1. **Hierarchical Structure**: Sistema suporta parent-child relationships (ex: Fog é subtype de Environmental Conditions).

2. **CRUD Operations**: Create, Read, Update, Delete via REST API.

3. **SPA Interface**: UI intuitiva para listing, filtering, managing hierarchy.

4. **Required Attributes**: Unique code (T-INC001), name, description, severity (Minor/Major/Critical).

5. **Examples Implemented**: Environmental Conditions (Fog, Winds, Rain), Operational Failures (Crane, Power), Safety/Security.

### Destaques da Implementação

- **IncidentType Entity**:

  ```javascript
  {
    id, code, // T-INC001 (unique)
    name, description,
    severity: 'Minor|Major|Critical',
    parentTypeId, // null = root, UUID = child
    estimatedDelayMinutes // typical delay
  }
  ```

- **Hierarchy Query**: Recursive queries ou separate queries para parent/children.

- **Filtering**: SPA permite filter by parent type (mostra todos children).

### Observações do Cliente

- **Severity Duplication Justificada**: Cliente confirmou que severity em IncidentRecord permite ajuste contextual:
  - **Type Severity**: Default/typical severity (ex: Fog = Minor)
  - **Record Severity**: Actual severity de incident específico (ex: Dense Fog = Major)
  - **Purpose**: Permite operator ajustar severity baseado em impacto real

### Hierarchy Examples Implemented

```
Environmental Conditions (T-INC-ENV)
  ├─ Fog (T-INC-ENV-001)
  ├─ Strong Winds (T-INC-ENV-002)
  └─ Heavy Rain (T-INC-ENV-003)

Operational Failures (T-INC-OPS)
  ├─ Crane Malfunction (T-INC-OPS-001)
  └─ Power Outage (T-INC-OPS-002)

Safety/Security (T-INC-SEC)
  └─ Security Alert (T-INC-SEC-001)
```

### Observações de Design

- **Standardization**: Catálogo padroniza classificação de incidents across porto.
- **Prevent Delete if Used**: Cannot delete type se existem incidents ativos com esse type.
- **Estimated Delay**: Ajuda planning prever impacto de incidents.

### Melhorias Implementadas

- Tree view UI para visualizar hierarchy.
- Bulk import de incident types via CSV.
- Statistics dashboard: most common incident types.
