using System;
using System.Collections.Generic;
using System.IO;
using Microsoft.VisualBasic.FileIO;
using System.Linq;
using ToolManagementAppV2.Models.Domain;
using ToolManagementAppV2.Models.ImportExport;

namespace ToolManagementAppV2.Utilities.IO
{
    public static class CsvHelperUtil
    {
        public static List<ToolModel> LoadToolsFromCsv(string filePath, IDictionary<string, string> map)
        {
            var list = new List<ToolModel>();
            using var parser = new TextFieldParser(filePath);
            parser.SetDelimiters(",");
            parser.HasFieldsEnclosedInQuotes = true;

            if (parser.EndOfData) return list;
            var headers = parser.ReadFields();

            while (!parser.EndOfData)
            {
                var cols = parser.ReadFields();
                list.Add(new ToolModel
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
                });
            }

            return list;
        }

        public static void ExportToolsToCsv(string filePath, List<ToolModel> tools)
        {
            var lines = new List<string>
            {
                "ToolNumber,NameDescription,Location,Brand,PartNumber,Supplier,PurchasedDate,Notes,AvailableQuantity"
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
                    Quote(t.QuantityOnHand.ToString()))));
            File.WriteAllLines(filePath, lines);
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
            var index = Array.IndexOf(headers, column);
            return index >= 0 && index < row.Length ? row[index].Trim() : null;
        }

        private static int TryParseInt(string input) =>
            int.TryParse(input, out var result) ? result : 0;

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
