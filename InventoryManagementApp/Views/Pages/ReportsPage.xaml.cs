using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using InventoryManagementApp.ViewModels;
using WpfMessageBox = System.Windows.MessageBox;
using WpfPrintDialog = System.Windows.Controls.PrintDialog;

namespace InventoryManagementApp.Views.Pages
{
    public partial class ReportsPage : Page
    {
        public ReportsPage()
        {
            InitializeComponent();
        }

        private void ReportGrid_MouseDoubleClick(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            CopySelectedRow_Click(sender, e);
        }

        private void CopySelectedRow_Click(object sender, RoutedEventArgs e)
        {
            if (ReportGrid.SelectedItem is not ReportLine line)
            {
                WpfMessageBox.Show("Select a report row first.", "Reports", MessageBoxButton.OK, MessageBoxImage.Information);
                return;
            }

            System.Windows.Clipboard.SetText($"{line.Category}: {line.Text}{Environment.NewLine}Next action: {line.ActionHint}");
        }

        private void PrintReport_Click(object sender, RoutedEventArgs e)
        {
            if (DataContext is not ReportsViewModel vm || vm.ReportLines.Count == 0)
            {
                WpfMessageBox.Show("Run a report before printing.", "Reports", MessageBoxButton.OK, MessageBoxImage.Information);
                return;
            }

            var printDialog = new WpfPrintDialog();
            if (printDialog.ShowDialog() != true)
                return;

            var document = BuildReportDocument(vm.ReportTitle, vm.ReportSummary, vm.LastRunText, vm.ReportLines.ToList());
            document.PageWidth = printDialog.PrintableAreaWidth;
            document.PageHeight = printDialog.PrintableAreaHeight;
            document.PagePadding = new Thickness(36);
            document.ColumnWidth = printDialog.PrintableAreaWidth;
            printDialog.PrintDocument(((IDocumentPaginatorSource)document).DocumentPaginator, vm.ReportTitle);
        }

        private static FlowDocument BuildReportDocument(string title, string summary, string lastRunText, IReadOnlyCollection<ReportLine> lines)
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
            document.Blocks.Add(new Paragraph(new Run($"Printed {DateTime.Now:g} - Last run {lastRunText} - {lines.Count} row(s)"))
            {
                FontSize = 10,
                Margin = new Thickness(0, 0, 0, 4)
            });
            document.Blocks.Add(new Paragraph(new Run(summary))
            {
                Margin = new Thickness(0, 0, 0, 10)
            });

            var table = new Table { CellSpacing = 0 };
            foreach (var width in new[] { 45.0, 95.0, 330.0, 230.0 })
                table.Columns.Add(new TableColumn { Width = new GridLength(width) });

            var rowGroup = new TableRowGroup();
            table.RowGroups.Add(rowGroup);

            var header = new TableRow { FontWeight = FontWeights.SemiBold };
            rowGroup.Rows.Add(header);
            AddCell(header, "#");
            AddCell(header, "Type");
            AddCell(header, "Report Detail");
            AddCell(header, "Next Action");

            foreach (var line in lines)
            {
                var row = new TableRow();
                rowGroup.Rows.Add(row);
                AddCell(row, line.Number.ToString());
                AddCell(row, line.Category);
                AddCell(row, line.Text);
                AddCell(row, line.ActionHint);
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
