namespace BMSF.Reactive.Validation
{
    public interface IValidationResult
    {
        ValidationResultType ValidationResultType { get; }
        string Message { get; }
    }
}