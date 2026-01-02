const express = require("express");
const router = express.Router();
const { authenticateJWT, authorizeRole } = require("../middleware/auth");
const incidentService = require("../services/IncidentService");

/**
 * @swagger
 * tags:
 *   name: Incidents
 *   description: Incident management operations (US 4.1.13)
 */

/**
 * @swagger
 * /api/oem/incidents:
 *   post:
 *     tags: [Incidents]
 *     summary: Create a new incident
 *     security:
 *       - bearerAuth: []
 *     requestBody:
 *       required: true
 *       content:
 *         application/json:
 *           schema:
 *             type: object
 *             required:
 *               - incidentTypeId
 *               - startTime
 *               - description
 *             properties:
 *               incidentTypeId:
 *                 type: string
 *                 format: uuid
 *               startTime:
 *                 type: string
 *                 format: date-time
 *               endTime:
 *                 type: string
 *                 format: date-time
 *                 nullable: true
 *               description:
 *                 type: string
 *               affectsAllVVEs:
 *                 type: boolean
 *               affectedVVEIds:
 *                 type: array
 *                 items:
 *                   type: string
 *                   format: uuid
 *     responses:
 *       201:
 *         description: Incident created successfully
 *       400:
 *         description: Validation error
 *       401:
 *         description: Unauthorized
 *       403:
 *         description: Forbidden - LogisticOperator role required
 */
router.post(
  "/",
  authenticateJWT,
  authorizeRole("LogisticOperator"),
  async (req, res) => {
    try {
      const result = await incidentService.createIncidentAsync(req.body);

      if (result.success) {
        res.status(201).json(result);
      } else {
        res.status(400).json(result);
      }
    } catch (error) {
      res.status(500).json({
        success: false,
        error: "Internal server error",
      });
    }
  }
);

/**
 * @swagger
 * /api/oem/incidents:
 *   get:
 *     tags: [Incidents]
 *     summary: Get all incidents with optional filters
 *     security:
 *       - bearerAuth: []
 *     parameters:
 *       - in: query
 *         name: date
 *         schema:
 *           type: string
 *           format: date
 *         description: Filter by date (YYYY-MM-DD)
 *       - in: query
 *         name: status
 *         schema:
 *           type: string
 *           enum: [active, inactive]
 *         description: Filter by status
 *       - in: query
 *         name: incidentTypeId
 *         schema:
 *           type: string
 *           format: uuid
 *         description: Filter by incident type
 *     responses:
 *       200:
 *         description: List of incidents
 *       401:
 *         description: Unauthorized
 */
router.get(
  "/",
  authenticateJWT,
  authorizeRole("LogisticOperator", "PortAuthorityOfficer"),
  async (req, res) => {
    try {
      const filters = {
        date: req.query.date ? new Date(req.query.date) : undefined,
        status: req.query.status,
        incidentTypeId: req.query.incidentTypeId,
      };

      const result = await incidentService.getAllIncidentsAsync(filters);

      if (result.success) {
        res.status(200).json(result);
      } else {
        res.status(500).json(result);
      }
    } catch (error) {
      res.status(500).json({
        success: false,
        error: "Internal server error",
      });
    }
  }
);

/**
 * @swagger
 * /api/oem/incidents/active:
 *   get:
 *     tags: [Incidents]
 *     summary: Get today's active incidents
 *     security:
 *       - bearerAuth: []
 *     responses:
 *       200:
 *         description: List of active incidents
 *       401:
 *         description: Unauthorized
 */
router.get(
  "/active",
  authenticateJWT,
  authorizeRole("LogisticOperator", "PortAuthorityOfficer"),
  async (req, res) => {
    try {
      const result = await incidentService.getTodaysActiveIncidentsAsync();

      if (result.success) {
        res.status(200).json(result);
      } else {
        res.status(500).json(result);
      }
    } catch (error) {
      res.status(500).json({
        success: false,
        error: "Internal server error",
      });
    }
  }
);

/**
 * @swagger
 * /api/oem/incidents/{id}:
 *   get:
 *     tags: [Incidents]
 *     summary: Get incident by ID
 *     security:
 *       - bearerAuth: []
 *     parameters:
 *       - in: path
 *         name: id
 *         required: true
 *         schema:
 *           type: string
 *           format: uuid
 *     responses:
 *       200:
 *         description: Incident details
 *       404:
 *         description: Incident not found
 *       401:
 *         description: Unauthorized
 */
router.get(
  "/:id",
  authenticateJWT,
  authorizeRole("LogisticOperator", "PortAuthorityOfficer"),
  async (req, res) => {
    try {
      const result = await incidentService.getIncidentByIdAsync(req.params.id);

      if (result.success) {
        res.status(200).json(result);
      } else {
        res.status(404).json(result);
      }
    } catch (error) {
      res.status(500).json({
        success: false,
        error: "Internal server error",
      });
    }
  }
);

/**
 * @swagger
 * /api/oem/incidents/{id}:
 *   put:
 *     tags: [Incidents]
 *     summary: Update incident
 *     security:
 *       - bearerAuth: []
 *     parameters:
 *       - in: path
 *         name: id
 *         required: true
 *         schema:
 *           type: string
 *           format: uuid
 *     requestBody:
 *       required: true
 *       content:
 *         application/json:
 *           schema:
 *             type: object
 *             properties:
 *               startTime:
 *                 type: string
 *                 format: date-time
 *               endTime:
 *                 type: string
 *                 format: date-time
 *                 nullable: true
 *               description:
 *                 type: string
 *               affectsAllVVEs:
 *                 type: boolean
 *               affectedVVEIds:
 *                 type: array
 *                 items:
 *                   type: string
 *                   format: uuid
 *     responses:
 *       200:
 *         description: Incident updated successfully
 *       400:
 *         description: Validation error
 *       404:
 *         description: Incident not found
 *       401:
 *         description: Unauthorized
 *       403:
 *         description: Forbidden - LogisticOperator role required
 */
router.put(
  "/:id",
  authenticateJWT,
  authorizeRole("LogisticOperator"),
  async (req, res) => {
    try {
      const result = await incidentService.updateIncidentAsync(
        req.params.id,
        req.body
      );

      if (result.success) {
        res.status(200).json(result);
      } else {
        const statusCode = result.error === "Incident not found" ? 404 : 400;
        res.status(statusCode).json(result);
      }
    } catch (error) {
      res.status(500).json({
        success: false,
        error: "Internal server error",
      });
    }
  }
);

/**
 * @swagger
 * /api/oem/incidents/{id}:
 *   delete:
 *     tags: [Incidents]
 *     summary: Delete incident (soft delete)
 *     security:
 *       - bearerAuth: []
 *     parameters:
 *       - in: path
 *         name: id
 *         required: true
 *         schema:
 *           type: string
 *           format: uuid
 *     responses:
 *       200:
 *         description: Incident deleted successfully
 *       404:
 *         description: Incident not found
 *       401:
 *         description: Unauthorized
 *       403:
 *         description: Forbidden - LogisticOperator role required
 */
router.delete(
  "/:id",
  authenticateJWT,
  authorizeRole("LogisticOperator"),
  async (req, res) => {
    try {
      const result = await incidentService.deleteIncidentAsync(req.params.id);

      if (result.success) {
        res.status(200).json(result);
      } else {
        const statusCode = result.error === "Incident not found" ? 404 : 400;
        res.status(statusCode).json(result);
      }
    } catch (error) {
      res.status(500).json({
        success: false,
        error: "Internal server error",
      });
    }
  }
);

module.exports = router;
