using System;
using System.IO;
using Xunit;

namespace InventoryManagementApp.Tests
{
    public class ResultModelContractTests
    {
        [Fact]
        public void GenericResultUsesCanonicalValueOnly()
        {
            var source = ReadRepoFile("InventoryManagementApp", "Models", "Result.cs");

            Assert.Contains("public record Result<T>(T? Value, bool Success, string? ErrorMessage = null)", source, StringComparison.Ordinal);
            Assert.DoesNotContain("List<ActivityLog>", source, StringComparison.Ordinal);
            Assert.DoesNotContain("Data {", source, StringComparison.Ordinal);
            Assert.DoesNotContain("using InventoryManagementApp.Models.Domain;", source, StringComparison.Ordinal);
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
