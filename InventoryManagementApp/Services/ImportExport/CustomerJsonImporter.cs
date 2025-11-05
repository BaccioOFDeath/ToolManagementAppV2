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
    /// Imports customers from JSON format.
    /// </summary>
    public class CustomerJsonImporter : IDataImporter<Customer>
    {
        private static readonly JsonSerializerOptions JsonOptions = new()
        {
            PropertyNameCaseInsensitive = true,
            DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull
        };

        public string FileExtension => ".json";
        public string FileFilter => "JSON Files|*.json";
        public string FormatName => "JSON";

        public async Task<(IEnumerable<Customer> Data, List<int> SkippedRows)> ImportAsync(string filePath, CancellationToken cancellationToken = default)
        {
            if (string.IsNullOrWhiteSpace(filePath))
                throw new ArgumentException("File path cannot be null or empty.", nameof(filePath));

            if (!File.Exists(filePath))
                throw new FileNotFoundException("File not found.", filePath);

            var skippedRows = new List<int>();
            var json = await File.ReadAllTextAsync(filePath, cancellationToken).ConfigureAwait(false);
            
            var customers = JsonSerializer.Deserialize<List<Customer>>(json, JsonOptions);
            
            if (customers == null || customers.Count == 0)
                return (Enumerable.Empty<Customer>(), skippedRows);

            // Validate - customers don't have strict required fields, but we can validate basic structure
            var validCustomers = new List<Customer>();
            for (int i = 0; i < customers.Count; i++)
            {
                var customer = customers[i];
                // A customer should have at least a company name or contact
                if (string.IsNullOrWhiteSpace(customer.Company) && string.IsNullOrWhiteSpace(customer.Contact))
                {
                    skippedRows.Add(i + 1);
                    continue;
                }
                validCustomers.Add(customer);
            }

            return (validCustomers, skippedRows);
        }
    }
}
