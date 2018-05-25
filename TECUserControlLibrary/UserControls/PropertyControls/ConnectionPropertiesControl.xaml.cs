using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using EstimatingLibrary;
using EstimatingLibrary.Interfaces;

namespace TECUserControlLibrary.UserControls.PropertyControls
{
    /// <summary>
    /// Interaction logic for ConnectionPropertiesControl.xaml
    /// </summary>
    public partial class ConnectionPropertiesControl : UserControl
    {
        public IControllerConnection Selected
        {
            get { return (IControllerConnection)GetValue(SelectedProperty); }
            set { SetValue(SelectedProperty, value); }
        }

        // Using a DependencyProperty as the backing store for Selected.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty SelectedProperty =
            DependencyProperty.Register("Selected", typeof(IControllerConnection), typeof(ConnectionPropertiesControl));

        public IEnumerable<TECElectricalMaterial> ConduitTypes
        {
            get { return (IEnumerable<TECElectricalMaterial>)GetValue(ConduitTypesProperty); }
            set { SetValue(ConduitTypesProperty, value); }
        }

        // Using a DependencyProperty as the backing store for ConduitTypes.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty ConduitTypesProperty =
            DependencyProperty.Register("ConduitTypes", typeof(IEnumerable<TECElectricalMaterial>), typeof(ConnectionPropertiesControl));


        public ConnectionPropertiesControl()
        {
            InitializeComponent();
        }
    }
}
