namespace BMSF.Reactive.Validation
{
    using System;
    using System.Threading.Tasks;

    public interface IComplexValidatorMolecule : IObservable<ValidationFieldResults>
    {
        Task<ValidationFieldResults> ValidateNowAsync();
    }
}