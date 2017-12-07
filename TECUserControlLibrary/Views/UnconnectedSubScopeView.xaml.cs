﻿using EstimatingLibrary;
using GongSolutions.Wpf.DragDrop;
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

namespace TECUserControlLibrary.Views
{
    /// <summary>
    /// Interaction logic for UnconnectedSubScopeView.xaml
    /// </summary>
    public partial class UnconnectedSubScopeView : UserControl
    {
        public double SystemWidth
        {
            get { return (double)GetValue(SystemWidthProperty); }
            set { SetValue(SystemWidthProperty, value); }
        }

        public static readonly DependencyProperty SystemWidthProperty =
            DependencyProperty.Register("SystemWidth", typeof(double),
              typeof(UnconnectedSubScopeView), new PropertyMetadata(0.0));

        public double EquipmentWidth
        {
            get { return (double)GetValue(EquipmentWidthProperty); }
            set { SetValue(EquipmentWidthProperty, value); }
        }

        public static readonly DependencyProperty EquipmentWidthProperty =
            DependencyProperty.Register("EquipmentWidth", typeof(double),
              typeof(UnconnectedSubScopeView), new PropertyMetadata(0.0));

        public IEnumerable<TECSystem> SystemSource
        {
            get { return (ObservableCollection<TECSystem>)GetValue(SourceProperty); }
            set { SetValue(SourceProperty, value); }
        }

        public static readonly DependencyProperty SourceProperty =
            DependencyProperty.Register("SystemSource", typeof(IEnumerable<TECSystem>),
              typeof(UnconnectedSubScopeView), new PropertyMetadata(default(IEnumerable<TECSystem>)));


        public IEnumerable<TECEquipment> EquipmentSource
        {
            get { return (IEnumerable<TECEquipment>)GetValue(EquipmentSourceProperty); }
            set { SetValue(EquipmentSourceProperty, value); }
        }

        // Using a DependencyProperty as the backing store for EquipmentSource.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty EquipmentSourceProperty =
            DependencyProperty.Register("EquipmentSource", typeof(IEnumerable<TECEquipment>), typeof(UnconnectedSubScopeView));


        public IEnumerable<TECSubScope> SubScopeSource
        {
            get { return (IEnumerable<TECSubScope>)GetValue(SubScopeSourceProperty); }
            set { SetValue(SubScopeSourceProperty, value); }
        }

        // Using a DependencyProperty as the backing store for SubScopeSource.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty SubScopeSourceProperty =
            DependencyProperty.Register("SubScopeSource", typeof(IEnumerable<TECSubScope>), typeof(UnconnectedSubScopeView));
        
        public TECSystem SelectedSystem
        {
            get { return (TECSystem)GetValue(SelectedSystemProperty); }
            set { SetValue(SelectedSystemProperty, value); }
        }

        public static readonly DependencyProperty SelectedSystemProperty =
            DependencyProperty.Register("SelectedSystem", typeof(TECSystem),
                typeof(UnconnectedSubScopeView), new FrameworkPropertyMetadata(default(TECSystem))
                {
                    BindsTwoWayByDefault = true,
                    DefaultUpdateSourceTrigger = UpdateSourceTrigger.PropertyChanged
                });

        public TECEquipment SelectedEquipment
        {
            get { return (TECEquipment)GetValue(SelectedEquipmentProperty); }
            set { SetValue(SelectedEquipmentProperty, value); }
        }

        public static readonly DependencyProperty SelectedEquipmentProperty =
            DependencyProperty.Register("SelectedEquipment", typeof(TECEquipment),
                typeof(UnconnectedSubScopeView), new FrameworkPropertyMetadata(default(TECEquipment))
                {
                    BindsTwoWayByDefault = true,
                    DefaultUpdateSourceTrigger = UpdateSourceTrigger.PropertyChanged
                });

        public TECSubScope SelectedSubScope
        {
            get { return (TECSubScope)GetValue(SelectedSubScopeProperty); }
            set { SetValue(SelectedSubScopeProperty, value); }
        }

        public static readonly DependencyProperty SelectedSubScopeProperty =
            DependencyProperty.Register("SelectedSubScope", typeof(TECSubScope),
                typeof(UnconnectedSubScopeView), new FrameworkPropertyMetadata(default(TECSubScope))
                {
                    BindsTwoWayByDefault = true,
                    DefaultUpdateSourceTrigger = UpdateSourceTrigger.PropertyChanged
                });
        
        public IDropTarget DropHandler
        {
            get { return (IDropTarget)GetValue(DropHandlerProperty); }
            set { SetValue(DropHandlerProperty, value); }
        }

        // Using a DependencyProperty as the backing store for DropHandler.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty DropHandlerProperty =
            DependencyProperty.Register("DropHandler", typeof(IDropTarget), typeof(UnconnectedSubScopeView));



        public UnconnectedSubScopeView()
        {
            InitializeComponent();
            bool initial = true;
            this.SizeChanged += (s, e) =>
            {
                if (e.WidthChanged)
                {
                    if (initial && e.NewSize.Width != 0)
                    {
                        SystemWidth = e.NewSize.Width;
                        EquipmentWidth = e.NewSize.Width;
                        initial = false;
                    }
                    if (SystemWidth != 0)
                    {
                        SystemWidth = e.NewSize.Width;
                    }
                    if (EquipmentWidth != 0)
                    {
                        EquipmentWidth = e.NewSize.Width;
                    }
                }
            };
        }

        private void systemBack_Click(object sender, RoutedEventArgs e)
        {
            SelectedEquipment = null;
        }

        private void equipmentBack_Click(object sender, RoutedEventArgs e)
        {
            SelectedSubScope = null;
        }

        private void systemMove_Completed(object sender, EventArgs e)
        {
            SystemWidth = 0.0;
        }

        private void systemMoveBack_Completed(object sender, EventArgs e)
        {
            SystemWidth = this.ActualWidth;
        }

        private void equipmentMove_Completed(object sender, EventArgs e)
        {
            EquipmentWidth = 0;
        }

        private void equipmentMoveBack_Completed(object sender, EventArgs e)
        {
            EquipmentWidth = this.ActualWidth;
        }
    }
}