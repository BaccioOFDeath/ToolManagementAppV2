using System.Windows;
using System.Windows.Controls;

namespace InventoryManagementApp.Controls
{
    public partial class UserAvatar : UserControl
    {
        public UserAvatar()
        {
            InitializeComponent();
        }

        public string? UserName
        {
            get => (string?)GetValue(UserNameProperty);
            set => SetValue(UserNameProperty, value);
        }

        public static readonly DependencyProperty UserNameProperty =
            DependencyProperty.Register(nameof(UserName), typeof(string), typeof(UserAvatar));

        public string? UserPhotoPath
        {
            get => (string?)GetValue(UserPhotoPathProperty);
            set => SetValue(UserPhotoPathProperty, value);
        }

        public static readonly DependencyProperty UserPhotoPathProperty =
            DependencyProperty.Register(nameof(UserPhotoPath), typeof(string), typeof(UserAvatar));
    }
}
