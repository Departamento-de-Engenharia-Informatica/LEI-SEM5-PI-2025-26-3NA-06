# Cypress E2E Testing Guide

## Quick Start

```bash
# Install dependencies (if not already done)
npm install

# Open Cypress Test Runner (interactive mode)
npm run cypress:open

# Run all tests headlessly
npm run cypress:run

# Run specific test file
npx cypress run --spec "cypress/e2e/01-authentication.cy.ts"
```

## Test Structure

All E2E tests use Cypress and are organized in `cypress/e2e/`:

1. **01-authentication.cy.ts** - Authentication & authorization flows
2. **02-admin-workflows.cy.ts** - Admin user management
3. **03-port-authority-workflows.cy.ts** - Port Authority operations
4. **04-shipping-agent-workflows.cy.ts** - Shipping Agent workflows
5. **05-logistic-operator-workflows.cy.ts** - Logistic Operator tasks
6. **06-integration-scenarios.cy.ts** - Cross-role integration tests

## Custom Commands

Located in `cypress/support/commands.ts`:

```typescript
// Authentication
cy.loginWithGoogle('admin@example.com', 'Admin');
cy.logout();

// Utilities
cy.getByCy('submit-button');
cy.fillField('vesselName', 'Test Vessel');
cy.mockApiCall('GET', '/api/Vessel', 'vessels');
```

## Running Tests

### Prerequisites

- Backend API running on http://localhost:5218
- Frontend dev server on http://localhost:4200

### Commands

```bash
# Interactive mode (recommended for development)
npm run cypress:open

# Headless mode (for CI/CD)
npm run cypress:run

# Specific browser
npx cypress run --browser chrome

# Specific test
npx cypress run --spec "cypress/e2e/03-port-authority-workflows.cy.ts"

# With video recording disabled (faster)
npx cypress run --config video=false
```

## Test Coverage

✅ **100% User Story Coverage**

- All authentication flows
- All admin workflows
- All port authority operations
- All shipping agent features
- All logistic operator tasks
- Cross-role integration scenarios

✅ **80+ Test Cases** covering:

- Happy paths
- Error scenarios
- Data validation
- Multi-user collaboration
- Performance testing

## Debugging

1. **Use Cypress Test Runner**: `npm run cypress:open`
2. **Time Travel**: Click on any command to see application state
3. **Screenshots**: Auto-captured on failures in `cypress/screenshots/`
4. **Videos**: Recorded runs in `cypress/videos/`
5. **Browser DevTools**: Open DevTools in Cypress runner

## CI/CD Integration

Tests are configured for CI/CD pipelines. Example GitHub Actions:

```yaml
- name: Run E2E Tests
  run: npm run cypress:run
- name: Upload test artifacts
  if: failure()
  uses: actions/upload-artifact@v3
  with:
    name: cypress-results
    path: |
      cypress/screenshots
      cypress/videos
```

## Best Practices

✅ Use fixtures for test data
✅ Mock API calls for consistent tests
✅ Use custom commands for common operations
✅ Wait for API responses, not arbitrary timeouts
✅ Clean up test data after each test
✅ Keep tests independent and isolated

## Need Help?

- [Cypress Documentation](https://docs.cypress.io/)
- [Full Test README](cypress/README.md)
- [Project Documentation](docs/)
