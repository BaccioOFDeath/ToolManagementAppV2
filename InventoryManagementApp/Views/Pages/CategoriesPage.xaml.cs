// Views/Pages/CategoriesPage.xaml.cs
using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
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

        private void CategoryRow_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            OpenCategoryDetail_Click(sender, e);
        }

        private void CategoryRow_PreviewMouseRightButtonDown(object sender, MouseButtonEventArgs e)
        {
            if (sender is DataGridRow row && !row.IsSelected)
            {
                row.IsSelected = true;
                e.Handled = true;
            }
        }

        private void OpenCategoryDetail_Click(object sender, RoutedEventArgs e)
        {
            if (CategoryGrid.SelectedItem is not CategoryManagementViewModel.CategoryItem category)
            {
                WpfMessageBox.Show("Select a category row first.", "Category Detail", MessageBoxButton.OK, MessageBoxImage.Information);
                return;
            }

            WpfMessageBox.Show(FormatCategoryDetail(category), $"Category Detail - {category.Name}", MessageBoxButton.OK, MessageBoxImage.Information);
        }

        private void CopyCategory_Click(object sender, RoutedEventArgs e)
        {
            if (CategoryGrid.SelectedItem is not CategoryManagementViewModel.CategoryItem category)
            {
                WpfMessageBox.Show("Select a category row first.", "Category Detail", MessageBoxButton.OK, MessageBoxImage.Information);
                return;
            }

            System.Windows.Clipboard.SetText(FormatCategoryDetail(category));
        }

        private void PrintCategories_Click(object sender, RoutedEventArgs e)
        {
            if (DataContext is not CategoryManagementViewModel vm || vm.FilteredCategories.Count == 0)
            {
                WpfMessageBox.Show("There are no categories to print.", "Category Directory", MessageBoxButton.OK, MessageBoxImage.Information);
                return;
            }

            var printDialog = new WpfPrintDialog();
            if (printDialog.ShowDialog() != true)
                return;

            var document = BuildPrintDocument(vm.FilteredCategories.ToList(), vm.CategoryResultsSummary);
            document.PageWidth = printDialog.PrintableAreaWidth;
            document.PageHeight = printDialog.PrintableAreaHeight;
            document.PagePadding = new Thickness(36);
            document.ColumnWidth = printDialog.PrintableAreaWidth;
            printDialog.PrintDocument(((IDocumentPaginatorSource)document).DocumentPaginator, "Category Directory");
        }

        private static string FormatCategoryDetail(CategoryManagementViewModel.CategoryItem category)
        {
            return $"Category #: {category.CategoryID}{Environment.NewLine}" +
                   $"Name: {category.Name}{Environment.NewLine}{Environment.NewLine}" +
                   "Next steps: assign matching inventory items to this category, review search results by category, or rename the category if advisors cannot find it quickly.";
        }

        private static FlowDocument BuildPrintDocument(IReadOnlyCollection<CategoryManagementViewModel.CategoryItem> categories, string summary)
        {
            var document = new FlowDocument
            {
                FontFamily = new System.Windows.Media.FontFamily("Segoe UI"),
                FontSize = 10
            };

            document.Blocks.Add(new Paragraph(new Run("Category Directory"))
            {
                FontSize = 16,
                FontWeight = FontWeights.SemiBold,
                Margin = new Thickness(0, 0, 0, 4)
            });
            document.Blocks.Add(new Paragraph(new Run($"Printed {DateTime.Now:g} - {summary}"))
            {
                FontSize = 10,
                Margin = new Thickness(0, 0, 0, 10)
            });

            var table = new Table { CellSpacing = 0 };
            table.Columns.Add(new TableColumn { Width = new GridLength(90) });
            table.Columns.Add(new TableColumn { Width = new GridLength(420) });

            var rowGroup = new TableRowGroup();
            table.RowGroups.Add(rowGroup);

            var header = new TableRow { FontWeight = FontWeights.SemiBold };
            rowGroup.Rows.Add(header);
            AddCell(header, "ID");
            AddCell(header, "Name");

            foreach (var category in categories)
            {
                var row = new TableRow();
                rowGroup.Rows.Add(row);
                AddCell(row, category.CategoryID.ToString());
                AddCell(row, category.Name);
            }

            document.Blocks.Add(table);
            return document;
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
