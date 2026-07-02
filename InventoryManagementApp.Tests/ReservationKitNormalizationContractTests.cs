using System;
using System.IO;
using Xunit;

namespace InventoryManagementApp.Tests
{
    public class ReservationKitNormalizationContractTests
    {
        [Fact]
        public void ReservationCreateNormalizesTextBeforeReferenceChecksAndInsert()
        {
            var source = ReadRepoFile("InventoryManagementApp", "Services", "Reservations", "ReservationService.cs");
            var method = ExtractMethod(
                source,
                "public async Task<int> CreateReservationAsync",
                "public async Task<bool> UpdateReservationAsync");

            Assert.Contains("NormalizeReservationForSave(reservation);", method, StringComparison.Ordinal);
            Assert.True(
                method.IndexOf("NormalizeReservationForSave(reservation);", StringComparison.Ordinal) < method.IndexOf("EnsureReservationReferencesExist(conn, reservation);", StringComparison.Ordinal),
                "Reservation creation should normalize user-entered status and notes before reference checks and insert work.");
            Assert.Contains("cmd.Parameters.AddWithValue(\"@Status\", reservation.Status);", method, StringComparison.Ordinal);
            Assert.Contains("cmd.Parameters.AddWithValue(\"@Notes\", ToDbNullableText(reservation.Notes));", method, StringComparison.Ordinal);
            Assert.DoesNotContain("cmd.Parameters.AddWithValue(\"@Status\", NormalizeStatus(reservation.Status));", method, StringComparison.Ordinal);
        }

        [Fact]
        public void ReservationUpdateNormalizesTextBeforeReferenceChecksAndUpdate()
        {
            var source = ReadRepoFile("InventoryManagementApp", "Services", "Reservations", "ReservationService.cs");
            var method = ExtractMethod(
                source,
                "public async Task<bool> UpdateReservationAsync",
                "public async Task<bool> ConfirmReservationAsync");

            Assert.Contains("NormalizeReservationForSave(reservation);", method, StringComparison.Ordinal);
            Assert.True(
                method.IndexOf("NormalizeReservationForSave(reservation);", StringComparison.Ordinal) < method.IndexOf("EnsureReservationExists(conn, reservation.ReservationID);", StringComparison.Ordinal),
                "Reservation updates should normalize workflow text before existing-row and reference checks.");
            Assert.Contains("cmd.Parameters.AddWithValue(\"@Status\", reservation.Status);", method, StringComparison.Ordinal);
            Assert.Contains("cmd.Parameters.AddWithValue(\"@Notes\", ToDbNullableText(reservation.Notes));", method, StringComparison.Ordinal);
            Assert.DoesNotContain("cmd.Parameters.AddWithValue(\"@Status\", NormalizeStatus(reservation.Status));", method, StringComparison.Ordinal);
        }

        [Fact]
        public void ReservationNormalizerCoversPersistedWorkflowTextFields()
        {
            var source = ReadRepoFile("InventoryManagementApp", "Services", "Reservations", "ReservationService.cs");
            var normalizer = ExtractMethod(
                source,
                "private static void NormalizeReservationForSave",
                "private static void EnsureReservationExists");

            Assert.Contains("reservation.Status = NormalizeStatus(reservation.Status);", normalizer, StringComparison.Ordinal);
            Assert.Contains("reservation.Notes = NormalizeOptionalText(reservation.Notes);", normalizer, StringComparison.Ordinal);
            Assert.Contains("private static string NormalizeOptionalText(string? value) => value?.Trim() ?? string.Empty;", source, StringComparison.Ordinal);
            Assert.Contains("var normalizedStatus = string.IsNullOrWhiteSpace(status) ? \"Pending\" : status.Trim();", source, StringComparison.Ordinal);
        }

        [Fact]
        public void KitCreateNormalizesTextBeforeInsert()
        {
            var source = ReadRepoFile("InventoryManagementApp", "Services", "Kits", "KitService.cs");
            var method = ExtractMethod(
                source,
                "public async Task<int> CreateKitAsync",
                "public async Task<bool> UpdateKitAsync");

            Assert.Contains("NormalizeKitForSave(kit);", method, StringComparison.Ordinal);
            Assert.True(
                method.IndexOf("NormalizeKitForSave(kit);", StringComparison.Ordinal) < method.IndexOf("cmd.Parameters.AddWithValue(\"@KitNumber\", kit.KitNumber);", StringComparison.Ordinal),
                "Kit creation should normalize kit text before insert parameters are bound.");
            Assert.Contains("cmd.Parameters.AddWithValue(\"@KitNumber\", kit.KitNumber);", method, StringComparison.Ordinal);
            Assert.Contains("cmd.Parameters.AddWithValue(\"@Name\", kit.Name);", method, StringComparison.Ordinal);
            Assert.DoesNotContain("cmd.Parameters.AddWithValue(\"@KitNumber\", kit.KitNumber.Trim());", method, StringComparison.Ordinal);
            Assert.DoesNotContain("cmd.Parameters.AddWithValue(\"@Name\", kit.Name.Trim());", method, StringComparison.Ordinal);
        }

        [Fact]
        public void KitUpdateNormalizesTextBeforeExistingRowCheckAndUpdate()
        {
            var source = ReadRepoFile("InventoryManagementApp", "Services", "Kits", "KitService.cs");
            var method = ExtractMethod(
                source,
                "public async Task<bool> UpdateKitAsync",
                "public async Task<bool> DeleteKitAsync");

            Assert.Contains("NormalizeKitForSave(kit);", method, StringComparison.Ordinal);
            Assert.True(
                method.IndexOf("NormalizeKitForSave(kit);", StringComparison.Ordinal) < method.IndexOf("EnsureKitExists(conn, kit.KitID);", StringComparison.Ordinal),
                "Kit updates should normalize user-entered kit text before existing-row checks and update work.");
            Assert.Contains("cmd.Parameters.AddWithValue(\"@KitNumber\", kit.KitNumber);", method, StringComparison.Ordinal);
            Assert.Contains("cmd.Parameters.AddWithValue(\"@Name\", kit.Name);", method, StringComparison.Ordinal);
            Assert.DoesNotContain("cmd.Parameters.AddWithValue(\"@KitNumber\", kit.KitNumber.Trim());", method, StringComparison.Ordinal);
            Assert.DoesNotContain("cmd.Parameters.AddWithValue(\"@Name\", kit.Name.Trim());", method, StringComparison.Ordinal);
        }

        [Fact]
        public void KitNormalizerCoversPersistedWorkflowTextFields()
        {
            var source = ReadRepoFile("InventoryManagementApp", "Services", "Kits", "KitService.cs");
            var normalizer = ExtractMethod(
                source,
                "private static void NormalizeKitForSave",
                "private static void ValidateKitItem");

            Assert.Contains("kit.KitNumber = NormalizeRequiredText(kit.KitNumber);", normalizer, StringComparison.Ordinal);
            Assert.Contains("kit.Name = NormalizeRequiredText(kit.Name);", normalizer, StringComparison.Ordinal);
            Assert.Contains("kit.Description = NormalizeOptionalText(kit.Description);", normalizer, StringComparison.Ordinal);
            Assert.Contains("kit.Category = NormalizeOptionalText(kit.Category);", normalizer, StringComparison.Ordinal);
            Assert.Contains("private static string NormalizeRequiredText(string? value) => value?.Trim() ?? string.Empty;", source, StringComparison.Ordinal);
            Assert.Contains("private static string NormalizeOptionalText(string? value) => value?.Trim() ?? string.Empty;", source, StringComparison.Ordinal);
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
                    return NormalizeLineEndings(File.ReadAllText(candidate));

                var parent = Directory.GetParent(directory);
                if (parent is null)
                    break;

                directory = parent.FullName;
            }

            throw new FileNotFoundException($"Could not find repository file: {Path.Combine(parts)}");
        }

        private static string NormalizeLineEndings(string text) =>
            text.Replace("\r\n", "\n", StringComparison.Ordinal).Replace("\r", "\n", StringComparison.Ordinal);
    }
}
