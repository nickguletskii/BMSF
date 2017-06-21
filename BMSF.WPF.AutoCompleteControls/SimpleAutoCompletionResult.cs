namespace BMSF.WPF.AutoCompleteControls
{
    public class SimpleAutoCompletionResult : IAutoCompletionResult
    {
        public SimpleAutoCompletionResult(string value)
        {
            this.Value = value;
        }

        public string Value { get; set; }

        public override string ToString()
        {
            return this.Value;
        }
    }
}