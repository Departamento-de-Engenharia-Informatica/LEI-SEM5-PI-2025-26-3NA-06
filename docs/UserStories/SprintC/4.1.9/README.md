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

_A desenvolver: Identificar as entidades, agregados e value objects do domínio relacionados com esta US._

### 3.2. Regras de Negócio

_A desenvolver: Documentar as regras de negócio específicas desta funcionalidade._

### 3.3. Casos de Uso

_A desenvolver: Descrever os principais casos de uso e seus fluxos._

## Perguntas do Fórum (Dev-Cliente)

**Q1:**
Na User Story 4.1.9 é dito, num critério de aceitação, o seguinte: "The corresponding planned operations must be marked as 'started,' 'completed,' or 'delayed.'".
Isto significa que cada operação no Operation Plan referente ao VVE tem um estado?

**A1:**
Sim. Qual é a dúvida?

---

**Q2:**
Na US 4.1.9 é dito: "The corresponding planned operations must be marked as 'started,' 'completed,' or 'delayed.'"
Mas este status não poderia fazer parte das executed operations, sendo que, assim que começam, seria registada a sua hora de início e teriam o estado "started"?

**A2:**
Sim, as operações executadas também podem ter um estado.
Por exemplo: "started", "completed", "suspended" (por exemplo, devido a um incidente em curso).
