namespace BMSF.Utilities
{
    using System;
    using System.Threading.Tasks;

    public class AsyncCachedValue<T>
    {
        private readonly Func<Task<T>> _factory;
        private readonly object _lock = new object();
        private bool _isReady;
        private Task<T> _task;
        private T _value;

        public AsyncCachedValue(Func<Task<T>> factory)
        {
            this._factory = factory;
        }

        public Task<T> Get()
        {
            lock (this._lock)
            {
                if (this._isReady)
                    return Task.FromResult(this._value);
                if (this._task == null)
                    this._task = this._factory.Invoke()
                        .ContinueWith(t =>
                        {
                            lock (this._lock)
                            {
                                this._value = t.Result;
                                this._isReady = true;
                            }
                            return t.Result;
                        });
                return this._task;
            }
        }

        public void Invalidate()
        {
            lock (this._lock)
            {
                this._isReady = false;
                this._task = null;
                this._value = default(T);
            }
        }
    }
}