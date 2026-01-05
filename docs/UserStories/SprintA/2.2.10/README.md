# US 2.2.10 - View Vessel Visit Notification Status

## Descrição

As a Shipping Agent Representative, I want to view the status of all my submitted Vessel Visit Notifications (in progress, pending, approved with current dock assignment, or rejected with reason), so that I am always informed about the decisions of the Port Authority.

## Critérios de Aceitação

- The Shipping Agent Representative may also view the status of Vessel Visit Notifications submitted by other representatives working for the same shipping agent organization.
- Vessel Visit Notifications must be searchable and filterable by vessel, status, representative and time.

## 3. Análise

### 3.1. Domínio

_A desenvolver: Identificar as entidades, agregados e value objects do domínio relacionados com esta US._

### 3.2. Regras de Negócio

_A desenvolver: Documentar as regras de negócio específicas desta funcionalidade._

### 3.3. Casos de Uso

_A desenvolver: Descrever os principais casos de uso e seus fluxos._

## Perguntas do Fórum (Dev-Cliente)

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
