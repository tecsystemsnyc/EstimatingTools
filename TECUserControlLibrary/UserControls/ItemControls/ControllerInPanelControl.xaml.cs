using System.Windows;
using System.Windows.Input;
using TECUserControlLibrary.Models;
using TECUserControlLibrary.ViewModels;

namespace TECUserControlLibrary.UserControls.ItemControls
{
    /// <summary>
    /// Interaction logic for ControllerInPanelControl.xaml
    /// </summary>
    public partial class ControllerInPanelControl : BaseItemControl
    {

        public ControllersPanelsVM ViewModel
        {
            get { return (ControllersPanelsVM)GetValue(ViewModelProperty); }
            set { SetValue(ViewModelProperty, value); }
        }

        // Using a DependencyProperty as the backing store for ViewModel.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty ViewModelProperty =
            DependencyProperty.Register("ViewModel", typeof(ControllersPanelsVM), typeof(ControllerInPanelControl));
        

        public ControllerInPanel Controller
        {
            get { return (ControllerInPanel)GetValue(ControllerProperty); }
            set { SetValue(ControllerProperty, value); }
        }

        // Using a DependencyProperty as the backing store for Controller.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty ControllerProperty =
            DependencyProperty.Register("Controller", typeof(ControllerInPanel), typeof(ControllerInPanelControl));
        
        public ICommand ChangeTypeCommand
        {
            get { return (ICommand)GetValue(ChangeTypeCommandProperty); }
            set { SetValue(ChangeTypeCommandProperty, value); }
        }

        // Using a DependencyProperty as the backing store for ChangeTypeCommand.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty ChangeTypeCommandProperty =
            DependencyProperty.Register("ChangeTypeCommand", typeof(ICommand), typeof(ControllerInPanelControl));

        public static readonly RoutedEvent ChangeTypeEvent =
        EventManager.RegisterRoutedEvent("ChangeType", RoutingStrategy.Bubble,
        typeof(RoutedEventHandler), typeof(ControllerInPanelControl));

        public event RoutedEventHandler ChangeType
        {
            add { AddHandler(ChangeTypeEvent, value); }
            remove { RemoveHandler(ChangeTypeEvent, value); }
        }
        
        public ControllerInPanelControl()
        {
            InitializeComponent();
        }

        protected void changeType_Click(object sender, RoutedEventArgs e)
        {
            RaiseEvent(new RoutedEventArgs(ChangeTypeEvent, this));
        }
    }
}
