using System;
using System.IO;
using System.Linq;
using InventoryManagementApp.Utilities;
using Xunit;

namespace InventoryManagementApp.Tests
{
    public class QaScreenshotResolutionTests
    {
        [Fact]
        public void QaScreenshotOptionsParseExplicitWindowSize()
        {
            var options = QaScreenshotRunOptions.Parse(new[]
            {
                "InventoryManagementApp.exe",
                "--qa-screenshots",
                "--qa-window-width=1366",
                "--qa-window-height=650",
                "--qa-captures=02-operations/02-rentals.png,06-dialogs/09-rentals-filter.png"
            });

            Assert.NotNull(options);
            Assert.Equal(1366, options!.WindowWidth);
            Assert.Equal(650, options.WindowHeight);
            Assert.False(options.FullScreen);
            Assert.Equal(new[] { "02-operations/02-rentals.png", "06-dialogs/09-rentals-filter.png" }, options.CaptureFilters);
        }

        [Fact]
        public void QaPrintPdfOptionsParseOutputAndCaptureFilters()
        {
            var options = QaScreenshotRunOptions.Parse(new[]
            {
                "InventoryManagementApp.exe",
                "--qa-print-pdfs",
                "--qa-output-dir=C:\\qa\\print-pdfs",
                "--qa-captures=08-rental-invoice.pdf,activity"
            });

            Assert.NotNull(options);
            Assert.True(options!.PrintPdfMode);
            Assert.Equal("C:\\qa\\print-pdfs", options.OutputDirectory);
            Assert.Equal(new[] { "08-rental-invoice.pdf", "activity" }, options.CaptureFilters);
        }

        [Fact]
        public void QaScreenshotScriptDefinesRequiredResolutionFolders()
        {
            var script = ReadRepositoryFile("scripts", "run-app-qa-screenshots.ps1");

            Assert.Contains("$physicalResolutionRuns = @(", script, StringComparison.Ordinal);
            Assert.Contains("$browserViewportRuns = @(", script, StringComparison.Ordinal);
            Assert.Contains("1366x768-old-small-laptop", script, StringComparison.Ordinal);
            Assert.Contains("3840x2160-4k-desktop", script, StringComparison.Ordinal);
            Assert.Contains("1280x720-cramped-fallback", script, StringComparison.Ordinal);
            Assert.Contains("3840x2000-4k-browser-space", script, StringComparison.Ordinal);
            Assert.Contains("--qa-window-width=$WindowWidth", script, StringComparison.Ordinal);
            Assert.Contains("--qa-window-height=$WindowHeight", script, StringComparison.Ordinal);
            Assert.Contains("[string[]]$Resolution = @()", script, StringComparison.Ordinal);
            Assert.Contains("[string[]]$Capture = @()", script, StringComparison.Ordinal);
            Assert.Contains("--qa-captures=$($Capture -join ',')", script, StringComparison.Ordinal);
            Assert.Contains("Test-CaptureFilterMatch", script, StringComparison.Ordinal);
            Assert.Contains("Join-Path (Join-Path $sessionOutput $resolutionRun.Group) $resolutionRun.Name", script, StringComparison.Ordinal);
            Assert.Contains("InventoryManagementApp\\Assets\\Themes\\Good.json", script, StringComparison.Ordinal);
        }

        [Fact]
        public void QaPrintPdfScriptRunsAppModeAndValidatesExpectedPrintSurfaces()
        {
            var script = ReadRepositoryFile("scripts", "run-app-qa-print-pdfs.ps1");

            Assert.Contains("--qa-print-pdfs", script, StringComparison.Ordinal);
            Assert.Contains("[int]$ExpectedPdfCount = 19", script, StringComparison.Ordinal);
            Assert.Contains("08-rental-invoice.pdf", script, StringComparison.Ordinal);
            Assert.Contains("19-reports-preview.pdf", script, StringComparison.Ordinal);
            Assert.Contains("Get-PdfPageCount", script, StringComparison.Ordinal);
            Assert.Contains("does not start with a PDF header", script, StringComparison.Ordinal);
            Assert.Contains("Open '$sessionOutput\\README.md' for the document manifest", script, StringComparison.Ordinal);
        }

        private static string ReadRepositoryFile(params string[] relativePathParts)
        {
            var baseDirectory = AppContext.BaseDirectory;
            var repositoryRoot = Path.GetFullPath(Path.Combine(baseDirectory, "..", "..", "..", ".."));
            return File.ReadAllText(Path.Combine(new[] { repositoryRoot }.Concat(relativePathParts).ToArray()));
        }
    }
}
