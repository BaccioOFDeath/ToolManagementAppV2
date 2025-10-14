using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using InventoryManagementApp.Models.Domain;
using InventoryManagementApp.Services.Core;
using InventoryManagementApp.Services.Targets;
using Xunit;

public class MonthlyTargetServiceTests
{
    static List<MonthlyTarget> CreateTargets(int financialYearStart, decimal baseValue)
    {
        var targets = new List<MonthlyTarget>();
        var month = 1;
        var year = financialYearStart;
        for (var offset = 0; offset < 12; offset++)
        {
            if (month > 12)
            {
                month = 1;
                year++;
            }

            targets.Add(new MonthlyTarget
            {
                FinancialYearStart = financialYearStart,
                MonthOffset = offset,
                Month = month,
                Year = year,
                TargetAmount = baseValue + offset
            });

            month++;
        }

        return targets;
    }

    [Fact]
    public async Task SaveTargetsAsync_PersistsTargets()
    {
        var dbPath = Path.Combine(Path.GetTempPath(), Guid.NewGuid() + ".db");
        await using var database = new DatabaseService(dbPath);
        var service = new MonthlyTargetService(database);
        var targets = CreateTargets(2024, 1000m);

        await service.SaveTargetsAsync(2024, targets);
        var result = await service.GetTargetsAsync(2024);

        Assert.Equal(12, result.Count);
        Assert.Equal(1000m, result.First().TargetAmount);
        Assert.Equal(1011m, result.Last().TargetAmount);
    }

    [Fact]
    public async Task SaveTargetsAsync_ReplacesExistingTargets()
    {
        var dbPath = Path.Combine(Path.GetTempPath(), Guid.NewGuid() + ".db");
        await using var database = new DatabaseService(dbPath);
        var service = new MonthlyTargetService(database);
        var initial = CreateTargets(2024, 500m);
        await service.SaveTargetsAsync(2024, initial);

        var updated = CreateTargets(2024, 600m);
        updated[0].TargetAmount = 1234m;
        await service.SaveTargetsAsync(2024, updated);

        var result = await service.GetTargetsAsync(2024);
        Assert.Equal(12, result.Count);
        Assert.Equal(1234m, result.First().TargetAmount);
        Assert.Equal(611m, result.Last().TargetAmount);
    }
}
