using System.Collections.Generic;
using Xunit;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Moq;
using InventoryManagementApp.Utilities;

namespace InventoryManagementApp.Tests
{
    public class ConfigurationValidatorTests
    {
        [Fact]
        public void Validate_ValidConfiguration_ReturnsNoErrors()
        {
            // Arrange
            var config = new ConfigurationBuilder()
                .AddInMemoryCollection(new Dictionary<string, string?>
                {
                    ["Database:Path"] = "inventory.db",
                    ["Logging:Directory"] = "Logs",
                    ["Email:SmtpHost"] = "smtp.company.com",
                    ["Email:SmtpPort"] = "587",
                    ["Company:Name"] = "My Company"
                })
                .Build();

            var logger = new Mock<ILogger<ConfigurationValidator>>();
            var validator = new ConfigurationValidator(config, logger.Object);

            // Act
            var errors = validator.Validate();

            // Assert
            Assert.Empty(errors);
        }

        [Fact]
        public void Validate_MissingDatabasePath_ReturnsError()
        {
            // Arrange
            var config = new ConfigurationBuilder()
                .AddInMemoryCollection(new Dictionary<string, string?>
                {
                    ["Logging:Directory"] = "Logs"
                })
                .Build();

            var logger = new Mock<ILogger<ConfigurationValidator>>();
            var validator = new ConfigurationValidator(config, logger.Object);

            // Act
            var errors = validator.Validate();

            // Assert
            Assert.Single(errors);
            Assert.Contains("Database:Path", errors[0]);
        }

        [Fact]
        public void Validate_EmptyDatabasePath_ReturnsError()
        {
            // Arrange
            var config = new ConfigurationBuilder()
                .AddInMemoryCollection(new Dictionary<string, string?>
                {
                    ["Database:Path"] = "",
                    ["Logging:Directory"] = "Logs"
                })
                .Build();

            var logger = new Mock<ILogger<ConfigurationValidator>>();
            var validator = new ConfigurationValidator(config, logger.Object);

            // Act
            var errors = validator.Validate();

            // Assert
            Assert.Single(errors);
            Assert.Contains("Database:Path", errors[0]);
        }

        [Fact]
        public void Validate_MissingLoggingDirectory_LogsWarning()
        {
            // Arrange
            var config = new ConfigurationBuilder()
                .AddInMemoryCollection(new Dictionary<string, string?>
                {
                    ["Database:Path"] = "inventory.db"
                })
                .Build();

            var logger = new Mock<ILogger<ConfigurationValidator>>();
            var validator = new ConfigurationValidator(config, logger.Object);

            // Act
            var errors = validator.Validate();

            // Assert
            Assert.Empty(errors); // Should not be a critical error
            logger.Verify(
                x => x.Log(
                    LogLevel.Warning,
                    It.IsAny<EventId>(),
                    It.Is<It.IsAnyType>((v, t) => v.ToString()!.Contains("Logging:Directory")),
                    It.IsAny<Exception>(),
                    It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
                Times.Once);
        }

        [Fact]
        public void Validate_ExampleEmailConfiguration_LogsWarning()
        {
            // Arrange
            var config = new ConfigurationBuilder()
                .AddInMemoryCollection(new Dictionary<string, string?>
                {
                    ["Database:Path"] = "inventory.db",
                    ["Email:SmtpHost"] = "smtp.example.com"
                })
                .Build();

            var logger = new Mock<ILogger<ConfigurationValidator>>();
            var validator = new ConfigurationValidator(config, logger.Object);

            // Act
            var errors = validator.Validate();

            // Assert
            Assert.Empty(errors); // Email is optional, so no critical error
            logger.Verify(
                x => x.Log(
                    LogLevel.Warning,
                    It.IsAny<EventId>(),
                    It.Is<It.IsAnyType>((v, t) => v.ToString()!.Contains("Email:SmtpHost")),
                    It.IsAny<Exception>(),
                    It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
                Times.Once);
        }

        [Fact]
        public void Validate_DefaultCompanyName_LogsInformation()
        {
            // Arrange
            var config = new ConfigurationBuilder()
                .AddInMemoryCollection(new Dictionary<string, string?>
                {
                    ["Database:Path"] = "inventory.db",
                    ["Company:Name"] = "Equipment Rentals"
                })
                .Build();

            var logger = new Mock<ILogger<ConfigurationValidator>>();
            var validator = new ConfigurationValidator(config, logger.Object);

            // Act
            var errors = validator.Validate();

            // Assert
            Assert.Empty(errors);
            logger.Verify(
                x => x.Log(
                    LogLevel.Information,
                    It.IsAny<EventId>(),
                    It.Is<It.IsAnyType>((v, t) => v.ToString()!.Contains("Company:Name")),
                    It.IsAny<Exception>(),
                    It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
                Times.Once);
        }
    }
}
