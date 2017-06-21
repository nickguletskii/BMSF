namespace BMSF.WPF.AutoCompleteControls
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel;
    using System.Reactive.Concurrency;
    using System.Reactive.Linq;
    using System.Windows;
    using System.Windows.Controls;
    using System.Windows.Controls.Primitives;
    using System.Windows.Input;
    using System.Windows.Media;
    using Reactive.Utilities;
    using ReactiveUI;
    using Utilities;

    [TemplatePart(Name = TextBoxName, Type = typeof(TextBox))]
    [TemplatePart(Name = PopupName, Type = typeof(Popup))]
    [TemplatePart(Name = ListBoxName, Type = typeof(ListBox))]
    public class AutoCompleteTextBox : AutoCompleteTextBoxBase
    {
        public const string TextBoxName = "PART_MainTextBox";
        public const string PopupName = "PART_CompletionPopup";
        public const string ListBoxName = "PART_CompletionListBox";


        public static readonly DependencyProperty SetValueFromCompletionCallbackProperty =
            DependencyProperty.Register("SetValueFromCompletionCallback", typeof(Action<IAutoCompletionResult>),
                typeof(AutoCompleteTextBox),
                new PropertyMetadata(null));

        public static readonly DependencyProperty IsAutoFocusEnabledProperty =
            DependencyProperty.Register("IsAutoFocusEnabled", typeof(bool), typeof(AutoCompleteTextBox),
                new PropertyMetadata(true));

        private bool _isTemplateApplied;
        private ListBox _listBox;
        private TextBox _textBox;

        static AutoCompleteTextBox()
        {
            DefaultStyleKeyProperty.OverrideMetadata(typeof(AutoCompleteTextBox),
                new FrameworkPropertyMetadata(typeof(AutoCompleteTextBox)));
        }

        public AutoCompleteTextBox()
        {
            this.Loaded += (sender, args) =>
            {
                this.RegisterKeyboardAndMouseEventHandlers();
                this.OnConfigurationChanged();
            };
            this.Unloaded += (sender, args) =>
            {
                this.CompositeDisposable.Clear();
                this.UnregisterKeyboardAndMouseEventHandlers();
            };
        }

        public Action<IAutoCompletionResult> SetValueFromCompletionCallback
        {
            get => (Action<IAutoCompletionResult>) this.GetValue(SetValueFromCompletionCallbackProperty);
            set => this.SetValue(SetValueFromCompletionCallbackProperty, value);
        }

        public bool IsAutoFocusEnabled
        {
            get => (bool) this.GetValue(IsAutoFocusEnabledProperty);
            set => this.SetValue(IsAutoFocusEnabledProperty, value);
        }

        public override void OnApplyTemplate()
        {
            base.OnApplyTemplate();
            this.UnregisterKeyboardAndMouseEventHandlers();

            this._textBox = (TextBox) this.GetTemplateChild(TextBoxName);
            this.Popup = (Popup) this.GetTemplateChild(PopupName);
            this._listBox = (ListBox) this.GetTemplateChild(ListBoxName);

            this._isTemplateApplied = true;
            if (!DesignerProperties.GetIsInDesignMode(this))
            {
                this.RegisterKeyboardAndMouseEventHandlers();
                this.OnConfigurationChanged();
            }
        }


        private void DisconnectAllReactivityAndResults()
        {
            this.Results.Clear();
            this.CompositeDisposable.Clear();
        }

        protected override void OnConfigurationChanged()
        {
            if (!DesignerProperties.GetIsInDesignMode(this))
            {
                if (!this._isTemplateApplied) return;

                this.DisconnectAllReactivityAndResults();

                this.TextChangesToPopupVisibility();

                this.SubscribeToQueryEvents();
            }
        }

        private void SubscribeToQueryEvents()
        {
            var textChanged =
                this.WhenAnyValue(x => x.Text)
                    .ObserveOn(RxApp.MainThreadScheduler)
                    .Where(text => text != null && text.Length >= this.MinimumCharacters)
                    .ObserveOn(RxApp.TaskpoolScheduler)
                    .DistinctUntilChanged()
                    .Throttle(TimeSpan.FromMilliseconds(this.Delay));

            var autocompletionResultsProviderFunc = this.AutoCompleteProvider.GetAutocompletionResults;

            var busyStatusMonitor = new BusyStatusMonitor(RxApp.MainThreadScheduler);
            this.CompositeDisposable.Add(busyStatusMonitor);
            this.CompositeDisposable.Add(busyStatusMonitor
                .ObserveOn(RxApp.MainThreadScheduler)
                .Subscribe(x => this.IsBusy = x));

            var loadingText = this.LoadingText;

            this.PublishedResultsObservable = textChanged
                .OnEachExecuteCancellingPrevious(async t =>
                {
                    using (busyStatusMonitor.ReportStatus(loadingText))
                    {
                        try
                        {
                            return await autocompletionResultsProviderFunc.Invoke(t);
                        }
                        catch
                        {
                            return new List<IAutoCompletionResult>();
                        }
                    }
                })
                .Publish();
            this.CompositeDisposable.Add(
                this.PublishedResultsObservableConnection = this.PublishedResultsObservable.Connect());
            this.CompositeDisposable.Add(
                this.PublishedResultsObservable.Retry()
                    .ObserveOn(RxApp.MainThreadScheduler)
                    .Subscribe(
                        results =>
                        {
                            using (this.Results.SuppressChangeNotifications())
                            {
                                this.Results.Clear();
                                this.Results.AddRange(results);
                            }

                            if (this._listBox?.SelectedItem != null)
                            {
                                this._listBox.ScrollIntoView(this._listBox.SelectedItem);
                            }
                        }));
        }

        private void TextChangesToPopupVisibility()
        {
            this.CompositeDisposable.Add(
                this.WhenAnyValue(x => x.Text)
                    .Where(text => text != null && text.Length < this.MinimumCharacters)
                    .ObserveOn(Scheduler.Immediate)
                    .Subscribe(ctx => { this.ClosePopup(); })
            );
            this.CompositeDisposable.Add(
                this.WhenAnyValue(x => x.Text)
                    .Where(text => text != null && text.Length >= this.MinimumCharacters)
                    .ObserveOn(Scheduler.Immediate)
                    .Subscribe(ctx =>
                    {
                        if (this._textBox == null)
                            return;
                        if (this._textBox.IsKeyboardFocusWithin)
                        {
                            this.OpenPopup();
                        }
                    }));
        }

        private void SetResultText(IAutoCompletionResult autoCompletionResult)
        {
            this.PublishedResultsObservableConnection.Dispose();
            this.SetValueFromCompletion(autoCompletionResult);
            this.ClosePopup();
            this.PublishedResultsObservableConnection = this.PublishedResultsObservable.Connect();
            this._textBox.CaretIndex = this.Text.Length;
            this._textBox.Focus();
        }

        protected virtual void SetValueFromCompletion(IAutoCompletionResult autoCompletionResult)
        {
            (this.SetValueFromCompletionCallback ?? (autoCompletion => this.Text = autoCompletion.ToString()))
                (autoCompletionResult);
        }

        private void RegisterKeyboardAndMouseEventHandlers()
        {
            this.UnregisterKeyboardAndMouseEventHandlers();

            if (!DesignerProperties.GetIsInDesignMode(this))
            {
                if (this._textBox != null)
                    this._textBox.PreviewKeyDown += this.OnTextBoxOnPreviewKeyDown;
                if (this._listBox != null)
                {
                    this._listBox.PreviewTextInput += this.OnListBoxOnPreviewTextInput;
                    this._listBox.PreviewKeyDown += this.OnListBoxOnPreviewKeyDown;
                    this._listBox.PreviewMouseDown += this.OnListBoxOnPreviewMouseDown;
                }
            }
        }

        private void UnregisterKeyboardAndMouseEventHandlers()
        {
            if (this._textBox != null)
                this._textBox.PreviewKeyDown -= this.OnTextBoxOnPreviewKeyDown;
            if (this._listBox != null)
            {
                this._listBox.PreviewTextInput -= this.OnListBoxOnPreviewTextInput;
                this._listBox.PreviewKeyDown -= this.OnListBoxOnPreviewKeyDown;
                this._listBox.PreviewMouseDown -= this.OnListBoxOnPreviewMouseDown;
            }
        }

        private void OnListBoxOnPreviewMouseDown(object sender, MouseButtonEventArgs args)
        {
            var listboxItem = FindFirstVisualParent<ListBoxItem>((DependencyObject) args.OriginalSource);
            if (listboxItem != null)
            {
                this.SetResultText((IAutoCompletionResult) listboxItem.DataContext);
                args.Handled = true;
            }
        }

        private static T FindFirstVisualParent<T>(DependencyObject element) where T : DependencyObject
        {
            var parent = element;
            while (parent != null)
            {
                var correctlyTyped = parent as T;
                if (correctlyTyped != null)
                {
                    return correctlyTyped;
                }

                parent = VisualTreeHelper.GetParent(parent);
            }
            return null;
        }

        private void OnListBoxOnPreviewKeyDown(object sender, KeyEventArgs args)
        {
            if ((args.Key == Key.Return || args.Key == Key.Enter) && this._listBox.SelectedIndex != -1 &&
                this.Popup.IsOpen)
            {
                this.SetResultText((IAutoCompletionResult) this._listBox.SelectedItem);
                args.Handled = true;
            }
            else if (args.Key == Key.Escape)
            {
                this.ClosePopup();
                args.Handled = true;
                this._textBox.CaretIndex = this.Text.Length;
                this._textBox.Focus();
            }
        }

        private void OnListBoxOnPreviewTextInput(object sender, TextCompositionEventArgs args)
        {
            this.Text += args.Text;
        }

        private void OnTextBoxOnPreviewKeyDown(object sender, KeyEventArgs args)
        {
            if (this._listBox == null)
                return;

            if (args.Key == Key.Up && this._listBox.SelectedIndex > 0)
            {
                this._listBox.SelectedIndex--;
            }
            else if (args.Key == Key.Down && this._listBox.SelectedIndex < this._listBox.Items.Count - 1)
            {
                this._listBox.SelectedIndex++;
            }
            else if ((args.Key == Key.Return || args.Key == Key.Enter) && this._listBox.SelectedIndex != -1 &&
                     this.Popup.IsOpen)
            {
                this.SetResultText((IAutoCompletionResult) this._listBox.SelectedItem);
                args.Handled = true;
            }
            else if (args.Key == Key.Escape && this.Popup.IsOpen)
            {
                this.ClosePopup();
                args.Handled = true;
            }

            this._listBox.ScrollIntoView(this._listBox.SelectedItem);
        }
    }
}