namespace BMSF.Reactive.Validation
{
    using System;
    using System.Collections.Generic;
    using System.Reactive.Linq;
    using System.Threading.Tasks;
    using Splat;
    using Utilities;

    public class ComplexValidatorMolecule<T, TData> : IComplexValidatorMolecule, IEnableLogger
    {
        private readonly ComplexValidatorDataProvider<T, TData> _complexValidatorDataProvider;
        private readonly IObservable<ValidationFieldResults> _refCountedObservable;

        public ComplexValidatorMolecule(ComplexValidatorDataProvider<T, TData> complexValidatorDataProvider)
        {
            this._complexValidatorDataProvider = complexValidatorDataProvider;
            this._refCountedObservable = this._complexValidatorDataProvider
                .OnEachExecuteCancellingPrevious(async x =>
                {
                    try
                    {
                        using (this._complexValidatorDataProvider.Validator.ObservableEntranceExitCounter.Enter())
                        {
                            var validationResults = new List<IValidationResult>();
                            foreach (var validationRule in this.Rules)
                            {
                                var validationResult = await validationRule.ValidationFunction.Invoke(x,
                                    this._complexValidatorDataProvider.Validator);
                                if (validationResult.ValidationResultType == ValidationResultType.Valid)
                                    continue;
                                validationResults.Add(validationResult);
                            }
                            return new ValidationFieldResults(this._complexValidatorDataProvider.FieldName,
                                validationResults);
                        }
                    }
                    catch (Exception e)
                    {
                        this.Log().Error($"Error while validating field {complexValidatorDataProvider.FieldName}:", e);
                        return new ValidationFieldResults(this._complexValidatorDataProvider.FieldName,
                            new List<IValidationResult>
                            {
                                new ValidationResult
                                {
                                    Message = Localisation.ErrorWhileValidating,
                                    ValidationResultType = ValidationResultType.Error
                                }
                            });
                    }
                })
                .Catch<ValidationFieldResults, ObjectDisposedException>(
                    ex => Observable.Empty<ValidationFieldResults>())
                .Publish(new ValidationFieldResults(complexValidatorDataProvider.FieldName,
                    new List<IValidationResult>()))
                .RefCount();
        }

        private IList<ValidationRule<T, TData>> Rules { get; } = new List<ValidationRule<T, TData>>();


        public IDisposable Subscribe(IObserver<ValidationFieldResults> observer)
        {
            return this._refCountedObservable
                .Subscribe(observer);
        }

        public async Task<ValidationFieldResults> ValidateNowAsync()
        {
            try
            {
                var data = this._complexValidatorDataProvider.Get();
                var validationResults = new List<IValidationResult>();
                foreach (var validationRule in this.Rules)
                {
                    var validationResult = await validationRule.ValidationFunction.Invoke(data,
                        this._complexValidatorDataProvider.Validator);
                    if (validationResult.ValidationResultType == ValidationResultType.Valid)
                        continue;
                    validationResults.Add(validationResult);
                }
                return new ValidationFieldResults(this._complexValidatorDataProvider.FieldName,
                    validationResults);
            }
            catch (Exception e)
            {
                this.Log().Error($"Error while validating field {this._complexValidatorDataProvider.FieldName}:", e);
                return new ValidationFieldResults(this._complexValidatorDataProvider.FieldName,
                    new List<IValidationResult>
                    {
                        new ValidationResult
                        {
                            Message = Localisation.ErrorWhileValidating,
                            ValidationResultType = ValidationResultType.Error
                        }
                    });
            }
        }

        public ComplexValidatorMolecule<T, TData> RuleAsync(
            Func<TData, IValidator<T>, Task<IValidationResult>> validationFunction)
        {
            this.Rules.Add(new ValidationRule<T, TData>(validationFunction));
            return this;
        }


        public ComplexValidatorMolecule<T, TData> Rule(Func<TData, IValidator<T>, IValidationResult> validationFunction)
        {
            return this.RuleAsync(async (data, validator) => validationFunction(data, validator));
        }


        public ComplexValidatorMolecule<T, TData> RuleAsync(
            Func<TData, Task<IValidationResult>> validationFunction)
        {
            return this.RuleAsync((data, validator) => validationFunction(data));
        }


        public ComplexValidatorMolecule<T, TData> Rule(Func<TData, IValidationResult> validationFunction)
        {
            return this.RuleAsync(async (data, validator) => validationFunction(data));
        }


        public ComplexValidatorMolecule<T, TData> MustBeTrueAsync(ValidationResultType resultType, string message,
            Func<TData, IValidator<T>, Task<bool>> validationFunction)
        {
            return this.RuleAsync(async (data, validator) =>
            {
                var valid = await validationFunction.Invoke(data, validator);
                return new ValidationResult
                {
                    ValidationResultType =
                        valid ? ValidationResultType.Valid : resultType,
                    Message = valid ? null : message
                };
            });
        }

        public ComplexValidatorMolecule<T, TData> MustBeTrue(ValidationResultType resultType, string message,
            Func<TData, IValidator<T>, bool> validationFunction)
        {
            return this.MustBeTrueAsync(resultType, message,
                async (data, validator) => validationFunction(data, validator));
        }

        public ComplexValidatorMolecule<T, TData> MustBeTrueAsync(ValidationResultType resultType, string message,
            Func<TData, Task<bool>> validationFunction)
        {
            return this.MustBeTrueAsync(resultType, message, (data, validator) => validationFunction(data));
        }

        public ComplexValidatorMolecule<T, TData> MustBeTrue(ValidationResultType resultType, string message,
            Func<TData, bool> validationFunction)
        {
            return this.MustBeTrueAsync(resultType, message, async (data, validator) => validationFunction(data));
        }

        public ComplexValidatorMolecule<T, TData> MustBeFalseAsync(ValidationResultType resultType, string message,
            Func<TData, IValidator<T>, Task<bool>> validationFunction)
        {
            return this.MustBeTrueAsync(resultType, message,
                async (data, validator) => !await validationFunction(data, validator));
        }

        public ComplexValidatorMolecule<T, TData> MustBeFalse(ValidationResultType resultType, string message,
            Func<TData, IValidator<T>, bool> validationFunction)
        {
            return this.MustBeFalseAsync(resultType, message,
                async (data, validator) => validationFunction(data, validator));
        }

        public ComplexValidatorMolecule<T, TData> MustBeFalseAsync(ValidationResultType resultType, string message,
            Func<TData, Task<bool>> validationFunction)
        {
            return this.MustBeFalseAsync(resultType, message, (data, validator) => validationFunction(data));
        }

        public ComplexValidatorMolecule<T, TData> MustBeFalse(ValidationResultType resultType, string message,
            Func<TData, bool> validationFunction)
        {
            return this.MustBeFalseAsync(resultType, message, async (data, validator) => validationFunction(data));
        }
    }
}