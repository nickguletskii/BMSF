namespace BMSF.Reactive.Validation
{
    using System;
    using System.Collections.Generic;
    using System.Collections.Immutable;
    using System.Linq;
    using System.Linq.Expressions;
    using System.Reactive;
    using System.Reactive.Linq;
    using System.Reactive.Subjects;
    using System.Runtime.CompilerServices;
    using System.Text.RegularExpressions;
    using System.Threading.Tasks;
    using BMSF.Utilities;
    using ReactiveUI;
    using Utilities;

    public abstract class Validator<T> : ReactiveObject, IValidator<T>, IDisposable
    {
        private readonly Subject<Unit> _forcedRefreshSignals = new Subject<Unit>();
        private IObservable<CompleteValidationResult> _aggregateObservable;
        private IDisposable _connection;

        private ObservableAsPropertyHelper<bool> _hasErrors;
        private bool _isSealed;
        private ObservableAsPropertyHelper<bool> _isValidated;
        private ObservableAsPropertyHelper<bool> _isValidating;

        protected Validator(T dataObject)
        {
            this.DataObject = dataObject;
        }

        private Dictionary<string, IComplexValidatorMolecule> Fields { get; } =
            new Dictionary<string, IComplexValidatorMolecule>();

        private Dictionary<string, ObservableAsPropertyHelper<ValidationFieldResults>> PropertyHelpers { get; } =
            new Dictionary<string, ObservableAsPropertyHelper<ValidationFieldResults>>();

        private List<IValidator> ExpandedValidators { get; } = new List<IValidator>();
        private List<IValidator> OwnedValidators { get; } = new List<IValidator>();

        internal ObservableEntranceExitCounter ObservableEntranceExitCounter { get; } =
            new ObservableEntranceExitCounter();

        public IObservable<bool> HasErrorsObservable => this.Select(x => !x.IsValid);

        public IObservable<bool> IsValidatedObservable => this.IsValidatingObservable
            .CombineLatest(this, (isBusy, validationResult) => !isBusy && validationResult.IsValid);

        public bool IsValidating => this._isValidating.Value;
        public bool IsValidated => this._isValidated.Value;
        public bool HasErrors => this._hasErrors.Value;

        public IObservable<bool> IsValidatingObservable
        {
            get
            {
                var observable = this.ObservableEntranceExitCounter.Select(x => x != 0)
                    .StartWith(false);
                foreach (var expandedValidator in this.ExpandedValidators)
                {
                    observable = observable.CombineLatest(expandedValidator.IsValidatingObservable,
                        (most, current) => most || current);
                }
                return
                    observable
                        .Publish()
                        .RefCount();
            }
        }

        public void Dispose()
        {
            this._connection.Dispose();
            this._forcedRefreshSignals.Dispose();

            foreach (var expandedValidator in this.OwnedValidators)
            {
                expandedValidator.Dispose();
            }
            foreach (var observableAsPropertyHelper in this.PropertyHelpers)
            {
                observableAsPropertyHelper.Value.Dispose();
            }

            this._hasErrors?.Dispose();
            this._isValidated?.Dispose();
            this._isValidating?.Dispose();
            this.ObservableEntranceExitCounter?.Dispose();
        }

        public T DataObject { get; }

        public IDisposable Subscribe(IObserver<CompleteValidationResult> observer)
        {
            return this._aggregateObservable
                .Subscribe(observer);
        }

        public async Task<CompleteValidationResult> ValidateNowAsync()
        {
            this.CheckIsSealed();
            this._forcedRefreshSignals.OnNext(Unit.Default);
            var list = new List<ValidationFieldResults>();
            foreach (var field in this.Fields)
            {
                list.Add(await field.Value.ValidateNowAsync());
            }
            foreach (var expandedValidator in this.ExpandedValidators)
            {
                list.AddRange((await expandedValidator.ValidateNowAsync()).Fields);
            }
            return new CompleteValidationResult(list);
        }


        public IEnumerable<string> FieldNames()
        {
            return this.Fields.Keys
                .Union(this.ExpandedValidators
                    .SelectMany(x => x.FieldNames()));
        }

        public IObservable<ValidationFieldResults> GetFieldObservable(string field)
        {
            this.CheckIsSealed();
            if (this.Fields.ContainsKey(field))
                return this.Fields[field];
            foreach (var expandedValidator in this.ExpandedValidators)
            {
                var observable = expandedValidator.GetFieldObservable(field);
                if (observable != null)
                    return observable;
            }
            return null;
        }

        public void ForceRefresh()
        {
            this._forcedRefreshSignals.OnNext(Unit.Default);
            foreach (var expandedValidator in this.ExpandedValidators)
            {
                expandedValidator.ForceRefresh();
            }
        }

        private void CheckIsSealed()
        {
            if (!this._isSealed)
                throw new InvalidOperationException("Can't query a validator that hasn't been sealed yet");
        }

        private void BuildAggregateObservable()
        {
            var observable = Observable.Return(ImmutableList.Create<ValidationFieldResults>());
            foreach (var field in this.Fields)
            {
                observable = observable.CombineLatest(
                    field.Value.StartWith(new List<ValidationFieldResults>()),
                    (most, tail) =>
                        most.Add(tail));
            }
            foreach (var expandedValidator in this.ExpandedValidators)
            {
                observable = observable.CombineLatest(
                    expandedValidator
                        .Select(x => x.Fields)
                        .StartWith(new List<ValidationFieldResults>()),
                    (most, tail) =>
                        most.AddRange(tail));
            }

            var connectableObservable = observable
                .Select(x => new CompleteValidationResult(x.ToList()))
                .Publish(new CompleteValidationResult(new List<ValidationFieldResults>()));
            this._aggregateObservable = connectableObservable;
            this._connection = connectableObservable.Connect();
        }

        public IList<IValidationResult> Errors([CallerMemberName] string propertyName = null)
        {
            this.CheckIsSealed();
            if (string.IsNullOrEmpty(propertyName))
                throw new ArgumentException("Value cannot be null or empty.", nameof(propertyName));
            var regex = new Regex(@"^([\w\d]+)Errors$");
            var name = regex.Match(propertyName).Groups[1].Value;
            if (!this.PropertyHelpers.ContainsKey(name))
                return new List<IValidationResult>();
            return this.PropertyHelpers[name].Value.ValidationResults;
        }

        public ComplexValidatorDataProvider<T, TData> For<TData>(string fieldName, IObservable<TData> observable,
            Func<IValidator<T>, TData> immediateGetter)
        {
            this.CheckNotSealed();
            if (this.Fields.ContainsKey(fieldName))
                throw new InvalidOperationException(
                    "There are already rules defined for this validation field. " +
                    "Combine the mutliple For chains into one or change each one to correspond to a unique name.");

            var complexValidatorDataProvider = new ComplexValidatorDataProvider<T, TData>(this, fieldName, observable,
                immediateGetter, this._forcedRefreshSignals);
            this.Fields.Add(fieldName, complexValidatorDataProvider.Molecule);
            return complexValidatorDataProvider;
        }

        private void CheckNotSealed()
        {
            if (this._isSealed)
                throw new InvalidOperationException("Can't modify a validator that is already sealed");
        }

        public void ExpandValidator(IValidator validator, bool disposeWhenDisposed = true)
        {
            this.CheckNotSealed();
            this.ExpandedValidators.Add(validator);
            if (disposeWhenDisposed)
                this.OwnedValidators.Add(validator);
        }

        public ComplexValidatorDataProvider<T, TData> ForWhenAnyValue<TData>(Expression<Func<T, TData>> expr)
        {
            this.CheckNotSealed();
            return this.ForWhenAnyValue(((MemberExpression) expr.Body).Member.Name, expr);
        }

        public ComplexValidatorDataProvider<T, TData> ForWhenAnyValue<TData>(string fieldName,
            Expression<Func<T, TData>> expr)
        {
            this.CheckNotSealed();
            var objectSelector = (Expression<Func<Validator<T>, T>>) (x => x.DataObject);
            var compiledExpr = expr.Compile();
            return this.For(fieldName,
                this.WhenAnyValue(ExpressionCompositionExtensions.Compose(expr, objectSelector)),
                x => compiledExpr.Invoke(x.DataObject));
        }

        protected void Seal()
        {
            this.CheckNotSealed();
            foreach (var field in this.Fields)
            {
                if (this.PropertyHelpers.ContainsKey(field.Key))
                    throw new InvalidOperationException($"{field.Key} is registered twice in this validator!");
                this.PropertyHelpers.Add(field.Key,
                    new ObservableAsPropertyHelper<ValidationFieldResults>(
                        field.Value
                            .ObserveOn(RxApp.MainThreadScheduler),
                        _ =>
                            this.RaisePropertyChanged($"{field.Key}Errors"),
                        _ =>
                            this.RaisePropertyChanging($"{field.Key}Errors"),
                        new ValidationFieldResults(field.Key, new List<IValidationResult>()),
                        false,
                        RxApp.MainThreadScheduler));
            }
            foreach (var expandedValidator in this.ExpandedValidators)
            {
                foreach (var field in expandedValidator.FieldNames())
                {
                    if (this.PropertyHelpers.ContainsKey(field))
                        throw new InvalidOperationException($"{field} is registered twice in this validator!");
                    this.PropertyHelpers.Add(field,
                        new ObservableAsPropertyHelper<ValidationFieldResults>(
                            expandedValidator.GetFieldObservable(field)
                                .ObserveOn(RxApp.MainThreadScheduler),
                            _ =>
                                this.RaisePropertyChanged($"{field}Errors"),
                            _ =>
                                this.RaisePropertyChanging($"{field}Errors"),
                            new ValidationFieldResults(field, new List<IValidationResult>()),
                            false,
                            RxApp.MainThreadScheduler));
                }
            }
            this.BuildAggregateObservable();
            this._isValidating = this.IsValidatingObservable
                .ToProperty(this, x => x.IsValidating, false, false, RxApp.MainThreadScheduler);
            this._isValidated = this.IsValidatedObservable
                .ToProperty(this, x => x.IsValidated, false, false, RxApp.MainThreadScheduler);
            this._hasErrors = this.HasErrorsObservable
                .ToProperty(this, x => x.HasErrors, false, false, RxApp.MainThreadScheduler);
            this._isSealed = true;
        }
    }
}