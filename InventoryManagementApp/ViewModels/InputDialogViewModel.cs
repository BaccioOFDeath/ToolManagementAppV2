using System;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;

namespace InventoryManagementApp.ViewModels
{
    public class InputDialogViewModel : ObservableObject
    {
        private string _inputText = string.Empty;

        public string Title { get; }

        public string Message { get; }

        public bool IsRequired { get; }

        public string InputText
        {
            get => _inputText;
            set => SetProperty(ref _inputText, value);
        }

        public IRelayCommand OkCommand { get; }

        public IRelayCommand CancelCommand { get; }

        public InputDialogViewModel(string title, string message, bool isRequired, Action onOk, Action onCancel)
        {
            Title = title;
            Message = message;
            IsRequired = isRequired;
            OkCommand = new RelayCommand(() =>
            {
                if (!IsRequired || !string.IsNullOrWhiteSpace(InputText))
                {
                    onOk();
                }
            });
            CancelCommand = new RelayCommand(onCancel);
        }
    }
}
