using InventoryManagementApp.Interfaces;
using InventoryManagementApp.ViewModels;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using System.Windows.Controls;

namespace InventoryManagementApp.Views.Pages
{
    public partial class ThemeDesignerControl : UserControl
    {
        private bool _initialized;

        public ThemeDesignerControl()
        {
            InitializeComponent();
            Loaded += ThemeDesignerControl_Loaded;
        }

        private async void ThemeDesignerControl_Loaded(object sender, System.Windows.RoutedEventArgs e)
        {
            if (_initialized || DataContext is ThemeDesignerViewModel)
                return;

            if (System.Windows.Application.Current is not App app)
                return;

            var services = app.Host.Services;
            var viewModel = new ThemeDesignerViewModel(
                services.GetRequiredService<ISettingsService>(),
                services.GetRequiredService<IThemeService>(),
                services.GetRequiredService<IFileDialogService>(),
                services.GetRequiredService<IDialogService>(),
                services.GetService<ILogger<ThemeDesignerViewModel>>());

            DataContext = viewModel;
            await viewModel.InitializeAsync();
            _initialized = true;
        }
    }
}
