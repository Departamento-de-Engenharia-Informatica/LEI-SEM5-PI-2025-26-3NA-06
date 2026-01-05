# Cypress E2E Tests

Este diretório contém os testes end-to-end usando Cypress.

## Estrutura

```
cypress/
├── e2e/            # Testes E2E
├── fixtures/       # Dados de teste mockados
└── support/        # Comandos customizados e configuração
```

## Executar Testes

### Modo interativo (com UI):

```bash
npm run cypress:open
```

### Modo headless (CI):

```bash
npm run cypress:run
```

### Executar teste específico:

```bash
npx cypress run --spec "cypress/e2e/example.cy.ts"
```

## Comandos Customizados

Definidos em `cypress/support/commands.ts`:

- `cy.login(username, password)` - Fazer login
- `cy.getByCy(value)` - Selecionar elemento por data-cy attribute

## Boas Práticas

1. Use `data-cy` attributes para seletores estáveis
2. Use fixtures para dados de teste
3. Evite timeouts fixos - use `cy.wait()` com aliases
4. Organize testes por fluxo de usuário
5. Mantenha testes independentes entre si

## Exemplo de Teste

```typescript
describe('Operation Plan Management', () => {
  beforeEach(() => {
    cy.login('operator@example.com', 'password');
    cy.visit('/operation-plans');
  });

  it('should create new operation plan', () => {
    cy.getByCy('create-plan-button').click();
    cy.getByCy('plan-date-input').type('2026-01-15');
    cy.getByCy('algorithm-select').select('PRIORITY');
    cy.getByCy('submit-button').click();

    cy.getByCy('success-message').should('be.visible');
    cy.url().should('include', '/operation-plans/');
  });
});
```
