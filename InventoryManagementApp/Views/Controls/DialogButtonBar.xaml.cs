// InventoryManagementApp/Views/Controls/DialogButtonBar.xaml.cs
using System.Windows;
using System.Windows.Input;

namespace InventoryManagementApp.Controls
{
    public partial class DialogButtonBar : System.Windows.Controls.UserControl
    {
        public DialogButtonBar()
        {
            InitializeComponent();
        }

        public string LeftButtonText
        {
            get => (string)GetValue(LeftButtonTextProperty);
            set => SetValue(LeftButtonTextProperty, value);
        }
        public static readonly DependencyProperty LeftButtonTextProperty =
            DependencyProperty.Register(nameof(LeftButtonText), typeof(string), typeof(DialogButtonBar), new PropertyMetadata("Cancel"));

        public string RightButtonText
        {
            get => (string)GetValue(RightButtonTextProperty);
            set => SetValue(RightButtonTextProperty, value);
        }
        public static readonly DependencyProperty RightButtonTextProperty =
            DependencyProperty.Register(nameof(RightButtonText), typeof(string), typeof(DialogButtonBar), new PropertyMetadata("OK"));

        public ICommand? LeftCommand
        {
            get => (ICommand?)GetValue(LeftCommandProperty);
            set => SetValue(LeftCommandProperty, value);
        }
        public static readonly DependencyProperty LeftCommandProperty =
            DependencyProperty.Register(nameof(LeftCommand), typeof(ICommand), typeof(DialogButtonBar));

        public ICommand? RightCommand
        {
            get => (ICommand?)GetValue(RightCommandProperty);
            set => SetValue(RightCommandProperty, value);
        }
        public static readonly DependencyProperty RightCommandProperty =
            DependencyProperty.Register(nameof(RightCommand), typeof(ICommand), typeof(DialogButtonBar));
    }
}
