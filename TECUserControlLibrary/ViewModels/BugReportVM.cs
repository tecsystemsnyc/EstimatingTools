using GalaSoft.MvvmLight;
using GalaSoft.MvvmLight.CommandWpf;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;

namespace TECUserControlLibrary.ViewModels
{
    public class BugReportVM : ViewModelBase
    {
        private string _bugDescription = "";
        private string _userName = "";
        private string _userEmail = "";

        public string BugDescription
        {
            get { return _bugDescription; }
            set
            {
                _bugDescription = value;
                RaisePropertyChanged("BugDescription");
            }
        }
        public string UserName
        {
            get { return _userName; }
            set
            {
                _userName = value;
                RaisePropertyChanged("UserName");
            }
        }
        public string UserEmail
        {
            get { return _userEmail; }
            set
            {
                _userEmail = value;
                RaisePropertyChanged("UserEmail");
            }
        }

        public string UserPrompt { get; }

        public ICommand SubmitReportCommand { get; }

        public BugReportVM(string prompt)
        {
            UserPrompt = prompt;

            SubmitReportCommand = new RelayCommand(submitReportExecute);
        }

        private void submitReportExecute()
        {

        }
    }
}
