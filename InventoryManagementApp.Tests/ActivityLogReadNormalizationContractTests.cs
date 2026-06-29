using System;
using System.IO;
using Xunit;

namespace InventoryManagementApp.Tests
{
    public class ActivityLogReadNormalizationContractTests
    {
        [Fact]
        public void MapLogNormalizesLegacyAuditFieldsWhenReading()
        {
            var source = ReadRepoFile("InventoryManagementApp", "Services", "Users", "ActivityLogService.cs");
            var method = ExtractMethod(
                source,
                "ActivityLog MapLog",
                "            return log;");

            Assert.Contains("UserName = (r[\"UserName\"]?.ToString() ?? string.Empty).Trim(),", method, StringComparison.Ordinal);
            Assert.Contains("Action   = (r[\"Action\"]?.ToString() ?? string.Empty).Trim(),", method, StringComparison.Ordinal);
            Assert.DoesNotContain("UserName = r[\"UserName\"]?.ToString() ?? string.Empty,", method, StringComparison.Ordinal);
            Assert.DoesNotContain("Action   = r[\"Action\"]?.ToString() ?? string.Empty,", method, StringComparison.Ordinal);
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
