using System;
using System.IO;
using Xunit;

namespace InventoryManagementApp.Tests
{
    public class RentalItemPickerWindowResponsiveContractTests
    {
        [Fact]
        public void RentalItemPickerWindow_UsesCompactResponsiveBoundsAndHeaderSummary()
        {
            var xaml = ReadRepoFile("InventoryManagementApp", "Views", "Windows", "RentalItemPickerWindow.xaml");

            Assert.Contains("Width=\"760\"", xaml, StringComparison.Ordinal);
            Assert.Contains("Height=\"540\"", xaml, StringComparison.Ordinal);
            Assert.Contains("MinWidth=\"560\"", xaml, StringComparison.Ordinal);
            Assert.Contains("MinHeight=\"420\"", xaml, StringComparison.Ordinal);
            Assert.Contains("x:Name=\"RentalItemPickerRoot\" Margin=\"10\" MinWidth=\"0\" ClipToBounds=\"True\"", xaml, StringComparison.Ordinal);
            Assert.Contains("x:Name=\"ResultSummaryBadge\"", xaml, StringComparison.Ordinal);
            Assert.Contains("x:Name=\"ResultSummaryText\"", xaml, StringComparison.Ordinal);
            Assert.Contains("<StackPanel MinWidth=\"0\" MaxWidth=\"500\">", xaml, StringComparison.Ordinal);
            Assert.DoesNotContain("Width=\"720\"", xaml, StringComparison.Ordinal);
            Assert.DoesNotContain("MinWidth=\"620\"", xaml, StringComparison.Ordinal);
        }

        [Fact]
        public void RentalItemPickerWindow_WrapsSearchAndFooterActionsForScaledWidths()
        {
            var xaml = ReadRepoFile("InventoryManagementApp", "Views", "Windows", "RentalItemPickerWindow.xaml");

            Assert.Contains("<WrapPanel DockPanel.Dock=\"Right\"", xaml, StringComparison.Ordinal);
            Assert.Contains("x:Name=\"FindButton\"", xaml, StringComparison.Ordinal);
            Assert.Contains("MinWidth=\"84\"", xaml, StringComparison.Ordinal);
            Assert.Contains("MinWidth=\"220\"", xaml, StringComparison.Ordinal);
            Assert.Contains("MaxWidth=\"360\"", xaml, StringComparison.Ordinal);
            Assert.Contains("TextWrapping=\"Wrap\"", xaml, StringComparison.Ordinal);
            Assert.Contains("MaxWidth=\"420\"", xaml, StringComparison.Ordinal);
            Assert.Contains("<WrapPanel DockPanel.Dock=\"Right\" HorizontalAlignment=\"Right\"", xaml, StringComparison.Ordinal);
            Assert.Contains("x:Name=\"UseItemButton\"", xaml, StringComparison.Ordinal);
            Assert.DoesNotContain("<StackPanel DockPanel.Dock=\"Right\" Orientation=\"Horizontal\"", xaml, StringComparison.Ordinal);
            Assert.DoesNotContain("MinWidth=\"260\"", xaml, StringComparison.Ordinal);
        }

        [Fact]
        public void RentalItemPickerWindow_EnablesVirtualizedScrollableGridWithBoundedColumns()
        {
            var xaml = ReadRepoFile("InventoryManagementApp", "Views", "Windows", "RentalItemPickerWindow.xaml");

            Assert.Contains("x:Name=\"ItemsGrid\"", xaml, StringComparison.Ordinal);
            Assert.Contains("SelectionChanged=\"ItemsGrid_SelectionChanged\"", xaml, StringComparison.Ordinal);
            Assert.Contains("EnableRowVirtualization=\"True\"", xaml, StringComparison.Ordinal);
            Assert.Contains("EnableColumnVirtualization=\"True\"", xaml, StringComparison.Ordinal);
            Assert.Contains("SelectionMode=\"Single\"", xaml, StringComparison.Ordinal);
            Assert.Contains("SelectionUnit=\"FullRow\"", xaml, StringComparison.Ordinal);
            Assert.Contains("ScrollViewer.CanContentScroll=\"True\"", xaml, StringComparison.Ordinal);
            Assert.Contains("ScrollViewer.HorizontalScrollBarVisibility=\"Auto\"", xaml, StringComparison.Ordinal);
            Assert.Contains("ScrollViewer.VerticalScrollBarVisibility=\"Auto\"", xaml, StringComparison.Ordinal);
            Assert.Contains("Width=\"104\" MinWidth=\"82\"", xaml, StringComparison.Ordinal);
            Assert.Contains("Width=\"2*\" MinWidth=\"150\"", xaml, StringComparison.Ordinal);
            Assert.Contains("Width=\"1.1*\" MinWidth=\"112\"", xaml, StringComparison.Ordinal);
            Assert.DoesNotContain("Width=\"110\"", xaml, StringComparison.Ordinal);
            Assert.DoesNotContain("Width=\"120\"", xaml, StringComparison.Ordinal);
        }

        [Fact]
        public void RentalItemPickerWindow_ShowsSeparateLoadingAndEmptyStates()
        {
            var xaml = ReadRepoFile("InventoryManagementApp", "Views", "Windows", "RentalItemPickerWindow.xaml");

            Assert.Contains("x:Name=\"EmptyStatePanel\"", xaml, StringComparison.Ordinal);
            Assert.Contains("MaxWidth=\"340\"", xaml, StringComparison.Ordinal);
            Assert.Contains("MinHeight=\"112\"", xaml, StringComparison.Ordinal);
            Assert.Contains("x:Name=\"LoadingOverlay\"", xaml, StringComparison.Ordinal);
            Assert.Contains("IsHitTestVisible=\"True\"", xaml, StringComparison.Ordinal);
            Assert.Contains("Loading available rental items", xaml, StringComparison.Ordinal);
            Assert.Contains("Keeping the picker responsive", xaml, StringComparison.Ordinal);
            Assert.Contains("Visibility=\"Collapsed\"", xaml, StringComparison.Ordinal);
        }

        [Fact]
        public void RentalItemPickerWindow_VersionsLoadsAndSuppressesStaleResults()
        {
            var codeBehind = ReadRepoFile("InventoryManagementApp", "Views", "Windows", "RentalItemPickerWindow.xaml.cs");

            Assert.Contains("int _loadVersion;", codeBehind, StringComparison.Ordinal);
            Assert.Contains("bool _isLoading;", codeBehind, StringComparison.Ordinal);
            Assert.Contains("Interlocked.Increment(ref _loadVersion)", codeBehind, StringComparison.Ordinal);
            Assert.Contains("if (version != _loadVersion)", codeBehind, StringComparison.Ordinal);
            Assert.Contains("_searchTimer.Stop();", codeBehind, StringComparison.Ordinal);
            Assert.Contains("_loadVersion++;", codeBehind, StringComparison.Ordinal);
            Assert.Contains("Unloaded += RentalItemPickerWindow_Unloaded;", codeBehind, StringComparison.Ordinal);
            Assert.Contains("await Dispatcher.InvokeAsync(() => { }, DispatcherPriority.ContextIdle);", codeBehind, StringComparison.Ordinal);
        }

        [Fact]
        public void RentalItemPickerWindow_DisablesActionsAndGridWhileLoading()
        {
            var codeBehind = ReadRepoFile("InventoryManagementApp", "Views", "Windows", "RentalItemPickerWindow.xaml.cs");

            Assert.Contains("UpdatePickerState(isLoading: true", codeBehind, StringComparison.Ordinal);
            Assert.Contains("LoadingOverlay.Visibility = isLoading ? Visibility.Visible : Visibility.Collapsed;", codeBehind, StringComparison.Ordinal);
            Assert.Contains("EmptyStatePanel.Visibility = !isLoading && showEmptyState ? Visibility.Visible : Visibility.Collapsed;", codeBehind, StringComparison.Ordinal);
            Assert.Contains("FindButton.IsEnabled = !isLoading;", codeBehind, StringComparison.Ordinal);
            Assert.Contains("UseItemButton.IsEnabled = !isLoading && ItemsGrid.SelectedItem is ItemModel;", codeBehind, StringComparison.Ordinal);
            Assert.Contains("ItemsGrid.IsEnabled = !isLoading;", codeBehind, StringComparison.Ordinal);
            Assert.Contains("private void ItemsGrid_SelectionChanged(object sender, SelectionChangedEventArgs e)", codeBehind, StringComparison.Ordinal);
            Assert.Contains("Wait for available rental items to finish loading.", codeBehind, StringComparison.Ordinal);
        }

        [Fact]
        public void RentalItemPickerWindow_AddsKeyboardAndRowSelectionSafety()
        {
            var codeBehind = ReadRepoFile("InventoryManagementApp", "Views", "Windows", "RentalItemPickerWindow.xaml.cs");

            Assert.Contains("PreviewKeyDown += RentalItemPickerWindow_PreviewKeyDown;", codeBehind, StringComparison.Ordinal);
            Assert.Contains("Keyboard.Modifiers == ModifierKeys.Control && e.Key == Key.F", codeBehind, StringComparison.Ordinal);
            Assert.Contains("SearchBox.Focus();", codeBehind, StringComparison.Ordinal);
            Assert.Contains("SearchBox.SelectAll();", codeBehind, StringComparison.Ordinal);
            Assert.Contains("Keyboard.Modifiers == ModifierKeys.None && e.Key == Key.Escape", codeBehind, StringComparison.Ordinal);
            Assert.Contains("if (_isLoading && IsPickerActionShortcut(e))", codeBehind, StringComparison.Ordinal);
            Assert.Contains("Keyboard.Modifiers == ModifierKeys.None && e.Key == Key.Enter", codeBehind, StringComparison.Ordinal);
            Assert.Contains("FindAncestor<DataGridRow>(e.OriginalSource as DependencyObject)", codeBehind, StringComparison.Ordinal);
            Assert.Contains("ItemsGrid.SelectedItem = item;", codeBehind, StringComparison.Ordinal);
            Assert.Contains("e.Handled = true;", codeBehind, StringComparison.Ordinal);
        }

        [Fact]
        public void RentalItemPickerWindow_UsesSameStockAvailabilityRuleAsRentalService()
        {
            var codeBehind = ReadRepoFile("InventoryManagementApp", "Views", "Windows", "RentalItemPickerWindow.xaml.cs");
            var rentalService = ReadRepoFile("InventoryManagementApp", "Services", "Rentals", "RentalService.cs");

            Assert.Contains("bool IsAvailableForRentalPick(ItemModel item)", codeBehind, StringComparison.Ordinal);
            Assert.Contains("item.IsRentalItem", codeBehind, StringComparison.Ordinal);
            Assert.Contains("!item.IsIncomplete", codeBehind, StringComparison.Ordinal);
            Assert.Contains("item.QuantityOnHand > 0", codeBehind, StringComparison.Ordinal);
            Assert.Contains("GetAvailableQuantityForExistingItemAsync(conn, tx, itemID)", rentalService, StringComparison.Ordinal);
            Assert.Contains("if (avail < 1)", rentalService, StringComparison.Ordinal);
            Assert.DoesNotContain("&& !item.IsCheckedOut", codeBehind, StringComparison.Ordinal);
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
