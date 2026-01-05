# US 4.1.3 - Search and List Operation Plans

## Descrição

As a Logistics Operator, I want to search and list Operation Plans for a given day or period, so that I can quickly review all scheduled activities within that timeframe.

## Critérios de Aceitação

- The REST API must support querying Operation Plans by date range and/or vessel identifier.
- The SPA must provide a searchable and filterable table showing plan summaries (e.g., vessel, dock, start/end time, assigned resources).
- Results must be sortable (e.g., by start time, vessel name, or expected delay).

## 3. Análise

### 3.1. Domínio

**Aggregate Root:** OperationPlan (same as US 4.1.2)

**Query Filters:**

- startDate: Filter plans from this date onwards
- endDate: Filter plans up to this date
- vesselIMO: Filter plans containing a specific vessel

### 3.2. Regras de Negócio

1. Users can search operation plans by date range
2. Users can filter plans by vessel IMO number
3. Results include plan metadata (date, algorithm, author, feasibility)
4. Results include list of assignments (VVNs) in each plan
5. Query by specific date returns the operation plan for that exact date
6. Only LogisticOperator role can access operation plans

### 3.3. Casos de Uso

#### UC1 - Search Plans by Date Range

Logistics Operator specifies start and end dates. System returns all operation plans within that range.

#### UC2 - Get Plan by Specific Date

Logistics Operator requests operation plan for a specific date. System returns the plan if it exists.

#### UC3 - Filter Plans by Vessel

Logistics Operator searches for plans containing a specific vessel (by IMO). System returns all plans where that vessel has assignments.

### 3.4. API Routes

| Method | Endpoint                             | Description                         | Auth Required          |
| ------ | ------------------------------------ | ----------------------------------- | ---------------------- |
| GET    | /api/oem/operation-plans             | Search operation plans with filters | Yes (LogisticOperator) |
| GET    | /api/oem/operation-plans/date/{date} | Get operation plan by date          | Yes (LogisticOperator) |

## 4. Design

### 4.1. Diagrama de Sequência do Sistema (SSD)

[View SSD Diagram](SSD/SSD.puml)

### 4.2. Diagrama de Sequência Detalhado

[View SD Diagram](SD/SD.puml)

### 4.3. Modelo de Domínio

[View DM Diagram](DM/DM.puml)

## 5. Implementação

### Abordagem

A pesquisa e listagem de Operation Plans foi implementada com filtros flexíveis:

1. **Query Filters**: Por intervalo de datas (startDate, endDate) e/ou vessel IMO
2. **Pagination**: Suporte para skip/take para resultados grandes
3. **Enrichment**: Enriquece dados com informação de navio e doca do Backend
4. **Specific Date Lookup**: Endpoint dedicado para buscar plano de uma data específica
5. **Sortable Results**: Frontend pode ordenar por data, navio, doca

O serviço retorna metadata do plano (feasibility, warnings, algorithm) e lista de assignments.

### Excertos de Código Relevantes

**1. Operation Plan Service - Search (OemApiNode/src/services/OperationPlanService.js) - Excerto**

```javascript
class OperationPlanService {
  async searchOperationPlansAsync(filters) {
    try {
      const { startDate, endDate, vesselIMO, skip, take } = filters;

      logger.info(
        `Searching operation plans with filters: startDate=${startDate}, endDate=${endDate}, vesselIMO=${vesselIMO}, skip=${skip}, take=${take}`
      );

      const plans = await operationPlanRepository.searchAsync(
        startDate,
        endDate,
        vesselIMO,
        skip,
        take
      );

      // Enrich each plan's assignments with vessel and dock info from Backend
      for (const plan of plans) {
        plan.assignments = await backendApiClient.enrichAssignmentsAsync(
          plan.assignments
        );
      }

      const responseDtos = plans.map((p) =>
        OperationPlanMapper.toResponseDto(p)
      );

      return {
        success: true,
        data: responseDtos,
        count: responseDtos.length,
      };
    } catch (error) {
      logger.error("Error searching operation plans:", error);
      return {
        success: false,
        error: error.message || "Unknown error occurred",
      };
    }
  }

  async getOperationPlanByDateAsync(date) {
    try {
      const plan = await operationPlanRepository.getByDateAsync(date);

      if (!plan) {
        return {
          success: false,
          error: `No operation plan found for date ${date}`,
        };
      }

      // Enrich assignments
      plan.assignments = await backendApiClient.enrichAssignmentsAsync(
        plan.assignments
      );

      const responseDto = OperationPlanMapper.toResponseDto(plan);

      return {
        success: true,
        data: responseDto,
      };
    } catch (error) {
      logger.error(`Error fetching operation plan for date ${date}:`, error);
      return {
        success: false,
        error: error.message || "Unknown error occurred",
      };
    }
  }
}
```

**2. Operation Plan Controller - Search Endpoint (OemApiNode/src/controllers/operationPlanController.js) - Excerto**

```javascript
exports.search = async (req, res, next) => {
  try {
    const { startDate, endDate, vesselIMO, skip, take } = req.query;

    const filters = {
      startDate,
      endDate,
      vesselIMO,
      skip: skip ? parseInt(skip) : 0,
      take: take ? parseInt(take) : 50,
    };

    const result = await operationPlanService.searchOperationPlansAsync(
      filters
    );

    if (result.success) {
      res.status(200).json(result);
    } else {
      res.status(400).json(result);
    }
  } catch (error) {
    logger.error("Error in search operation plans controller:", error);
    next(error);
  }
};

exports.getByDate = async (req, res, next) => {
  try {
    const { date } = req.params;

    const result = await operationPlanService.getOperationPlanByDateAsync(date);

    if (result.success) {
      res.status(200).json(result);
    } else {
      res.status(404).json(result);
    }
  } catch (error) {
    logger.error("Error in get operation plan by date controller:", error);
    next(error);
  }
};
```

**3. Operation Plan Repository - Search Query (OemApiNode/src/infrastructure/OperationPlanRepository.js) - Excerto**

```javascript
const sql = require("mssql");
const database = require("../config/database");
const logger = require("../utils/logger");

class OperationPlanRepository {
  async searchAsync(startDate, endDate, vesselIMO, skip = 0, take = 50) {
    const pool = await database.getPool();

    let query = `
      SELECT * FROM OperationPlans
      WHERE 1=1
    `;

    const params = [];

    if (startDate) {
      query += ` AND planDate >= @startDate`;
      params.push({ name: "startDate", type: sql.Date, value: startDate });
    }

    if (endDate) {
      query += ` AND planDate <= @endDate`;
      params.push({ name: "endDate", type: sql.Date, value: endDate });
    }

    if (vesselIMO) {
      query += ` AND assignments LIKE @vesselIMO`;
      params.push({
        name: "vesselIMO",
        type: sql.VarChar,
        value: `%${vesselIMO}%`,
      });
    }

    query += ` ORDER BY planDate DESC OFFSET @skip ROWS FETCH NEXT @take ROWS ONLY`;
    params.push({ name: "skip", type: sql.Int, value: skip });
    params.push({ name: "take", type: sql.Int, value: take });

    const request = pool.request();
    params.forEach((p) => request.input(p.name, p.type, p.value));

    const result = await request.query(query);

    return result.recordset.map((row) => this.mapRowToEntity(row));
  }

  async getByDateAsync(date) {
    const pool = await database.getPool();
    const result = await pool
      .request()
      .input("planDate", sql.Date, date)
      .query("SELECT * FROM OperationPlans WHERE planDate = @planDate");

    if (result.recordset.length === 0) return null;

    return this.mapRowToEntity(result.recordset[0]);
  }
}

module.exports = new OperationPlanRepository();
```

## 6. Testes

### Como Executar: `npm test -- --testPathPattern="OperationPlanService"` | `npm run cypress:open`

### Testes: ~30+ (Service search logic 15+, Controller filters 10+, E2E)

### Excertos

**1. Search by Date**: `GET /api/oem/operation-plans?startDate=2025-05-01&endDate=2025-05-07 → Assert array returned`
**2. Filter by Vessel**: `operationPlanService.searchByVessel('IMO1234567') → Assert plans contain vessel`
**3. E2E**: `cy.searchPlans({startDate: '2025-05-01'}) → cy.get('.plan-row').should('have.length.greaterThan', 0)`

## 7. Observações

### Conformidade com Critérios de Aceitação

✅ **Pesquisa completa implementada:**

1. **Query by Date Range/Vessel**: API suporta filtros por startDate, endDate, vesselIMO.

2. **SPA Table**: Tabela searchable/filterable com plan summaries (vessel, dock, times, resources).

3. **Sortable Results**: Ordenação por start time, vessel name, expected delay.

### Destaques da Implementação

- **Flexible Query Filters**:

  ```javascript
  GET /api/oem/operation-plans?
    startDate=2025-05-01&
    endDate=2025-05-31&
    vesselIMO=IMO1234567
  ```

- **Specific Date Lookup**: Endpoint dedicado `/api/oem/operation-plans/date/{date}` para busca exata.

- **Data Enrichment**: Resultados enriquecidos com vessel e dock info do Backend API:
  ```javascript
  {
    planDate, algorithm, isFeasible,
    assignments: [{
      vvnId, vesselName, vesselIMO, // enriched
      dockName, dockLocation, // enriched
      arrivalTime, departureTime
    }]
  }
  ```

### Observações de Performance

- **SQL Filtering**: Filtros aplicados a nível de query SQL (não post-processing).
- **Pagination**: Suporte para skip/take para datasets grandes.
- **Indexes**: Indexes em `planDate` e campos de filtro para performance.

### UX Features

- **Combined Filters**: Permite queries complexas ("Plans com Vessel X entre Date A e B").
- **Quick Access**: Busca por data específica para plano de hoje.
- **Summary View**: Lista compacta com details on-click.

### Melhorias Implementadas

- Sorting por múltiplos campos (date, vessel name, feasibility).
- Highlight de planos com conflicts (`isFeasible = false` em vermelho).
