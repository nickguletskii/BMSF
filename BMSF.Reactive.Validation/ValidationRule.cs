namespace BMSF.Reactive.Validation
{
    using System;
    using System.Threading.Tasks;

    public class ValidationRule<T, TData>
    {
        public ValidationRule(Func<TData, IValidator<T>, Task<IValidationResult>> validationFunction)
        {
            this.ValidationFunction = validationFunction;
        }

        public Func<TData, IValidator<T>, Task<IValidationResult>> ValidationFunction { get; set; }
    }
}