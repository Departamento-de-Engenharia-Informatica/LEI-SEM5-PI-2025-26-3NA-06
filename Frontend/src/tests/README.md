# Frontend Tests

Este diretório contém os testes para o frontend do projeto ProjArqsi.

## Estrutura

```
tests/
├── unit/           # Testes unitários com Jasmine/Karma
├── integration/    # Testes de integração
└── setup.ts        # Configuração do ambiente de testes
```

## Testes Unitários (Jasmine/Karma)

Os testes unitários testam componentes, serviços e pipes de forma isolada.

### Executar testes unitários:

```bash
npm test
```

### Executar com cobertura:

```bash
npm test -- --code-coverage
```

### Executar em modo headless (CI):

```bash
npm test -- --browsers=ChromeHeadlessCI --watch=false
```

## Testes E2E (Cypress)

Os testes E2E testam o fluxo completo da aplicação.

### Executar Cypress interativo:

```bash
npm run cypress:open
```

### Executar Cypress headless:

```bash
npm run cypress:run
```

## Convenções

- Arquivos de teste unitário: `*.spec.ts`
- Arquivos de teste E2E: `*.cy.ts`
- Use `data-cy` attributes para seletores Cypress
- Organize testes por feature/componente

## Exemplos

### Teste de Componente

```typescript
describe('MyComponent', () => {
  beforeEach(() => {
    TestBed.configureTestingModule({
      imports: [MyComponent],
    });
  });

  it('should create', () => {
    const fixture = TestBed.createComponent(MyComponent);
    expect(fixture.componentInstance).toBeTruthy();
  });
});
```

### Teste E2E

```typescript
describe('Feature flow', () => {
  it('should complete user journey', () => {
    cy.visit('/');
    cy.getByCy('login-button').click();
    cy.url().should('include', '/dashboard');
  });
});
```
