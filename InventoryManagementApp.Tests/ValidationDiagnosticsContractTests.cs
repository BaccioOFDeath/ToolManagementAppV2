using System;
using System.IO;
using Xunit;

namespace InventoryManagementApp.Tests
{
    public class ValidationDiagnosticsContractTests
    {
        [Fact]
        public void FullValidationRunnerCapturesEnvironmentDiagnosticsBeforeRestore()
        {
            var source = ReadRepoFile("scripts", "run-full-validation.ps1");

            Assert.Contains("Capture validation environment", source);
            Assert.Contains("Get-ValidationLogPath \"environment.txt\"", source);
            Assert.Contains("GeneratedAtUtc", source);
            Assert.Contains("RepositoryRoot=$repoRoot", source);
            Assert.Contains("Configuration=$Configuration", source);
            Assert.Contains("Runtime=$Runtime", source);
            Assert.Contains("SkipPublish=$SkipPublish", source);
            Assert.Contains("PowerShellVersion=$($PSVersionTable.PSVersion)", source);
            Assert.Contains("dotnet --info:", source);
            Assert.Contains("dotnet --info | Out-File -FilePath $environmentLogPath -Append -Encoding UTF8", source);
            AssertAppearsBefore(source, "Clean validation logs", "Capture validation environment", "The full validation runner should create a fresh diagnostics directory before writing environment details.");
            AssertAppearsBefore(source, "Capture validation environment", "Restore solution", "Environment diagnostics should be captured before restore can fail.");
        }

        [Fact]
        public void BuildWorkflowCapturesEnvironmentDiagnosticsBeforeRestore()
        {
            var source = ReadRepoFile(".github", "workflows", "build.yml");

            Assert.Contains("Capture validation environment", source);
            Assert.Contains("./ValidationLogs/environment.txt", source);
            Assert.Contains("GeneratedAtUtc", source);
            Assert.Contains("GitHubSha=${{ github.sha }}", source);
            Assert.Contains("GitHubRef=${{ github.ref }}", source);
            Assert.Contains("RunnerOS=${{ runner.os }}", source);
            Assert.Contains("Configuration=Release", source);
            Assert.Contains("Runtime=win-x64", source);
            Assert.Contains("PowerShellVersion=$($PSVersionTable.PSVersion)", source);
            Assert.Contains("dotnet --info:", source);
            Assert.Contains("dotnet --info | Out-File -FilePath $environmentLogPath -Append -Encoding UTF8", source);
            AssertAppearsBefore(source, "Prepare validation logs", "Capture validation environment", "The workflow should create a fresh diagnostics directory before writing environment details.");
            AssertAppearsBefore(source, "Capture validation environment", "Restore dependencies", "CI environment diagnostics should be captured before restore can fail.");
            AssertAppearsBefore(source, "Capture validation environment", "Upload validation logs", "The environment diagnostics file should be included in the validation log artifact.");
        }

        [Fact]
        public void GitIgnoreExcludesGeneratedValidationDiagnostics()
        {
            var source = ReadRepoFile(".gitignore");

            Assert.Contains("ValidationLogs/", source);
            Assert.Contains("*.binlog", source);
            Assert.Contains("[Tt]est[Rr]esult*/", source);
            Assert.Contains("publish/", source);
            AssertAppearsBefore(source, "ValidationLogs/", "# Visual Studio 2015/2017 cache/options directory", "The generated validation diagnostics directory should be grouped with other build output ignores.");
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
