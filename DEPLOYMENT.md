# Deployment Guide

This guide provides instructions for deploying the Inventory Management Application to a production environment.

## Prerequisites

### System Requirements
- **Operating System**: Windows 10 (version 1809 or later) or Windows Server 2019 or later
- **.NET Runtime**: .NET 8.0 Desktop Runtime (x64)
- **Memory**: Minimum 2GB RAM, 4GB recommended
- **Disk Space**: 500MB for application and database
- **Display**: 1280x720 minimum resolution

### Download .NET 8.0 Desktop Runtime
Download and install from: https://dotnet.microsoft.com/en-us/download/dotnet/8.0

## Pre-Deployment Checklist

- [ ] .NET 8.0 Desktop Runtime installed on target system
- [ ] SMTP server credentials obtained (if using email notifications)
- [ ] Company branding assets prepared (logo, company information)
- [ ] Database backup strategy defined
- [ ] User accounts and permissions planned

## Deployment Steps

### 1. Build the Application

On a development machine with .NET 8 SDK installed:

```powershell
cd /path/to/ToolManagementAppV2
dotnet publish InventoryManagementApp/InventoryManagementApp.csproj -c Release -r win-x64 --self-contained false -o ./publish
```

This creates a published application in the `./publish` directory.

### 2. Prepare Configuration

Before deploying, customize the configuration:

1. Copy `appsettings.Production.json` to `appsettings.json` in the publish directory
2. Edit `appsettings.json` with your production settings:

```json
{
  "Database": {
    "Path": "inventory.db"
  },
  "Logging": {
    "Directory": "Logs"
  },
  "Email": {
    "SmtpHost": "smtp.yourdomain.com",
    "SmtpPort": 587,
    "SmtpUsername": "your-smtp-username",
    "SmtpPassword": "your-secure-password",
    "FromEmail": "noreply@yourdomain.com",
    "FromName": "Your Company Name",
    "EnableSsl": true,
    "ContactInfo": "Contact us at support@yourdomain.com or call (555) 123-4567"
  },
  "Company": {
    "Name": "Your Company Name",
    "Address": "123 Your Street, City, State ZIP",
    "Phone": "(555) 123-4567"
  }
}
```

**Important**: Keep SMTP credentials secure. Consider using Windows Credential Manager or environment variables for sensitive data.

### 3. Deploy to Target System

1. Create application directory on target system:
   ```powershell
   mkdir C:\InventoryManagement
   ```

2. Copy the entire `publish` folder contents to `C:\InventoryManagement`

3. Set appropriate folder permissions:
   - Application files: Read & Execute for users
   - Logs directory: Write permissions for application
   - Database directory: Write permissions for application

### 4. First-Time Setup

1. Launch the application: `C:\InventoryManagement\InventoryManagementApp.exe`

2. The **Setup Wizard** will guide you through initial configuration:
   - Set administrator password (minimum 8 characters, must include uppercase, lowercase, and digit)
   - Configure application name
   - Set item terminology (singular/plural)
   - Upload company logo (optional)

3. The default admin username is `admin`

### 5. Post-Deployment Configuration

After initial setup, configure additional settings through the application:

1. **User Management**: Create user accounts with appropriate permissions
2. **Rental Configuration**: Set rental rates, late fees, and email settings
3. **Categories**: Define item categories for your inventory
4. **Company Settings**: Verify company information for invoices and documents

## Security Considerations

### Password Requirements
- Minimum 8 characters
- Must contain uppercase letter, lowercase letter, and digit
- Default passwords (admin, changeme, newpassword) are automatically flagged as expired

### Database Security
- SQLite database file (`inventory.db`) contains sensitive data
- Store in a directory with restricted permissions
- Regular backups recommended (see Backup Strategy below)
- Database connections use shared cache mode for performance

### SMTP Credentials
- Never commit `appsettings.json` with real credentials to source control
- Use strong, unique passwords for SMTP authentication
- Enable SSL/TLS for SMTP connections (EnableSsl: true)

## Backup Strategy

### Database Backups

The application includes built-in backup functionality:

1. **Manual Backup**: Use the application's backup feature (File > Backup Database)
2. **Automated Backup**: Schedule Windows Task to copy `inventory.db` regularly

Example PowerShell script for automated backup:

```powershell
$source = "C:\InventoryManagement\inventory.db"
$destination = "C:\Backups\inventory_$(Get-Date -Format 'yyyyMMdd_HHmmss').db"
Copy-Item -Path $source -Destination $destination
```

### Log Files

Logs are stored in the `Logs` directory:
- Rotated daily
- Retained for 14 days by default
- Review logs regularly for errors or security issues

## Troubleshooting

### Application Won't Start
- Verify .NET 8.0 Desktop Runtime is installed
- Check Windows Event Log for error details
- Review application logs in `Logs\app-<date>.log`

### Database Locked Errors
- Ensure only one instance of the application is running
- Check file permissions on `inventory.db`
- Verify antivirus isn't blocking database file

### Email Notifications Not Sending
- Verify SMTP settings in `appsettings.json`
- Test SMTP credentials separately
- Check firewall allows outbound connections on SMTP port
- Review logs for detailed error messages

### Performance Issues
- Monitor memory usage (default budget: configurable)
- Consider scheduled database maintenance (VACUUM)
- Ensure adequate disk space for logs and database

## Updating the Application

To update to a new version:

1. **Backup Database**: Create backup of `inventory.db`
2. **Stop Application**: Ensure application is closed
3. **Backup Configuration**: Save `appsettings.json`
4. **Deploy New Version**: Replace application files
5. **Restore Configuration**: Copy back `appsettings.json`
6. **Test**: Launch application and verify functionality

Database migrations run automatically on startup.

## Monitoring

### Health Checks
- Monitor application logs daily
- Review user activity logs
- Check database size growth
- Verify automated email reminders are sending

### Key Log Locations
- Application logs: `Logs\app-<date>.log`
- Windows Event Log: Application section
- Database location: As specified in `appsettings.json`

## Running Full-Time on a Server

The application is designed to run continuously on a server and includes automated background services.

### Automated Rental Reminders

When properly configured, the application automatically sends email reminders:
- Runs daily at 2:30 PM
- Sends reminders 24 hours before rental due dates
- Requires valid SMTP configuration in `appsettings.json`

The reminder service starts automatically when:
1. Email settings are properly configured (not using example.com)
2. A user successfully logs into the application

### Ensuring Continuous Operation

For reliable server deployment:

1. **Keep Application Running**: The application must remain running for reminders to be sent
   - Consider using Task Scheduler to auto-start on login
   - Use auto-login on the server for unattended operation
   - Keep the application minimized to system tray if desired

2. **Monitor Service Status**: Check logs regularly for:
   ```
   "Rental reminder service started successfully"
   ```
   This confirms the service is running.

3. **Email Configuration**: Verify in `appsettings.json`:
   - `SmtpHost` is not `smtp.example.com`
   - All SMTP credentials are valid
   - `SmtpPort`, `SmtpUsername`, `SmtpPassword` are set
   - `FromEmail` and `ContactInfo` are configured

4. **Test Email Functionality**: After deployment, verify emails are sending by:
   - Checking application logs for successful email sends
   - Creating a test rental due tomorrow
   - Waiting for the 2:30 PM reminder cycle

### Server-Specific Considerations

**Windows Server Deployment**:
- Install .NET 8.0 Desktop Runtime (includes Windows Forms support)
- Configure Windows to allow the application through the firewall
- Set up automatic user login if running unattended
- Consider using Windows Service wrapper (e.g., NSSM) for production environments

**High Availability**:
- Multiple instances can run simultaneously using SQLite WAL mode
- ⚠️ **Important**: Only ONE instance should have email configured to avoid duplicate reminder emails
- For multi-user deployments, disable email on client instances (set SmtpHost to "smtp.example.com")
- Implement external monitoring (e.g., ping endpoint, log monitoring)
- Set up database backup automation
- Document restart procedures for maintenance windows

**Security**:
- Run with least-privilege user account
- Store SMTP credentials securely (Windows Credential Manager)
- Restrict network access to required ports only
- Keep .NET runtime updated with security patches

## Support

For issues or questions:
1. Check application logs for error details
2. Review this deployment guide
3. Consult the main [README.md](README.md) for feature documentation
4. See [RENTAL_ENHANCEMENTS.md](RENTAL_ENHANCEMENTS.md) for rental features

## Environment-Specific Configurations

### Development
- Uses `appsettings.json` with example values
- Debug logging enabled via separate configuration

### Production
- Uses production SMTP and company settings
- Information-level logging
- Secure credential storage

### Testing/Staging
- Can use separate database file
- Test SMTP server or email sandbox services
- Consider using test data generators
