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
    "Path": "\\\\fileserver\\shared\\InventoryManagementApp\\Data\\inventory.db",
    "UseWalJournal": false,
    "SecureFilePermissions": false
  }
}
```

For mapped drives:

```json
{
  "Database": {
    "Path": "Z:\\InventoryManagementApp\\Data\\inventory.db",
    "UseWalJournal": false,
    "SecureFilePermissions": false
  }
}
```

When the resolved database path is on a UNC share or mapped network drive, the application defaults `UseWalJournal` and `SecureFilePermissions` to `false`. Keep those settings explicit in deployed `appsettings.json` files so each workstation behaves the same way.

If you copy the release folder to a shared directory and run the app from more than one computer, put `inventory.db` under the shared folder and point every workstation at the same path. Do not copy separate `Data/inventory.db` files to each computer unless each workstation should have its own independent inventory.


## Updating While Users Are Active

Windows locks executable files and loaded DLLs while users are running a WPF application from a shared folder, so a traditional in-place overwrite still requires everyone to close InventoryManagementApp first. To make updates possible during the workday, publish each build side-by-side and move new launches to the new release while existing users finish in the old copy.

Recommended active-user update flow:

1. Publish the new build to a clean local folder such as `publish-clean`.
2. Stage the build without touching files active users already have loaded:

   ```powershell
   ./scripts/update-shared-release.ps1 -Source ./publish-clean -Destination X:\V2 -DeploymentMode SideBySide -ReleaseName 2026.06.25-1
   ```

   Use a folder-safe release name that does not end with a dot or space. Avoid Windows reserved device names such as `CON`, `NUL`, `COM1`, or `LPT1`, because those names are invalid deployment folder targets on Windows.

   The update script also refreshes the launcher at `X:\V2\scripts\start-current-release.ps1` so shared shortcuts can target the deployed folder instead of depending on a repository checkout.
3. Point the shared shortcut, launcher script, or deployment utility at the current-release launcher instead of at a specific release executable:

   ```powershell
   powershell.exe -ExecutionPolicy Bypass -File X:\V2\scripts\start-current-release.ps1 -Destination X:\V2
   ```

   `start-current-release.ps1` reads `X:\V2\current-release.txt`, starts `X:\V2\_releases\<ReleaseName>\InventoryManagementApp.exe`, and falls back to `X:\V2\InventoryManagementApp.exe` when no marker exists for an in-place deployment.
4. Ask users to restart the app when convenient. Users already running the previous release can continue current rentals/check-ins because the SQLite database and preserved asset folders remain shared.
5. After confirming nobody is running the older release folder, archive or delete old folders under `_releases`.

In side-by-side mode, the script copies the preserved `appsettings.json` into the staged release and links the release-local data, photo, theme, and log folders back to the shared destination folders. That keeps versioned binaries isolated while the operational SQLite database, uploaded photos, theme files, and logs continue to use the shared location.

Use the default in-place mode only for maintenance windows when all users are closed:

```powershell
./scripts/update-shared-release.ps1 -Source ./publish-clean -Destination X:\V2
```

Side-by-side deployment avoids replacing locked application binaries, but it does not make database schema changes magically compatible with old clients. If a release includes a database migration that older app versions cannot safely use, schedule a normal maintenance window and have everyone close the app before launching the new release.

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