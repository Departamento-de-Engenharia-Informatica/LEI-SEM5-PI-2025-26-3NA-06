/// <reference types="cypress" />

/**
 * E2E Tests for Admin Workflows
 * User Story: As an Admin, I want to manage users and system settings
 */

describe('Admin User Management', () => {
  beforeEach(() => {
    cy.loginWithGoogle('admin@example.com', 'Admin');
    cy.intercept('GET', '**/api/User/inactive-users', { fixture: 'users.json' }).as('getUsers');
  });

  it('should display admin dashboard with navigation options', () => {
    cy.url().should('include', '/admin');
    cy.contains('Welcome, Admin!').should('be.visible');
    cy.contains('User Management').should('be.visible');
  });

  it('should navigate to user management page', () => {
    cy.contains('User Management').click();
    cy.url().should('include', '/admin/user-management');
  });

  it('should display list of inactive users', () => {
    cy.visit('/admin/user-management');
    cy.wait('@getUsers');
    cy.get('table').should('be.visible');
    cy.get('tbody tr').should('have.length.greaterThan', 0);
  });

  it('should toggle filter to show all or inactive users', () => {
    cy.visit('/admin/user-management');
    cy.wait('@getUsers');
    cy.contains('Show All Users').should('be.visible');
  });

  it('should assign role and activate a pending user', () => {
    cy.visit('/admin/user-management');
    cy.wait('@getUsers');

    cy.intercept('PUT', '**/api/User/*/assign-role', {
      statusCode: 200,
      body: { message: 'Role assigned successfully' },
    }).as('assignRole');

    cy.contains('Assign Role & Send Email').first().click();
    cy.wait('@assignRole');
  });

  it('should toggle user activation status', () => {
    cy.visit('/admin/user-management');
    cy.wait('@getUsers');

    cy.intercept('PUT', '**/api/User/*/toggle-active', {
      statusCode: 200,
      body: { message: 'User status updated' },
    }).as('toggleUser');

    cy.contains('Activate').first().click();
    cy.wait('@toggleUser');
  });

  it('should toggle between showing all users and inactive only', () => {
    cy.visit('/admin/user-management');
    cy.wait('@getUsers');

    // Mock the all-users endpoint
    cy.intercept('GET', '**/api/User/all-users', { fixture: 'users.json' }).as('getAllUsers');

    // Initially shows inactive users, button says "Show All Users"
    cy.contains('Show All Users').should('be.visible');
    cy.contains('Show All Users').click();
    cy.wait('@getAllUsers');

    // After clicking, button should change to "Show Inactive Users Only"
    cy.contains('Show Inactive Users Only').should('be.visible');
  });

  it('should handle error when loading users fails', () => {
    cy.intercept('GET', '**/api/User/inactive-users', {
      statusCode: 500,
      body: { message: 'Internal Server Error' },
    }).as('getUsersError');

    cy.visit('/admin/user-management');
    cy.wait('@getUsersError');
    // The app shows an alert, check for no users message
    cy.contains('No inactive users found').should('be.visible');
  });
});
