namespace BMSF.Reactive.Validation
{
    using System;
    using System.Collections.Generic;
    using System.Threading.Tasks;

    public interface IValidator : IObservable<CompleteValidationResult>, IDisposable
    {
        IObservable<bool> IsValidatingObservable { get; }
        Task<CompleteValidationResult> ValidateNowAsync();
        IEnumerable<string> FieldNames();
        IObservable<ValidationFieldResults> GetFieldObservable(string field);
        void ForceRefresh();
    }

    public interface IValidator<T> : IValidator
    {
        T DataObject { get; }
    }
}