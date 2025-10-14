using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using InventoryManagementApp.Services.Dashboard;
using Microsoft.Extensions.Logging.Abstractions;
using Xunit;

public class DashboardFeedServiceTests
{
    [Fact]
    public async Task BuildDailyTotalsAsync_UsesSelectedAmountColumn()
    {
        var rows = new List<IReadOnlyDictionary<string, object?>>
        {
            new Dictionary<string, object?>
            {
                ["DATE"] = Serial(new DateOnly(2025, 10, 14)),
                ["NETT"] = 492m,
                ["GROSS"] = 520m
            },
            new Dictionary<string, object?>
            {
                ["DATE"] = Serial(new DateOnly(2025, 10, 15)),
                ["NETT"] = 650m,
                ["GROSS"] = 700m
            }
        };

        var config = new DashboardFeedConfig
        {
            DateColumn = "DATE",
            AmountColumn = "NETT"
        };

        var service = new DashboardFeedService(new StubRepository(rows), NullLogger<DashboardFeedService>.Instance);

        var result = await service.BuildDailyTotalsAsync(config, CancellationToken.None);

        Assert.Equal(2, result.Count);
        Assert.Equal(492m, result[0].Amount);
        Assert.Equal(650m, result[1].Amount);
    }

    [Fact]
    public async Task BuildDailyTotalsAsync_ConvertsPostgresSerialDates()
    {
        var serial = Serial(new DateOnly(2025, 10, 14));
        var rows = new List<IReadOnlyDictionary<string, object?>>
        {
            new Dictionary<string, object?>
            {
                ["DATE"] = serial,
                ["NETT"] = 100m
            }
        };

        var config = new DashboardFeedConfig
        {
            DateColumn = "DATE",
            AmountColumn = "NETT"
        };

        var service = new DashboardFeedService(new StubRepository(rows), NullLogger<DashboardFeedService>.Instance);

        var result = await service.BuildDailyTotalsAsync(config, CancellationToken.None);

        Assert.Single(result);
        Assert.Equal(new DateOnly(2025, 10, 14), result[0].Date);
    }

    [Fact]
    public async Task BuildDailyTotalsAsync_IncludesDistinctFirstDays()
    {
        var rows = new List<IReadOnlyDictionary<string, object?>>
        {
            new Dictionary<string, object?>
            {
                ["DATE"] = Serial(new DateOnly(2025, 10, 1)),
                ["NETT"] = 100m
            },
            new Dictionary<string, object?>
            {
                ["DATE"] = Serial(new DateOnly(2025, 10, 2)),
                ["NETT"] = 200m
            },
            new Dictionary<string, object?>
            {
                ["DATE"] = Serial(new DateOnly(2025, 10, 3)),
                ["NETT"] = 300m
            }
        };

        var config = new DashboardFeedConfig
        {
            DateColumn = "DATE",
            AmountColumn = "NETT"
        };

        var service = new DashboardFeedService(new StubRepository(rows), NullLogger<DashboardFeedService>.Instance);

        var result = await service.BuildDailyTotalsAsync(config, CancellationToken.None);

        Assert.Equal(new[]
        {
            new DateOnly(2025, 10, 1),
            new DateOnly(2025, 10, 2),
            new DateOnly(2025, 10, 3)
        }, result.Select(r => r.Date).ToArray());

        Assert.Equal(new[] { 100m, 200m, 300m }, result.Select(r => r.Amount).ToArray());
    }

    [Fact]
    public async Task BuildDailyTotalsAsync_SumsMultipleRowsPerDay()
    {
        var day = new DateOnly(2025, 10, 14);
        var rows = new List<IReadOnlyDictionary<string, object?>>
        {
            new Dictionary<string, object?>
            {
                ["DATE"] = Serial(day),
                ["NETT"] = 100m
            },
            new Dictionary<string, object?>
            {
                ["DATE"] = Serial(day),
                ["NETT"] = 25.5m
            }
        };

        var config = new DashboardFeedConfig
        {
            DateColumn = "DATE",
            AmountColumn = "NETT"
        };

        var service = new DashboardFeedService(new StubRepository(rows), NullLogger<DashboardFeedService>.Instance);

        var result = await service.BuildDailyTotalsAsync(config, CancellationToken.None);

        Assert.Single(result);
        Assert.Equal(125.5m, result[0].Amount);
    }

    static int Serial(DateOnly date)
    {
        var epoch = new DateOnly(1800, 12, 29);
        return date.DayNumber - epoch.DayNumber;
    }

    sealed class StubRepository : IDashboardFeedRepository
    {
        readonly IReadOnlyList<IReadOnlyDictionary<string, object?>> _rows;

        public StubRepository(IReadOnlyList<IReadOnlyDictionary<string, object?>> rows)
        {
            _rows = rows;
        }

        public async IAsyncEnumerable<IReadOnlyDictionary<string, object?>> GetRowsAsync(
            DashboardFeedConfig config,
            [System.Runtime.CompilerServices.EnumeratorCancellation] CancellationToken cancellationToken = default)
        {
            foreach (var row in _rows)
            {
                cancellationToken.ThrowIfCancellationRequested();
                yield return row;
                await Task.Yield();
            }
        }
    }
}
