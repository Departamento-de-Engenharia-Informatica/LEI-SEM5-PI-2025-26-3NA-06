const express = require("express");
const router = express.Router();
const { authenticateJWT, authorizeRole } = require("../middleware/auth");

/**
 * @swagger
 * tags:
 *   name: Incidents
 *   description: CRUD operations for Incidents (US 4.1.13)
 */

// Placeholder implementations
router.post(
  "/",
  authenticateJWT,
  authorizeRole("LogisticOperator"),
  (req, res) => {
    res.status(501).json({ message: "Create incident - Not implemented yet" });
  }
);

router.get(
  "/",
  authenticateJWT,
  authorizeRole("LogisticOperator"),
  (req, res) => {
    res.status(501).json({ message: "List incidents - Not implemented yet" });
  }
);

router.get(
  "/:id",
  authenticateJWT,
  authorizeRole("LogisticOperator"),
  (req, res) => {
    res
      .status(501)
      .json({ message: "Get incident by ID - Not implemented yet" });
  }
);

router.put(
  "/:id",
  authenticateJWT,
  authorizeRole("LogisticOperator"),
  (req, res) => {
    res.status(501).json({ message: "Update incident - Not implemented yet" });
  }
);

router.delete(
  "/:id",
  authenticateJWT,
  authorizeRole("LogisticOperator"),
  (req, res) => {
    res.status(501).json({ message: "Delete incident - Not implemented yet" });
  }
);

module.exports = router;
