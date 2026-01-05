# Frontend Testing Setup

## Instalação

### 1. Instalar dependências para Jasmine/Karma (testes unitários)

```bash
npm install --save-dev jasmine-core karma karma-jasmine karma-chrome-launcher karma-jasmine-html-reporter karma-coverage @angular-devkit/build-angular
```

### 2. Instalar Cypress (testes E2E)

```bash
npm install --save-dev cypress @cypress/webpack-dev-server
```

### 3. Instalar tipos TypeScript para Cypress

```bash
npm install --save-dev @types/jasmine
```

## Estrutura de Testes

```
Frontend/
├── src/
│   └── tests/
│       ├── unit/              # Testes unitários Jasmine
│       ├── integration/       # Testes de integração
│       ├── setup.ts          # Setup do ambiente de testes
│       └── README.md         # Documentação
├── cypress/
│   ├── e2e/                  # Testes E2E
│   ├── fixtures/             # Dados mock
│   ├── support/              # Comandos customizados
│   └── README.md             # Documentação
├── karma.conf.js             # Configuração Karma
├── cypress.config.ts         # Configuração Cypress
└── run-tests.ps1             # Script para executar testes
```

## Executar Testes

### Usando o script PowerShell (Recomendado)

```powershell
# Testes unitários
.\run-tests.ps1 -Unit

# Testes unitários com cobertura
.\run-tests.ps1 -Unit -Coverage

# Testes unitários headless (CI)
.\run-tests.ps1 -Unit -Headless

# Testes E2E (modo interativo)
.\run-tests.ps1 -E2E

# Testes E2E headless (CI)
.\run-tests.ps1 -E2E -Headless

# Todos os testes
.\run-tests.ps1 -Unit -E2E -Headless
```

### Usando npm scripts

```bash
# Testes unitários
npm test

# Testes unitários com cobertura
npm run test:coverage

# Testes unitários headless
npm run test:headless

# Cypress interativo
npm run cypress:open

# Cypress headless
npm run cypress:run
```

## Configuração Angular para testes

O arquivo `angular.json` já está configurado com:

```json
"test": {
  "builder": "@angular/build:unit-test"
}
```

## Convenções de Nomenclatura

- **Testes unitários**: `*.spec.ts` (exemplo: `my-component.spec.ts`)
- **Testes E2E**: `*.cy.ts` (exemplo: `login-flow.cy.ts`)
- **Seletores Cypress**: Use `data-cy` attributes nos elementos HTML

## Exemplo: Teste Unitário

```typescript
// my-component.spec.ts
import { TestBed } from '@angular/core/testing';
import { MyComponent } from './my-component';

describe('MyComponent', () => {
  beforeEach(() => {
    TestBed.configureTestingModule({
      imports: [MyComponent],
    });
  });

  it('should create', () => {
    const fixture = TestBed.createComponent(MyComponent);
    const component = fixture.componentInstance;
    expect(component).toBeTruthy();
  });
});
```

## Exemplo: Teste E2E

```typescript
// login-flow.cy.ts
describe('Login Flow', () => {
  beforeEach(() => {
    cy.visit('/');
  });

  it('should login successfully', () => {
    cy.getByCy('login-button').click();
    cy.get('input[name="username"]').type('user@example.com');
    cy.get('input[name="password"]').type('password');
    cy.getByCy('submit-button').click();
    cy.url().should('include', '/dashboard');
  });
});
```

## Comandos Customizados Cypress

Definidos em `cypress/support/commands.ts`:

```typescript
// Login
cy.login('username', 'password');

// Selecionar por data-cy
cy.getByCy('element-id');
```

## Coverage Report

Após executar testes com cobertura:

```bash
npm run test:coverage
```

O relatório será gerado em: `./coverage/proj-arqsi-frontend/index.html`

## CI/CD

Para integração contínua, use os comandos headless:

```yaml
# Exemplo GitHub Actions
- name: Run unit tests
  run: npm run test:headless

- name: Run E2E tests
  run: npm run cypress:run
```

## Troubleshooting

### Karma não encontra o Chrome

```bash
# Use ChromeHeadless
npm run test:headless
```

### Cypress não conecta ao servidor

```bash
# Certifique-se que a aplicação está rodando
npm start
# Em outro terminal:
npm run cypress:open
```

### Erros de timeout nos testes

```typescript
// Aumentar timeout específico
cy.get('.slow-element', { timeout: 10000 });

// Ou globalmente em cypress.config.ts
defaultCommandTimeout: 10000;
```
