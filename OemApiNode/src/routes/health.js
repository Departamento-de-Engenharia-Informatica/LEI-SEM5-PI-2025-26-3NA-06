const express = require("express");
const router = express.Router();
const { authenticateJWT, authorizeRole } = require("../middleware/auth");

/**
 * @swagger
 * /api/oem/health:
 *   get:
 *     summary: Health check endpoint
 *     description: Verify OEM API is accessible and properly authenticated
 *     tags: [Health]
 *     security:
 *       - bearerAuth: []
 *     responses:
 *       200:
 *         description: Service is healthy
 *         content:
 *           application/json:
 *             schema:
 *               type: object
 *               properties:
 *                 service:
 *                   type: string
 *                   example: OEM
 *                 status:
 *                   type: string
 *                   example: ok
 *                 utcNow:
 *                   type: string
 *                   format: date-time
 *                 user:
 *                   type: object
 *                   properties:
 *                     id:
 *                       type: string
 *                     username:
 *                       type: string
 *                     email:
 *                       type: string
 *                     roles:
 *                       type: array
 *                       items:
 *                         type: string
 *       401:
 *         description: Unauthorized - No valid token
 *       403:
 *         description: Forbidden - Insufficient permissions
 */
router.get(
  "/",
  authenticateJWT,
  authorizeRole("LogisticOperator"),
  (req, res) => {
    const user = req.user;

    // .NET ClaimTypes.Role is stored as this full URI in JWT
    const dotnetRoleClaim =
      "http://schemas.microsoft.com/ws/2008/06/identity/claims/role";

    // Extract roles from multiple possible claim names
    const userRoles = user.role || user.roles || user[dotnetRoleClaim] || [];

    const roles = Array.isArray(userRoles) ? userRoles : [userRoles];

    res.json({
      service: "OEM",
      status: "ok",
      utcNow: new Date().toISOString(),
      user: {
        id: user.sub || user.id || user.nameid || "unknown",
        username: user.name || user.given_name || user.email || "unknown",
        email: user.email || "unknown",
        roles: roles,
      },
    });
  }
);

module.exports = router;
