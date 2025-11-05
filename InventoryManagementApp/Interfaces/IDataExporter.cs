using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace InventoryManagementApp.Interfaces
{
    /// <summary>
    /// Interface for exporting data to different file formats.
    /// </summary>
    /// <typeparam name="T">The type of data to export.</typeparam>
    public interface IDataExporter<T>
    {
        /// <summary>
        /// Gets the file extension for this exporter (e.g., ".csv", ".json", ".xml").
        /// </summary>
        string FileExtension { get; }

        /// <summary>
        /// Gets the file filter for file dialogs (e.g., "CSV Files|*.csv").
        /// </summary>
        string FileFilter { get; }

        /// <summary>
        /// Gets a human-readable name for this export format.
        /// </summary>
        string FormatName { get; }

        /// <summary>
        /// Exports data to the specified file path.
        /// </summary>
        /// <param name="filePath">The path where the file should be saved.</param>
        /// <param name="data">The data to export.</param>
        /// <param name="cancellationToken">Cancellation token.</param>
        Task ExportAsync(string filePath, IEnumerable<T> data, CancellationToken cancellationToken = default);
    }
}
