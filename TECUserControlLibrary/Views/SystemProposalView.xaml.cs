using EstimatingLibrary;
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
using TECUserControlLibrary.Utilities;
using TECUserControlLibrary.ViewModels;

namespace TECUserControlLibrary.Views
{
    /// <summary>
    /// Interaction logic for SystemProposalView.xaml
    /// </summary>
    public partial class SystemProposalView : UserControl
    {
        
        public TECSystem SystemSource
        {
            get { return (TECSystem)GetValue(SystemSourceProperty); }
            set { SetValue(SystemSourceProperty, value); }
        }

        // Using a DependencyProperty as the backing store for System.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty SystemSourceProperty =
            DependencyProperty.Register("SystemSource", typeof(TECSystem), typeof(SystemProposalView));


        public SystemProposalView()
        {
            InitializeComponent();
        }

        private void EquipmentListControl_Dropped(object sender, RoutedEventArgs e)
        {
            e.Handled = true;
        }
    }

    public class SystemToProposalViewModelConverter : BaseConverter, IValueConverter
    {
        #region IValueConverter Members

        public object Convert(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            return new SystemProposalVM(value as TECSystem);
        }

        public object ConvertBack(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            throw new NotImplementedException();
        }

        #endregion
    }
}
