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
using TECUserControlLibrary.ViewModels.CatalogVMs;

namespace TECUserControlLibrary.Views.CatalogViews
{
    /// <summary>
    /// Interaction logic for ConduitTypesView.xaml
    /// </summary>
    public partial class ConduitTypesCatalogView : UserControl
    {
        public ConduitTypesCatalogVM VM
        {
            get { return (ConduitTypesCatalogVM)GetValue(VMProperty); }
            set { SetValue(VMProperty, value); }
        }

        // Using a DependencyProperty as the backing store for VM.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty VMProperty =
            DependencyProperty.Register("VM", typeof(ConduitTypesCatalogVM), typeof(ConduitTypesCatalogView));
        
        public ConduitTypesCatalogView()
        {
            InitializeComponent();
        }
    }
}
