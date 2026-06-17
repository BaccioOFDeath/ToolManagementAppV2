using System;
using System.IO;
using InventoryManagementApp.Interfaces;

namespace InventoryManagementApp.Utilities
{
    internal sealed class QaScreenshotRunOptions
    {
        public const string DefaultAdminUserName = "admin";

        public required string OutputDirectory { get; init; }
        public required string ApplicationName { get; init; }
        public required string ItemLabelSingular { get; init; }
        public required string ItemLabelPlural { get; init; }
        public required string AdminPassword { get; init; }
        public string AdminUserName { get; init; } = DefaultAdminUserName;
        public double NarrowWindowWidth { get; init; } = 1040d;

        public static QaScreenshotRunOptions? Parse(string[] args)
        {
            if (args == null || args.Length == 0)
                return null;

            var enabled = false;
            var outputDirectory = string.Empty;
            var applicationName = "QA Inventory";
            var itemLabelSingular = "Tool";
            var itemLabelPlural = "Tools";
            var adminPassword = "AdminQ123";
            var narrowWindowWidth = 1040d;

            foreach (var arg in args)
            {
                if (string.Equals(arg, "--qa-screenshots", StringComparison.OrdinalIgnoreCase))
                {
                    enabled = true;
                    continue;
                }

                if (TryReadValue(arg, "--qa-output-dir=", out var output))
                {
                    outputDirectory = output;
                    continue;
                }

                if (TryReadValue(arg, "--qa-app-name=", out var appName))
                {
                    applicationName = appName;
                    continue;
                }

                if (TryReadValue(arg, "--qa-item-singular=", out var singular))
                {
                    itemLabelSingular = singular;
                    continue;
                }

                if (TryReadValue(arg, "--qa-item-plural=", out var plural))
                {
                    itemLabelPlural = plural;
                    continue;
                }

                if (TryReadValue(arg, "--qa-password=", out var password))
                {
                    adminPassword = password;
                    continue;
                }

                if (TryReadValue(arg, "--qa-narrow-width=", out var narrowWidthText) &&
                    double.TryParse(narrowWidthText, out var parsedNarrowWidth) &&
                    parsedNarrowWidth > 0)
                {
                    narrowWindowWidth = parsedNarrowWidth;
                }
            }

            if (!enabled)
                return null;

            if (string.IsNullOrWhiteSpace(outputDirectory))
                outputDirectory = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "qa-screenshots");

            return new QaScreenshotRunOptions
            {
                OutputDirectory = outputDirectory,
                ApplicationName = applicationName,
                ItemLabelSingular = itemLabelSingular,
                ItemLabelPlural = itemLabelPlural,
                AdminPassword = adminPassword,
                NarrowWindowWidth = narrowWindowWidth
            };
        }

        public SetupWizardResult ToSetupWizardResult()
            => new(AdminPassword, ApplicationName, ItemLabelSingular, ItemLabelPlural, string.Empty);

        public string BuildItemSlug()
            => ItemLabelPlural.Trim().ToLowerInvariant().Replace(' ', '-');

        static bool TryReadValue(string argument, string prefix, out string value)
        {
            if (argument.StartsWith(prefix, StringComparison.OrdinalIgnoreCase))
            {
                value = argument[prefix.Length..].Trim('"');
                return true;
            }

            value = string.Empty;
            return false;
        }
    }
}
