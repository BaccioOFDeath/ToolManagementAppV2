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

        [Fact]
        public void EnsureIndex_ConcurrentCalls_NoDuplicateException()
        {
            var dbPath = Path.GetTempFileName();
            try
            {
                var colMethod = typeof(DatabaseService).GetMethod("EnsureColumn", BindingFlags.NonPublic | BindingFlags.Instance);
                var idxMethod = typeof(DatabaseService).GetMethod("EnsureIndex", BindingFlags.NonPublic | BindingFlags.Instance);

                var db = new DatabaseService(dbPath);
                colMethod.Invoke(db, new object[] { "Items", "ConcurrentIdxCol", "TEXT" });

                var t1 = Task.Run(() =>
                {
                    var d = new DatabaseService(dbPath);
                    using var conn = d.CreateConnection();
                    idxMethod.Invoke(d, new object[] { conn, "Items", "ConcurrentIdxCol", false });
                });
                var t2 = Task.Run(() =>
                {
                    var d = new DatabaseService(dbPath);
                    using var conn = d.CreateConnection();
                    idxMethod.Invoke(d, new object[] { conn, "Items", "ConcurrentIdxCol", false });
                });

                var ex = Record.Exception(() => Task.WaitAll(t1, t2));
                Assert.Null(ex);

                using var checkDb = new DatabaseService(dbPath);
                using var checkConn = checkDb.CreateConnection();
                Assert.True(SqliteHelper.IndexExists(checkConn, "idx_Items_ConcurrentIdxCol"));
            }
            finally
            {
                if (File.Exists(dbPath)) File.Delete(dbPath);
            }
        }
    }
}
