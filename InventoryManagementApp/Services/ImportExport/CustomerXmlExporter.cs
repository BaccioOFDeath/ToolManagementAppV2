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
    /// Exports customers to XML format.
    /// </summary>
    public class CustomerXmlExporter : IDataExporter<Customer>
    {
        public string FileExtension => ".xml";
        public string FileFilter => "XML Files|*.xml";
        public string FormatName => "XML";

        public async Task ExportAsync(string filePath, IEnumerable<Customer> data, CancellationToken cancellationToken = default)
        {
            if (string.IsNullOrWhiteSpace(filePath))
                throw new ArgumentException("File path cannot be null or empty.", nameof(filePath));

            if (data == null)
                throw new ArgumentNullException(nameof(data));

            await Task.Run(() =>
            {
                var customers = data.ToList();
                var serializer = new XmlSerializer(typeof(List<Customer>), new XmlRootAttribute("Customers"));
                using var writer = new StreamWriter(filePath);
                serializer.Serialize(writer, customers);
            }, cancellationToken).ConfigureAwait(false);
        }
    }
}
