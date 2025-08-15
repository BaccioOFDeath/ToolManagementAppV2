using System.IO;
using ToolManagementAppV2.Services.Core;
using Xunit;

namespace ToolManagementAppV2.Tests.Services
{
    public class DatabaseIndexTests
    {
        [Fact]
        public void InitializeDatabase_CreatesIndexes()
        {
            var dbPath = Path.GetTempFileName();
            try
            {
                var db = new DatabaseService(dbPath);
                Assert.True(SqliteHelper.IndexExists(db.ConnectionString, "idx_Tools_ToolNumber"));
                Assert.True(SqliteHelper.IndexExists(db.ConnectionString, "idx_Users_UserName"));
                Assert.True(SqliteHelper.IndexExists(db.ConnectionString, "idx_Customers_Contact"));
                Assert.True(SqliteHelper.IndexExists(db.ConnectionString, "idx_Rentals_ToolID_CustomerID"));
                Assert.True(SqliteHelper.IndexExists(db.ConnectionString, "idx_Tools_Keywords"));
            }
            finally
            {
                if (File.Exists(dbPath))
                    File.Delete(dbPath);
            }
        }
    }
}
