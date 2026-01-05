const mockDatabase = require("./mockDatabase");

// Setup before all tests
beforeAll(() => {
  mockDatabase.initialize();
});

// Clear and re-seed data before each test
beforeEach(() => {
  mockDatabase.clear();
  mockDatabase.seed();
});

// Cleanup after all tests
afterAll(() => {
  mockDatabase.close();
});

module.exports = mockDatabase;
