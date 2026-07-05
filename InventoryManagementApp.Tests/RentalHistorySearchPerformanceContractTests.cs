using System;
using System.IO;
using Xunit;

namespace InventoryManagementApp.Tests
{
    public class RentalHistorySearchPerformanceContractTests
    {
        [Fact]
        public void RentalHistoryViewModel_UsesAsyncCancellableSearchInsteadOfBlockingCommand()
        {
            var source = ReadRepoFile("InventoryManagementApp", "ViewModels", "RentalHistoryViewModel.cs");

            Assert.Contains("public IAsyncRelayCommand SearchCommand { get; }", source, StringComparison.Ordinal);
            Assert.Contains("SearchCommand = new AsyncRelayCommand(ExecuteSearchAsync, () => !IsFiltering);", source, StringComparison.Ordinal);
            Assert.Contains("private CancellationTokenSource? _searchCts;", source, StringComparison.Ordinal);
            Assert.Contains("Task.Run(() => BuildFilteredHistory(term, cts.Token), cts.Token)", source, StringComparison.Ordinal);
            Assert.Contains("catch (OperationCanceledException)", source, StringComparison.Ordinal);
            Assert.DoesNotContain("SearchCommand = new RelayCommand(ExecuteSearch);", source, StringComparison.Ordinal);
            Assert.DoesNotContain("void ExecuteSearch()", source, StringComparison.Ordinal);
        }

        [Fact]
        public void RentalHistoryViewModel_CachesSearchRowsAndSearchesOperationalFields()
        {
            var source = ReadRepoFile("InventoryManagementApp", "ViewModels", "RentalHistoryViewModel.cs");

            Assert.Contains("private sealed class RentalHistorySearchRow", source, StringComparison.Ordinal);
            Assert.Contains(".OrderByDescending(r => r.RentalDate)", source, StringComparison.Ordinal);
            Assert.Contains(".ThenByDescending(r => r.RentalID)", source, StringComparison.Ordinal);
            Assert.Contains("rental.RentalID.ToString(CultureInfo.InvariantCulture)", source, StringComparison.Ordinal);
            Assert.Contains("rental.ItemNumber", source, StringComparison.Ordinal);
            Assert.Contains("rental.ItemLocation", source, StringComparison.Ordinal);
            Assert.Contains("rental.CustomerName", source, StringComparison.Ordinal);
            Assert.Contains("rental.Status", source, StringComparison.Ordinal);
            Assert.Contains("rental.DueDate.ToString(\"yyyy-MM-dd\", CultureInfo.InvariantCulture)", source, StringComparison.Ordinal);
            Assert.Contains("rental.ReturnDate?.ToString(\"yyyy-MM-dd\", CultureInfo.InvariantCulture)", source, StringComparison.Ordinal);
            Assert.Contains("SearchText.Contains(term, StringComparison.OrdinalIgnoreCase)", source, StringComparison.Ordinal);
        }

        [Fact]
        public void RentalHistoryViewModel_PublishesProfessionalFilteredViewState()
        {
            var source = ReadRepoFile("InventoryManagementApp", "ViewModels", "RentalHistoryViewModel.cs");

            Assert.Contains("public string SearchStatus", source, StringComparison.Ordinal);
            Assert.Contains("if (IsFiltering)", source, StringComparison.Ordinal);
            Assert.Contains("public bool HasActiveSearch => !string.IsNullOrWhiteSpace(AppliedSearchText);", source, StringComparison.Ordinal);
            Assert.Contains("public bool HasNoResults => History.Count == 0;", source, StringComparison.Ordinal);
            Assert.Contains("public bool CanExportHistory => History.Count > 0 && !IsFiltering;", source, StringComparison.Ordinal);
            Assert.Contains("public string EmptyStateTitle => HasActiveSearch ? \"No matching rental records\" : \"No rental history records\";", source, StringComparison.Ordinal);
            Assert.Contains("public string ExportSummary => CanExportHistory", source, StringComparison.Ordinal);
            Assert.Contains("void RestoreSelection(int? previousSelectionId)", source, StringComparison.Ordinal);
            Assert.Contains("void NotifyHistoryViewChanged()", source, StringComparison.Ordinal);
        }

        [Fact]
        public void RentalHistoryViewModel_ExportsVisibleRowsWithContextAndUserFeedback()
        {
            var source = ReadRepoFile("InventoryManagementApp", "ViewModels", "RentalHistoryViewModel.cs");

            Assert.Contains("if (!CanExportHistory)", source, StringComparison.Ordinal);
            Assert.Contains("FileName = BuildExportFileName()", source, StringComparison.Ordinal);
            Assert.Contains("RentalID,ItemNumber,ItemLocation,CustomerName,RentalDate,DueDate,ReturnDate,Status,FilteredView", source, StringComparison.Ordinal);
            Assert.Contains("Escape(SearchStatus)", source, StringComparison.Ordinal);
            Assert.Contains("new UTF8Encoding(encoderShouldEmitUTF8Identifier: true)", source, StringComparison.Ordinal);
            Assert.Contains("_dialogService.ShowInfo($\"Exported {History.Count} rental record(s) to {path}.\", \"Rental History Export\");", source, StringComparison.Ordinal);
            Assert.Contains("rental_history{suffix}_{DateTime.Now:yyyyMMdd_HHmm}.csv", source, StringComparison.Ordinal);
        }

        [Fact]
        public void RentalHistoryViewModel_DisposesOutstandingSearchWork()
        {
            var source = ReadRepoFile("InventoryManagementApp", "ViewModels", "RentalHistoryViewModel.cs");

            Assert.Contains("public class RentalHistoryViewModel : ObservableObject, IDisposable", source, StringComparison.Ordinal);
            Assert.Contains("_searchCts?.Cancel();", source, StringComparison.Ordinal);
            Assert.Contains("_searchCts?.Dispose();", source, StringComparison.Ordinal);
            Assert.Contains("throw new ObjectDisposedException(nameof(RentalHistoryViewModel));", source, StringComparison.Ordinal);
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

        private static string NormalizeLineEndings(string text)
            => text.Replace("\r\n", "\n");
    }
}
