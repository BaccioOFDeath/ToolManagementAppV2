using System;
using System.IO;
using Xunit;

namespace InventoryManagementApp.Tests
{
    public class ValidationDocumentationContractTests
    {
        [Fact]
        public void ReadmeDocumentsValidationRunnerAsPrimaryEntryPoint()
        {
            var source = ReadRepoFile("README.md");
            const string currentStatusGuidance = "Use the checked-in validation runner for the current restore/build/test/publish/check sequence:";
            const string developmentGuidance = "Validation commands from the repository root:";
            const string fullRunnerCommand = "pwsh -File scripts/run-full-validation.ps1";
            const string skipPublishCommand = "pwsh -File scripts/run-full-validation.ps1 -SkipPublish";
            const string skipPublishGuidance = "For a faster compile-and-test pass without publishing or source scan checks:";
            const string manualEquivalent = "Manual equivalent:";

            Assert.Contains(currentStatusGuidance, source);
            Assert.Contains(developmentGuidance, source);
            Assert.Contains(fullRunnerCommand, source);
            Assert.Contains(skipPublishGuidance, source);
            Assert.Contains(skipPublishCommand, source);
            Assert.Contains(manualEquivalent, source);
            AssertAppearsBefore(source, currentStatusGuidance, fullRunnerCommand, "The README should present the checked-in validation runner as the current validation entrypoint.");
            AssertAppearsBeforeAfter(source, developmentGuidance, fullRunnerCommand, manualEquivalent, "The Development section should point maintainers to the runner before listing manual commands.");
            AssertAppearsBefore(source, skipPublishGuidance, skipPublishCommand, "The README should document the fast compile-and-test checkpoint with the explicit SkipPublish command.");
        }

        [Fact]
        public void ReadmeManualValidationAuditsVulnerablePackagesAfterRestoreBeforeBuild()
        {
            var source = ReadRepoFile("README.md");
            const string restoreCommand = "dotnet restore InventoryManagementApp.sln";
            const string auditCommand = "dotnet list InventoryManagementApp.sln package --vulnerable --include-transitive";
            const string buildCommand = "dotnet build InventoryManagementApp.sln --configuration Release --no-restore";

            Assert.Contains("Manual equivalent:", source);
            Assert.Contains(restoreCommand, source);
            Assert.Contains(auditCommand, source);
            Assert.Contains(buildCommand, source);
            AssertAppearsBefore(source, restoreCommand, auditCommand, "The README manual validation sequence should audit packages after restore.");
            AssertAppearsBefore(source, auditCommand, buildCommand, "The README manual validation sequence should audit packages before the no-restore build.");
        }

        [Fact]
        public void ReadmeManualValidationCleansPublishOutputBeforePublishing()
        {
            var source = ReadRepoFile("README.md");
            const string cleanPublishCommand = "if (Test-Path ./publish) { Remove-Item ./publish -Recurse -Force }";
            const string publishCommand = "dotnet publish InventoryManagementApp/InventoryManagementApp.csproj -c Release -r win-x64 --self-contained false --no-restore -o ./publish";

            Assert.Contains("Manual equivalent:", source);
            Assert.Contains(cleanPublishCommand, source);
            Assert.Contains(publishCommand, source);

            var cleanIndex = source.IndexOf(cleanPublishCommand, StringComparison.Ordinal);
            var publishIndex = source.IndexOf(publishCommand, StringComparison.Ordinal);

            Assert.True(cleanIndex >= 0, "The README manual validation sequence should document publish-output cleanup.");
            Assert.True(publishIndex >= 0, "The README manual validation sequence should document the publish command.");
            Assert.True(cleanIndex < publishIndex, "The README manual validation sequence should clean stale publish output before publishing fresh artifacts.");
        }

        [Fact]
        public void ReadmeManualValidationDocumentsFullReleaseSequenceInRunnerOrder()
        {
            var source = ReadRepoFile("README.md");
            var orderedCommands = new[]
            {
                "dotnet restore InventoryManagementApp.sln",
                "dotnet list InventoryManagementApp.sln package --vulnerable --include-transitive",
                "dotnet build InventoryManagementApp.sln --configuration Release --no-restore",
                "dotnet test InventoryManagementApp.sln --configuration Release --no-build --verbosity normal",
                "dotnet restore InventoryManagementApp/InventoryManagementApp.csproj --runtime win-x64",
                "if (Test-Path ./publish) { Remove-Item ./publish -Recurse -Force }",
                "dotnet publish InventoryManagementApp/InventoryManagementApp.csproj -c Release -r win-x64 --self-contained false --no-restore -o ./publish",
                "bash scripts/check-banned-words.sh",
                "$env:BANNED_WORD_CHECK_FORCE_POWERSHELL = \"1\"; bash scripts/check-banned-words.sh; Remove-Item Env:BANNED_WORD_CHECK_FORCE_POWERSHELL"
            };

            Assert.Contains("Manual equivalent:", source);

            for (var index = 0; index < orderedCommands.Length; index++)
            {
                Assert.Contains(orderedCommands[index], source);

                if (index > 0)
                {
                    AssertAppearsBefore(
                        source,
                        orderedCommands[index - 1],
                        orderedCommands[index],
                        "The README manual validation sequence should stay aligned with the checked-in validation runner order.");
                }
            }
        }

        private static void AssertAppearsBefore(string source, string first, string second, string because)
        {
            var firstIndex = source.IndexOf(first, StringComparison.Ordinal);
            var secondIndex = source.IndexOf(second, StringComparison.Ordinal);

            Assert.True(firstIndex >= 0, $"Expected to find '{first}'.");
            Assert.True(secondIndex >= 0, $"Expected to find '{second}'.");
            Assert.True(firstIndex < secondIndex, because);
        }

        private static void AssertAppearsBeforeAfter(string source, string anchor, string first, string second, string because)
        {
            var anchorIndex = source.IndexOf(anchor, StringComparison.Ordinal);
            Assert.True(anchorIndex >= 0, $"Expected to find '{anchor}'.");

            var firstIndex = source.IndexOf(first, anchorIndex, StringComparison.Ordinal);
            var secondIndex = source.IndexOf(second, anchorIndex, StringComparison.Ordinal);

            Assert.True(firstIndex >= 0, $"Expected to find '{first}' after '{anchor}'.");
            Assert.True(secondIndex >= 0, $"Expected to find '{second}' after '{anchor}'.");
            Assert.True(firstIndex < secondIndex, because);
        }

        private static string ReadRepoFile(params string[] parts)
        {
            var directory = AppContext.BaseDirectory;

            while (!string.IsNullOrEmpty(directory))
            {
                var candidate = Path.Combine(directory, Path.Combine(parts));
                if (File.Exists(candidate))
                    return File.ReadAllText(candidate);

                var parent = Directory.GetParent(directory);
                if (parent is null)
                    break;

                directory = parent.FullName;
            }

            throw new FileNotFoundException($"Could not find repository file: {Path.Combine(parts)}");
        }
    }
}