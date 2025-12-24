const express = require("express");
const router = express.Router();
const { authenticateJWT, authorizeRole } = require("../middleware/auth");

/**
 * @swagger
 * tags:
 *   name: Incident Types
 *   description: CRUD operations for Incident Types catalog (US 4.1.12)
 */

// Placeholder implementations
router.post(
  "/",
  authenticateJWT,
  authorizeRole("PortAuthority"),
  (req, res) => {
    res
      .status(501)
      .json({ message: "Create incident type - Not implemented yet" });
  }
);

router.get(
  "/",
  authenticateJWT,
  authorizeRole("LogisticOperator", "PortAuthority"),
  (req, res) => {
    res
      .status(501)
      .json({ message: "List incident types - Not implemented yet" });
  }
);

router.get(
  "/:id",
  authenticateJWT,
  authorizeRole("LogisticOperator", "PortAuthority"),
  (req, res) => {
    res
      .status(501)
      .json({ message: "Get incident type by ID - Not implemented yet" });
  }
);

router.put(
  "/:id",
  authenticateJWT,
  authorizeRole("PortAuthority"),
  (req, res) => {
    res
      .status(501)
      .json({ message: "Update incident type - Not implemented yet" });
  }
);

router.delete(
  "/:id",
  authenticateJWT,
  authorizeRole("PortAuthority"),
  (req, res) => {
    res
      .status(501)
      .json({ message: "Delete incident type - Not implemented yet" });
  }
);

module.exports = router;
