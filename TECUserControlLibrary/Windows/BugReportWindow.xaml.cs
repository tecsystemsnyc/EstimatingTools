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
using System.Windows.Shapes;
using TECUserControlLibrary.ViewModels;

namespace TECUserControlLibrary.Windows
{
    public partial class BugReportWindow : Window
    {
        public BugReportVM VM
        {
            get { return (BugReportVM)GetValue(VMProperty); }
            set { SetValue(VMProperty, value); }
        }

        // Using a DependencyProperty as the backing store for VM.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty VMProperty =
            DependencyProperty.Register("VM", typeof(BugReportVM), typeof(BugReportWindow));
        
        public BugReportWindow(string reportType, string prompt, string logPath)
        {
            InitializeComponent();

            VM = new BugReportVM(reportType, prompt, logPath);
        }

        private void SubmitClicked(object sender, RoutedEventArgs e)
        {
            this.Close();
        }
    }
}
