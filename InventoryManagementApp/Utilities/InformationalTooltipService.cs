using System;
using System.Collections.Generic;
using System.Globalization;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Media;

namespace InventoryManagementApp.Utilities
{
    public static class InformationalTooltipService
    {
        private static readonly DependencyProperty ProcessedProperty =
            DependencyProperty.RegisterAttached(
                "Processed",
                typeof(bool),
                typeof(InformationalTooltipService),
                new PropertyMetadata(false));

        private static readonly string[] InstructionPrefixes =
        {
            "use ",
            "select ",
            "search ",
            "clear ",
            "double-click",
            "open ",
            "keep ",
            "confirm ",
            "capture ",
            "choose ",
            "export ",
            "import ",
            "adjust ",
            "set ",
            "apply",
            "tick ",
            "shown ",
            "singular ",
            "plural ",
            "re-enter",
            "ready to",
            "review ",
            "prioritize ",
            "find ",
            "redesign ",
            "pick ",
            "quick ",
            "before ",
            "after ",
            "when ",
            "if ",
            "this ",
            "these ",
            "passwords should",
            "no returns"
        };

        private static readonly string[] InstructionFragments =
        {
            " before ",
            " after ",
            " should ",
            " can ",
            " drives ",
            " keeps ",
            " to ",
            " for ",
            " from ",
            " during "
        };

        public static void Apply(DependencyObject root)
        {
            foreach (var textBlock in EnumerateDescendants(root))
            {
                if (textBlock is not TextBlock caption || !IsStaticInstruction(caption))
                    continue;

                var target = FindTooltipTarget(caption);
                if (target == null)
                    continue;

                AddTooltip(target, caption.Text.Trim());
                caption.Visibility = Visibility.Collapsed;
            }
        }

        internal static bool IsStaticInstruction(TextBlock textBlock)
        {
            if ((bool)textBlock.GetValue(ProcessedProperty))
                return false;

            textBlock.SetValue(ProcessedProperty, true);

            if (BindingOperations.GetBindingExpressionBase(textBlock, TextBlock.TextProperty) != null)
                return false;

            if (textBlock.Visibility != Visibility.Visible)
                return false;

            var text = textBlock.Text?.Trim();
            if (string.IsNullOrWhiteSpace(text) || text.Length < 18)
                return false;

            var lower = text.ToLower(CultureInfo.InvariantCulture);
            if (Array.Exists(InstructionPrefixes, prefix => lower.StartsWith(prefix, StringComparison.Ordinal)))
                return true;

            return text.EndsWith(".", StringComparison.Ordinal)
                && Array.Exists(InstructionFragments, fragment => lower.Contains(fragment, StringComparison.Ordinal));
        }

        private static FrameworkElement? FindTooltipTarget(TextBlock caption)
        {
            var parent = LogicalTreeHelper.GetParent(caption);

            if (parent is Panel panel)
            {
                var previous = FindPreviousSibling(panel, caption);
                if (previous != null)
                    return previous;
            }

            return parent as FrameworkElement;
        }

        private static FrameworkElement? FindPreviousSibling(Panel panel, TextBlock caption)
        {
            FrameworkElement? previous = null;

            foreach (UIElement child in panel.Children)
            {
                if (ReferenceEquals(child, caption))
                    return previous;

                if (child is FrameworkElement element && element.Visibility == Visibility.Visible)
                    previous = element;
            }

            return previous;
        }

        private static void AddTooltip(FrameworkElement target, string text)
        {
            switch (target.ToolTip)
            {
                case null:
                    target.ToolTip = text;
                    break;
                case string existing when !existing.Contains(text, StringComparison.Ordinal):
                    target.ToolTip = $"{existing}{Environment.NewLine}{Environment.NewLine}{text}";
                    break;
                case TextBlock existingText when !existingText.Text.Contains(text, StringComparison.Ordinal):
                    existingText.Text = $"{existingText.Text}{Environment.NewLine}{Environment.NewLine}{text}";
                    break;
            }
        }

        private static IEnumerable<DependencyObject> EnumerateDescendants(DependencyObject root)
        {
            var visited = new HashSet<DependencyObject>(ReferenceEqualityComparer.Instance);
            var queue = new Queue<DependencyObject>();
            visited.Add(root);
            queue.Enqueue(root);

            while (queue.Count > 0)
            {
                var current = queue.Dequeue();
                yield return current;

                foreach (var child in GetChildren(current))
                {
                    if (visited.Add(child))
                        queue.Enqueue(child);
                }
            }
        }

        private static IEnumerable<DependencyObject> GetChildren(DependencyObject current)
        {
            var visualChildren = 0;

            try
            {
                visualChildren = VisualTreeHelper.GetChildrenCount(current);
            }
            catch (InvalidOperationException)
            {
            }

            for (var i = 0; i < visualChildren; i++)
                yield return VisualTreeHelper.GetChild(current, i);

            foreach (var child in LogicalTreeHelper.GetChildren(current))
            {
                if (child is DependencyObject dependencyObject)
                    yield return dependencyObject;
            }

            if (current is ContentControl { Content: DependencyObject content })
                yield return content;
        }
    }
}
