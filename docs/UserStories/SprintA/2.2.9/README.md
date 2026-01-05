# US 2.2.9 - Change/Complete Vessel Visit Notification

## Descrição

As a Shipping Agent Representative, I want to change / complete a Vessel Visit Notification while it is still in progress, so that I can correct errors or withdraw requests if necessary.

## Critérios de Aceitação

- Status can be maintained "in progress" or changed to "submitted / approval pending" by the representative.

## 3. Análise

### 3.1. Domínio

_A desenvolver: Identificar as entidades, agregados e value objects do domínio relacionados com esta US._

### 3.2. Regras de Negócio

_A desenvolver: Documentar as regras de negócio específicas desta funcionalidade._

### 3.3. Casos de Uso

_A desenvolver: Descrever os principais casos de uso e seus fluxos._

## Perguntas do Fórum (Dev-Cliente)

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
