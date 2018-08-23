using EstimatingLibrary;
using GongSolutions.Wpf.DragDrop;
using System.Windows;
using System.Windows.Controls;

namespace TECUserControlLibrary.UserControls.PropertyControls
{
    /// <summary>
    /// Interaction logic for ScopePropertiesControl.xaml
    /// </summary>
    public partial class TaggedPropertiesControl: UserControl
    {
        public TECTagged Selected
        {
            get { return (TECTagged)GetValue(SelectedProperty); }
            set { SetValue(SelectedProperty, value); }
        }
        
        public static readonly DependencyProperty SelectedProperty =
            DependencyProperty.Register("Selected", typeof(TECTagged),
              typeof(TaggedPropertiesControl));



        public bool ReadOnly
        {
            get { return (bool)GetValue(ReadOnlyProperty); }
            set { SetValue(ReadOnlyProperty, value); }
        }

        // Using a DependencyProperty as the backing store for ReadOnly.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty ReadOnlyProperty =
            DependencyProperty.Register("ReadOnly", typeof(bool), typeof(TaggedPropertiesControl), new PropertyMetadata(false));



        public IDropTarget DropHandler
        {
            get { return (IDropTarget)GetValue(DropHandlerProperty); }
            set { SetValue(DropHandlerProperty, value); }
        }

        // Using a DependencyProperty as the backing store for DropHandler.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty DropHandlerProperty =
            DependencyProperty.Register("DropHandler", typeof(IDropTarget), typeof(TaggedPropertiesControl));


        public TaggedPropertiesControl()
        {
            InitializeComponent();
        }
    }
}
