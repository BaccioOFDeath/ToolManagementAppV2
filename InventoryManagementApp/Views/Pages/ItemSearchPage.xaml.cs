using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using InventoryManagementApp.Models.Domain;
using InventoryManagementApp.ViewModels;

namespace InventoryManagementApp.Views.Pages
{
    public partial class ItemSearchPage : Page
    {
        public ItemSearchPage()
        {
            InitializeComponent();
        }

        private async void Page_Loaded(object sender, RoutedEventArgs e)
        {
            UpdateState();
            if (DataContext is ItemManagementViewModel vm)
            {
                vm.SelectedCategory = "All";
                await vm.SearchCommand.ExecuteAsync(null);
            }
        }

        private void UpdateState()
        {
            VisualStateManager.GoToState(this, "Wide", true);
        }

        private void ItemGrid_MouseDoubleClick(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            if (DataContext is not ItemManagementViewModel vm || sender is not DataGrid grid || grid.SelectedItem is not ItemModel item)
                return;

            vm.SelectedItem = item;
            if (vm.ViewDetailsCommand.CanExecute(null))
                vm.ViewDetailsCommand.Execute(null);
        }

        private void PrintSearchResults_Click(object sender, RoutedEventArgs e)
        {
            if (DataContext is ItemManagementViewModel vm)
                PrintItems("Tool Search Results", vm.SearchResults);
        }

        private void PrintCheckedOut_Click(object sender, RoutedEventArgs e)
        {
            if (DataContext is ItemManagementViewModel vm)
                PrintItems("Currently Checked Out Tools", vm.CheckedOutItems);
        }

        private void PrintItems(string title, IEnumerable<ItemModel> items)
        {
            var itemList = items.ToList();
            if (itemList.Count == 0)
            {
                MessageBox.Show("There are no rows to print.", "Print", MessageBoxButton.OK, MessageBoxImage.Information);
                return;
            }

            var printDialog = new PrintDialog();
            if (printDialog.ShowDialog() != true)
                return;

            var document = BuildPrintDocument(title, itemList);
            document.PageWidth = printDialog.PrintableAreaWidth;
            document.PageHeight = printDialog.PrintableAreaHeight;
            document.PagePadding = new Thickness(36);
            document.ColumnWidth = printDialog.PrintableAreaWidth;
            printDialog.PrintDocument(((IDocumentPaginatorSource)document).DocumentPaginator, title);
        }

        private static FlowDocument BuildPrintDocument(string title, IReadOnlyCollection<ItemModel> items)
        {
            var document = new FlowDocument
            {
                FontFamily = new System.Windows.Media.FontFamily("Segoe UI"),
                FontSize = 11
            };

            document.Blocks.Add(new Paragraph(new Run(title))
            {
                FontSize = 16,
                FontWeight = FontWeights.SemiBold,
                Margin = new Thickness(0, 0, 0, 4)
            });
            document.Blocks.Add(new Paragraph(new Run($"Printed {DateTime.Now:g} - {items.Count} row(s)"))
            {
                FontSize = 10,
                Margin = new Thickness(0, 0, 0, 10)
            });

            var table = new Table { CellSpacing = 0 };
            foreach (var width in new[] { 80.0, 180.0, 90.0, 80.0, 70.0, 110.0, 110.0 })
                table.Columns.Add(new TableColumn { Width = new GridLength(width) });

            var rowGroup = new TableRowGroup();
            table.RowGroups.Add(rowGroup);
            var header = new TableRow { FontWeight = FontWeights.SemiBold };
            rowGroup.Rows.Add(header);
            AddCell(header, "Tool #");
            AddCell(header, "Name");
            AddCell(header, "Status");
            AddCell(header, "Location");
            AddCell(header, "On Hand");
            AddCell(header, "Holder");
            AddCell(header, "Out Since");

            foreach (var item in items)
            {
                var row = new TableRow();
                rowGroup.Rows.Add(row);
                AddCell(row, item.ItemNumber);
                AddCell(row, item.Name);
                AddCell(row, GetStatus(item));
                AddCell(row, item.Location);
                AddCell(row, item.QuantityOnHand.ToString());
                AddCell(row, item.CheckedOutBy);
                AddCell(row, item.CheckedOutTime?.ToString("g") ?? string.Empty);
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

        private static string GetStatus(ItemModel item)
        {
            if (item.IsIncomplete)
                return "Incomplete";
            if (item.HasNoOnHand)
                return "Unavailable";
            if (item.IsCheckedOut)
                return "Checked Out";
            if (item.HasRentedStock)
                return "Rented";
            return "Available";
        }
    }
}
