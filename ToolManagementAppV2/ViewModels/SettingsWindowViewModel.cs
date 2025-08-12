using System;
using System.Windows.Controls;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using ToolManagementAppV2.Views;

namespace ToolManagementAppV2.ViewModels
{
    public class SettingsWindowViewModel : ObservableObject
    {
        public Page SettingsPageContent { get; }

        public IRelayCommand SaveSettingsCommand { get; }
        public IRelayCommand CloseCommand { get; }

        public SettingsWindowViewModel(Action closeAction, Action saveAction = null)
        {
            SettingsPageContent = new SettingsPage { DataContext = new SettingsViewModel() };
            SaveSettingsCommand = new RelayCommand(() => saveAction?.Invoke());
            CloseCommand = new RelayCommand(() => closeAction?.Invoke());
        }
    }
}

