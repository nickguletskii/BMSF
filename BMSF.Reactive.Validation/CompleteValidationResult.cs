namespace BMSF.Reactive.Validation
{
    using System.Collections.Generic;
    using System.Linq;

    public class CompleteValidationResult
    {
        public CompleteValidationResult(IList<ValidationFieldResults> fields)
        {
            this.Fields = fields;
        }

        public IList<ValidationFieldResults> Fields { get; }

        public bool IsValid
            => this.Fields.All(x => x.ValidationResults.All(y => y.ValidationResultType != ValidationResultType.Error));

        public override string ToString()
        {
            return $"{nameof(this.Fields)}: {this.Fields}, {nameof(this.IsValid)}: {this.IsValid}";
        }
    }
}