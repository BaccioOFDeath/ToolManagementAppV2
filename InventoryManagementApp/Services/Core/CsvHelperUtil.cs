using System;
using System.Collections.Generic;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
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
            return parser.EndOfData ? Array.Empty<string>() : parser.ReadFields().Select(h => h.Trim());
        }

        public static async Task<IEnumerable<string>> ReadHeadersAsync(string filePath)
        {
            using var parser = new TextFieldParser(filePath);
            parser.SetDelimiters(",");
            parser.HasFieldsEnclosedInQuotes = true;
            return await Task.Run(() =>
            {
                return parser.EndOfData ? Array.Empty<string>() : parser.ReadFields().Select(h => h.Trim());
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
            var headers = parser.ReadFields();

            var row = 1; // header already read
            while (!parser.EndOfData)
            {
                row++;
                var cols = parser.ReadFields();
                var itemNumber = GetMapped(cols, headers, map, "ItemNumber");
                if (string.IsNullOrWhiteSpace(itemNumber))
                {
                    invalidRows.Add(row);
                    continue;
                }

                list.Add(new ItemModel
                {
                    ItemNumber = itemNumber,
                    Name = GetMapped(cols, headers, map, "NameDescription"),
                    Location = GetMapped(cols, headers, map, "Location"),
                    Brand = GetMapped(cols, headers, map, "Brand"),
                    PartNumber = GetMapped(cols, headers, map, "PartNumber"),
                    Supplier = GetMapped(cols, headers, map, "Supplier"),
                    PurchasedDate = TryParseDate(GetMapped(cols, headers, map, "PurchasedDate")),
                    Notes = GetMapped(cols, headers, map, "Notes"),
                    Keywords = GetMapped(cols, headers, map, nameof(ItemImportDto.Keywords)),
                    QuantityOnHand = TryParseInt(GetMapped(cols, headers, map, "AvailableQuantity")),
                    IsPowered = TryParseBool(GetMapped(cols, headers, map, "IsPowered"))
                });
            }

            return list;
        }

        public static async Task<(List<ItemModel> Items, List<int> InvalidRows)> LoadItemsFromCsvAsync(
            string filePath, IDictionary<string, string> map, CancellationToken cancellationToken = default)
        {
            return await Task.Run(() =>
            {
                ValidateRequired(map, "ItemNumber");
                var list = new List<ItemModel>();
                var invalidRows = new List<int>();
                using var parser = new TextFieldParser(filePath);
                parser.SetDelimiters(",");
                parser.HasFieldsEnclosedInQuotes = true;

                if (parser.EndOfData) return (list, invalidRows);
                var headers = parser.ReadFields();

                var row = 1; // header already read
                while (!parser.EndOfData)
                {
                    row++;
                    var cols = parser.ReadFields();
                    var itemNumber = GetMapped(cols, headers, map, "ItemNumber");
                    if (string.IsNullOrWhiteSpace(itemNumber))
                    {
                        invalidRows.Add(row);
                        continue;
                    }

                    list.Add(new ItemModel
                    {
                        ItemNumber = itemNumber,
                        Name = GetMapped(cols, headers, map, "NameDescription"),
                        Location = GetMapped(cols, headers, map, "Location"),
                        Brand = GetMapped(cols, headers, map, "Brand"),
                        PartNumber = GetMapped(cols, headers, map, "PartNumber"),
                        Supplier = GetMapped(cols, headers, map, "Supplier"),
                        PurchasedDate = TryParseDate(GetMapped(cols, headers, map, "PurchasedDate")),
                        Notes = GetMapped(cols, headers, map, "Notes"),
                        Keywords = GetMapped(cols, headers, map, nameof(ItemImportDto.Keywords)),
                        QuantityOnHand = TryParseInt(GetMapped(cols, headers, map, "AvailableQuantity")),
                        IsPowered = TryParseBool(GetMapped(cols, headers, map, "IsPowered"))
                    });
                }

                return (list, invalidRows);
            }, cancellationToken).ConfigureAwait(false);
        }

        public static void ExportItemsToCsv(string filePath, List<ItemModel> items)
        {
            var lines = new List<string>
            {
                "ItemNumber,NameDescription,Location,Brand,PartNumber,Supplier,PurchasedDate,Notes,AvailableQuantity,IsPowered"
            };
            lines.AddRange(items.Select(t =>
                string.Join(",",
                    Quote(t.ItemNumber),
                    Quote(t.Name),
                    Quote(t.Location),
                    Quote(t.Brand),
                    Quote(t.PartNumber),
                    Quote(t.Supplier),
                    Quote(t.PurchasedDate?.ToString("yyyy-MM-dd")),
                    Quote(t.Notes),
                    Quote(t.QuantityOnHand.ToString()),
                    Quote(t.IsPowered ? "1" : "0"))));
            File.WriteAllLines(filePath, lines);
        }

        public static async Task ExportItemsToCsvAsync(string filePath, List<ItemModel> items)
        {
            var lines = new List<string>
            {
                "ItemNumber,NameDescription,Location,Brand,PartNumber,Supplier,PurchasedDate,Notes,AvailableQuantity,IsPowered"
            };
            lines.AddRange(items.Select(t =>
                string.Join(",",
                    Quote(t.ItemNumber),
                    Quote(t.Name),
                    Quote(t.Location),
                    Quote(t.Brand),
                    Quote(t.PartNumber),
                    Quote(t.Supplier),
                    Quote(t.PurchasedDate?.ToString("yyyy-MM-dd")),
                    Quote(t.Notes),
                    Quote(t.QuantityOnHand.ToString()),
                    Quote(t.IsPowered ? "1" : "0"))));
            await File.WriteAllLinesAsync(filePath, lines).ConfigureAwait(false);
        }

        public static List<CustomerModel> LoadCustomersFromCsv(string filePath, IDictionary<string, string> map)
        {
            var list = new List<CustomerModel>();
            using var parser = new TextFieldParser(filePath);
            parser.SetDelimiters(",");
            parser.HasFieldsEnclosedInQuotes = true;

            if (parser.EndOfData) return list;
            var headers = parser.ReadFields();

            while (!parser.EndOfData)
            {
                var cols = parser.ReadFields();
                list.Add(new CustomerModel
                {
                    Company = GetMapped(cols, headers, map, "Company"),
                    Email = GetMapped(cols, headers, map, "Email"),
                    Contact = GetMapped(cols, headers, map, "Contact"),
                    Phone = GetMapped(cols, headers, map, "Phone"),
                    Mobile = GetMapped(cols, headers, map, "Mobile"),
                    Address = GetMapped(cols, headers, map, "Address")
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
                    Quote(c.Company),
                    Quote(c.Email),
                    Quote(c.Contact),
                    Quote(c.Phone),
                    Quote(c.Mobile),
                    Quote(c.Address))));
            File.WriteAllLines(filePath, lines);
        }

        private static string GetMapped(string[] row, string[] headers, IDictionary<string, string> map, string key)
        {
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

        private static int TryParseInt(string input) =>
            int.TryParse(input, out var result) ? result : 0;

        private static bool TryParseBool(string input) =>
            input != null && (input.Equals("1") || bool.TryParse(input, out var b) && b);

        private static DateTime? TryParseDate(string input) =>
            DateTime.TryParse(input, out var result) ? result : null;
    

        private static string Quote(string value)
        {
            if (string.IsNullOrEmpty(value)) return string.Empty;
            if (value.Contains(",") || value.Contains("\"") || value.Contains("\n"))
                return $"\"{value.Replace("\"", "\"\"")}\"";
            return value;
        }
    }
}
