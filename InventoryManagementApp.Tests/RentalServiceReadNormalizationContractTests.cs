using System;
using System.IO;
using Xunit;

namespace InventoryManagementApp.Tests
{
    public class RentalServiceReadNormalizationContractTests
    {
        [Fact]
        public void RentalMapperNormalizesAllDisplayTextFields()
        {
            var source = ReadRepoFile("InventoryManagementApp", "Services", "Rentals", "RentalService.cs");
            var mapRental = ExtractMethod(source, "Rental? MapRental(IDataRecord r)", "DateTime ParseDateOrDefault");

            AssertContainsAll(
                mapRental,
                "Status = ValidateString(r[\"Status\"], \"Status\")",
                "ItemNumber = ValidateString(r[\"ItemNumber\"], \"ItemNumber\")",
                "CustomerName = ValidateString(r[\"Company\"], \"Company\")",
                "CustomerContact = ValidateString(r[\"Contact\"], \"Contact\")",
                "CustomerEmail = ValidateString(r[\"Email\"], \"Email\")",
                "CustomerPhone = ValidateString(r[\"Phone\"], \"Phone\")",
                "CustomerMobile = ValidateString(r[\"Mobile\"], \"Mobile\")",
                "CustomerAddress = ValidateString(r[\"Address\"], \"Address\")",
                "ImagePath = ValidateString(r[\"ImagePath\"], \"ImagePath\")",
                "ItemLocation = ValidateString(r[\"ItemLocation\"], \"ItemLocation\")");

            var validateString = ExtractMethod(source, "string ValidateString(object? value, string field)", "private static string NormalizeRentalReadText");
            AssertContainsAll(
                validateString,
                "var text = NormalizeRentalReadText(value);",
                "if (string.IsNullOrEmpty(text))",
                "return text;");

            var normalizer = ExtractMethod(source, "private static string NormalizeRentalReadText(object? value)", "public async Task RentItemAsync");
            AssertContainsAll(
                normalizer,
                "value?.ToString()?.Trim() ?? string.Empty");
        }

        [Fact]
        public void RentalFrequencyNormalizesItemSummaryText()
        {
            var source = ReadRepoFile("InventoryManagementApp", "Services", "Rentals", "RentalService.cs");
            var frequencyMethod = ExtractMethod(source, "public async Task<List<ItemRentalFrequency>> GetRentalFrequencyAsync", "private static async Task<int> GetAvailableQuantityForExistingItemAsync");

            AssertContainsAll(
                frequencyMethod,
                "ItemNumber = NormalizeRentalReadText(reader[\"ItemNumber\"])",
                "ItemName = NormalizeRentalReadText(reader[\"NameDescription\"])");

            Assert.DoesNotContain("ItemNumber = reader[\"ItemNumber\"]?.ToString() ?? string.Empty", frequencyMethod, StringComparison.Ordinal);
            Assert.DoesNotContain("ItemName = reader[\"NameDescription\"]?.ToString() ?? string.Empty", frequencyMethod, StringComparison.Ordinal);
        }

        private static void AssertContainsAll(string source, params string[] expectedSnippets)
        {
            foreach (var snippet in expectedSnippets)
            {
                Assert.Contains(snippet, source, StringComparison.Ordinal);
            }
        }

        private static string ExtractMethod(string source, string startMarker, string endMarker)
        {
            var start = source.IndexOf(startMarker, StringComparison.Ordinal);
            Assert.True(start >= 0, $"Could not find method start marker: {startMarker}");

            var end = source.IndexOf(endMarker, start, StringComparison.Ordinal);
            Assert.True(end > start, $"Could not find method end marker: {endMarker}");

            return source[start..end];
        }

        private static string ReadRepoFile(params string[] parts)
        {
            var directory = AppContext.BaseDirectory;

            while (!string.IsNullOrEmpty(directory))
            {
                var candidate = Path.Combine(directory, Path.Combine(parts));
                if (File.Exists(candidate))
                    return File.ReadAllText(candidate);

                var parent = Directory.GetParent(directory);
                if (parent is null)
                    break;

                directory = parent.FullName;
            }

            throw new FileNotFoundException($"Could not find repository file: {Path.Combine(parts)}");
        }
    }
}
