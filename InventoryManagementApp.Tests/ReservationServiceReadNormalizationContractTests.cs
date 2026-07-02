using System;
using System.IO;
using Xunit;

namespace InventoryManagementApp.Tests
{
    public class ReservationServiceReadNormalizationContractTests
    {
        [Fact]
        public void ReservationMapperNormalizesAllDisplayTextFields()
        {
            var source = ReadRepoFile("InventoryManagementApp", "Services", "Reservations", "ReservationService.cs");
            var mapReservation = ExtractMethod(source, "private Reservation MapReservation(SqliteDataReader reader)", "private static string NormalizeReservationReadText");

            AssertContainsAll(
                mapReservation,
                "ItemNumber = NormalizeReservationReadText(reader, \"ItemNumber\")",
                "ItemName = NormalizeReservationReadText(reader, \"ItemName\")",
                "CustomerName = NormalizeReservationReadText(reader, \"CustomerName\")",
                "ImagePath = NormalizeReservationReadText(reader, \"ImagePath\")",
                "Status = NormalizeReservationReadText(reader, \"Status\")",
                "Notes = NormalizeReservationReadText(reader, \"Notes\")");

            Assert.DoesNotContain("reader.GetString(reader.GetOrdinal(\"ItemNumber\"))", mapReservation, StringComparison.Ordinal);
            Assert.DoesNotContain("reader.GetString(reader.GetOrdinal(\"ItemName\"))", mapReservation, StringComparison.Ordinal);
            Assert.DoesNotContain("reader.GetString(reader.GetOrdinal(\"CustomerName\"))", mapReservation, StringComparison.Ordinal);
            Assert.DoesNotContain("reader.GetString(reader.GetOrdinal(\"ImagePath\"))", mapReservation, StringComparison.Ordinal);
            Assert.DoesNotContain("Status = reader.GetString", mapReservation, StringComparison.Ordinal);
            Assert.DoesNotContain("Notes = reader.IsDBNull", mapReservation, StringComparison.Ordinal);
        }

        [Fact]
        public void ReservationReadNormalizerTrimsValuesAndPreservesEmptyFallback()
        {
            var source = ReadRepoFile("InventoryManagementApp", "Services", "Reservations", "ReservationService.cs");
            var normalizer = ExtractMethod(source, "private static string NormalizeReservationReadText(SqliteDataReader reader, string columnName)", "private static void ValidateReservation");

            AssertContainsAll(
                normalizer,
                "var ordinal = reader.GetOrdinal(columnName);",
                "return reader.IsDBNull(ordinal) ? string.Empty : reader.GetString(ordinal).Trim();");
        }

        [Fact]
        public void ReservationReadMethodsShareTheNormalizedMapper()
        {
            var source = ReadRepoFile("InventoryManagementApp", "Services", "Reservations", "ReservationService.cs");

            AssertReadMethodUsesMapper(source, "public async Task<List<Reservation>> GetAllReservationsAsync()", "public async Task<int> CountReservationsAsync()");
            AssertReadMethodUsesMapper(source, "public async Task<List<Reservation>> GetActiveReservationsAsync()", "public async Task<List<Reservation>> GetReservationsByItemAsync(int itemID)");
            AssertReadMethodUsesMapper(source, "public async Task<List<Reservation>> GetReservationsByItemAsync(int itemID)", "public async Task<List<Reservation>> GetReservationsByCustomerAsync(int customerID)");
            AssertReadMethodUsesMapper(source, "public async Task<List<Reservation>> GetReservationsByCustomerAsync(int customerID)", "public async Task<List<Reservation>> GetUpcomingReservationsAsync(int days = 7)");
            AssertReadMethodUsesMapper(source, "public async Task<List<Reservation>> GetUpcomingReservationsAsync(int days = 7)", "public async Task<int> CountActiveReservationsAsync()");
            AssertReadMethodUsesMapper(source, "public async Task<Reservation?> GetReservationByIdAsync(int reservationID)", "public async Task<int> CreateReservationAsync(Reservation reservation)");
        }

        private static void AssertReadMethodUsesMapper(string source, string startMarker, string endMarker)
        {
            var method = ExtractMethod(source, startMarker, endMarker);
            Assert.Contains("MapReservation(reader)", method, StringComparison.Ordinal);
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
