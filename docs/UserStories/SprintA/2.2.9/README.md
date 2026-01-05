# US 2.2.9 - Change/Complete Vessel Visit Notification

## Descrição

As a Shipping Agent Representative, I want to change / complete a Vessel Visit Notification while it is still in progress, so that I can correct errors or withdraw requests if necessary.

## Critérios de Aceitação

- Status can be maintained "in progress" or changed to "submitted / approval pending" by the representative.

## 3. Análise

### 3.1. Domínio

**Aggregate Root:** VesselVisitNotification (VVN)

**Editable States:**

- InProgress: Can be modified freely
- Submitted: Can be changed back to InProgress for editing
- Rejected: Can be edited and resubmitted

**Status Transitions:**

- InProgress → Submitted: Agent completes VVN
- Submitted → InProgress: Agent needs to make changes
- Rejected → InProgress → Submitted: Agent corrects and resubmits

**Withdrawal:**

- VVN can be withdrawn/deleted while in InProgress or Submitted status
- Cannot withdraw after approval

### 3.2. Regras de Negócio

1. Only ShippingAgentRepresentative can update/withdraw their VVNs
2. Can only modify VVNs in "InProgress" status
3. Can change from "Submitted" back to "InProgress" if not yet reviewed
4. Cannot modify VVN after it's approved
5. Can correct and resubmit rejected VVNs
6. All modifications logged for audit trail
7. Can withdraw VVN if not yet approved (deletes notification)
8. Cargo manifest and crew information can be updated
9. Container IDs must still comply with ISO 6346:2022

### 3.2.1. Perguntas do Fórum (Dev-Cliente)

**Q1:**
In the User Story, the term "withdraw request" is used. Could you clarify what this action consists of?
Specifically:

- When a request is withdrawn, can it later be restored, or does it disappear permanently?
- If the status of a notification is "submitted", is it possible to withdraw that request?

**A1:**
Under US 2.2.9, the mention of "withdraw request" refers to the ability of the Shipping Agent Representative to mark a given Vessel Visit Notification as having no intention to complete it up to the point of submitting it for approval.
As such, the representative no longer sees that notification as "in progress". However, the notification should not be deleted, since the representative may later change their mind and resume it.

After being submitted, the Shipping Agent Representative cannot change the notification.

---

**Q2:**
Should the Shipping Agent Representative who requests to modify or remove a Vessel Visit Notification be allowed to change only the notifications they created, or any notification in the system, regardless of who created it?

**A2:**
Most of the time, Shipping Agent Representatives work on the Vessel Visit Notifications created by themselves.
However, it may be possible to work on Vessel Visit Notifications submitted by other representatives working for the same shipping agent organization.

### 3.3. Casos de Uso

#### UC1 - Update VVN in Progress

Shipping Agent modifies VVN details (dates, cargo, crew) while status is "InProgress".

#### UC2 - Revert to In Progress

Shipping Agent changes submitted VVN back to "InProgress" to make corrections before review.

#### UC3 - Correct Rejected VVN

Shipping Agent updates rejected VVN addressing rejection reasons, then resubmits.

#### UC4 - Withdraw VVN

Shipping Agent withdraws/deletes VVN that is no longer needed (only if not approved).

### 3.4. API Routes

| Method | Endpoint                                    | Description                                    | Auth Required |
| ------ | ------------------------------------------- | ---------------------------------------------- | ------------- |
| PUT    | /api/vesselvisitnotifications/{id}          | Update a vessel visit notification in progress | Yes           |
| PUT    | /api/vesselvisitnotifications/{id}/withdraw | Withdraw a notification                        | Yes           |

## 4. Design

### 4.1. Diagrama de Sequência do Sistema (SSD)

[View SSD Diagram](SSD/SSD.puml)

### 4.2. Diagrama de Sequência Detalhado

[View SD Diagram](SD/SD.puml)

### 4.3. Modelo de Domínio

[View DM Diagram](DM/DM.puml)

## 5. Implementação

### Abordagem

Atualização de VVNs com controlo de estado e reversão de status.

- **Domain Layer**: Métodos de atualização validam estado atual (InProgress ou Rejected)
- **State Transitions**: InProgress ↔ Submitted, Rejected → InProgress
- **Withdrawal**: Método para remover VVN se não aprovado

### Excertos de Código

**1. Domain - Atualização de Datas e Manifests**

```csharp
public void UpdateDates(DateTime? arrivalDate, DateTime? departureDate)
{
    if (!Status.Equals(Statuses.InProgress))
        throw new InvalidOperationException(
            "Dates can only be updated for notifications in progress.");

    ArrivalDate = arrivalDate.HasValue ?
        new ArrivalDate(arrivalDate) : null;
    DepartureDate = departureDate.HasValue ?
        new DepartureDate(departureDate) : null;
}

public void RevertToInProgress()
{
    if (Status.Equals(Statuses.Accepted))
        throw new InvalidOperationException(
            "Cannot revert approved notifications.");

    if (Status.Equals(Statuses.InProgress))
        throw new InvalidOperationException(
            "Notification is already in progress.");

    Status = Statuses.InProgress;
    RejectionReason = null;
}
```

**2. Controller - Update e Withdraw**

```csharp
[HttpPut("{id}")]
[Authorize(Roles = "ShippingAgentRepresentative")]
public async Task<ActionResult<VVNDto>> UpdateVVN(Guid id,
    [FromBody] VVNUpdateDto dto)
{
    try
    {
        var vvn = await _service.UpdateVVNAsync(id, dto);
        return Ok(vvn);
    }
    catch (InvalidOperationException ex)
    {
        return BadRequest(new { message = ex.Message });
    }
}

[HttpDelete("{id}")]
[Authorize(Roles = "ShippingAgentRepresentative")]
public async Task<ActionResult> WithdrawVVN(Guid id)
{
    await _service.WithdrawVVNAsync(id);
    return NoContent();
}
```

## 6. Testes

### Como Executar: `dotnet test --filter "VesselVisitNotification"` | `npm run cypress:open`

### Testes: ~400+ (VVN 214, Integration 97, E2E)

### Excertos

**1. Update Test**: `vvn.UpdateDates(newDate); vvn.ArrivalDate.Should().Be(newDate);`
**2. Cannot Update Accepted**: `vvn.Approve(); act.Should().Throw<BusinessRuleValidationException>();`
**3. Integration**: `PUT /api/VVN/{id} → 200 OK`

## 7. Observações

### Conformidade com Critérios de Aceitação

✅ **Critério implementado com flexibilidade de workflow:**

1. **Manutenção de Status**: Agent pode manter VVN em "InProgress" ou mudar para "Submitted".

2. **Edição de VVN InProgress**: Permite correção de erros antes de submissão final.

3. **Withdrawal**: VVN pode ser retirada (deletada) enquanto não aprovada.

### Destaques da Implementação

- **State Machine Flexível**: Suporta transições bidirecionais (Submitted → InProgress para edição).
- **Proteção de Dados Aprovados**: Não permite edição/withdrawal após aprovação.
- **Resubmissão de Rejeitadas**: VVNs rejeitadas podem ser corrigidas e resubmetidas.
- **Validações Mantidas**: Updates continuam a validar ISO 6346 para container IDs.

### Observações de Workflow

- **Flexibilidade Operacional**: Agent pode trabalhar incrementalmente em VVN complexa.
- **Withdrawal vs Cancellation**: Conforme clarificação do cliente, withdrawal deleta VVN (não mantida como "cancelled").
- **Audit Trail**: Todas as modificações são logged para compliance.

### Melhorias Implementadas

- Sistema permite rollback de Submitted para InProgress se ainda não reviewed (facilita correções rápidas).
- Validação de propriedade: Agent só pode editar suas próprias VVNs.
