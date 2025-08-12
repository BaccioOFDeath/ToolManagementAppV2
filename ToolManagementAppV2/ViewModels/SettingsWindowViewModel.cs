using System;
using System.Windows.Controls;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using ToolManagementAppV2.Views;

namespace ToolManagementAppV2.ViewModels
{
    public class SettingsWindowViewModel : ObservableObject
    {
        public SettingsViewModel SettingsViewModel { get; }
        public Page SettingsPageContent { get; }

        public IRelayCommand SaveSettingsCommand { get; }
        public IRelayCommand CloseCommand { get; }

        public SettingsWindowViewModel(SettingsViewModel settingsViewModel, Action closeAction, Action saveAction = null)
        {
            SettingsViewModel = settingsViewModel;
            SettingsPageContent = new SettingsPage { DataContext = SettingsViewModel };
            SaveSettingsCommand = new RelayCommand(() => saveAction?.Invoke());
            CloseCommand = new RelayCommand(() => closeAction?.Invoke());
        }
    }
}

