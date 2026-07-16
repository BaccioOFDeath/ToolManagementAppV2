using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Windows;
using System.Windows.Documents;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Controls; // WPF PrintDialog
using System.Windows.Input;
using System.Windows.Threading;
using InventoryManagementApp.ViewModels;
using InventoryManagementApp.Interfaces;
using InventoryManagementApp.Utilities.Extensions;
using InventoryManagementApp.Utilities.Printing;
using Microsoft.Extensions.DependencyInjection;

#nullable enable

namespace InventoryManagementApp.Views.Windows
{
    public partial class PrintPreviewWindow : Window
    {
        internal const double DefaultPreviewPageWidth = 816;
        internal const double DefaultPreviewPageHeight = 1056;
        private const double MinimumPrintableExtent = 320;
        private const string CompanyLogoSettingKey = "CompanyLogoPath";
        private static readonly Thickness PrintPagePadding = new(36, 36, 36, 36);
        private static readonly Uri DefaultLogoUri = new("pack://application:,,,/Resources/DefaultLogo.png");

        private FlowDocument? _document;
        private string _title = string.Empty;
        private string _description = string.Empty;
        private string _logoPath = string.Empty;

        public PrintPreviewWindow()
        {
            InitializeComponent();
            DataContext = new PrintPreviewViewModel(OnPageSetup, OnPrint, Close);
            PreviewKeyDown += PrintPreviewWindow_PreviewKeyDown;
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
            SetPreviewLogo(DefaultLogoUri);

            ApplyDocumentPolish(_document, _title);
            ConfigureDocumentForPage(_document, DefaultPreviewPageWidth, DefaultPreviewPageHeight);
            ApplyTablePolish(_document);
            DocViewer.Document = _document;

            if (DataContext is PrintPreviewViewModel vm)
                vm.SetPreviewReady(_description);

            Dispatcher.BeginInvoke(new Action(() => LoadLogoForPreview(_logoPath)), DispatcherPriority.Background);
            Dispatcher.BeginInvoke(new Action(() => DocViewer.Focus()), DispatcherPriority.ContextIdle);

            Owner = System.Windows.Application.Current.MainWindow;
            ShowDialog();
        }

        internal static void PrepareDocumentForPrint(FlowDocument document, string title, double pageWidth = DefaultPreviewPageWidth, double pageHeight = DefaultPreviewPageHeight)
        {
            if (document is null)
                throw new ArgumentNullException(nameof(document));

            ApplyDocumentPolish(document, title);
            ConfigureDocumentForPage(document, pageWidth, pageHeight);
            ApplyTablePolish(document);
        }

        private async void LoadLogoForPreview(string path)
        {
            try
            {
                var resolvedPath = path;
                if (string.IsNullOrWhiteSpace(resolvedPath))
                {
                    var settingsService = (System.Windows.Application.Current as App)?.Host.Services.GetService<ISettingsService>();
                    if (settingsService != null)
                        resolvedPath = await settingsService.GetSettingAsync(CompanyLogoSettingKey).ConfigureAwait(true) ?? string.Empty;
                }

                if (!TryResolveCustomLogoUri(resolvedPath, out var logoUri))
                    return;

                SetPreviewLogo(logoUri);
            }
            catch (Exception ex) when (ex is IOException or InvalidOperationException or Microsoft.Data.Sqlite.SqliteException)
            {
                SetPreviewLogo(DefaultLogoUri);
            }
        }

        private static bool TryResolveCustomLogoUri(string path, out Uri logoUri)
        {
            logoUri = DefaultLogoUri;
            if (string.IsNullOrWhiteSpace(path))
                return false;

            try
            {
                var full = Utilities.Helpers.PathHelper.GetAbsolutePath(path, true);
                if (!string.IsNullOrEmpty(full) && File.Exists(full))
                {
                    logoUri = new Uri(full, UriKind.Absolute);
                    return true;
                }
            }
            catch (InvalidOperationException)
            {
                return false;
            }
            catch (UriFormatException)
            {
                return false;
            }

            return false;
        }

        private void SetPreviewLogo(Uri logoUri)
        {
            try
            {
                var bmp = new BitmapImage();
                bmp.BeginInit();
                bmp.CacheOption = BitmapCacheOption.OnLoad;
                bmp.UriSource = logoUri;
                bmp.EndInit();
                bmp.Freeze();
                PreviewLogo.Source = bmp;
            }
            catch (IOException)
            {
                if (!Equals(logoUri, DefaultLogoUri))
                    SetPreviewLogo(DefaultLogoUri);
            }
            catch (InvalidOperationException)
            {
                if (!Equals(logoUri, DefaultLogoUri))
                    SetPreviewLogo(DefaultLogoUri);
            }
            catch (NotSupportedException)
            {
                if (!Equals(logoUri, DefaultLogoUri))
                    SetPreviewLogo(DefaultLogoUri);
            }
        }

        private static void ApplyDocumentPolish(FlowDocument document, string title)
        {
            document.FontFamily = new FontFamily("Segoe UI");
            document.FontSize = Math.Max(document.FontSize, 10.5);
            PrintDocumentTheme.ApplyLightTheme(document);
            document.PagePadding = PrintPagePadding;
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

        }

        private static void ApplyTablePolish(FlowDocument document)
        {
            var contentWidth = Math.Max(120, document.ColumnWidth);
            foreach (var table in GetTables(document.Blocks))
                ApplyTablePolish(table, contentWidth);
        }

        private static void ConfigureDocumentForPage(FlowDocument document, double pageWidth, double pageHeight)
        {
            var safePageWidth = Math.Max(MinimumPrintableExtent, pageWidth);
            var safePageHeight = Math.Max(MinimumPrintableExtent, pageHeight);
            var contentWidth = Math.Max(120, safePageWidth - document.PagePadding.Left - document.PagePadding.Right);

            document.PageWidth = safePageWidth;
            document.PageHeight = safePageHeight;
            document.ColumnGap = 0;
            document.ColumnWidth = contentWidth;
        }

        private static IEnumerable<Table> GetTables(BlockCollection blocks)
        {
            foreach (var block in blocks)
            {
                if (block is Table table)
                {
                    yield return table;
                }
                else if (block is Section section)
                {
                    foreach (var nestedTable in GetTables(section.Blocks))
                        yield return nestedTable;
                }
            }
        }

        private static Section BuildDocumentHeader(string title)
        {
            var header = new Section
            {
                Tag = "PrintPolishHeader",
                Background = PrintDocumentTheme.HeaderPanelBackgroundBrush,
                BorderBrush = PrintDocumentTheme.AccentBorderBrush,
                BorderThickness = new Thickness(0, 0, 0, 2),
                Padding = new Thickness(12, 10, 12, 10),
                Margin = new Thickness(0, 0, 0, 14)
            };

            header.Blocks.Add(new Paragraph(new Run(SafeText(title, "Inventory Print Package")))
            {
                FontSize = 20,
                FontWeight = FontWeights.SemiBold,
                Foreground = PrintDocumentTheme.HeaderForegroundBrush,
                Margin = new Thickness(0, 0, 0, 3)
            });
            header.Blocks.Add(new Paragraph(new Run($"Prepared {DateTime.Now:g} | Review, sign off, and file with the matching workflow."))
            {
                FontSize = 10.5,
                Foreground = PrintDocumentTheme.MutedForegroundBrush,
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
                Foreground = PrintDocumentTheme.MutedForegroundBrush,
                BorderBrush = PrintDocumentTheme.RuleBorderBrush,
                BorderThickness = new Thickness(0, 1, 0, 0),
                Padding = new Thickness(0, 8, 0, 0),
                Margin = new Thickness(0, 14, 0, 0)
            };
        }

        private static void ApplyTablePolish(Table table, double contentWidth)
        {
            RebalanceTableColumns(table, contentWidth);
            table.CellSpacing = 0;
            table.Margin = new Thickness(0, 4, 0, 12);
            table.TextAlignment = TextAlignment.Left;
            var isKeyValueTable = string.Equals(table.Tag as string, "KeyValue", StringComparison.Ordinal);

            foreach (var rowGroup in table.RowGroups)
            {
                for (var rowIndex = 0; rowIndex < rowGroup.Rows.Count; rowIndex++)
                {
                    var row = rowGroup.Rows[rowIndex];
                    row.FontSize = !isKeyValueTable && rowIndex == 0 ? 10.5 : 10;

                    if (!isKeyValueTable && rowIndex == 0)
                    {
                        row.Background = PrintDocumentTheme.HeaderBackgroundBrush;
                        row.Foreground = PrintDocumentTheme.HeaderForegroundBrush;
                        row.FontWeight = FontWeights.SemiBold;
                    }
                    else if (!isKeyValueTable && rowIndex % 2 == 0)
                    {
                        row.Background = PrintDocumentTheme.AlternatingRowBackgroundBrush;
                    }

                    foreach (var cell in row.Cells)
                    {
                        cell.BorderBrush = PrintDocumentTheme.RuleBorderBrush;
                        cell.BorderThickness = new Thickness(0, 0, 0, 0.6);
                        cell.Padding = new Thickness(6, 4, 6, 4);
                        cell.TextAlignment = TextAlignment.Left;

                        foreach (var paragraph in cell.Blocks.OfType<Paragraph>())
                            paragraph.Margin = new Thickness(0);
                    }
                }
            }
        }

        private static void RebalanceTableColumns(Table table, double contentWidth)
        {
            if (table.Columns.Count == 0)
                return;

            var safeContentWidth = Math.Max(120, contentWidth);
            if (table.Columns.Count == 2)
            {
                table.Columns[0].Width = new GridLength(safeContentWidth * 0.32);
                table.Columns[1].Width = new GridLength(safeContentWidth * 0.68);
                return;
            }

            var weights = table.Columns
                .Select(column => column.Width.IsAbsolute ? Math.Max(1, column.Width.Value) : 80)
                .ToArray();
            var totalWeight = Math.Max(1, weights.Sum());

            for (var index = 0; index < table.Columns.Count; index++)
                table.Columns[index].Width = new GridLength(safeContentWidth * weights[index] / totalWeight);
        }

        private static string SafeText(string? text, string fallback)
            => string.IsNullOrWhiteSpace(text) ? fallback : text.Trim();

        private void OnPageSetup()
        {
            if (DocViewer.Document == null || DataContext is not PrintPreviewViewModel vm || !vm.CanPreviewActions)
                return;

            var pageWidth = SafePreviewExtent(DocViewer.ActualWidth, DefaultPreviewPageWidth);
            var pageHeight = SafePreviewExtent(DocViewer.ActualHeight, DefaultPreviewPageHeight);
            ConfigureDocumentForPage(DocViewer.Document, pageWidth, pageHeight);
            ApplyTablePolish(DocViewer.Document);
            vm.SetPageSetupAdjusted();
        }

        private static double SafePreviewExtent(double actualExtent, double fallbackExtent)
            => double.IsFinite(actualExtent) && actualExtent >= MinimumPrintableExtent
                ? actualExtent
                : fallbackExtent;

        private void OnPrint()
        {
            if (_document == null || DataContext is not PrintPreviewViewModel vm || !vm.TryBeginPrint())
                return;

            var printed = false;
            try
            {
                var dlg = new System.Windows.Controls.PrintDialog();
                if (dlg.ShowDialog() == true)
                {
                    ConfigureDocumentForPage(_document, dlg.PrintableAreaWidth, dlg.PrintableAreaHeight);
                    ApplyTablePolish(_document);

                    var paginator = ((IDocumentPaginatorSource)_document).DocumentPaginator;
                    paginator.PageSize = new Size(dlg.PrintableAreaWidth, dlg.PrintableAreaHeight);
                    dlg.PrintDocument(paginator, _title);
                    printed = true;
                }
            }
            catch (InvalidOperationException ex)
            {
                System.Windows.MessageBox.Show($"Print preview could not send this document to the printer. {ex.Message}", "Print Preview");
            }
            catch (System.Printing.PrintSystemException ex)
            {
                System.Windows.MessageBox.Show($"Windows could not complete the print request. {ex.Message}", "Print Preview");
            }
            finally
            {
                vm.EndPrint(printed);
            }
        }

        private void PrintPreviewWindow_PreviewKeyDown(object sender, KeyEventArgs e)
        {
            if (DataContext is not PrintPreviewViewModel vm)
                return;

            if (Keyboard.Modifiers == ModifierKeys.Control && e.Key == Key.P)
            {
                if (vm.PrintCommand.CanExecute(null))
                    vm.PrintCommand.Execute(null);
                e.Handled = true;
                return;
            }

            if (Keyboard.Modifiers == (ModifierKeys.Control | ModifierKeys.Shift) && e.Key == Key.P)
            {
                if (vm.PageSetupCommand.CanExecute(null))
                    vm.PageSetupCommand.Execute(null);
                e.Handled = true;
                return;
            }

            if (Keyboard.Modifiers == ModifierKeys.None && e.Key == Key.Escape && !vm.IsPrinting)
            {
                vm.CloseCommand.Execute(null);
                e.Handled = true;
            }
        }
    }
}
