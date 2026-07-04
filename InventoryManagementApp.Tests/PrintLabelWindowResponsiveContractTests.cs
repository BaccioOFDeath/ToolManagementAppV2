using System;
using System.IO;
using Xunit;

namespace InventoryManagementApp.Tests
{
    public class PrintLabelWindowResponsiveContractTests
    {
        [Fact]
        public void PrintLabelWindow_UsesCompactResponsiveSizingAndRootBounds()
        {
            var xaml = ReadRepoFile("InventoryManagementApp", "Views", "Windows", "PrintLabelWindow.xaml");
            var codeBehind = ReadRepoFile("InventoryManagementApp", "Views", "Windows", "PrintLabelWindow.xaml.cs");

            Assert.Contains("Width=\"760\" Height=\"520\"", xaml, StringComparison.Ordinal);
            Assert.Contains("MinWidth=\"560\" MinHeight=\"420\"", xaml, StringComparison.Ordinal);
            Assert.Contains("x:Name=\"PrintLabelRoot\" Margin=\"10\" MinWidth=\"0\" ClipToBounds=\"True\"", xaml, StringComparison.Ordinal);
            Assert.Contains("this.UseResponsiveDefaultSize(760, 520);", codeBehind, StringComparison.Ordinal);
            Assert.DoesNotContain("this.UseResponsiveDefaultSize(820, 540);", codeBehind, StringComparison.Ordinal);
            Assert.DoesNotContain("Width=\"820\" Height=\"540\"", xaml, StringComparison.Ordinal);
            Assert.DoesNotContain("MinWidth=\"700\" MinHeight=\"480\"", xaml, StringComparison.Ordinal);
        }

        [Fact]
        public void PrintLabelWindow_BoundsHeaderQueueAndStatusText()
        {
            var xaml = NormalizeNewlines(ReadRepoFile("InventoryManagementApp", "Views", "Windows", "PrintLabelWindow.xaml"));

            Assert.Contains("<WrapPanel DockPanel.Dock=\"Right\"", xaml, StringComparison.Ordinal);
            Assert.Contains("MaxWidth=\"260\"", xaml, StringComparison.Ordinal);
            Assert.Contains("MinWidth=\"142\"", xaml, StringComparison.Ordinal);
            Assert.Contains("MaxWidth=\"240\"", xaml, StringComparison.Ordinal);
            Assert.Contains("<StackPanel MinWidth=\"0\" MaxWidth=\"520\">", xaml, StringComparison.Ordinal);
            Assert.Contains("Text=\"{Binding LabelActionStatusText}\"", xaml, StringComparison.Ordinal);
            Assert.DoesNotContain("<Grid.ColumnDefinitions>\n                    <ColumnDefinition Width=\"*\" MinWidth=\"0\"/>\n                    <ColumnDefinition Width=\"Auto\"/>\n                </Grid.ColumnDefinitions>", xaml, StringComparison.Ordinal);
            Assert.DoesNotContain("MaxWidth=\"190\"", xaml, StringComparison.Ordinal);
        }

        [Fact]
        public void PrintLabelWindow_KeepsTemplateControlsAndFooterWrappingAtScaledWidths()
        {
            var xaml = ReadRepoFile("InventoryManagementApp", "Views", "Windows", "PrintLabelWindow.xaml");

            Assert.Contains("MinWidth=\"160\"", xaml, StringComparison.Ordinal);
            Assert.Contains("MaxWidth=\"260\"", xaml, StringComparison.Ordinal);
            Assert.Contains("<WrapPanel HorizontalAlignment=\"Right\">", xaml, StringComparison.Ordinal);
            Assert.Contains("Content=\"Preview\" Command=\"{Binding PreviewCommand}\" MinWidth=\"96\"", xaml, StringComparison.Ordinal);
            Assert.Contains("Content=\"Print\" Command=\"{Binding PrintCommand}\" MinWidth=\"96\"", xaml, StringComparison.Ordinal);
            Assert.Contains("Content=\"Close\" Command=\"{Binding CloseCommand}\" MinWidth=\"96\"", xaml, StringComparison.Ordinal);
            Assert.Contains("Text=\"{Binding LabelActionStatusText}\" Style=\"{StaticResource CaptionTextBlock}\" TextWrapping=\"Wrap\" MaxWidth=\"560\"", xaml, StringComparison.Ordinal);
            Assert.DoesNotContain("MinWidth=\"104\"", xaml, StringComparison.Ordinal);
        }

        [Fact]
        public void PrintLabelWindow_EnablesVirtualizedScrollableQueueGridWithLowerColumnPressure()
        {
            var xaml = ReadRepoFile("InventoryManagementApp", "Views", "Windows", "PrintLabelWindow.xaml");

            Assert.Contains("x:Name=\"LabelQueueGrid\"", xaml, StringComparison.Ordinal);
            Assert.Contains("EnableRowVirtualization=\"True\"", xaml, StringComparison.Ordinal);
            Assert.Contains("EnableColumnVirtualization=\"True\"", xaml, StringComparison.Ordinal);
            Assert.Contains("SelectionMode=\"Single\"", xaml, StringComparison.Ordinal);
            Assert.Contains("SelectionUnit=\"FullRow\"", xaml, StringComparison.Ordinal);
            Assert.Contains("ScrollViewer.CanContentScroll=\"True\"", xaml, StringComparison.Ordinal);
            Assert.Contains("ScrollViewer.HorizontalScrollBarVisibility=\"Auto\"", xaml, StringComparison.Ordinal);
            Assert.Contains("ScrollViewer.VerticalScrollBarVisibility=\"Auto\"", xaml, StringComparison.Ordinal);
            Assert.Contains("Width=\"104\" MinWidth=\"82\"", xaml, StringComparison.Ordinal);
            Assert.Contains("Width=\"2*\" MinWidth=\"140\"", xaml, StringComparison.Ordinal);
            Assert.Contains("Width=\"1.1*\" MinWidth=\"112\"", xaml, StringComparison.Ordinal);
            Assert.DoesNotContain("Width=\"120\" MinWidth=\"96\"", xaml, StringComparison.Ordinal);
            Assert.DoesNotContain("Width=\"2*\" MinWidth=\"180\"", xaml, StringComparison.Ordinal);
            Assert.DoesNotContain("Width=\"1.2*\" MinWidth=\"140\"", xaml, StringComparison.Ordinal);
        }

        [Fact]
        public void PrintLabelWindow_BoundsEmptyStateAndShowsBusyOverlay()
        {
            var xaml = ReadRepoFile("InventoryManagementApp", "Views", "Windows", "PrintLabelWindow.xaml");

            Assert.Contains("Visibility=\"{Binding EmptyQueueVisibility}\"", xaml, StringComparison.Ordinal);
            Assert.Contains("MaxWidth=\"340\"", xaml, StringComparison.Ordinal);
            Assert.Contains("MinHeight=\"112\"", xaml, StringComparison.Ordinal);
            Assert.Contains("Visibility=\"{Binding LabelActionBusyVisibility}\"", xaml, StringComparison.Ordinal);
            Assert.Contains("IsHitTestVisible=\"True\"", xaml, StringComparison.Ordinal);
            Assert.Contains("Preparing label document", xaml, StringComparison.Ordinal);
            Assert.DoesNotContain("MaxWidth=\"380\"", xaml, StringComparison.Ordinal);
            Assert.DoesNotContain("The label sheet will stay capped for responsive previews", xaml, StringComparison.Ordinal);
        }

        [Fact]
        public void PrintLabelViewModel_ExposesGenerationStateAndPausesCommandsWhilePreparing()
        {
            var source = ReadRepoFile("InventoryManagementApp", "ViewModels", "PrintLabelViewModel.cs");

            Assert.Contains("public bool IsGeneratingDocument", source, StringComparison.Ordinal);
            Assert.Contains("public bool CanGenerateLabels => HasItems && !IsGeneratingDocument;", source, StringComparison.Ordinal);
            Assert.Contains("public Visibility LabelActionBusyVisibility => IsGeneratingDocument ? Visibility.Visible : Visibility.Collapsed;", source, StringComparison.Ordinal);
            Assert.Contains("public string LabelActionStatusText => IsGeneratingDocument", source, StringComparison.Ordinal);
            Assert.Contains("PreviewCommand = new RelayCommand(Preview, () => CanGenerateLabels);", source, StringComparison.Ordinal);
            Assert.Contains("PrintCommand = new RelayCommand(Print, () => CanGenerateLabels);", source, StringComparison.Ordinal);
            Assert.Contains("PreviewCommand.NotifyCanExecuteChanged();", source, StringComparison.Ordinal);
            Assert.Contains("PrintCommand.NotifyCanExecuteChanged();", source, StringComparison.Ordinal);
            Assert.DoesNotContain("PreviewCommand = new RelayCommand(Preview, () => HasItems);", source, StringComparison.Ordinal);
            Assert.DoesNotContain("PrintCommand = new RelayCommand(Print, () => HasItems);", source, StringComparison.Ordinal);
        }

        [Fact]
        public void PrintLabelViewModel_GuardsPreviewAndPrintWithFinallyReset()
        {
            var source = NormalizeNewlines(ReadRepoFile("InventoryManagementApp", "ViewModels", "PrintLabelViewModel.cs"));

            Assert.Contains("if (!CanGenerateLabels)\n                return;", source, StringComparison.Ordinal);
            Assert.Contains("IsGeneratingDocument = true;", source, StringComparison.Ordinal);
            Assert.Contains("finally\n            {\n                IsGeneratingDocument = false;\n            }", source, StringComparison.Ordinal);
            Assert.Contains("_dialogService.ShowPrintPreview(doc, $\"{LabelProvider.Instance.ItemLabelSingular} Labels\", PrintReadinessText);", source, StringComparison.Ordinal);
            Assert.Contains("_printAction(doc);", source, StringComparison.Ordinal);
            Assert.Contains("Failed to print labels", source, StringComparison.Ordinal);
        }

        [Fact]
        public void PrintLabelViewModel_StatusTextReflectsResponsivenessCapForPreviewAndPrint()
        {
            var source = ReadRepoFile("InventoryManagementApp", "ViewModels", "PrintLabelViewModel.cs");

            Assert.Contains("private const int MaxPrintableLabels = 250;", source, StringComparison.Ordinal);
            Assert.Contains("public int VisibleLabelCount => Math.Min(Items.Count, MaxPrintableLabels);", source, StringComparison.Ordinal);
            Assert.Contains("public int OmittedLabelCount => Math.Max(0, Items.Count - MaxPrintableLabels);", source, StringComparison.Ordinal);
            Assert.Contains("prepared for preview or print", source, StringComparison.Ordinal);
            Assert.Contains("additional labels omitted from this run for responsiveness", source, StringComparison.Ordinal);
            Assert.Contains("Ready to prepare", source, StringComparison.Ordinal);
            Assert.DoesNotContain("printable preview labels", source, StringComparison.Ordinal);
            Assert.DoesNotContain("omitted from this preview for responsiveness", source, StringComparison.Ordinal);
        }

        private static string NormalizeNewlines(string value) => value.Replace("\r\n", "\n", StringComparison.Ordinal);

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