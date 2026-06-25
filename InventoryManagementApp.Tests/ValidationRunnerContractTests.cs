using System;
using System.IO;
using Xunit;

namespace InventoryManagementApp.Tests
{
    public class ValidationRunnerContractTests
    {
        [Fact]
        public void FullValidationRunnerAuditsVulnerablePackagesAfterRestoreBeforeBuild()
        {
            var source = ReadRepoFile("scripts", "run-full-validation.ps1");

            Assert.Contains("Audit vulnerable packages", source);
            Assert.Contains("dotnet list InventoryManagementApp.sln package --vulnerable --include-transitive", source);
            AssertAppearsBefore(source, "Restore solution", "Audit vulnerable packages", "The full validation runner should audit packages immediately after restore.");
            AssertAppearsBefore(source, "Audit vulnerable packages", "Build solution", "The full validation runner should audit packages before build/test work continues.");
            AssertAppearsBefore(source, "dotnet restore InventoryManagementApp.sln", "dotnet list InventoryManagementApp.sln package --vulnerable --include-transitive", "The audit command should run after solution restore.");
            AssertAppearsBefore(source, "dotnet list InventoryManagementApp.sln package --vulnerable --include-transitive", "dotnet build InventoryManagementApp.sln --configuration $Configuration --no-restore", "The audit command should run before the no-restore build.");
        }

        [Fact]
        public void BuildWorkflowAuditsVulnerablePackagesAfterRestoreBeforeBuild()
        {
            var source = ReadRepoFile(".github", "workflows", "build.yml");

            Assert.Contains("Audit vulnerable packages", source);
            Assert.Contains("dotnet list InventoryManagementApp.sln package --vulnerable --include-transitive", source);
            AssertAppearsBefore(source, "Restore dependencies", "Audit vulnerable packages", "The Build and Test workflow should audit packages immediately after restore.");
            AssertAppearsBefore(source, "Audit vulnerable packages", "- name: Build", "The Build and Test workflow should audit packages before build/test work continues.");
            AssertAppearsBefore(source, "dotnet restore InventoryManagementApp.sln", "dotnet list InventoryManagementApp.sln package --vulnerable --include-transitive", "The workflow audit command should run after solution restore.");
            AssertAppearsBefore(source, "dotnet list InventoryManagementApp.sln package --vulnerable --include-transitive", "dotnet build InventoryManagementApp.sln --configuration Release --no-restore", "The workflow audit command should run before the no-restore build.");
        }

        [Fact]
        public void BuildWorkflowRunsOnMasterAndMainPushesAndPullRequests()
        {
            var source = ReadRepoFile(".github", "workflows", "build.yml");
            var pushTrigger = ExtractIndentedBlock(source, "  push:");
            var pullRequestTrigger = ExtractIndentedBlock(source, "  pull_request:");

            Assert.Contains("branches: [ master, main ]", pushTrigger);
            Assert.Contains("branches: [ master, main ]", pullRequestTrigger);
            Assert.Contains("  workflow_dispatch:", source);
            AssertAppearsBefore(source, "  push:", "jobs:", "The Build and Test workflow should keep push validation enabled before job definitions.");
            AssertAppearsBefore(source, "  pull_request:", "jobs:", "The Build and Test workflow should keep pull-request validation enabled before job definitions.");
            AssertAppearsBefore(source, "  workflow_dispatch:", "jobs:", "The Build and Test workflow should keep manual dispatch available before job definitions.");
        }

        [Fact]
        public void FullValidationRunnerCleansPublishOutputBeforePublishing()
        {
            var source = ReadRepoFile("scripts", "run-full-validation.ps1");

            Assert.Contains("$publishOutputPath = Join-Path $repoRoot \"publish\"", source);
            Assert.Contains("Clean publish output", source);
            Assert.Contains("Test-Path $publishOutputPath", source);
            Assert.Contains("Remove-Item $publishOutputPath -Recurse -Force", source);

            var cleanIndex = source.IndexOf("Clean publish output", StringComparison.Ordinal);
            var publishIndex = source.IndexOf("dotnet publish InventoryManagementApp/InventoryManagementApp.csproj", StringComparison.Ordinal);

            Assert.True(cleanIndex >= 0, "The full validation runner should name the publish-output cleanup step.");
            Assert.True(publishIndex >= 0, "The full validation runner should publish the app.");
            Assert.True(cleanIndex < publishIndex, "The full validation runner should clean stale publish output before publishing fresh artifacts.");
        }

        [Fact]
        public void FullValidationRunnerSkipPublishStopsAfterCompileAndTestValidation()
        {
            var source = ReadRepoFile("scripts", "run-full-validation.ps1");
            var publishValidationBlock = ExtractBracedBlock(source, "if (-not $SkipPublish)");

            Assert.Contains("Restore publish runtime", publishValidationBlock);
            Assert.Contains("Clean publish output", publishValidationBlock);
            Assert.Contains("Publish app", publishValidationBlock);
            Assert.Contains("Check banned words", publishValidationBlock);
            Assert.Contains("Check banned words PowerShell fallback", publishValidationBlock);
            AssertAppearsBefore(source, "Test solution", "if (-not $SkipPublish)", "The SkipPublish path should complete after restore, audit, build, and test validation.");
        }

        [Fact]
        public void FullValidationRunnerResetsExternalExitCodeForEachStep()
        {
            var source = ReadRepoFile("scripts", "run-full-validation.ps1");

            Assert.Contains("$global:LASTEXITCODE = 0", source);
            Assert.Contains("$exitCode = $global:LASTEXITCODE", source);
            Assert.Contains("if ($null -ne $exitCode -and $exitCode -ne 0)", source);
            Assert.Contains("throw \"$Name failed with exit code $exitCode.\"", source);
            Assert.DoesNotContain("if ($LASTEXITCODE -ne 0)", source);
            AssertAppearsBefore(source, "$global:LASTEXITCODE = 0", "& $Action", "The validation runner should clear stale external exit codes before each named step runs.");
            AssertAppearsBefore(source, "& $Action", "$exitCode = $global:LASTEXITCODE", "The validation runner should capture the exit code produced by the current step.");
        }

        [Fact]
        public void BuildWorkflowCleansPublishOutputBeforePublishing()
        {
            var source = ReadRepoFile(".github", "workflows", "build.yml");

            Assert.Contains("Clean publish output", source);
            Assert.Contains("shell: pwsh", source);
            Assert.Contains("if (Test-Path ./publish) { Remove-Item ./publish -Recurse -Force }", source);

            var cleanIndex = source.IndexOf("Clean publish output", StringComparison.Ordinal);
            var publishIndex = source.IndexOf("dotnet publish InventoryManagementApp/InventoryManagementApp.csproj", StringComparison.Ordinal);

            Assert.True(cleanIndex >= 0, "The Build and Test workflow should name the publish-output cleanup step.");
            Assert.True(publishIndex >= 0, "The Build and Test workflow should publish the app.");
            Assert.True(cleanIndex < publishIndex, "The Build and Test workflow should clean stale publish output before publishing fresh artifacts.");
        }

        [Fact]
        public void BuildWorkflowRunsBannedWordChecksAfterPublishingBeforeUpload()
        {
            var source = ReadRepoFile(".github", "workflows", "build.yml");

            Assert.Contains("Check banned words", source);
            Assert.Contains("Check banned words PowerShell fallback", source);
            Assert.Contains("BANNED_WORD_CHECK_FORCE_POWERSHELL=1 bash scripts/check-banned-words.sh", source);
            AssertAppearsBefore(source, "dotnet publish InventoryManagementApp/InventoryManagementApp.csproj", "bash scripts/check-banned-words.sh", "The Build and Test workflow should mirror the full validation runner by scanning source after publish completes.");
            AssertAppearsBefore(source, "bash scripts/check-banned-words.sh", "BANNED_WORD_CHECK_FORCE_POWERSHELL=1 bash scripts/check-banned-words.sh", "The normal banned-word path should run before the forced PowerShell fallback path.");
            AssertAppearsBefore(source, "BANNED_WORD_CHECK_FORCE_POWERSHELL=1 bash scripts/check-banned-words.sh", "Upload build artifacts", "Both banned-word paths should pass before the workflow uploads generated artifacts.");
        }

        private static void AssertAppearsBefore(string source, string first, string second, string because)
        {
            var firstIndex = source.IndexOf(first, StringComparison.Ordinal);
            var secondIndex = source.IndexOf(second, StringComparison.Ordinal);

            Assert.True(firstIndex >= 0, $"Expected to find '{first}'.");
            Assert.True(secondIndex >= 0, $"Expected to find '{second}'.");
            Assert.True(firstIndex < secondIndex, because);
        }

        private static string ExtractBracedBlock(string source, string marker)
        {
            var markerIndex = source.IndexOf(marker, StringComparison.Ordinal);
            Assert.True(markerIndex >= 0, $"Expected to find '{marker}'.");

            var openIndex = source.IndexOf('{', markerIndex);
            Assert.True(openIndex >= 0, $"Expected '{marker}' to start a braced block.");

            var depth = 0;
            for (var index = openIndex; index < source.Length; index++)
            {
                if (source[index] == '{')
                    depth++;
                else if (source[index] == '}')
                    depth--;

                if (depth == 0)
                    return source.Substring(openIndex + 1, index - openIndex - 1);
            }

            throw new InvalidOperationException($"Could not find the end of the '{marker}' block.");
        }

        private static string ExtractIndentedBlock(string source, string marker)
        {
            var markerIndex = source.IndexOf(marker, StringComparison.Ordinal);
            Assert.True(markerIndex >= 0, $"Expected to find '{marker}'.");

            var nextPeerIndex = source.IndexOf("\n  ", markerIndex + marker.Length, StringComparison.Ordinal);
            if (nextPeerIndex < 0)
                nextPeerIndex = source.Length;

            return source.Substring(markerIndex, nextPeerIndex - markerIndex);
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