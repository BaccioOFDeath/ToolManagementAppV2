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
    /// Imports items from XML format.
    /// </summary>
    public class ItemXmlImporter : IDataImporter<ItemModel>
    {
        public string FileExtension => ".xml";
        public string FileFilter => "XML Files|*.xml";
        public string FormatName => "XML";

        public async Task<(IEnumerable<ItemModel> Data, List<int> SkippedRows)> ImportAsync(string filePath, CancellationToken cancellationToken = default)
        {
            if (string.IsNullOrWhiteSpace(filePath))
                throw new ArgumentException("File path cannot be null or empty.", nameof(filePath));

            if (!File.Exists(filePath))
                throw new FileNotFoundException("File not found.", filePath);

            var skippedRows = new List<int>();
            
            var items = await Task.Run(() =>
            {
                var serializer = new XmlSerializer(typeof(List<ItemModel>), new XmlRootAttribute("Items"));
                using var reader = new StreamReader(filePath);
                return serializer.Deserialize(reader) as List<ItemModel>;
            }, cancellationToken).ConfigureAwait(false);

            if (items == null || items.Count == 0)
                return (Enumerable.Empty<ItemModel>(), skippedRows);

            // Validate required fields and skip invalid items
            var validItems = new List<ItemModel>();
            for (int i = 0; i < items.Count; i++)
            {
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
