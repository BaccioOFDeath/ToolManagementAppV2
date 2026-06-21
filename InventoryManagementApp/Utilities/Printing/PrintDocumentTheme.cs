using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Media;

namespace InventoryManagementApp.Utilities.Printing;

public static class PrintDocumentTheme
{
    public static readonly Brush PageBackgroundBrush = Brushes.White;
    public static readonly Brush BodyForegroundBrush = new SolidColorBrush(Color.FromRgb(31, 41, 55));
    public static readonly Brush MutedForegroundBrush = new SolidColorBrush(Color.FromRgb(75, 85, 99));
    public static readonly Brush HeaderForegroundBrush = new SolidColorBrush(Color.FromRgb(17, 24, 39));
    public static readonly Brush HeaderBackgroundBrush = new SolidColorBrush(Color.FromRgb(230, 236, 246));
    public static readonly Brush AlternatingRowBackgroundBrush = new SolidColorBrush(Color.FromRgb(249, 250, 252));
    public static readonly Brush HeaderPanelBackgroundBrush = new SolidColorBrush(Color.FromRgb(244, 247, 251));
    public static readonly Brush AccentBorderBrush = new SolidColorBrush(Color.FromRgb(49, 130, 206));
    public static readonly Brush RuleBorderBrush = new SolidColorBrush(Color.FromRgb(209, 213, 219));

    static PrintDocumentTheme()
    {
        Freeze(PageBackgroundBrush);
        Freeze(BodyForegroundBrush);
        Freeze(MutedForegroundBrush);
        Freeze(HeaderForegroundBrush);
        Freeze(HeaderBackgroundBrush);
        Freeze(AlternatingRowBackgroundBrush);
        Freeze(HeaderPanelBackgroundBrush);
        Freeze(AccentBorderBrush);
        Freeze(RuleBorderBrush);
    }

    public static void ApplyLightTheme(FlowDocument document)
    {
        ArgumentNullException.ThrowIfNull(document);

        document.Background = PageBackgroundBrush;
        document.Foreground = BodyForegroundBrush;

        foreach (var block in document.Blocks)
            ApplyToTextElement(block);
    }

    static void ApplyToTextElement(TextElement element)
    {
        element.Foreground = BodyForegroundBrush;

        switch (element)
        {
            case Block block:
                block.ClearValue(TextElement.BackgroundProperty);
                break;
            case Inline inline:
                inline.ClearValue(TextElement.BackgroundProperty);
                break;
        }

        switch (element)
        {
            case Section section:
                foreach (var block in section.Blocks)
                    ApplyToTextElement(block);
                break;
            case Paragraph paragraph:
                foreach (var inline in paragraph.Inlines)
                    ApplyToTextElement(inline);
                break;
            case Span span:
                foreach (var inline in span.Inlines)
                    ApplyToTextElement(inline);
                break;
            case List list:
                foreach (var item in list.ListItems)
                    ApplyToTextElement(item);
                break;
            case ListItem item:
                foreach (var block in item.Blocks)
                    ApplyToTextElement(block);
                break;
            case Table table:
                table.Background = PageBackgroundBrush;
                foreach (var rowGroup in table.RowGroups)
                    ApplyToTextElement(rowGroup);
                break;
            case TableRowGroup rowGroup:
                foreach (var row in rowGroup.Rows)
                    ApplyToTextElement(row);
                break;
            case TableRow row:
                row.Background = Brushes.Transparent;
                foreach (var cell in row.Cells)
                    ApplyToTextElement(cell);
                break;
            case TableCell cell:
                cell.Background = Brushes.Transparent;
                foreach (var block in cell.Blocks)
                    ApplyToTextElement(block);
                break;
            case BlockUIContainer blockUi:
                ApplyToElement(blockUi.Child);
                break;
            case InlineUIContainer inlineUi:
                ApplyToElement(inlineUi.Child);
                break;
        }
    }

    static void ApplyToElement(UIElement? element)
    {
        if (element is null)
            return;

        switch (element)
        {
            case TextBlock textBlock:
                textBlock.Foreground = BodyForegroundBrush;
                textBlock.Background = Brushes.Transparent;
                foreach (var inline in textBlock.Inlines)
                    ApplyToTextElement(inline);
                break;
            case Panel panel:
                panel.Background = Brushes.Transparent;
                foreach (UIElement child in panel.Children)
                    ApplyToElement(child);
                break;
            case Decorator decorator:
                ApplyToElement(decorator.Child);
                break;
            case ContentControl contentControl when contentControl.Content is UIElement child:
                ApplyToElement(child);
                break;
            case ItemsControl itemsControl:
                foreach (var item in itemsControl.Items)
                {
                    if (item is UIElement childElement)
                        ApplyToElement(childElement);
                }
                break;
            case Control control:
                control.Foreground = BodyForegroundBrush;
                control.Background = Brushes.Transparent;
                break;
        }
    }

    static void Freeze(Brush brush)
    {
        if (brush.CanFreeze)
            brush.Freeze();
    }
}
