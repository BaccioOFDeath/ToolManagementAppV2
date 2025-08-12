using System.Collections.ObjectModel;
using System.Linq;
using System.Windows.Documents;
using System.Windows.Controls;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using ToolManagementAppV2.Models.Domain;
using ToolManagementAppV2.Views;

namespace ToolManagementAppV2.ViewModels
{
    public class PrintLabelViewModel : ObservableObject
    {
        private readonly System.Action _closeAction;

        public ObservableCollection<string> Templates { get; }

        private string _selectedTemplate;
        public string SelectedTemplate
        {
            get => _selectedTemplate;
            set => SetProperty(ref _selectedTemplate, value);
        }

        private bool _includeQr;
        public bool IncludeQr
        {
            get => _includeQr;
            set => SetProperty(ref _includeQr, value);
        }

        public ObservableCollection<Tool> Items { get; }

        public IRelayCommand PreviewCommand { get; }
        public IRelayCommand PrintCommand { get; }
        public IRelayCommand CloseCommand { get; }

        public PrintLabelViewModel(System.Action closeAction)
        {
            _closeAction = closeAction;
            Templates = new ObservableCollection<string> { "Standard", "Compact" };
            _selectedTemplate = Templates.First();
            Items = new ObservableCollection<Tool>();
            PreviewCommand = new RelayCommand(Preview);
            PrintCommand = new RelayCommand(Print);
            CloseCommand = new RelayCommand(() => _closeAction?.Invoke());
        }

        private void Preview()
        {
            var doc = BuildDocument();
            var preview = new PrintPreviewWindow();
            preview.ShowPreview(doc, "Tool Labels", string.Empty);
        }

        private void Print()
        {
            var doc = BuildDocument();
            var dlg = new System.Windows.Controls.PrintDialog();
            if (dlg.ShowDialog() == true)
                dlg.PrintDocument(((IDocumentPaginatorSource)doc).DocumentPaginator, "Tool Labels");
        }

        private FlowDocument BuildDocument()
        {
            var doc = new FlowDocument();
            foreach (var t in Items)
            {
                var sp = new StackPanel { Orientation = System.Windows.Controls.Orientation.Vertical };
                sp.Children.Add(new TextBlock { Text = t.ToolNumber });
                sp.Children.Add(new TextBlock { Text = t.NameDescription });
                sp.Children.Add(new TextBlock { Text = t.Location });
                if (IncludeQr)
                    sp.Children.Add(new TextBlock { Text = "[QR]" });
                doc.Blocks.Add(new BlockUIContainer(sp));
            }
            return doc;
        }
    }
}
