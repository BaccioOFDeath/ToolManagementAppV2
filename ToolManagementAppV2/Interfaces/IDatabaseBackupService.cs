using System.Threading.Tasks;

namespace ToolManagementAppV2.Interfaces
{
    /// <summary>
    /// Provides database backup capabilities.
    /// </summary>
    public interface IDatabaseBackupService
    {
        /// <summary>
        /// Creates a backup of the current database asynchronously.
        /// </summary>
        /// <param name="backupFilePath">Destination path for the backup file.</param>
        Task BackupDatabaseAsync(string backupFilePath);
    }
}
