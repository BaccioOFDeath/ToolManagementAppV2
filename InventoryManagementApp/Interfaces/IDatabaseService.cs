using System;
using Microsoft.Data.Sqlite;

namespace InventoryManagementApp.Interfaces
{
    /// <summary>
    /// Provides access to create Sqlite connections.
    /// </summary>
    public interface IDatabaseService : IDisposable
    {
        /// <summary>
        /// Creates and opens a new Sqlite connection.
        /// </summary>
        SqliteConnection CreateConnection();
    }
}
