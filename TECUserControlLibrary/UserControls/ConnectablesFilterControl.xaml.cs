using EstimatingLibrary;
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

namespace TECUserControlLibrary.UserControls
{
    /// <summary>
    /// Interaction logic for ConnectablesFilterControl.xaml
    /// </summary>
    public partial class ConnectablesFilterControl : UserControl
    {


        public bool OmitConnected
        {
            get { return (bool)GetValue(OmitConnectedProperty); }
            set { SetValue(OmitConnectedProperty, value); }
        }

        // Using a DependencyProperty as the backing store for OmitConnected.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty OmitConnectedProperty =
            DependencyProperty.Register("OmitConnected", typeof(bool), typeof(ConnectablesFilterControl));



        public IEnumerable<TECProtocol> Protocols
        {
            get { return (IEnumerable<TECProtocol>)GetValue(ProtocolsProperty); }
            set { SetValue(ProtocolsProperty, value); }
        }

        // Using a DependencyProperty as the backing store for Protocols.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty ProtocolsProperty =
            DependencyProperty.Register("Protocols", typeof(IEnumerable<TECProtocol>), typeof(ConnectablesFilterControl));
        
        public TECProtocol SelectedProtocol
        {
            get { return (TECProtocol)GetValue(SelectedProtocolProperty); }
            set { SetValue(SelectedProtocolProperty, value); }
        }

        // Using a DependencyProperty as the backing store for SelectedProtocol.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty SelectedProtocolProperty =
            DependencyProperty.Register("SelectedProtocol", typeof(TECProtocol), typeof(ConnectablesFilterControl));



        public IEnumerable<TECLocation> Locations
        {
            get { return (IEnumerable<TECLocation>)GetValue(LocationsProperty); }
            set { SetValue(LocationsProperty, value); }
        }

        // Using a DependencyProperty as the backing store for Locations.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty LocationsProperty =
            DependencyProperty.Register("Locations", typeof(IEnumerable<TECLocation>), typeof(ConnectablesFilterControl));
        
        public TECLocation SelectedLocation
        {
            get { return (TECLocation)GetValue(SelectedLocationProperty); }
            set { SetValue(SelectedLocationProperty, value); }
        }

        // Using a DependencyProperty as the backing store for SelectedLocation.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty SelectedLocationProperty =
            DependencyProperty.Register("SelectedLocation", typeof(TECLocation), typeof(ConnectablesFilterControl));




        public ConnectablesFilterControl()
        {
            InitializeComponent();
        }

        private void clearFiltersClicked(object sender, RoutedEventArgs e)
        {
            OmitConnected = false;
            SelectedProtocol = null;
            SelectedLocation = null;
        }
    }
}
