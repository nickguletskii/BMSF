namespace BMSF.WPF.AutoCompleteControls
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel;
    using System.Reactive.Disposables;
    using System.Reactive.Subjects;
    using System.Windows;
    using System.Windows.Controls;
    using System.Windows.Controls.Primitives;
    using ReactiveUI;

    public abstract class AutoCompleteTextBoxBase : Control
    {
        public static readonly DependencyProperty PopupWidthProperty = DependencyProperty.Register(
            "PopupWidth",
            typeof(double),
            typeof(AutoCompleteTextBoxBase),
            new PropertyMetadata(100.0d));

        public static readonly DependencyProperty PopupHeightProperty = DependencyProperty.Register(
            "PopupHeight",
            typeof(double),
            typeof(AutoCompleteTextBoxBase),
            new UIPropertyMetadata(250.0d));

        public static readonly DependencyProperty AutoCompleteProviderProperty = DependencyProperty.Register(
            "AutoCompleteProvider",
            typeof(AutoCompleteProvider),
            typeof(AutoCompleteTextBoxBase),
            new UIPropertyMetadata(AutoCompleteProvider.Empty, OnAutoCompleteProviderChanged));

        private static readonly DependencyPropertyKey IsBusyPropertyKey = DependencyProperty.RegisterReadOnly(
            "IsBusy",
            typeof(bool),
            typeof(AutoCompleteTextBoxBase),
            new FrameworkPropertyMetadata(false));

        public static readonly DependencyProperty IsBusyProperty = IsBusyPropertyKey.DependencyProperty;

        private static readonly DependencyPropertyKey ResultsPropertyKey = DependencyProperty.RegisterReadOnly(
            "Results",
            typeof(ReactiveList<object>),
            typeof(AutoCompleteTextBoxBase),
            new FrameworkPropertyMetadata(new ReactiveList<object>()));

        public static readonly DependencyProperty ResultsProperty = ResultsPropertyKey.DependencyProperty;

        public static readonly DependencyProperty DelayProperty = DependencyProperty.Register(
            "Delay",
            typeof(int),
            typeof(AutoCompleteTextBoxBase), new UIPropertyMetadata(200, OnDelayChanged, OnDelayCoerceValue));

        public static readonly DependencyProperty MinimumCharactersProperty = DependencyProperty.Register(
            "MinimumCharacters",
            typeof(int),
            typeof(AutoCompleteTextBoxBase),
            new UIPropertyMetadata(2, OnMinimumCharactersChanged, OnMinimumCharactersCoerceValue));

        public static readonly DependencyProperty ItemTemplateProperty = DependencyProperty.Register(
            "ItemTemplate",
            typeof(DataTemplate),
            typeof(AutoCompleteTextBoxBase),
            new UIPropertyMetadata(null));

        public static readonly DependencyProperty TextProperty =
            DependencyProperty.Register("Text", typeof(string), typeof(AutoCompleteTextBoxBase),
                new PropertyMetadata(""));

        public static readonly DependencyProperty EmptyResultsTextProperty =
            DependencyProperty.Register("EmptyResultsText", typeof(string), typeof(AutoCompleteTextBoxBase),
                new PropertyMetadata("No matching data found."));

        public static readonly DependencyProperty LoadingTextProperty =
            DependencyProperty.Register("LoadingText", typeof(string), typeof(AutoCompleteTextBoxBase),
                new PropertyMetadata("Loading..."));

        protected Popup Popup;
        protected IConnectableObservable<IEnumerable<object>> PublishedResultsObservable;
        protected IDisposable PublishedResultsObservableConnection;

        public string EmptyResultsText
        {
            get => (string) this.GetValue(EmptyResultsTextProperty);
            set => this.SetValue(EmptyResultsTextProperty, value);
        }

        public string LoadingText
        {
            get => (string) this.GetValue(LoadingTextProperty);
            set => this.SetValue(LoadingTextProperty, value);
        }

        [Bindable(true)]
        public DataTemplate ItemTemplate
        {
            get => (DataTemplate) this.GetValue(ItemTemplateProperty);
            set => this.SetValue(ItemTemplateProperty, value);
        }

        public double PopupWidth
        {
            get => (double) this.GetValue(PopupWidthProperty);
            set => this.SetValue(PopupWidthProperty, value);
        }

        protected CompositeDisposable CompositeDisposable { get; set; } = new CompositeDisposable();

        public string Text
        {
            get => (string) this.GetValue(TextProperty);
            set => this.SetValue(TextProperty, value);
        }

        public double PopupHeight
        {
            get => (double) this.GetValue(PopupHeightProperty);
            set => this.SetValue(PopupHeightProperty, value);
        }

        public bool IsBusy
        {
            get => (bool) this.GetValue(IsBusyProperty);
            protected set => this.SetValue(IsBusyPropertyKey, value);
        }

        public ReactiveList<object> Results
        {
            get => (ReactiveList<object>) this.GetValue(ResultsProperty);
            protected set => this.SetValue(ResultsPropertyKey, value);
        }

        public int Delay
        {
            get => (int) this.GetValue(DelayProperty);
            set => this.SetValue(DelayProperty, value);
        }

        public int MinimumCharacters
        {
            get => (int) this.GetValue(MinimumCharactersProperty);
            set => this.SetValue(MinimumCharactersProperty, value);
        }

        public AutoCompleteProvider AutoCompleteProvider
        {
            get => (AutoCompleteProvider) this.GetValue(AutoCompleteProviderProperty);
            set => this.SetValue(AutoCompleteProviderProperty, value);
        }

        protected void OpenPopup()
        {
            if (this.Popup != null && !this.Popup.IsOpen)
                this.Popup.IsOpen = true;
        }

        protected void ClosePopup()
        {
            if (this.Popup != null && this.Popup.IsOpen)
                this.Popup.IsOpen = false;
        }

        private static void OnAutoCompleteProviderChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            var self = (AutoCompleteTextBoxBase) d;
            self.OnConfigurationChanged();
        }

        private static void OnDelayChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            var self = (AutoCompleteTextBoxBase) d;
            self.OnConfigurationChanged();
        }

        private static object OnDelayCoerceValue(DependencyObject d, object baseValue)
        {
            var value = (int) baseValue;
            return value <= 0 ? 0 : value;
        }

        private static void OnMinimumCharactersChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            var self = (AutoCompleteTextBoxBase) d;
            self.OnConfigurationChanged();
        }

        protected abstract void OnConfigurationChanged();

        private static object OnMinimumCharactersCoerceValue(DependencyObject d, object baseValue)
        {
            var value = (int) baseValue;
            return value <= 0 ? 1 : value;
        }
    }
}