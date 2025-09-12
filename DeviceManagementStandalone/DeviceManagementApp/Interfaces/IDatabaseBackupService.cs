using System.Threading;
using System.Threading.Tasks;

namespace DeviceManagementApp.Interfaces
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
        /// <param name="cancellationToken">Token to observe for cancellation.</param>
        Task BackupDatabaseAsync(string backupFilePath, CancellationToken cancellationToken);
    }
}
