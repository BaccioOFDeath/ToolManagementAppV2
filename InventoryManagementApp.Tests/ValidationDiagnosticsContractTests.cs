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
        public void FullValidationRunnerCapturesPackageAuditDiagnosticsBeforeBuild()
        {
            var source = ReadRepoFile("scripts", "run-full-validation.ps1");

            Assert.Contains("Audit vulnerable packages", source);
            Assert.Contains("Get-ValidationLogPath \"package-audit.txt\"", source);
            Assert.Contains("dotnet list InventoryManagementApp.sln package --vulnerable --include-transitive 2>&1 | Tee-Object -FilePath $auditLogPath", source);
            Assert.Contains("$global:LASTEXITCODE = $LASTEXITCODE", source);
            AssertAppearsBefore(source, "Clean validation logs", "Audit vulnerable packages", "The package audit log should be written into a freshly prepared validation log directory.");
            AssertAppearsBefore(source, "Restore solution", "Audit vulnerable packages", "The package audit should run after restore resolves the solution graph.");
            AssertAppearsBefore(source, "Audit vulnerable packages", "Build solution", "Package advisory evidence should be captured before build can fail.");
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
        public void BuildWorkflowCapturesPackageAuditDiagnosticsBeforeBuild()
        {
            var source = ReadRepoFile(".github", "workflows", "build.yml");

            Assert.Contains("Audit vulnerable packages", source);
            Assert.Contains("shell: pwsh", source);
            Assert.Contains("./ValidationLogs/package-audit.txt", source);
            Assert.Contains("dotnet list InventoryManagementApp.sln package --vulnerable --include-transitive 2>&1 | Tee-Object -FilePath $auditLogPath", source);
            Assert.Contains("if ($LASTEXITCODE -ne 0) { exit $LASTEXITCODE }", source);
            AssertAppearsBefore(source, "Prepare validation logs", "Audit vulnerable packages", "The workflow should write the package audit log into a freshly prepared diagnostics directory.");
            AssertAppearsBefore(source, "Restore dependencies", "Audit vulnerable packages", "CI package audit should run after restore resolves the solution graph.");
            AssertAppearsBefore(source, "Audit vulnerable packages", "Build", "Package advisory evidence should be captured before build can fail.");
            AssertAppearsBefore(source, "Audit vulnerable packages", "Upload validation logs", "The package audit file should be included in the validation log artifact.");
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
