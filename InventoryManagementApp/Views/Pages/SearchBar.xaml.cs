using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

namespace InventoryManagementApp.Views.Pages
{
    public partial class SearchBar : System.Windows.Controls.UserControl
    {
        bool _skipNextLostKeyboardFocusSearch;

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

        public bool SearchOnLostKeyboardFocus
        {
            get => (bool)GetValue(SearchOnLostKeyboardFocusProperty);
            set => SetValue(SearchOnLostKeyboardFocusProperty, value);
        }

        public static readonly DependencyProperty SearchOnLostKeyboardFocusProperty =
            DependencyProperty.Register(nameof(SearchOnLostKeyboardFocus), typeof(bool), typeof(SearchBar), new PropertyMetadata(false));

        public string SearchLabel
        {
            get => (string)GetValue(SearchLabelProperty);
            set => SetValue(SearchLabelProperty, value);
        }

        public static readonly DependencyProperty SearchLabelProperty =
            DependencyProperty.Register(nameof(SearchLabel), typeof(string), typeof(SearchBar), new PropertyMetadata("Search"));

        void SearchTextBox_PreviewKeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Tab)
            {
                ExecuteLostFocusSearch();
                _skipNextLostKeyboardFocusSearch = true;
            }
        }

        void SearchTextBox_LostKeyboardFocus(object sender, KeyboardFocusChangedEventArgs e)
        {
            if (_skipNextLostKeyboardFocusSearch)
            {
                _skipNextLostKeyboardFocusSearch = false;
                return;
            }

            if (e.NewFocus is DependencyObject newFocus && IsAncestorOf(newFocus))
                return;

            ExecuteLostFocusSearch();
        }

        void ExecuteLostFocusSearch()
        {
            if (!SearchOnLostKeyboardFocus || SearchCommand == null)
                return;

            if (SearchCommand.CanExecute(null))
                SearchCommand.Execute(null);
        }
    }
}
