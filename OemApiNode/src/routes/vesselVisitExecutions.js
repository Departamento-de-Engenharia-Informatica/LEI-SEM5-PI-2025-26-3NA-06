const express = require("express");
const router = express.Router();
const { authenticateJWT, authorizeRole } = require("../middleware/auth");
const vveController = require("../controllers/vesselVisitExecutionController");

/**
 * @swagger
 * tags:
 *   name: Vessel Visit Executions
 *   description: CRUD operations for Vessel Visit Executions (VVE)
 */

/**
 * @swagger
 * /api/oem/vves/prepare-today:
 *   post:
 *     summary: Prepare today's VVEs
 *     description: |
 *       Creates NotStarted VVE placeholders for all VVNs in today's operation plan.
 *       Business Rule: Only creates VVEs for planDate = today.
 *       VVEs start with status=NotStarted and null actual fields.
 *     tags: [Vessel Visit Executions]
 *     security:
 *       - bearerAuth: []
 *     responses:
 *       201:
 *         description: VVEs prepared successfully
 *       400:
 *         description: No operation plan for today or validation error
 *       401:
 *         description: Unauthorized
 */
router.post(
  "/prepare-today",
  authenticateJWT,
  authorizeRole("LogisticOperator"),
  vveController.prepareTodaysVVEs
);

/**
 * @swagger
 * /api/oem/vves:
 *   post:
 *     summary: Create a new VVE manually
 *     tags: [Vessel Visit Executions]
 *     security:
 *       - bearerAuth: []
 *     responses:
 *       201:
 *         description: VVE created successfully
 *       400:
 *         description: Validation error
 *       401:
 *         description: Unauthorized
 */
router.post(
  "/",
  authenticateJWT,
  authorizeRole("LogisticOperator"),
  vveController.create
);

/**
 * @swagger
 * /api/oem/vves:
 *   get:
 *     summary: Search VVEs with filters
 *     tags: [Vessel Visit Executions]
 *     security:
 *       - bearerAuth: []
 *     responses:
 *       200:
 *         description: List of VVEs
 *       401:
 *         description: Unauthorized
 */
router.get(
  "/",
  authenticateJWT,
  authorizeRole("LogisticOperator"),
  vveController.search
);

/**
 * @swagger
 * /api/oem/vves/{id}:
 *   get:
 *     summary: Get VVE by ID
 *     tags: [Vessel Visit Executions]
 *     security:
 *       - bearerAuth: []
 *     responses:
 *       200:
 *         description: VVE details
 *       404:
 *         description: VVE not found
 *       401:
 *         description: Unauthorized
 */
router.get(
  "/:id",
  authenticateJWT,
  authorizeRole("LogisticOperator"),
  vveController.getById
);

/**
 * @swagger
 * /api/oem/vves/vvn/{vvnId}:
 *   get:
 *     summary: Get VVE by VVN ID
 *     tags: [Vessel Visit Executions]
 *     security:
 *       - bearerAuth: []
 *     responses:
 *       200:
 *         description: VVE for the specified VVN
 *       404:
 *         description: VVE not found
 *       401:
 *         description: Unauthorized
 */
router.get(
  "/vvn/:vvnId",
  authenticateJWT,
  authorizeRole("LogisticOperator"),
  vveController.getByVvnId
);

/**
 * @swagger
 * /api/oem/vves/date/{date}:
 *   get:
 *     summary: Get VVEs by date
 *     tags: [Vessel Visit Executions]
 *     security:
 *       - bearerAuth: []
 *     responses:
 *       200:
 *         description: List of VVEs for the date
 *       401:
 *         description: Unauthorized
 */
router.get(
  "/date/:date",
  authenticateJWT,
  authorizeRole("LogisticOperator"),
  vveController.getByDate
);

/**
 * @swagger
 * /api/oem/vves/{id}/berth:
 *   patch:
 *     summary: Update VVE berth time and dock (US 4.1.8)
 *     description: |
 *       Update actual arrival time and/or actual dock.
 *       Auto-transitions from NotStarted to InProgress when actualArrivalTime is set.
 *       Generates warnings if actual dock differs from planned dock or if delayed.
 *     tags: [Vessel Visit Executions]
 *     security:
 *       - bearerAuth: []
 *     responses:
 *       200:
 *         description: Berth information updated
 *       400:
 *         description: Validation error
 *       404:
 *         description: VVE not found
 *       401:
 *         description: Unauthorized
 */
router.patch(
  "/:id/berth",
  authenticateJWT,
  authorizeRole("LogisticOperator"),
  vveController.updateBerth
);

/**
 * @swagger
 * /api/oem/vves/{id}/status:
 *   patch:
 *     summary: Update VVE status
 *     description: |
 *       Transition VVE to a new status with validation.
 *       Valid transitions:
 *       - NotStarted -> InProgress, Delayed
 *       - InProgress -> Delayed, Completed
 *       - Delayed -> InProgress, Completed
 *       - Completed -> (terminal, no transitions)
 *     tags: [Vessel Visit Executions]
 *     security:
 *       - bearerAuth: []
 *     responses:
 *       200:
 *         description: Status updated
 *       400:
 *         description: Invalid status transition
 *       404:
 *         description: VVE not found
 *       401:
 *         description: Unauthorized
 */
router.patch(
  "/:id/status",
  authenticateJWT,
  authorizeRole("LogisticOperator"),
  vveController.updateStatus
);

/**
 * @swagger
 * /api/oem/vves/{id}:
 *   delete:
 *     summary: Delete VVE
 *     tags: [Vessel Visit Executions]
 *     security:
 *       - bearerAuth: []
 *     responses:
 *       204:
 *         description: VVE deleted
 *       404:
 *         description: VVE not found
 *       401:
 *         description: Unauthorized
 */
router.delete(
  "/:id",
  authenticateJWT,
  authorizeRole("LogisticOperator"),
  vveController.delete
);

module.exports = router;
