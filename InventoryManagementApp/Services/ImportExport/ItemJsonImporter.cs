using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Threading;
using System.Threading.Tasks;
using InventoryManagementApp.Interfaces;
using InventoryManagementApp.Models.Domain;

namespace InventoryManagementApp.Services.ImportExport
{
    /// <summary>
    /// Imports items from JSON format.
    /// </summary>
    public class ItemJsonImporter : IDataImporter<ItemModel>
    {
        private static readonly JsonSerializerOptions JsonOptions = new()
        {
            PropertyNameCaseInsensitive = true,
            DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull
        };

        public string FileExtension => ".json";
        public string FileFilter => "JSON Files|*.json";
        public string FormatName => "JSON";

        public async Task<(IEnumerable<ItemModel> Data, List<int> SkippedRows)> ImportAsync(string filePath, CancellationToken cancellationToken = default)
        {
            if (string.IsNullOrWhiteSpace(filePath))
                throw new ArgumentException("File path cannot be null or empty.", nameof(filePath));

            if (!File.Exists(filePath))
                throw new FileNotFoundException("File not found.", filePath);

            cancellationToken.ThrowIfCancellationRequested();
            var skippedRows = new List<int>();
            var json = await File.ReadAllTextAsync(filePath, cancellationToken).ConfigureAwait(false);

            cancellationToken.ThrowIfCancellationRequested();
            var items = JsonSerializer.Deserialize<List<ItemModel>>(json, JsonOptions);
            cancellationToken.ThrowIfCancellationRequested();

            if (items == null || items.Count == 0)
                return (Enumerable.Empty<ItemModel>(), skippedRows);

            // Validate required fields and skip invalid items
            var validItems = new List<ItemModel>();
            for (int i = 0; i < items.Count; i++)
            {
                cancellationToken.ThrowIfCancellationRequested();
                var item = items[i];
                if (string.IsNullOrWhiteSpace(item.ItemNumber))
                {
                    skippedRows.Add(i + 1); // 1-based index for user display
                    continue;
                }
                validItems.Add(item);
            }

            return (validItems, skippedRows);
        }
    }
}
