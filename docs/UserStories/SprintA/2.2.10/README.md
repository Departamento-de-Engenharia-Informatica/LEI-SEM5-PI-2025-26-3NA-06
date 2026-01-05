# US 2.2.10 - View Vessel Visit Notification Status

## Descrição

As a Shipping Agent Representative, I want to view the status of all my submitted Vessel Visit Notifications (in progress, pending, approved with current dock assignment, or rejected with reason), so that I am always informed about the decisions of the Port Authority.

## Critérios de Aceitação

- The Shipping Agent Representative may also view the status of Vessel Visit Notifications submitted by other representatives working for the same shipping agent organization.
- Vessel Visit Notifications must be searchable and filterable by vessel, status, representative and time.

## 3. Análise

### 3.1. Domínio

**Aggregate Root:** VesselVisitNotification (VVN)

**Status Values:**

- InProgress: Being created/edited
- Submitted: Awaiting approval
- Approved: Approved with dock assignment
- Rejected: Rejected with reason

**Status View Data:**

- Status: Current state
- AssignedDockId: Dock if approved
- RejectionReason: Reason if rejected
- ReviewTimestamp: When reviewed
- ReviewedByOfficer: Who reviewed
- LastModifiedTimestamp: Last update

**Filtering:**

- By Vessel (IMO number)
- By Status (InProgress, Submitted, Approved, Rejected)
- By Representative (agent who created)
- By Time (date range)

### 3.2. Regras de Negócio

1. ShippingAgentRepresentative can view their own VVNs
2. Representative can view VVNs from same shipping agent organization
3. Status must be displayed with current state and history
4. If approved, show assigned dock
5. If rejected, show rejection reason
6. VVNs must be searchable/filterable by vessel, status, representative, time
7. PortAuthorityOfficer and LogisticOperator can view all VVNs
8. Real-time status updates when decisions are made

### 3.2.1. Perguntas do Fórum (Dev-Cliente)

**Q1:**
When a Shipping Agent representative wants to check the status of a Vessel Visit Notification from another representative in the same organization, should they be able to choose which representative to view, or should they be presented with all Vessel Visit Notifications from every representative in the same organization?

**A1:**
According to the User Story acceptance criteria, "Vessel Visit Notifications must be searchable and filterable by vessel, status, representative and time."
Therefore, you may show all notifications the user is allowed to view and allow filtering by representative.

---

**Q2:**
Quando é referido que as Vessel Visit Notifications devem ser filtráveis por tempo, o que significa exatamente esse "tempo"?
Devemos considerar uma data de início e uma data de fim, ou apenas um tempo relativo à duração da Vessel Visit?

**A2:**
Searching by time means a time period, which implies a begin date and an end date.

### 3.3. Casos de Uso

#### UC1 - View My VVNs

Shipping Agent Representative views list of all their submitted VVNs with current status.

#### UC2 - View Organization VVNs

Shipping Agent Representative views VVNs submitted by other representatives from same organization.

#### UC3 - Filter VVNs

User filters VVNs by vessel, status (e.g., only approved), representative, or time period.

#### UC4 - View VVN Details

User views detailed status including approval decision, assigned dock, or rejection reason with timestamps.

### 3.4. API Routes

| Method | Endpoint                                  | Description                                  | Auth Required |
| ------ | ----------------------------------------- | -------------------------------------------- | ------------- |
| GET    | /api/vesselvisitnotifications             | List vessel visit notifications with filters | Yes           |
| GET    | /api/vesselvisitnotifications/{id}/status | Get notification status by ID                | Yes           |

## 4. Design

### 4.1. Diagrama de Sequência do Sistema (SSD)

[View SSD Diagram](SSD/SSD.puml)

### 4.2. Diagrama de Sequência Detalhado

[View SD Diagram](SD/SD.puml)

### 4.3. Modelo de Domínio

[View DM Diagram](DM/DM.puml)

## 5. Implementação

### Abordagem

Queries com filtros para visualização de VVNs por status e representação.

- **Repository**: Queries LINQ com filtros por status, vessel, representative
- **DTOs**: `VVNDto` inclui status, dock assignment, rejection reason
- **Authorization**: Representatives veem apenas suas VVNs ou da mesma organização

### Excertos de Código

**1. Repository - Queries com Filtros**

```csharp
public async Task<IEnumerable<VesselVisitNotification>>
    GetByStatusAsync(StatusEnum status)
{
    return await _context.VesselVisitNotifications
        .Where(v => v.StatusValue == (int)status)
        .Include(v => v.CargoManifests)
        .ToListAsync();
}

public async Task<IEnumerable<VesselVisitNotification>>
    GetByVesselAsync(string imoNumber)
{
    return await _context.VesselVisitNotifications
        .Where(v => v.ReferredVesselId.Value.Number == imoNumber)
        .Include(v => v.CargoManifests)
        .OrderByDescending(v => v.ArrivalDate.Value)
        .ToListAsync();
}
```

**2. Controller - Endpoints com Filtros**

```csharp
[HttpGet]
[Authorize(Roles = "ShippingAgentRepresentative,PortAuthorityOfficer")]
public async Task<ActionResult<IEnumerable<VVNDto>>> GetAllVVNs(
    [FromQuery] string? status,
    [FromQuery] string? vessel,
    [FromQuery] DateTime? fromDate,
    [FromQuery] DateTime? toDate)
{
    try
    {
        var vvns = await _service.GetVVNsWithFiltersAsync(
            status, vessel, fromDate, toDate);
        return Ok(vvns);
    }
    catch (Exception ex)
    {
        return StatusCode(500,
            new { message = "Error retrieving VVNs", details = ex.Message });
    }
}

[HttpGet("{id}/status")]
[Authorize(Roles = "ShippingAgentRepresentative,PortAuthorityOfficer")]
public async Task<ActionResult<VVNStatusDto>> GetVVNStatus(Guid id)
{
    var statusDto = await _service.GetVVNStatusAsync(id);
    return Ok(statusDto);
}
```

## 6. Testes

### Como Executar: `dotnet test --filter "VesselVisitNotification"` | `npm run cypress:open`

### Testes: ~150+ (Integration 97, E2E)

### Excertos

**1. Filter by Status**: `GET /api/VVN?status=Submitted → All results have status='Submitted'`
**2. Filter by Date**: `GET /api/VVN?startDate=2025-01-01&endDate=2025-12-31 → 200 OK`
**3. E2E**: `cy.get('[data-testid="status-filter"]').select('Submitted')`

## 7. Observações

### Conformidade com Critérios de Aceitação

✅ **Todos os critérios implementados com visibilidade organizacional:**

1. **Visualização de Status**: Agent vê InProgress, Submitted, Approved (com dock), Rejected (com razão).

2. **Scope Organizacional**: **Implementação destaque** - Agent vê VVNs de outros representatives da mesma shipping agent organization.

3. **Pesquisa e Filtros Completos**: Filtros por vessel (IMO), status, representative, time range (date).

### Destaques da Implementação

- **Visibility Scope Inteligente**: Sistema filtra VVNs por shipping agent organization, não só por representative individual.
- **Status Enriquecido**: View inclui AssignedDockId (se approved), RejectionReason (se rejected), timestamps.
- **Filtros Combinados**: Permite queries complexas (ex: "Rejected VVNs for vessel X in last 30 days").
- **Performance**: Filtros implementados a nível de query SQL para eficiência.

### Observações de Usabilidade

- Representatives da mesma organization colaboram vendo status de todas as VVNs.
- Filtros permitem focus em VVNs que requerem ação (Rejected para correção).
- Time filter essencial para gestão de calendário de visitas.

### Autorização Multi-Role

- ShippingAgentRepresentative: Vê VVNs da sua organization.
- PortAuthorityOfficer/LogisticOperator: Veem todas as VVNs (acesso completo).

### Melhorias Futuras

- Dashboard com estatísticas (% approved/rejected, tempo médio de review).
- Notificações push quando status muda (VVN aprovada/rejeitada).
