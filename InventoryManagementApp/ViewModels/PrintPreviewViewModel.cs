using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using System;

namespace InventoryManagementApp.ViewModels
{
    public class PrintPreviewViewModel : ObservableObject
    {
        private bool _hasDocument;
        private bool _isPrinting;
        private string _previewStatus = "Preparing print preview...";
        private string _footerStatus = "Preview preparing";

        public IRelayCommand PageSetupCommand { get; }
        public IRelayCommand PrintCommand { get; }
        public IRelayCommand CloseCommand { get; }

        public bool HasDocument
        {
            get => _hasDocument;
            private set
            {
                if (SetProperty(ref _hasDocument, value))
                    RefreshCommandState();
            }
        }

        public bool IsPrinting
        {
            get => _isPrinting;
            private set
            {
                if (SetProperty(ref _isPrinting, value))
                    RefreshCommandState();
            }
        }

        public bool CanPreviewActions => HasDocument && !IsPrinting;

        public string PreviewStatus
        {
            get => _previewStatus;
            private set => SetProperty(ref _previewStatus, value);
        }

        public string FooterStatus
        {
            get => _footerStatus;
            private set => SetProperty(ref _footerStatus, value);
        }

        public PrintPreviewViewModel(Action onPageSetup, Action onPrint, Action onClose)
        {
            PageSetupCommand = new RelayCommand(onPageSetup, () => CanPreviewActions);
            PrintCommand = new RelayCommand(onPrint, () => CanPreviewActions);
            CloseCommand = new RelayCommand(onClose, () => !IsPrinting);
        }

        public void SetPreviewReady(string? description)
        {
            HasDocument = true;
            PreviewStatus = string.IsNullOrWhiteSpace(description)
                ? "Preview ready. Review page setup before printing."
                : description.Trim();
            FooterStatus = "Preview ready for final print review";
            RefreshCommandState();
        }

        public void SetPageSetupAdjusted()
        {
            PreviewStatus = "Page setup applied. Review the document canvas before printing.";
            FooterStatus = "Page setup refreshed";
        }

        public bool TryBeginPrint()
        {
            if (!CanPreviewActions)
                return false;

            IsPrinting = true;
            PreviewStatus = "Print dialog open. Finish or cancel printing before changing the preview.";
            FooterStatus = "Printing in progress";
            return true;
        }

        public void EndPrint(bool printed)
        {
            IsPrinting = false;
            PreviewStatus = printed
                ? "Print job sent. Review the originating workflow for any follow-up filing."
                : "Print canceled. Preview remains ready for page setup or printing.";
            FooterStatus = printed ? "Print job sent" : "Preview ready for final print review";
        }

        private void RefreshCommandState()
        {
            OnPropertyChanged(nameof(CanPreviewActions));
            PageSetupCommand.NotifyCanExecuteChanged();
            PrintCommand.NotifyCanExecuteChanged();
            CloseCommand.NotifyCanExecuteChanged();
        }
    }
}
