const fs = require("fs");
const path = require("path");

const testFiles = [
  "./2.ServiceTests/IncidentType/IncidentTypeService.test.js",
  "./2.ServiceTests/OperationPlan/OperationPlanService.test.js",
  "./2.ServiceTests/VesselVisitExecution/VesselVisitExecutionService.test.js",
];

testFiles.forEach((filePath) => {
  const fullPath = path.resolve(__dirname, filePath);
  let content = fs.readFileSync(fullPath, "utf8");

  // Get service name from file path
  const serviceName = path.basename(filePath, ".test.js");

  // Remove the service variable declaration and instantiation
  content = content.replace(/let service;/g, "");
  content = content.replace(/service = new \w+Service\(\);/g, "");

  // Replace all service. with ServiceName.
  content = content.replace(
    /\$\(\$file\.BaseName\.Replace\("\.test", ""\)\)\./g,
    serviceName + "."
  );

  // Move mocks before require statements
  const lines = content.split("\n");
  const requireLines = [];
  const mockLines = [];
  const otherLines = [];

  let inDescribe = false;
  for (let i = 0; i < lines.length; i++) {
    const line = lines[i];
    if (line.includes("describe(")) {
      inDescribe = true;
    }
    if (!inDescribe) {
      if (line.includes("require(") && !line.includes("jest.mock")) {
        requireLines.push(line);
      } else if (line.includes("jest.mock(")) {
        mockLines.push(line);
      } else if (line.trim() !== "") {
        otherLines.push(line);
      }
    } else {
      otherLines.push(line);
    }
  }

  // Rebuild with mocks first, then requires
  const newContent = [
    ...mockLines,
    "",
    ...requireLines,
    "",
    ...otherLines,
  ].join("\n");

  fs.writeFileSync(fullPath, newContent, "utf8");
  console.log(`Fixed ${filePath}`);
});

console.log("All files fixed!");
