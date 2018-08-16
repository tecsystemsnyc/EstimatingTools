using EstimatingLibrary;
using System;
using System.Collections.Generic;
using System.Diagnostics;
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
using System.Windows.Shapes;

namespace TECUserControlLibrary.Windows
{
    /// <summary>
    /// Interaction logic for ToDoWindow.xaml
    /// </summary>
    public partial class ToDoWindow : Window
    {
        public IEnumerable<TECToDoItem> ToDos
        {
            get { return (IEnumerable<TECToDoItem>)GetValue(ToDosProperty); }
            set { SetValue(ToDosProperty, value); }
        }

        // Using a DependencyProperty as the backing store for ToDos.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty ToDosProperty =
            DependencyProperty.Register("ToDos", typeof(IEnumerable<TECToDoItem>), typeof(ToDoWindow));
        
        public ToDoWindow(IEnumerable<TECToDoItem> items)
        {
            ToDos = items;
            InitializeComponent();
        }

        private void Hyperlink_RequestNavigate(object sender, System.Windows.Navigation.RequestNavigateEventArgs e)
        {
            Process.Start(new ProcessStartInfo(e.Uri.AbsoluteUri));
            e.Handled = true;
        }
    }
}
