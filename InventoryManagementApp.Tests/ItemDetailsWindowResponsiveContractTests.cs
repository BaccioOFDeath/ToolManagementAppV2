using System;
using System.IO;
using Xunit;

namespace InventoryManagementApp.Tests
{
    public class ItemDetailsWindowResponsiveContractTests
    {
        [Fact]
        public void ItemDetailsWindow_UsesScaledDesktopSafeWindowSizing()
        {
            var xaml = ReadRepoFile("InventoryManagementApp", "Views", "Windows", "ItemDetailsWindow.xaml");
            var codeBehind = ReadRepoFile("InventoryManagementApp", "Views", "Windows", "ItemDetailsWindow.xaml.cs");

            Assert.Contains("Width=\"880\" Height=\"760\" MinWidth=\"700\" MinHeight=\"560\"", xaml, StringComparison.Ordinal);
            Assert.Contains("this.UseResponsiveDefaultSize(880, 760);", codeBehind, StringComparison.Ordinal);
            Assert.DoesNotContain("Width=\"920\" Height=\"820\" MinWidth=\"780\" MinHeight=\"620\"", xaml, StringComparison.Ordinal);
            Assert.DoesNotContain("UseResponsiveDefaultSize(920, 820)", codeBehind, StringComparison.Ordinal);
        }

        [Fact]
        public void ItemDetailsWindow_WrapsHeaderAndSummaryCards()
        {
            var xaml = ReadRepoFile("InventoryManagementApp", "Views", "Windows", "ItemDetailsWindow.xaml");

            Assert.Contains("<StackPanel DockPanel.Dock=\"Left\" VerticalAlignment=\"Center\" MinWidth=\"220\" MaxWidth=\"460\">", xaml, StringComparison.Ordinal);
            Assert.Contains("<WrapPanel Grid.Row=\"1\" Margin=\"0,0,0,8\">", xaml, StringComparison.Ordinal);
            Assert.Contains("ItemDetailSummaryCard", xaml, StringComparison.Ordinal);
            Assert.Contains("ItemDetailSummaryValueText", xaml, StringComparison.Ordinal);
            Assert.Contains("<Setter Property=\"MinWidth\" Value=\"155\"/>", xaml, StringComparison.Ordinal);
            Assert.Contains("<Setter Property=\"MaxWidth\" Value=\"230\"/>", xaml, StringComparison.Ordinal);
            Assert.DoesNotContain("<Grid Grid.Row=\"1\" Margin=\"0,0,0,8\">", xaml, StringComparison.Ordinal);
            Assert.DoesNotContain("<ColumnDefinition Width=\"*\"/>\n                <ColumnDefinition Width=\"*\"/>\n                <ColumnDefinition Width=\"*\"/>\n                <ColumnDefinition Width=\"*\"/>", xaml, StringComparison.Ordinal);
        }

        [Fact]
        public void ItemDetailsWindow_AvoidsLargeFixedMainPanePressure()
        {
            var xaml = ReadRepoFile("InventoryManagementApp", "Views", "Windows", "ItemDetailsWindow.xaml");

            Assert.Contains("<Grid MinHeight=\"420\">", xaml, StringComparison.Ordinal);
            Assert.Contains("<ColumnDefinition Width=\"0.75*\" MinWidth=\"240\"/>", xaml, StringComparison.Ordinal);
            Assert.Contains("<ColumnDefinition Width=\"6\"/>", xaml, StringComparison.Ordinal);
            Assert.Contains("<ColumnDefinition Width=\"1.85*\" MinWidth=\"0\"/>", xaml, StringComparison.Ordinal);
            Assert.Contains("Style=\"{StaticResource Card}\" Padding=\"0\" MinWidth=\"0\"", xaml, StringComparison.Ordinal);
            Assert.Contains("<Grid Grid.Column=\"2\" MinWidth=\"0\">", xaml, StringComparison.Ordinal);
            Assert.DoesNotContain("<ColumnDefinition Width=\"255\"/>", xaml, StringComparison.Ordinal);
            Assert.DoesNotContain("<ColumnDefinition Width=\"8\"/>", xaml, StringComparison.Ordinal);
            Assert.DoesNotContain("<Grid MinHeight=\"500\">", xaml, StringComparison.Ordinal);
        }

        [Fact]
        public void ItemDetailsWindow_KeepsScrollableReachableDetailPanes()
        {
            var xaml = ReadRepoFile("InventoryManagementApp", "Views", "Windows", "ItemDetailsWindow.xaml");

            Assert.Contains("<ScrollViewer VerticalScrollBarVisibility=\"Auto\" HorizontalScrollBarVisibility=\"Disabled\">", xaml, StringComparison.Ordinal);
            Assert.Contains("Height=\"160\" MaxHeight=\"184\"", xaml, StringComparison.Ordinal);
            Assert.Contains("<Grid MinWidth=\"0\">", xaml, StringComparison.Ordinal);
            Assert.Contains("<ColumnDefinition Width=\"76\"/>", xaml, StringComparison.Ordinal);
            Assert.Contains("<Border Grid.Row=\"1\" BorderBrush=\"{DynamicResource BorderBrushAlt}\" BorderThickness=\"1\" Background=\"{DynamicResource ControlBackgroundBrush}\" Padding=\"8\" MinHeight=\"120\">", xaml, StringComparison.Ordinal);
            Assert.DoesNotContain("<ColumnDefinition Width=\"82\"/>", xaml, StringComparison.Ordinal);
            Assert.DoesNotContain("Background=\"{DynamicResource ControlBackgroundBrush}\" Height=\"184\" Margin=\"0,0,0,10\"", xaml, StringComparison.Ordinal);
        }

        [Fact]
        public void ItemDetailsWindow_WrapsOperationalFieldsAndActions()
        {
            var xaml = ReadRepoFile("InventoryManagementApp", "Views", "Windows", "ItemDetailsWindow.xaml");

            Assert.Contains("ItemDetailInlineFieldCard", xaml, StringComparison.Ordinal);
            Assert.Contains("<WrapPanel Margin=\"12\">", xaml, StringComparison.Ordinal);
            Assert.Contains("<WrapPanel Grid.Column=\"1\" VerticalAlignment=\"Top\" HorizontalAlignment=\"Right\">", xaml, StringComparison.Ordinal);
            Assert.Contains("<WrapPanel Grid.Row=\"2\" Margin=\"0,8,0,0\">", xaml, StringComparison.Ordinal);
            Assert.Contains("<WrapPanel HorizontalAlignment=\"Right\">", xaml, StringComparison.Ordinal);
            Assert.Contains("<WrapPanel>", xaml, StringComparison.Ordinal);
            Assert.DoesNotContain("<StackPanel Grid.Column=\"1\" Orientation=\"Horizontal\" VerticalAlignment=\"Top\">", xaml, StringComparison.Ordinal);
            Assert.DoesNotContain("<StackPanel Orientation=\"Horizontal\" HorizontalAlignment=\"Right\">", xaml, StringComparison.Ordinal);
            Assert.DoesNotContain("<Grid Grid.Row=\"2\" Margin=\"0,8,0,0\">", xaml, StringComparison.Ordinal);
        }

        [Fact]
        public void ItemDetailsWindow_PreservesPrimaryCommandsAndShortcuts()
        {
            var xaml = ReadRepoFile("InventoryManagementApp", "Views", "Windows", "ItemDetailsWindow.xaml");

            Assert.Contains("Key=\"P\" Modifiers=\"Control\" Command=\"{Binding PrintDetailsCommand}\"", xaml, StringComparison.Ordinal);
            Assert.Contains("Key=\"R\" Modifiers=\"Control\" Command=\"{Binding PlaceReservationCommand}\"", xaml, StringComparison.Ordinal);
            Assert.Contains("EditCommand", xaml, StringComparison.Ordinal);
            Assert.Contains("RentOutCommand", xaml, StringComparison.Ordinal);
            Assert.Contains("ToggleCheckOutCommand", xaml, StringComparison.Ordinal);
            Assert.Contains("PlaceReservationCommand", xaml, StringComparison.Ordinal);
            Assert.Contains("PrintDetailsCommand", xaml, StringComparison.Ordinal);
            Assert.Contains("OpenCheckoutHistoryCommand", xaml, StringComparison.Ordinal);
            Assert.Contains("OpenRentalHistoryCommand", xaml, StringComparison.Ordinal);
            Assert.Contains("CloseCommand", xaml, StringComparison.Ordinal);
        }

        [Fact]
        public void ItemDetailsViewModel_RoutesCheckoutHistoryToStructuredDialog()
        {
            var viewModel = ReadRepoFile("InventoryManagementApp", "ViewModels", "ItemDetailsViewModel.cs");
            var dialogService = ReadRepoFile("InventoryManagementApp", "Services", "DialogService.cs");
            var dialogInterface = ReadRepoFile("InventoryManagementApp", "Interfaces", "IDialogService.cs");

            Assert.Contains("_activityLogService.GetCheckoutHistoryForItemAsync(ItemModel.ItemID, ItemModel.ItemNumber)", viewModel, StringComparison.Ordinal);
            Assert.Contains("_dialogService.ShowCheckoutHistory(ItemModel, logs);", viewModel, StringComparison.Ordinal);
            Assert.DoesNotContain("string.Join(Environment.NewLine, lines)", viewModel, StringComparison.Ordinal);
            Assert.DoesNotContain("Select(log => $\"{log.Timestamp:yyyy-MM-dd HH:mm} -", viewModel, StringComparison.Ordinal);
            Assert.Contains("void ShowCheckoutHistory(ItemModel item, IEnumerable<ActivityLog> logs)", dialogInterface, StringComparison.Ordinal);
            Assert.Contains("public void ShowCheckoutHistory(ItemModel item, IEnumerable<ActivityLog> logs)", dialogService, StringComparison.Ordinal);
            Assert.Contains("new CheckoutHistoryWindow(item, logs)", dialogService, StringComparison.Ordinal);
            Assert.Contains("InvokeOnDispatcher(() => ShowCheckoutHistoryCore(item, logs));", dialogService, StringComparison.Ordinal);
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