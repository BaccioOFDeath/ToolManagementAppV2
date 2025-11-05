using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using InventoryManagementApp.Interfaces;
using InventoryManagementApp.Models.Domain;
using InventoryManagementApp.Utilities.IO;

namespace InventoryManagementApp.Services.ImportExport
{
    /// <summary>
    /// Exports items to CSV format using the existing CsvHelperUtil.
    /// </summary>
    public class ItemCsvExporter : IDataExporter<ItemModel>
    {
        public string FileExtension => ".csv";
        public string FileFilter => "CSV Files|*.csv";
        public string FormatName => "CSV";

        public async Task ExportAsync(string filePath, IEnumerable<ItemModel> data, CancellationToken cancellationToken = default)
        {
            if (string.IsNullOrWhiteSpace(filePath))
                throw new ArgumentException("File path cannot be null or empty.", nameof(filePath));

            if (data == null)
                throw new ArgumentNullException(nameof(data));

            var items = data.ToList();
            await CsvHelperUtil.ExportItemsToCsvAsync(filePath, items).ConfigureAwait(false);
        }
    }
}
