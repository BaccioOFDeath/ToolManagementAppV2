# Server Deployment Guide

## Overview

This document provides specific guidance for running the Inventory Management Application full-time on a Windows server. The application has been enhanced with automated background services that run continuously.

## What's New for Server Deployment

### Automated Rental Reminder Service

The application now includes an **automated rental reminder service** that:

- **Runs automatically** when the application starts (after successful login)
- **Sends email reminders** daily at 2:30 PM to customers with rentals due the next day
- **Operates in the background** without user interaction
- **Logs all activity** for monitoring and troubleshooting

### Key Components Added

1. **EmailService**: Handles all email communications using SMTP
2. **RentalReminderService**: Scheduled background service for sending reminders
3. **Automatic startup**: Services start when a user logs in successfully
4. **Graceful degradation**: Application works normally even if email is not configured

## Prerequisites for Server Operation

### Required Configuration

For the automated reminder service to work, you must configure email settings in `appsettings.json`:

```json
{
  "Email": {
    "SmtpHost": "smtp.yourdomain.com",        // NOT smtp.example.com
    "SmtpPort": 587,
    "SmtpUsername": "your-username",
    "SmtpPassword": "your-password",
    "FromEmail": "noreply@yourdomain.com",
    "FromName": "Your Company Name",
    "EnableSsl": true,
    "ContactInfo": "Contact us at support@yourdomain.com or call (555) 123-4567"
  }
}
```

### System Requirements

- **Windows Server 2019 or later** (or Windows 10 version 1809+)
- **.NET 8.0 Desktop Runtime** (x64)
- **Reliable internet connection** for sending emails
- **Valid SMTP server credentials**

## Deployment Steps for Server

### 1. Initial Setup

Follow the standard deployment steps in [DEPLOYMENT.md](DEPLOYMENT.md), ensuring you:

1. Install .NET 8.0 Desktop Runtime
2. Copy application files to server
3. Configure `appsettings.json` with production SMTP settings
4. Run the application and complete setup wizard
5. Log in with admin credentials

### 2. Verify Reminder Service Started

After logging in, check the application logs (`Logs/app-<date>.log`) for:

```
Rental reminder service started successfully
```

If you see this message, the service is running and will send reminders at 2:30 PM daily.

If email is not configured, you'll see:
```
Email service not configured properly. Email features will be disabled.
```

### 3. Configure for Continuous Operation

#### Option A: Keep Application Running (Simplest)

1. Log into the server with a dedicated service account
2. Launch the application and log in
3. Minimize the application (don't close it)
4. Lock the session (don't log out)

**Pros**: Simple, no additional software needed
**Cons**: Application stops if server restarts or user logs out

#### Option B: Auto-Start on Login (Recommended)

1. Create a shortcut to the application
2. Place it in: `C:\ProgramData\Microsoft\Windows\Start Menu\Programs\StartUp`
3. Configure Windows to auto-login the service account
4. Test by restarting the server

**Pros**: Survives server restarts
**Cons**: Requires auto-login configuration

#### Option C: Windows Service (Production)

For production environments, consider wrapping the application as a Windows Service using tools like:
- **NSSM** (Non-Sucking Service Manager)
- **WinSW** (Windows Service Wrapper)

This provides:
- Automatic startup on server boot
- Automatic restart on failure
- No user session required
- Better management and monitoring

## Monitoring Server Operation

### Daily Checks

1. **Verify Application is Running**
   - Check Task Manager for InventoryManagementApp.exe process
   - Verify application window is visible (if not running as service)

2. **Check Log Files**
   - Location: `Logs/app-<date>.log`
   - Look for: "Rental reminder service started successfully"
   - Look for: "Checking for rentals due tomorrow..."
   - Look for: "Sent reminder for rental {ID} to {Email}"

3. **Monitor Email Sending**
   - At 2:30 PM, check logs for reminder activity
   - Verify emails are being delivered (check with test account)
   - Check for any SMTP errors in logs

### Weekly Checks

1. Review error logs for any patterns
2. Verify database backups are working
3. Check disk space usage
4. Monitor memory usage of application

### Monthly Checks

1. Test the reminder system with a test rental
2. Review and rotate application logs
3. Check for .NET runtime updates
4. Verify SMTP credentials are still valid

## Troubleshooting

### Reminder Service Not Starting

**Symptom**: No "Rental reminder service started successfully" in logs

**Possible Causes**:
1. Email not configured properly
   - Check `appsettings.json` for valid SMTP settings
   - Ensure SmtpHost is not "smtp.example.com"
   - Verify all required fields are filled

2. Application not fully started
   - Ensure user has logged in successfully
   - Service only starts after login, not at application launch

**Solution**:
1. Review logs for specific error messages
2. Verify email configuration
3. Test SMTP credentials manually
4. Restart application after fixing configuration

### Emails Not Sending

**Symptom**: Service started but no emails at 2:30 PM

**Possible Causes**:
1. No rentals due tomorrow
2. SMTP credentials incorrect
3. Firewall blocking SMTP port
4. Network connectivity issues

**Solution**:
1. Create a test rental due tomorrow
2. Test SMTP credentials using a separate tool
3. Check firewall rules (allow outbound port 587 or 465)
4. Review logs for specific SMTP errors

### Application Stops Running

**Symptom**: Application not visible in Task Manager

**Possible Causes**:
1. Server was restarted
2. User session was logged out
3. Application crashed
4. Windows updates forced restart

**Solution**:
1. Implement auto-start mechanism (see Option B or C above)
2. Check Windows Event Log for crash details
3. Review application logs before crash
4. Consider using Windows Service wrapper for reliability

## Best Practices

### Security

1. **Use dedicated service account** with minimal privileges
2. **Store SMTP passwords securely** (Windows Credential Manager)
3. **Enable SSL/TLS** for SMTP connections (EnableSsl: true)
4. **Restrict network access** to only required ports
5. **Keep .NET runtime updated** with security patches

### Reliability

1. **Set up automatic database backups** (daily or more frequent)
2. **Monitor disk space** for logs and database
3. **Implement health checks** (external monitoring)
4. **Document restart procedures** for maintenance
5. **Test failover scenarios** before production

### Performance

1. **Monitor memory usage** (typical: 100-300 MB)
2. **Review log file rotation** (14 days retention by default)
3. **Optimize database** periodically (VACUUM)
4. **Limit concurrent users** (SQLite works best with single writer)

## Configuration Examples

### Example 1: Office 365 SMTP

```json
{
  "Email": {
    "SmtpHost": "smtp.office365.com",
    "SmtpPort": 587,
    "SmtpUsername": "noreply@yourcompany.com",
    "SmtpPassword": "your-password",
    "FromEmail": "noreply@yourcompany.com",
    "FromName": "Equipment Rentals",
    "EnableSsl": true,
    "ContactInfo": "support@yourcompany.com"
  }
}
```

### Example 2: Gmail SMTP

```json
{
  "Email": {
    "SmtpHost": "smtp.gmail.com",
    "SmtpPort": 587,
    "SmtpUsername": "your-email@gmail.com",
    "SmtpPassword": "your-app-password",
    "FromEmail": "your-email@gmail.com",
    "FromName": "Equipment Rentals",
    "EnableSsl": true,
    "ContactInfo": "your-email@gmail.com"
  }
}
```

**Note**: Gmail requires an "App Password" if 2FA is enabled.

### Example 3: Custom SMTP Server

```json
{
  "Email": {
    "SmtpHost": "mail.yourcompany.com",
    "SmtpPort": 25,
    "SmtpUsername": "notifications",
    "SmtpPassword": "secure-password",
    "FromEmail": "noreply@yourcompany.com",
    "FromName": "Rental System",
    "EnableSsl": false,
    "ContactInfo": "(555) 123-4567"
  }
}
```

## Testing the Reminder Service

### Test Procedure

1. **Create Test Rental**:
   - Create a customer with a valid email address
   - Create a rental with due date = tomorrow
   - Ensure customer has an email address set

2. **Verify Service Status**:
   - Check logs for "Rental reminder service started successfully"
   - Note the time (service runs at 2:30 PM)

3. **Wait for Reminder Time**:
   - At 2:30 PM, the service will check for rentals
   - Check logs for: "Found {Count} rentals due tomorrow"
   - Check logs for: "Sent reminder for rental {ID} to {Email}"

4. **Verify Email Received**:
   - Check the customer's email inbox
   - Verify email content is correct
   - Confirm all rental details are present

### Manual Testing

You can also test the reminder system manually by:

1. Setting system time to 2:29 PM
2. Creating a rental due tomorrow
3. Waiting for 2:30 PM
4. Checking logs for reminder activity

## Support and Additional Resources

- **Deployment Guide**: [DEPLOYMENT.md](DEPLOYMENT.md) - Detailed deployment instructions
- **Production Readiness**: [PRODUCTION_READINESS.md](PRODUCTION_READINESS.md) - Complete checklist
- **Security Policy**: [SECURITY.md](SECURITY.md) - Security best practices
- **Feature Documentation**: [README.md](README.md) - Application features
- **Rental Features**: [RENTAL_ENHANCEMENTS.md](RENTAL_ENHANCEMENTS.md) - Rental system details

## Summary

The Inventory Management Application is now fully prepared for server deployment with:

✅ **Automated background services** for rental reminders
✅ **Daily reminder checks** at 2:30 PM
✅ **Graceful error handling** if email is not configured
✅ **Comprehensive logging** for monitoring and troubleshooting
✅ **Production-ready configuration** with security best practices

With proper configuration and monitoring, the application will run reliably on your server and automatically send rental reminders to customers.

---

**Version**: 1.0.0  
**Last Updated**: 2025-11-05  
**Prepared for**: Production Server Deployment
