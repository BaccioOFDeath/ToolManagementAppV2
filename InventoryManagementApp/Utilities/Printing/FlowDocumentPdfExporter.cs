using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Documents;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using InventoryManagementApp.Views.Windows;

namespace InventoryManagementApp.Utilities.Printing
{
    internal sealed record FlowDocumentPdfExportResult(int PageCount, long FileSizeBytes);

    internal static class FlowDocumentPdfExporter
    {
        public static FlowDocumentPdfExportResult Export(FlowDocument document, string title, string outputPath)
        {
            if (document is null)
                throw new ArgumentNullException(nameof(document));
            if (string.IsNullOrWhiteSpace(outputPath))
                throw new ArgumentException("Output path cannot be empty.", nameof(outputPath));

            PrintPreviewWindow.PrepareDocumentForPrint(document, title);

            var paginator = ((IDocumentPaginatorSource)document).DocumentPaginator;
            paginator.PageSize = new Size(PrintPreviewWindow.DefaultPreviewPageWidth, PrintPreviewWindow.DefaultPreviewPageHeight);
            paginator.ComputePageCount();

            var pageImages = new List<byte[]>();
            for (var pageIndex = 0; pageIndex < paginator.PageCount; pageIndex++)
            {
                var renderedPage = RenderPageToJpeg(paginator.GetPage(pageIndex), paginator.PageSize);
                if (renderedPage.HasMeaningfulContent || pageImages.Count == 0)
                    pageImages.Add(renderedPage.ImageBytes);
            }

            Directory.CreateDirectory(Path.GetDirectoryName(outputPath) ?? ".");
            WriteImagePdf(outputPath, pageImages, paginator.PageSize);

            var file = new FileInfo(outputPath);
            return new FlowDocumentPdfExportResult(pageImages.Count, file.Length);
        }

        private static RenderedPdfPage RenderPageToJpeg(DocumentPage page, Size pageSize)
        {
            var width = Math.Max(1, (int)Math.Ceiling(pageSize.Width));
            var height = Math.Max(1, (int)Math.Ceiling(pageSize.Height));
            var visual = new DrawingVisual();

            using (var context = visual.RenderOpen())
            {
                context.DrawRectangle(Brushes.White, null, new Rect(0, 0, pageSize.Width, pageSize.Height));
                context.DrawRectangle(new VisualBrush(page.Visual), null, new Rect(0, 0, pageSize.Width, pageSize.Height));
            }

            var bitmap = new RenderTargetBitmap(width, height, 96, 96, PixelFormats.Pbgra32);
            bitmap.Render(visual);

            var stride = width * 4;
            var pixels = new byte[stride * height];
            bitmap.CopyPixels(pixels, stride, 0);
            var darkPixelCount = 0;
            for (var index = 0; index < pixels.Length; index += 4)
            {
                if (pixels[index] < 245 || pixels[index + 1] < 245 || pixels[index + 2] < 245)
                    darkPixelCount++;
            }

            var meaningfulContent = darkPixelCount / (double)(width * height) >= 0.01;

            var encoder = new JpegBitmapEncoder { QualityLevel = 92 };
            encoder.Frames.Add(BitmapFrame.Create(bitmap));

            using var stream = new MemoryStream();
            encoder.Save(stream);
            return new RenderedPdfPage(stream.ToArray(), meaningfulContent);
        }

        private sealed record RenderedPdfPage(byte[] ImageBytes, bool HasMeaningfulContent);

        private static void WriteImagePdf(string outputPath, IReadOnlyList<byte[]> pageImages, Size pageSize)
        {
            using var stream = File.Create(outputPath);
            using var writer = new BinaryWriter(stream, Encoding.ASCII, leaveOpen: true);
            var offsets = new List<long> { 0 };
            var objectNumber = 1;

            writer.Write(Encoding.ASCII.GetBytes("%PDF-1.4\n%\u00e2\u00e3\u00cf\u00d3\n"));

            void BeginObject(int id)
            {
                offsets.Add(stream.Position);
                writer.Write(Encoding.ASCII.GetBytes(FormattableString.Invariant($"{id} 0 obj\n")));
            }

            void EndObject() => writer.Write(Encoding.ASCII.GetBytes("endobj\n"));

            var catalogObject = objectNumber++;
            var pagesObject = objectNumber++;
            var pageObjects = new List<int>();
            var contentObjects = new List<int>();
            var imageObjects = new List<int>();

            foreach (var _ in pageImages)
            {
                pageObjects.Add(objectNumber++);
                contentObjects.Add(objectNumber++);
                imageObjects.Add(objectNumber++);
            }

            BeginObject(catalogObject);
            writer.Write(Encoding.ASCII.GetBytes(FormattableString.Invariant($"<< /Type /Catalog /Pages {pagesObject} 0 R >>\n")));
            EndObject();

            BeginObject(pagesObject);
            var kids = string.Join(" ", pageObjects.Select(id => FormattableString.Invariant($"{id} 0 R")));
            writer.Write(Encoding.ASCII.GetBytes(FormattableString.Invariant($"<< /Type /Pages /Count {pageObjects.Count} /Kids [{kids}] >>\n")));
            EndObject();

            for (var index = 0; index < pageImages.Count; index++)
            {
                var width = PdfNumber(pageSize.Width);
                var height = PdfNumber(pageSize.Height);
                var imageName = $"Im{index + 1}";

                BeginObject(pageObjects[index]);
                writer.Write(Encoding.ASCII.GetBytes(
                    FormattableString.Invariant(
                        $"<< /Type /Page /Parent {pagesObject} 0 R /MediaBox [0 0 {width} {height}] /Resources << /XObject << /{imageName} {imageObjects[index]} 0 R >> >> /Contents {contentObjects[index]} 0 R >>\n")));
                EndObject();

                var content = Encoding.ASCII.GetBytes(FormattableString.Invariant($"q\n{width} 0 0 {height} 0 0 cm\n/{imageName} Do\nQ\n"));
                BeginObject(contentObjects[index]);
                writer.Write(Encoding.ASCII.GetBytes(FormattableString.Invariant($"<< /Length {content.Length} >>\nstream\n")));
                writer.Write(content);
                writer.Write(Encoding.ASCII.GetBytes("\nendstream\n"));
                EndObject();

                var image = pageImages[index];
                BeginObject(imageObjects[index]);
                writer.Write(Encoding.ASCII.GetBytes(
                    FormattableString.Invariant(
                        $"<< /Type /XObject /Subtype /Image /Width {(int)Math.Ceiling(pageSize.Width)} /Height {(int)Math.Ceiling(pageSize.Height)} /ColorSpace /DeviceRGB /BitsPerComponent 8 /Filter /DCTDecode /Length {image.Length} >>\nstream\n")));
                writer.Write(image);
                writer.Write(Encoding.ASCII.GetBytes("\nendstream\n"));
                EndObject();
            }

            var xrefPosition = stream.Position;
            writer.Write(Encoding.ASCII.GetBytes(FormattableString.Invariant($"xref\n0 {objectNumber}\n")));
            writer.Write(Encoding.ASCII.GetBytes("0000000000 65535 f \n"));
            foreach (var offset in offsets.Skip(1))
                writer.Write(Encoding.ASCII.GetBytes(FormattableString.Invariant($"{offset:0000000000} 00000 n \n")));

            writer.Write(Encoding.ASCII.GetBytes(
                FormattableString.Invariant(
                    $"trailer\n<< /Size {objectNumber} /Root {catalogObject} 0 R >>\nstartxref\n{xrefPosition}\n%%EOF\n")));
        }

        private static string PdfNumber(double value)
            => value.ToString("0.###", CultureInfo.InvariantCulture);
    }
}
