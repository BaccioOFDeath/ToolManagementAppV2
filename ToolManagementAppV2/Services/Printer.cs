// Services/Printer.cs
using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using ToolManagementAppV2.Models;
using ToolManagementAppV2.Views;

namespace ToolManagementAppV2.Services
{
    public class Printer
    {
        private readonly SettingsService _settingsService;

        public Printer(SettingsService settingsService)
        {
            _settingsService = settingsService;
        }

        /// <summary>
        /// Prints a sorted list of tools with the given title.
        /// If currentUserName is non‐null, only prints tools checked out by that user.
        /// </summary>
        public void PrintTools(IEnumerable<Tool> tools, string title, string currentUserName = null)
        {
            // 1) sort by Location
            var list = tools.OrderBy(t => t.Location).ToList();

            // 2) filter if needed
            if (!string.IsNullOrEmpty(currentUserName))
                list = list.Where(t => t.CheckedOutBy == currentUserName).ToList();

            // 3) build the document
            var logoPath = LoadCompanyLogoPath();
            var doc = BuildDocument(list, title, logoPath);

            // 4) show preview window instead of calling PrintDialog directly
            var preview = new PrintPreviewWindow();
            preview.ShowPreview(doc, title, logoPath);
            // printing now happens from inside the preview window
        }

        private string? LoadCompanyLogoPath()
        {
            var path = _settingsService.GetSetting("CompanyLogoPath");
            return !string.IsNullOrEmpty(path) && File.Exists(path)
                ? path
                : null;
        }

        private FlowDocument BuildDocument(List<Tool> tools, string title, string logoPath)
        {
            var doc = new FlowDocument
            {
                PagePadding = new Thickness(50),
                PageWidth = 8.27 * 96,
                PageHeight = 11.69 * 96,
                FontFamily = new FontFamily("Calibri"),
                FontSize = 18
            };

            // Header: logo + title
            var headerContainer = new BlockUIContainer();
            var headerStack = new StackPanel { Orientation = Orientation.Vertical };
            AddCompanyLogo(headerStack, logoPath);
            AddTitle(headerStack, title);
            headerContainer.Child = headerStack;
            doc.Blocks.Add(headerContainer);

            // Each tool
            foreach (var t in tools)
            {
                var row = CreateToolRow(t);
                var block = new BlockUIContainer(row)
                {
                    Margin = new Thickness(0, 20, 0, 20)
                };
                doc.Blocks.Add(block);
            }

            return doc;
        }

        private StackPanel CreateToolRow(Tool tool)
        {
            var panel = new StackPanel { Orientation = Orientation.Vertical };
            var grid = new Grid();
            // columns: image | details left | details right
            grid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(150) });
            grid.ColumnDefinitions.Add(new ColumnDefinition());
            grid.ColumnDefinitions.Add(new ColumnDefinition { Width = GridLength.Auto });

            // image or placeholder
            if (!string.IsNullOrEmpty(tool.ToolImagePath) && File.Exists(tool.ToolImagePath))
            {
                var imgBorder = CreateOptimizedImage(tool.ToolImagePath);
                Grid.SetColumn(imgBorder, 0);
                grid.Children.Add(imgBorder);
            }
            else
            {
                var ph = new Border
                {
                    Width = 120,
                    Height = 120,
                    Background = Brushes.LightGray,
                    CornerRadius = new CornerRadius(10)
                };
                Grid.SetColumn(ph, 0);
                grid.Children.Add(ph);
            }

            // left: ID + description
            var left = new TextBlock
            {
                FontSize = 14,
                TextWrapping = TextWrapping.Wrap,
                Margin = new Thickness(10, 0, 0, 0)
            };
            left.Inlines.Add(new Run("\n"));
            left.Inlines.Add(new Bold(new Run("Tool ID: ")));
            left.Inlines.Add(new Run(tool.ToolID + "\n"));
            left.Inlines.Add(new Bold(new Run("NameDescription: ")));
            left.Inlines.Add(new Run(tool.NameDescription + "\n"));
            Grid.SetColumn(left, 1);
            grid.Children.Add(left);

            // right: location, who, when
            var right = new TextBlock
            {
                FontSize = 14,
                TextWrapping = TextWrapping.Wrap,
                HorizontalAlignment = HorizontalAlignment.Right,
                TextAlignment = TextAlignment.Right,
                Margin = new Thickness(0, 0, 20, 0)
            };

            right.Inlines.Add(new LineBreak());
            right.Inlines.Add(new Bold(new Run("Location: ")));
            right.Inlines.Add(new Run(tool.Location));
            right.Inlines.Add(new LineBreak());
            right.Inlines.Add(new LineBreak());
            right.Inlines.Add(new Bold(new Run("Checked Out By: ")));
            right.Inlines.Add(new Run(tool.CheckedOutBy));
            right.Inlines.Add(new LineBreak());
            right.Inlines.Add(new Bold(new Run("Checked Out Time: ")));
            if (tool.CheckedOutTime.HasValue)
                right.Inlines.Add(new Run(tool.CheckedOutTime.Value.ToString("yyyy-MM-dd HH:mm:ss")));
            else
                right.Inlines.Add(new Run("N/A"));
            right.Inlines.Add(new LineBreak());

            Grid.SetColumn(right, 2);
            grid.Children.Add(right);

            panel.Children.Add(grid);
            panel.Children.Add(new System.Windows.Shapes.Rectangle
            {
                Height = 1,
                Fill = Brushes.LightGray
            });

            return panel;
        }

        private Border CreateOptimizedImage(string path)
        {
            var bmp = new BitmapImage();
            bmp.BeginInit();
            bmp.CacheOption = BitmapCacheOption.OnLoad;
            bmp.DecodePixelWidth = 720;
            bmp.UriSource = new Uri(path, UriKind.Absolute);
            bmp.EndInit();
            bmp.Freeze();

            return new Border
            {
                Width = 120,
                Height = 120,
                CornerRadius = new CornerRadius(10),
                Background = new ImageBrush(bmp) { Stretch = Stretch.UniformToFill },
                ClipToBounds = true
            };
        }

        private void AddCompanyLogo(StackPanel host, string logoPath)
        {
            if (string.IsNullOrEmpty(logoPath) || !File.Exists(logoPath)) return;
            host.Children.Add(new Image
            {
                Source = new BitmapImage(new Uri(logoPath, UriKind.Absolute)),
                Width = 50,
                Height = 50,
                Stretch = Stretch.Uniform
            });
        }

        private void AddTitle(StackPanel host, string title)
        {
            host.Children.Add(new TextBlock(new Bold(new Run(title)))
            {
                FontSize = 20,
                TextAlignment = TextAlignment.Center,
                Margin = new Thickness(0, 5, 0, 0)
            });
        }

        private class CustomDocumentPaginator : DocumentPaginator
        {
            private readonly DocumentPaginator _inner;
            private readonly Typeface _tf;
            private readonly double _fs;

            public CustomDocumentPaginator(DocumentPaginator inner, Typeface tf, double fs)
            {
                _inner = inner;
                _tf = tf;
                _fs = fs;
            }

            [Obsolete]
            public override DocumentPage GetPage(int pageNumber)
            {
                var page = _inner.GetPage(pageNumber);
                var dv = new DrawingVisual();
                using (var dc = dv.RenderOpen())
                {
                    var text = $"Page {pageNumber + 1}";
                    var ft = new FormattedText(
                        text,
                        CultureInfo.CurrentCulture,
                        FlowDirection.LeftToRight,
                        _tf,
                        _fs,
                        Brushes.Black);

                    var x = (page.Size.Width - ft.Width) / 2;
                    var y = page.Size.Height - ft.Height - 50;
                    dc.DrawText(ft, new Point(x, y));
                }
                var cv = new ContainerVisual();
                cv.Children.Add(page.Visual);
                cv.Children.Add(dv);
                return new DocumentPage(cv, page.Size, page.BleedBox, page.ContentBox);
            }

            public override bool IsPageCountValid => _inner.IsPageCountValid;
            public override int PageCount => _inner.PageCount;
            public override Size PageSize
            {
                get => _inner.PageSize;
                set => _inner.PageSize = value;
            }
            public override IDocumentPaginatorSource Source => _inner.Source;
        }
    }
}
