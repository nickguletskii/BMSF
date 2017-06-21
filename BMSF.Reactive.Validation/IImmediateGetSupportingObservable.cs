namespace BMSF.Reactive.Validation
{
    using System;

    public interface IImmediateGetSupportingObservable<T> : IObservable<T>
    {
        T Get();
    }
}