using EstimatingLibrary;
using GongSolutions.Wpf.DragDrop;
using System;
using System.Collections.ObjectModel;
using System.Windows;
using System.Windows.Controls;

namespace TECUserControlLibrary.UserControls
{
    /// <summary>
    /// Interaction logic for IOGridControl.xaml
    /// </summary>
    public partial class IOGridControl : UserControl
    {
        /// <summary>
        /// Gets or sets the SystemSource which is displayed
        /// </summary>
        public ObservableCollection<TECIO> IOSource
        {
            get { return (ObservableCollection<TECIO>)GetValue(IOSourceProperty); }
            set { SetValue(IOSourceProperty, value); }
        }

        /// <summary>
        /// Identified the SystemSource dependency property
        /// </summary>
        public static readonly DependencyProperty IOSourceProperty =
            DependencyProperty.Register("IOSource", typeof(ObservableCollection<TECIO>),
              typeof(IOGridControl), new PropertyMetadata(default(ObservableCollection<TECIO>)));



        public IDropTarget DropHandler
        {
            get { return (IDropTarget)GetValue(DropHandlerProperty); }
            set { SetValue(DropHandlerProperty, value); }
        }

        // Using a DependencyProperty as the backing store for DropHandler.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty DropHandlerProperty =
            DependencyProperty.Register("DropHandler", typeof(IDropTarget), typeof(IOGridControl));

        

        public IOGridControl()
        {
            InitializeComponent();
        }
    }
}
