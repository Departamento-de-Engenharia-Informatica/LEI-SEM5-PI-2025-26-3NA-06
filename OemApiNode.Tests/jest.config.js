module.exports = {
  testEnvironment: "node",
  testMatch: ["**/__tests__/**/*.test.js", "**/?(*.)+(spec|test).js"],
  collectCoverageFrom: [
    "../OemApiNode/src/**/*.js",
    "!../OemApiNode/src/server.js",
  ],
  coverageDirectory: "./coverage",
  verbose: true,
  testTimeout: 10000,
};
