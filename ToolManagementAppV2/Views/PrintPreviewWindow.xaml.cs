using System;
using System.IO;
using System.Windows;
using System.Windows.Documents;
using System.Windows.Media.Imaging;
using System.Windows.Controls; // WPF PrintDialog

namespace ToolManagementAppV2.Views
{
    public partial class PrintPreviewWindow : Window
    {
        private FlowDocument _document;
        private string _title;
        private string _logoPath;

        public PrintPreviewWindow()
        {
            InitializeComponent();
        }

        public void ShowPreview(FlowDocument document, string title, string logoPath)
        {
            _document = document ?? throw new ArgumentNullException(nameof(document));
            _title = title ?? throw new ArgumentNullException(nameof(title));
            _logoPath = logoPath ?? "";

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
                var full = Utilities.Helpers.PathHelper.GetAbsolutePath(path);
                if (!string.IsNullOrEmpty(full) && File.Exists(full))
                    return new Uri(full, UriKind.Absolute);
            }
            return new Uri("pack://application:,,,/Resources/DefaultLogo.png");
        }

        private void PageSetup_Click(object sender, RoutedEventArgs e)
        {
            DocViewer.FitToWidth();
        }

        private void Print_Click(object sender, RoutedEventArgs e)
        {
            if (_document == null) return;
            var dlg = new System.Windows.Controls.PrintDialog();
            if (dlg.ShowDialog() == true) // ✅ Correct nullable bool check
            {
                dlg.PrintDocument(((IDocumentPaginatorSource)_document).DocumentPaginator, _title);
            }
        }

        private void Close_Click(object sender, RoutedEventArgs e)
        {
            Close();
        }
    }
}
