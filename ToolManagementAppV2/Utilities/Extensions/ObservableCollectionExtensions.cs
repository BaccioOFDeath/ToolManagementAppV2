using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.Reflection;

namespace ToolManagementAppV2.Utilities.Extensions
{
    internal static class ObservableCollectionExtensions
    {
        public static IDisposable SuspendNotifications<T>(this ObservableCollection<T> collection) => new NotificationSuspender<T>(collection);

        private sealed class NotificationSuspender<T> : IDisposable
        {
            private readonly ObservableCollection<T> _collection;
            private readonly NotifyCollectionChangedEventHandler? _handlers;

            public NotificationSuspender(ObservableCollection<T> collection)
            {
                _collection = collection;

                var field = typeof(ObservableCollection<T>).GetField("CollectionChanged", BindingFlags.Instance | BindingFlags.NonPublic);
                if (field?.GetValue(collection) is NotifyCollectionChangedEventHandler handlers)
                {
                    _handlers = handlers;
                    foreach (NotifyCollectionChangedEventHandler h in handlers.GetInvocationList())
                        collection.CollectionChanged -= h;
                }
            }

            public void Dispose()
            {
                if (_handlers != null)
                {
                    foreach (NotifyCollectionChangedEventHandler h in _handlers.GetInvocationList())
                        _collection.CollectionChanged += h;

                    _collection.CollectionChanged?.Invoke(_collection,
                        new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Reset));
                }
            }
        }

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

