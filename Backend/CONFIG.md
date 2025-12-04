# Configuration Setup

## Secrets Management

This project uses `appsettings.Development.json` for local development secrets. This file is **not committed** to the repository for security reasons.

### Setup Instructions

1. Copy `appsettings.example.json` to `appsettings.Development.json`:

   ```bash
   cp appsettings.example.json appsettings.Development.json
   ```

2. Update `appsettings.Development.json` with your actual credentials:
   - **ConnectionStrings:DefaultConnection**: Your database connection string
   - **GoogleKeys:ClientId**: Your Google OAuth Client ID
   - **GoogleKeys:ClientSecret**: Your Google OAuth Client Secret

### Required Configuration Keys

- `ConnectionStrings:DefaultConnection` - Database connection string (Azure SQL or local)
- `GoogleKeys:ClientId` - Google OAuth 2.0 Client ID
- `GoogleKeys:ClientSecret` - Google OAuth 2.0 Client Secret

### Security Notes

- Never commit `appsettings.Development.json` or any file containing secrets
- The `.gitignore` file is configured to exclude these files
- Use Azure Key Vault or similar for production environments
