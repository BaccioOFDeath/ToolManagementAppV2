using System;
using System.IO;
using Xunit;

namespace InventoryManagementApp.Tests
{
    public class ManageItemsPageResponsiveContractTests
    {
        [Fact]
        public void ManageItemsPage_KeepsDirectorySummaryCardsWrappedAndBounded()
        {
            var xaml = ReadRepoFile("InventoryManagementApp", "Views", "Pages", "ManageItemsPage.xaml");

            Assert.Contains("<Border Grid.Row=\"0\" Style=\"{StaticResource PageHeaderBand}\" Background=\"{DynamicResource PageHeaderManageItemsBrush}\">", xaml, StringComparison.Ordinal);
            Assert.Contains("<WrapPanel Style=\"{StaticResource PageHeaderStatsPanel}\" Margin=\"12,0,0,0\">", xaml, StringComparison.Ordinal);
            Assert.Contains("<Style x:Key=\"DirectoryStatCard\" TargetType=\"Border\" BasedOn=\"{StaticResource PageHeaderStatCard}\"/>", xaml, StringComparison.Ordinal);
            Assert.Contains("DirectoryStatValueText", xaml, StringComparison.Ordinal);
            Assert.Contains("Virtualized directory rows currently in memory", xaml, StringComparison.Ordinal);
            Assert.DoesNotContain("<Grid Grid.Row=\"1\" Margin=\"0,0,0,5\">", xaml, StringComparison.Ordinal);
            Assert.DoesNotContain("<WrapPanel Grid.Row=\"1\" Margin=\"0,0,0,5\">", xaml, StringComparison.Ordinal);
            Assert.DoesNotContain("<ColumnDefinition Width=\"1.25*\"/>", xaml, StringComparison.Ordinal);
        }

        [Fact]
        public void ManageItemsPage_WrapsHeaderActionsAndFilterControls()
        {
            var xaml = ReadRepoFile("InventoryManagementApp", "Views", "Pages", "ManageItemsPage.xaml");

            Assert.Contains("<StackPanel DockPanel.Dock=\"Left\" VerticalAlignment=\"Center\" MinWidth=\"220\" MaxWidth=\"460\">", xaml, StringComparison.Ordinal);
            Assert.Contains("Content=\"New\" Command=\"{Binding NewItemCommand}\" Margin=\"0,0,4,4\"", xaml, StringComparison.Ordinal);
            Assert.Contains("Content=\"Mobile Capture\" Command=\"{Binding OpenMobileCaptureCommand}\" Margin=\"0,0,4,4\"", xaml, StringComparison.Ordinal);
            Assert.Contains("<WrapPanel DockPanel.Dock=\"Left\" VerticalAlignment=\"Center\">", xaml, StringComparison.Ordinal);
            Assert.Contains("<WrapPanel DockPanel.Dock=\"Right\" VerticalAlignment=\"Center\" HorizontalAlignment=\"Right\" Margin=\"12,0,0,0\">", xaml, StringComparison.Ordinal);
            Assert.Contains("<pages:SearchBar Width=\"240\"", xaml, StringComparison.Ordinal);
            Assert.Contains("MinWidth=\"180\"", xaml, StringComparison.Ordinal);
            Assert.DoesNotContain("<StackPanel DockPanel.Dock=\"Right\" Orientation=\"Horizontal\"", xaml, StringComparison.Ordinal);
            Assert.DoesNotContain("<StackPanel DockPanel.Dock=\"Left\" Orientation=\"Horizontal\"", xaml, StringComparison.Ordinal);
        }

        [Fact]
        public void ManageItemsPage_AvoidsLargeFixedMinimumsInMainItemSplit()
        {
            var xaml = ReadRepoFile("InventoryManagementApp", "Views", "Pages", "ManageItemsPage.xaml");

            Assert.Contains("<ColumnDefinition Width=\"1.7*\" MinWidth=\"0\"/>", xaml, StringComparison.Ordinal);
            Assert.Contains("<ColumnDefinition Width=\"0.95*\" MinWidth=\"300\"/>", xaml, StringComparison.Ordinal);
            Assert.Contains("<Border Grid.Column=\"0\" Style=\"{StaticResource Card}\" Padding=\"0\" MinWidth=\"0\">", xaml, StringComparison.Ordinal);
            Assert.Contains("<Border Grid.Column=\"2\" Style=\"{StaticResource Card}\" Padding=\"0\" MinWidth=\"0\">", xaml, StringComparison.Ordinal);
            Assert.DoesNotContain("<ColumnDefinition Width=\"3.4*\" MinWidth=\"620\"/>", xaml, StringComparison.Ordinal);
            Assert.DoesNotContain("<ColumnDefinition Width=\"1*\" MinWidth=\"250\"/>", xaml, StringComparison.Ordinal);
        }

        [Fact]
        public void ManageItemsPage_EnablesDirectoryGridVirtualizationScrollingAndFullRowSelection()
        {
            var xaml = ReadRepoFile("InventoryManagementApp", "Views", "Pages", "ManageItemsPage.xaml");

            Assert.Contains("x:Name=\"ItemDirectoryGrid\"", xaml, StringComparison.Ordinal);
            Assert.Contains("EnableRowVirtualization=\"True\"", xaml, StringComparison.Ordinal);
            Assert.Contains("EnableColumnVirtualization=\"True\"", xaml, StringComparison.Ordinal);
            Assert.Contains("VirtualizingPanel.IsVirtualizing=\"True\"", xaml, StringComparison.Ordinal);
            Assert.Contains("VirtualizingPanel.VirtualizationMode=\"Recycling\"", xaml, StringComparison.Ordinal);
            Assert.Contains("VirtualizingPanel.ScrollUnit=\"Item\"", xaml, StringComparison.Ordinal);
            Assert.Contains("VirtualizingPanel.CacheLength=\"1\"", xaml, StringComparison.Ordinal);
            Assert.Contains("VirtualizingPanel.CacheLengthUnit=\"Page\"", xaml, StringComparison.Ordinal);
            Assert.Contains("RowDetailsVisibilityMode=\"Collapsed\"", xaml, StringComparison.Ordinal);
            Assert.Contains("SelectionMode=\"Extended\"", xaml, StringComparison.Ordinal);
            Assert.Contains("SelectionUnit=\"FullRow\"", xaml, StringComparison.Ordinal);
            Assert.Contains("ScrollViewer.CanContentScroll=\"True\"", xaml, StringComparison.Ordinal);
            Assert.Contains("ScrollViewer.HorizontalScrollBarVisibility=\"Auto\"", xaml, StringComparison.Ordinal);
            Assert.Contains("ScrollViewer.VerticalScrollBarVisibility=\"Auto\"", xaml, StringComparison.Ordinal);
            Assert.Contains("ScrollViewer.ScrollChanged=\"ItemDirectoryGrid_ScrollChanged\"", xaml, StringComparison.Ordinal);
            Assert.Contains("Source=\"{Binding IsAsync=True, Converter={StaticResource NullToDefaultImageConverter}, ConverterParameter=item}\"", xaml, StringComparison.Ordinal);
        }

        [Fact]
        public void ManageItemsPage_KeepsGridProfessionalAndOperatorAdjustable()
        {
            var xaml = ReadRepoFile("InventoryManagementApp", "Views", "Pages", "ManageItemsPage.xaml");

            Assert.Contains("HeadersVisibility=\"Column\"", xaml, StringComparison.Ordinal);
            Assert.Contains("GridLinesVisibility=\"Horizontal\"", xaml, StringComparison.Ordinal);
            Assert.Contains("CanUserResizeColumns=\"True\"", xaml, StringComparison.Ordinal);
            Assert.Contains("CanUserReorderColumns=\"True\"", xaml, StringComparison.Ordinal);
            Assert.Contains("CanUserSortColumns=\"True\"", xaml, StringComparison.Ordinal);
            Assert.Contains("ClipboardCopyMode=\"IncludeHeader\"", xaml, StringComparison.Ordinal);
            Assert.Contains("DirectoryGridTextBlockStyle", xaml, StringComparison.Ordinal);
            Assert.Contains("<Setter Property=\"TextTrimming\" Value=\"CharacterEllipsis\"/>", xaml, StringComparison.Ordinal);
            Assert.Contains("<Setter Property=\"ToolTip\" Value=\"{Binding Text, RelativeSource={RelativeSource Self}}\"/>", xaml, StringComparison.Ordinal);
            Assert.Contains("Header=\"Part Number\"", xaml, StringComparison.Ordinal);
            Assert.Contains("Header=\"Quantity\"", xaml, StringComparison.Ordinal);
            Assert.Contains("Header=\"Unit Price\"", xaml, StringComparison.Ordinal);
            Assert.Contains("ToolTip=\"{Binding AvailabilityDetail}\"", xaml, StringComparison.Ordinal);
            Assert.DoesNotContain("Header=\"Part #\"", xaml, StringComparison.Ordinal);
            Assert.DoesNotContain("Header=\"Qty\"", xaml, StringComparison.Ordinal);
        }

        [Fact]
        public void ManageItemsPage_BoundsEmptyStateHandoffScrollingAndFooterStatus()
        {
            var xaml = ReadRepoFile("InventoryManagementApp", "Views", "Pages", "ManageItemsPage.xaml");

            Assert.Contains("<Border Grid.Row=\"2\" MaxWidth=\"320\" MinHeight=\"120\" Margin=\"12\"", xaml, StringComparison.Ordinal);
            Assert.Contains("<ScrollViewer VerticalScrollBarVisibility=\"Auto\" HorizontalScrollBarVisibility=\"Disabled\">", xaml, StringComparison.Ordinal);
            Assert.Contains("<WrapPanel>", xaml, StringComparison.Ordinal);
            Assert.Contains("Text=\"Loaded Rows\"", xaml, StringComparison.Ordinal);
            Assert.Contains("Text=\"Page Size\"", xaml, StringComparison.Ordinal);
            Assert.Contains("Text=\"Pending Edits\"", xaml, StringComparison.Ordinal);
            Assert.Contains("Text=\"Missing Images\"", xaml, StringComparison.Ordinal);
            Assert.DoesNotContain("Loaded rows: {0}", xaml, StringComparison.Ordinal);
            Assert.DoesNotContain("<Border Grid.Row=\"2\" Width=\"300\"", xaml, StringComparison.Ordinal);
            Assert.DoesNotContain("VerticalScrollBarVisibility=\"Hidden\"", xaml, StringComparison.Ordinal);
            Assert.DoesNotContain("<DockPanel LastChildFill=\"False\">\n                <TextBlock DockPanel.Dock=\"Left\" Text=\"{Binding PendingEdits.Count", xaml, StringComparison.Ordinal);
        }

        [Fact]
        public void ManageItemsPage_DisplaysAndGatesDirectoryLoadingState()
        {
            var xaml = ReadRepoFile("InventoryManagementApp", "Views", "Pages", "ManageItemsPage.xaml");

            Assert.Contains("LoadingAwarePrimaryButton", xaml, StringComparison.Ordinal);
            Assert.Contains("LoadingAwareGhostButton", xaml, StringComparison.Ordinal);
            Assert.Contains("DirectoryStatusValueText", xaml, StringComparison.Ordinal);
            Assert.Contains("DirectoryStatusDetailText", xaml, StringComparison.Ordinal);
            Assert.Contains("<TextBlock Text=\"Directory Status\"", xaml, StringComparison.Ordinal);
            Assert.Contains("<Setter Property=\"Text\" Value=\"Loading rows\"/>", xaml, StringComparison.Ordinal);
            Assert.Contains("<Setter Property=\"IsEnabled\" Value=\"False\"/>", xaml, StringComparison.Ordinal);
            Assert.Contains("IsEnabled=\"{Binding IsDirectoryBusy, Converter={StaticResource InverseBooleanConverter}}\"", xaml, StringComparison.Ordinal);
            Assert.Contains("Loading item rows", xaml, StringComparison.Ordinal);
            Assert.Contains("Visibility=\"{Binding IsDirectoryBusy, Converter={StaticResource BoolToVis}}\"", xaml, StringComparison.Ordinal);
            Assert.Contains("Row actions will resume when the current page is ready", xaml, StringComparison.Ordinal);
        }

        [Fact]
        public void ManageItemsPage_PreservesPrimaryItemCommandsAndRowHandlers()
        {
            var xaml = ReadRepoFile("InventoryManagementApp", "Views", "Pages", "ManageItemsPage.xaml");

            Assert.Contains("NewItemCommand", xaml, StringComparison.Ordinal);
            Assert.Contains("OpenMobileCaptureCommand", xaml, StringComparison.Ordinal);
            Assert.Contains("EditItemCommand", xaml, StringComparison.Ordinal);
            Assert.Contains("ViewDetailsCommand", xaml, StringComparison.Ordinal);
            Assert.Contains("OpenRentalHistoryCommand", xaml, StringComparison.Ordinal);
            Assert.Contains("DeleteSelectedItemCommand", xaml, StringComparison.Ordinal);
            Assert.Contains("CommitChangesCommand", xaml, StringComparison.Ordinal);
            Assert.Contains("SearchCommand", xaml, StringComparison.Ordinal);
            Assert.Contains("DataGridRow_PreviewMouseRightButtonDown", xaml, StringComparison.Ordinal);
            Assert.Contains("DataGridRow_MouseDoubleClick", xaml, StringComparison.Ordinal);
        }

        [Fact]
        public void ManageItemsPage_CodeBehindGuardsStartupAndRowActionsWhileLoading()
        {
            var codeBehind = ReadRepoFile("InventoryManagementApp", "Views", "Pages", "ManageItemsPage.xaml.cs");

            Assert.Contains("private ItemsViewModel? _loadedViewModel;", codeBehind, StringComparison.Ordinal);
            Assert.Contains("DataContextChanged += ManageItemsPage_DataContextChanged;", codeBehind, StringComparison.Ordinal);
            Assert.Contains("PreviewKeyDown += ManageItemsPage_PreviewKeyDown;", codeBehind, StringComparison.Ordinal);
            Assert.Contains("if (ReferenceEquals(_loadedViewModel, vm))", codeBehind, StringComparison.Ordinal);
            Assert.Contains("if (vm.IsDirectoryBusy || vm.Items.Count > 0)", codeBehind, StringComparison.Ordinal);
            Assert.Contains("await System.Windows.Threading.Dispatcher.Yield(System.Windows.Threading.DispatcherPriority.Background);", codeBehind, StringComparison.Ordinal);
            Assert.Contains("if (!vm.IsDirectoryBusy && vm.Items.Count == 0 && vm.Items.HasMoreItems)", codeBehind, StringComparison.Ordinal);
            Assert.Contains("if (IsItemDirectoryBusy())", codeBehind, StringComparison.Ordinal);
            Assert.Contains("e.Handled = true;", codeBehind, StringComparison.Ordinal);
            Assert.Contains("return DataContext is ItemsViewModel { IsDirectoryBusy: true };", codeBehind, StringComparison.Ordinal);
            Assert.Contains("private async void ItemDirectoryGrid_ScrollChanged(object sender, ScrollChangedEventArgs e)", codeBehind, StringComparison.Ordinal);
            Assert.Contains("await vm.LoadMoreAsync(_loadCts.Token);", codeBehind, StringComparison.Ordinal);
            Assert.DoesNotContain("private bool _isLoadedForCurrentLifetime;", codeBehind, StringComparison.Ordinal);
            Assert.DoesNotContain("if (_isLoadedForCurrentLifetime)", codeBehind, StringComparison.Ordinal);
        }

        [Fact]
        public void ItemsViewModel_SuppressesSavedOptionReloadsDuringInitializationAndPublishesBusyState()
        {
            var source = ReadRepoFile("InventoryManagementApp", "ViewModels", "ItemsViewModel.cs");

            Assert.Contains("private bool _suppressViewOptionRefresh;", source, StringComparison.Ordinal);
            Assert.Contains("private const int DefaultInteractivePageSize = 40;", source, StringComparison.Ordinal);
            Assert.Contains("private const int MaxInteractivePageSize = 60;", source, StringComparison.Ordinal);
            Assert.Contains("public bool IsDirectoryBusy => IsInitializing || Items.IsLoading;", source, StringComparison.Ordinal);
            Assert.Contains("IsInitializing = true;", source, StringComparison.Ordinal);
            Assert.Contains("_suppressViewOptionRefresh = true;", source, StringComparison.Ordinal);
            Assert.Contains("_suppressViewOptionRefresh = false;", source, StringComparison.Ordinal);
            Assert.Contains("private static int NormalizeInteractivePageSize(int value)", source, StringComparison.Ordinal);
            Assert.Contains("if (_suppressViewOptionRefresh)", source, StringComparison.Ordinal);
            Assert.Contains("((INotifyPropertyChanged)Items).PropertyChanged += Items_PropertyChanged;", source, StringComparison.Ordinal);
            Assert.Contains("OnPropertyChanged(nameof(IsDirectoryBusy));", source, StringComparison.Ordinal);
        }

        [Fact]
        public void MainViewModel_NavigatesToManageItemsBeforeDirectoryRowsLoad()
        {
            var source = ReadRepoFile("InventoryManagementApp", "ViewModels", "MainViewModel.cs");
            var command = ExtractBlock(source, "OpenManageItemsCommand = new AsyncRelayCommand", "OpenRentalsCommand = new AsyncRelayCommand");

            Assert.Contains("CurrentPage = page;", command, StringComparison.Ordinal);
            Assert.Contains("await Task.Yield();", command, StringComparison.Ordinal);
            Assert.DoesNotContain("await vm.InitializeAsync();", command, StringComparison.Ordinal);
            Assert.DoesNotContain("await vm.LoadMoreAsync();", command, StringComparison.Ordinal);
        }

        [Fact]
        public void ManageItemsPage_ProvidesKeyboardShortcutsThroughCommandAvailability()
        {
            var codeBehind = ReadRepoFile("InventoryManagementApp", "Views", "Pages", "ManageItemsPage.xaml.cs");

            Assert.Contains("private void ManageItemsPage_PreviewKeyDown(object sender, KeyEventArgs e)", codeBehind, StringComparison.Ordinal);
            Assert.Contains("IsManagedDirectoryShortcut(e)", codeBehind, StringComparison.Ordinal);
            Assert.Contains("Keyboard.Modifiers == ModifierKeys.Control && e.Key == Key.N && vm.NewItemCommand.CanExecute(null)", codeBehind, StringComparison.Ordinal);
            Assert.Contains("Keyboard.Modifiers == ModifierKeys.Control && e.Key == Key.M && vm.OpenMobileCaptureCommand.CanExecute(null)", codeBehind, StringComparison.Ordinal);
            Assert.Contains("Keyboard.Modifiers == ModifierKeys.Control && e.Key == Key.E && vm.EditItemCommand.CanExecute(null)", codeBehind, StringComparison.Ordinal);
            Assert.Contains("Keyboard.Modifiers == ModifierKeys.Control && e.Key == Key.D && vm.ViewDetailsCommand.CanExecute(null)", codeBehind, StringComparison.Ordinal);
            Assert.Contains("Keyboard.Modifiers == ModifierKeys.Control && e.Key == Key.H && vm.OpenRentalHistoryCommand.CanExecute(null)", codeBehind, StringComparison.Ordinal);
            Assert.Contains("Keyboard.Modifiers == ModifierKeys.Control && e.Key == Key.S && vm.CommitChangesCommand.CanExecute(null)", codeBehind, StringComparison.Ordinal);
            Assert.Contains("Keyboard.Modifiers == ModifierKeys.None && e.Key == Key.Delete && vm.DeleteItemsCommand.CanExecute(ItemDirectoryGrid.SelectedItems)", codeBehind, StringComparison.Ordinal);
            Assert.Contains("Keyboard.Modifiers == ModifierKeys.None && e.Key == Key.Enter && vm.ViewDetailsCommand.CanExecute(null)", codeBehind, StringComparison.Ordinal);
            Assert.Contains("UiActionGuard.RunAsync(this, \"Manage Items\", async () => await vm.NewItemCommand.ExecuteAsync(null));", codeBehind, StringComparison.Ordinal);
            Assert.Contains("UiActionGuard.Run(this, \"Manage Items\", () => vm.ViewDetailsCommand.Execute(null));", codeBehind, StringComparison.Ordinal);
            Assert.Contains("return e.Key is Key.N or Key.M or Key.E or Key.D or Key.H or Key.S;", codeBehind, StringComparison.Ordinal);
            Assert.Contains("return Keyboard.Modifiers == ModifierKeys.None && e.Key is Key.Delete or Key.Enter;", codeBehind, StringComparison.Ordinal);
        }

        [Fact]
        public void SharedConverters_RegisterInverseBooleanConverterForLoadingState()
        {
            var converters = ReadRepoFile("InventoryManagementApp", "Resources", "Converters.xaml");

            Assert.Contains("<conv:InverseBooleanConverter x:Key=\"InverseBooleanConverter\"/>", converters, StringComparison.Ordinal);
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

        private static string ExtractBlock(string source, string start, string end)
        {
            var startIndex = source.IndexOf(start, StringComparison.Ordinal);
            Assert.True(startIndex >= 0, $"Could not find start marker: {start}");
            var endIndex = source.IndexOf(end, startIndex + start.Length, StringComparison.Ordinal);
            Assert.True(endIndex > startIndex, $"Could not find end marker: {end}");
            return source[startIndex..endIndex];
        }
    }
}
