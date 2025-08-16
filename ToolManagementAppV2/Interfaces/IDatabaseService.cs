using System;
using System.Data.SQLite;

namespace ToolManagementAppV2.Interfaces
{
    /// <summary>
    /// Provides access to create SQLite connections.
    /// </summary>
    public interface IDatabaseService : IDisposable
    {
        /// <summary>
        /// Creates and opens a new SQLite connection.
        /// </summary>
        SQLiteConnection CreateConnection();
    }
}
