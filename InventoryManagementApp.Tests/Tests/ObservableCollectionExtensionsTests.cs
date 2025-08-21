using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using InventoryManagementApp.Utilities.Extensions;
using Xunit;

namespace InventoryManagementApp.Tests;

public class ObservableCollectionExtensionsTests
{
    [Fact]
    public void ReplaceRange_RaisesSingleResetNotification()
    {
        var collection = new ObservableCollection<int> { 1, 2, 3 };
        var notifications = new List<NotifyCollectionChangedEventArgs>();
        collection.CollectionChanged += (_, e) => notifications.Add(e);

        collection.ReplaceRange(new[] { 4, 5, 6 });

        Assert.Single(notifications);
        Assert.Equal(NotifyCollectionChangedAction.Reset, notifications[0].Action);
        Assert.Equal(new[] { 4, 5, 6 }, collection);
    }

    [Fact]
    public void AddRange_RaisesSingleResetNotification()
    {
        var collection = new ObservableCollection<int> { 1, 2, 3 };
        var notifications = new List<NotifyCollectionChangedEventArgs>();
        collection.CollectionChanged += (_, e) => notifications.Add(e);

        collection.AddRange(new[] { 4, 5 });

        Assert.Single(notifications);
        Assert.Equal(NotifyCollectionChangedAction.Reset, notifications[0].Action);
        Assert.Equal(new[] { 1, 2, 3, 4, 5 }, collection);
    }
}
