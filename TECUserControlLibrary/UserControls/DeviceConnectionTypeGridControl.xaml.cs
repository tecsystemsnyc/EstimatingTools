using EstimatingLibrary;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Windows;
using System.Windows.Controls;

namespace TECUserControlLibrary.UserControls
{
    /// <summary>
    /// Interaction logic for DeviceConnectionTypeGrid.xaml
    /// </summary>
    public partial class DeviceConnectionTypeGridControl : UserControl
    {
        #region DPs

        /// <summary>
        /// Gets or sets the DevicesSource which is displayed
        /// </summary>
        public IEnumerable<TECElectricalMaterial> ConnectionTypesSource
        {
            get { return (IEnumerable<TECElectricalMaterial>)GetValue(ConnectionTypesSourceProperty); }
            set { SetValue(ConnectionTypesSourceProperty, value); }
        }

        /// <summary>
        /// Identified the DevicesSource dependency property
        /// </summary>
        public static readonly DependencyProperty ConnectionTypesSourceProperty =
            DependencyProperty.Register("ConnectionTypesSource", typeof(IEnumerable<TECElectricalMaterial>),
              typeof(DeviceConnectionTypeGridControl), new PropertyMetadata(default(IEnumerable<TECElectricalMaterial>)));
        
        #endregion
        public DeviceConnectionTypeGridControl()
        {
            InitializeComponent();
        }
    }
}
