using System;
using System.Threading.Tasks;
using InventoryManagementApp.Utilities;
using Xunit;

public class MemoryBudgetTests
{
    [Fact]
    public async Task SteadyExceeded_IsRaised_WhenMemoryAboveSteadyThreshold()
    {
        using var budget = new MemoryBudget(TimeSpan.FromMilliseconds(50), 0, long.MaxValue);
        var tcs = new TaskCompletionSource();
        budget.SteadyExceeded += (s, e) => tcs.TrySetResult();
        await Task.WhenAny(tcs.Task, Task.Delay(1000));
        Assert.True(tcs.Task.IsCompleted);
    }

    [Fact]
    public async Task PeakExceeded_IsRaised_WhenMemoryAbovePeakThreshold()
    {
        using var budget = new MemoryBudget(TimeSpan.FromMilliseconds(50), 0, 0);
        var tcs = new TaskCompletionSource();
        budget.PeakExceeded += (s, e) => tcs.TrySetResult();
        await Task.WhenAny(tcs.Task, Task.Delay(1000));
        Assert.True(tcs.Task.IsCompleted);
    }
}
