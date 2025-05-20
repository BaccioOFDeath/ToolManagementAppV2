using System.IO;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Media.Imaging;

namespace ToolManagementAppV2.Views
{
    public partial class PrintPreviewWindow : Window
    {
        FlowDocument _document;
        string _title;
        string _logoPath;

        public PrintPreviewWindow()
        {
            InitializeComponent();
        }

        public void ShowPreview(FlowDocument document, string title, string logoPath)
        {
            _document = document ?? throw new ArgumentNullException(nameof(document));
            _title = title ?? throw new ArgumentNullException(nameof(title));
            _logoPath = logoPath;

            Title = $"Print Preview – {_title}";
            PreviewTitle.Text = _title;

            var logoUri = !string.IsNullOrWhiteSpace(_logoPath) && File.Exists(_logoPath)
                ? new Uri(_logoPath, UriKind.Absolute)
                : new Uri("pack://application:,,,/Resources/DefaultLogo.png");
            PreviewLogo.Source = new BitmapImage(logoUri);

            DocViewer.Document = _document;
            Owner = Application.Current.MainWindow;
            ShowDialog();
        }

        void Print_Click(object sender, RoutedEventArgs e)
        {
            if (_document == null) return;

            var dlg = new PrintDialog();
            if (dlg.ShowDialog() != true) return;

            dlg.PrintDocument(((IDocumentPaginatorSource)_document).DocumentPaginator, _title);
        }

        void Close_Click(object sender, RoutedEventArgs e)
        {
            Close();
        }
    }
}
