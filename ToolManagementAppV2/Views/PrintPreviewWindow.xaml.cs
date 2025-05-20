using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;

namespace ToolManagementAppV2.Views
{
    public partial class PrintPreviewWindow : Window
    {
        private FlowDocument _doc;
        private string _title;

        public PrintPreviewWindow()
        {
            InitializeComponent();
        }

        /// <summary>
        /// Call this with your FlowDocument and title
        /// </summary>
        public void ShowPreview(FlowDocument doc, string title)
        {
            _doc = doc;
            _title = title;

            DocViewer.Document = doc;
            this.Owner = Application.Current.MainWindow;
            this.ShowDialog();
        }

        private void Print_Click(object sender, RoutedEventArgs e)
        {
            if (_doc == null) return;

            var dlg = new PrintDialog();
            if (dlg.ShowDialog() == true)
            {
                // cast to IDocumentPaginatorSource to get the paginator
                var paginator = ((IDocumentPaginatorSource)_doc).DocumentPaginator;
                dlg.PrintDocument(paginator, _title);
                this.Close();
            }
        }

        private void Cancel_Click(object sender, RoutedEventArgs e)
        {
            this.Close();
        }
    }
}