namespace BMSF.Reactive.Validation
{
    using System.Collections.Generic;

    public class ValidationFieldResults
    {
        public ValidationFieldResults(string fieldName, IList<IValidationResult> validationResults)
        {
            this.FieldName = fieldName;
            this.ValidationResults = validationResults;
        }

        public string FieldName { get; }
        public IList<IValidationResult> ValidationResults { get; }
    }
}