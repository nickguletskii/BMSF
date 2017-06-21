namespace BMSF.Reactive.Validation.Tests
{
    using System;
    using System.Collections.Generic;
    using System.Globalization;
    using System.Linq;
    using System.Reactive.Linq;
    using System.Threading;
    using System.Threading.Tasks;
    using Microsoft.Reactive.Testing;
    using ReactiveUI;
    using ReactiveUI.Testing;
    using Xunit;

    public class ValidatorTests
    {
        public class TestData : ReactiveObject
        {
            private bool _dummy;
            private bool _dummy3;

            public bool Dummy
            {
                get => this._dummy;
                set => this.RaiseAndSetIfChanged(ref this._dummy, value);
            }

            public bool Dummy3
            {
                get => this._dummy3;
                set => this.RaiseAndSetIfChanged(ref this._dummy3, value);
            }
        }

        public class CompositeValidator : Validator<TestData>
        {
            public CompositeValidator(TestData dataObject) : base(dataObject)
            {
                this.ExpandValidator(new BasicValidator(dataObject));
                this.ExpandValidator(new ComplexValidator(dataObject));
                this.Seal();
            }

            public IList<IValidationResult> DummyErrors => this.Errors();
        }

        public class BasicValidator : Validator<TestData>
        {
            public BasicValidator(TestData dataObject) : base(dataObject)
            {
                this.ForWhenAnyValue(x => x.Dummy)
                    .MustBeTrue(ValidationResultType.Error, "Dummy must be true!", x => x);
                this.Seal();
            }

            public IList<IValidationResult> DummyErrors => this.Errors();
        }

        public class ComplexValidator : Validator<TestData>
        {
            public ComplexValidator(TestData dataObject) : base(dataObject)
            {
                this.For("Dummy2", this.WhenAnyValue(x => x.DataObject.Dummy), x => x.DataObject.Dummy)
                    .MustBeTrue(ValidationResultType.Error, "Dummy must be true!", x => x);
                this.ForWhenAnyValue(x => x.Dummy3)
                    .MustBeTrue(ValidationResultType.Error, "Dummy must be true!", x => x);
                this.Seal();
            }
        }

        [Fact]
        public void TestBasicValidation()
        {
            var testScheduler = new TestScheduler();

            testScheduler.With(scheduler =>
            {
                var testData = new TestData {Dummy = false};
                using (var validator = new BasicValidator(testData))
                {
                    CompleteValidationResult result = null;
                    validator
                        .ObserveOn(testScheduler)
                        .Subscribe(x => result = x);
                    testScheduler.Start();
                    Assert.False(result.IsValid);
                    Assert.False(validator.IsValidated);
                    Assert.True(validator.HasErrors);
                    Assert.Equal(1, validator.DummyErrors.Count);
                    Assert.Equal("Dummy must be true!", validator.DummyErrors[0].Message);
                    Assert.Equal(ValidationResultType.Error, validator.DummyErrors[0].ValidationResultType);
                    testData.Dummy = true;
                    testScheduler.Start();
                    Assert.True(result.IsValid);
                    Assert.True(validator.IsValidated);
                    Assert.False(validator.HasErrors);
                    Assert.Equal(0, validator.DummyErrors.Count);
                    testData.Dummy = false;
                    testScheduler.Start();
                    Assert.False(result.IsValid);
                    Assert.False(validator.IsValidated);
                    Assert.True(validator.HasErrors);
                    Assert.Equal(1, validator.DummyErrors.Count);
                    Assert.Equal("Dummy must be true!", validator.DummyErrors[0].Message);
                    Assert.Equal(ValidationResultType.Error, validator.DummyErrors[0].ValidationResultType);
                }
            });
        }

        [Fact]
        public void TestCompositeValidation()
        {
            var testScheduler = new TestScheduler();

            testScheduler.With(scheduler =>
            {
                var testData = new TestData {Dummy = false, Dummy3 = true};
                using (var validator = new CompositeValidator(testData))
                {
                    CompleteValidationResult result = null;
                    validator
                        .ObserveOn(testScheduler)
                        .Subscribe(x => result = x);
                    testScheduler.Start();
                    Assert.False(result.IsValid);
                    Assert.False(validator.IsValidated);
                    Assert.True(validator.HasErrors);
                    Assert.Equal(1, validator.DummyErrors.Count);
                    Assert.Equal("Dummy must be true!", validator.DummyErrors[0].Message);
                    Assert.Equal(ValidationResultType.Error, validator.DummyErrors[0].ValidationResultType);
                    testData.Dummy = true;
                    testScheduler.Start();
                    Assert.True(result.IsValid);
                    Assert.True(validator.IsValidated);
                    Assert.False(validator.HasErrors);
                    Assert.Equal(0, validator.DummyErrors.Count);
                    testData.Dummy = false;
                    testScheduler.Start();
                    Assert.False(result.IsValid);
                    Assert.False(validator.IsValidated);
                    Assert.True(validator.HasErrors);
                    Assert.Equal(1, validator.DummyErrors.Count);
                    Assert.Equal("Dummy must be true!", validator.DummyErrors[0].Message);
                    Assert.Equal(ValidationResultType.Error, validator.DummyErrors[0].ValidationResultType);
                }
            });
        }

        [Fact]
        public void TestCompositeValidationGetFieldObservable()
        {
            var testScheduler = new TestScheduler();

            testScheduler.With(scheduler =>
            {
                var testData = new TestData {Dummy = false, Dummy3 = true};
                using (var validator = new CompositeValidator(testData))
                {
                    ValidationFieldResults result = null;
                    validator
                        .GetFieldObservable("Dummy")
                        .ObserveOn(testScheduler)
                        .Subscribe(x => result = x);
                    testScheduler.Start();
                    Assert.False(
                        result.ValidationResults.All(x => x.ValidationResultType == ValidationResultType.Valid));
                    testData.Dummy = true;
                    testScheduler.Start();
                    Assert.True(
                        result.ValidationResults.All(x => x.ValidationResultType == ValidationResultType.Valid));
                    testData.Dummy = false;
                    testScheduler.Start();
                    Assert.False(
                        result.ValidationResults.All(x => x.ValidationResultType == ValidationResultType.Valid));
                }
            });
        }

        [Fact]
        public async Task TestCompositeValidationValidateNow()
        {
            var testScheduler = new TestScheduler();

            await testScheduler.WithAsync(async scheduler =>
            {
                var testData = new TestData {Dummy = false, Dummy3 = true};
                using (var validator = new CompositeValidator(testData))
                {
                    var result = await validator.ValidateNowAsync();
                    Assert.False(result.IsValid);
                    testData.Dummy = true;
                    result = await validator.ValidateNowAsync();
                    Assert.True(result.IsValid);
                    testData.Dummy = false;
                    result = await validator.ValidateNowAsync();
                    Assert.False(result.IsValid);
                }
            });
        }
    }
}