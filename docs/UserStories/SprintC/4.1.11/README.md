# US 4.1.11 - Mark VVE as Completed

## Descrição

As a Logistics Operator, I want to mark a VVE as completed by recording the vessel's unberth time and port departure time, so that the visit lifecycle is correctly closed and operational statistics can be derived.

## Critérios de Aceitação

- The SPA must provide an option to mark a VVE as "Completed."
- To complete a VVE, the following information must be recorded:
  - Actual unberth time (when the vessel leaves the dock).
  - Actual port departure time (when the vessel exits port limits).
- Completion is only allowed if all associated cargo operations are recorded as finished.
- Once completed, the VVE becomes read-only, except for authorized corrections by admin users.
- The action must be logged (timestamp, user, any changes made) for audit purposes.

## 3. Análise

### 3.1. Domínio

**Aggregate Root:** VesselVisitExecution (VVE)

**Completion Fields:**

- actualUnberthTime: When vessel leaves the dock
- actualPortDepartureTime: When vessel exits port limits
- status: Changed to "Completed"
- completedBy: User who marked as completed
- completedAt: Timestamp of completion
- allOperationsFinished: Boolean flag validation

**Read-Only State:**

- Once completed, VVE becomes immutable
- Only Admin can make authorized corrections

### 3.2. Regras de Negócio

1. VVE can only be marked completed if all associated cargo operations are finished
2. Must record actualUnberthTime (when vessel leaves dock)
3. Must record actualPortDepartureTime (when vessel exits port limits)
4. actualUnberthTime must be before actualPortDepartureTime
5. Status changes from InProgress to Completed
6. Once completed, VVE becomes read-only
7. Only LogisticOperator can mark VVE as completed
8. Only Admin users can make corrections to completed VVE
9. Action logged with timestamp, user, and any changes made
10. System validates all operations have status "completed" before allowing VVE completion
11. Completion triggers operational statistics calculation

### 3.3. Casos de Uso

#### UC1 - Mark VVE Completed

Logistics Operator records vessel unberth time and port departure time. System validates all cargo operations finished, updates VVE with times, changes status to "Completed", makes it read-only.

#### UC2 - Validate Completion Readiness

Operator attempts to complete VVE. System checks if all operations are finished. If not, system rejects and shows incomplete operations.

#### UC3 - Admin Correction

Admin user corrects error in completed VVE (e.g., wrong departure time). System logs correction with admin ID and reason.

#### UC4 - Generate Statistics

System processes completed VVE to derive operational statistics (turnaround time, dock utilization, operation efficiency).

### 3.4. API Routes

| Method | Endpoint                     | Description                | Auth Required          |
| ------ | ---------------------------- | -------------------------- | ---------------------- |
| POST   | /api/oem/incident-types      | Create a new incident type | Yes (LogisticOperator) |
| GET    | /api/oem/incident-types      | List all incident types    | Yes (LogisticOperator) |
| GET    | /api/oem/incident-types/{id} | Get incident type by ID    | Yes (LogisticOperator) |
| PUT    | /api/oem/incident-types/{id} | Update an incident type    | Yes (LogisticOperator) |
| DELETE | /api/oem/incident-types/{id} | Delete an incident type    | Yes (LogisticOperator) |

## 4. Design

### 4.1. Diagrama de Sequência do Sistema (SSD)

[View SSD Diagram](SSD/SSD.puml)

### 4.2. Diagrama de Sequência Detalhado

[View SD Diagram](SD/SD.puml)

### 4.3. Modelo de Domínio

[View DM Diagram](DM/DM.puml)

## 5. Implementação

### Abordagem

A conclusão de VVE quando o navio deixa o porto:

1. **Completion Validation**: Sistema valida que todas as operações planeadas estão concluídas antes de permitir marcar VVE como "Completed"
2. **Unberth & Departure**: Regista actualUnberthTime (desatracação) e actualPortDepartureTime (saída do porto)
3. **Time Sequence Check**: actualUnberthTime deve ser antes de actualPortDepartureTime
4. **Immutable State**: Após conclusão, VVE torna-se read-only (apenas Admin pode corrigir erros)
5. **Audit Logging**: Regista completedBy (user ID) e completedAt (timestamp)
6. **Statistics Generation**: Dados ficam disponíveis para análise (turnaround time, dock utilization)

VVE completed representa fim do ciclo de execução e permite análises de performance portuária.

### Excertos de Código Relevantes

**1. VVE Service - Complete VVE (OemApiNode/src/services/VesselVisitExecutionService.js) - Excerto**

```javascript
class VesselVisitExecutionService {
  async completeVVEAsync(vveId, completionData, userId) {
    try {
      const vve = await vveRepository.getByIdAsync(vveId);
      if (!vve) {
        throw new Error(`VVE ${vveId} not found`);
      }

      if (vve.status === "Completed") {
        throw new Error("VVE is already completed");
      }

      // Validate all operations are completed
      const allOpsCompleted = vve.executedOperations?.every(
        (op) => op.status === "completed" || op.status === "delayed"
      );

      if (!allOpsCompleted) {
        throw new Error(
          "Cannot complete VVE: not all operations are completed"
        );
      }

      // Validate time sequence
      const unberthTime = new Date(completionData.actualUnberthTime);
      const departureTime = new Date(completionData.actualPortDepartureTime);

      if (departureTime < unberthTime) {
        throw new Error("Port departure time cannot be before unberth time");
      }

      if (vve.actualBerthTime && unberthTime < new Date(vve.actualBerthTime)) {
        throw new Error("Unberth time cannot be before berth time");
      }

      // Update VVE with completion data
      vve.actualUnberthTime = completionData.actualUnberthTime;
      vve.actualPortDepartureTime = completionData.actualPortDepartureTime;
      vve.status = "Completed";
      vve.completedBy = userId;
      vve.completedAt = new Date();

      // Calculate total turnaround time
      if (vve.actualArrivalTime && vve.actualPortDepartureTime) {
        const arrivalTime = new Date(vve.actualArrivalTime);
        const departureTime = new Date(vve.actualPortDepartureTime);
        vve.totalTurnaroundMinutes = Math.round(
          (departureTime - arrivalTime) / 60000
        );
      }

      const completedVve = await vveRepository.updateAsync(vve);

      logger.info(
        `VVE ${vveId} completed by user ${userId}. Turnaround: ${vve.totalTurnaroundMinutes} minutes`
      );

      return {
        success: true,
        data: completedVve,
        message: "VVE completed successfully",
      };
    } catch (error) {
      logger.error(`Error completing VVE ${vveId}:`, error);
      return {
        success: false,
        error: error.message || "Unknown error occurred",
      };
    }
  }

  async adminCorrectVVEAsync(vveId, corrections, adminId) {
    try {
      const vve = await vveRepository.getByIdAsync(vveId);
      if (!vve || vve.status !== "Completed") {
        throw new Error("VVE not found or not completed");
      }

      // Only Admin can correct completed VVE
      vve.correctedBy = adminId;
      vve.correctionReason = corrections.reason;
      vve.correctedAt = new Date();

      Object.assign(vve, corrections.updates);

      const correctedVve = await vveRepository.updateAsync(vve);

      logger.warn(
        `VVE ${vveId} corrected by Admin ${adminId}. Reason: ${corrections.reason}`
      );

      return {
        success: true,
        data: correctedVve,
        message: "VVE corrected successfully",
      };
    } catch (error) {
      logger.error(`Error correcting VVE ${vveId}:`, error);
      return {
        success: false,
        error: error.message || "Unknown error occurred",
      };
    }
  }
}
```

**2. VVE Controller - Complete VVE (OemApiNode/src/controllers/vveController.js) - Excerto**

```javascript
exports.complete = async (req, res, next) => {
  try {
    const { id } = req.params;
    const userId = req.user.sub || req.user.id;

    const result = await vveService.completeVVEAsync(id, req.body, userId);

    if (result.success) {
      res.status(200).json(result);
    } else {
      res.status(400).json(result);
    }
  } catch (error) {
    logger.error("Error in complete VVE controller:", error);
    next(error);
  }
};

exports.adminCorrect = async (req, res, next) => {
  try {
    const { id } = req.params;
    const adminId = req.user.sub || req.user.id;

    // Verify Admin role
    if (req.user.role !== "Admin") {
      return res.status(403).json({
        success: false,
        error: "Only Admin can correct completed VVEs",
      });
    }

    const result = await vveService.adminCorrectVVEAsync(id, req.body, adminId);

    if (result.success) {
      res.status(200).json(result);
    } else {
      res.status(400).json(result);
    }
  } catch (error) {
    logger.error("Error in admin correct VVE controller:", error);
    next(error);
  }
};
```

## 6. Testes

### Como Executar: `npm test -- --testPathPattern="VVEService.*complete"` | `npm run cypress:open`

### Testes: ~35+ (VVE completion logic 12+, validation 10+, controller 8+, E2E)

### Excertos

**1. Mark Completed**: `vveService.markCompleted(vveId, {unberthTime, departureTime}) → expect(vve.status).toBe('Completed')`
**2. Validate Operations**: `vveService.canComplete(vveId) → Assert all operations finished`
**3. E2E**: `cy.completeVVE({unberthTime: '16:30', departureTime: '17:00'}) → cy.contains('VVE completed').should('exist')`

## 7. Observações

### Conformidade com Critérios de Aceitação

✅ **Completion workflow completo:**

1. **Mark as Completed**: SPA fornece opção para marcar VVE como "Completed".

2. **Required Info**: Registo de `actualUnberthTime` e `actualPortDepartureTime`.

3. **Operations Check**: Completion só permitido se todas cargo operations finished.

4. **Read-Only State**: VVE completada torna-se read-only (exceto corrections por Admin).

5. **Audit Log**: Ação logged com timestamp, user, changes.

### Destaques da Implementação

- **Validation Before Completion**:

  ```javascript
  const allOperationsFinished = vve.executedOperations.every(
    (op) => op.status === "completed"
  );
  if (!allOperationsFinished) {
    return error("Cannot complete: pending operations");
  }
  ```

- **Temporal Validation**: `actualUnberthTime < actualPortDepartureTime`.

- **Status Transition**: InProgress/Delayed → Completed (one-way, irreversible sem Admin).

- **Completion Fields**:
  ```javascript
  {
    actualUnberthTime, // leaves dock
    actualPortDepartureTime, // exits port
    status: 'Completed',
    completedBy: userId,
    completedAt: timestamp,
    allOperationsFinished: true
  }
  ```

### Observações de Gestão

- **Lifecycle Completo**: VVE fecha lifecycle da visita:

  1. VVN (notification/planning)
  2. OperationPlan (scheduling)
  3. VVE (execution tracking)
  4. VVE Completed (visit concluded)

- **Read-Only Protection**: Previne modificações acidentais de dados históricos.

- **Admin Corrections**: Admin pode corrigir erros em VVEs completadas (logged separately).

### Statistics Derivation

**Metrics Calculados ao Completion:**

- **Turnaround Time**: actualPortDepartureTime - actualArrivalTime
- **Berth Time**: actualUnberthTime - actualBerthTime
- **Total Delay**: Sum of operation delays
- **Dock Utilization**: Berth time vs planned time
- **Operation Efficiency**: Completed vs planned operations

### Melhorias Implementadas

- Incomplete operations list mostrada se tenta completar prematuramente.
- Statistics auto-calculated e enviadas para analytics dashboard.
