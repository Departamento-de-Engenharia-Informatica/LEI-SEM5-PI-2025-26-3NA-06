# Mapeamento entre Vistas Lógica, Implementação e Física (Nível 2)

Este documento estabelece a rastreabilidade entre os conceitos lógicos do sistema, os artefactos de implementação e a infraestrutura física de deployment.

---

## 1. VVN (Vessel Visit Notice)

| Vista             | Elementos                                                                                    |
| ----------------- | -------------------------------------------------------------------------------------------- |
| **Lógica**        | Agregado VVN - notificação de visita de navio ao porto                                       |
| **Implementação** | `VVNController.cs`, `VVNService.cs`, `VVN.cs` (aggregate), `VVNRepository.cs`                |
| **Física**        | **Core API** (Application Server, porta 5218) + **ProjArqsiDB** (Database Server, Azure SQL) |

---

## 2. Planos de Operação (Operation Plans)

| Vista             | Elementos                                                                                                                     |
| ----------------- | ----------------------------------------------------------------------------------------------------------------------------- |
| **Lógica**        | Planos de operação - gestão e execução de operações portuárias                                                                |
| **Implementação** | `OperationPlanController.js`, `OperationPlanService.js`, `OperationPlan.js` (modelo de domínio), `OperationPlanRepository.js` |
| **Física**        | **OEM API** (Node.js Server, porta 5004) + **projArqsiDBOEM** (Database Server, Azure SQL)                                    |

---

## 3. VVE (Vessel Visit Execution)

| Vista             | Elementos                                                                                  |
| ----------------- | ------------------------------------------------------------------------------------------ |
| **Lógica**        | Agregado VVE - execução real da visita do navio e registo de operações                     |
| **Implementação** | `VVEController.js`, `VVEService.js`, `VVE.js` (domain model), `VVERepository.js`           |
| **Física**        | **OEM API** (Node.js Server, porta 5004) + **projArqsiDBOEM** (Database Server, Azure SQL) |

---

## 4. Navios (Vessels)

| Vista             | Elementos                                                                                    |
| ----------------- | -------------------------------------------------------------------------------------------- |
| **Lógica**        | Agregado Vessel - gestão de navios, capacidades e características                            |
| **Implementação** | `VesselController.cs`, `VesselService.cs`, `Vessel.cs` (aggregate), `VesselRepository.cs`    |
| **Física**        | **Core API** (Application Server, porta 5218) + **ProjArqsiDB** (Database Server, Azure SQL) |

---

## 5. Autenticação e Autorização

| Vista             | Elementos                                                                        |
| ----------------- | -------------------------------------------------------------------------------- |
| **Lógica**        | Autenticação via Google OIDC + gestão de tokens JWT                              |
| **Implementação** | `AuthController.cs`, `JwtTokenGenerator.cs`, `GoogleAuthService.cs`              |
| **Física**        | **Auth API** (Application Server, porta 5001) + **Google IAM** (External System) |

---

## 6. Escalonamento (Scheduling)

| Vista             | Elementos                                                                         |
| ----------------- | --------------------------------------------------------------------------------- |
| **Lógica**        | Algoritmos de escalonamento para otimização de docas e recursos                   |
| **Implementação** | `SchedulingController.cs`, `SchedulingService.cs`, algoritmos de planeamento      |
| **Física**        | **Scheduling API** (Application Server, porta 5002) - stateless, sem persistência |

---

## 7. Interface do Utilizador

| Vista             | Elementos                                                                                                                                                          |
| ----------------- | ------------------------------------------------------------------------------------------------------------------------------------------------------------------ |
| **Lógica**        | Dashboards específicos por role (Admin, Port Officer, Shipping Agent, Logistics Operator)                                                                          |
| **Implementação** | Angular Components (`admin-dashboard.component.ts`, `vvn-list.component.ts`, etc.), HTTP Services (`vvn.service.ts`, `operation.service.ts`), Guards, Interceptors |
| **Física**        | **SPA** (Client Device - Browser, porta 4200)                                                                                                                      |

---

## Relações entre Containers

```
SPA (4200)
  ├─→ Auth API (5001) - Autenticação
  ├─→ Core API (5218) - VVN, Vessels, Docks, etc.
  ├─→ Scheduling API (5002) - Otimização de recursos
  └─→ OEM API (5004) - Operation Plans, VVE, Incidents

Core API (5218)
  ├─→ Auth API (5001) - Validação JWT
  └─→ ProjArqsiDB - Persistência domínio core

Scheduling API (5002)
  ├─→ Auth API (5001) - Validação JWT
  └─→ Core API (5218) - Dados de VVN e recursos

OEM API (5004)
  ├─→ Auth API (5001) - Validação JWT
  ├─→ Core API (5218) - Consulta VVN e Vessels
  └─→ projArqsiDBOEM - Persistência operações
```

---

## Notas de Deployment

- **Ambiente Atual**: Todos os servidores em localhost (desenvolvimento)
- **Ambiente Produção (Ideal)**:
  - Application Servers em Azure App Service
  - Node.js Server em Azure Container Instances ou App Service
  - Database Servers em Azure SQL Database (geo-replicação)
  - SPA servida via Azure Static Web Apps ou CDN
  - Google IAM para autenticação OAuth 2.0

---

## Separação de Bases de Dados

A arquitetura utiliza duas bases de dados separadas seguindo o princípio de **Bounded Context**:

1. **ProjArqsiDB**: Domínio core - VVN, Vessels, Docks, StorageAreas, Containers, VesselTypes
2. **projArqsiDBOEM**: Domínio de execução - Operation Plans, VVE, Incidents, Assignments

Esta separação permite:

- Escalabilidade independente
- Manutenção e deploy separados
- Isolamento de falhas
- Optimização específica por contexto
