/// <reference types="cypress" />

/**
 * E2E Tests for Authentication Flow
 * User Story: As a user, I want to authenticate using Google OAuth
 */

describe('Authentication Flow', () => {
  beforeEach(() => {
    cy.visit('/');
  });

  it('should redirect to login page when not authenticated', () => {
    cy.url().should('include', '/login');
  });

  it('should display login page with Google button container', () => {
    cy.visit('/login');
    cy.get('#google-signin-button').should('exist');
    cy.contains('Welcome to ProjArqsi').should('be.visible');
  });

  it('should show error page when accessing without authentication', () => {
    cy.visit('/admin', { failOnStatusCode: false });
    cy.url().should('include', '/login');
  });

  it('should allow Admin to login and access admin dashboard', () => {
    cy.loginWithGoogle('admin@example.com', 'Admin');
    cy.url().should('include', '/admin');
    cy.contains('Welcome, Admin!').should('be.visible');
  });

  it('should allow PortAuthorityOfficer to login and access port authority dashboard', () => {
    cy.loginWithGoogle('port@example.com', 'PortAuthorityOfficer');
    cy.url().should('include', '/port-authority');
    cy.contains('Welcome, Port Authority Officer!').should('be.visible');
  });

  it('should allow ShippingAgentRepresentative to login and access shipping agent dashboard', () => {
    cy.loginWithGoogle('shipping@example.com', 'ShippingAgentRepresentative');
    cy.url().should('include', '/shipping-agent');
    cy.contains('Shipping Agent Representative Dashboard').should('be.visible');
  });

  it('should allow LogisticOperator to login and access logistic operator dashboard', () => {
    cy.loginWithGoogle('logistic@example.com', 'LogisticOperator');
    cy.url().should('include', '/logistic-operator');
    cy.contains('Logistics Operator Dashboard').should('be.visible');
  });

  it('should handle logout correctly', () => {
    cy.loginWithGoogle('admin@example.com', 'Admin');
    cy.get('.logout-btn').click();
    cy.url().should('include', '/login');
    cy.window().then((win) => {
      expect(win.sessionStorage.getItem('auth_token')).to.be.null;
    });
  });

  it('should prevent access to protected routes after logout', () => {
    cy.loginWithGoogle('admin@example.com', 'Admin');
    cy.logout();
    cy.visit('/admin', { failOnStatusCode: false });
    cy.url().should('include', '/login');
  });

  it('should show unauthorized page for wrong role access', () => {
    cy.loginWithGoogle('shipping@example.com', 'ShippingAgentRepresentative');
    cy.visit('/admin', { failOnStatusCode: false });
    cy.url().should('match', /(unauthorized|access-denied)/);
  });
});
