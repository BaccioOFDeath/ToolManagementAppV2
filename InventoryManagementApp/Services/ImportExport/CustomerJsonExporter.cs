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
    /// Exports customers to JSON format.
    /// </summary>
    public class CustomerJsonExporter : IDataExporter<Customer>
    {
        private static readonly JsonSerializerOptions JsonOptions = new()
        {
            WriteIndented = true,
            DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull,
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase
        };

        public string FileExtension => ".json";
        public string FileFilter => "JSON Files|*.json";
        public string FormatName => "JSON";

        public async Task ExportAsync(string filePath, IEnumerable<Customer> data, CancellationToken cancellationToken = default)
        {
            if (string.IsNullOrWhiteSpace(filePath))
                throw new ArgumentException("File path cannot be null or empty.", nameof(filePath));

            if (data == null)
                throw new ArgumentNullException(nameof(data));

            var customers = data.ToList();
            var json = JsonSerializer.Serialize(customers, JsonOptions);
            await File.WriteAllTextAsync(filePath, json, cancellationToken).ConfigureAwait(false);
        }
    }
}
