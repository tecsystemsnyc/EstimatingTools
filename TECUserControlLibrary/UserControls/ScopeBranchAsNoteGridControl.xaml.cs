using EstimatingLibrary;
using System.Collections.ObjectModel;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

namespace TECUserControlLibrary.UserControls
{
    /// <summary>
    /// Interaction logic for ScopeBranchAsNoteGridControl.xaml
    /// </summary>
    public partial class ScopeBranchAsNoteGridControl : UserControl
    {
        public ObservableCollection<TECScopeBranch> ScopeTreeSource
        {
            get { return (ObservableCollection<TECScopeBranch>)GetValue(ScopeTreeSourceProperty); }
            set { SetValue(ScopeTreeSourceProperty, value); }
        }

        /// <summary>
        /// Identified the SystemSource dependency property
        /// </summary>
        public static readonly DependencyProperty ScopeTreeSourceProperty =
            DependencyProperty.Register("ScopeTreeSource", typeof(ObservableCollection<TECScopeBranch>),
              typeof(ScopeBranchAsNoteGridControl), new PropertyMetadata(default(ObservableCollection<TECScopeBranch>)));

        public ICommand AddCommand
        {
            get { return (ICommand)GetValue(AddCommandProperty); }
            set { SetValue(AddCommandProperty, value); }
        }

        // Using a DependencyProperty as the backing store for AddCommand.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty AddCommandProperty =
            DependencyProperty.Register("AddCommand", typeof(ICommand), typeof(ScopeBranchAsNoteGridControl));


        public ScopeBranchAsNoteGridControl()
        {
            InitializeComponent();
        }
    }
}
