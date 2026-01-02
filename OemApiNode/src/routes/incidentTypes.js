const express = require("express");
const router = express.Router();
const { authenticateJWT, authorizeRole } = require("../middleware/auth");
const incidentTypeService = require("../services/IncidentTypeService");

/**
 * @swagger
 * tags:
 *   name: Incident Types
 *   description: CRUD operations for Incident Types catalog (US 4.1.12)
 */

/**
 * @swagger
 * /api/oem/incident-types:
 *   post:
 *     tags: [Incident Types]
 *     summary: Create a new incident type
 *     security:
 *       - bearerAuth: []
 *     requestBody:
 *       required: true
 *       content:
 *         application/json:
 *           schema:
 *             type: object
 *             required:
 *               - code
 *               - name
 *               - severity
 *             properties:
 *               code:
 *                 type: string
 *                 maxLength: 10
 *                 example: "T-INC001"
 *               name:
 *                 type: string
 *                 maxLength: 255
 *                 example: "Equipment Failure"
 *               description:
 *                 type: string
 *                 example: "Equipment malfunctions affecting operations"
 *               severity:
 *                 type: string
 *                 enum: [Minor, Major, Critical]
 *                 example: "Major"
 *               parentId:
 *                 type: string
 *                 format: uuid
 *                 nullable: true
 *     responses:
 *       201:
 *         description: Incident type created successfully
 *       400:
 *         description: Validation error
 *       401:
 *         description: Unauthorized
 *       403:
 *         description: Forbidden - PortAuthority role required
 */
router.post(
  "/",
  authenticateJWT,
  authorizeRole("PortAuthorityOfficer"),
  async (req, res) => {
    try {
      const result = await incidentTypeService.createIncidentTypeAsync(
        req.body,
        req.user.userId,
        req.user.username
      );

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
 * /api/oem/incident-types:
 *   get:
 *     tags: [Incident Types]
 *     summary: Get all incident types (flat list)
 *     security:
 *       - bearerAuth: []
 *     parameters:
 *       - in: query
 *         name: includeInactive
 *         schema:
 *           type: boolean
 *         description: Include inactive incident types
 *     responses:
 *       200:
 *         description: List of incident types
 *       401:
 *         description: Unauthorized
 */
router.get(
  "/",
  authenticateJWT,
  authorizeRole("LogisticOperator", "PortAuthorityOfficer"),
  async (req, res) => {
    try {
      // Map query param: 'true' -> 'inactive' (only inactive), undefined/false -> false (active only)
      const filter = req.query.includeInactive === "true" ? "inactive" : false;
      const result = await incidentTypeService.getAllIncidentTypesAsync(filter);

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
 * /api/oem/incident-types/hierarchy:
 *   get:
 *     tags: [Incident Types]
 *     summary: Get incident types as hierarchical tree
 *     security:
 *       - bearerAuth: []
 *     responses:
 *       200:
 *         description: Hierarchical tree of incident types
 *       401:
 *         description: Unauthorized
 */
router.get(
  "/hierarchy",
  authenticateJWT,
  authorizeRole("LogisticOperator", "PortAuthorityOfficer"),
  async (req, res) => {
    try {
      const result = await incidentTypeService.getIncidentTypesHierarchyAsync();

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
 * /api/oem/incident-types/code/{code}:
 *   get:
 *     tags: [Incident Types]
 *     summary: Get incident type by code
 *     security:
 *       - bearerAuth: []
 *     parameters:
 *       - in: path
 *         name: code
 *         required: true
 *         schema:
 *           type: string
 *     responses:
 *       200:
 *         description: Incident type details
 *       404:
 *         description: Incident type not found
 *       401:
 *         description: Unauthorized
 */
router.get(
  "/code/:code",
  authenticateJWT,
  authorizeRole("LogisticOperator", "PortAuthorityOfficer"),
  async (req, res) => {
    try {
      const result = await incidentTypeService.getIncidentTypeByCodeAsync(
        req.params.code
      );

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
 * /api/oem/incident-types/{id}:
 *   get:
 *     tags: [Incident Types]
 *     summary: Get incident type by ID
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
 *         description: Incident type details
 *       404:
 *         description: Incident type not found
 *       401:
 *         description: Unauthorized
 */
router.get(
  "/:id",
  authenticateJWT,
  authorizeRole("LogisticOperator", "PortAuthorityOfficer"),
  async (req, res) => {
    try {
      const result = await incidentTypeService.getIncidentTypeByIdAsync(
        req.params.id
      );

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
 * /api/oem/incident-types/{id}:
 *   put:
 *     tags: [Incident Types]
 *     summary: Update incident type (code is immutable)
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
 *               name:
 *                 type: string
 *                 maxLength: 255
 *               description:
 *                 type: string
 *               severity:
 *                 type: string
 *                 enum: [Minor, Major, Critical]
 *               parentId:
 *                 type: string
 *                 format: uuid
 *                 nullable: true
 *               isActive:
 *                 type: boolean
 *     responses:
 *       200:
 *         description: Incident type updated successfully
 *       400:
 *         description: Validation error
 *       404:
 *         description: Incident type not found
 *       401:
 *         description: Unauthorized
 *       403:
 *         description: Forbidden - PortAuthority role required
 */
router.put(
  "/:id",
  authenticateJWT,
  authorizeRole("PortAuthorityOfficer"),
  async (req, res) => {
    try {
      const result = await incidentTypeService.updateIncidentTypeAsync(
        req.params.id,
        req.body,
        req.user.userId
      );

      if (result.success) {
        res.status(200).json(result);
      } else {
        const statusCode =
          result.error === "Incident type not found" ? 404 : 400;
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
 * /api/oem/incident-types/{id}:
 *   delete:
 *     tags: [Incident Types]
 *     summary: Soft delete incident type
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
 *         description: Incident type deleted successfully
 *       400:
 *         description: Cannot delete - has children or is used
 *       404:
 *         description: Incident type not found
 *       401:
 *         description: Unauthorized
 *       403:
 *         description: Forbidden - PortAuthority role required
 */
router.delete(
  "/:id",
  authenticateJWT,
  authorizeRole("PortAuthorityOfficer"),
  async (req, res) => {
    try {
      const result = await incidentTypeService.deleteIncidentTypeAsync(
        req.params.id,
        req.user.userId
      );

      if (result.success) {
        res.status(200).json(result);
      } else {
        const statusCode =
          result.error === "Incident type not found" ? 404 : 400;
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
 * /api/oem/incident-types/{id}/activate:
 *   post:
 *     tags: [Incident Types]
 *     summary: Activate a deactivated incident type
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
 *         description: Incident type activated successfully
 *       400:
 *         description: Cannot activate (already active, parent inactive, etc.)
 *       404:
 *         description: Incident type not found
 *       401:
 *         description: Unauthorized
 *       403:
 *         description: Forbidden - PortAuthority role required
 */
router.post(
  "/:id/activate",
  authenticateJWT,
  authorizeRole("PortAuthorityOfficer"),
  async (req, res) => {
    try {
      const result = await incidentTypeService.activateIncidentTypeAsync(
        req.params.id,
        req.user.userId
      );

      if (result.success) {
        res.status(200).json(result);
      } else {
        const statusCode =
          result.error === "Incident type not found" ? 404 : 400;
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
