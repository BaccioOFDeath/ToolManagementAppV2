using System;
using System.IO;
using Xunit;

namespace InventoryManagementApp.Tests
{
    public class RentalReminderWorkflowContractTests
    {
        [Fact]
        public void ReminderRunUsesDueDateQueryInsteadOfMaterializingAllActiveRentals()
        {
            var reminderSource = ReadRepoFile("InventoryManagementApp", "Services", "Notifications", "RentalReminderService.cs");
            var rentalInterfaceSource = ReadRepoFile("InventoryManagementApp", "Interfaces", "IRentalService.cs");
            var rentalServiceSource = ReadRepoFile("InventoryManagementApp", "Services", "Rentals", "RentalService.cs");
            var reminderMethod = ExtractMethod(
                reminderSource,
                "public async Task CheckAndSendRemindersAsync()",
                "private Task<string> GetEmailSignatureAsync()");
            var dueDateQuery = ExtractMethod(
                rentalServiceSource,
                "public async Task<List<Rental>> GetActiveRentalsDueOnAsync(DateTime dueDate)",
                "public async Task<List<Rental>> GetOverdueRentalsAsync()");

            Assert.Contains("Task<List<Rental>> GetActiveRentalsDueOnAsync(DateTime dueDate);", rentalInterfaceSource, StringComparison.Ordinal);
            Assert.Contains("var tomorrow = DateTime.Today.AddDays(1);", reminderMethod, StringComparison.Ordinal);
            Assert.Contains("var rentalsDueTomorrowTask = _rentalService.GetActiveRentalsDueOnAsync(tomorrow);", reminderMethod, StringComparison.Ordinal);
            Assert.DoesNotContain("GetActiveRentalsAsync()", reminderMethod, StringComparison.Ordinal);
            Assert.DoesNotContain(".Where(r => r.DueDate.Date == tomorrow)", reminderMethod, StringComparison.Ordinal);

            AssertContainsAll(
                dueDateQuery,
                "WHERE r.Status='Rented'",
                "AND date(r.DueDate) = date(@DueDate)",
                "ORDER BY r.DueDate ASC, r.RentalID ASC",
                "LIMIT @RentalListLimit",
                "new SqliteParameter(\"@DueDate\", dueDate.Date)",
                "new SqliteParameter(\"@RentalListLimit\", MaxRentalListCount)");
        }

        [Fact]
        public void ReminderRunPreventsOverlappingTimerAndManualExecutions()
        {
            var source = ReadRepoFile("InventoryManagementApp", "Services", "Notifications", "RentalReminderService.cs");
            var method = ExtractMethod(source, "public async Task CheckAndSendRemindersAsync()", "private Task<string> GetEmailSignatureAsync()");

            Assert.Contains("private readonly SemaphoreSlim _checkLock = new(1, 1);", source, StringComparison.Ordinal);
            Assert.Contains("if (!await _checkLock.WaitAsync(0).ConfigureAwait(false))", method, StringComparison.Ordinal);
            Assert.Contains("Rental reminder check is already running. Skipping overlapping run.", method, StringComparison.Ordinal);
            Assert.Contains("finally", method, StringComparison.Ordinal);
            Assert.Contains("_checkLock.Release();", method, StringComparison.Ordinal);
            Assert.Contains("_checkLock.Dispose();", source, StringComparison.Ordinal);
        }

        [Fact]
        public void ReminderRunLoadsSettingsAndTemplatesConcurrentlyBeforeSending()
        {
            var source = ReadRepoFile("InventoryManagementApp", "Services", "Notifications", "RentalReminderService.cs");
            var method = ExtractMethod(source, "public async Task CheckAndSendRemindersAsync()", "private Task<string> GetEmailSignatureAsync()");

            AssertContainsAll(
                method,
                "var rentalsDueTomorrowTask = _rentalService.GetActiveRentalsDueOnAsync(tomorrow);",
                "var emailSignatureTask = GetEmailSignatureAsync();",
                "var reminderSubjectTemplateTask = GetReminderSubjectTemplateAsync();",
                "var reminderBodyTemplateTask = GetReminderBodyTemplateAsync();",
                "var companyNameTask = GetCompanyNameAsync();",
                "var logoPathTask = GetCompanyLogoPathAsync();",
                "await Task.WhenAll(",
                "rentalsDueTomorrowTask,",
                "emailSignatureTask,",
                "reminderSubjectTemplateTask,",
                "reminderBodyTemplateTask,",
                "companyNameTask,",
                "logoPathTask).ConfigureAwait(false);");

            Assert.True(
                method.IndexOf("var rentalsDueTomorrowTask = _rentalService.GetActiveRentalsDueOnAsync(tomorrow);", StringComparison.Ordinal) <
                method.IndexOf("await Task.WhenAll(", StringComparison.Ordinal),
                "Expected reminder workflow to start rental/config reads before awaiting them.");
            Assert.True(
                method.IndexOf("await Task.WhenAll(", StringComparison.Ordinal) <
                method.IndexOf("foreach (var rental in rentalsDueTomorrow)", StringComparison.Ordinal),
                "Expected reminder workflow to finish shared reads before sending emails.");
        }

        [Fact]
        public void ReminderRunReportsHonestSentSkippedAndFailedCounts()
        {
            var source = ReadRepoFile("InventoryManagementApp", "Services", "Notifications", "RentalReminderService.cs");
            var method = ExtractMethod(source, "public async Task CheckAndSendRemindersAsync()", "private Task<string> GetEmailSignatureAsync()");

            AssertContainsAll(
                method,
                "var sentCount = 0;",
                "var skippedCount = 0;",
                "var failedCount = 0;",
                "skippedCount++;",
                "sentCount++;",
                "failedCount++;",
                "Completed rental reminder run. Due: {DueCount}, Sent: {SentCount}, Skipped: {SkippedCount}, Failed: {FailedCount}",
                "rentalsDueTomorrow.Count,",
                "sentCount,",
                "skippedCount,",
                "failedCount");

            Assert.DoesNotContain("Completed sending {Count} rental reminders", method, StringComparison.Ordinal);
        }

        [Fact]
        public void ReminderStartReplacesExistingTimerBeforeSchedulingNextRun()
        {
            var source = ReadRepoFile("InventoryManagementApp", "Services", "Notifications", "RentalReminderService.cs");
            var startMethod = ExtractMethod(source, "public void Start()", "public void Stop()");

            Assert.Contains("Stop();", startMethod, StringComparison.Ordinal);
            Assert.True(
                startMethod.IndexOf("Stop();", StringComparison.Ordinal) <
                startMethod.IndexOf("_timer = new System.Threading.Timer(", StringComparison.Ordinal),
                "Expected repeated Start calls to dispose the old timer before scheduling a replacement.");
        }

        private static void AssertContainsAll(string source, params string[] expectedSnippets)
        {
            foreach (var snippet in expectedSnippets)
            {
                Assert.Contains(snippet, source, StringComparison.Ordinal);
            }
        }

        private static string ExtractMethod(string source, string startMarker, string endMarker)
        {
            var start = source.IndexOf(startMarker, StringComparison.Ordinal);
            Assert.True(start >= 0, $"Could not find method start marker: {startMarker}");

            var end = source.IndexOf(endMarker, start, StringComparison.Ordinal);
            Assert.True(end > start, $"Could not find method end marker: {endMarker}");

            return source[start..end];
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
