const express = require("express");
const router = express.Router();
const { authenticateJWT, authorizeRole } = require("../middleware/auth");

/**
 * @swagger
 * tags:
 *   name: Complementary Tasks
 *   description: CRUD operations for Complementary Tasks
 */

// Placeholder implementations
router.post(
  "/",
  authenticateJWT,
  authorizeRole("LogisticOperator"),
  (req, res) => {
    res
      .status(501)
      .json({ message: "Create complementary task - Not implemented yet" });
  }
);

router.get(
  "/",
  authenticateJWT,
  authorizeRole("LogisticOperator"),
  (req, res) => {
    res
      .status(501)
      .json({ message: "List complementary tasks - Not implemented yet" });
  }
);

router.get(
  "/:id",
  authenticateJWT,
  authorizeRole("LogisticOperator"),
  (req, res) => {
    res
      .status(501)
      .json({ message: "Get complementary task by ID - Not implemented yet" });
  }
);

router.put(
  "/:id",
  authenticateJWT,
  authorizeRole("LogisticOperator"),
  (req, res) => {
    res
      .status(501)
      .json({ message: "Update complementary task - Not implemented yet" });
  }
);

router.delete(
  "/:id",
  authenticateJWT,
  authorizeRole("LogisticOperator"),
  (req, res) => {
    res
      .status(501)
      .json({ message: "Delete complementary task - Not implemented yet" });
  }
);

module.exports = router;
