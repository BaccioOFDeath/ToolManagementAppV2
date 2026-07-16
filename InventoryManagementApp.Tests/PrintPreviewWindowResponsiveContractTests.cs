using System;
using System.IO;
using Xunit;

namespace InventoryManagementApp.Tests
{
    public class PrintPreviewWindowResponsiveContractTests
    {
        [Fact]
        public void PrintPreviewWindow_UsesCompactResponsiveBoundsAndStatusSurfaces()
        {
            var xaml = ReadRepoFile("InventoryManagementApp", "Views", "Windows", "PrintPreviewWindow.xaml");

            Assert.Contains("Width=\"1040\"", xaml, StringComparison.Ordinal);
            Assert.Contains("Height=\"720\"", xaml, StringComparison.Ordinal);
            Assert.Contains("MinWidth=\"720\"", xaml, StringComparison.Ordinal);
            Assert.Contains("MinHeight=\"520\"", xaml, StringComparison.Ordinal);
            Assert.Contains("x:Name=\"PreviewRoot\" Style=\"{StaticResource ThemedWindowRoot}\" MinWidth=\"0\" ClipToBounds=\"True\"", xaml, StringComparison.Ordinal);
            Assert.Contains("x:Name=\"PreviewStatus\"", xaml, StringComparison.Ordinal);
            Assert.Contains("Text=\"{Binding PreviewStatus}\"", xaml, StringComparison.Ordinal);
            Assert.Contains("x:Name=\"PreviewFooterStatus\"", xaml, StringComparison.Ordinal);
            Assert.Contains("Text=\"{Binding FooterStatus}\"", xaml, StringComparison.Ordinal);
            Assert.DoesNotContain("Width=\"1120\"", xaml, StringComparison.Ordinal);
            Assert.DoesNotContain("MinWidth=\"760\"", xaml, StringComparison.Ordinal);
        }

        [Fact]
        public void PrintPreviewWindow_UsesTheFullWidthForTheScrollableDocumentCanvas()
        {
            var xaml = ReadRepoFile("InventoryManagementApp", "Views", "Windows", "PrintPreviewWindow.xaml");

            Assert.DoesNotContain("<GridSplitter", xaml, StringComparison.Ordinal);
            Assert.DoesNotContain("<ColumnDefinition Width=\"0.32*\" MinWidth=\"220\"/>", xaml, StringComparison.Ordinal);
            Assert.Contains("Style=\"{StaticResource ThemedDocumentCanvasFrame}\" MinWidth=\"0\" ClipToBounds=\"True\"", xaml, StringComparison.Ordinal);
            Assert.Contains("VerticalScrollBarVisibility=\"Auto\"", xaml, StringComparison.Ordinal);
            Assert.Contains("HorizontalScrollBarVisibility=\"Auto\"", xaml, StringComparison.Ordinal);
            Assert.Contains("Focusable=\"True\"", xaml, StringComparison.Ordinal);
            Assert.DoesNotContain("<ColumnDefinition Width=\"6\"/>", xaml, StringComparison.Ordinal);
            Assert.DoesNotContain("<ColumnDefinition Width=\"0.36*\" MinWidth=\"240\"/>", xaml, StringComparison.Ordinal);
        }

        [Fact]
        public void PrintPreviewWindow_WrapsActionsAndDocumentsKeyboardReviewShortcuts()
        {
            var xaml = ReadRepoFile("InventoryManagementApp", "Views", "Windows", "PrintPreviewWindow.xaml");

            Assert.Contains("<WrapPanel Grid.Column=\"1\" HorizontalAlignment=\"Right\" VerticalAlignment=\"Center\" MaxWidth=\"300\">", xaml, StringComparison.Ordinal);
            Assert.Contains("Command=\"{Binding PageSetupCommand}\"", xaml, StringComparison.Ordinal);
            Assert.Contains("Command=\"{Binding PrintCommand}\"", xaml, StringComparison.Ordinal);
            Assert.Contains("Command=\"{Binding CloseCommand}\"", xaml, StringComparison.Ordinal);
            Assert.DoesNotContain("Ctrl+P prints", xaml, StringComparison.Ordinal);
            Assert.Contains("MinWidth=\"78\"", xaml, StringComparison.Ordinal);
            Assert.DoesNotContain("MaxWidth=\"330\"", xaml, StringComparison.Ordinal);
        }

        [Fact]
        public void PrintPreviewViewModel_GatesPreviewActionsDuringPrint()
        {
            var viewModel = ReadRepoFile("InventoryManagementApp", "ViewModels", "PrintPreviewViewModel.cs");

            Assert.Contains("private bool _hasDocument;", viewModel, StringComparison.Ordinal);
            Assert.Contains("private bool _isPrinting;", viewModel, StringComparison.Ordinal);
            Assert.Contains("public bool CanPreviewActions => HasDocument && !IsPrinting;", viewModel, StringComparison.Ordinal);
            Assert.Contains("PageSetupCommand = new RelayCommand(onPageSetup, () => CanPreviewActions);", viewModel, StringComparison.Ordinal);
            Assert.Contains("PrintCommand = new RelayCommand(onPrint, () => CanPreviewActions);", viewModel, StringComparison.Ordinal);
            Assert.Contains("CloseCommand = new RelayCommand(onClose, () => !IsPrinting);", viewModel, StringComparison.Ordinal);
            Assert.Contains("public bool TryBeginPrint()", viewModel, StringComparison.Ordinal);
            Assert.Contains("public void EndPrint(bool printed)", viewModel, StringComparison.Ordinal);
            Assert.Contains("PageSetupCommand.NotifyCanExecuteChanged();", viewModel, StringComparison.Ordinal);
            Assert.Contains("PrintCommand.NotifyCanExecuteChanged();", viewModel, StringComparison.Ordinal);
            Assert.Contains("CloseCommand.NotifyCanExecuteChanged();", viewModel, StringComparison.Ordinal);
        }

        [Fact]
        public void PrintPreviewWindow_CodeBehindDefersLogoWorkAndAvoidsBlockingInvalidPathDialogs()
        {
            var codeBehind = ReadRepoFile("InventoryManagementApp", "Views", "Windows", "PrintPreviewWindow.xaml.cs");

            Assert.Contains("private static readonly Uri DefaultLogoUri", codeBehind, StringComparison.Ordinal);
            Assert.Contains("SetPreviewLogo(DefaultLogoUri);", codeBehind, StringComparison.Ordinal);
            Assert.Contains("Dispatcher.BeginInvoke(new Action(() => LoadLogoForPreview(_logoPath)), DispatcherPriority.Background);", codeBehind, StringComparison.Ordinal);
            Assert.Contains("GetSettingAsync(CompanyLogoSettingKey)", codeBehind, StringComparison.Ordinal);
            Assert.Contains("GetService<ISettingsService>()", codeBehind, StringComparison.Ordinal);
            Assert.Contains("private static bool TryResolveCustomLogoUri", codeBehind, StringComparison.Ordinal);
            Assert.Contains("return false;", codeBehind, StringComparison.Ordinal);
            Assert.Contains("if (!Equals(logoUri, DefaultLogoUri))\n                    SetPreviewLogo(DefaultLogoUri);", codeBehind, StringComparison.Ordinal);
            Assert.DoesNotContain("MessageBox.Show(\"Logo path is invalid.", codeBehind, StringComparison.Ordinal);
            Assert.DoesNotContain("private static Uri ResolveLogoUri", codeBehind, StringComparison.Ordinal);
        }

        [Fact]
        public void PrintPreviewWindow_CodeBehindUsesSafePageSetupAndPrintBusyGuards()
        {
            var codeBehind = ReadRepoFile("InventoryManagementApp", "Views", "Windows", "PrintPreviewWindow.xaml.cs");

            Assert.Contains("if (DocViewer.Document == null || DataContext is not PrintPreviewViewModel vm || !vm.CanPreviewActions)", codeBehind, StringComparison.Ordinal);
            Assert.Contains("SafePreviewExtent(DocViewer.ActualWidth, DefaultPreviewPageWidth)", codeBehind, StringComparison.Ordinal);
            Assert.Contains("SafePreviewExtent(DocViewer.ActualHeight, DefaultPreviewPageHeight)", codeBehind, StringComparison.Ordinal);
            Assert.Contains("private static double SafePreviewExtent(double actualExtent, double fallbackExtent)", codeBehind, StringComparison.Ordinal);
            Assert.Contains("double.IsFinite(actualExtent) && actualExtent >= MinimumPrintableExtent", codeBehind, StringComparison.Ordinal);
            Assert.Contains("!vm.TryBeginPrint()", codeBehind, StringComparison.Ordinal);
            Assert.Contains("vm.EndPrint(printed);", codeBehind, StringComparison.Ordinal);
            Assert.Contains("catch (System.Printing.PrintSystemException ex)", codeBehind, StringComparison.Ordinal);
        }

        [Fact]
        public void PrintPreviewWindow_CodeBehindAddsKeyboardShortcutsAndFastCanvasFocus()
        {
            var codeBehind = ReadRepoFile("InventoryManagementApp", "Views", "Windows", "PrintPreviewWindow.xaml.cs");

            Assert.Contains("PreviewKeyDown += PrintPreviewWindow_PreviewKeyDown;", codeBehind, StringComparison.Ordinal);
            Assert.Contains("Dispatcher.BeginInvoke(new Action(() => DocViewer.Focus()), DispatcherPriority.ContextIdle);", codeBehind, StringComparison.Ordinal);
            Assert.Contains("Keyboard.Modifiers == ModifierKeys.Control && e.Key == Key.P", codeBehind, StringComparison.Ordinal);
            Assert.Contains("Keyboard.Modifiers == (ModifierKeys.Control | ModifierKeys.Shift) && e.Key == Key.P", codeBehind, StringComparison.Ordinal);
            Assert.Contains("Keyboard.Modifiers == ModifierKeys.None && e.Key == Key.Escape && !vm.IsPrinting", codeBehind, StringComparison.Ordinal);
            Assert.Contains("vm.PrintCommand.CanExecute(null)", codeBehind, StringComparison.Ordinal);
            Assert.Contains("vm.PageSetupCommand.CanExecute(null)", codeBehind, StringComparison.Ordinal);
            Assert.Contains("vm.CloseCommand.Execute(null);", codeBehind, StringComparison.Ordinal);
            Assert.Contains("e.Handled = true;", codeBehind, StringComparison.Ordinal);
        }

        private static string ReadRepoFile(params string[] parts)
        {
            var directory = AppContext.BaseDirectory;

            while (!string.IsNullOrEmpty(directory))
            {
                var candidate = Path.Combine(directory, Path.Combine(parts));
                if (File.Exists(candidate))
                    return NormalizeLineEndings(File.ReadAllText(candidate));

                var parent = Directory.GetParent(directory);
                if (parent is null)
                    break;

                directory = parent.FullName;
            }

            throw new FileNotFoundException($"Could not find repository file: {Path.Combine(parts)}");
        }
        static string NormalizeLineEndings(string text)
            => text.Replace("\r\n", "\n");

    }
}
