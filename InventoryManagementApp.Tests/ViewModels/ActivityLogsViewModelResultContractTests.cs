using System;
using System.IO;
using Xunit;

namespace InventoryManagementApp.Tests.ViewModels
{
    public class ActivityLogsViewModelResultContractTests
    {
        [Fact]
        public void LoadLogsUsesCanonicalResultValueCollection()
        {
            var source = ReadRepoFile("InventoryManagementApp", "ViewModels", "ActivityLogsViewModel.cs");

            var method = ExtractMethod(source, "public async Task<bool> LoadLogsAsync()", "private void PreserveActivityLogRowsAfterLoadFailure");

            Assert.Contains("var result = await _service.GetRecentLogsAsync();", method, StringComparison.Ordinal);
            Assert.Contains("if (!result.Success || result.Value == null)", method, StringComparison.Ordinal);
            Assert.Contains("var refreshedRows = result.Value", method, StringComparison.Ordinal);
            Assert.Contains("foreach (var log in refreshedRows)", method, StringComparison.Ordinal);
            Assert.DoesNotContain("result.Data", method, StringComparison.Ordinal);
            Assert.DoesNotContain("result?.Data", method, StringComparison.Ordinal);
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
