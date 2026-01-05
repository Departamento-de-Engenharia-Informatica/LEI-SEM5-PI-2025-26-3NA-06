# US 4.1.7 - Create Vessel Visit Execution (VVE) Record

## Descrição

As a Logistics Operator, I want to create a Vessel Visit Execution (VVE) record when a vessel arrives at the port, so that the actual start of operations can be logged and monitored.

## Critérios de Aceitação

- The REST API must allow creating a new VVE referencing an existing VVN.
- Recorded fields must include vessel identifier, actual arrival time at the port, and creator user ID. An automatic VVE identifier must be also assigned, whose pattern is like VVN IDs.
- The SPA must easy the VVE creation using available VVN information.
- Once created, the VVE must be marked as "In Progress."

## 3. Análise

### 3.1. Domínio

**Aggregate Root:** VesselVisitExecution (VVE)

**Attributes:**

- vveId: Unique identifier (pattern like VVN IDs)
- vvnId: Reference to VesselVisitNotification
- vesselIMO: Vessel identifier
- vveDate: Date of execution
- plannedDockId: Planned dock from VVN
- plannedArrivalTime: Planned from VVN
- plannedDepartureTime: Planned from VVN
- actualDockId: Actual dock (initially null)
- actualArrivalTime: Actual arrival at port
- actualDepartureTime: Actual departure (initially null)
- status: InProgress (set on creation)
- createdBy: User who created the VVE
- createdAt: Timestamp of creation

**Repository:** VesselVisitExecutions table in OEM database

### 3.2. Regras de Negócio

1. VVE must reference an existing VVN
2. VVE creation requires: VVN ID, vessel identifier, actual arrival time at port, creator user ID
3. VVE ID automatically assigned following VVN pattern
4. Initial status set to "InProgress" upon creation
5. Planned times/dock copied from referenced VVN
6. Only LogisticOperator can create VVEs
7. actualArrivalTime required at creation
8. actualDockId and actualDepartureTime initially null (set later)
9. SPA can prepopulate fields using VVN information
10. One VVE per VVN (1:1 relationship)

### 3.2.1. Perguntas do Fórum (Dev-Cliente)

**Q1:**
Queríamos confirmar se os conceitos de VVE e Operation Plan estão interligados através do VVN.
Isto é, como na User Story 4.1.9 é dito que é possível atualizar VVEs com operações executadas — operações estas que vêm do Operation Plan — e, na User Story 4.1.7, para criar um VVE, este tem como referência um VVN, gostaríamos de perceber se o VVE conhece diretamente o Operation Plan devido ao que é descrito na US 4.1.9 ou se essa ligação é feita indiretamente através do VVN.

**A1:**
Parece-me que o que está em causa é mais uma questão técnica.
Contudo, uma VVE é sempre relativa a uma VVN.
O plano de operações dessa VVE é o que está estabelecido para a respetiva VVN (cf. sugerido na US 4.1.9).
Nota, no entanto, que a informação relativa à execução das operações pode ser distinta da planeada.
Por exemplo, a descarga do contentor X pode estar planeada para se iniciar às 13h52 e, na prática, apenas começar às 14h01.
Ou seja, neste exemplo, haveria um atraso de 9 minutos na realização dessa operação.

### 3.3. Casos de Uso

#### UC1 - Create VVE from VVN

Logistics Operator selects a VVN, records actual arrival time at port. System creates VVE with VVN reference, vessel ID, planned data from VVN, actual arrival time, and sets status to "InProgress".

#### UC2 - Auto-populate VVE Fields

SPA retrieves VVN data and pre-fills VVE creation form with vessel, planned dock, and planned times for easy data entry.

#### UC3 - Validate VVE Creation

System validates VVN exists, vessel matches, and actual arrival time is reasonable before creating VVE.

### 3.4. API Routes

| Method | Endpoint                                     | Description                                               | Auth Required          |
| ------ | -------------------------------------------- | --------------------------------------------------------- | ---------------------- |
| PATCH  | /api/oem/vessel-visit-executions/{id}/berth  | Update actual berth assignment (dock, arrival, departure) | Yes (LogisticOperator) |
| PATCH  | /api/oem/vessel-visit-executions/{id}/status | Update VVE status                                         | Yes (LogisticOperator) |

## 4. Design

### 4.1. Diagrama de Sequência do Sistema (SSD)

[View SSD Diagram](SSD/SSD.puml)

### 4.2. Diagrama de Sequência Detalhado

[View SD Diagram](SD/SD.puml)

### 4.3. Modelo de Domínio

[View DM Diagram](DM/DM.puml)

## 5. Implementação

### Abordagem

A criação de VVE quando o navio chega ao porto:

1. **Manual Creation**: Logistics Operator cria VVE manualmente ao registar a chegada real
2. **VVN Reference**: VVE referencia VVN existente para copiar dados planeados
3. **Actual Arrival Recording**: Sistema regista actualArrivalTime (hora real de chegada ao porto)
4. **Status InProgress**: VVE começa automaticamente com status "InProgress"
5. **ID Generation**: VVE ID segue padrão similar ao VVN (e.g., VVE-timestamp-random)
6. **Data Prepopulation**: Frontend pré-preenche campos com dados do VVN

VVE representa a execução real e permite tracking de desvios entre planeado e executado.

### Excertos de Código Relevantes

**1. VVE Service - Create Manual VVE (OemApiNode/src/services/VesselVisitExecutionService.js) - Excerto**

```javascript
class VesselVisitExecutionService {
  async createVVEAsync(dto, userId) {
    try {
      // Validate VVN exists in Backend
      const vvn = await backendApiClient.getVVNByIdAsync(dto.vvnId);
      if (!vvn) {
        throw new Error(`VVN ${dto.vvnId} not found`);
      }

      // Check if VVE already exists for this VVN
      const existingVve = await vveRepository.getByVvnIdAsync(dto.vvnId);
      if (existingVve) {
        throw new Error(`VVE already exists for VVN ${dto.vvnId}`);
      }

      // Create VVE with InProgress status
      const vve = new VesselVisitExecution({
        vvnId: dto.vvnId,
        operationPlanId: dto.operationPlanId,
        vveDate: dto.vveDate || new Date().toISOString().split("T")[0],
        plannedDockId: vvn.requestedDockId,
        plannedArrivalTime: vvn.plannedArrivalDate,
        plannedDepartureTime: vvn.plannedDepartureDate,
        actualArrivalTime: dto.actualArrivalTime, // Required at creation
        status: "InProgress", // Automatic status
        actualDockId: null, // Set later when vessel berths
        actualDepartureTime: null, // Set later when vessel departs
      });

      vve.validate();

      const savedVve = await vveRepository.createAsync(vve);

      logger.info(
        `VVE created: ${savedVve.id} for VVN ${dto.vvnId} by user ${userId}`
      );

      return {
        success: true,
        data: savedVve,
        message: "VVE created successfully",
      };
    } catch (error) {
      logger.error("Error creating VVE:", error);
      return {
        success: false,
        error: error.message || "Unknown error occurred",
      };
    }
  }
}
```

**2. VVE Controller - Create Endpoint (OemApiNode/src/controllers/vveController.js) - Excerto**

```javascript
exports.create = async (req, res, next) => {
  try {
    const userId = req.user.sub || req.user.id;

    logger.info(`User ${userId} creating VVE for VVN ${req.body.vvnId}`);

    const result = await vveService.createVVEAsync(req.body, userId);

    if (result.success) {
      res.status(201).json(result);
    } else {
      res.status(400).json(result);
    }
  } catch (error) {
    logger.error("Error in create VVE controller:", error);
    next(error);
  }
};
```

**3. Backend API Client - Get VVN (OemApiNode/src/services/BackendApiClient.js) - Excerto**

```javascript
class BackendApiClient {
  async getVVNByIdAsync(vvnId) {
    try {
      const response = await axios.get(
        `${BACKEND_API_URL}/api/vesselvisitnotification/${vvnId}`
      );
      return response.data;
    } catch (error) {
      logger.error(`Failed to fetch VVN ${vvnId} from Backend:`, error.message);
      return null;
    }
  }

  async enrichVVEWithVVNData(vve) {
    const vvn = await this.getVVNByIdAsync(vve.vvnId);
    if (vvn) {
      return {
        ...vve,
        vesselIMO: vvn.imonumber,
        vesselName: vvn.vesselName || "Unknown",
        plannedCargo: vvn.cargoManifestLoad?.length || 0,
      };
    }
    return vve;
  }
}
```

## 6. Testes

### Como Executar: `npm test -- --testPathPattern="VesselVisitExecution"` | `npm run cypress:open`

### Testes: ~50+ (VVE domain 12+, service 18+, controller 15+, E2E)

### Excertos

**1. VVE Creation**: `const vve = new VesselVisitExecution({vvnId, actualArrivalTime, ...}) → expect(vve.status).toBe('InProgress')`
**2. Create VVE Endpoint**: `POST /api/oem/vessel-visit-executions with vvnId → Assert 201 Created`
**3. E2E**: `cy.createVVE({vvnId: 'VVN123', actualArrival: '2025-05-01T08:00'}) → cy.contains('VVE created').should('exist')`

## 7. Observações

### Conformidade com Critérios de Aceitação

✅ **Criação de VVE completa:**

1. **VVN Reference**: VVE refere existing VVN (valida existência).

2. **Required Fields**: Vessel identifier, actual arrival time at port, creator user ID.

3. **Automatic VVE ID**: Pattern similar a VVN IDs (auto-generated).

4. **SPA Pre-population**: Frontend usa VVN data para prepopular form.

5. **Status "InProgress"**: VVE marcado como "InProgress" na criação.

### Destaques da Implementação

- **VVE Creation Flow**:

  1. Vessel arrives at port
  2. LogisticOperator seleciona VVN
  3. Sistema carrega VVN data (vessel, planned dock/times)
  4. Operator regista `actualArrivalTime`
  5. Sistema cria VVE:
     - Copia planned data de VVN
     - Define `actualArrivalTime` (provided)
     - Status = "InProgress"
     - `actualDockId`, `actualDepartureTime` = null (set later)

- **1:1 Relationship**: Uma VVE por VVN (validação previne duplicação).

### Observações do Cliente

- **VVE ligada a VVN e Operation Plan**: Conforme clarificação, VVE sempre relativa a VVN; operações vem do Operation Plan dessa VVN.
- **Execution vs Planning**: VVE regista realidade (actual times) que pode diferir do planeado.

### Exemplo de Diferenças Planned vs Actual

- **Planned**: Vessel berth às 08:00, container X unload 13:52
- **Actual**: Vessel berth às 08:15 (atraso 15min), container X unload 14:01 (atraso 9min)
- **VVE regista**: actualBerthTime = 08:15, operation delays

### Validações Implementadas

- VVN must exist e estar approved.
- `actualArrivalTime` reasonable (not far future, not too far past).
- LogisticOperator role required.
