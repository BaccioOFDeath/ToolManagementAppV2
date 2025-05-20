using System;
using System.IO;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Media.Imaging;

namespace ToolManagementAppV2.Views
{
    public partial class PrintPreviewWindow : Window
    {
        private FlowDocument _doc;   // store the FlowDocument
        private string _title;
        private string _logoPath;

        public PrintPreviewWindow()
        {
            InitializeComponent();
        }

        /// <summary>
        /// Show a FlowDocument for preview.
        /// </summary>
        public void ShowPreview(FlowDocument document, string title, string logoPath)
        {
            _doc = document;
            _title = title;
            _logoPath = logoPath;

            // window & header
            this.Title = $"Print Preview – {title}";
            PreviewTitle.Text = title;

            // logo
            if (!string.IsNullOrEmpty(logoPath) && File.Exists(logoPath))
                PreviewLogo.Source = new BitmapImage(new Uri(logoPath, UriKind.Absolute));
            else
                PreviewLogo.Source = new BitmapImage(new Uri("pack://application:,,,/Resources/DefaultLogo.png"));

            // feed the FlowDocument into the reader
            DocViewer.Document = _doc;

            this.Owner = Application.Current.MainWindow;
            this.ShowDialog();
        }

        private void Print_Click(object sender, RoutedEventArgs e)
        {
            if (_doc == null) return;

            var dlg = new PrintDialog();
            if (dlg.ShowDialog() == true)
                dlg.PrintDocument(((IDocumentPaginatorSource)_doc).DocumentPaginator, _title);
        }

        private void Close_Click(object sender, RoutedEventArgs e)
        {
            this.Close();
        }
    }
}
