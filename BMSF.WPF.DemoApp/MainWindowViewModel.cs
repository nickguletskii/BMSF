using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BMSF.WPF.DemoApp
{
    using AutoCompleteControls;
    using ReactiveUI;

    public class MainWindowViewModel : ReactiveObject
    {
        private string _demoProperty;

        public string DemoProperty
        {
            get => this._demoProperty;
            set => this.RaiseAndSetIfChanged(ref this._demoProperty, value);
        }

        public AutoCompleteProvider DemoAutoCompleteProvider { get; } = new AutoCompleteProvider(async str =>
        {
            await Task.Delay(500);
            return new[] {"Apple", "Banana", "Pear", "Cherry", "Tomato"}
                .Where(x => 
                x.ToLowerInvariant().Contains(str.ToLowerInvariant()))
                .Select(x => new SimpleAutoCompletionResult(x))
                .ToList();
        });

    }
}
