using System;
using System.Threading.Tasks;
using InventoryManagementApp.Utilities;
using Xunit;

public class MemoryBudgetTests
{
    [Fact]
    public async Task ThresholdExceeded_IsRaised_WhenMemoryAboveThreshold()
    {
        using var budget = new MemoryBudget(TimeSpan.FromMilliseconds(50), 0);
        var tcs = new TaskCompletionSource();
        budget.ThresholdExceeded += (s, e) => tcs.TrySetResult();
        await Task.WhenAny(tcs.Task, Task.Delay(1000));
        Assert.True(tcs.Task.IsCompleted);
    }
}
