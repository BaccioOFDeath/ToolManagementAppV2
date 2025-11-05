using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace InventoryManagementApp.Interfaces
{
    /// <summary>
    /// Interface for importing data from different file formats.
    /// </summary>
    /// <typeparam name="T">The type of data to import.</typeparam>
    public interface IDataImporter<T>
    {
        /// <summary>
        /// Gets the file extension for this importer (e.g., ".csv", ".json", ".xml").
        /// </summary>
        string FileExtension { get; }

        /// <summary>
        /// Gets the file filter for file dialogs (e.g., "CSV Files|*.csv").
        /// </summary>
        string FileFilter { get; }

        /// <summary>
        /// Gets a human-readable name for this import format.
        /// </summary>
        string FormatName { get; }

        /// <summary>
        /// Imports data from the specified file path.
        /// </summary>
        /// <param name="filePath">The path to the file to import.</param>
        /// <param name="cancellationToken">Cancellation token.</param>
        /// <returns>The imported data and list of skipped row numbers.</returns>
        Task<(IEnumerable<T> Data, List<int> SkippedRows)> ImportAsync(string filePath, CancellationToken cancellationToken = default);
    }
}
