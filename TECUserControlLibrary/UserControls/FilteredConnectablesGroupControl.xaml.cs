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
using TECUserControlLibrary.Utilities;

namespace TECUserControlLibrary.UserControls
{
    /// <summary>
    /// Interaction logic for FilteredConnectablesGroupControl.xaml
    /// </summary>
    public partial class FilteredConnectablesGroupControl : UserControl
    {
        public IEnumerable<FilteredConnectablesGroup> ItemsSource
        {
            get { return (IEnumerable<FilteredConnectablesGroup>)GetValue(ItemsSourceProperty); }
            set { SetValue(ItemsSourceProperty, value); }
        }

        // Using a DependencyProperty as the backing store for ItemsSource.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty ItemsSourceProperty =
            DependencyProperty.Register("ItemsSource", typeof(IEnumerable<FilteredConnectablesGroup>), typeof(FilteredConnectablesGroupControl));

        public FilteredConnectablesGroup SelectedItem
        {
            get { return (FilteredConnectablesGroup)GetValue(SelectedItemProperty); }
            set { SetValue(SelectedItemProperty, value); }
        }

        public static readonly DependencyProperty SelectedItemProperty =
            DependencyProperty.Register("SelectedItem", typeof(FilteredConnectablesGroup),
                typeof(FilteredConnectablesGroupControl), new FrameworkPropertyMetadata(null)
                {
                    BindsTwoWayByDefault = true,
                    DefaultUpdateSourceTrigger = UpdateSourceTrigger.PropertyChanged
                });

        public bool IsDropTarget
        {
            get { return (bool)GetValue(IsDropTargetProperty); }
            set { SetValue(IsDropTargetProperty, value); }
        }

        // Using a DependencyProperty as the backing store for IsDropTarget.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty IsDropTargetProperty =
            DependencyProperty.Register("IsDropTarget", typeof(bool), typeof(FilteredConnectablesGroupControl), new PropertyMetadata(true));


        public bool IsDragSource
        {
            get { return (bool)GetValue(IsDragSourceProperty); }
            set { SetValue(IsDragSourceProperty, value); }
        }

        // Using a DependencyProperty as the backing store for IsDragSource.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty IsDragSourceProperty =
            DependencyProperty.Register("IsDragSource", typeof(bool), typeof(FilteredConnectablesGroupControl), new PropertyMetadata(true));



        public FilteredConnectablesGroupControl()
        {
            InitializeComponent();
        }

        protected void ScopeGroupControl_MouseUp(object sender, MouseButtonEventArgs e)
        {
            var item = this.SelectedItem;
            ListView child = UIHelpers.FindVisualChild<ListView>(this);
            if (child != null && child.SelectedItems.Count == 1)
            {
                this.SelectedItem = null;
                this.SelectedItem = item;
            }
        }
    }
}
