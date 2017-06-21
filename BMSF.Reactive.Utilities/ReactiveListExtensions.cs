namespace BMSF.Reactive.Utilities
{
    using System;
    using System.Reactive.Linq;
    using ReactiveUI;

    public static class ReactiveListExtensions
    {
        public static IObservable<IReactiveCollection<T>> AnyChange<T>(this IReactiveCollection<T> This)
        {
            if (This == null)
                return Observable.Empty<IReactiveCollection<T>>();
            return This.Changed
                .Select(_ => This)
                .Merge(This.ItemChanged
                    .Select(_ => This))
                .Merge(This.ItemsAdded
                    .Select(_ => This))
                .Merge(This.ItemsRemoved
                    .Select(_ => This))
                .Merge(This.ItemsMoved
                    .Select(_ => This))
                .Merge(This.ShouldReset
                    .Select(_ => This))
                .StartWith(This);
        }
    }
}