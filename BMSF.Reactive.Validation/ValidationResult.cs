namespace BMSF.Reactive.Validation
{
    public struct ValidationResult : IValidationResult
    {
        public ValidationResultType ValidationResultType { get; set; }

        public string Message { get; set; }

        public static ValidationResult Valid { get; } = new ValidationResult
        {
            ValidationResultType = ValidationResultType.Valid
        };
    }
}