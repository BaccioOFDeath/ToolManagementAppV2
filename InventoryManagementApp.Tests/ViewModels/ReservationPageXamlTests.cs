using System;
using System.IO;
using Xunit;

namespace InventoryManagementApp.Tests.ViewModels
{
    public class ReservationPageXamlTests
    {
        [Fact]
        public void ReservationPage_UsesWorkbenchSummariesAndCommands()
        {
            var xaml = ReadRepositoryFile("InventoryManagementApp", "Views", "Pages", "ReservationPage.xaml");

            Assert.Contains("Reservations Workbench", xaml, StringComparison.Ordinal);
            Assert.Contains("ReservationResultsSummary", xaml, StringComparison.Ordinal);
            Assert.Contains("SelectedReservationSummary", xaml, StringComparison.Ordinal);
            Assert.Contains("SelectedReservationTitle", xaml, StringComparison.Ordinal);
            Assert.Contains("SelectedReservationNextAction", xaml, StringComparison.Ordinal);
            Assert.Contains("SelectedReservationShelfChecklist", xaml, StringComparison.Ordinal);
            Assert.Contains("AddReservationCommand", xaml, StringComparison.Ordinal);
            Assert.Contains("ConfirmReservationCommand", xaml, StringComparison.Ordinal);
            Assert.Contains("FulfillReservationCommand", xaml, StringComparison.Ordinal);
            Assert.Contains("PrintReservationDirectoryCommand", xaml, StringComparison.Ordinal);
            Assert.Contains("ReservationRow_MouseDoubleClick", xaml, StringComparison.Ordinal);
            Assert.Contains("ReservationRow_PreviewMouseRightButtonDown", xaml, StringComparison.Ordinal);
        }

        [Fact]
        public void ReservationPage_HasStyledDirectoryAndHandoffSections()
        {
            var xaml = ReadRepositoryFile("InventoryManagementApp", "Views", "Pages", "ReservationPage.xaml");

            Assert.Contains("ReservationStatCard", xaml, StringComparison.Ordinal);
            Assert.Contains("Pickup Handoff", xaml, StringComparison.Ordinal);
            Assert.Contains("No reservations match this filter", xaml, StringComparison.Ordinal);
            Assert.Contains("FilteredReservations.Count", xaml, StringComparison.Ordinal);
            Assert.Contains("DesktopNoteCard", xaml, StringComparison.Ordinal);
        }

        static string ReadRepositoryFile(params string[] relativePathParts)
        {
            var directory = new DirectoryInfo(AppContext.BaseDirectory);
            while (directory != null && !File.Exists(Path.Combine(directory.FullName, "InventoryManagementApp.sln")))
                directory = directory.Parent;

            Assert.NotNull(directory);
            var path = Path.Combine(directory!.FullName, Path.Combine(relativePathParts));
            Assert.True(File.Exists(path), $"Expected repository file at {path}");
            return File.ReadAllText(path);
        }
    }
}
