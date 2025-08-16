using System;
using System.Collections.Generic;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.VisualBasic.FileIO;
using System.Linq;
using ToolManagementAppV2.Models.Domain;
using ToolManagementAppV2.Models.ImportExport;

namespace ToolManagementAppV2.Utilities.IO
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
            return await Task.Run(()
                parser.EndOfData ? Array.Empty<string>() : parser.ReadFields().Select(h => h.Trim())
            ).ConfigureAwait(false);
        }

        public static List<ToolModel> LoadToolsFromCsv(string filePath, IDictionary<string, string> map, out List<int> invalidRows)
        {
            ValidateRequired(map, "ToolNumber");
            var list = new List<ToolModel>();
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
                var toolNumber = GetMapped(cols, headers, map, "ToolNumber");
                if (string.IsNullOrWhiteSpace(toolNumber))
                {
                    invalidRows.Add(row);
                    continue;
                }

                list.Add(new ToolModel
                {
                    ToolNumber = toolNumber,
                    NameDescription = GetMapped(cols, headers, map, "NameDescription"),
                    Location = GetMapped(cols, headers, map, "Location"),
                    Brand = GetMapped(cols, headers, map, "Brand"),
                    PartNumber = GetMapped(cols, headers, map, "PartNumber"),
                    Supplier = GetMapped(cols, headers, map, "Supplier"),
                    PurchasedDate = TryParseDate(GetMapped(cols, headers, map, "PurchasedDate")),
                    Notes = GetMapped(cols, headers, map, "Notes"),
                    QuantityOnHand = TryParseInt(GetMapped(cols, headers, map, "AvailableQuantity")),
                    IsPowerTool = TryParseBool(GetMapped(cols, headers, map, "IsPowerTool"))
                });
            }

            return list;
        }

        public static async Task<(List<ToolModel> Tools, List<int> InvalidRows)> LoadToolsFromCsvAsync(
            string filePath, IDictionary<string, string> map, CancellationToken cancellationToken = default)
        {
            return await Task.Run(() =>
            {
                var tools = LoadToolsFromCsv(filePath, map, out var invalid);
                return (tools, invalid);
            }, cancellationToken).ConfigureAwait(false);
        }

        public static void ExportToolsToCsv(string filePath, List<ToolModel> tools)
        {
            var lines = new List<string>
            {
                "ToolNumber,NameDescription,Location,Brand,PartNumber,Supplier,PurchasedDate,Notes,AvailableQuantity,IsPowerTool"
            };
            lines.AddRange(tools.Select(t =>
                string.Join(",",
                    Quote(t.ToolNumber),
                    Quote(t.NameDescription),
                    Quote(t.Location),
                    Quote(t.Brand),
                    Quote(t.PartNumber),
                    Quote(t.Supplier),
                    Quote(t.PurchasedDate?.ToString("yyyy-MM-dd")),
                    Quote(t.Notes),
                    Quote(t.QuantityOnHand.ToString()),
                    Quote(t.IsPowerTool ? "1" : "0"))));
            File.WriteAllLines(filePath, lines);
        }

        public static async Task ExportToolsToCsvAsync(string filePath, List<ToolModel> tools)
        {
            var lines = new List<string>
            {
                "ToolNumber,NameDescription,Location,Brand,PartNumber,Supplier,PurchasedDate,Notes,AvailableQuantity,IsPowerTool"
            };
            lines.AddRange(tools.Select(t =>
                string.Join(",",
                    Quote(t.ToolNumber),
                    Quote(t.NameDescription),
                    Quote(t.Location),
                    Quote(t.Brand),
                    Quote(t.PartNumber),
                    Quote(t.Supplier),
                    Quote(t.PurchasedDate?.ToString("yyyy-MM-dd")),
                    Quote(t.Notes),
                    Quote(t.QuantityOnHand.ToString()),
                    Quote(t.IsPowerTool ? "1" : "0"))));
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
