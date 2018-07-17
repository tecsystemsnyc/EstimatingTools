using GalaSoft.MvvmLight.CommandWpf;
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
using TECUserControlLibrary.Utilities;
using TECUserControlLibrary.ViewModels;

namespace TECUserControlLibrary.Views
{
    /// <summary>
    /// Interaction logic for RiserView.xaml
    /// </summary>
    public partial class RiserView : UserControl
    {

        public RiserVM ViewModel
        {
            get { return (RiserVM)GetValue(ViewModelProperty); }
            set { SetValue(ViewModelProperty, value); }
        }

        // Using a DependencyProperty as the backing store for ViewModel.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty ViewModelProperty =
            DependencyProperty.Register("ViewModel", typeof(RiserVM), typeof(RiserView));

        private static RelayCommand<LocationContainer> defaultDelete = new RelayCommand<LocationContainer>(item => { }, item => false);

        public ICommand DeleteCommand
        {
            get { return (ICommand)GetValue(DeleteCommandProperty); }
            set { SetValue(DeleteCommandProperty, value); }
        }

        // Using a DependencyProperty as the backing store for DeleteCommand.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty DeleteCommandProperty =
            DependencyProperty.Register("DeleteCommand", typeof(ICommand), typeof(RiserView), new PropertyMetadata(defaultDelete));

        public RiserView()
        {
            InitializeComponent();
        }

        protected void ItemControl_MouseUp(object sender, MouseButtonEventArgs e)
        {
            ListView parent = UIHelpers.FindVisualParent<ListView>(sender as FrameworkElement);
            var item = parent.SelectedItem;
            if (parent.SelectedItems.Count == 1)
            {
                parent.SelectedItem = null;
                parent.SelectedItem = item;
            }
        }
    }
}
