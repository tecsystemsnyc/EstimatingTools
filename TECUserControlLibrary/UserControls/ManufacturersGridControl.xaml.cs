using EstimatingLibrary;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Windows;
using System.Windows.Controls;

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


        public TECManufacturer SelectedItem
        {
            get { return (TECManufacturer)GetValue(SelectedItemProperty); }
            set { SetValue(SelectedItemProperty, value); }
        }

        // Using a DependencyProperty as the backing store for SelectedItem.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty SelectedItemProperty =
            DependencyProperty.Register("SelectedItem", typeof(TECManufacturer), typeof(ManufacturersGridControl));
        
        #endregion

        public ManufacturersGridControl()
        {
            InitializeComponent();
        }
    }
}
