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
using TECUserControlLibrary.Models;

namespace TECUserControlLibrary.Views
{
    /// <summary>
    /// Interaction logic for ItemizedDetailsView.xaml
    /// </summary>
    public partial class ItemizedDetailsView : UserControl
    {
        
        public ScopeSummaryItem Selected
        {
            get { return (ScopeSummaryItem)GetValue(SelectedProperty); }
            set { SetValue(SelectedProperty, value); }
        }

        // Using a DependencyProperty as the backing store for Selected.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty SelectedProperty =
            DependencyProperty.Register("Selected", typeof(ScopeSummaryItem), typeof(ItemizedDetailsView));


        public ItemizedDetailsView()
        {
            InitializeComponent();
        }
    }
}
