# Security Policy

## Supported Versions

The following versions of the Inventory Management Application are currently supported with security updates:

| Version | Supported          |
| ------- | ------------------ |
| 1.0.x   | :white_check_mark: |

## Reporting a Vulnerability

We take the security of the Inventory Management Application seriously. If you discover a security vulnerability, please follow these steps:

### How to Report

1. **Do Not** open a public GitHub issue for security vulnerabilities
2. Send an email to the repository owner with details of the vulnerability
3. Include the following information in your report:
   - Description of the vulnerability
   - Steps to reproduce the issue
   - Potential impact
   - Suggested fix (if applicable)

### What to Expect

- **Acknowledgment**: We will acknowledge receipt of your vulnerability report within 48 hours
- **Assessment**: We will investigate and assess the severity of the issue
- **Updates**: We will keep you informed of the progress toward resolving the issue
- **Resolution**: We will work to release a fix as soon as possible
- **Credit**: With your permission, we will credit you in the security advisory

### Security Best Practices for Deployment

When deploying the Inventory Management Application, follow these security best practices:

#### 1. Password Security
- Change the default admin password immediately after setup
- Enforce strong passwords (8+ characters, mixed case, numbers)
- Never use default passwords like "admin", "changeme", or "newpassword"
- Rotate passwords periodically

#### 2. Database Security
- Store the SQLite database file in a secure location
- Set appropriate file system permissions (restrict to application user)
- Enable encryption at rest using Windows BitLocker or similar
- Perform regular backups and store them securely
- Never commit database files to version control

#### 3. Configuration Security
- Keep appsettings.json secure and restrict file access
- Never commit production credentials to source control
- Use Windows Credential Manager for sensitive credentials
- Validate all configuration values at deployment
- Use strong SMTP passwords and enable SSL/TLS

#### 4. Network Security
- Enable firewall rules to restrict network access
- Use SSL/TLS for all email communications (EnableSsl: true)
- Monitor network traffic for unusual patterns
- Keep SMTP ports restricted to necessary connections

#### 5. System Security
- Keep Windows operating system updated with security patches
- Keep .NET 8.0 runtime updated with latest security releases
- Run the application with minimum required privileges
- Monitor application logs for security events
- Review user access permissions regularly

#### 6. Application Security
- Review logs regularly for failed login attempts
- Monitor for unusual user activity patterns
- Keep audit logs of administrative actions
- Implement account lockout after repeated failed logins
- Use the latest version of the application

### Known Security Considerations

#### SQLite Database
- The SQLite database stores sensitive customer and rental information
- Database file should be protected with file system permissions
- Consider enabling database encryption for highly sensitive deployments
- Regular backups should also be secured appropriately

#### SMTP Credentials
- SMTP credentials are stored in appsettings.json
- Production deployments should secure this file
- Consider using Windows Credential Manager for credential storage
- Rotate SMTP passwords periodically

#### User Authentication
- Passwords are hashed using PBKDF2-SHA256 with 100,000+ iterations
- Legacy password hashes are automatically upgraded on login
- Default/weak passwords trigger automatic expiration
- No session timeout is enforced by default (consider implementing for high-security deployments)

### Security Features

The application includes the following built-in security features:

- ✅ Strong password hashing (PBKDF2-SHA256)
- ✅ Configurable password iteration count
- ✅ Password complexity requirements
- ✅ Automatic password expiration for weak passwords
- ✅ User authentication and authorization
- ✅ Role-based access control (admin vs. regular users)
- ✅ Activity logging for audit trails
- ✅ Global exception handling (prevents information disclosure)
- ✅ SSL/TLS support for email
- ✅ Configuration validation at startup

### Security Updates

Security updates will be released as patch versions (e.g., 1.0.1, 1.0.2) and announced through:
- GitHub Security Advisories
- Repository release notes
- CHANGELOG.md

Monitor the repository for security updates and apply them promptly.

### Compliance

This application is designed for general inventory management purposes. If you need to comply with specific regulations (GDPR, HIPAA, PCI-DSS, etc.), additional security controls may be required beyond what is provided by default.

### Questions?

If you have questions about security but have not discovered a vulnerability, please open a regular GitHub issue with the "security" label.

---

**Last Updated:** 2025-11-04  
**Application Version:** 1.0.0
