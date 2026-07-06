// Views/Pages/CategoriesPage.xaml.cs
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Threading;
using InventoryManagementApp.ViewModels;
using InventoryManagementApp.Views.Windows;
using Microsoft.Extensions.DependencyInjection;
using WpfMessageBox = System.Windows.MessageBox;

namespace InventoryManagementApp.Views.Pages
{
    public partial class CategoriesPage : Page
    {
        private const int MaxDirectoryPrintRows = 250;
        private readonly int _inventoryId;
        private Task? _initializeCategoriesTask;
        private CategoryManagementViewModel? _initializedViewModel;
        private CancellationTokenSource? _initializeCategoriesCancellation;
        private int _initializeCategoriesVersion;

        public CategoriesPage(int inventoryId)
        {
            InitializeComponent();
            _inventoryId = inventoryId;
            var sp = ((App)System.Windows.Application.Current).Host.Services;
            var vm = sp.GetRequiredService<CategoryManagementViewModel>();
            DataContext = vm;
            Loaded += CategoriesPage_Loaded;
            Unloaded += CategoriesPage_Unloaded;
            DataContextChanged += CategoriesPage_DataContextChanged;
            CategoryGrid.ContextMenuOpening += CategoryGrid_ContextMenuOpening;
        }

        private CategoryManagementViewModel? ViewModel => DataContext as CategoryManagementViewModel;

        private async void CategoriesPage_Loaded(object sender, RoutedEventArgs e)
        {
            Focus();
            FindBox.Focus();
            FindBox.SelectAll();

            if (DataContext is CategoryManagementViewModel vm)
            {
                await InitializeCategoriesOnceAsync(vm);
            }
        }

        private void CategoriesPage_Unloaded(object sender, RoutedEventArgs e)
        {
            CancelPageOwnedInitialization();
        }

        private void CategoriesPage_DataContextChanged(object sender, DependencyPropertyChangedEventArgs e)
        {
            if (!ReferenceEquals(_initializedViewModel, e.NewValue))
            {
                CancelPageOwnedInitialization();
                _initializedViewModel = null;
                _initializeCategoriesTask = null;
            }
        }

        private async Task InitializeCategoriesOnceAsync(CategoryManagementViewModel vm)
        {
            if (ReferenceEquals(_initializedViewModel, vm) && _initializeCategoriesTask is { IsCompleted: false })
            {
                await _initializeCategoriesTask;
                return;
            }

            if (ReferenceEquals(_initializedViewModel, vm) && _initializeCategoriesTask is { IsCompletedSuccessfully: true })
            {
                return;
            }

            CancelPageOwnedInitialization();
            _initializedViewModel = vm;
            vm.SelectedInventoryId = _inventoryId;
            var loadVersion = ++_initializeCategoriesVersion;
            _initializeCategoriesCancellation = new CancellationTokenSource();
            var cancellationToken = _initializeCategoriesCancellation.Token;

            await Dispatcher.Yield(DispatcherPriority.Background);

            if (!IsCurrentCategoryInitialization(vm, loadVersion, cancellationToken) || vm.IsCategoryInteractionBusy)
            {
                if (!ReferenceEquals(DataContext, vm) || vm.IsCategoryInteractionBusy)
                    return;

                return;
            }

            _initializeCategoriesTask = vm.InitializeAsync();
            await _initializeCategoriesTask;

            if (!IsCurrentCategoryInitialization(vm, loadVersion, cancellationToken))
            {
                return;
            }
        }

        private bool IsCurrentCategoryInitialization(CategoryManagementViewModel vm, int loadVersion, CancellationToken cancellationToken)
        {
            return !cancellationToken.IsCancellationRequested &&
                   loadVersion == _initializeCategoriesVersion &&
                   ReferenceEquals(DataContext, vm);
        }

        private void CancelPageOwnedInitialization()
        {
            _initializeCategoriesVersion++;
            _initializeCategoriesCancellation?.Cancel();
            _initializeCategoriesCancellation?.Dispose();
            _initializeCategoriesCancellation = null;
        }

        private void Page_PreviewKeyDown(object sender, System.Windows.Input.KeyEventArgs e)
        {
            if (ViewModel == null) return;

            if (Keyboard.Modifiers == ModifierKeys.Control && e.Key == Key.F)
            {
                FindBox.Focus();
                FindBox.SelectAll();
                e.Handled = true;
                return;
            }

            if (Keyboard.Modifiers == ModifierKeys.Control && e.Key == Key.N)
            {
                CategoryNameBox.Focus();
                CategoryNameBox.SelectAll();
                e.Handled = true;
                return;
            }

            if (ViewModel.IsCategoryInteractionBusy && IsCategoryActionShortcut(e))
            {
                e.Handled = true;
                return;
            }

            if (IsTextInputFocused() && IsCategoryActionShortcut(e))
            {
                return;
            }

            if (Keyboard.Modifiers == ModifierKeys.Control && e.Key == Key.R && ViewModel.RefreshCommand.CanExecute(null))
            {
                ViewModel.RefreshCommand.Execute(null);
                e.Handled = true;
                return;
            }

            if (Keyboard.Modifiers == ModifierKeys.Control && e.Key == Key.S && ViewModel.SaveCommand.CanExecute(null))
            {
                ViewModel.SaveCommand.Execute(null);
                e.Handled = true;
                return;
            }

            if (Keyboard.Modifiers == ModifierKeys.Control && e.Key == Key.P)
            {
                PrintCategories_Click(sender, e);
                e.Handled = true;
                return;
            }

            if (Keyboard.Modifiers == ModifierKeys.Control && e.Key == Key.C)
            {
                CopyCategory_Click(sender, e);
                e.Handled = true;
                return;
            }

            if (e.Key == Key.Delete && ViewModel.DeleteCommand.CanExecute(null))
            {
                ViewModel.DeleteCommand.Execute(null);
                e.Handled = true;
                return;
            }

            if (e.Key == Key.Enter)
            {
                OpenCategoryDetail_Click(sender, e);
                e.Handled = true;
            }
        }

        private static bool IsCategoryActionShortcut(KeyEventArgs e)
        {
            if (Keyboard.Modifiers == ModifierKeys.Control)
            {
                return e.Key is Key.R or Key.S or Key.P or Key.C;
            }

            return Keyboard.Modifiers == ModifierKeys.None && e.Key is Key.Enter or Key.Delete;
        }

        private static bool IsTextInputFocused()
        {
            return Keyboard.FocusedElement is TextBoxBase or PasswordBox or ComboBox;
        }

        private void CategoryRow_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            if (ViewModel is { IsCategoryInteractionBusy: true })
            {
                e.Handled = true;
                return;
            }

            if (GridContextMenuSelection.SelectRow(sender, e) == null)
                return;

            OpenCategoryDetail_Click(sender, e);
            e.Handled = true;
        }

        private void CategoryRow_PreviewMouseRightButtonDown(object sender, MouseButtonEventArgs e)
        {
            if (ViewModel is { IsCategoryInteractionBusy: true })
            {
                e.Handled = true;
                return;
            }

            GridContextMenuSelection.SelectRow(sender, e);
        }

        private void CategoryGrid_ContextMenuOpening(object sender, ContextMenuEventArgs e)
        {
            if (ViewModel is { IsCategoryInteractionBusy: true })
            {
                e.Handled = true;
            }
        }

        private void OpenCategoryDetail_Click(object sender, RoutedEventArgs e)
        {
            UiActionGuard.Run(this, "Category Detail", () =>
            {
                if (!AreCategoryRowsReady("Category Detail") || !TryGetSelectedCategory(out var category))
                    return;

                DetailDialogWindow.ShowDialogFor(
                    Window.GetWindow(this),
                    $"Category Detail - {category.Name}",
                    "Category Detail",
                    FormatCategoryDetail(category),
                    "Review naming, directory labeling, and setup handoff guidance before changing category structure.",
                    $"Category #{category.CategoryID}",
                    "Close returns to Categories with the selected category ready for copy, print, rename, or refresh actions.");
            });
        }

        private void CopyCategory_Click(object sender, RoutedEventArgs e)
        {
            UiActionGuard.Run(this, "Category Detail", () =>
            {
                if (!AreCategoryRowsReady("Category Detail") || !TryGetSelectedCategory(out var category))
                    return;

                System.Windows.Clipboard.SetText(FormatCategoryDetail(category));
            });
        }

        private void PrintCategories_Click(object sender, RoutedEventArgs e)
        {
            UiActionGuard.Run(this, "Category Directory", () =>
            {
                if (DataContext is not CategoryManagementViewModel vm || !vm.IsDirectoryPrintAvailable)
                {
                    WpfMessageBox.Show("There are no categories ready to print. Wait for loading to finish or clear the filter.", "Category Directory", MessageBoxButton.OK, MessageBoxImage.Information);
                    return;
                }

                var visibleRowCount = vm.FilteredCategories.Count;
                var snapshot = vm.FilteredCategories.Take(MaxDirectoryPrintRows).ToList();
                var document = BuildDirectoryPrintDocument(
                    snapshot,
                    visibleRowCount,
                    vm.Categories.Count,
                    vm.SearchText,
                    vm.CategoryResultsSummary,
                    vm.CategorySetupSummary);
                ShowPrintPreview(document, "Category Directory");
            });
        }

        private void PrintSelectedCategory_Click(object sender, RoutedEventArgs e)
        {
            UiActionGuard.Run(this, "Category Sheet", () =>
            {
                if (!AreCategoryRowsReady("Category Sheet") || !TryGetSelectedCategory(out var category))
                    return;

                var document = BuildSelectedCategoryPrintDocument(category);
                ShowPrintPreview(document, $"Category Sheet - {category.Name}");
            });
        }

        private bool AreCategoryRowsReady(string title)
        {
            if (ViewModel is not { IsCategoryInteractionBusy: true })
                return true;

            WpfMessageBox.Show("Category rows are still loading. Wait for the refresh to finish before using category actions.", title, MessageBoxButton.OK, MessageBoxImage.Information);
            return false;
        }

        private bool TryGetSelectedCategory(out CategoryManagementViewModel.CategoryItem category)
        {
            if (CategoryGrid.SelectedItem is CategoryManagementViewModel.CategoryItem selected)
            {
                category = selected;
                return true;
            }

            category = null!;
            WpfMessageBox.Show("Select a category row first.", "Category Detail", MessageBoxButton.OK, MessageBoxImage.Information);
            return false;
        }

        private static void ShowPrintPreview(FlowDocument document, string title)
        {
            new PrintPreviewWindow().ShowPreview(document, title, null);
        }

        private static string FormatCategoryDetail(CategoryManagementViewModel.CategoryItem category)
        {
            return $"Category #: {category.CategoryID}{Environment.NewLine}" +
                   $"Name: {ValueOrNotRecorded(category.Name)}{Environment.NewLine}" +
                   $"Directory label: {ValueOrNotRecorded(category.DirectoryLabel)}{Environment.NewLine}{Environment.NewLine}" +
                   "Admin handoff: confirm the category name matches staff language, assign matching inventory records, review search/filter coverage, and remove obsolete duplicates.";
        }

        private static FlowDocument BuildDirectoryPrintDocument(
            IReadOnlyCollection<CategoryManagementViewModel.CategoryItem> categories,
            int visibleRowCount,
            int totalRowCount,
            string searchText,
            string summary,
            string setupSummary)
        {
            var document = CreateBaseDocument();
            var printedRowCount = categories.Count;
            var omittedRowCount = Math.Max(0, visibleRowCount - printedRowCount);
            var filterText = string.IsNullOrWhiteSpace(searchText)
                ? "No filter applied"
                : $"Filter: {searchText.Trim()}";

            document.Blocks.Add(new Paragraph(new Run("Category Directory"))
            {
                FontSize = 16,
                FontWeight = FontWeights.SemiBold,
                Margin = new Thickness(0, 0, 0, 4)
            });
            document.Blocks.Add(new Paragraph(new Run($"Printed {DateTime.Now:g} - {summary}. {setupSummary}"))
            {
                FontSize = 10,
                Margin = new Thickness(0, 0, 0, 4)
            });
            document.Blocks.Add(new Paragraph(new Run($"Rows visible: {visibleRowCount}. Rows printed: {printedRowCount}. Rows omitted: {omittedRowCount}. Total linked categories: {totalRowCount}. {filterText}."))
            {
                FontSize = 10,
                Margin = new Thickness(0, 0, 0, 8)
            });
            document.Blocks.Add(new Paragraph(new Run("Review note: verify category names, item assignments, search/filter coverage, and any omitted rows before using this packet for setup cleanup."))
            {
                FontSize = 10,
                FontStyle = FontStyles.Italic,
                Margin = new Thickness(0, 0, 0, 10)
            });

            if (printedRowCount == 0)
            {
                document.Blocks.Add(new Paragraph(new Run("No category rows were available for this directory packet."))
                {
                    Margin = new Thickness(0, 0, 0, 10)
                });
                return document;
            }

            var table = new Table { CellSpacing = 0 };
            table.Columns.Add(new TableColumn { Width = new GridLength(0.7, GridUnitType.Star) });
            table.Columns.Add(new TableColumn { Width = new GridLength(1.6, GridUnitType.Star) });
            table.Columns.Add(new TableColumn { Width = new GridLength(2.4, GridUnitType.Star) });
            table.Columns.Add(new TableColumn { Width = new GridLength(1.8, GridUnitType.Star) });

            var rowGroup = new TableRowGroup();
            table.RowGroups.Add(rowGroup);

            var header = new TableRow { FontWeight = FontWeights.SemiBold };
            rowGroup.Rows.Add(header);
            AddCell(header, "ID");
            AddCell(header, "Category");
            AddCell(header, "Directory Label");
            AddCell(header, "Admin Handoff");

            foreach (var category in categories)
            {
                var row = new TableRow();
                rowGroup.Rows.Add(row);
                AddCell(row, category.CategoryID.ToString());
                AddCell(row, ValueOrNotRecorded(category.Name));
                AddCell(row, ValueOrNotRecorded(category.DirectoryLabel));
                AddCell(row, "Verify item assignment and search/filter coverage.");
            }

            document.Blocks.Add(table);
            return document;
        }

        private static FlowDocument BuildSelectedCategoryPrintDocument(CategoryManagementViewModel.CategoryItem category)
        {
            var document = CreateBaseDocument();

            document.Blocks.Add(new Paragraph(new Run($"Category Sheet - {ValueOrNotRecorded(category.Name)}"))
            {
                FontSize = 16,
                FontWeight = FontWeights.SemiBold,
                Margin = new Thickness(0, 0, 0, 4)
            });
            document.Blocks.Add(new Paragraph(new Run($"Printed {DateTime.Now:g}"))
            {
                FontSize = 10,
                Margin = new Thickness(0, 0, 0, 10)
            });

            document.Blocks.Add(new Paragraph(new Run(FormatCategoryDetail(category)))
            {
                Margin = new Thickness(0, 0, 0, 10)
            });
            document.Blocks.Add(new Paragraph(new Run("Checklist"))
            {
                FontWeight = FontWeights.SemiBold,
                Margin = new Thickness(0, 0, 0, 4)
            });
            document.Blocks.Add(new Paragraph(new Run("[ ] Name matches staff language")) { Margin = new Thickness(0, 0, 0, 2) });
            document.Blocks.Add(new Paragraph(new Run("[ ] Matching inventory records are assigned")) { Margin = new Thickness(0, 0, 0, 2) });
            document.Blocks.Add(new Paragraph(new Run("[ ] Search and filter coverage has been checked")) { Margin = new Thickness(0, 0, 0, 2) });
            document.Blocks.Add(new Paragraph(new Run("[ ] Duplicate or obsolete categories have been removed")) { Margin = new Thickness(0, 0, 0, 2) });
            document.Blocks.Add(new Paragraph(new Run("[ ] Printed directory totals were reviewed if this category appears in a filtered packet")) { Margin = new Thickness(0, 0, 0, 2) });
            return document;
        }

        private static FlowDocument CreateBaseDocument()
        {
            return new FlowDocument
            {
                FontFamily = new System.Windows.Media.FontFamily("Segoe UI"),
                FontSize = 10
            };
        }

        private static string ValueOrNotRecorded(string? value)
        {
            return string.IsNullOrWhiteSpace(value) ? "Not recorded" : value.Trim();
        }

        private static void AddCell(TableRow row, string text)
        {
            row.Cells.Add(new TableCell(new Paragraph(new Run(text ?? string.Empty))
            {
                Margin = new Thickness(2)
            })
            {
                BorderBrush = System.Windows.Media.Brushes.Gray,
                BorderThickness = new Thickness(0, 0, 0, 0.5),
                Padding = new Thickness(3, 2, 3, 2)
            });
        }
    }
}
