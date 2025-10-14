using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;

namespace InventoryManagementApp.Services.Dashboard
{
    public sealed class DashboardFeedService
    {
        static readonly DateOnly PostgresDateEpoch = new(1800, 12, 29);

        readonly IDashboardFeedRepository _repository;
        readonly ILogger<DashboardFeedService> _logger;

        public DashboardFeedService(IDashboardFeedRepository repository, ILogger<DashboardFeedService>? logger = null)
        {
            _repository = repository ?? throw new ArgumentNullException(nameof(repository));
            _logger = logger ?? NullLogger<DashboardFeedService>.Instance;
        }

        public async Task<IReadOnlyList<DashboardFeedEntry>> BuildDailyTotalsAsync(
            DashboardFeedConfig config,
            CancellationToken cancellationToken = default)
        {
            if (config is null)
                throw new ArgumentNullException(nameof(config));
            if (string.IsNullOrWhiteSpace(config.DateColumn))
                throw new ArgumentException("Date column is required.", nameof(config));
            if (string.IsNullOrWhiteSpace(config.AmountColumn))
                throw new ArgumentException("Amount column is required.", nameof(config));

            var totals = new SortedDictionary<DateOnly, decimal>();

            await foreach (var row in _repository.GetRowsAsync(config, cancellationToken).ConfigureAwait(false))
            {
                if (!TryReadDate(row, config.DateColumn, out var date))
                {
                    _logger.LogWarning("Skipping row without valid date column {Column}.", config.DateColumn);
                    continue;
                }

                if (config.StartDate.HasValue && date < config.StartDate.Value)
                    continue;

                if (config.EndDate.HasValue && date > config.EndDate.Value)
                    continue;

                if (!TryReadAmount(row, config.AmountColumn, out var amount))
                {
                    _logger.LogWarning("Skipping row without valid amount column {Column}.", config.AmountColumn);
                    continue;
                }

                if (!totals.TryAdd(date, amount))
                    totals[date] += amount;
            }

            return totals.Select(kvp => new DashboardFeedEntry(kvp.Key, decimal.Round(kvp.Value, 2, MidpointRounding.AwayFromZero))).ToList();
        }

        static bool TryReadDate(IReadOnlyDictionary<string, object?> row, string column, out DateOnly date)
        {
            date = default;
            if (!TryGetColumnValue(row, column, out var value) || value is null)
                return false;

            switch (value)
            {
                case DateOnly d:
                    date = d;
                    return true;
                case DateTime dt:
                    date = DateOnly.FromDateTime(dt);
                    return true;
                case DateTimeOffset dto:
                    date = DateOnly.FromDateTime(dto.DateTime);
                    return true;
                case int i:
                    return TryConvertNumericDate(i, out date);
                case long l:
                    return TryConvertNumericDate(l, out date);
                case short s:
                    return TryConvertNumericDate(s, out date);
                case double dbl when !double.IsNaN(dbl) && !double.IsInfinity(dbl):
                    return TryConvertNumericDate((long)Math.Round(dbl, MidpointRounding.AwayFromZero), out date);
                case float flt when !float.IsNaN(flt) && !float.IsInfinity(flt):
                    return TryConvertNumericDate((long)Math.Round(flt, MidpointRounding.AwayFromZero), out date);
                case decimal dec:
                    return TryConvertNumericDate((long)Math.Round(dec, MidpointRounding.AwayFromZero), out date);
                case string s when !string.IsNullOrWhiteSpace(s):
                    return TryParseDateString(s, out date);
                default:
                    return false;
            }
        }

        static bool TryReadAmount(IReadOnlyDictionary<string, object?> row, string column, out decimal amount)
        {
            amount = 0m;
            if (!TryGetColumnValue(row, column, out var value) || value is null)
                return false;

            switch (value)
            {
                case decimal dec:
                    amount = dec;
                    return true;
                case double dbl when !double.IsNaN(dbl) && !double.IsInfinity(dbl):
                    amount = Convert.ToDecimal(dbl);
                    return true;
                case float flt when !float.IsNaN(flt) && !float.IsInfinity(flt):
                    amount = Convert.ToDecimal(flt);
                    return true;
                case int i:
                    amount = i;
                    return true;
                case long l:
                    amount = l;
                    return true;
                case short s:
                    amount = s;
                    return true;
                case string s when decimal.TryParse(s, NumberStyles.Any, CultureInfo.InvariantCulture, out var parsed):
                    amount = parsed;
                    return true;
                default:
                    return false;
            }
        }

        static bool TryGetColumnValue(IReadOnlyDictionary<string, object?> row, string column, out object? value)
        {
            if (row.TryGetValue(column, out value))
                return true;

            foreach (var kvp in row)
            {
                if (string.Equals(kvp.Key, column, StringComparison.OrdinalIgnoreCase))
                {
                    value = kvp.Value;
                    return true;
                }
            }

            value = null;
            return false;
        }

        static bool TryConvertNumericDate(long serial, out DateOnly date)
        {
            try
            {
                date = PostgresDateEpoch.AddDays(serial);
                return true;
            }
            catch (ArgumentOutOfRangeException)
            {
                date = default;
                return false;
            }
        }

        static bool TryParseDateString(string value, out DateOnly date)
        {
            if (DateOnly.TryParse(value, CultureInfo.InvariantCulture, DateTimeStyles.AssumeLocal, out date))
                return true;

            if (DateTime.TryParse(value, CultureInfo.InvariantCulture, DateTimeStyles.AssumeLocal | DateTimeStyles.AllowWhiteSpaces, out var dt))
            {
                date = DateOnly.FromDateTime(dt);
                return true;
            }

            if (long.TryParse(value, NumberStyles.Integer, CultureInfo.InvariantCulture, out var numeric) && TryConvertNumericDate(numeric, out date))
                return true;

            date = default;
            return false;
        }
    }
}
