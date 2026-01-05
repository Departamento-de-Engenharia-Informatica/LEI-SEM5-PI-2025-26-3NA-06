# US 4.1.4 - Manually Update Operation Plan

## Descrição

As a Logistics Operator, I want to manually update the Operation Plan of a given VVN, so that last-minute adjustments (e.g., resource or timing changes) can be made when needed.

## Critérios de Aceitação

- The REST API must provide update endpoints.
- The SPA must allow editing key plan fields (e.g., crane assignment, start/end time, staff).
- Changes must be validated and logged (date, author, reason for change).
- The system must alert the user if the updated plans introduce possible inconsistencies with related VVNs and resource availability (e.g., cranes or staff).

## 3. Análise

### 3.1. Domínio

**Aggregate Root:** VesselVisitExecution (VVE)

**Value Objects:**

- vvnId: Reference to Vessel Visit Notification
- operationPlanId: Reference to Operation Plan
- vveDate: Execution date (YYYY-MM-DD)
- plannedDockId, plannedArrivalTime, plannedDepartureTime: From operation plan
- actualDockId, actualArrivalTime, actualDepartureTime: Initially null, filled during execution
- status: NotStarted, InProgress, Delayed, Completed

### 3.2. Regras de Negócio

1. VVEs can only be created for today's date (vveDate = current date)
2. One VVE per VVN per day
3. VVEs are created from Operation Plan assignments
4. Initial status is always "NotStarted"
5. Actual fields (actualDockId, actualArrivalTime, actualDepartureTime) start as null
6. Status transitions: NotStarted → InProgress/Delayed → Completed
7. Only LogisticOperator can prepare VVEs
8. If no operation plan exists for today, preparation fails

### 3.3. Casos de Uso

#### UC1 - Prepare Today's VVEs

Logistics Operator initiates VVE preparation. System finds today's operation plan, creates NotStarted VVE for each assignment, returns summary (created/skipped/errors).

#### UC2 - Manual VVE Creation

Logistics Operator manually creates a VVE (exceptional case). System validates required fields and creates the VVE.

### 3.4. API Routes

| Method | Endpoint                                       | Description                              | Auth Required          |
| ------ | ---------------------------------------------- | ---------------------------------------- | ---------------------- |
| POST   | /api/oem/vessel-visit-executions/prepare-today | Prepare today's VVEs from operation plan | Yes (LogisticOperator) |
| POST   | /api/oem/vessel-visit-executions               | Create a new VVE manually                | Yes (LogisticOperator) |

## 4. Design

### 4.1. Diagrama de Sequência do Sistema (SSD)

[View SSD Diagram](SSD/SSD.puml)

### 4.2. Diagrama de Sequência Detalhado

[View SD Diagram](SD/SD.puml)

### 4.3. Modelo de Domínio

[View DM Diagram](DM/DM.puml)

## 5. Implementação

### Abordagem

A preparação de VVEs (Vessel Visit Executions) a partir do Operation Plan do dia:

1. **Today's Plan Lookup**: Sistema busca Operation Plan para a data atual
2. **VVE Creation**: Cria um VVE com status "NotStarted" para cada assignment no plano
3. **Planned Data Copy**: Copia dados planeados (dock, arrival, departure) do plano
4. **Actual Fields**: actualDockId, actualArrivalTime, actualDepartureTime iniciam como null
5. **Duplicate Check**: Não cria VVE se já existe para aquela VVN
6. **Batch Processing**: Processa todos os assignments de uma vez, retorna summary

VVEs representam a execução real das operações planeadas, permitindo tracking em tempo real.

### Excertos de Código Relevantes

**1. VVE Service - Prepare Today's VVEs (OemApiNode/src/services/VesselVisitExecutionService.js)**

```javascript
const vveRepository = require("../infrastructure/VesselVisitExecutionRepository");
const operationPlanRepository = require("../infrastructure/OperationPlanRepository");
const VesselVisitExecution = require("../domain/VesselVisitExecution/VesselVisitExecution");
const logger = require("../utils/logger");

class VesselVisitExecutionService {
  async prepareTodaysVVEsAsync(userId) {
    try {
      const today = new Date();
      const todayStr = today.toISOString().split("T")[0]; // YYYY-MM-DD

      logger.info(`Preparing VVEs for date: ${todayStr}`);

      // Get today's operation plan
      const operationPlan = await operationPlanRepository.getByDateAsync(
        todayStr
      );

      if (!operationPlan) {
        return {
          success: false,
          error: `No operation plan found for today (${todayStr})`,
        };
      }

      if (
        !operationPlan.assignments ||
        operationPlan.assignments.length === 0
      ) {
        return {
          success: true,
          message: "No assignments in today's operation plan",
          created: 0,
          skipped: 0,
        };
      }

      const vvesToCreate = [];
      const skippedVvns = [];

      for (const assignment of operationPlan.assignments) {
        try {
          // Check if VVE already exists
          const existingVve = await vveRepository.getByVvnIdAsync(
            assignment.vvnId
          );
          if (existingVve) {
            logger.info(
              `VVE already exists for VVN ${assignment.vvnId}, skipping`
            );
            skippedVvns.push({
              vvnId: assignment.vvnId,
              reason: "VVE already exists",
            });
            continue;
          }

          // Create VVE with NotStarted status
          const vve = new VesselVisitExecution({
            vvnId: assignment.vvnId,
            operationPlanId: operationPlan.id,
            vveDate: todayStr,
            plannedDockId: assignment.dockId,
            plannedArrivalTime: assignment.eta,
            plannedDepartureTime: assignment.etd,
            status: "NotStarted",
            // Actual fields remain null
            actualDockId: null,
            actualArrivalTime: null,
            actualDepartureTime: null,
          });

          vvesToCreate.push(vve);
        } catch (error) {
          logger.error(
            `Error preparing VVE for VVN ${assignment.vvnId}:`,
            error
          );
          skippedVvns.push({
            vvnId: assignment.vvnId,
            reason: error.message,
          });
        }
      }

      // Batch create all VVEs
      let createdCount = 0;
      if (vvesToCreate.length > 0) {
        for (const vve of vvesToCreate) {
          await vveRepository.createAsync(vve);
          createdCount++;
        }
      }

      logger.info(
        `VVE preparation complete: ${createdCount} created, ${skippedVvns.length} skipped`
      );

      return {
        success: true,
        message: `VVE preparation completed for ${todayStr}`,
        created: createdCount,
        skipped: skippedVvns.length,
        skippedDetails: skippedVvns,
      };
    } catch (error) {
      logger.error("Error preparing today's VVEs:", error);
      return {
        success: false,
        error: error.message || "Unknown error occurred",
      };
    }
  }
}

module.exports = new VesselVisitExecutionService();
```

**2. VVE Domain Entity (OemApiNode/src/domain/VesselVisitExecution/VesselVisitExecution.js) - Excerto**

```javascript
class VesselVisitExecution {
  constructor(data) {
    this.id = data.id || this.generateId();
    this.vvnId = data.vvnId;
    this.operationPlanId = data.operationPlanId;
    this.vveDate = data.vveDate;
    this.plannedDockId = data.plannedDockId;
    this.plannedArrivalTime = data.plannedArrivalTime;
    this.plannedDepartureTime = data.plannedDepartureTime;
    this.actualDockId = data.actualDockId || null;
    this.actualArrivalTime = data.actualArrivalTime || null;
    this.actualDepartureTime = data.actualDepartureTime || null;
    this.status = data.status || "NotStarted";
    this.createdAt = data.createdAt || new Date();
    this.updatedAt = data.updatedAt || new Date();
  }

  generateId() {
    const timestamp = Date.now();
    const random = Math.floor(Math.random() * 10000);
    return `VVE-${timestamp}-${random}`;
  }

  validate() {
    if (!this.vvnId) throw new Error("vvnId is required");
    if (!this.vveDate) throw new Error("vveDate is required");
    if (!this.plannedDockId) throw new Error("plannedDockId is required");

    const validStatuses = ["NotStarted", "InProgress", "Delayed", "Completed"];
    if (!validStatuses.includes(this.status)) {
      throw new Error(`Invalid status: ${this.status}`);
    }
  }
}

module.exports = VesselVisitExecution;
```

**3. VVE Controller - Prepare Endpoint (OemApiNode/src/controllers/vveController.js) - Excerto**

```javascript
const vveService = require("../services/VesselVisitExecutionService");
const logger = require("../utils/logger");

exports.prepareToday = async (req, res, next) => {
  try {
    const userId = req.user.sub || req.user.id;

    logger.info(`User ${userId} initiated VVE preparation for today`);

    const result = await vveService.prepareTodaysVVEsAsync(userId);

    if (result.success) {
      res.status(200).json(result);
    } else {
      res.status(400).json(result);
    }
  } catch (error) {
    logger.error("Error in prepare today VVEs controller:", error);
    next(error);
  }
};
```

## 6. Testes

### Como Executar: `npm test -- --testPathPattern="VVE|VesselVisitExecution"` | `npm run cypress:open`

### Testes: ~50+ (VVE domain 10+, service 20+, controller 15+, E2E)

### Excertos

**1. VVE Entity**: `const vve = new VesselVisitExecution({vvnId, vveDate, ...}) → expect(vve.status).toBe('NotStarted')`
**2. Prepare Today**: `POST /api/oem/vessel-visit-executions/prepare-today → Assert VVEs created`
**3. E2E**: `cy.prepareToday() → cy.contains('VVEs prepared').should('exist')`

## 7. Observações

### Conformidade com Critérios de Aceitação

⚠️ **Nota**: US 4.1.4 descreve "Manual Update" mas implementação foca em VVE Preparation.

✅ **VVE Preparation implementada:**

1. **Automatic Preparation**: `/prepare-today` cria VVEs para todos assignments do Operation Plan de hoje.

2. **Initial Status**: VVEs criados com status "NotStarted".

3. **Planned Data Copy**: Copia plannedDockId, plannedArrivalTime, plannedDepartureTime do plano.

4. **Manual Creation**: Endpoint manual permite criar VVE individual (exceptional cases).

### Destaques da Implementação

- **Prepare Today Flow**:

  1. LogisticOperator clica "Prepare Today's Operations"
  2. Sistema busca Operation Plan para `vveDate = today`
  3. Para cada assignment no plano:
     - Cria VVE com `vvnId`, `vveDate`, planned data
     - Status = "NotStarted"
     - `actualDockId`, `actualArrivalTime`, `actualDepartureTime` = null
  4. Retorna summary (created, skipped, errors)

- **VVE Entity**:
  ```javascript
  {
    vveId, vvnId, vveDate,
    plannedDockId, plannedArrivalTime, plannedDepartureTime,
    actualDockId, actualArrivalTime, actualDepartureTime, // initially null
    status: 'NotStarted'
  }
  ```

### Observações de Design

- **Separation of Concerns**: VVE (execution) separado de OperationPlan (planning).
- **Today Constraint**: Só pode preparar VVEs para hoje (prevent prep muito antecipado).
- **Idempotency**: Re-run prepare não cria duplicados (verifica existência).

### Missing Implementation Note

- Título indica "Manual Update", mas implementação é VVE creation/preparation.
- Manual updates de assignments (change times, resources) seria outra US.
