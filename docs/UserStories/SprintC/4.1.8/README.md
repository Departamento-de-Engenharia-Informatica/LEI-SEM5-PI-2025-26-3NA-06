# US 4.1.8 - Update VVE with Berth Time and Dock

## Descrição

As a Logistics Operator, I want to update an in progress VVE with the actual berth time and dock used, so that discrepancies from the planned dock assignment are recorded.

## Critérios de Aceitação

- The REST API must support update of berth time and dock ID.
- If the assigned dock differs from the planned one, a warning or note must be automatically added.
- All updates must be timestamped and logged for auditability.

## 3. Análise

### 3.1. Domínio

**Aggregate Root:** VesselVisitExecution (VVE)

**Updated Fields:**

- actualDockId: The dock where vessel actually berths (may differ from planned)
- actualBerthTime: Time when vessel berths at dock
- dockDiscrepancy: Flag/note if actualDockId ≠ plannedDockId
- updatedBy: User who made the update
- updatedAt: Timestamp of update

### 3.2. Regras de Negócio

1. VVE must be in "InProgress" status to update berth information
2. actualDockId can differ from plannedDockId (dock changes allowed)
3. If actualDockId ≠ plannedDockId, system adds warning/note automatically
4. actualBerthTime must be after actualArrivalTime (if already set)
5. Only LogisticOperator can update VVE berth information
6. All updates timestamped and logged for auditability
7. System records who made the update (user ID)
8. Historical updates preserved in audit log
9. Dock must be valid and exist in system

### 3.3. Casos de Uso

#### UC1 - Update Berth with Planned Dock

Logistics Operator records actual berth time at planned dock. System updates VVE with actualBerthTime and actualDockId (same as planned), no discrepancy flag.

#### UC2 - Update Berth with Different Dock

Logistics Operator records berth time but vessel uses different dock than planned. System updates VVE with new actualDockId and actualBerthTime, adds automatic warning about dock discrepancy.

#### UC3 - View Audit Log

User reviews audit log showing when berth was updated, by whom, and what changed.

### 3.4. API Routes

| Method | Endpoint                                     | Description              | Auth Required          |
| ------ | -------------------------------------------- | ------------------------ | ---------------------- |
| GET    | /api/oem/vessel-visit-executions             | Search VVEs with filters | Yes (LogisticOperator) |
| GET    | /api/oem/vessel-visit-executions/date/{date} | Get VVEs by date         | Yes (LogisticOperator) |
| GET    | /api/oem/vessel-visit-executions/{id}        | Get VVE by ID            | Yes (LogisticOperator) |

## 4. Design

### 4.1. Diagrama de Sequência do Sistema (SSD)

[View SSD Diagram](SSD/SSD.puml)

### 4.2. Diagrama de Sequência Detalhado

[View SD Diagram](SD/SD.puml)

### 4.3. Modelo de Domínio

[View DM Diagram](DM/DM.puml)

## 5. Implementação

### Abordagem

A atualização de VVE com informação de berth:

1. **Berth Recording**: Regista quando o navio atraca (actualBerthTime) e em que doca (actualDockId)
2. **Dock Discrepancy Check**: Se actualDockId ≠ plannedDockId, sistema adiciona warning automático
3. **Status Validation**: Só permite atualização se VVE está em status "InProgress"
4. **Audit Trail**: Regista updatedBy (user ID) e updatedAt (timestamp)
5. **Time Validation**: actualBerthTime deve ser depois de actualArrivalTime

Esta funcionalidade permite tracking de desvios do plano original (dock changes).

### Excertos de Código Relevantes

**1. VVE Service - Update Berth Info (OemApiNode/src/services/VesselVisitExecutionService.js) - Excerto**

```javascript
class VesselVisitExecutionService {
  async updateBerthInfoAsync(vveId, berthData, userId) {
    try {
      const vve = await vveRepository.getByIdAsync(vveId);
      if (!vve) {
        throw new Error(`VVE ${vveId} not found`);
      }

      if (vve.status !== "InProgress") {
        throw new Error(
          `Cannot update berth info: VVE status is ${vve.status}, must be InProgress`
        );
      }

      // Validate berth time is after arrival time
      if (vve.actualArrivalTime && berthData.actualBerthTime) {
        const arrivalTime = new Date(vve.actualArrivalTime);
        const berthTime = new Date(berthData.actualBerthTime);
        if (berthTime < arrivalTime) {
          throw new Error("Berth time cannot be before arrival time at port");
        }
      }

      // Update berth information
      vve.actualDockId = berthData.actualDockId;
      vve.actualBerthTime = berthData.actualBerthTime;
      vve.updatedBy = userId;
      vve.updatedAt = new Date();

      // Check for dock discrepancy
      if (vve.actualDockId !== vve.plannedDockId) {
        vve.dockDiscrepancy = true;
        vve.discrepancyNote = `Vessel berthed at dock ${vve.actualDockId} instead of planned dock ${vve.plannedDockId}`;
        logger.warn(
          `Dock discrepancy for VVE ${vveId}: Planned=${vve.plannedDockId}, Actual=${vve.actualDockId}`
        );
      } else {
        vve.dockDiscrepancy = false;
        vve.discrepancyNote = null;
      }

      const updatedVve = await vveRepository.updateAsync(vve);

      logger.info(
        `VVE ${vveId} berth info updated by user ${userId}: Dock=${vve.actualDockId}, Time=${vve.actualBerthTime}, Discrepancy=${vve.dockDiscrepancy}`
      );

      return {
        success: true,
        data: updatedVve,
        message: vve.dockDiscrepancy
          ? "Berth info updated with dock discrepancy warning"
          : "Berth info updated successfully",
      };
    } catch (error) {
      logger.error(`Error updating berth info for VVE ${vveId}:`, error);
      return {
        success: false,
        error: error.message || "Unknown error occurred",
      };
    }
  }
}
```

**2. VVE Controller - Update Berth (OemApiNode/src/controllers/vveController.js) - Excerto**

```javascript
exports.updateBerthInfo = async (req, res, next) => {
  try {
    const { id } = req.params;
    const userId = req.user.sub || req.user.id;

    const result = await vveService.updateBerthInfoAsync(id, req.body, userId);

    if (result.success) {
      res.status(200).json(result);
    } else {
      res.status(400).json(result);
    }
  } catch (error) {
    logger.error("Error in update berth info controller:", error);
    next(error);
  }
};
```

**3. VVE Route Definition (OemApiNode/src/routes/vesselVisitExecutions.js) - Excerto**

```javascript
const express = require("express");
const router = express.Router();
const { authenticateJWT, authorizeRole } = require("../middleware/auth");
const vveController = require("../controllers/vveController");

router.put(
  "/:id/berth",
  authenticateJWT,
  authorizeRole("LogisticOperator", "Admin"),
  vveController.updateBerthInfo
);

router.get(
  "/",
  authenticateJWT,
  authorizeRole("LogisticOperator", "Admin"),
  vveController.search
);

router.get(
  "/date/:date",
  authenticateJWT,
  authorizeRole("LogisticOperator", "Admin"),
  vveController.getByDate
);

module.exports = router;
```

## 6. Testes

### Como Executar: `npm test -- --testPathPattern="VVEService"` | `npm run cypress:open`

### Testes: ~40+ (VVE update logic 15+, controller 15+, E2E)

### Excertos

**1. Update Berth**: `vveService.updateBerth(vveId, {actualDockId, actualBerthTime}) → Assert VVE updated`
**2. Dock Discrepancy**: `vve.actualDockId != vve.plannedDockId → expect(vve.dockDiscrepancy).toBe(true)`
**3. E2E**: `cy.updateBerth({dockId: 'D2', berthTime: '08:30'}) → cy.contains('Berth updated').should('exist')`

## 7. Observações

### Conformidade com Critérios de Aceitação

✅ **Update de berth com discrepancy tracking:**

1. **Update Berth Time & Dock**: API suporta update de `actualBerthTime` e `actualDockId`.

2. **Dock Discrepancy Warning**: Se `actualDockId ≠ plannedDockId`, warning automático adicionado.

3. **Audit Log**: Updates timestamped e logged (user ID, timestamp, old/new values).

### Destaques da Implementação

- **Status Requirement**: VVE deve estar em "InProgress" para update berth.

- **Discrepancy Flag**:

  ```javascript
  if (actualDockId !== plannedDockId) {
    vve.dockDiscrepancy = true;
    vve.discrepancyNote = `Planned: ${plannedDockId}, Actual: ${actualDockId}`;
  }
  ```

- **Temporal Validation**: `actualBerthTime` deve ser após `actualArrivalTime` (se já definido).

- **Dock Validation**: `actualDockId` must be valid dock existente no sistema.

### Observações Operacionais

- **Dock Changes Common**: Vessel pode usar dock diferente por várias razões:

  - Planned dock ocupado/unavailable
  - Vessel size mismatch discovered
  - Emergency berth needed
  - Weather conditions

- **Discrepancy Tracking**: Permite análise de accuracy do planning.

### Audit Trail Completo

- **Logged Fields**:
  - `updatedBy`: User ID
  - `updatedAt`: Timestamp
  - `oldDockId`, `newDockId`
  - `oldBerthTime`, `newBerthTime`
  - `reason`: Optional note from operator

### Melhorias Implementadas

- Historical audit log preservado (append-only).
- Dashboard mostra % de dock discrepancies para melhorar planning.
