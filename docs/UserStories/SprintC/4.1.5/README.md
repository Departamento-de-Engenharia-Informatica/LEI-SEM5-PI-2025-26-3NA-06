# US 4.1.5 - Identify VVNs Without Operation Plans

## Descrição

As a Logistics Operator, I want to identify Vessel Visit Notifications (VVNs) that do not yet have an Operation Plan, so that missing plans can be easily detected and generated.

## Critérios de Aceitação

- The REST API must include an endpoint returning VVNs without an associated Operation Plan.
- The SPA must display these VVNs in a dedicated section or tab ("Missing Plans").
- If one or more VVNs of a given day are missing an Operation Plan, the operator must be able to trigger regeneration of all Operation Plans for that day using the selected scheduling algorithm.
- For regenerated plans, the system must record some metadata such as creation date, author, algorithm used.
- Operators must be warned that regeneration plans will overwrite any existing plans for that day as soon as they are confirmed.

## 3. Análise

### 3.1. Domínio

_A desenvolver: Identificar as entidades, agregados e value objects do domínio relacionados com esta US._

### 3.2. Regras de Negócio

_A desenvolver: Documentar as regras de negócio específicas desta funcionalidade._

### 3.3. Casos de Uso

_A desenvolver: Descrever os principais casos de uso e seus fluxos._
