using System;
using System.IO;
using System.Windows;
using System.Windows.Media.Imaging;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using InventoryManagementApp.Models;
using QRCoder;

namespace InventoryManagementApp.Views.Windows
{
    public partial class MobileCaptureWindow : Window
    {
        public MobileCaptureWindow(MobileCaptureSession session)
        {
            InitializeComponent();
            DataContext = new MobileCaptureWindowViewModel(session, () => Close());
        }
    }

    public partial class MobileCaptureWindowViewModel : ObservableObject
    {
        private readonly Action _close;

        public MobileCaptureWindowViewModel(MobileCaptureSession session, Action close)
        {
            ArgumentNullException.ThrowIfNull(session);
            _close = close;
            Url = session.Url;
            ExpiresDisplay = $"Session expires at {session.ExpiresAt:yyyy-MM-dd HH:mm}.";
            Status = "Ready to scan";
            QrImage = CreateQrImage(session.Url);
            CopyUrlCommand = new RelayCommand(CopyUrl);
            CloseCommand = new RelayCommand(_close);
        }

        public string Url { get; }
        public string Status { get; }
        public string ExpiresDisplay { get; }
        public BitmapImage QrImage { get; }
        public IRelayCommand CopyUrlCommand { get; }
        public IRelayCommand CloseCommand { get; }

        private void CopyUrl()
        {
            Clipboard.SetText(Url);
        }

        private static BitmapImage CreateQrImage(string url)
        {
            using var generator = new QRCodeGenerator();
            using var data = generator.CreateQrCode(url, QRCodeGenerator.ECCLevel.Q);
            var png = new PngByteQRCode(data);
            var bytes = png.GetGraphic(12);

            var image = new BitmapImage();
            image.BeginInit();
            image.CacheOption = BitmapCacheOption.OnLoad;
            image.StreamSource = new MemoryStream(bytes);
            image.EndInit();
            image.Freeze();
            return image;
        }
    }
}
