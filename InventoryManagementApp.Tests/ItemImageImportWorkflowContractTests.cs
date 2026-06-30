using System;
using System.IO;
using Xunit;

namespace InventoryManagementApp.Tests
{
    public class ItemImageImportWorkflowContractTests
    {
        [Fact]
        public void ImageImportReportsProgressForEveryEnumeratedFile()
        {
            var source = ReadRepoFile("InventoryManagementApp", "Services", "Items", "ItemService.cs");
            var method = ExtractMethod(
                source,
                "private async Task<ImageImportResult> ImportItemImagesInternalAsync",
                "protected virtual Task CopyFileAsync");

            Assert.Contains("var total = files.Count;", method, StringComparison.Ordinal);
            Assert.Contains("finally", method, StringComparison.Ordinal);
            Assert.Contains("processed++;", method, StringComparison.Ordinal);
            Assert.Contains("progress?.Report(new ImageImportProgress { Processed = processed, Total = total });", method, StringComparison.Ordinal);

            Assert.True(
                method.LastIndexOf("processed++;", StringComparison.Ordinal) >
                method.LastIndexOf("finally", StringComparison.Ordinal),
                "Image import progress should advance from the finally block so unsupported, unmatched, conflicting, failed, and imported files all move the progress meter.");
        }

        [Fact]
        public void ImageImportKeepsCancellationEscapingButContinuesAfterPerFileFailures()
        {
            var source = ReadRepoFile("InventoryManagementApp", "Services", "Items", "ItemService.cs");
            var method = ExtractMethod(
                source,
                "private async Task<ImageImportResult> ImportItemImagesInternalAsync",
                "protected virtual Task CopyFileAsync");

            Assert.Contains("catch (OperationCanceledException)", method, StringComparison.Ordinal);
            Assert.Contains("throw;", method, StringComparison.Ordinal);
            Assert.Contains("catch (Exception ex)", method, StringComparison.Ordinal);
            Assert.Contains("_logger.LogError(ex, \"Failed to import image {Source}\", file);", method, StringComparison.Ordinal);
            Assert.Contains("result.ConflictingFiles.Add(file);", method, StringComparison.Ordinal);
            Assert.DoesNotContain("catch (IOException ex)", method, StringComparison.Ordinal);

            Assert.True(
                method.IndexOf("catch (OperationCanceledException)", StringComparison.Ordinal) <
                method.IndexOf("catch (Exception ex)", StringComparison.Ordinal),
                "Cancellation must be rethrown before broad per-file failure handling.");
            Assert.True(
                method.IndexOf("await CopyFileAsync(file, dest, 256, 256, cancellationToken);", StringComparison.Ordinal) <
                method.IndexOf("await UpdateItemImageAsync(item.ItemID, relative, cancellationToken);", StringComparison.Ordinal),
                "Image imports should still copy the asset before updating the item record.");
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
