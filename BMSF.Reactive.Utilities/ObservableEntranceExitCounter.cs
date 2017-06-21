namespace BMSF.Reactive.Utilities
{
    using System;
    using System.Collections.Generic;
    using System.Reactive.Disposables;
    using System.Reactive.Subjects;
    using System.Threading;

    /// <summary>
    ///     Counts the amount of items currently in a box.
    /// </summary>
    /// Think of this as a counter for the amount of people in a house.
    /// A physicist, a biologist, and a mathematician are sitting on a bench across from a house. They watch as two people
    /// go into the house, and then a little later, three people walk out.
    /// The physicist says, "The initial measurement was incorrect."
    /// The biologist says, "They must have reproduced."
    /// And the mathematician says, "If exactly one person enters that house, it will be empty."
    /// 
    /// When you call Enter, a person walks in. When the returned disposable is disposed, a person walks out. This way, the
    /// house always has a non-negative amount of people in it.
    public class ObservableEntranceExitCounter : IObservable<int>, IDisposable
    {
        private static int _currentId;
        private readonly Subject<int> _backingSubject = new Subject<int>();
        private readonly HashSet<int> _heldIds = new HashSet<int>();
        private readonly object _lockObject = new object();
        private bool _isDisposed;

        public ObservableEntranceExitCounter()
        {
            this._backingSubject.OnNext(0);
        }

        public void Dispose()
        {
            this._isDisposed = true;

            lock (this._lockObject)
            {
                if (this._isDisposed)
                    return;
                this._backingSubject.OnNext(0);
                this._backingSubject.Dispose();
            }
        }

        public IDisposable Subscribe(IObserver<int> observer)
        {
            if (this._isDisposed)
                throw new InvalidOperationException("Can't use a disposed object");
            return this._backingSubject.Subscribe(observer);
        }

        /// <summary>
        ///     The equivalent of a person entering a house - the count of people in the house increases.
        /// </summary>
        /// <returns>
        ///     A disposable which when disposed will make the person that entered exit the house - the count of people in the
        ///     house decreases.
        /// </returns>
        public IDisposable Enter()
        {
            lock (this._lockObject)
            {
                if (this._isDisposed)
                    throw new ObjectDisposedException("Can't use a disposed object.");
                var id = Interlocked.Increment(ref _currentId);
                lock (this._heldIds)
                {
                    this._heldIds.Add(id);
                    var count = this._heldIds.Count;
                    this._backingSubject.OnNext(count);
                }
                return Disposable.Create(() =>
                {
                    lock (this._heldIds)
                    {
                        if (this._heldIds.Contains(id))
                            this._heldIds.Remove(id);

                        var count2 = this._heldIds.Count;
                        this._backingSubject.OnNext(count2);
                    }
                });
            }
        }
    }
}