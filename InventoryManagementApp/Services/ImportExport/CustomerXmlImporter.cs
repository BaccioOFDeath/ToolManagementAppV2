using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Xml.Serialization;
using InventoryManagementApp.Interfaces;
using InventoryManagementApp.Models.Domain;

namespace InventoryManagementApp.Services.ImportExport
{
    /// <summary>
    /// Imports customers from XML format.
    /// </summary>
    public class CustomerXmlImporter : IDataImporter<Customer>
    {
        public string FileExtension => ".xml";
        public string FileFilter => "XML Files|*.xml";
        public string FormatName => "XML";

        public async Task<(IEnumerable<Customer> Data, List<int> SkippedRows)> ImportAsync(string filePath, CancellationToken cancellationToken = default)
        {
            if (string.IsNullOrWhiteSpace(filePath))
                throw new ArgumentException("File path cannot be null or empty.", nameof(filePath));

            if (!File.Exists(filePath))
                throw new FileNotFoundException("File not found.", filePath);

            cancellationToken.ThrowIfCancellationRequested();
            var skippedRows = new List<int>();
            
            var customers = await Task.Run(() =>
            {
                cancellationToken.ThrowIfCancellationRequested();
                var serializer = new XmlSerializer(typeof(List<Customer>), new XmlRootAttribute("Customers"));
                using var reader = new StreamReader(filePath);
                var deserializedCustomers = serializer.Deserialize(reader) as List<Customer>;
                cancellationToken.ThrowIfCancellationRequested();
                return deserializedCustomers;
            }, cancellationToken).ConfigureAwait(false);
            cancellationToken.ThrowIfCancellationRequested();

            if (customers == null || customers.Count == 0)
                return (Enumerable.Empty<Customer>(), skippedRows);

            // Validate - customers don't have strict required fields, but we can validate basic structure
            var validCustomers = new List<Customer>();
            for (int i = 0; i < customers.Count; i++)
            {
                cancellationToken.ThrowIfCancellationRequested();
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
