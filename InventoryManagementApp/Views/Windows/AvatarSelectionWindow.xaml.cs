using System;
using System.IO;
using System.Linq;
using System.Windows;
using System.Threading.Tasks;
using InventoryManagementApp.Interfaces;
using InventoryManagementApp.ViewModels;
using InventoryManagementApp.Utilities.Extensions;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;
using InventoryManagementApp.Utilities.Helpers;


namespace InventoryManagementApp.Views.Windows
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

            DataContext = new AvatarSelectionViewModel(Array.Empty<Uri>(), () => DialogResult = true);
            this.DisposeDataContextOnUnload();
            Loaded += OnLoaded;
        }

        private async void OnLoaded(object sender, RoutedEventArgs e)
        {
            try
            {
                var avatarDir = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Resources", "Avatars");
                if (Directory.Exists(avatarDir))
                {
                    var avatars = await Task.Run(() =>
                        Directory.EnumerateFiles(avatarDir, "*.png")
                            .Select(path => new Uri(path, UriKind.Absolute))
                            .ToArray());

                    VM.Avatars.Clear();
                    foreach (var uri in avatars)
                        VM.Avatars.Add(uri);
                }

                var appName = await _settingsService.GetSettingAsync("ApplicationName");
                Title = !string.IsNullOrWhiteSpace(appName)
                    ? $"{appName} – Select Avatar"
                    : $"{LabelProvider.Instance.ItemLabelSingular} Inventory Management – Select Avatar";
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to load avatars or ApplicationName setting");
            }
        }
    }
}
