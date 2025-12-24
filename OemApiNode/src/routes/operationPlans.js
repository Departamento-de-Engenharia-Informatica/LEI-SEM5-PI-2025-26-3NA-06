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
 * /api/oem/operation-plans:
 *   get:
 *     summary: Search and list Operation Plans
 *     description: Query Operation Plans by date range or vessel (US 4.1.3)
 *     tags: [Operation Plans]
 *     security:
 *       - bearerAuth: []
 *     parameters:
 *       - in: query
 *         name: startDate
 *         schema:
 *           type: string
 *           format: date
 *       - in: query
 *         name: endDate
 *         schema:
 *           type: string
 *           format: date
 *       - in: query
 *         name: vesselId
 *         schema:
 *           type: string
 *     responses:
 *       200:
 *         description: List of Operation Plans
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

module.exports = router;
