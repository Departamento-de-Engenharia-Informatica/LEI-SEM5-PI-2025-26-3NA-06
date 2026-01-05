/// <reference types="cypress" />

/**
 * E2E Tests for Logistic Operator Workflows
 * User Stories:
 * - As a Logistic Operator, I want to view daily schedules
 * - As a Logistic Operator, I want to manage operation plans
 * - As a Logistic Operator, I want to manage vessel visit events
 * - As a Logistic Operator, I want to report and track incidents
 */

describe('Logistic Operator - Dashboard', () => {
  beforeEach(() => {
    cy.loginWithGoogle('logistic@example.com', 'LogisticOperator');
    cy.intercept('POST', '**/api/audit/unauthorized-access', { statusCode: 200 });
  });

  it('should display logistic operator dashboard', () => {
    cy.url().should('include', '/logistic-operator');
    cy.contains('Logistics Operator Dashboard').should('be.visible');
    cy.contains('Daily Schedule').should('be.visible');
    cy.contains('Operation Plans').should('be.visible');
    cy.contains('VVE Management').should('be.visible');
  });
});

describe('Logistic Operator - Daily Schedule', () => {
  beforeEach(() => {
    cy.loginWithGoogle('logistic@example.com', 'LogisticOperator');
    cy.intercept('POST', '**/api/audit/unauthorized-access', { statusCode: 200 });
    cy.intercept('POST', '**/api/Scheduling/daily*', {
      fixture: 'daily-schedule.json',
    }).as('generateSchedule');
  });

  it('should navigate to daily schedule', () => {
    cy.visit('/logistic-operator');
    cy.contains('Daily Schedule').click();
    cy.url().should('include', '/logistic-operator/daily-schedule');
    cy.contains('Daily Schedule Generator').should('be.visible');
  });

  it('should display schedule generator form', () => {
    cy.visit('/logistic-operator/daily-schedule');
    cy.get('input[type="date"]').should('be.visible');
    cy.contains('Generate Schedule').should('be.visible');
  });

  it('should generate schedule for a selected date', () => {
    cy.visit('/logistic-operator/daily-schedule');
    cy.get('input[type="date"]').type('2026-02-01');
    cy.contains('Generate Schedule').click();
    cy.wait('@generateSchedule');
    cy.contains('Schedule for 2026-02-01').should('be.visible');
    cy.contains('Dock Schedules').should('be.visible');
  });

  it('should display schedule warnings if any', () => {
    cy.visit('/logistic-operator/daily-schedule');
    cy.intercept('POST', '**/api/Scheduling/daily*', {
      body: {
        date: '2026-02-01',
        isFeasible: false,
        warnings: ['Dock 1 overbooked', 'Insufficient storage'],
        dockSchedules: [],
      },
    }).as('generateScheduleWithWarnings');

    cy.get('input[type="date"]').type('2026-02-01');
    cy.contains('Generate Schedule').click();
    cy.wait('@generateScheduleWithWarnings');
    cy.contains('Warnings').should('be.visible');
  });
});

describe('Logistic Operator - Operation Plans', () => {
  beforeEach(() => {
    cy.loginWithGoogle('logistic@example.com', 'LogisticOperator');
    cy.intercept('POST', '**/api/audit/unauthorized-access', { statusCode: 200 });
    cy.intercept('GET', '**/api/oem/operation-plans*', { fixture: 'operation-plans.json' }).as(
      'getOperationPlans'
    );
  });

  it('should navigate to operation plans', () => {
    cy.visit('/logistic-operator');
    cy.contains('Operation Plans').click();
    cy.url().should('include', '/logistic-operator/operation-plans');
    cy.wait('@getOperationPlans');
  });

  it('should display operation plans list', () => {
    cy.visit('/logistic-operator/operation-plans');
    cy.wait('@getOperationPlans');
    cy.contains('Operation Plans').should('be.visible');
    cy.contains('Search & Filter').should('be.visible');
  });

  it('should filter operation plans by date range', () => {
    cy.visit('/logistic-operator/operation-plans');
    cy.wait('@getOperationPlans');

    cy.intercept('GET', '**/api/oem/operation-plans?startDate=2026-02-01&endDate=2026-02-28', {
      fixture: 'operation-plans.json',
    }).as('getFilteredPlans');

    cy.get('input#startDate').type('2026-02-01');
    cy.get('input#endDate').type('2026-02-28');
    cy.contains('button', 'Search').click();
    cy.wait('@getFilteredPlans');
  });

  it('should display search results when plans exist', () => {
    cy.visit('/logistic-operator/operation-plans');
    cy.wait('@getOperationPlans');

    cy.contains('Results (').should('be.visible');
  });
});

describe('Logistic Operator - VVE Management', () => {
  beforeEach(() => {
    cy.loginWithGoogle('logistic@example.com', 'LogisticOperator');
    cy.intercept('POST', '**/api/audit/unauthorized-access', { statusCode: 200 });
    cy.intercept('GET', '**/api/Dock', { fixture: 'docks.json' }).as('getDocks');
    cy.intercept('GET', '**/api/oem/incidents*', { fixture: 'incidents.json' }).as('getIncidents');
    cy.intercept('GET', '**/api/oem/operation-plans/date/*', {
      fixture: 'operation-plans.json',
    }).as('getOperationPlansByDate');
  });

  it('should navigate to VVE management', () => {
    cy.visit('/logistic-operator');
    cy.contains('VVE Management').click();
    cy.url().should('include', '/logistic-operator/vve-management');
  });

  it('should display VVE management page', () => {
    cy.visit('/logistic-operator/vve-management');
    cy.contains('Vessel Visit Executions').should('be.visible');
  });

  it('should display message when no operation plan exists', () => {
    cy.intercept('GET', '**/api/oem/operation-plans/date/*', {
      body: { success: false, data: null },
    }).as('getNoOperationPlan');

    cy.visit('/logistic-operator/vve-management');
    cy.wait('@getNoOperationPlan');
    cy.contains('No Operation Plan for Today').should('be.visible');
  });

  it('should prepare VVEs when operation plan exists', () => {
    // Intercept VVE fetch to return empty (no VVEs prepared yet)
    cy.intercept('GET', '**/api/oem/vessel-visit-executions/date/*', {
      body: { success: true, data: [] },
    }).as('getVVEs');

    cy.intercept('POST', '**/api/oem/vessel-visit-executions/prepare*', {
      fixture: 'vve-prepare-result.json',
    }).as('prepareVVEs');

    cy.visit('/logistic-operator/vve-management');
    cy.wait('@getVVEs');
    cy.contains('button', "Prepare Today's VVEs").click();
    cy.wait('@prepareVVEs');
    cy.get('.prepare-result').should('be.visible');
    cy.contains('.stat-label', 'Created').should('be.visible');
  });
});

describe('Logistic Operator - Incidents Management', () => {
  beforeEach(() => {
    cy.loginWithGoogle('logistic@example.com', 'LogisticOperator');
    cy.intercept('POST', '**/api/audit/unauthorized-access', { statusCode: 200 });
    cy.intercept('GET', '**/api/oem/incidents*', { fixture: 'incidents.json' }).as('getIncidents');
    cy.intercept('GET', '**/api/oem/incident-types*', { fixture: 'incident-types.json' }).as(
      'getIncidentTypes'
    );
  });

  it('should navigate to incidents page', () => {
    cy.visit('/logistic-operator/incidents');
    cy.url().should('include', '/logistic-operator/incidents');
    cy.wait('@getIncidents');
  });

  it('should display incidents table with data', () => {
    cy.visit('/logistic-operator/incidents');
    cy.wait('@getIncidents');
    cy.get('table').should('be.visible');
    cy.contains('Incident Management').should('be.visible');
    cy.get('tbody tr').should('have.length', 3);
  });

  it('should display incident details in table', () => {
    cy.visit('/logistic-operator/incidents');
    cy.wait('@getIncidents');

    // Check first incident
    cy.contains('EQUIP-001').should('be.visible');
    cy.contains('Equipment Failure').should('be.visible');
    cy.contains('Critical').should('be.visible');
  });

  it('should have filter controls', () => {
    cy.visit('/logistic-operator/incidents');
    cy.wait('@getIncidents');

    cy.get('input[type="date"]').should('be.visible');
    cy.contains('Apply').should('be.visible');
    cy.contains('Clear').should('be.visible');
  });

  it('should edit an incident', () => {
    cy.visit('/logistic-operator/incidents');
    cy.wait('@getIncidents');

    cy.intercept('PUT', '**/api/oem/incidents/801', {
      statusCode: 200,
      body: { success: true, message: 'Incident updated' },
    }).as('updateIncident');

    cy.get('tbody tr')
      .first()
      .within(() => {
        cy.contains('button', 'Edit').click();
      });

    cy.get('.modal-overlay').should('be.visible');
    cy.get('textarea').clear().type('Updated description');
    cy.contains('button', 'Update').click();
    cy.wait('@updateIncident');
  });

  it('should delete an incident', () => {
    cy.visit('/logistic-operator/incidents');
    cy.wait('@getIncidents');

    cy.intercept('DELETE', '**/api/oem/incidents/801', {
      statusCode: 200,
      body: { success: true, message: 'Incident deleted' },
    }).as('deleteIncident');

    cy.get('tbody tr')
      .first()
      .within(() => {
        cy.contains('button', 'Delete').click();
      });

    cy.get('.modal-overlay').should('be.visible');
    cy.get('.modal-content').within(() => {
      cy.contains('button', 'Delete').click();
    });
    cy.wait('@deleteIncident');
  });

  it('should filter incidents by date', () => {
    cy.visit('/logistic-operator/incidents');
    cy.wait('@getIncidents');

    cy.intercept('GET', '**/api/oem/incidents?date=2026-02-01', {
      fixture: 'incidents.json',
    }).as('getFilteredIncidents');

    cy.get('input[type="date"]').clear().type('2026-02-01');
    cy.contains('Apply').click();
    cy.wait('@getFilteredIncidents');
  });

  it('should filter incidents by status', () => {
    cy.visit('/logistic-operator/incidents');
    cy.wait('@getIncidents');

    cy.get('select').eq(0).select('active');
    cy.contains('Apply').click();

    cy.get('tbody tr').should('have.length.greaterThan', 0);
    cy.get('tbody tr')
      .first()
      .within(() => {
        cy.get('.badge').should('exist');
      });
  });

  it('should filter incidents by incident type', () => {
    cy.visit('/logistic-operator/incidents');
    cy.wait('@getIncidents');
    cy.wait('@getIncidentTypes');

    cy.get('select').eq(1).select(1);
    cy.contains('Apply').click();
  });

  it('should clear all filters', () => {
    cy.visit('/logistic-operator/incidents');
    cy.wait('@getIncidents');

    cy.get('input[type="date"]').type('2026-02-01');
    cy.get('select').eq(0).select('active');
    cy.contains('Clear').click();

    cy.get('input[type="date"]').should('not.have.value', '2026-02-01');
  });
});
