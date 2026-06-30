using System;
using System.IO;
using Xunit;

namespace InventoryManagementApp.Tests
{
    public class ValidationWorkflowCleanupContractTests
    {
        [Fact]
        public void BuildWorkflowClearsManifestArtifactGroupsBeforeFallibleValidationSteps()
        {
            var source = ReadRepoFile(".github", "workflows", "build.yml");

            Assert.Contains("if (Test-Path ./ValidationLogs) { Remove-Item ./ValidationLogs -Recurse -Force }", source);
            Assert.Contains("if (Test-Path ./TestResults) { Remove-Item ./TestResults -Recurse -Force }", source);
            Assert.Contains("if (Test-Path ./publish) { Remove-Item ./publish -Recurse -Force }", source);
            Assert.Contains("New-Item -ItemType Directory -Path ./ValidationLogs | Out-Null", source);
            AssertAppearsBefore(source, "if (Test-Path ./TestResults) { Remove-Item ./TestResults -Recurse -Force }", "Capture validation environment", "The workflow should clear stale test results before environment capture, restore, audit, or build can fail and write a manifest.");
            AssertAppearsBefore(source, "if (Test-Path ./publish) { Remove-Item ./publish -Recurse -Force }", "Capture validation environment", "The workflow should clear stale publish output before early failures can write a manifest.");
            AssertAppearsBefore(source, "if (Test-Path ./publish) { Remove-Item ./publish -Recurse -Force }", "Restore dependencies", "The workflow should clear stale publish output before restore can fail.");
            AssertAppearsBefore(source, "Prepare test results", "- name: Test", "The workflow should still recreate TestResults immediately before tests run.");
            AssertAppearsBefore(source, "Clean publish output", "- name: Publish", "The workflow should still clean publish output immediately before fresh publish artifacts are produced.");
        }

        [Fact]
        public void LocalAndCiValidationCleanupStayAligned()
        {
            var runner = ReadRepoFile("scripts", "run-full-validation.ps1");
            var workflow = ReadRepoFile(".github", "workflows", "build.yml");

            Assert.Contains("Remove-Item $testResultsPath -Recurse -Force", runner);
            Assert.Contains("Remove-Item $publishOutputPath -Recurse -Force", runner);
            Assert.Contains("if (Test-Path ./TestResults) { Remove-Item ./TestResults -Recurse -Force }", workflow);
            Assert.Contains("if (Test-Path ./publish) { Remove-Item ./publish -Recurse -Force }", workflow);
            AssertAppearsBefore(runner, "Remove-Item $testResultsPath -Recurse -Force", "Invoke-ValidationStep \"Capture validation environment\"", "The local runner should clear stale test results before early fallible steps.");
            AssertAppearsBefore(runner, "Remove-Item $publishOutputPath -Recurse -Force", "Invoke-ValidationStep \"Capture validation environment\"", "The local runner should clear stale publish output before early fallible full-validation steps.");
            AssertAppearsBefore(workflow, "if (Test-Path ./TestResults) { Remove-Item ./TestResults -Recurse -Force }", "Capture validation environment", "The workflow should match the local runner's early test-results cleanup.");
            AssertAppearsBefore(workflow, "if (Test-Path ./publish) { Remove-Item ./publish -Recurse -Force }", "Capture validation environment", "The workflow should match the local runner's early publish-output cleanup.");
        }

        private static void AssertAppearsBefore(string source, string first, string second, string because)
        {
            var firstIndex = source.IndexOf(first, StringComparison.Ordinal);
            var secondIndex = source.IndexOf(second, StringComparison.Ordinal);

            Assert.True(firstIndex >= 0, $"Expected to find '{first}'.");
            Assert.True(secondIndex >= 0, $"Expected to find '{second}'.");
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
