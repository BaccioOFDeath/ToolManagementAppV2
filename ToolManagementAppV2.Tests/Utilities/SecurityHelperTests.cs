using System;
using System.IO;
using System.Security.Cryptography;
using System.Text;
using ToolManagementAppV2.Services.Core;
using ToolManagementAppV2.Services.Settings;
using ToolManagementAppV2.Utilities.Helpers;
using ToolManagementAppV2.Interfaces;
using Xunit;

namespace ToolManagementAppV2.Tests.Utilities
{
    public class SecurityHelperTests
    {
        [Fact]
        public void HashPassword_UsesConfiguredIterations()
        {
            var dbPath = Path.GetTempFileName();
            try
            {
                var dbService = new DatabaseService(dbPath);
                ISettingsService settings = new SettingsService(dbService);
                settings.SavePasswordIterations(5);
                SecurityHelper.SettingsService = settings;

                var saltBytes = Encoding.UTF8.GetBytes("1234567890ABCDEF");
                var salt = Convert.ToBase64String(saltBytes);

                using var pbkdf2 = new Rfc2898DeriveBytes("secret", saltBytes, 5, HashAlgorithmName.SHA256);
                var expected = Convert.ToBase64String(pbkdf2.GetBytes(32));
                var actual = SecurityHelper.HashPassword("secret", salt);
                Assert.Equal(expected, actual);
            }
            finally
            {
                SecurityHelper.SettingsService = null;
                if (File.Exists(dbPath))
                    File.Delete(dbPath);
            }
        }

        [Fact]
        public void HashPassword_DefaultIterationsWhenNotConfigured()
        {
            SecurityHelper.SettingsService = null;
            var saltBytes = Encoding.UTF8.GetBytes("1234567890ABCDEF");
            var salt = Convert.ToBase64String(saltBytes);

            using var pbkdf2 = new Rfc2898DeriveBytes("secret", saltBytes, 100_000, HashAlgorithmName.SHA256);
            var expected = Convert.ToBase64String(pbkdf2.GetBytes(32));
            var actual = SecurityHelper.HashPassword("secret", salt);
            Assert.Equal(expected, actual);
        }
    }
}
