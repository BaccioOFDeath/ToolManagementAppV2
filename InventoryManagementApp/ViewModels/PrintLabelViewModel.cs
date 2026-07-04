using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.Linq;
using System.Windows;
using System.Windows.Documents;
using System.Windows.Media;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using InventoryManagementApp.Interfaces;
using InventoryManagementApp.Utilities.Helpers;
using InventoryManagementApp.Utilities.Printing;

#nullable enable

namespace InventoryManagementApp.ViewModels
{
    public class PrintLabelViewModel : ObservableObject
    {
        private const int MaxPrintableLabels = 250;

        private readonly System.Action _closeAction;
        private readonly IDialogService _dialogService;
        private readonly System.Action<FlowDocument> _printAction;

        public ObservableCollection<string> Templates { get; }

        private string _selectedTemplate = string.Empty;
        public string SelectedTemplate
        {
            get => _selectedTemplate;
            set
            {
                if (SetProperty(ref _selectedTemplate, value))
                    OnPropertyChanged(nameof(PrintReadinessText));
            }
        }

        private bool _includeQr;
        public bool IncludeQr
        {
            get => _includeQr;
            set
            {
                if (SetProperty(ref _includeQr, value))
                    OnPropertyChanged(nameof(PrintReadinessText));
            }
        }

        public ObservableCollection<ItemModel> Items { get; }

        public bool HasItems => Items.Count > 0;
        public Visibility EmptyQueueVisibility => HasItems ? Visibility.Collapsed : Visibility.Visible;
        public int VisibleLabelCount => Math.Min(Items.Count, MaxPrintableLabels);
        public int OmittedLabelCount => Math.Max(0, Items.Count - MaxPrintableLabels);

        public string QueueStatusText => HasItems
            ? $"{Items.Count:N0} queued; {VisibleLabelCount:N0} printable preview labels"
            : "No queued labels";

        public string PrintReadinessText
        {
            get
            {
                if (!HasItems)
                    return "Add at least one queued item before previewing or printing labels.";

                var qrText = IncludeQr ? "QR markers included" : "QR markers excluded";
                var omittedText = OmittedLabelCount > 0
                    ? $"; {OmittedLabelCount:N0} additional labels omitted from this preview for responsiveness"
                    : string.Empty;

                return $"Ready to preview {VisibleLabelCount:N0} {SelectedTemplate.ToLowerInvariant()} labels; {qrText}{omittedText}.";
            }
        }

        public IRelayCommand PreviewCommand { get; }
        public IRelayCommand PrintCommand { get; }
        public IRelayCommand CloseCommand { get; }

        public PrintLabelViewModel(IDialogService dialogService, System.Action closeAction, System.Action<FlowDocument>? printAction = null)
        {
            _dialogService = dialogService;
            _closeAction = closeAction;
            _printAction = printAction ?? (doc =>
            {
                var dlg = new System.Windows.Controls.PrintDialog();
                if (dlg.ShowDialog() == true)
                    dlg.PrintDocument(((IDocumentPaginatorSource)doc).DocumentPaginator, $"{LabelProvider.Instance.ItemLabelSingular} Labels");
            });
            Templates = new ObservableCollection<string> { "Standard", "Compact" };
            _selectedTemplate = Templates.First();
            Items = new ObservableCollection<ItemModel>();
            Items.CollectionChanged += Items_CollectionChanged;
            PreviewCommand = new RelayCommand(Preview, () => HasItems);
            PrintCommand = new RelayCommand(Print, () => HasItems);
            CloseCommand = new RelayCommand(() => _closeAction?.Invoke());
        }

        private void Items_CollectionChanged(object? sender, NotifyCollectionChangedEventArgs e)
        {
            OnPropertyChanged(nameof(HasItems));
            OnPropertyChanged(nameof(EmptyQueueVisibility));
            OnPropertyChanged(nameof(VisibleLabelCount));
            OnPropertyChanged(nameof(OmittedLabelCount));
            OnPropertyChanged(nameof(QueueStatusText));
            OnPropertyChanged(nameof(PrintReadinessText));
            PreviewCommand.NotifyCanExecuteChanged();
            PrintCommand.NotifyCanExecuteChanged();
        }

        private void Preview()
        {
            var doc = BuildDocument();
            _dialogService.ShowPrintPreview(doc, $"{LabelProvider.Instance.ItemLabelSingular} Labels", PrintReadinessText);
        }

        private void Print()
        {
            var doc = BuildDocument();
            PrintDocumentTheme.ApplyLightTheme(doc);
            try
            {
                _printAction(doc);
            }
            catch (System.Exception ex)
            {
                _dialogService.ShowInfo($"Failed to print labels: {ex.Message}", "Print Labels");
            }
        }

        private FlowDocument BuildDocument()
        {
            var printableLabels = Items
                .Take(MaxPrintableLabels)
                .Select(LabelSnapshot.FromItem)
                .ToList();

            var doc = new FlowDocument
            {
                PagePadding = new Thickness(36),
                ColumnWidth = double.PositiveInfinity,
                FontFamily = new FontFamily("Segoe UI"),
                FontSize = 11,
                Background = PrintDocumentTheme.PageBackgroundBrush,
                Foreground = PrintDocumentTheme.BodyForegroundBrush
            };

            AddDocumentHeader(doc, printableLabels.Count);

            if (printableLabels.Count == 0)
            {
                doc.Blocks.Add(CreateMutedParagraph("No label rows are queued. Add items from the label workflow before previewing or printing."));
                return doc;
            }

            doc.Blocks.Add(CreateLabelTable(printableLabels));

            if (OmittedLabelCount > 0)
            {
                doc.Blocks.Add(CreateMutedParagraph(
                    $"Large queue note: {OmittedLabelCount:N0} queued labels were omitted from this preview to keep rendering responsive. Print the current batch, then queue the remaining items separately."));
            }

            doc.Blocks.Add(CreateMutedParagraph("End of label sheet. Review item numbers, names, locations, and QR marker choices before applying labels."));
            return doc;
        }

        private void AddDocumentHeader(FlowDocument doc, int printableCount)
        {
            var itemLabel = LabelProvider.Instance.ItemLabelSingular;
            doc.Blocks.Add(new Paragraph(new Run($"{itemLabel} Label Sheet"))
            {
                FontSize = 20,
                FontWeight = FontWeights.SemiBold,
                Foreground = PrintDocumentTheme.HeaderForegroundBrush,
                Margin = new Thickness(0, 0, 0, 4)
            });

            doc.Blocks.Add(new Paragraph(new Run(
                $"Prepared {DateTime.Now:g} | Template: {SelectedTemplate} | QR: {(IncludeQr ? "Included" : "Excluded")} | Queued: {Items.Count:N0} | Printed in preview: {printableCount:N0} | Omitted: {OmittedLabelCount:N0}"))
            {
                Foreground = PrintDocumentTheme.MutedForegroundBrush,
                Margin = new Thickness(0, 0, 0, 14)
            });
        }

        private Table CreateLabelTable(IReadOnlyList<LabelSnapshot> labels)
        {
            var columnsPerRow = string.Equals(SelectedTemplate, "Compact", StringComparison.OrdinalIgnoreCase) ? 3 : 2;
            var table = new Table
            {
                CellSpacing = 8,
                Margin = new Thickness(0, 0, 0, 12)
            };

            for (var i = 0; i < columnsPerRow; i++)
                table.Columns.Add(new TableColumn { Width = new GridLength(1, GridUnitType.Star) });

            var group = new TableRowGroup();
            foreach (var rowLabels in labels.Chunk(columnsPerRow))
            {
                var row = new TableRow();
                foreach (var label in rowLabels)
                    row.Cells.Add(CreateLabelCell(label));

                while (row.Cells.Count < columnsPerRow)
                    row.Cells.Add(new TableCell(new Paragraph(new Run(string.Empty))) { BorderThickness = new Thickness(0) });

                group.Rows.Add(row);
            }

            table.RowGroups.Add(group);
            return table;
        }

        private TableCell CreateLabelCell(LabelSnapshot label)
        {
            var cell = new TableCell
            {
                Padding = new Thickness(10),
                BorderBrush = PrintDocumentTheme.RuleBorderBrush,
                BorderThickness = new Thickness(1)
            };

            cell.Blocks.Add(new Paragraph(new Run(label.ItemNumber))
            {
                FontSize = 14,
                FontWeight = FontWeights.SemiBold,
                Foreground = PrintDocumentTheme.HeaderForegroundBrush,
                Margin = new Thickness(0, 0, 0, 4)
            });
            cell.Blocks.Add(new Paragraph(new Run(label.Name))
            {
                Margin = new Thickness(0, 0, 0, 4)
            });
            cell.Blocks.Add(new Paragraph(new Run($"Location: {label.Location}"))
            {
                Foreground = PrintDocumentTheme.MutedForegroundBrush,
                Margin = new Thickness(0, 0, 0, IncludeQr ? 8 : 0)
            });

            if (IncludeQr)
            {
                cell.Blocks.Add(new Paragraph(new Run($"[QR] {label.ItemNumber}"))
                {
                    FontFamily = new FontFamily("Consolas"),
                    FontSize = 10,
                    Foreground = PrintDocumentTheme.MutedForegroundBrush,
                    Margin = new Thickness(0)
                });
            }

            return cell;
        }

        private static Paragraph CreateMutedParagraph(string text) => new(new Run(text))
        {
            Foreground = PrintDocumentTheme.MutedForegroundBrush,
            Margin = new Thickness(0, 6, 0, 0)
        };

        private readonly record struct LabelSnapshot(string ItemNumber, string Name, string Location)
        {
            public static LabelSnapshot FromItem(ItemModel item) => new(
                Normalize(item.ItemNumber, "Unnumbered item"),
                Normalize(item.Name, "Unnamed item"),
                Normalize(item.Location, "No location"));

            private static string Normalize(string? value, string fallback)
            {
                var text = value?.Trim();
                return string.IsNullOrWhiteSpace(text) ? fallback : text;
            }
        }
    }
}
