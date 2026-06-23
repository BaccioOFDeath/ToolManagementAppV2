# InventoryManagementApp Server Deployment Guide

This guide covers running InventoryManagementApp continuously on a Windows machine for shared rental operations and automated email reminders.

## Purpose

InventoryManagementApp is a WPF desktop application. A server-style deployment can be useful when one workstation or Windows Server instance should stay signed in, keep the SQLite database available, and send scheduled rental reminder emails.

## Requirements

- Windows Server 2019 or later, or Windows 10 version 1809 or later.
- .NET 10 Desktop Runtime x64.
- A writable SQLite database location.
- SMTP settings when email reminders should be enabled.
- Appropriate file permissions for the application folder, database file, and log folder.

## Reminder Service

The rental reminder service starts after a successful login when email configuration is valid. It checks for rentals due the next day and sends reminder emails using the configured SMTP account.

If SMTP settings are incomplete or intentionally left as example values, the rest of the application should continue to run and email reminders remain disabled.

Example email configuration:

```json
{
  "Email": {
    "SmtpHost": "smtp.yourdomain.com",
    "SmtpPort": 587,
    "SmtpUsername": "your-username",
    "SmtpPassword": "your-password",
    "FromEmail": "noreply@yourdomain.com",
    "FromName": "Inventory Rentals",
    "EnableSsl": true,
    "ContactInfo": "Contact support@yourdomain.com"
  }
}
```

Do not commit production credentials.

## Shared Database Deployment

InventoryManagementApp uses SQLite. SQLite supports multiple readers and one writer at a time. For shared use, prefer a reliable local or network path with stable connectivity.

Example database configuration:

```json
{
  "Database": {
    "Path": "\\\\fileserver\\shared\\InventoryManagementApp\\inventory.db"
  }
}
```

For mapped drives:

```json
{
  "Database": {
    "Path": "Z:\\InventoryManagementApp\\inventory.db"
  }
}
```

## Avoid Duplicate Reminder Emails

Each running application instance can start its own reminder service when SMTP is configured. In a multi-user deployment, configure exactly one always-on instance with real SMTP settings.

Recommended setup:

- One server or designated workstation has SMTP enabled.
- Other workstations point to the shared database but leave SMTP disabled by using example values or blank credentials.
- All users share the same SQLite database path.

## Continuous Operation Options

### Keep One Session Running

1. Sign in with a dedicated Windows account.
2. Start InventoryManagementApp.
3. Log in to the application.
4. Leave the session locked rather than signed out.

### Auto-Start on Login

1. Create a shortcut to the application.
2. Place it in the Windows Startup folder for the service account.
3. Configure the Windows account and restart policy according to local IT requirements.
4. Restart and confirm the application launches and logs in as expected.

### Windows Service Wrapper

For stricter production operations, use a wrapper such as NSSM or WinSW to supervise the desktop executable. Validate printing, dialogs, and reminder behavior carefully before relying on this approach, because WPF applications are normally interactive desktop processes.

## Monitoring

Daily checks:

- Confirm the process is running.
- Review `Logs/app-<date>.log`.
- Confirm reminder-service startup messages when SMTP is enabled.
- Check for SMTP errors, database lock warnings, or unhandled exceptions.

Weekly checks:

- Verify database backups.
- Review disk space for the database and logs.
- Confirm reminder emails are not duplicated.
- Test check-in and checkout on the shared database path.

## Troubleshooting

### Reminder Service Does Not Start

Check that:

- The user has logged in successfully.
- `Email:SmtpHost` is not an example host.
- SMTP username, password, sender address, and SSL settings are valid.
- The log folder is writable.

### Emails Do Not Send

Check that:

- At least one rental is due tomorrow.
- The customer has an email address.
- The SMTP account accepts the configured port and SSL mode.
- Local firewall or network policy allows outbound SMTP.

### Database Lock Messages Appear

Check that:

- The database path is stable and reachable.
- Network connectivity is reliable.
- Large imports or reports are not running repeatedly during peak checkout activity.
- Users retry the operation after a short wait.

## Related Docs

- [README.md](README.md)
- [DEPLOYMENT.md](DEPLOYMENT.md)
- [SECURITY.md](SECURITY.md)
- [ToDo.md](ToDo.md)
