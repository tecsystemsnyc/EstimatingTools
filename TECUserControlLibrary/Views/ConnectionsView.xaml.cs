using GalaSoft.MvvmLight;
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
using System.Windows.Media.Animation;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using TECUserControlLibrary.ViewModels;

namespace TECUserControlLibrary.Views
{
    /// <summary>
    /// Interaction logic for ConnectionsView.xaml
    /// </summary>
    public partial class ConnectionsView : UserControl
    {

        public double ModalHeight
        {
            get { return (double)GetValue(ModalHeightProperty); }
            set { SetValue(ModalHeightProperty, value); }
        }
        public static readonly DependencyProperty ModalHeightProperty =
            DependencyProperty.Register("ModalHeight", typeof(double),
              typeof(ConnectionsView), new PropertyMetadata(1.0));

        public ConnectionsVM VM
        {
            get { return (ConnectionsVM)GetValue(VMProperty); }
            set { SetValue(VMProperty, value); }
        }

        // Using a DependencyProperty as the backing store for VM.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty VMProperty =
            DependencyProperty.Register("VM", typeof(ConnectionsVM), 
                typeof(ConnectionsView));
        
        public bool SelectionNeeded
        {
            get { return (bool)GetValue(SelectionNeededProperty); }
            set { SetValue(SelectionNeededProperty, value); }
        }

        // Using a DependencyProperty as the backing store for SelectionNeeded.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty SelectionNeededProperty =
            DependencyProperty.Register("SelectionNeeded", typeof(bool), typeof(ConnectionsView), new PropertyMetadata(false, selectionNeededChanged));

        private static void selectionNeededChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            var thisView = d as ConnectionsView;
            Grid grid = (Grid)thisView.FindName("mainGrid");
            var value = (bool)e.NewValue;
            if (value)
            {
                Storyboard modalIn = (Storyboard)grid.FindResource("modalIn");
                modalIn.Begin();
            }
            else
            {
                Storyboard modalOut = (Storyboard)grid.FindResource("modalOut");
                modalOut.Begin();
            }
        }
        public ConnectionsView()
        {
            InitializeComponent();
            SizeChanged += handleSizeChanged;
        }

        private void modalOut_Completed(object sender, EventArgs e)
        {
            ModalHeight = this.ActualHeight;
        }

        private void modalIn_Completed(object sender, EventArgs e)
        {
            ModalHeight = 0;
        }

        private void handleSizeChanged(object sender, SizeChangedEventArgs e)
        {
            if (e.HeightChanged)
            {
                if (ModalHeight != 0.0)
                {
                    ModalHeight = e.NewSize.Height;
                }
            }
        }


    }
}
