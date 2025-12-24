const express = require("express");
const router = express.Router();
const { authenticateJWT, authorizeRole } = require("../middleware/auth");

/**
 * @swagger
 * tags:
 *   name: Vessel Visit Executions
 *   description: CRUD operations for Vessel Visit Executions (VVE)
 */

// Placeholder implementations - to be expanded
router.post(
  "/",
  authenticateJWT,
  authorizeRole("LogisticOperator"),
  (req, res) => {
    res.status(501).json({ message: "Create VVE - Not implemented yet" });
  }
);

router.get(
  "/",
  authenticateJWT,
  authorizeRole("LogisticOperator"),
  (req, res) => {
    res.status(501).json({ message: "List VVE - Not implemented yet" });
  }
);

router.get(
  "/:id",
  authenticateJWT,
  authorizeRole("LogisticOperator"),
  (req, res) => {
    res.status(501).json({ message: "Get VVE by ID - Not implemented yet" });
  }
);

router.put(
  "/:id/berth",
  authenticateJWT,
  authorizeRole("LogisticOperator"),
  (req, res) => {
    res
      .status(501)
      .json({
        message:
          "Update VVE berth time and dock (US 4.1.8) - Not implemented yet",
      });
  }
);

router.put(
  "/:id/operations",
  authenticateJWT,
  authorizeRole("LogisticOperator"),
  (req, res) => {
    res
      .status(501)
      .json({
        message:
          "Update VVE with executed operations (US 4.1.9) - Not implemented yet",
      });
  }
);

module.exports = router;
