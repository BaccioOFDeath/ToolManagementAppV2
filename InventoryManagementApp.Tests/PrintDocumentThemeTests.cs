using System;
using System.Threading;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Media;
using InventoryManagementApp.Utilities.Printing;
using Xunit;

namespace InventoryManagementApp.Tests;

public class PrintDocumentThemeTests
{
    [Fact]
    public void ApplyLightTheme_ReplacesDarkDocumentAndEmbeddedUiBrushes()
    {
        Exception? failure = null;

        var thread = new Thread(() =>
        {
            try
            {
                var run = new Run("Dark themed text") { Foreground = Brushes.White };
                var paragraph = new Paragraph(run)
                {
                    Background = Brushes.Black,
                    Foreground = Brushes.White
                };
                var label = new TextBlock
                {
                    Text = "Label",
                    Foreground = Brushes.White,
                    Background = Brushes.Black
                };
                var document = new FlowDocument
                {
                    Background = Brushes.Black,
                    Foreground = Brushes.White
                };

                document.Blocks.Add(paragraph);
                document.Blocks.Add(new BlockUIContainer(new StackPanel
                {
                    Children = { label }
                }));

                PrintDocumentTheme.ApplyLightTheme(document);

                Assert.Same(PrintDocumentTheme.PageBackgroundBrush, document.Background);
                Assert.Same(PrintDocumentTheme.BodyForegroundBrush, document.Foreground);
                Assert.Same(PrintDocumentTheme.BodyForegroundBrush, paragraph.Foreground);
                Assert.Same(PrintDocumentTheme.BodyForegroundBrush, run.Foreground);
                Assert.Same(PrintDocumentTheme.BodyForegroundBrush, label.Foreground);
                Assert.Same(Brushes.Transparent, label.Background);
                Assert.Equal(DependencyProperty.UnsetValue, paragraph.ReadLocalValue(TextElement.BackgroundProperty));
            }
            catch (Exception ex)
            {
                failure = ex;
            }
        });

        thread.SetApartmentState(ApartmentState.STA);
        thread.Start();
        thread.Join();

        if (failure != null)
            throw failure;
    }
}
