using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;

namespace InventoryManagementApp.Utilities
{
    /// <summary>
    /// Validates application configuration at startup to ensure required settings are present.
    /// </summary>
    public class ConfigurationValidator
    {
        private const string DefaultCompanyName = "Equipment Rentals";
        private const string ExampleDomain = "example.com";
        
        private readonly IConfiguration _configuration;
        private readonly ILogger<ConfigurationValidator> _logger;

        public ConfigurationValidator(IConfiguration configuration, ILogger<ConfigurationValidator> logger)
        {
            _configuration = configuration ?? throw new ArgumentNullException(nameof(configuration));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        /// <summary>
        /// Validates the configuration and returns any validation errors.
        /// </summary>
        /// <returns>List of validation errors, or empty list if valid.</returns>
        public List<string> Validate()
        {
            var errors = new List<string>();

            // Validate Database configuration
            var dbPath = _configuration["Database:Path"];
            if (string.IsNullOrWhiteSpace(dbPath))
            {
                errors.Add("Database:Path is not configured.");
            }

            // Validate Logging configuration
            var logsDir = _configuration["Logging:Directory"];
            if (string.IsNullOrWhiteSpace(logsDir))
            {
                _logger.LogWarning("Logging:Directory is not configured, using default 'Logs'.");
            }

            // Validate Email configuration (warnings only, as email is optional)
            var smtpHost = _configuration["Email:SmtpHost"];
            if (string.IsNullOrWhiteSpace(smtpHost) || smtpHost.Contains(ExampleDomain))
            {
                _logger.LogWarning("Email:SmtpHost is not properly configured. Email features will not work until configured.");
            }

            var smtpPort = _configuration["Email:SmtpPort"];
            if (string.IsNullOrWhiteSpace(smtpPort))
            {
                _logger.LogWarning("Email:SmtpPort is not configured.");
            }

            // Validate Company configuration (informational)
            var companyName = _configuration["Company:Name"];
            if (string.IsNullOrWhiteSpace(companyName) || companyName.Contains(DefaultCompanyName))
            {
                _logger.LogInformation("Company:Name uses default value. Consider customizing for your organization.");
            }

            if (errors.Any())
            {
                _logger.LogError("Configuration validation failed with {ErrorCount} error(s).", errors.Count);
                foreach (var error in errors)
                {
                    _logger.LogError("Configuration error: {Error}", error);
                }
            }
            else
            {
                _logger.LogInformation("Configuration validation passed.");
            }

            return errors;
        }
    }
}
