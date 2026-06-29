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

        [Fact]
        public void MobileCaptureService_SavesNewItemPhotoUnderItemNumberFileName()
        {
            var source = ReadRepositoryFile("InventoryManagementApp", "Services", "MobileCapture", "MobileCaptureService.cs");
            var submitMethod = ExtractMethod(source, "private async Task<IResult> SubmitItemAsync");
            var saveMethod = ExtractMethod(source, "private async Task<string> SaveUploadAsync");

            Assert.Contains("item.ItemNumber = await itemService.GenerateNextItemNumberAsync", submitMethod, StringComparison.Ordinal);
            Assert.Contains("SaveUploadAsync(image, \"ItemImages\", item.ItemNumber, useExactName: true", submitMethod, StringComparison.Ordinal);
            Assert.Contains("useExactName", saveMethod, StringComparison.Ordinal);
            Assert.Contains("? $\"{safeSeed}{extension.ToLowerInvariant()}\"", saveMethod, StringComparison.Ordinal);
            Assert.Contains("useExactName ? FileMode.Create : FileMode.CreateNew", saveMethod, StringComparison.Ordinal);
        }

        [Fact]
        public void MobileCaptureService_CanUpdateExistingItemPhotoByItemNumber()
        {
            var source = ReadRepositoryFile("InventoryManagementApp", "Services", "MobileCapture", "MobileCaptureService.cs");
            var submitMethod = ExtractMethod(source, "private async Task<IResult> SubmitItemImageAsync");

            Assert.Contains("app.MapPost(\"/mobile-capture/item-image\", SubmitItemImageAsync);", source, StringComparison.Ordinal);
            Assert.Contains("action=\"/mobile-capture/item-image\"", source, StringComparison.Ordinal);
            Assert.Contains("name=\"updateItemNumber\"", source, StringComparison.Ordinal);
            Assert.Contains("name=\"existingItemPhoto\"", source, StringComparison.Ordinal);
            Assert.Contains("FindItemByNumberAsync(itemNumber", submitMethod, StringComparison.Ordinal);
            Assert.Contains("SaveUploadAsync(photo, \"ItemImages\", item.ItemNumber, useExactName: true", submitMethod, StringComparison.Ordinal);
            Assert.Contains("itemService.UpdateItemImageAsync(item.ItemID, imagePath", submitMethod, StringComparison.Ordinal);
            Assert.Contains("Item photo updated", submitMethod, StringComparison.Ordinal);
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
