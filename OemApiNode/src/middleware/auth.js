const jwt = require("jsonwebtoken");
const logger = require("../utils/logger");

const authenticateJWT = (req, res, next) => {
  const authHeader = req.headers.authorization;

  if (!authHeader) {
    return res.status(401).json({ message: "No authorization token provided" });
  }

  const token = authHeader.split(" ")[1]; // Bearer <token>

  try {
    const decoded = jwt.verify(token, process.env.JWT_SECRET, {
      issuer: process.env.JWT_ISSUER,
      audience: process.env.JWT_AUDIENCE,
    });

    req.user = decoded;
    logger.info(`User authenticated: ${decoded.email || decoded.sub}`);
    logger.info(`Token claims: ${JSON.stringify(decoded, null, 2)}`);
    next();
  } catch (error) {
    logger.error("JWT verification failed:", error.message);
    return res.status(403).json({ message: "Invalid or expired token" });
  }
};

const authorizeRole = (...allowedRoles) => {
  return (req, res, next) => {
    if (!req.user) {
      return res.status(401).json({ message: "Unauthorized" });
    }

    // .NET ClaimTypes.Role is stored as this full URI in JWT
    const dotnetRoleClaim =
      "http://schemas.microsoft.com/ws/2008/06/identity/claims/role";

    // Check multiple possible role claim names
    const userRoles =
      req.user.role || req.user.roles || req.user[dotnetRoleClaim] || [];

    const roles = Array.isArray(userRoles) ? userRoles : [userRoles];

    logger.info(
      `User roles: ${JSON.stringify(roles)}, Required: ${allowedRoles.join(
        ", "
      )}`
    );

    const hasRole = allowedRoles.some((role) => roles.includes(role));

    if (!hasRole) {
      logger.warn(
        `Access denied for user ${req.user.email}: Has roles [${roles.join(
          ", "
        )}], Required: [${allowedRoles.join(", ")}]`
      );
      return res.status(403).json({
        message: "Insufficient permissions",
        requiredRoles: allowedRoles,
        userRoles: roles,
      });
    }

    next();
  };
};

module.exports = { authenticateJWT, authorizeRole };
