using System;
using System.IO;
using System.Windows;
using System.Windows.Documents;
using System.Windows.Media.Imaging;
using System.Windows.Controls; // WPF PrintDialog
using InventoryManagementApp.ViewModels;
using InventoryManagementApp.Utilities.Extensions;

namespace InventoryManagementApp.Views.Windows
{
    public partial class PrintPreviewWindow : Window
    {
        private FlowDocument _document;
        private string _title;
        private string _logoPath = string.Empty;

        public PrintPreviewWindow()
        {
            InitializeComponent();
            DataContext = new PrintPreviewViewModel(OnPageSetup, OnPrint, Close);
            this.DisposeDataContextOnUnload();
        }

        public void ShowPreview(FlowDocument document, string title, string? logoPath)
        {
            _document = document ?? throw new ArgumentNullException(nameof(document));
            _title = title ?? throw new ArgumentNullException(nameof(title));
            _logoPath = logoPath ?? string.Empty;

            Title = $"Print Preview – {_title}";
            PreviewTitle.Text = _title;

            var logoUri = ResolveLogoUri(_logoPath);
            var bmp = new BitmapImage();
            bmp.BeginInit();
            bmp.CacheOption = BitmapCacheOption.OnLoad;
            bmp.UriSource = logoUri;
            bmp.EndInit();
            bmp.Freeze();
            PreviewLogo.Source = bmp;

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

        private void OnPageSetup()
        {
            DocViewer.FitToWidth();
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
