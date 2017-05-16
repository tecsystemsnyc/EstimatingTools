﻿using EstimatingLibrary;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
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

namespace TECUserControlLibrary.DataGrids
{
    /// <summary>
    /// Interaction logic for ControlledScopeDataGrid.xaml
    /// </summary>
    public partial class ControlledScopeDataGrid : UserControl
    {
        #region DPs

        /// <summary>
        /// Gets or sets the ControlledScopeSource which is displayed
        /// </summary>
        public ObservableCollection<TECControlledScope> ControlledScopeSource
        {
            get { return (ObservableCollection<TECControlledScope>)GetValue(ControlledScopeSourceProperty); }
            set { SetValue(ControlledScopeSourceProperty, value); }
        }

        /// <summary>
        /// Identified the ControlledScopeSource dependency property
        /// </summary>
        public static readonly DependencyProperty ControlledScopeSourceProperty =
            DependencyProperty.Register("ControlledScopeSource", typeof(ObservableCollection<TECControlledScope>),
              typeof(ControlledScopeDataGrid), new PropertyMetadata(default(ObservableCollection<TECControlledScope>)));

        public object SelectedItem
        {
            get { return GetValue(SelectedItemProperty); }
            set { SetValue(SelectedItemProperty, value); }
        }

        public static readonly DependencyProperty SelectedItemProperty = DependencyProperty.Register("SelectedItem", typeof(object), typeof(ControlledScopeDataGrid), new FrameworkPropertyMetadata(null)
        {
            BindsTwoWayByDefault = true,
            DefaultUpdateSourceTrigger = UpdateSourceTrigger.PropertyChanged
        });

        /// <summary>
        /// Gets or sets the ViewModel which is used
        /// </summary>
        public Object ViewModel
        {
            get { return (Object)GetValue(ViewModelProperty); }
            set { SetValue(ViewModelProperty, value); }
        }

        /// <summary>
        /// Identified the ViewModel dependency property
        /// </summary>
        public static readonly DependencyProperty ViewModelProperty =
            DependencyProperty.Register("ViewModel", typeof(Object),
              typeof(ControlledScopeDataGrid));

        #endregion
        public ControlledScopeDataGrid()
        {
            InitializeComponent();
           
        }
    }
}
