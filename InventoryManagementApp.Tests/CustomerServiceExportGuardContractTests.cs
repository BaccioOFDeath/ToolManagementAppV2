using System;
using System.IO;
using Xunit;

namespace InventoryManagementApp.Tests
{
    public class CustomerServiceExportGuardContractTests
    {
        [Fact]
        public void CsvCustomerExportRequiresImportExportPermissionBeforeExportWork()
        {
            var source = ReadRepoFile("InventoryManagementApp", "Services", "Customers", "CustomerService.cs");
            var method = ExtractMethod(
                source,
                "public Task ExportCustomersToCsvAsync",
                "async Task AddCustomerInternalAsync");

            Assert.Contains("_auth.EnsurePermission(User.PermissionImportExport);", method, StringComparison.Ordinal);
            Assert.Contains("return ExportCustomersToCsvInternalAsync(filePath, cancellationToken);", method, StringComparison.Ordinal);

            Assert.True(
                method.IndexOf("_auth.EnsurePermission(User.PermissionImportExport);", StringComparison.Ordinal) <
                method.IndexOf("return ExportCustomersToCsvInternalAsync(filePath, cancellationToken);", StringComparison.Ordinal),
                "CSV customer exports should enforce import/export permission before starting export work.");
        }

        [Fact]
        public void CsvCustomerExportHonorsCancellationBeforeReadAndFileWriterWork()
        {
            var source = ReadRepoFile("InventoryManagementApp", "Services", "Customers", "CustomerService.cs");
            var method = ExtractMethod(
                source,
                "async Task ExportCustomersToCsvInternalAsync",
                "async Task InsertCustomerAsync");

            AssertCancellationBefore(method, "GetAllCustomersInternalAsync(cancellationToken)");
            AssertCancellationBetween(method, "GetAllCustomersInternalAsync(cancellationToken)", "Task.Run");
            AssertCancellationBetween(method, "Task.Run", "CsvHelperUtil.ExportCustomersToCsv(filePath, all)");
            Assert.Contains("CsvHelperUtil.ExportCustomersToCsv(filePath, all);", method, StringComparison.Ordinal);
        }

        [Fact]
        public void GenericCustomerExportRequiresPermissionAndHonorsCancellationBeforeExporterWrites()
        {
            var source = ReadRepoFile("InventoryManagementApp", "Services", "Customers", "CustomerService.cs");
            var method = ExtractMethod(
                source,
                "public async Task ExportCustomersAsync",
                "static void NotifyChanged");

            Assert.Contains("_auth.EnsurePermission(User.PermissionImportExport);", method, StringComparison.Ordinal);
            AssertCancellationBefore(method, "GetAllCustomersAsync(cancellationToken)");
            AssertCancellationBetween(method, "GetAllCustomersAsync(cancellationToken)", "exporter.ExportAsync(filePath, all, cancellationToken)");

            Assert.True(
                method.IndexOf("_auth.EnsurePermission(User.PermissionImportExport);", StringComparison.Ordinal) <
                method.IndexOf("cancellationToken.ThrowIfCancellationRequested();", StringComparison.Ordinal),
                "Generic customer exports should enforce permission before beginning cancellable export work.");
        }

        private static void AssertCancellationBefore(string source, string marker)
        {
            var markerIndex = source.IndexOf(marker, StringComparison.Ordinal);
            Assert.True(markerIndex >= 0, $"Expected source marker: {marker}");

            var cancellationIndex = source.LastIndexOf("cancellationToken.ThrowIfCancellationRequested();", markerIndex, StringComparison.Ordinal);
            Assert.True(cancellationIndex >= 0, $"Expected cancellation check before marker: {marker}");
        }

        private static void AssertCancellationBetween(string source, string firstMarker, string secondMarker)
        {
            var firstIndex = source.IndexOf(firstMarker, StringComparison.Ordinal);
            var secondIndex = source.IndexOf(secondMarker, firstIndex, StringComparison.Ordinal);

            Assert.True(firstIndex >= 0, $"Expected first source marker: {firstMarker}");
            Assert.True(secondIndex > firstIndex, $"Expected second source marker after first marker: {secondMarker}");

            var cancellationIndex = source.IndexOf("cancellationToken.ThrowIfCancellationRequested();", firstIndex, StringComparison.Ordinal);
            Assert.True(
                cancellationIndex > firstIndex && cancellationIndex < secondIndex,
                $"Expected cancellation check between markers: {firstMarker} -> {secondMarker}");
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
                    return NormalizeLineEndings(File.ReadAllText(candidate));

                var parent = Directory.GetParent(directory);
                if (parent is null)
                    break;

                directory = parent.FullName;
            }

            throw new FileNotFoundException($"Could not find repository file: {Path.Combine(parts)}");
        }

        private static string NormalizeLineEndings(string text) =>
            text.Replace("\r\n", "\n", StringComparison.Ordinal).Replace("\r", "\n", StringComparison.Ordinal);
    }
}
