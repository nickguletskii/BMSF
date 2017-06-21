namespace BMSF.Reactive.Validation
{
    using System;
    using System.Reactive;
    using System.Reactive.Linq;
    using System.Reactive.Subjects;
    using System.Threading.Tasks;

    public class ComplexValidatorDataProvider<T, TData> : IImmediateGetSupportingObservable<TData>
    {
        private readonly Func<IValidator<T>, TData> _immediateGetter;
        private readonly IObservable<TData> _observable;

        public ComplexValidatorDataProvider(Validator<T> validator, string fieldName,
            IObservable<TData> observable,
            Func<IValidator<T>, TData> immediateGetter,
            Subject<Unit> forcedUpdates)
        {
            this.Validator = validator;
            this.FieldName = fieldName;
            this._observable = observable
                .CombineLatest(forcedUpdates
                    .StartWith(Unit.Default), (data, _) => data);
            this._immediateGetter = immediateGetter;
            this.Molecule = new ComplexValidatorMolecule<T, TData>(this);
        }

        internal Validator<T> Validator { get; }

        internal ComplexValidatorMolecule<T, TData> Molecule { get; }

        public string FieldName { get; }

        public IDisposable Subscribe(IObserver<TData> observer)
        {
            return this._observable.Subscribe(observer);
        }

        public TData Get()
        {
            return this._immediateGetter(this.Validator);
        }

        public ComplexValidatorMolecule<T, TData> RuleAsync(
            Func<TData, IValidator<T>, Task<IValidationResult>> validationFunction)
        {
            return this.Molecule.RuleAsync(validationFunction);
        }

        public ComplexValidatorMolecule<T, TData> Rule(Func<TData, IValidator<T>, IValidationResult> validationFunction)
        {
            return this.Molecule.Rule(validationFunction);
        }

        public ComplexValidatorMolecule<T, TData> RuleAsync(Func<TData, Task<IValidationResult>> validationFunction)
        {
            return this.Molecule.RuleAsync(validationFunction);
        }

        public ComplexValidatorMolecule<T, TData> Rule(Func<TData, IValidationResult> validationFunction)
        {
            return this.Molecule.Rule(validationFunction);
        }

        public ComplexValidatorMolecule<T, TData> MustBeTrueAsync(ValidationResultType resultType, string message,
            Func<TData, IValidator<T>, Task<bool>> validationFunction)
        {
            return this.Molecule.MustBeTrueAsync(resultType, message, validationFunction);
        }

        public ComplexValidatorMolecule<T, TData> MustBeTrue(ValidationResultType resultType, string message,
            Func<TData, IValidator<T>, bool> validationFunction)
        {
            return this.Molecule.MustBeTrue(resultType, message, validationFunction);
        }

        public ComplexValidatorMolecule<T, TData> MustBeTrueAsync(ValidationResultType resultType, string message,
            Func<TData, Task<bool>> validationFunction)
        {
            return this.Molecule.MustBeTrueAsync(resultType, message, validationFunction);
        }

        public ComplexValidatorMolecule<T, TData> MustBeTrue(ValidationResultType resultType, string message,
            Func<TData, bool> validationFunction)
        {
            return this.Molecule.MustBeTrue(resultType, message, validationFunction);
        }

        public ComplexValidatorMolecule<T, TData> MustBeFalseAsync(ValidationResultType resultType, string message,
            Func<TData, IValidator<T>, Task<bool>> validationFunction)
        {
            return this.Molecule.MustBeFalseAsync(resultType, message, validationFunction);
        }

        public ComplexValidatorMolecule<T, TData> MustBeFalse(ValidationResultType resultType, string message,
            Func<TData, IValidator<T>, bool> validationFunction)
        {
            return this.Molecule.MustBeFalse(resultType, message, validationFunction);
        }

        public ComplexValidatorMolecule<T, TData> MustBeFalseAsync(ValidationResultType resultType, string message,
            Func<TData, Task<bool>> validationFunction)
        {
            return this.Molecule.MustBeFalseAsync(resultType, message, validationFunction);
        }

        public ComplexValidatorMolecule<T, TData> MustBeFalse(ValidationResultType resultType, string message,
            Func<TData, bool> validationFunction)
        {
            return this.Molecule.MustBeFalse(resultType, message, validationFunction);
        }
    }
}