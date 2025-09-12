using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

namespace ToolManagementApp.Controls
{
    public partial class SearchBar : System.Windows.Controls.UserControl
    {
        public SearchBar()
        {
            InitializeComponent();
        }

        public string Text
        {
            get => (string)GetValue(TextProperty);
            set => SetValue(TextProperty, value);
        }

        public static readonly DependencyProperty TextProperty =
            DependencyProperty.Register(nameof(Text), typeof(string), typeof(SearchBar),
                new FrameworkPropertyMetadata(string.Empty, FrameworkPropertyMetadataOptions.BindsTwoWayByDefault));

        public ICommand? SearchCommand
        {
            get => (ICommand?)GetValue(SearchCommandProperty);
            set => SetValue(SearchCommandProperty, value);
        }

        public static readonly DependencyProperty SearchCommandProperty =
            DependencyProperty.Register(nameof(SearchCommand), typeof(ICommand), typeof(SearchBar));

        public ICommand? ClearCommand
        {
            get => (ICommand?)GetValue(ClearCommandProperty);
            set => SetValue(ClearCommandProperty, value);
        }

        public static readonly DependencyProperty ClearCommandProperty =
            DependencyProperty.Register(nameof(ClearCommand), typeof(ICommand), typeof(SearchBar));

        public string SearchLabel
        {
            get => (string)GetValue(SearchLabelProperty);
            set => SetValue(SearchLabelProperty, value);
        }

        public static readonly DependencyProperty SearchLabelProperty =
            DependencyProperty.Register(nameof(SearchLabel), typeof(string), typeof(SearchBar), new PropertyMetadata("Search"));
    }
}
