using System;
using System.Collections.Generic;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using System.Runtime.CompilerServices;
using Microsoft.VisualBasic.FileIO;
using System.Linq;
using InventoryManagementApp.Models.Domain;
using InventoryManagementApp.Models.ImportExport;

namespace InventoryManagementApp.Utilities.IO
{
    public static class CsvHelperUtil
    {
        public static IEnumerable<string> ReadHeaders(string filePath)
        {
            using var parser = new TextFieldParser(filePath);
            parser.SetDelimiters(",");
            parser.HasFieldsEnclosedInQuotes = true;
            if (parser.EndOfData) return Array.Empty<string>();
            var headers = parser.ReadFields() ?? Array.Empty<string>();
            return headers.Select(h => h.Trim());
        }

        public static async Task<IEnumerable<string>> ReadHeadersAsync(string filePath)
        {
            using var parser = new TextFieldParser(filePath);
            parser.SetDelimiters(",");
            parser.HasFieldsEnclosedInQuotes = true;
            return await Task.Run(() =>
            {
                if (parser.EndOfData) return Array.Empty<string>();
                var headers = parser.ReadFields() ?? Array.Empty<string>();
                return headers.Select(h => h.Trim());
            }).ConfigureAwait(false);
        }


        public static List<ItemModel> LoadItemsFromCsv(string filePath, IDictionary<string, string> map, out List<int> invalidRows)
        {
            ValidateRequired(map, "ItemNumber");
            var list = new List<ItemModel>();
            invalidRows = new List<int>();
            using var parser = new TextFieldParser(filePath);
            parser.SetDelimiters(",");
            parser.HasFieldsEnclosedInQuotes = true;

            if (parser.EndOfData) return list;
            var headers = parser.ReadFields() ?? Array.Empty<string>();

            var row = 1; // header already read
            while (!parser.EndOfData)
            {
                row++;
                var cols = parser.ReadFields();
                if (cols == null) continue;
                var itemNumber = GetMapped(cols, headers, map, "ItemNumber");
                if (string.IsNullOrWhiteSpace(itemNumber))
                {
                    invalidRows.Add(row);
                    continue;
                }

                list.Add(new ItemModel
                {
                    ItemNumber = itemNumber,
                    Name = GetMapped(cols, headers, map, nameof(ItemImportDto.Name)) ?? string.Empty,
                    Location = GetMapped(cols, headers, map, "Location") ?? string.Empty,
                    Brand = GetMapped(cols, headers, map, "Brand") ?? string.Empty,
                    PartNumber = GetMapped(cols, headers, map, "PartNumber") ?? string.Empty,
                    Supplier = GetMapped(cols, headers, map, "Supplier") ?? string.Empty,
                    PurchasedDate = TryParseDate(GetMapped(cols, headers, map, "PurchasedDate")),
                    Notes = GetMapped(cols, headers, map, "Notes") ?? string.Empty,
                    Keywords = GetMapped(cols, headers, map, nameof(ItemImportDto.Keywords)) ?? string.Empty,
                    QuantityOnHand = TryParseInt(GetMapped(cols, headers, map, "AvailableQuantity")),
                    IsPowered = TryParseBool(GetMapped(cols, headers, map, "IsPowered")),
                    IsRentalItem = TryParseBool(GetMapped(cols, headers, map, nameof(ItemImportDto.IsRentalItem)))
                });
            }

            return list;
        }

        public static async IAsyncEnumerable<ItemModel> StreamItemsFromCsvAsync(
            string filePath,
            IDictionary<string, string> map,
            List<int>? invalidRows = null,
            [EnumeratorCancellation] CancellationToken cancellationToken = default)
        {
            ValidateRequired(map, "ItemNumber");
            invalidRows ??= new List<int>();
            using var parser = new TextFieldParser(filePath);
            parser.SetDelimiters(",");
            parser.HasFieldsEnclosedInQuotes = true;

            if (parser.EndOfData) yield break;
            var headers = parser.ReadFields() ?? Array.Empty<string>();

            var row = 1; // header already read
            while (!parser.EndOfData)
            {
                cancellationToken.ThrowIfCancellationRequested();
                row++;
                var cols = parser.ReadFields();
                if (cols == null)
                {
                    invalidRows.Add(row);
                    continue;
                }
                var itemNumber = GetMapped(cols, headers, map, "ItemNumber");
                if (string.IsNullOrWhiteSpace(itemNumber))
                {
                    invalidRows.Add(row);
                    continue;
                }

                yield return new ItemModel
                {
                    ItemNumber = itemNumber,
                    Name = GetMapped(cols, headers, map, nameof(ItemImportDto.Name)) ?? string.Empty,
                    Location = GetMapped(cols, headers, map, "Location") ?? string.Empty,
                    Brand = GetMapped(cols, headers, map, "Brand") ?? string.Empty,
                    PartNumber = GetMapped(cols, headers, map, "PartNumber") ?? string.Empty,
                    Supplier = GetMapped(cols, headers, map, "Supplier") ?? string.Empty,
                    PurchasedDate = TryParseDate(GetMapped(cols, headers, map, "PurchasedDate")),
                    Notes = GetMapped(cols, headers, map, "Notes") ?? string.Empty,
                    Keywords = GetMapped(cols, headers, map, nameof(ItemImportDto.Keywords)) ?? string.Empty,
                    QuantityOnHand = TryParseInt(GetMapped(cols, headers, map, "AvailableQuantity")),
                    IsPowered = TryParseBool(GetMapped(cols, headers, map, "IsPowered")),
                    IsRentalItem = TryParseBool(GetMapped(cols, headers, map, nameof(ItemImportDto.IsRentalItem)))
                };

                await Task.Yield();
            }
        }

        public static async Task<(List<ItemModel> Items, List<int> InvalidRows)> LoadItemsFromCsvAsync(
            string filePath, IDictionary<string, string> map, CancellationToken cancellationToken = default)
        {
            var items = new List<ItemModel>();
            var invalidRows = new List<int>();
            await foreach (var item in StreamItemsFromCsvAsync(filePath, map, invalidRows, cancellationToken)
                .WithCancellation(cancellationToken))
            {
                items.Add(item);
            }

            return (items, invalidRows);
        }

        public static void ExportItemsToCsv(string filePath, List<ItemModel> items)
        {
            var lines = new List<string>
            {
                "ItemNumber,NameDescription,Location,Brand,PartNumber,Supplier,PurchasedDate,Notes,AvailableQuantity,IsPowered,IsRentalItem"
            };
            lines.AddRange(items.Select(t =>
                string.Join(",",
                    Quote(t.ItemNumber),
                    Quote(t.Name ?? string.Empty),
                    Quote(t.Location ?? string.Empty),
                    Quote(t.Brand ?? string.Empty),
                    Quote(t.PartNumber ?? string.Empty),
                    Quote(t.Supplier ?? string.Empty),
                    Quote(t.PurchasedDate?.ToString("yyyy-MM-dd") ?? string.Empty),
                    Quote(t.Notes ?? string.Empty),
                    Quote(t.QuantityOnHand.ToString()),
                    Quote(t.IsPowered ? "1" : "0"),
                    Quote(t.IsRentalItem ? "1" : "0"))));
            File.WriteAllLines(filePath, lines);
        }

        public static async Task ExportItemsToCsvAsync(string filePath, List<ItemModel> items)
        {
            var lines = new List<string>
            {
                "ItemNumber,NameDescription,Location,Brand,PartNumber,Supplier,PurchasedDate,Notes,AvailableQuantity,IsPowered,IsRentalItem"
            };
            lines.AddRange(items.Select(t =>
                string.Join(",",
                    Quote(t.ItemNumber),
                    Quote(t.Name ?? string.Empty),
                    Quote(t.Location ?? string.Empty),
                    Quote(t.Brand ?? string.Empty),
                    Quote(t.PartNumber ?? string.Empty),
                    Quote(t.Supplier ?? string.Empty),
                    Quote(t.PurchasedDate?.ToString("yyyy-MM-dd") ?? string.Empty),
                    Quote(t.Notes ?? string.Empty),
                    Quote(t.QuantityOnHand.ToString()),
                    Quote(t.IsPowered ? "1" : "0"),
                    Quote(t.IsRentalItem ? "1" : "0"))));
            await File.WriteAllLinesAsync(filePath, lines).ConfigureAwait(false);
        }

        public static List<CustomerModel> LoadCustomersFromCsv(string filePath, IDictionary<string, string> map)
        {
            var list = new List<CustomerModel>();
            using var parser = new TextFieldParser(filePath);
            parser.SetDelimiters(",");
            parser.HasFieldsEnclosedInQuotes = true;

            if (parser.EndOfData) return list;
            var headers = parser.ReadFields() ?? Array.Empty<string>();

            while (!parser.EndOfData)
            {
                var cols = parser.ReadFields();
                if (cols == null) continue;
                list.Add(new CustomerModel
                {
                    Company = GetMapped(cols, headers, map, "Company") ?? string.Empty,
                    Email = GetMapped(cols, headers, map, "Email") ?? string.Empty,
                    Contact = GetMapped(cols, headers, map, "Contact") ?? string.Empty,
                    Phone = GetMapped(cols, headers, map, "Phone") ?? string.Empty,
                    Mobile = GetMapped(cols, headers, map, "Mobile") ?? string.Empty,
                    Address = GetMapped(cols, headers, map, "Address") ?? string.Empty
                });
            }

            return list;
        }

        public static async Task<List<CustomerModel>> LoadCustomersFromCsvAsync(string filePath, IDictionary<string, string> map,
            CancellationToken cancellationToken = default)
            => await Task.Run(() => LoadCustomersFromCsv(filePath, map), cancellationToken).ConfigureAwait(false);


        public static void ExportCustomersToCsv(string filePath, List<CustomerModel> customers)
        {
            var lines = new List<string>
            {
                "Company,Email,Contact,Phone,Mobile,Address"
            };
            lines.AddRange(customers.Select(c =>
                string.Join(",",
                    Quote(c.Company ?? string.Empty),
                    Quote(c.Email ?? string.Empty),
                    Quote(c.Contact ?? string.Empty),
                    Quote(c.Phone ?? string.Empty),
                    Quote(c.Mobile ?? string.Empty),
                    Quote(c.Address ?? string.Empty))));
            File.WriteAllLines(filePath, lines);
        }

        public static string? GetMapped(string[] row, string[] headers, IDictionary<string, string> map, string key)
        {
            ArgumentNullException.ThrowIfNull(row);
            ArgumentNullException.ThrowIfNull(headers);

            if (!map.TryGetValue(key, out var column)) return null;
            var index = Array.FindIndex(headers,
                h => string.Equals(h, column, StringComparison.OrdinalIgnoreCase));
            return index >= 0 && index < row.Length ? row[index].Trim() : null;
        }

        private static void ValidateRequired(IDictionary<string, string> map, params string[] keys)
        {
            foreach (var key in keys)
                if (map == null || !map.ContainsKey(key) || string.IsNullOrWhiteSpace(map[key]))
                    throw new ArgumentException($"Mapping for required field '{key}' is missing.", nameof(map));
        }

        private static int TryParseInt(string? input) =>
            int.TryParse(input, out var result) ? result : 0;

        private static bool TryParseBool(string? input) =>
            input != null && (input.Equals("1") || bool.TryParse(input, out var b) && b);

        private static DateTime? TryParseDate(string? input) =>
            DateTime.TryParse(input, out var result) ? result : null;


        private static string Quote(string value)
        {
            ArgumentNullException.ThrowIfNull(value);
            if (string.IsNullOrEmpty(value)) return string.Empty;
            if (value.Contains(",") || value.Contains("\"") || value.Contains("\n"))
                return $"\"{value.Replace("\"", "\"\"")}\"";
            return value;
        }
    }
}
