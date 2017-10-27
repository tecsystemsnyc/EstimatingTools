﻿using System;
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
using TECUserControlLibrary.Utilities;
using TECUserControlLibrary.ViewModels;

namespace TECUserControlLibrary.Views
{
    /// <summary>
    /// Interaction logic for MaterialSummaryView.xaml
    /// </summary>
    public partial class MaterialSummaryView : UserControl
    {
        public MaterialSummaryView()
        {
            InitializeComponent();
        }

        

        public MaterialSummaryVM ViewModel
        {
            get { return (MaterialSummaryVM)GetValue(ViewModelProperty); }
            set { SetValue(ViewModelProperty, value); }
        }

        // Using a DependencyProperty as the backing store for ViewModel.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty ViewModelProperty =
            DependencyProperty.Register("ViewModel", typeof(MaterialSummaryVM), typeof(MaterialSummaryView));



        public MaterialSummaryIndex SelectedIndex
        {
            get { return (MaterialSummaryIndex)GetValue(SelectedIndexProperty); }
            set { SetValue(SelectedIndexProperty, value); }
        }

        // Using a DependencyProperty as the backing store for SelectedIndex.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty SelectedIndexProperty =
            DependencyProperty.Register("SelectedIndex", typeof(MaterialSummaryIndex), typeof(MaterialSummaryView), new PropertyMetadata(MaterialSummaryIndex.Devices));
        

        
    }
}