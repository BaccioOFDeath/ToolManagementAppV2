using System;
using System.IO;
using Xunit;

namespace InventoryManagementApp.Tests
{
    public class MobileCaptureSessionContractTests
    {
        [Fact]
        public void ItemsViewModel_DoesNotStopMobileCaptureWhenQrDialogCloses()
        {
            var source = ReadRepositoryFile("InventoryManagementApp", "ViewModels", "ItemsViewModel.cs");
            var method = ExtractMethod(source, "private async Task OpenMobileCaptureAsync");

            Assert.Contains("_mobileCaptureService.StartSessionAsync", method, StringComparison.Ordinal);
            Assert.DoesNotContain("_mobileCaptureService.StopAsync", method, StringComparison.Ordinal);
        }

        [Fact]
        public void MobileCaptureService_KeepsTokenAliveForFullWorkSession()
        {
            var source = ReadRepositoryFile("InventoryManagementApp", "Services", "MobileCapture", "MobileCaptureService.cs");

            Assert.Contains("SessionDuration = TimeSpan.FromHours(12)", source, StringComparison.Ordinal);
            Assert.Contains("_expiresAt = DateTime.Now.Add(SessionDuration);", source, StringComparison.Ordinal);
        }

        [Fact]
        public void MobileCaptureService_SubmitsAsSessionUserAfterDesktopLogout()
        {
            var source = ReadRepositoryFile("InventoryManagementApp", "Services", "MobileCapture", "MobileCaptureService.cs");

            Assert.Contains("_sessionUser = _services.GetService<IUserContext>()?.CurrentUser;", source, StringComparison.Ordinal);
            Assert.Contains("RunAsSessionUserAsync", source, StringComparison.Ordinal);
            Assert.Contains("context.CurrentUser = _sessionUser;", source, StringComparison.Ordinal);
            Assert.Contains("context.CurrentUser = previousUser;", source, StringComparison.Ordinal);
        }

        private static string ExtractMethod(string source, string signature)
        {
            var start = source.IndexOf(signature, StringComparison.Ordinal);
            Assert.True(start >= 0, $"Could not find method signature '{signature}'.");

            var brace = source.IndexOf('{', start);
            Assert.True(brace >= 0, $"Could not find method body for '{signature}'.");

            var depth = 0;
            for (var i = brace; i < source.Length; i++)
            {
                if (source[i] == '{')
                    depth++;
                else if (source[i] == '}')
                    depth--;

                if (depth == 0)
                    return source.Substring(start, i - start + 1);
            }

            throw new InvalidOperationException($"Could not parse method body for '{signature}'.");
        }

        private static string ReadRepositoryFile(params string[] path)
            => File.ReadAllText(Path.GetFullPath(Path.Combine(AppContext.BaseDirectory, "..", "..", "..", "..", Path.Combine(path))));
    }
}
