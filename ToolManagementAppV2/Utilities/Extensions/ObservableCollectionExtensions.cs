using System.Collections.Generic;
using System.Collections.ObjectModel;
using CommunityToolkit.Mvvm.Collections;

namespace ToolManagementAppV2.Utilities.Extensions
{
    internal static class ObservableCollectionExtensions
    {
        public static void ReplaceRange<T>(this ObservableCollection<T> collection, IEnumerable<T> items)
        {
            using (collection.SuspendNotifications())
            {
                collection.Clear();
                foreach (var i in items)
                    collection.Add(i);
            }
        }

        public static void AddRange<T>(this ObservableCollection<T> collection, IEnumerable<T> items)
        {
            foreach (var i in items)
                collection.Add(i);
        }
    }
}

