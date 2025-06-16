using System;
using System.IO;
using System.Reflection;
using System.Threading.Tasks;
using ToolManagementAppV2.Services.Core;
using Xunit;

namespace ToolManagementAppV2.Tests.Services
{
    public class DatabaseServiceConcurrentTests
    {
        [Fact]
        public void EnsureColumn_ConcurrentCalls_NoDuplicateException()
        {
            var dbPath = Path.GetTempFileName();
            try
            {
                var method = typeof(DatabaseService).GetMethod("EnsureColumn", BindingFlags.NonPublic | BindingFlags.Instance);
                var t1 = Task.Run(() =>
                {
                    var db = new DatabaseService(dbPath);
                    method.Invoke(db, new object[] { "Users", "ConcurrentCol", "TEXT" });
                });
                var t2 = Task.Run(() =>
                {
                    var db = new DatabaseService(dbPath);
                    method.Invoke(db, new object[] { "Users", "ConcurrentCol", "TEXT" });
                });

                var ex = Record.Exception(() => Task.WaitAll(t1, t2));
                Assert.Null(ex);

                var checkDb = new DatabaseService(dbPath);
                Assert.True(SqliteHelper.ColumnExists(checkDb.ConnectionString, "Users", "ConcurrentCol"));
            }
            finally
            {
                if (File.Exists(dbPath)) File.Delete(dbPath);
            }
        }
    }
}
