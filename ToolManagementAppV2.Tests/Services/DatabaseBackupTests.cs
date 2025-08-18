using System;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using ToolManagementAppV2.Services.Core;
using Xunit;

namespace ToolManagementAppV2.Tests.Services
{
    public class DatabaseBackupTests
    {
        [Fact]
        public async Task BackupDatabase_PathWithSemicolon_DoesNotThrow()
        {
            var dbPath = Path.GetTempFileName();
            var backupPath1 = Path.Combine(Path.GetTempPath(), $"backup;{Guid.NewGuid():N}.db");
            var backupPath2 = Path.Combine(Path.GetTempPath(), $"backup;{Guid.NewGuid():N}.db");
            try
            {
                var service = new DatabaseService(dbPath);

                var ex1 = Record.Exception(() => service.BackupDatabase(backupPath1));
                Assert.Null(ex1);
                Assert.True(File.Exists(backupPath1));

                var ex2 = await Record.ExceptionAsync(() => service.BackupDatabaseAsync(backupPath2, CancellationToken.None));
                Assert.Null(ex2);
                Assert.True(File.Exists(backupPath2));
            }
            finally
            {
                if (File.Exists(dbPath))
                    File.Delete(dbPath);
                if (File.Exists(backupPath1))
                    File.Delete(backupPath1);
                if (File.Exists(backupPath2))
                    File.Delete(backupPath2);
            }
        }
    }
}
