using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace BMSF.WPF.DemoApp
{
    using ReactiveUI;

    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window, IViewFor<MainWindowViewModel>
    {
        public static readonly DependencyProperty ViewModelProperty =
            DependencyProperty.Register("ViewModel", typeof(MainWindowViewModel), typeof(MainWindow),
                new PropertyMetadata(null));

        public MainWindowViewModel ViewModel
        {
            get => (MainWindowViewModel) this.GetValue(ViewModelProperty);
            set => this.SetValue(ViewModelProperty, value);
        }

        object IViewFor.ViewModel
        {
            get => this.ViewModel;
            set => this.ViewModel = (MainWindowViewModel) value;
        }
        public MainWindow()
        {
            this.DataContext= this.ViewModel = new MainWindowViewModel();
            InitializeComponent();
        }
    }
}
