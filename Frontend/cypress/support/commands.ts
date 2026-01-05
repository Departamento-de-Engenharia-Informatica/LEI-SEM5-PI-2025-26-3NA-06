/// <reference types="cypress" />

declare global {
  namespace Cypress {
    interface Chainable {
      /**
       * Custom command to login with Google (mocked)
       * @example cy.loginWithGoogle('admin@example.com', 'Admin')
       */
      loginWithGoogle(email: string, role: string): Chainable<void>;

      /**
       * Custom command to setup authentication token
       * @example cy.setupAuth('mock-token', { email: 'admin@example.com', role: 'Admin' })
       */
      setupAuth(token: string, user: any): Chainable<void>;

      /**
       * Custom command to logout
       * @example cy.logout()
       */
      logout(): Chainable<void>;

      /**
       * Custom command to get element by data-cy attribute
       * @example cy.getByCy('submit-button')
       */
      getByCy(value: string): Chainable<JQuery<HTMLElement>>;

      /**
       * Custom command to intercept API calls with fixtures
       * @example cy.mockApiCall('GET', '/api/Vessel', 'vessels')
       */
      mockApiCall(method: string, url: string, fixture: string): Chainable<void>;

      /**
       * Custom command to wait for page load
       * @example cy.waitForPageLoad()
       */
      waitForPageLoad(): Chainable<void>;

      /**
       * Custom command to fill form field
       * @example cy.fillField('vesselName', 'Test Vessel')
       */
      fillField(fieldName: string, value: string): Chainable<void>;
    }
  }
}

// Setup authentication with sessionStorage
Cypress.Commands.add('setupAuth', (token: string, user: any) => {
  cy.window().then((win) => {
    win.sessionStorage.setItem('auth_token', token);
    const expiryDate = new Date();
    expiryDate.setSeconds(expiryDate.getSeconds() + 3600); // 1 hour
    win.sessionStorage.setItem('auth_token_expiry', expiryDate.toISOString());
    win.sessionStorage.setItem('auth_user', JSON.stringify(user));
  });
});

// Login with Google (mocked for E2E tests)
Cypress.Commands.add('loginWithGoogle', (email: string, role: string) => {
  const mockUser = {
    userId: '123',
    email: email,
    name: email.split('@')[0],
    role: role,
  };

  const mockToken = `mock-token-${Date.now()}`;

  cy.visit('/login');

  // Setup auth before Google callback
  cy.setupAuth(mockToken, mockUser);

  // Navigate to appropriate dashboard
  if (role === 'Admin') {
    cy.visit('/admin');
  } else if (role === 'PortAuthority' || role === 'PortAuthorityOfficer') {
    cy.visit('/port-authority');
  } else if (role === 'ShippingAgent' || role === 'ShippingAgentRepresentative') {
    cy.visit('/shipping-agent');
  } else if (role === 'LogisticOperator') {
    cy.visit('/logistic-operator');
  }
});

// Logout
Cypress.Commands.add('logout', () => {
  cy.window().then((win) => {
    win.sessionStorage.clear();
  });
  cy.visit('/login');
});

// Get by data-cy attribute
Cypress.Commands.add('getByCy', (value: string) => {
  return cy.get(`[data-cy="${value}"]`);
});

// Mock API call
Cypress.Commands.add('mockApiCall', (method: string, url: string, fixture: string) => {
  cy.intercept(method, `**/api${url}`, { fixture: fixture }).as(fixture);
});

// Wait for page load
Cypress.Commands.add('waitForPageLoad', () => {
  cy.get('body').should('be.visible');
  cy.wait(500); // Wait for Angular to stabilize
});

// Fill form field
Cypress.Commands.add('fillField', (fieldName: string, value: string) => {
  cy.get(`[formControlName="${fieldName}"]`).clear().type(value);
});

export {};
