using System.Collections.Generic;
using System.Data.SQLite;
using System.IO;
using ToolManagementAppV2.Services.Core;
using ToolManagementAppV2.Services.Settings;
using ToolManagementAppV2.Interfaces;
using Xunit;

namespace ToolManagementAppV2.Tests.Services
{
    public class SettingsServiceTests
    {
        [Fact]
        public void UpdateSettings_DoesNotThrowOnFailure()
        {
            var dbPath = Path.GetTempFileName();
            try
            {
                var dbService = new DatabaseService(dbPath);
                ISettingsService service = new SettingsService(dbService);

                using (var conn = dbService.CreateConnection())
                {
                    using var cmd = new SQLiteCommand("DROP TABLE Settings", conn);
                    cmd.ExecuteNonQuery();
                }

                var settings = new Dictionary<string, string> { ["Key1"] = "Value1" };
                service.UpdateSettings(settings);
            }
            finally
            {
                if (File.Exists(dbPath))
                    File.Delete(dbPath);
            }
        }
    }
}
