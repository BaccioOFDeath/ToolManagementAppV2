using CommunityToolkit.Mvvm.Input;
using System;
using ToolManagementAppV2.Utilities.Helpers;

namespace ToolManagementAppV2.ViewModels
{
    public class SetupWizardViewModel : ChangePasswordViewModel
    {
        readonly Action<string> _onGenerated;
        bool _isRandom;
        public bool IsRandom
        {
            get => _isRandom;
            private set => SetProperty(ref _isRandom, value);
        }

        public IRelayCommand GenerateCommand { get; }

        public SetupWizardViewModel(Action onSave, Action onCancel, Action<string> onGenerated)
            : base(onSave, onCancel)
        {
            _onGenerated = onGenerated;
            GenerateCommand = new RelayCommand(() =>
            {
                var pwd = SecurityHelper.GeneratePassword(16);
                NewPassword = pwd;
                ConfirmPassword = pwd;
                IsRandom = true;
                _onGenerated(pwd);
            });
        }
    }
}
