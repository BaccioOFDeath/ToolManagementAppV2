using System;
using System.IO;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using InventoryManagementApp.Utilities.Extensions;

namespace InventoryManagementApp.Views.Windows
{
    public partial class ImageBackgroundRemovalWindow : Window
    {
        const double MinimumClipSize = 24;

        readonly string _sourcePath;
        readonly string _outputPath;
        readonly BitmapSource _source;
        Rect _clipSourceRect;
        double _scale = 1;
        double _imageLeft;
        double _imageTop;
        BitmapSource? _preview;

        public string? SavedImagePath { get; private set; }

        public ImageBackgroundRemovalWindow(string sourcePath, string outputPath)
        {
            InitializeComponent();
            this.UseResponsiveDefaultSize(1160, 760);

            _sourcePath = sourcePath;
            _outputPath = outputPath;
            _source = LoadBgra32(sourcePath);
            SourceImage.Source = _source;
            ResetClipToDefault();
            Loaded += (_, _) =>
            {
                LayoutSourceImage();
                RenderPreview();
            };
        }

        static BitmapSource LoadBgra32(string sourcePath)
        {
            var bitmap = new BitmapImage();
            bitmap.BeginInit();
            bitmap.CacheOption = BitmapCacheOption.OnLoad;
            bitmap.UriSource = new Uri(sourcePath, UriKind.Absolute);
            bitmap.EndInit();
            bitmap.Freeze();

            BitmapSource source = bitmap.Format == PixelFormats.Bgra32
                ? bitmap
                : new FormatConvertedBitmap(bitmap, PixelFormats.Bgra32, null, 0);
            source.Freeze();
            return source;
        }

        void ResetClipToDefault()
        {
            var insetX = Math.Max(0, _source.PixelWidth * 0.06);
            var insetY = Math.Max(0, _source.PixelHeight * 0.06);
            _clipSourceRect = new Rect(insetX, insetY, Math.Max(MinimumClipSize, _source.PixelWidth - insetX * 2), Math.Max(MinimumClipSize, _source.PixelHeight - insetY * 2));
        }

        void ClipCanvas_SizeChanged(object sender, SizeChangedEventArgs e)
        {
            LayoutSourceImage();
        }

        void LayoutSourceImage()
        {
            if (ClipCanvas.ActualWidth <= 0 || ClipCanvas.ActualHeight <= 0)
                return;

            _scale = Math.Min(ClipCanvas.ActualWidth / _source.PixelWidth, ClipCanvas.ActualHeight / _source.PixelHeight);
            if (double.IsInfinity(_scale) || double.IsNaN(_scale) || _scale <= 0)
                _scale = 1;

            var displayedWidth = _source.PixelWidth * _scale;
            var displayedHeight = _source.PixelHeight * _scale;
            _imageLeft = (ClipCanvas.ActualWidth - displayedWidth) / 2;
            _imageTop = (ClipCanvas.ActualHeight - displayedHeight) / 2;

            SourceImage.Width = displayedWidth;
            SourceImage.Height = displayedHeight;
            Canvas.SetLeft(SourceImage, _imageLeft);
            Canvas.SetTop(SourceImage, _imageTop);

            DimOverlay.Width = ClipCanvas.ActualWidth;
            DimOverlay.Height = ClipCanvas.ActualHeight;
            Canvas.SetLeft(DimOverlay, 0);
            Canvas.SetTop(DimOverlay, 0);

            UpdateClipVisuals();
        }

        void UpdateClipVisuals()
        {
            var display = SourceToDisplay(_clipSourceRect);

            ClipBorder.Width = display.Width;
            ClipBorder.Height = display.Height;
            Canvas.SetLeft(ClipBorder, display.Left);
            Canvas.SetTop(ClipBorder, display.Top);

            MoveThumb.Width = display.Width;
            MoveThumb.Height = display.Height;
            Canvas.SetLeft(MoveThumb, display.Left);
            Canvas.SetTop(MoveThumb, display.Top);

            PlaceThumb(TopLeftThumb, display.Left, display.Top);
            PlaceThumb(TopRightThumb, display.Right, display.Top);
            PlaceThumb(BottomLeftThumb, display.Left, display.Bottom);
            PlaceThumb(BottomRightThumb, display.Right, display.Bottom);
        }

        static void PlaceThumb(FrameworkElement thumb, double x, double y)
        {
            Canvas.SetLeft(thumb, x - thumb.Width / 2);
            Canvas.SetTop(thumb, y - thumb.Height / 2);
        }

        Rect SourceToDisplay(Rect sourceRect)
            => new(
                _imageLeft + sourceRect.X * _scale,
                _imageTop + sourceRect.Y * _scale,
                sourceRect.Width * _scale,
                sourceRect.Height * _scale);

        void MoveThumb_DragDelta(object sender, System.Windows.Controls.Primitives.DragDeltaEventArgs e)
        {
            MoveClip(e.HorizontalChange / _scale, e.VerticalChange / _scale);
        }

        void TopLeftThumb_DragDelta(object sender, System.Windows.Controls.Primitives.DragDeltaEventArgs e)
        {
            ResizeClip(e.HorizontalChange / _scale, e.VerticalChange / _scale, 0, 0);
        }

        void TopRightThumb_DragDelta(object sender, System.Windows.Controls.Primitives.DragDeltaEventArgs e)
        {
            ResizeClip(0, e.VerticalChange / _scale, e.HorizontalChange / _scale, 0);
        }

        void BottomLeftThumb_DragDelta(object sender, System.Windows.Controls.Primitives.DragDeltaEventArgs e)
        {
            ResizeClip(e.HorizontalChange / _scale, 0, 0, e.VerticalChange / _scale);
        }

        void BottomRightThumb_DragDelta(object sender, System.Windows.Controls.Primitives.DragDeltaEventArgs e)
        {
            ResizeClip(0, 0, e.HorizontalChange / _scale, e.VerticalChange / _scale);
        }

        void MoveClip(double deltaX, double deltaY)
        {
            var x = Math.Clamp(_clipSourceRect.X + deltaX, 0, _source.PixelWidth - _clipSourceRect.Width);
            var y = Math.Clamp(_clipSourceRect.Y + deltaY, 0, _source.PixelHeight - _clipSourceRect.Height);
            _clipSourceRect = new Rect(x, y, _clipSourceRect.Width, _clipSourceRect.Height);
            UpdateClipVisuals();
            RenderPreview();
        }

        void ResizeClip(double leftDelta, double topDelta, double rightDelta, double bottomDelta)
        {
            var left = _clipSourceRect.Left + leftDelta;
            var top = _clipSourceRect.Top + topDelta;
            var right = _clipSourceRect.Right + rightDelta;
            var bottom = _clipSourceRect.Bottom + bottomDelta;

            left = Math.Clamp(left, 0, _clipSourceRect.Right - MinimumClipSize);
            top = Math.Clamp(top, 0, _clipSourceRect.Bottom - MinimumClipSize);
            right = Math.Clamp(right, _clipSourceRect.Left + MinimumClipSize, _source.PixelWidth);
            bottom = Math.Clamp(bottom, _clipSourceRect.Top + MinimumClipSize, _source.PixelHeight);

            _clipSourceRect = new Rect(left, top, right - left, bottom - top);
            UpdateClipVisuals();
            RenderPreview();
        }

        void ThresholdSlider_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            if (IsLoaded)
                RenderPreview();
        }

        void ResetClip_Click(object sender, RoutedEventArgs e)
        {
            ResetClipToDefault();
            UpdateClipVisuals();
            RenderPreview();
        }

        void Save_Click(object sender, RoutedEventArgs e)
        {
            _preview ??= CreateBackgroundRemovedBitmap(_source, _clipSourceRect, ThresholdSlider.Value);
            Directory.CreateDirectory(Path.GetDirectoryName(_outputPath)!);
            var encoder = new PngBitmapEncoder();
            encoder.Frames.Add(BitmapFrame.Create(_preview));
            using var output = new FileStream(_outputPath, FileMode.Create, FileAccess.Write, FileShare.None);
            encoder.Save(output);
            SavedImagePath = _outputPath;
            DialogResult = true;
        }

        void Cancel_Click(object sender, RoutedEventArgs e)
        {
            DialogResult = false;
        }

        void RenderPreview()
        {
            _preview = CreateBackgroundRemovedBitmap(_source, _clipSourceRect, ThresholdSlider.Value);
            PreviewImage.Source = _preview;
        }

        public static BitmapSource CreateBackgroundRemovedBitmap(BitmapSource source, Rect clipRect, double threshold)
        {
            BitmapSource bgra = source.Format == PixelFormats.Bgra32
                ? source
                : new FormatConvertedBitmap(source, PixelFormats.Bgra32, null, 0);

            var width = Math.Max(1, Math.Min(bgra.PixelWidth, (int)Math.Round(clipRect.Width)));
            var height = Math.Max(1, Math.Min(bgra.PixelHeight, (int)Math.Round(clipRect.Height)));
            var sourceX = Math.Clamp((int)Math.Round(clipRect.X), 0, Math.Max(0, bgra.PixelWidth - width));
            var sourceY = Math.Clamp((int)Math.Round(clipRect.Y), 0, Math.Max(0, bgra.PixelHeight - height));
            var crop = new CroppedBitmap(bgra, new Int32Rect(sourceX, sourceY, width, height));
            var stride = width * 4;
            var pixels = new byte[height * stride];
            crop.CopyPixels(pixels, stride, 0);

            var background = EstimateBackgroundColor(pixels, width, height, stride);
            for (var offset = 0; offset < pixels.Length; offset += 4)
            {
                var blue = pixels[offset];
                var green = pixels[offset + 1];
                var red = pixels[offset + 2];
                if (IsBackgroundPixel(red, green, blue, background, threshold))
                    pixels[offset + 3] = 0;
            }

            var transparent = BitmapSource.Create(width, height, bgra.DpiX, bgra.DpiY, PixelFormats.Bgra32, null, pixels, stride);
            transparent.Freeze();
            return transparent;
        }

        static Color EstimateBackgroundColor(byte[] pixels, int width, int height, int stride)
        {
            var samplePoints = new[]
            {
                (X: 0, Y: 0),
                (X: Math.Max(0, width - 1), Y: 0),
                (X: 0, Y: Math.Max(0, height - 1)),
                (X: Math.Max(0, width - 1), Y: Math.Max(0, height - 1))
            };

            var red = 0;
            var green = 0;
            var blue = 0;
            foreach (var point in samplePoints)
            {
                var offset = point.Y * stride + point.X * 4;
                blue += pixels[offset];
                green += pixels[offset + 1];
                red += pixels[offset + 2];
            }

            return Color.FromRgb((byte)(red / samplePoints.Length), (byte)(green / samplePoints.Length), (byte)(blue / samplePoints.Length));
        }

        static bool IsBackgroundPixel(byte red, byte green, byte blue, Color background, double threshold)
        {
            var distance = Math.Abs(red - background.R) + Math.Abs(green - background.G) + Math.Abs(blue - background.B);
            var isNearCornerBackground = distance <= threshold;
            var isPlainLightBackground = red >= 238 && green >= 238 && blue >= 238 && Math.Abs(red - green) <= 12 && Math.Abs(red - blue) <= 12;
            return isNearCornerBackground || isPlainLightBackground;
        }
    }
}
