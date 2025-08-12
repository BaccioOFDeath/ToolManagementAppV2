using System;
using System.Windows.Controls;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using ToolManagementAppV2.Views;

namespace ToolManagementAppV2.ViewModels
{
    public class SettingsWindowViewModel : ObservableObject
    {
        public object SettingsViewModel { get; }
        public Page SettingsPageContent { get; }

        public IRelayCommand SaveSettingsCommand { get; }
        public IRelayCommand CloseCommand { get; }

        public SettingsWindowViewModel(object settingsViewModel, Action closeAction, Action saveAction = null)
        {
            if (settingsViewModel == null) throw new ArgumentNullException(nameof(settingsViewModel));
            if (closeAction == null) throw new ArgumentNullException(nameof(closeAction));

            SettingsViewModel = settingsViewModel;
            SettingsPageContent = new SettingsPage { DataContext = SettingsViewModel };
            SaveSettingsCommand = new RelayCommand(() => saveAction?.Invoke());
            CloseCommand = new RelayCommand(() => closeAction());
        }
    }
}
