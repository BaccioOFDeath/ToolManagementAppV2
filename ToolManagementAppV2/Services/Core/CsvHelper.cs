using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using ToolManagementAppV2.Models.Domain;
using ToolManagementAppV2.Models.ImportExport;

namespace ToolManagementAppV2.Utilities.IO
{
    public static class CsvHelperUtil
    {
        public static List<ToolModel> LoadToolsFromCsv(string filePath, IDictionary<string, string> map)
        {
            var lines = File.ReadAllLines(filePath);
            if (lines.Length < 2) return new List<ToolModel>();
            var headers = lines[0].Split(',').Select(h => h.Trim()).ToArray();
            return lines.Skip(1)
                .Select(line => line.Split(','))
                .Select(cols => new ToolModel
                {
                    ToolNumber = GetMapped(cols, headers, map, "ToolNumber"),
                    NameDescription = GetMapped(cols, headers, map, "NameDescription"),
                    Location = GetMapped(cols, headers, map, "Location"),
                    Brand = GetMapped(cols, headers, map, "Brand"),
                    PartNumber = GetMapped(cols, headers, map, "PartNumber"),
                    Supplier = GetMapped(cols, headers, map, "Supplier"),
                    PurchasedDate = TryParseDate(GetMapped(cols, headers, map, "PurchasedDate")),
                    Notes = GetMapped(cols, headers, map, "Notes"),
                    QuantityOnHand = TryParseInt(GetMapped(cols, headers, map, "AvailableQuantity"))
                }).ToList();
        }

        public static void ExportToolsToCsv(string filePath, List<ToolModel> tools)
        {
            var lines = new List<string>
            {
                "ToolNumber,NameDescription,Location,Brand,PartNumber,Supplier,PurchasedDate,Notes,AvailableQuantity"
            };
            lines.AddRange(tools.Select(t =>
                $"{t.ToolNumber},{t.NameDescription},{t.Location},{t.Brand},{t.PartNumber},{t.Supplier},{t.PurchasedDate:yyyy-MM-dd},{t.Notes},{t.QuantityOnHand}"));
            File.WriteAllLines(filePath, lines);
        }

        public static List<CustomerModel> LoadCustomersFromCsv(string filePath, IDictionary<string, string> map)
        {
            var lines = File.ReadAllLines(filePath);
            if (lines.Length < 2) return new List<CustomerModel>();
            var headers = lines[0].Split(',').Select(h => h.Trim()).ToArray();
            return lines.Skip(1)
                .Select(line => line.Split(','))
                .Select(cols => new CustomerModel
                {
                    Name = GetMapped(cols, headers, map, "Name"),
                    Email = GetMapped(cols, headers, map, "Email"),
                    Contact = GetMapped(cols, headers, map, "Contact"),
                    Phone = GetMapped(cols, headers, map, "Phone"),
                    Address = GetMapped(cols, headers, map, "Address")
                }).ToList();
        }

        public static void ExportCustomersToCsv(string filePath, List<CustomerModel> customers)
        {
            var lines = new List<string>
            {
                "Name,Email,Contact,Phone,Address"
            };
            lines.AddRange(customers.Select(c =>
                $"{c.Name},{c.Email},{c.Contact},{c.Phone},{c.Address}"));
            File.WriteAllLines(filePath, lines);
        }

        private static string GetMapped(string[] row, string[] headers, IDictionary<string, string> map, string key)
        {
            if (!map.TryGetValue(key, out var column)) return null;
            var index = Array.IndexOf(headers, column);
            return index >= 0 && index < row.Length ? row[index].Trim() : null;
        }

        private static int TryParseInt(string input) =>
            int.TryParse(input, out var result) ? result : 0;

        private static DateTime? TryParseDate(string input) =>
            DateTime.TryParse(input, out var result) ? result : null;
    }
}
