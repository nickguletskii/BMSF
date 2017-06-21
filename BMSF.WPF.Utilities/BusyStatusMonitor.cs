namespace BMSF.WPF.Utilities
{
    using System;
    using System.Linq;
    using System.Reactive.Concurrency;
    using System.Reactive.Disposables;
    using System.Reactive.Linq;
    using System.Threading;
    using Reactive.Utilities;
    using ReactiveUI;

    public class BusyStatusMonitor : ReactiveObject, IDisposable, IObservable<bool>
    {
        private static int _ids;

        private readonly CompositeDisposable _compositeDisposable = new CompositeDisposable();
        private readonly ObservableAsPropertyHelper<bool> _isBusy;

        private readonly ReactiveList<(int id, string description)> _running =
            new ReactiveList<(int id, string description)>();

        private readonly IScheduler _scheduler;

        private readonly ObservableAsPropertyHelper<string> _statusText;

        public BusyStatusMonitor(IScheduler scheduler)
        {
            this._scheduler = scheduler;
            scheduler = scheduler ?? RxApp.MainThreadScheduler;
            this._compositeDisposable.Add(
                this._isBusy =
                    this._running
                        .CountChanged
                        .Select(x => x > 0)
                        .ObserveOn(scheduler)
                        .ToProperty(this, x => x.IsBusy, false, false, scheduler));
            this._compositeDisposable.Add(
                this._statusText =
                    this._running
                        .AnyChange()
                        .Select(x => x.Any() ? string.Join("\n", x.Select(y => y.description).Distinct()) : null)
                        .ObserveOn(scheduler)
                        .ToProperty(this, x => x.StatusText, null, false, scheduler));
        }

        public bool IsBusy => this._isBusy.Value;
        public string StatusText => this._statusText.Value;


        public void Dispose()
        {
            this._compositeDisposable.Dispose();
            lock (this._running)
            {
                this._running.Clear();
            }
        }

        public IDisposable Subscribe(IObserver<bool> observer)
        {
            return this._running
                .CountChanged
                .Select(x => x > 0)
                .Subscribe(observer);
        }

        public void AddCommand<TParam, T>(ReactiveCommand<TParam, T> rx, string description)
        {
            var id = Interlocked.Increment(ref _ids);
            var tuple = (id, description);
            this._compositeDisposable.Add(
                rx.IsExecuting
                    .ObserveOn(this._scheduler)
                    .Subscribe(x =>
                    {
                        if (x)
                        {
                            lock (this._running)
                            {
                                this._running.Add(tuple);
                            }
                        }
                        else
                        {
                            lock (this._running)
                            {
                                this._running.Remove(tuple);
                            }
                        }
                    }));
        }

        public IDisposable ReportStatus(string status)
        {
            var id = Interlocked.Increment(ref _ids);
            var tuple = (id, status);
            lock (this._running)
            {
                this._running.Add(tuple);
            }
            return Disposable.Create(() =>
            {
                lock (this._running)
                {
                    this._running.Remove(tuple);
                }
            });
        }
    }
}