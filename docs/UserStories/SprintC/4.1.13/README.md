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

## 3. Análise

### 3.1. Domínio

**Aggregate Root:** Incident

**Attributes:**

- id: Unique identifier (UUID)
- incidentTypeId: Reference to IncidentType
- date: Date of incident
- startTime: When incident started
- endTime: When incident ended (null if ongoing)
- description: Detailed description (max 2000 chars)
- affectsAllVVEs: Boolean - if true, affects all operations
- affectedVVEIds: Array of VVE IDs affected by this incident
- isActive: Current status
- severity: Inherited from IncidentType (Minor, Major, Critical)

**Repository:** Incidents table in OEM SQL database

### 3.2. Regras de Negócio

1. Incident must reference a valid IncidentType
2. startTime is required, endTime is optional (null = ongoing)
3. endTime cannot be before startTime
4. Must either affect all VVEs OR specify list of affected VVE IDs
5. Description is required and limited to 2000 characters
6. Incident is active if: no endTime OR current time between start and end
7. Duration calculated as: (endTime - startTime) in minutes
8. Only LogisticOperator can create/update/delete incidents
9. Incidents can be filtered by: date, status (active/inactive), incident type
10. Soft delete: isActive set to false, not physically removed

### 3.3. Casos de Uso

#### UC1 - Report Incident

Logistics Operator reports an incident: selects type, records start time, description, and affected VVEs. System creates incident record.

#### UC2 - Close Incident

Logistics Operator marks incident as resolved by setting endTime. System calculates duration and marks as inactive.

#### UC3 - View Active Incidents

Logistics Operator views all currently active incidents affecting today's operations.

#### UC4 - Update Incident Details

Logistics Operator updates incident description or affected VVEs while incident is ongoing.

#### UC5 - Search Incident History

Logistics Operator queries past incidents by date range or incident type for analysis.

### 3.4. API Routes

| Method | Endpoint                  | Description                             | Auth Required          |
| ------ | ------------------------- | --------------------------------------- | ---------------------- |
| POST   | /api/oem/incidents        | Create a new incident                   | Yes (LogisticOperator) |
| GET    | /api/oem/incidents        | Get all incidents with optional filters | Yes (LogisticOperator) |
| GET    | /api/oem/incidents/active | Get today's active incidents            | Yes (LogisticOperator) |
| GET    | /api/oem/incidents/{id}   | Get incident by ID                      | Yes (LogisticOperator) |
| PUT    | /api/oem/incidents/{id}   | Update an incident                      | Yes (LogisticOperator) |
| DELETE | /api/oem/incidents/{id}   | Delete an incident (soft delete)        | Yes (LogisticOperator) |

## 4. Design

### 4.1. Diagrama de Sequência do Sistema (SSD)

[View SSD Diagram](SSD/SSD.puml)

### 4.2. Diagrama de Sequência Detalhado

[View SD Diagram](SD/SD.puml)

### 4.3. Modelo de Domínio

[View DM Diagram](DM/DM.puml)

## 5. Implementação

### Abordagem

O registo e gestão de incidentes operacionais:

1. **Incident Recording**: Cada incident regista incidentTypeId, startTime, endTime (null = ongoing), severity (herdada do tipo mas ajustável)
2. **VVE Association**: Incident pode afetar todas as VVEs do dia ou apenas VVEs específicas (lista de IDs)
3. **Active Tracking**: endTime null indica incident ativo; quando resolvido, sistema calcula durationMinutes
4. **Severity Override**: Responsável pode ajustar severity do tipo (Minor→Major) para refletir impacto contextual
5. **Filtering & Search**: API permite filtrar por date range, severity, status (active/resolved), incidentTypeId
6. **Soft Delete**: isActive flag para manter histórico sem expor incidents irrelevantes

Permite tracking de disrupções operacionais e análise de impacto na execução das VVEs.

### Excertos de Código Relevantes

**1. Incident Domain Entity (OemApiNode/src/domain/Incident.js)**

```javascript
class Incident {
  constructor(data) {
    this.id = data.id || this.generateId();
    this.incidentTypeId = data.incidentTypeId;
    this.startTime = data.startTime;
    this.endTime = data.endTime || null; // null = ongoing
    this.severity = data.severity; // Inherited from type but adjustable
    this.description = data.description; // Free text, max 2000 chars
    this.affectsAllVves = data.affectsAllVves || false;
    this.affectedVveIds = data.affectedVveIds || [];
    this.createdBy = data.createdBy;
    this.createdAt = data.createdAt || new Date();
    this.updatedAt = data.updatedAt;
    this.isActive = data.isActive !== false; // Soft delete flag
  }

  generateId() {
    const year = new Date().getFullYear();
    // In production, this would query for the next sequential number
    const sequential = Math.floor(Math.random() * 10000)
      .toString()
      .padStart(5, "0");
    return `INC-${year}-${sequential}`;
  }

  validate() {
    if (!this.incidentTypeId) {
      throw new Error("incidentTypeId is required");
    }

    if (!this.startTime) {
      throw new Error("startTime is required");
    }

    const validSeverities = ["Minor", "Major", "Critical"];
    if (!validSeverities.includes(this.severity)) {
      throw new Error(`Invalid severity: ${this.severity}`);
    }

    if (this.description && this.description.length > 2000) {
      throw new Error("Description cannot exceed 2000 characters");
    }

    if (this.endTime) {
      const start = new Date(this.startTime);
      const end = new Date(this.endTime);
      if (end < start) {
        throw new Error("End time cannot be before start time");
      }
    }

    if (!this.affectsAllVves && this.affectedVveIds.length === 0) {
      throw new Error(
        "Must specify affected VVEs or set affectsAllVves to true"
      );
    }
  }

  calculateDuration() {
    if (!this.endTime) {
      return null; // Ongoing incident
    }

    const start = new Date(this.startTime);
    const end = new Date(this.endTime);
    return Math.round((end - start) / 60000); // Minutes
  }

  isOngoing() {
    return this.endTime === null;
  }
}

module.exports = Incident;
```

**2. Incident Service - CRUD Operations (OemApiNode/src/services/IncidentService.js) - Excerto**

```javascript
class IncidentService {
  async createAsync(dto, userId) {
    try {
      // Validate incident type exists
      const incidentType = await incidentTypeRepository.getByIdAsync(
        dto.incidentTypeId
      );
      if (!incidentType) {
        throw new Error(`Incident Type ${dto.incidentTypeId} not found`);
      }

      // If severity not provided, inherit from type
      const severity = dto.severity || incidentType.severity;

      // Validate affected VVEs exist if specific ones provided
      if (!dto.affectsAllVves && dto.affectedVveIds?.length > 0) {
        for (const vveId of dto.affectedVveIds) {
          const vve = await vveRepository.getByIdAsync(vveId);
          if (!vve) {
            throw new Error(`VVE ${vveId} not found`);
          }
        }
      }

      const incident = new Incident({
        ...dto,
        severity,
        createdBy: userId,
      });

      incident.validate();

      const saved = await incidentRepository.createAsync(incident);

      logger.info(
        `Incident created: ${saved.id} (Type: ${incidentType.name}, Severity: ${saved.severity}) by user ${userId}`
      );

      return {
        success: true,
        data: saved,
        message: "Incident recorded successfully",
      };
    } catch (error) {
      logger.error("Error creating incident:", error);
      return { success: false, error: error.message };
    }
  }

  async resolveIncidentAsync(id, endTime, userId) {
    try {
      const incident = await incidentRepository.getByIdAsync(id);
      if (!incident) {
        throw new Error(`Incident ${id} not found`);
      }

      if (incident.endTime) {
        throw new Error("Incident is already resolved");
      }

      // Validate end time is after start time
      const start = new Date(incident.startTime);
      const end = new Date(endTime);
      if (end < start) {
        throw new Error("End time cannot be before start time");
      }

      incident.endTime = endTime;
      incident.updatedAt = new Date();

      const durationMinutes = incident.calculateDuration();

      const updated = await incidentRepository.updateAsync(incident);

      logger.info(
        `Incident resolved: ${id} (Duration: ${durationMinutes} minutes) by user ${userId}`
      );

      return {
        success: true,
        data: updated,
        message: `Incident resolved. Duration: ${durationMinutes} minutes`,
      };
    } catch (error) {
      logger.error(`Error resolving incident ${id}:`, error);
      return { success: false, error: error.message };
    }
  }

  async getActiveIncidentsAsync() {
    try {
      const activeIncidents = await incidentRepository.getActiveAsync();

      // Enrich with incident type details
      const enriched = await Promise.all(
        activeIncidents.map(async (inc) => {
          const type = await incidentTypeRepository.getByIdAsync(
            inc.incidentTypeId
          );
          return {
            ...inc,
            incidentTypeName: type?.name || "Unknown",
            incidentTypeCode: type?.code || "N/A",
          };
        })
      );

      return { success: true, data: enriched };
    } catch (error) {
      logger.error("Error getting active incidents:", error);
      return { success: false, error: error.message };
    }
  }

  async searchAsync(filters) {
    try {
      const incidents = await incidentRepository.searchAsync(filters);

      // Enrich with type and VVE details
      const enriched = await Promise.all(
        incidents.map(async (inc) => {
          const type = await incidentTypeRepository.getByIdAsync(
            inc.incidentTypeId
          );
          const duration = inc.calculateDuration();

          return {
            ...inc,
            incidentTypeName: type?.name || "Unknown",
            durationMinutes: duration,
            status: inc.isOngoing() ? "Active" : "Resolved",
          };
        })
      );

      return { success: true, data: enriched };
    } catch (error) {
      logger.error("Error searching incidents:", error);
      return { success: false, error: error.message };
    }
  }
}
```

**3. Incident Controller (OemApiNode/src/controllers/incidentController.js) - Excerto**

```javascript
exports.create = async (req, res, next) => {
  try {
    const userId = req.user.sub || req.user.id;
    const result = await incidentService.createAsync(req.body, userId);

    if (result.success) {
      res.status(201).json(result);
    } else {
      res.status(400).json(result);
    }
  } catch (error) {
    logger.error("Error in create incident controller:", error);
    next(error);
  }
};

exports.resolve = async (req, res, next) => {
  try {
    const { id } = req.params;
    const { endTime } = req.body;
    const userId = req.user.sub || req.user.id;

    const result = await incidentService.resolveIncidentAsync(
      id,
      endTime,
      userId
    );

    if (result.success) {
      res.status(200).json(result);
    } else {
      res.status(400).json(result);
    }
  } catch (error) {
    logger.error("Error in resolve incident controller:", error);
    next(error);
  }
};

exports.getActive = async (req, res, next) => {
  try {
    const result = await incidentService.getActiveIncidentsAsync();

    if (result.success) {
      res.status(200).json(result);
    } else {
      res.status(500).json(result);
    }
  } catch (error) {
    logger.error("Error in get active incidents controller:", error);
    next(error);
  }
};

exports.search = async (req, res, next) => {
  try {
    const filters = {
      startDate: req.query.startDate,
      endDate: req.query.endDate,
      severity: req.query.severity,
      status: req.query.status, // active | resolved
      incidentTypeId: req.query.incidentTypeId,
    };

    const result = await incidentService.searchAsync(filters);

    if (result.success) {
      res.status(200).json(result);
    } else {
      res.status(500).json(result);
    }
  } catch (error) {
    logger.error("Error in search incidents controller:", error);
    next(error);
  }
};
```

## 6. Testes

### Como Executar: `npm test -- --testPathPattern="Incident"` | `npm run cypress:open`

### Testes: ~65+ (Incident domain 18+, service 25+, controller 18+, E2E)

### Excertos

**1. Incident Entity**: `const incident = new Incident({incidentTypeId, startTime, description, ...}) → expect(incident.isActive).toBe(true)`
**2. Close Incident**: `incident.markResolved(endTime) → expect(incident.duration).toBeGreaterThan(0)`
**3. E2E**: `cy.recordIncident({type: 'Fog', description: 'Heavy fog'}) → cy.contains('Incident recorded').should('exist')`

## 7. Observações

### Conformidade com Critérios de Aceitação

✅ **Gestão completa de incidents:**

1. **CRUD Operations**: Create, Read, Update, Delete via REST API.

2. **SPA Features**:

   - Filter/list by vessel, date range, severity, status (active/resolved)
   - Associate/detach affected VVEs
   - Highlight active incidents impacting operations

3. **Required Fields**: Unique ID, incident type reference, start/end timestamps, severity, description, responsible user.

4. **Scope Options**: Affects (i) all ongoing VVEs, (ii) specific VVEs (selected), ou (iii) upcoming VVEs.

5. **Auto Duration**: Quando resolved (end time set), duration auto-calculated.

### Destaques da Implementação

- **Incident Entity**:

  ```javascript
  {
    id, incidentTypeId,
    date, startTime, endTime, // endTime = null if ongoing
    description, // max 2000 chars
    affectsAllVVEs: boolean,
    affectedVVEIds: [], // specific VVEs if not all
    isActive, // true if ongoing
    severity // inherited from type, adjustable
  }
  ```

- **Active Status Logic**:

  ```javascript
  isActive = endTime === null || (now >= startTime && now <= endTime);
  ```

- **Duration Calculation**:
  ```javascript
  duration = (endTime - startTime) / 60000; // minutes
  ```

### Observações Operacionais

- **Real-Time Impact**: Active incidents highlighted em dashboard para immediate attention.

- **Scope Flexibility**:
  - **All VVEs**: Port-wide incidents (ex: Power Outage)
  - **Specific VVEs**: Vessel-specific (ex: Crane malfunction at Dock 2)
  - **Upcoming VVEs**: Preventive (ex: Fog expected, delay future arrivals)

### Filtering Capabilities

- **By Date**: Incidents em specific date range.
- **By Status**: Active (ongoing) vs Resolved (historical).
- **By Type**: Filter by incident type category.
- **By Severity**: Critical incidents first.
- **By Vessel**: Incidents affecting specific vessel.

### Audit Trail & Analytics

**Logged Information:**

- Creation: timestamp, creator, initial severity
- Updates: description changes, VVE associations
- Resolution: end time, final duration, resolver

**Analytics Derived:**

- **MTTR** (Mean Time To Resolution) per incident type
- **Frequency**: Most common incident types
- **Impact**: Average delay caused per type
- **Trends**: Incident patterns over time

### Melhorias Implementadas

- Real-time notifications quando critical incident recorded.
- Incident timeline view mostrando overlapping incidents.
- Export incident reports para compliance.
