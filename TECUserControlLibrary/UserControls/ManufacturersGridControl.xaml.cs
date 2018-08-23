using EstimatingLibrary;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;

namespace TECUserControlLibrary.UserControls
{
    /// <summary>
    /// Interaction logic for ManufacturersGridControl.xaml
    /// </summary>
    public partial class ManufacturersGridControl : UserControl
    {
        #region DPs

        /// <summary>
        /// Gets or sets the DevicesSource which is displayed
        /// </summary>
        public IEnumerable<TECManufacturer> ManufacturersSource
        {
            get { return (IEnumerable<TECManufacturer>)GetValue(ManufacturersSourceProperty); }
            set { SetValue(ManufacturersSourceProperty, value); }
        }

        /// <summary>
        /// Identified the DevicesSource dependency property
        /// </summary>
        public static readonly DependencyProperty ManufacturersSourceProperty =
            DependencyProperty.Register("ManufacturersSource", typeof(IEnumerable<TECManufacturer>),
              typeof(ManufacturersGridControl), new PropertyMetadata(default(IEnumerable<TECManufacturer>)));


        public TECManufacturer Selected
        {
            get { return (TECManufacturer)GetValue(SelectedProperty); }
            set { SetValue(SelectedProperty, value); }
        }

        // Using a DependencyProperty as the backing store for SelectedItem.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty SelectedProperty =
            DependencyProperty.Register("Selected", typeof(TECManufacturer), typeof(ManufacturersGridControl), new FrameworkPropertyMetadata(null)
            {
                BindsTwoWayByDefault = true,
                DefaultUpdateSourceTrigger = UpdateSourceTrigger.PropertyChanged
            });
        
        #endregion

        public ManufacturersGridControl()
        {
            InitializeComponent();
        }
    }
}
