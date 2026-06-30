using System;
using System.Collections.Generic;
using System.IO;
using System.Text.RegularExpressions;
using Xunit;

namespace InventoryManagementApp.Tests
{
    public class ViewButtonActionWiringTests
    {
        static readonly string[] RoutedEvents =
        {
            "Click",
            "SelectionChanged",
            "TextChanged",
            "KeyDown",
            "MouseDoubleClick"
        };

        [Fact]
        public void ViewButtonsAreWiredToAnActionOrMarkedAsPreviewOnly()
        {
            var failures = new List<string>();

            foreach (var file in EnumerateViewXamlFiles())
            {
                var xaml = File.ReadAllText(file);
                foreach (Match match in Regex.Matches(xaml, @"<Button\b(?:(?!/>|</Button>).)*(?:/>|</Button>)", RegexOptions.Singleline))
                {
                    var button = match.Value;
                    if (HasAttribute(button, "Command") || HasAttribute(button, "Click") || IsPreviewOnly(button))
                        continue;

                    failures.Add($"{RelativePath(file)}:{LineNumber(xaml, match.Index)} Button '{DisplayLabel(button)}' has no Command or Click handler.");
                }
            }

            Assert.Empty(failures);
        }

        [Fact]
        public void ViewRoutedEventHandlersExistInCodeBehind()
        {
            var failures = new List<string>();

            foreach (var file in EnumerateViewXamlFiles())
            {
                var codeBehind = Path.ChangeExtension(file, ".xaml.cs");
                if (!File.Exists(codeBehind))
                    continue;

                var xaml = File.ReadAllText(file);
                var code = File.ReadAllText(codeBehind);

                foreach (var routedEvent in RoutedEvents)
                {
                    foreach (Match match in Regex.Matches(xaml, $@"\b{routedEvent}\s*=\s*""([^""]+)"""))
                    {
                        var handler = match.Groups[1].Value;
                        if (code.Contains(handler, StringComparison.Ordinal))
                            continue;

                        failures.Add($"{RelativePath(file)}:{LineNumber(xaml, match.Index)} {routedEvent} handler '{handler}' is not present in code-behind.");
                    }
                }
            }

            Assert.Empty(failures);
        }

        static bool IsPreviewOnly(string button)
            => HasAttributeValue(button, "IsHitTestVisible", "False") && HasAttributeValue(button, "Focusable", "False");

        static bool HasAttribute(string text, string attribute)
            => Regex.IsMatch(text, $@"\b{Regex.Escape(attribute)}\s*=");

        static bool HasAttributeValue(string text, string attribute, string value)
            => Regex.IsMatch(text, $@"\b{Regex.Escape(attribute)}\s*=\s*""{Regex.Escape(value)}""", RegexOptions.IgnoreCase);

        static string DisplayLabel(string button)
        {
            var content = Regex.Match(button, @"\bContent\s*=\s*""([^""]*)""");
            if (content.Success)
                return content.Groups[1].Value;

            var toolTip = Regex.Match(button, @"\bToolTip\s*=\s*""([^""]*)""");
            return toolTip.Success ? toolTip.Groups[1].Value : "(unlabelled)";
        }

        static int LineNumber(string text, int index)
            => text[..index].Split('\n').Length;

        static IEnumerable<string> EnumerateViewXamlFiles()
        {
            var viewsDirectory = RepoPath("InventoryManagementApp", "Views");
            return Directory.EnumerateFiles(viewsDirectory, "*.xaml", SearchOption.AllDirectories);
        }

        static string RelativePath(string path)
            => Path.GetRelativePath(RepoPath(), path).Replace('\\', '/');

        static string RepoPath(params string[] parts)
        {
            var directory = AppContext.BaseDirectory;
            while (!string.IsNullOrEmpty(directory))
            {
                var candidate = Path.Combine(directory, Path.Combine(parts));
                if (Directory.Exists(candidate) || File.Exists(candidate))
                    return candidate;

                var parent = Directory.GetParent(directory);
                if (parent is null)
                    break;

                directory = parent.FullName;
            }

            throw new DirectoryNotFoundException($"Could not find repository path: {Path.Combine(parts)}");
        }
    }
}
