using System;
using System.IO;
using System.Linq;
using System.Windows;
using ToolManagementAppV2.Interfaces;
using ToolManagementAppV2.ViewModels;
using ToolManagementAppV2.Utilities.Extensions;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;


namespace ToolManagementAppV2.Views
{
    public partial class AvatarSelectionWindow : Window
    {
        public AvatarSelectionViewModel VM => (AvatarSelectionViewModel)DataContext;
        public string SelectedAvatarPath => VM.SelectedAvatarPath;

        private readonly ISettingsService _settingsService;
        private readonly ILogger<AvatarSelectionWindow> _logger;

        public AvatarSelectionWindow(ISettingsService settingsService, ILogger<AvatarSelectionWindow>? logger = null)
        {
            InitializeComponent();
            _settingsService = settingsService;
            _logger = logger ?? NullLogger<AvatarSelectionWindow>.Instance;

            var avatarDir = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Resources", "Avatars");
            var avatars = Array.Empty<Uri>();
            if (Directory.Exists(avatarDir))
                avatars = Directory
                    .EnumerateFiles(avatarDir, "*.png")
                    .Select(path => new Uri(path, UriKind.Absolute))
                    .ToArray();

            DataContext = new AvatarSelectionViewModel(avatars, () => DialogResult = true);
            this.DisposeDataContextOnUnload();
            Loaded += OnLoaded;
        }

        private async void OnLoaded(object sender, RoutedEventArgs e)
        {
            try
            {
                var appName = await _settingsService.GetSettingAsync("ApplicationName");
                if (!string.IsNullOrWhiteSpace(appName))
                    Title = $"{appName} – Select Avatar";
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to load ApplicationName setting");
            }
        }
    }
}
