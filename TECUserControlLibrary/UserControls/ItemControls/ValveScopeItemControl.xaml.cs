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

namespace TECUserControlLibrary.UserControls.ItemControls
{
    /// <summary>
    /// Interaction logic for ValveScopeItemControl.xaml
    /// </summary>
    public partial class ValveScopeItemControl : UserControl
    {

        public ValveScopeItem Valve
        {
            get { return (ValveScopeItem)GetValue(ValveProperty); }
            set { SetValue(ValveProperty, value); }
        }

        // Using a DependencyProperty as the backing store for Valve.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty ValveProperty =
            DependencyProperty.Register("Valve", typeof(ValveScopeItem), typeof(ValveScopeItemControl));


        public ValveScopeItemControl()
        {
            InitializeComponent();
        }
    }
}
