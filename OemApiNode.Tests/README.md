# OEM API Tests

Testes para o módulo OEM (Operations & Execution Management) desenvolvido em Node.js.

## Estrutura

```
OemApiNode.Tests/
├── 1.DomainTests/           # Testes de Domain Objects
│   ├── Incident/
│   ├── IncidentType/
│   ├── OperationPlan/
│   └── VesselVisitExecution/
├── 2.ServiceTests/          # Testes de Services
├── 3.ControllerTests/       # Testes de Controllers
└── 4.IntegrationTests/      # Testes de Integração E2E
```

## Executar Testes

```bash
# Instalar dependências
npm install

# Executar todos os testes
npm test

# Executar com watch mode
npm run test:watch

# Executar com coverage
npm run test:coverage
```

## Tecnologias

- **Jest**: Framework de testes
- **Node.js**: Runtime

## Cobertura

Os testes cobrem:

- Domain Objects (Incident, IncidentType, OperationPlan, VesselVisitExecution)
- Services (Business Logic)
- Controllers (API Endpoints)
- Integração E2E
