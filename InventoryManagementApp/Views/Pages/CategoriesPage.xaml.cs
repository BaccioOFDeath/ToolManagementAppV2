// Views/Pages/CategoriesPage.xaml.cs
using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Documents;
using System.Windows.Input;
using InventoryManagementApp.ViewModels;
using Microsoft.Extensions.DependencyInjection;
using WpfMessageBox = System.Windows.MessageBox;
using WpfPrintDialog = System.Windows.Controls.PrintDialog;

namespace InventoryManagementApp.Views.Pages
{
    public partial class CategoriesPage : Page
    {
        public CategoriesPage(int inventoryId)
        {
            InitializeComponent();
            var sp = ((App)System.Windows.Application.Current).Host.Services;
            var vm = sp.GetRequiredService<CategoryManagementViewModel>();
            DataContext = vm;
            Loaded += async (_, __) =>
            {
                vm.SelectedInventoryId = inventoryId;
                await vm.InitializeAsync().ConfigureAwait(false);
            };
        }

        private CategoryManagementViewModel? ViewModel => DataContext as CategoryManagementViewModel;

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

            if (e.Key == Key.Enter && !IsTextInputFocused())
            {
                OpenCategoryDetail_Click(sender, e);
                e.Handled = true;
            }
        }

        private static bool IsTextInputFocused()
        {
            return Keyboard.FocusedElement is System.Windows.Controls.Primitives.TextBoxBase or PasswordBox or System.Windows.Controls.ComboBox;
        }

        private void CategoryRow_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            OpenCategoryDetail_Click(sender, e);
        }

        private void CategoryRow_PreviewMouseRightButtonDown(object sender, MouseButtonEventArgs e)
        {
            if (sender is DataGridRow row && !row.IsSelected)
            {
                row.Focus();
                row.IsSelected = true;
            }
        }

        private void OpenCategoryDetail_Click(object sender, RoutedEventArgs e)
        {
            if (!TryGetSelectedCategory(out var category))
                return;

            WpfMessageBox.Show(FormatCategoryDetail(category), $"Category Detail - {category.Name}", MessageBoxButton.OK, MessageBoxImage.Information);
        }

        private void CopyCategory_Click(object sender, RoutedEventArgs e)
        {
            if (!TryGetSelectedCategory(out var category))
                return;

            System.Windows.Clipboard.SetText(FormatCategoryDetail(category));
        }

        private void PrintCategories_Click(object sender, RoutedEventArgs e)
        {
            if (DataContext is not CategoryManagementViewModel vm || vm.FilteredCategories.Count == 0)
            {
                WpfMessageBox.Show("There are no categories to print.", "Category Directory", MessageBoxButton.OK, MessageBoxImage.Information);
                return;
            }

            var document = BuildDirectoryPrintDocument(vm.FilteredCategories.ToList(), vm.CategoryResultsSummary, vm.CategorySetupSummary);
            PrintDocument(document, "Category Directory");
        }

        private void PrintSelectedCategory_Click(object sender, RoutedEventArgs e)
        {
            if (!TryGetSelectedCategory(out var category))
                return;

            var document = BuildSelectedCategoryPrintDocument(category);
            PrintDocument(document, $"Category Sheet - {category.Name}");
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

        private static void PrintDocument(FlowDocument document, string title)
        {
            var printDialog = new WpfPrintDialog();
            if (printDialog.ShowDialog() != true)
                return;

            document.PageWidth = printDialog.PrintableAreaWidth;
            document.PageHeight = printDialog.PrintableAreaHeight;
            document.PagePadding = new Thickness(36);
            document.ColumnWidth = printDialog.PrintableAreaWidth;
            printDialog.PrintDocument(((IDocumentPaginatorSource)document).DocumentPaginator, title);
        }

        private static string FormatCategoryDetail(CategoryManagementViewModel.CategoryItem category)
        {
            return $"Category #: {category.CategoryID}{Environment.NewLine}" +
                   $"Name: {category.Name}{Environment.NewLine}" +
                   $"Directory label: {category.DirectoryLabel}{Environment.NewLine}{Environment.NewLine}" +
                   "Admin handoff: confirm the category name matches staff language, assign matching inventory records, review search/filter coverage, and remove obsolete duplicates.";
        }

        private static FlowDocument BuildDirectoryPrintDocument(IReadOnlyCollection<CategoryManagementViewModel.CategoryItem> categories, string summary, string setupSummary)
        {
            var document = CreateBaseDocument();

            document.Blocks.Add(new Paragraph(new Run("Category Directory"))
            {
                FontSize = 16,
                FontWeight = FontWeights.SemiBold,
                Margin = new Thickness(0, 0, 0, 4)
            });
            document.Blocks.Add(new Paragraph(new Run($"Printed {DateTime.Now:g} - {summary}. {setupSummary}"))
            {
                FontSize = 10,
                Margin = new Thickness(0, 0, 0, 10)
            });

            var table = new Table { CellSpacing = 0 };
            table.Columns.Add(new TableColumn { Width = new GridLength(90) });
            table.Columns.Add(new TableColumn { Width = new GridLength(220) });
            table.Columns.Add(new TableColumn { Width = new GridLength(280) });

            var rowGroup = new TableRowGroup();
            table.RowGroups.Add(rowGroup);

            var header = new TableRow { FontWeight = FontWeights.SemiBold };
            rowGroup.Rows.Add(header);
            AddCell(header, "ID");
            AddCell(header, "Category");
            AddCell(header, "Admin Handoff");

            foreach (var category in categories)
            {
                var row = new TableRow();
                rowGroup.Rows.Add(row);
                AddCell(row, category.CategoryID.ToString());
                AddCell(row, category.Name);
                AddCell(row, "Verify item assignment and search/filter coverage.");
            }

            document.Blocks.Add(table);
            return document;
        }

        private static FlowDocument BuildSelectedCategoryPrintDocument(CategoryManagementViewModel.CategoryItem category)
        {
            var document = CreateBaseDocument();

            document.Blocks.Add(new Paragraph(new Run($"Category Sheet - {category.Name}"))
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
