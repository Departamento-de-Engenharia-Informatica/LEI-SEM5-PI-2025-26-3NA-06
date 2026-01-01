const express = require("express");
const router = express.Router();
const { authenticateJWT, authorizeRole } = require("../middleware/auth");
const operationPlanController = require("../controllers/operationPlanController");

/**
 * @swagger
 * tags:
 *   name: Operation Plans
 *   description: CRUD operations for Operation Plans
 */

/**
 * @swagger
 * /api/oem/operation-plans:
 *   post:
 *     summary: Create a new Operation Plan
 *     description: Generate and store an Operation Plan for a specific date (US 4.1.2)
 *     tags: [Operation Plans]
 *     security:
 *       - bearerAuth: []
 *     requestBody:
 *       required: true
 *       content:
 *         application/json:
 *           schema:
 *             type: object
 *             properties:
 *               date:
 *                 type: string
 *                 format: date
 *               algorithmUsed:
 *                 type: string
 *               vvns:
 *                 type: array
 *                 items:
 *                   type: object
 *     responses:
 *       201:
 *         description: Operation Plan created successfully
 *       401:
 *         description: Unauthorized
 */
router.post(
  "/",
  authenticateJWT,
  authorizeRole("LogisticOperator"),
  operationPlanController.create
);

/**
 * @swagger
 * /api/oem/operation-plans/replace:
 *   post:
 *     summary: Replace an existing Operation Plan
 *     description: Delete existing Operation Plan for a date and create a new one
 *     tags: [Operation Plans]
 *     security:
 *       - bearerAuth: []
 *     requestBody:
 *       required: true
 *       content:
 *         application/json:
 *           schema:
 *             type: object
 *             properties:
 *               planDate:
 *                 type: string
 *                 format: date
 *               isFeasible:
 *                 type: boolean
 *               warnings:
 *                 type: array
 *                 items:
 *                   type: string
 *               vesselVisitNotifications:
 *                 type: array
 *                 items:
 *                   type: object
 *     responses:
 *       200:
 *         description: Operation Plan replaced successfully
 *       401:
 *         description: Unauthorized
 */
router.post(
  "/replace",
  authenticateJWT,
  authorizeRole("LogisticOperator"),
  operationPlanController.replace
);

/**
 * @swagger
 * /api/oem/operation-plans:
 *   get:
 *     summary: Search and list Operation Plans
 *     description: Query Operation Plans by date range and/or vessel identifier (US 4.1.3)
 *     tags: [Operation Plans]
 *     security:
 *       - bearerAuth: []
 *     parameters:
 *       - in: query
 *         name: startDate
 *         schema:
 *           type: string
 *           format: date
 *         description: Filter plans from this date (inclusive)
 *       - in: query
 *         name: endDate
 *         schema:
 *           type: string
 *           format: date
 *         description: Filter plans until this date (inclusive)
 *       - in: query
 *         name: vesselIMO
 *         schema:
 *           type: string
 *         description: Filter by vessel IMO or vessel ID (searches in assignments)
 *       - in: query
 *         name: skip
 *         schema:
 *           type: integer
 *           default: 0
 *         description: Number of records to skip (pagination)
 *       - in: query
 *         name: take
 *         schema:
 *           type: integer
 *           default: 50
 *         description: Number of records to return (pagination)
 *     responses:
 *       200:
 *         description: List of Operation Plans with sortable results
 *       401:
 *         description: Unauthorized
 */
router.get(
  "/",
  authenticateJWT,
  authorizeRole("LogisticOperator"),
  operationPlanController.search
);

/**
 * @swagger
 * /api/oem/operation-plans/{id}:
 *   get:
 *     summary: Get Operation Plan by ID
 *     tags: [Operation Plans]
 *     security:
 *       - bearerAuth: []
 *     parameters:
 *       - in: path
 *         name: id
 *         required: true
 *         schema:
 *           type: string
 *     responses:
 *       200:
 *         description: Operation Plan details
 *       404:
 *         description: Not found
 */
router.get(
  "/:id",
  authenticateJWT,
  authorizeRole("LogisticOperator"),
  operationPlanController.getById
);

/**
 * @swagger
 * /api/oem/operation-plans/{id}:
 *   put:
 *     summary: Update Operation Plan
 *     description: Manually update an Operation Plan (US 4.1.4)
 *     tags: [Operation Plans]
 *     security:
 *       - bearerAuth: []
 *     parameters:
 *       - in: path
 *         name: id
 *         required: true
 *         schema:
 *           type: string
 *     requestBody:
 *       required: true
 *       content:
 *         application/json:
 *           schema:
 *             type: object
 *     responses:
 *       200:
 *         description: Operation Plan updated
 *       404:
 *         description: Not found
 */
router.put(
  "/:id",
  authenticateJWT,
  authorizeRole("LogisticOperator"),
  operationPlanController.update
);

/**
 * @swagger
 * /api/oem/operation-plans/{id}/status:
 *   patch:
 *     summary: Update Operation Plan Status
 *     description: |
 *       Transition an Operation Plan to a new status with validation.
 *       Valid transitions:
 *       - Pending -> InProgress, Cancelled
 *       - InProgress -> Completed, Cancelled
 *       - Completed -> (terminal, no transitions allowed)
 *       - Cancelled -> (terminal, no transitions allowed)
 *     tags: [Operation Plans]
 *     security:
 *       - bearerAuth: []
 *     parameters:
 *       - in: path
 *         name: id
 *         required: true
 *         schema:
 *           type: string
 *           format: uuid
 *         description: Operation Plan ID
 *     requestBody:
 *       required: true
 *       content:
 *         application/json:
 *           schema:
 *             type: object
 *             required:
 *               - status
 *             properties:
 *               status:
 *                 type: string
 *                 enum: [Pending, InProgress, Completed, Cancelled]
 *                 description: The new status to transition to
 *     responses:
 *       200:
 *         description: Status updated successfully
 *       400:
 *         description: Invalid status transition
 *       404:
 *         description: Operation Plan not found
 *       401:
 *         description: Unauthorized
 */
router.patch(
  "/:id/status",
  authenticateJWT,
  authorizeRole("LogisticOperator"),
  operationPlanController.updateStatus
);

/**
 * @swagger
 * /api/oem/operation-plans/{id}:
 *   delete:
 *     summary: Delete Operation Plan
 *     tags: [Operation Plans]
 *     security:
 *       - bearerAuth: []
 *     parameters:
 *       - in: path
 *         name: id
 *         required: true
 *         schema:
 *           type: string
 *     responses:
 *       204:
 *         description: Deleted successfully
 *       404:
 *         description: Not found
 */
router.delete(
  "/:id",
  authenticateJWT,
  authorizeRole("LogisticOperator"),
  operationPlanController.delete
);

/**
 * @swagger
 * /api/oem/operation-plans/date/{date}:
 *   get:
 *     summary: Get Operation Plan by date
 *     tags: [Operation Plans]
 *     security:
 *       - bearerAuth: []
 *     parameters:
 *       - in: path
 *         name: date
 *         required: true
 *         schema:
 *           type: string
 *           format: date
 *     responses:
 *       200:
 *         description: Operation Plan for specific date
 *       404:
 *         description: Not found
 */
router.get(
  "/date/:date",
  authenticateJWT,
  authorizeRole("LogisticOperator"),
  operationPlanController.getByDate
);

/**
 * @swagger
 * /api/oem/operation-plans/validate:
 *   post:
 *     summary: Validate operation plan feasibility
 *     description: Check if an operation plan is feasible without saving it
 *     tags: [Operation Plans]
 *     security:
 *       - bearerAuth: []
 *     requestBody:
 *       required: true
 *       content:
 *         application/json:
 *           schema:
 *             type: object
 *     responses:
 *       200:
 *         description: Validation result
 */
router.post(
  "/validate",
  authenticateJWT,
  authorizeRole("LogisticOperator"),
  operationPlanController.validateFeasibility
);

/**
 * @swagger
 * /api/oem/operation-plans/vvn/{vvnId}/cargo-manifests:
 *   get:
 *     summary: Get cargo manifests for a VVN
 *     description: Fetch loading and unloading cargo manifests for a specific VVN from the Core API
 *     tags: [Operation Plans]
 *     security:
 *       - bearerAuth: []
 *     parameters:
 *       - in: path
 *         name: vvnId
 *         required: true
 *         schema:
 *           type: string
 *           format: uuid
 *     responses:
 *       200:
 *         description: Cargo manifests retrieved successfully
 *       404:
 *         description: VVN not found
 *       401:
 *         description: Unauthorized
 */
router.get(
  "/vvn/:vvnId/cargo-manifests",
  authenticateJWT,
  authorizeRole("LogisticOperator"),
  operationPlanController.getCargoManifests
);

module.exports = router;
