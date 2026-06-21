using System;
using System.IO;
using System.Linq;
using System.Windows;
using System.Windows.Documents;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Controls; // WPF PrintDialog
using InventoryManagementApp.ViewModels;
using InventoryManagementApp.Utilities.Extensions;

#nullable enable

namespace InventoryManagementApp.Views.Windows
{
    public partial class PrintPreviewWindow : Window
    {
        private FlowDocument? _document;
        private string _title = string.Empty;
        private string _description = string.Empty;
        private string _logoPath = string.Empty;

        public PrintPreviewWindow()
        {
            InitializeComponent();
            DataContext = new PrintPreviewViewModel(OnPageSetup, OnPrint, Close);
            this.DisposeDataContextOnUnload();
        }

        public void ShowPreview(FlowDocument document, string title, string? description = null, string? logoPath = null)
        {
            _document = document ?? throw new ArgumentNullException(nameof(document));
            _title = title ?? throw new ArgumentNullException(nameof(title));
            _description = SafeText(description, "Review page setup, content, and branding before sending output to the printer.");
            _logoPath = logoPath ?? string.Empty;

            Title = $"Print Preview - {_title}";
            PreviewTitle.Text = _title;
            PreviewDescription.Text = _description;

            var logoUri = ResolveLogoUri(_logoPath);
            var bmp = new BitmapImage();
            bmp.BeginInit();
            bmp.CacheOption = BitmapCacheOption.OnLoad;
            bmp.UriSource = logoUri;
            bmp.EndInit();
            bmp.Freeze();
            PreviewLogo.Source = bmp;

            ApplyDocumentPolish(_document, _title);
            DocViewer.Document = _document;
            Owner = System.Windows.Application.Current.MainWindow;
            ShowDialog();
        }

        private static Uri ResolveLogoUri(string path)
        {
            if (!string.IsNullOrWhiteSpace(path))
            {
                try
                {
                    var full = Utilities.Helpers.PathHelper.GetAbsolutePath(path, true);
                    if (!string.IsNullOrEmpty(full) && File.Exists(full))
                        return new Uri(full, UriKind.Absolute);
                    System.Windows.MessageBox.Show("Logo path is invalid.", "Invalid Path");
                }
                catch (InvalidOperationException)
                {
                    System.Windows.MessageBox.Show("Logo path is invalid.", "Invalid Path");
                }
            }
            return new Uri("pack://application:,,,/Resources/DefaultLogo.png");
        }

        private static void ApplyDocumentPolish(FlowDocument document, string title)
        {
            document.FontFamily = new FontFamily("Segoe UI");
            document.FontSize = Math.Max(document.FontSize, 10.5);
            document.Foreground = new SolidColorBrush(Color.FromRgb(31, 41, 55));
            document.Background = Brushes.White;
            document.PagePadding = new Thickness(44, 40, 44, 42);
            document.ColumnGap = 0;
            document.TextAlignment = TextAlignment.Left;

            var firstBlock = document.Blocks.FirstBlock;
            if (firstBlock is not Section { Tag: "PrintPolishHeader" })
            {
                var header = BuildDocumentHeader(title);
                if (firstBlock == null)
                    document.Blocks.Add(header);
                else
                    document.Blocks.InsertBefore(firstBlock, header);
            }

            if (document.Blocks.LastBlock is not Paragraph { Tag: "PrintPolishFooter" })
                document.Blocks.Add(BuildDocumentFooter());

            foreach (var table in document.Blocks.OfType<Table>())
                ApplyTablePolish(table);
        }

        private static Section BuildDocumentHeader(string title)
        {
            var header = new Section
            {
                Tag = "PrintPolishHeader",
                Background = new SolidColorBrush(Color.FromRgb(244, 247, 251)),
                BorderBrush = new SolidColorBrush(Color.FromRgb(49, 130, 206)),
                BorderThickness = new Thickness(0, 0, 0, 2),
                Padding = new Thickness(12, 10, 12, 10),
                Margin = new Thickness(0, 0, 0, 14)
            };

            header.Blocks.Add(new Paragraph(new Run(SafeText(title, "Inventory Print Package")))
            {
                FontSize = 20,
                FontWeight = FontWeights.SemiBold,
                Foreground = new SolidColorBrush(Color.FromRgb(17, 24, 39)),
                Margin = new Thickness(0, 0, 0, 3)
            });
            header.Blocks.Add(new Paragraph(new Run($"Prepared {DateTime.Now:g} | Review, sign off, and file with the matching workflow."))
            {
                FontSize = 10.5,
                Foreground = new SolidColorBrush(Color.FromRgb(75, 85, 99)),
                Margin = new Thickness(0)
            });

            return header;
        }

        private static Paragraph BuildDocumentFooter()
        {
            return new Paragraph(new Run("Generated from InventoryManagementApp print preview | Confirm details before customer, audit, or shelf handoff."))
            {
                Tag = "PrintPolishFooter",
                FontSize = 9.5,
                Foreground = new SolidColorBrush(Color.FromRgb(75, 85, 99)),
                BorderBrush = new SolidColorBrush(Color.FromRgb(209, 213, 219)),
                BorderThickness = new Thickness(0, 1, 0, 0),
                Padding = new Thickness(0, 8, 0, 0),
                Margin = new Thickness(0, 14, 0, 0)
            };
        }

        private static void ApplyTablePolish(Table table)
        {
            table.CellSpacing = 0;
            table.Margin = new Thickness(0, 4, 0, 12);

            foreach (var rowGroup in table.RowGroups)
            {
                for (var rowIndex = 0; rowIndex < rowGroup.Rows.Count; rowIndex++)
                {
                    var row = rowGroup.Rows[rowIndex];
                    row.FontSize = rowIndex == 0 ? 10.5 : 10;

                    if (rowIndex == 0)
                    {
                        row.Background = new SolidColorBrush(Color.FromRgb(230, 236, 246));
                        row.Foreground = new SolidColorBrush(Color.FromRgb(17, 24, 39));
                        row.FontWeight = FontWeights.SemiBold;
                    }
                    else if (rowIndex % 2 == 0)
                    {
                        row.Background = new SolidColorBrush(Color.FromRgb(249, 250, 252));
                    }

                    foreach (var cell in row.Cells)
                    {
                        cell.BorderBrush = new SolidColorBrush(Color.FromRgb(209, 213, 219));
                        cell.BorderThickness = new Thickness(0, 0, 0, 0.6);
                        cell.Padding = new Thickness(6, 4, 6, 4);

                        foreach (var paragraph in cell.Blocks.OfType<Paragraph>())
                            paragraph.Margin = new Thickness(0);
                    }
                }
            }
        }

        private static string SafeText(string? text, string fallback)
            => string.IsNullOrWhiteSpace(text) ? fallback : text.Trim();

        private void OnPageSetup()
        {
            if (DocViewer.Document != null)
            {
                // FlowDocumentScrollViewer does not have ViewportWidth.
                // Use a reasonable default or set PageWidth to the window/client width.
                DocViewer.Document.PageWidth = DocViewer.ActualWidth;
            }
        }

        private void OnPrint()
        {
            if (_document == null) return;
            var dlg = new System.Windows.Controls.PrintDialog();
            if (dlg.ShowDialog() == true)
            {
                dlg.PrintDocument(((IDocumentPaginatorSource)_document).DocumentPaginator, _title);
            }
        }
    }
}
