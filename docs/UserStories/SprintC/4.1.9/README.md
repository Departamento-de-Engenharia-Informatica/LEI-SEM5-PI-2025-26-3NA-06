# US 4.1.9 - Update VVE with Executed Operations

## Descrição

As a Logistics Operator, I want to update an in progress VVE with executed operations, so that the system reflects real execution progress and performance.

## Critérios de Aceitação

- Executed operations (mainly) derive from planned operations, which may be used to easy recording execution of operations.
- The SPA must allow the operator to confirm or modify start/end times and resource usage.
- The corresponding planned operations must be marked as "started," "completed," or "delayed."
- Execution updates must be stored with timestamps and operator ID.
- Completion status must synchronize with the corresponding Operation Plan for comparison.

## 3. Análise

### 3.1. Domínio

**Aggregate Root:** VesselVisitExecution (VVE)

**Entities:**

- ExecutedOperation: Individual operation execution record
  - operationId: Reference to planned operation
  - actualStartTime: When operation actually started
  - actualEndTime: When operation finished
  - status: started, completed, delayed, suspended
  - resourcesUsed: Actual resources used (cranes, personnel, etc.)
  - operatorId: Who recorded the execution
  - updatedAt: Timestamp of update

**Planned Operation (from Operation Plan):**

- plannedStartTime: Scheduled start
- plannedEndTime: Scheduled end
- plannedResources: Resources allocated

### 3.2. Regras de Negócio

1. Executed operations derive from planned operations in Operation Plan
2. Operator can confirm or modify start/end times and resource usage
3. Status transitions: planned → started → completed/delayed/suspended
4. actualStartTime and actualEndTime can differ from planned times
5. Execution updates stored with timestamps and operator ID
6. Completion status synchronizes with Operation Plan for comparison
7. Only LogisticOperator can update VVE with executed operations
8. SPA can prepopulate fields from planned operations for easy recording
9. Delayed status set if actual times significantly differ from planned
10. Suspended status for operations paused due to incidents

### 3.2.1. Perguntas do Fórum (Dev-Cliente)

**Q1:**
Na User Story 4.1.9 é dito, num critério de aceitação, o seguinte: "The corresponding planned operations must be marked as 'started,' 'completed,' or 'delayed.'".
Isto significa que cada operação no Operation Plan referente ao VVE tem um estado?

**A1:**
Sim. Qual é a dúvida?

### 3.3. Casos de Uso

#### UC1 - Record Operation Start

Logistics Operator marks planned operation as started, records actual start time. System updates operation status to "started".

#### UC2 - Record Operation Completion

Logistics Operator confirms operation completed, records actual end time and resources used. System marks operation as "completed" and calculates duration.

#### UC3 - Modify Execution Times

Logistics Operator adjusts start/end times if recorded incorrectly. System updates and logs change.

#### UC4 - Mark Operation Delayed

Logistics Operator marks operation as delayed if it's running behind schedule. System flags for attention.

#### UC5 - Compare Planned vs Actual

System displays planned vs actual times/resources for performance analysis.

### 3.4. API Routes

| Method | Endpoint                                             | Description                   | Auth Required          |
| ------ | ---------------------------------------------------- | ----------------------------- | ---------------------- |
| GET    | /api/oem/operation-plans/vvn/{vvnId}/cargo-manifests | Get cargo manifests for a VVN | Yes (LogisticOperator) |

## 4. Design

### 4.1. Diagrama de Sequência do Sistema (SSD)

[View SSD Diagram](SSD/SSD.puml)

### 4.2. Diagrama de Sequência Detalhado

[View SD Diagram](SD/SD.puml)

### 4.3. Modelo de Domínio

[View DM Diagram](DM/DM.puml)

## 5. Implementação

### Abordagem

A atualização de VVE com operações executadas:

1. **Executed Operations Tracking**: Cada operação planeada tem registo de execução (actualStartTime, actualEndTime, status)
2. **Status Progression**: planned → started → completed/delayed/suspended
3. **Prepopulation**: Frontend pré-popula com operações planeadas para facilitar registo
4. **Discrepancy Tracking**: Compara actualTimes vs plannedTimes para identificar delays
5. **Resource Recording**: Regista recursos realmente usados (cranes, personnel)
6. **Performance Analysis**: Sistema calcula métricas (duration variance, efficiency)

Permite tracking detalhado da execução real vs planeamento.

### Excertos de Código Relevantes

**1. VVE Service - Update Executed Operations (OemApiNode/src/services/VesselVisitExecutionService.js) - Excerto**

```javascript
class VesselVisitExecutionService {
  async updateExecutedOperationsAsync(vveId, operationsData, userId) {
    try {
      const vve = await vveRepository.getByIdAsync(vveId);
      if (!vve) {
        throw new Error(`VVE ${vveId} not found`);
      }

      if (vve.status === "Completed") {
        throw new Error("Cannot update operations: VVE is already completed");
      }

      // Get operation plan to compare planned vs actual
      const operationPlan = await operationPlanRepository.getByIdAsync(
        vve.operationPlanId
      );

      const updatedOperations = [];

      for (const opData of operationsData) {
        const plannedOp = operationPlan?.assignments.find(
          (a) => a.operationId === opData.operationId
        );

        const executedOp = {
          operationId: opData.operationId,
          actualStartTime: opData.actualStartTime,
          actualEndTime: opData.actualEndTime,
          status: opData.status, // started, completed, delayed, suspended
          resourcesUsed: opData.resourcesUsed,
          operatorId: userId,
          updatedAt: new Date(),
        };

        // Calculate delay if completed
        if (executedOp.status === "completed" && plannedOp) {
          const plannedDuration =
            new Date(plannedOp.endTime) - new Date(plannedOp.startTime);
          const actualDuration =
            new Date(executedOp.actualEndTime) -
            new Date(executedOp.actualStartTime);
          const delayMinutes = (actualDuration - plannedDuration) / 60000;

          if (delayMinutes > 5) {
            // More than 5 minutes delay
            executedOp.status = "delayed";
            executedOp.delayMinutes = Math.round(delayMinutes);
            logger.warn(
              `Operation ${opData.operationId} completed with ${executedOp.delayMinutes} min delay`
            );
          }
        }

        updatedOperations.push(executedOp);
      }

      vve.executedOperations = updatedOperations;
      vve.updatedBy = userId;
      vve.updatedAt = new Date();

      const updatedVve = await vveRepository.updateAsync(vve);

      logger.info(
        `VVE ${vveId} updated with ${updatedOperations.length} executed operations by user ${userId}`
      );

      return {
        success: true,
        data: updatedVve,
        message: "Executed operations updated successfully",
      };
    } catch (error) {
      logger.error(
        `Error updating executed operations for VVE ${vveId}:`,
        error
      );
      return {
        success: false,
        error: error.message || "Unknown error occurred",
      };
    }
  }
}
```

**2. VVE Controller - Update Operations (OemApiNode/src/controllers/vveController.js) - Excerto**

```javascript
exports.updateOperations = async (req, res, next) => {
  try {
    const { id } = req.params;
    const userId = req.user.sub || req.user.id;

    const result = await vveService.updateExecutedOperationsAsync(
      id,
      req.body.operations,
      userId
    );

    if (result.success) {
      res.status(200).json(result);
    } else {
      res.status(400).json(result);
    }
  } catch (error) {
    logger.error("Error in update operations controller:", error);
    next(error);
  }
};
```

**3. Executed Operation DTO (OemApiNode/src/dtos/ExecutedOperationDto.js)**

```javascript
class ExecutedOperationDto {
  constructor(data) {
    this.operationId = data.operationId;
    this.actualStartTime = data.actualStartTime;
    this.actualEndTime = data.actualEndTime;
    this.status = data.status; // started, completed, delayed, suspended
    this.resourcesUsed = data.resourcesUsed || {};
    this.operatorId = data.operatorId;
    this.delayMinutes = data.delayMinutes || 0;
    this.updatedAt = data.updatedAt || new Date();
  }

  validate() {
    if (!this.operationId) throw new Error("operationId is required");
    if (!this.actualStartTime) throw new Error("actualStartTime is required");

    const validStatuses = ["started", "completed", "delayed", "suspended"];
    if (!validStatuses.includes(this.status)) {
      throw new Error(`Invalid status: ${this.status}`);
    }

    if (this.status === "completed" && !this.actualEndTime) {
      throw new Error("actualEndTime is required for completed operations");
    }

    if (this.actualEndTime && this.actualStartTime) {
      const start = new Date(this.actualStartTime);
      const end = new Date(this.actualEndTime);
      if (end < start) {
        throw new Error("End time cannot be before start time");
      }
    }
  }
}

module.exports = ExecutedOperationDto;
```

## 6. Testes

### Como Executar: `npm test -- --testPathPattern="ExecutedOperation"` | `npm run cypress:open`

### Testes: ~45+ (ExecutedOperation domain 12+, service 18+, controller 10+, E2E)

### Excertos

**1. Mark Operation Started**: `executedOp.markStarted(actualStartTime) → expect(executedOp.status).toBe('started')`
**2. Complete Operation**: `executedOp.markCompleted(actualEndTime) → expect(executedOp.status).toBe('completed')`
**3. E2E**: `cy.updateOperation({status: 'completed', endTime: '10:30'}) → cy.contains('Operation completed').should('exist')`

## 7. Observações

### Conformidade com Critérios de Aceitação

✅ **Tracking de operações executadas:**

1. **Derive from Planned**: Executed operations derivam de planned operations (easy recording).

2. **Confirm/Modify Times**: SPA permite confirmar ou modificar start/end times e resources.

3. **Status Marking**: Operações marcadas como "started", "completed", "delayed".

4. **Timestamped Updates**: Execution updates armazenados com timestamps e operator ID.

5. **Comparison**: Status sincroniza com Operation Plan para comparação planned vs actual.

### Destaques da Implementação

- **ExecutedOperation Entity**:

  ```javascript
  {
    operationId, // reference to planned operation
    actualStartTime, actualEndTime,
    status: 'started|completed|delayed|suspended',
    resourcesUsed, // actual resources (may differ from planned)
    operatorId, updatedAt
  }
  ```

- **Status Transitions**:

  - planned → started (operator marca início)
  - started → completed (operação termina)
  - started → delayed (running behind schedule)
  - started → suspended (paused por incident)

- **SPA Pre-population**: Form pre-filled com planned times/resources para quick confirmation.

### Observações do Cliente

- **Operações têm Estado**: Conforme resposta cliente, cada operação no Operation Plan tem estado (planned → started → completed).

### Planned vs Actual Tracking

**Comparação Automática:**

```javascript
{
  operation: "Unload Container X",
  plannedStart: "13:52", actualStart: "14:01", // 9min delay
  plannedEnd: "14:10", actualEnd: "14:22", // 12min delay
  plannedResources: "Crane 1", actualResources: "Crane 2", // resource change
  status: "completed", delay: "9 minutes"
}
```

### Performance Analysis

- **Metrics Derivados**:
  - Average delay per operation type
  - Resource utilization (planned vs actual)
  - Operation duration (actual vs planned)
  - Completion rate

### Melhorias Implementadas

- Dashboard real-time mostrando operações in-progress.
- Alerts para operations significantly delayed.
- Resource conflict detection (dois operations usando same resource).
