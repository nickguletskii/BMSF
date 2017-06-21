namespace BMSF.Reactive.Utilities
{
    using System;
    using System.Reactive.Linq;
    using System.Threading;
    using System.Threading.Tasks;
    using ReactiveUI;

    public static class ObservableExtensions
    {
        public static IObservable<TOut> OnEachExecuteCancellingPrevious<TIn, TOut>(this IObservable<TIn> This,
            Func<TIn, Task<TOut>> func,
            TOut startEachWith)
        {
            return This.OnEachExecuteCancellingPrevious((item, token) => func(item), startEachWith);
        }

        public static IObservable<TOut> OnEachExecuteCancellingPrevious<TIn, TOut>(this IObservable<TIn> This,
            Func<TIn, Task<TOut>> func)
        {
            return This.OnEachExecuteCancellingPrevious((item, token) => func(item));
        }

        public static IObservable<TOut> OnEachExecuteCancellingPrevious<TIn, TOut>(this IObservable<TIn> This,
            Func<TIn, CancellationToken, Task<TOut>> func,
            TOut startEachWith)
        {
            return
                This
                    .Select(item => Observable.DeferAsync(async token =>
                        {
                            var val = await func.Invoke(item, token);
                            return Observable.Return(val);
                        })
                        .StartWith(startEachWith))
                    .Switch();
        }

        public static IObservable<TOut> OnEachExecuteCancellingPrevious<TIn, TOut>(this IObservable<TIn> This,
            Func<TIn, CancellationToken, Task<TOut>> func)
        {
            return
                This
                    .Select(item => Observable.DeferAsync(async token =>
                    {
                        var val = await func.Invoke(item, token);
                        return Observable.Return(val);
                    }))
                    .Switch();
        }

        public static IDisposable SubscribeNoErrors<TIn>(this IObservable<TIn> This)
        {
            return
                This.ObserveOn(RxApp.MainThreadScheduler)
                    .Subscribe(x => { }, ex => { RxApp.DefaultExceptionHandler.OnNext(ex); });
        }

        public static IDisposable SubscribeNoErrors<TIn>(this IObservable<TIn> This, Action<TIn> observer)
        {
            return
                This.ObserveOn(RxApp.MainThreadScheduler)
                    .Subscribe(observer, ex => { RxApp.DefaultExceptionHandler.OnNext(ex); });
        }
    }
}