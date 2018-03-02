using EstimatingLibrary;
using GongSolutions.Wpf.DragDrop;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;

namespace TECUserControlLibrary.UserControls
{
    /// <summary>
    /// Interaction logic for ConnectionTypeGridControl.xaml
    /// </summary>
    public partial class ElectricalMaterialGridControl : UserControl
    {
        public IEnumerable<TECElectricalMaterial> ElectricalMaterialSource
        {
            get { return (IEnumerable<TECElectricalMaterial>)GetValue(ConnectionTypesSourceProperty); }
            set { SetValue(ConnectionTypesSourceProperty, value); }
        }
        public static readonly DependencyProperty ConnectionTypesSourceProperty =
            DependencyProperty.Register("ElectricalMaterialSource", typeof(IEnumerable<TECElectricalMaterial>),
              typeof(ElectricalMaterialGridControl), new PropertyMetadata(default(IEnumerable<TECElectricalMaterial>)));


        public IDropTarget DropHandler
        {
            get { return (IDropTarget)GetValue(DropHandlerProperty); }
            set { SetValue(DropHandlerProperty, value); }
        }

        // Using a DependencyProperty as the backing store for DropHandler.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty DropHandlerProperty =
            DependencyProperty.Register("DropHandler", typeof(IDropTarget), typeof(ElectricalMaterialGridControl));



        public TECElectricalMaterial Selected
        {
            get { return (TECElectricalMaterial)GetValue(SelectedProperty); }
            set { SetValue(SelectedProperty, value); }
        }

        public static readonly DependencyProperty SelectedProperty =
            DependencyProperty.Register("Selected", typeof(TECElectricalMaterial),
                typeof(ElectricalMaterialGridControl), new FrameworkPropertyMetadata(null)
                {
                    BindsTwoWayByDefault = true,
                    DefaultUpdateSourceTrigger = UpdateSourceTrigger.PropertyChanged
                });

        public ElectricalMaterialGridControl()
        {
            InitializeComponent();
        }
    }
}
