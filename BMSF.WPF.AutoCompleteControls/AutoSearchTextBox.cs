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
    using Reactive.Utilities;
    using ReactiveUI;
    using Utilities;

    [TemplatePart(Name = TextBoxName, Type = typeof(TextBox))]
    [TemplatePart(Name = PopupName, Type = typeof(Popup))]
    [TemplatePart(Name = ItemsControlName, Type = typeof(ItemsControl))]
    public class AutoSearchTextBox : AutoCompleteTextBoxBase
    {
        public const string TextBoxName = "PART_MainTextBox";
        public const string PopupName = "PART_CompletionPopup";
        public const string ItemsControlName = "CompletionItemsControl";


        public static readonly DependencyProperty NonEmptyResultsTextProperty =
            DependencyProperty.Register("NonEmptyResultsText", typeof(string), typeof(AutoSearchTextBox),
                new PropertyMetadata("Similar entries found:"));

        private bool _isTemplateApplied;
        private ItemsControl _itemsControl;

        private TextBox _textBox;

        static AutoSearchTextBox()
        {
            DefaultStyleKeyProperty.OverrideMetadata(typeof(AutoSearchTextBox),
                new FrameworkPropertyMetadata(typeof(AutoSearchTextBox)));
        }

        public AutoSearchTextBox()
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


        public string NonEmptyResultsText
        {
            get => (string) this.GetValue(NonEmptyResultsTextProperty);
            set => this.SetValue(NonEmptyResultsTextProperty, value);
        }

        public override void OnApplyTemplate()
        {
            base.OnApplyTemplate();

            if (DesignerProperties.GetIsInDesignMode(this))
            {
                return;
            }
            this.UnregisterKeyboardAndMouseEventHandlers();

            this._textBox = (TextBox) this.GetTemplateChild(TextBoxName);
            this.Popup = (Popup) this.GetTemplateChild(PopupName);
            this._itemsControl = (ItemsControl) this.GetTemplateChild(ItemsControlName);

            this._isTemplateApplied = true;
            if (!DesignerProperties.GetIsInDesignMode(this))
            {
                this.RegisterKeyboardAndMouseEventHandlers();
                this.OnConfigurationChanged();
            }
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

        private void DisconnectAllReactivityAndResults()
        {
            this.Results.Clear();
            this.CompositeDisposable.Clear();
        }

        private void SubscribeToQueryEvents()
        {
            if (DesignerProperties.GetIsInDesignMode(this))
            {
                return;
            }
            var textChanged =
                this.WhenAnyValue(x => x.Text)
                    .ObserveOn(RxApp.MainThreadScheduler)
                    .Where(text => text != null && text.Length >= this.MinimumCharacters)
                    .ObserveOn(RxApp.TaskpoolScheduler)
                    .DistinctUntilChanged()
                    .Throttle(TimeSpan.FromMilliseconds(this.Delay));

            var busyStatusMonitor = new BusyStatusMonitor(RxApp.MainThreadScheduler);
            this.CompositeDisposable.Add(busyStatusMonitor);
            this.CompositeDisposable.Add(busyStatusMonitor
                .ObserveOn(RxApp.MainThreadScheduler)
                .Subscribe(x => this.IsBusy = x));

            var autocompletionResultsProviderFunc = this.AutoCompleteProvider.GetAutocompletionResults;

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
                        }));
        }

        private void TextChangesToPopupVisibility()
        {
            if (DesignerProperties.GetIsInDesignMode(this))
            {
                return;
            }
            this.CompositeDisposable.Add(
                this.WhenAnyValue(x => x.Text)
                    .Where(text => text != null && text.Length < this.MinimumCharacters)
                    .ObserveOn(Scheduler.Immediate)
                    .Subscribe(ctx => { this.ClosePopup(); }));

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

        private void RegisterKeyboardAndMouseEventHandlers()
        {
            this.UnregisterKeyboardAndMouseEventHandlers();

            if (!DesignerProperties.GetIsInDesignMode(this))
            {
                if (this._textBox != null)
                {
                    this._textBox.PreviewKeyDown += this.OnTextBoxOnPreviewKeyDown;
                    this._textBox.IsVisibleChanged += this.OnTextBoxOnIsVisibleChanged;
                }
                if (this._itemsControl != null)
                {
                    this._itemsControl.PreviewKeyDown += this.OnItemsControlOnPreviewKeyDown;
                }
            }
        }

        private void UnregisterKeyboardAndMouseEventHandlers()
        {
            if (DesignerProperties.GetIsInDesignMode(this))
            {
                return;
            }
            if (this._textBox != null)
            {
                this._textBox.PreviewKeyDown -= this.OnTextBoxOnPreviewKeyDown;
                this._textBox.IsVisibleChanged -= this.OnTextBoxOnIsVisibleChanged;
            }
            if (this._itemsControl != null)
                this._itemsControl.PreviewKeyDown -= this.OnItemsControlOnPreviewKeyDown;
        }

        private void OnTextBoxOnIsVisibleChanged(object sender, DependencyPropertyChangedEventArgs args)
        {
            if (DesignerProperties.GetIsInDesignMode(this))
            {
                return;
            }
            if ((bool) args.NewValue == false)
                this.ClosePopup();
        }

        private void OnItemsControlOnPreviewKeyDown(object sender, KeyEventArgs args)
        {
            if (DesignerProperties.GetIsInDesignMode(this))
            {
                return;
            }
            if (args.Key == Key.Escape)
            {
                this.ClosePopup();
                args.Handled = true;
                this._textBox.CaretIndex = this.Text.Length;
                this._textBox.Focus();
            }
        }

        private void OnTextBoxOnPreviewKeyDown(object sender, KeyEventArgs args)
        {
            if (DesignerProperties.GetIsInDesignMode(this))
            {
                return;
            }
            if (args.Key == Key.Escape && this.Popup.IsOpen)
            {
                this.ClosePopup();
                args.Handled = true;
            }
        }
    }
}