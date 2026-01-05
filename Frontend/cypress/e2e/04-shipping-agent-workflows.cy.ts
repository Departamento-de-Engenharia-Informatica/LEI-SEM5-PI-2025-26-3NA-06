/// <reference types="cypress" />

/**
 * E2E Tests for Shipping Agent Workflows
 * User Stories:
 * - As a Shipping Agent, I want to create vessel visit notifications
 * - As a Shipping Agent, I want to manage my VVN drafts and submissions
 * - As a Shipping Agent, I want to manage containers
 */

describe('Shipping Agent - VVN Creation', () => {
  beforeEach(() => {
    cy.loginWithGoogle('shipping@example.com', 'ShippingAgentRepresentative');
    cy.intercept('POST', '**/api/audit/unauthorized-access', { statusCode: 200 });
    cy.intercept('GET', '**/api/Vessel', { fixture: 'vessels.json' }).as('getVessels');
    cy.intercept('GET', '**/api/Dock', { fixture: 'docks.json' }).as('getDocks');
    cy.intercept('GET', '**/api/Container', { fixture: 'containers.json' }).as('getContainers');
    cy.intercept('GET', '**/api/StorageArea', { fixture: 'storage-areas.json' }).as(
      'getStorageAreas'
    );
  });

  it('should display shipping agent dashboard', () => {
    cy.url().should('include', '/shipping-agent');
    cy.contains('Shipping Agent Representative Dashboard').should('be.visible');
  });

  it('should navigate to create VVN page', () => {
    cy.contains('Create VVN').click();
    cy.url().should('include', '/shipping-agent/create-vvn');
  });

  it('should create a new VVN as draft', () => {
    cy.visit('/shipping-agent/create-vvn');
    cy.wait('@getVessels');

    cy.intercept('POST', '**/api/VesselVisitNotification/draft', {
      statusCode: 201,
      body: { id: '123', status: 'Draft' },
    }).as('createVVN');

    // Fill VVN form
    cy.get('select[name="referredVesselId"]').select(1); // Select first vessel (index 0 is placeholder)

    // Set arrival and departure dates
    cy.get('input[name="arrivalDate"]').type('2026-02-01T10:00');
    cy.get('input[name="departureDate"]').type('2026-02-03T14:00');

    // Save as draft
    cy.contains('Save as Draft').click();
    cy.wait('@createVVN');
    cy.contains('saved').should('be.visible');
  });

  it('should create and submit a VVN', () => {
    cy.visit('/shipping-agent/create-vvn');
    cy.wait('@getVessels');

    cy.intercept('POST', '**/api/VesselVisitNotification/submit', {
      statusCode: 201,
      body: { id: '123', status: 'Submitted' },
    }).as('submitVVN');

    // Fill VVN form
    cy.get('select[name="referredVesselId"]').select(1); // Select first vessel (index 0 is placeholder)
    cy.get('input[name="arrivalDate"]').type('2026-02-01T10:00');
    cy.get('input[name="departureDate"]').type('2026-02-03T14:00');

    // Submit directly
    cy.contains('Submit for Review').click();
    cy.wait('@submitVVN');
    cy.contains('submitted').should('be.visible');
  });

  it('should show validation error for empty vessel', () => {
    cy.visit('/shipping-agent/create-vvn');
    cy.wait('@getVessels');

    // Try to submit without selecting a vessel
    cy.contains('Submit for Review').click();
    cy.contains('Vessel is required').should('be.visible');
  });
});

describe('Shipping Agent - VVN Drafts Management', () => {
  beforeEach(() => {
    cy.loginWithGoogle('shipping@example.com', 'ShippingAgentRepresentative');
    cy.intercept('POST', '**/api/audit/unauthorized-access', { statusCode: 200 });
    cy.intercept('GET', '**/api/Vessel', { fixture: 'vessels.json' }).as('getVessels');
    cy.intercept('GET', '**/api/VesselVisitNotification/drafts', {
      fixture: 'vvn-drafts.json',
    }).as('getDrafts');
  });

  it('should display list of draft VVNs', () => {
    cy.visit('/shipping-agent/vvn-drafts');
    cy.wait('@getDrafts');
    cy.get('.draft-card').should('have.length.greaterThan', 0);
  });

  it('should delete a draft VVN', () => {
    cy.visit('/shipping-agent/vvn-drafts');
    cy.wait('@getDrafts');

    cy.intercept('DELETE', '**/api/VesselVisitNotification/drafts/*', {
      statusCode: 200,
      body: { message: 'VVN deleted successfully' },
    }).as('deleteVVN');

    // Confirm the browser's confirm dialog
    cy.on('window:confirm', () => true);

    cy.get('button').contains('Delete').first().click();
    cy.wait('@deleteVVN');
  });
});

describe('Shipping Agent - Submitted VVNs', () => {
  beforeEach(() => {
    cy.loginWithGoogle('shipping@example.com', 'ShippingAgentRepresentative');
    cy.intercept('POST', '**/api/audit/unauthorized-access', { statusCode: 200 });
    cy.intercept('GET', '**/api/Vessel', { fixture: 'vessels.json' }).as('getVessels');
    cy.intercept('GET', '**/api/VesselVisitNotification/submitted', {
      fixture: 'vvn-submitted.json',
    }).as('getSubmitted');
  });

  it('should display list of submitted VVNs', () => {
    cy.visit('/shipping-agent/vvn-submitted');
    cy.wait('@getSubmitted');
    cy.get('.vvn-card').should('have.length.greaterThan', 0);
  });
});

describe('Shipping Agent - Reviewed VVNs', () => {
  beforeEach(() => {
    cy.loginWithGoogle('shipping@example.com', 'ShippingAgentRepresentative');
    cy.intercept('POST', '**/api/audit/unauthorized-access', { statusCode: 200 });
    cy.intercept('GET', '**/api/Vessel', { fixture: 'vessels.json' }).as('getVessels');
    cy.intercept('GET', '**/api/Dock', { fixture: 'docks.json' }).as('getDocks');
    cy.intercept('GET', '**/api/VesselVisitNotification/reviewed', {
      fixture: 'vvn-reviewed.json',
    }).as('getReviewed');
  });

  it('should display list of reviewed VVNs', () => {
    cy.visit('/shipping-agent/vvn-reviewed');
    cy.wait('@getReviewed');
    cy.get('.vvn-card').should('have.length.greaterThan', 0);
  });
});

describe('Shipping Agent - Container Management', () => {
  beforeEach(() => {
    cy.loginWithGoogle('shipping@example.com', 'ShippingAgentRepresentative');
    cy.intercept('POST', '**/api/audit/unauthorized-access', { statusCode: 200 });
    cy.intercept('GET', '**/api/Container', { fixture: 'containers.json' }).as('getContainers');
  });

  it('should navigate to container management', () => {
    cy.visit('/shipping-agent');
    cy.contains('Containers').click();
    cy.url().should('include', '/shipping-agent/containers');
    cy.wait('@getContainers');
  });

  it('should display list of containers', () => {
    cy.visit('/shipping-agent/containers');
    cy.wait('@getContainers');
    cy.get('table').should('be.visible');
    cy.get('tbody tr').should('have.length.greaterThan', 0);
  });

  it('should create a new container', () => {
    cy.visit('/shipping-agent/containers');

    cy.intercept('POST', '**/api/Container', {
      statusCode: 201,
      body: { id: '123', containerNumber: 'CONT123456' },
    }).as('createContainer');

    cy.contains('+ Add New Container').click();

    // Fill form using name attributes (ngModel)
    cy.get('input[name="isoCode"]').type('MSKU3881445');
    cy.get('input[name="cargoType"]').type('General Cargo');
    cy.get('textarea[name="description"]').type('Test container for E2E');

    cy.get('button[type="submit"]').click();
    cy.wait('@createContainer');
    cy.contains('Container created successfully').should('be.visible');
  });

  it('should edit a container', () => {
    cy.visit('/shipping-agent/containers');
    cy.wait('@getContainers');

    cy.intercept('PUT', '**/api/Container/*', {
      statusCode: 200,
      body: { message: 'Container updated successfully' },
    }).as('updateContainer');

    // Click Edit on first container
    cy.get('button.btn-edit').first().click();

    // Update cargo type using name attribute
    cy.get('input[name="cargoType"]').clear().type('Refrigerated');

    // Submit form
    cy.get('button[type="submit"]').click();
    cy.wait('@updateContainer');
  });
});
