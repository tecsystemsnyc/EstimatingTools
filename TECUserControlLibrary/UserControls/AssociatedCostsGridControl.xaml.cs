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
    /// Interaction logic for AssociatedCostsGridControl.xaml
    /// </summary>
    public partial class AssociatedCostsGridControl : UserControl
    {
        #region DPs

        public IEnumerable<TECAssociatedCost> CostsSource
        {
            get { return (IEnumerable<TECAssociatedCost>)GetValue(CostsSourceProperty); }
            set { SetValue(CostsSourceProperty, value); }
        }

        public static readonly DependencyProperty CostsSourceProperty =
            DependencyProperty.Register("CostsSource", typeof(IEnumerable<TECAssociatedCost>),
              typeof(AssociatedCostsGridControl), new PropertyMetadata(default(IEnumerable<TECAssociatedCost>)));


        /// <summary>
        /// Gets or sets the ViewModel which is used
        /// </summary>
        public IDropTarget DropHandler
        {
            get { return (IDropTarget)GetValue(DropHandlerProperty); }
            set { SetValue(DropHandlerProperty, value); }
        }

        /// <summary>
        /// Identified the ViewModel dependency property
        /// </summary>
        public static readonly DependencyProperty DropHandlerProperty =
            DependencyProperty.Register("DropHandler", typeof(IDropTarget),
              typeof(AssociatedCostsGridControl));


        public TECAssociatedCost Selected
        {
            get { return (TECAssociatedCost)GetValue(SelectedProperty); }
            set { SetValue(SelectedProperty, value); }
        }
        
        public static readonly DependencyProperty SelectedProperty =
            DependencyProperty.Register("Selected", typeof(TECAssociatedCost),
                typeof(AssociatedCostsGridControl), new FrameworkPropertyMetadata(null)
                {
                    BindsTwoWayByDefault = true,
                    DefaultUpdateSourceTrigger = UpdateSourceTrigger.PropertyChanged
                });

        public bool ReadOnly
        {
            get { return (bool)GetValue(ReadOnlyProperty); }
            set { SetValue(ReadOnlyProperty, value); }
        }

        // Using a DependencyProperty as the backing store for CanEdit.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty ReadOnlyProperty =
            DependencyProperty.Register("ReadOnly", typeof(bool), typeof(AssociatedCostsGridControl), new PropertyMetadata(false));
        #endregion
        public AssociatedCostsGridControl()
        {
            InitializeComponent();
        }
    }
}
