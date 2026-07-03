using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using System.Xml;
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

            cancellationToken.ThrowIfCancellationRequested();

            await Task.Run(() =>
            {
                cancellationToken.ThrowIfCancellationRequested();
                var serializer = new XmlSerializer(typeof(Customer));
                var namespaces = new XmlSerializerNamespaces();
                namespaces.Add(string.Empty, string.Empty);

                using var writer = XmlWriter.Create(filePath, new XmlWriterSettings { Indent = true });
                writer.WriteStartDocument();
                writer.WriteStartElement("Customers");

                foreach (var customer in data)
                {
                    cancellationToken.ThrowIfCancellationRequested();
                    serializer.Serialize(writer, customer, namespaces);
                }

                cancellationToken.ThrowIfCancellationRequested();
                writer.WriteEndElement();
                writer.WriteEndDocument();
            }, cancellationToken).ConfigureAwait(false);
        }
    }
}