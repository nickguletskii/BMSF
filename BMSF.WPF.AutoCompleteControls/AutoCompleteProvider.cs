namespace BMSF.WPF.AutoCompleteControls
{
    using System;
    using System.Collections.Generic;
    using System.Threading.Tasks;

    public class AutoCompleteProvider
    {
        private static readonly Func<string, Task<IEnumerable<IAutoCompletionResult>>>
            EmptyResultSet =
                async q =>
                    new List<IAutoCompletionResult> {new SimpleAutoCompletionResult("BAD AUTOCOMPLETION CONFIG!")};

        public AutoCompleteProvider()
        {
            this.GetAutocompletionResults = EmptyResultSet;
        }

        public AutoCompleteProvider(
            Func<string, Task<IEnumerable<IAutoCompletionResult>>> getAutocompletionResults)
        {
            this.GetAutocompletionResults = getAutocompletionResults;
        }

        public static AutoCompleteProvider Empty { get; } =
            new AutoCompleteProvider(EmptyResultSet);

        public Func<string, Task<IEnumerable<IAutoCompletionResult>>> GetAutocompletionResults { get; }
    }
}