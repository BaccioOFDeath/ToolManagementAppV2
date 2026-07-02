using System;
using System.Threading;
using System.Threading.Tasks;

namespace InventoryManagementApp.Interfaces
{
    /// <summary>
    /// Provides database and application recovery capabilities.
    /// </summary>
    public interface IDatabaseBackupService
    {
        /// <summary>
        /// Creates a backup of the current database asynchronously.
        /// </summary>
        /// <param name="backupFilePath">Destination path for the backup file.</param>
        /// <param name="cancellationToken">Token to observe for cancellation.</param>
        Task BackupDatabaseAsync(string backupFilePath, CancellationToken cancellationToken);

        /// <summary>
        /// Creates a full application backup package containing the database and user assets.
        /// </summary>
        /// <param name="backupFilePath">Destination path for the backup package.</param>
        /// <param name="cancellationToken">Token to observe for cancellation.</param>
        Task BackupApplicationAsync(string backupFilePath, CancellationToken cancellationToken)
            => BackupDatabaseAsync(backupFilePath, cancellationToken);

        /// <summary>
        /// Restores the database and user assets from a full application backup package.
        /// </summary>
        /// <param name="backupFilePath">Source path for the backup package.</param>
        /// <param name="safetyBackupDirectory">Directory where a pre-restore safety backup should be created.</param>
        /// <param name="cancellationToken">Token to observe for cancellation.</param>
        /// <returns>The path of the pre-restore safety backup.</returns>
        Task<string> RestoreApplicationBackupAsync(string backupFilePath, string safetyBackupDirectory, CancellationToken cancellationToken)
            => throw new NotSupportedException("Full application restore is not supported by this backup service.");
    }
}
