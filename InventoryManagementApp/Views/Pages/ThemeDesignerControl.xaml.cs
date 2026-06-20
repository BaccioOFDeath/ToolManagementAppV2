using InventoryManagementApp.Interfaces;
using InventoryManagementApp.ViewModels;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;

namespace InventoryManagementApp.Views.Pages
{
    public partial class ThemeDesignerControl : UserControl
    {
        private bool _initialized;

        public ThemeDesignerControl()
        {
            InitializeComponent();
            AddDesignReadinessPanel();
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

        private void AddDesignReadinessPanel()
        {
            if (Content is not Border { Child: DockPanel dockPanel })
                return;

            var panel = new Border
            {
                Margin = new Thickness(14, 0, 14, 14)
            };
            panel.SetResourceReference(StyleProperty, "DesktopInsetCard");

            var stack = new StackPanel();

            var heading = new TextBlock
            {
                Text = "Design readiness",
                TextWrapping = TextWrapping.Wrap,
                Margin = new Thickness(0, 0, 0, 4)
            };
            heading.SetResourceReference(StyleProperty, "PageTitleTextBlock");
            stack.Children.Add(heading);

            var checklist = new TextBlock
            {
                Text = "Before saving a full-app redesign, review text contrast, transparent surface readability, focus-ring visibility, disabled-control clarity, table density, borderless affordances, and shadow depth in the preview lab.",
                TextWrapping = TextWrapping.Wrap,
                Margin = new Thickness(0, 0, 0, 8)
            };
            checklist.SetResourceReference(StyleProperty, "CaptionTextBlock");
            stack.Children.Add(checklist);

            var status = new TextBlock
            {
                TextWrapping = TextWrapping.Wrap
            };
            status.SetResourceReference(StyleProperty, "LabelTextBlock");
            status.SetBinding(TextBlock.TextProperty, new Binding(nameof(ThemeDesignerViewModel.Status))
            {
                FallbackValue = "Theme designer ready."
            });
            stack.Children.Add(status);

            panel.Child = stack;
            DockPanel.SetDock(panel, Dock.Bottom);
            dockPanel.Children.Add(panel);
        }
    }
}
