const express = require("express");
const router = express.Router();
const { authenticateJWT, authorizeRole } = require("../middleware/auth");

/**
 * @swagger
 * tags:
 *   name: Task Categories
 *   description: CRUD operations for Complementary Task Categories catalog (US 4.1.14)
 */

// Placeholder implementations
router.post(
  "/",
  authenticateJWT,
  authorizeRole("PortAuthority"),
  (req, res) => {
    res
      .status(501)
      .json({ message: "Create task category - Not implemented yet" });
  }
);

router.get(
  "/",
  authenticateJWT,
  authorizeRole("LogisticOperator", "PortAuthority"),
  (req, res) => {
    res
      .status(501)
      .json({ message: "List task categories - Not implemented yet" });
  }
);

router.get(
  "/:id",
  authenticateJWT,
  authorizeRole("LogisticOperator", "PortAuthority"),
  (req, res) => {
    res
      .status(501)
      .json({ message: "Get task category by ID - Not implemented yet" });
  }
);

router.put(
  "/:id",
  authenticateJWT,
  authorizeRole("PortAuthority"),
  (req, res) => {
    res
      .status(501)
      .json({ message: "Update task category - Not implemented yet" });
  }
);

router.delete(
  "/:id",
  authenticateJWT,
  authorizeRole("PortAuthority"),
  (req, res) => {
    res
      .status(501)
      .json({ message: "Delete task category - Not implemented yet" });
  }
);

module.exports = router;
