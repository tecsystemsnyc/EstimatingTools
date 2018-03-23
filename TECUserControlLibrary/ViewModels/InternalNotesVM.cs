using EstimatingLibrary;
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
    public class InternalNotesVM : ViewModelBase
    {
        private TECInternalNote _selectedNote;
        private string _newText = "";
        
        public TECBid Bid { get; }
        public TECInternalNote SelectedNote
        {
            get { return _selectedNote; }
            set
            {
                _selectedNote = value;
                RaisePropertyChanged("SelectedNote");
            }
        }
        public string NewText
        {
            get { return _newText; }
            set
            {
                _newText = value;
                RaisePropertyChanged("NewText");
            }
        }

        public ICommand AddNewCommand { get; private set; }

        public InternalNotesVM(TECBid bid)
        {
            this.Bid = bid;
            AddNewCommand = new RelayCommand(addNewExecute, canAddNew);
        }

        private void addNewExecute()
        {
            TECInternalNote newNote = new TECInternalNote();
            newNote.Label = NewText;
            Bid.InternalNotes.Add(newNote);
            SelectedNote = newNote;
            NewText = "";
        }
        private bool canAddNew()
        {
            return NewText != "";
        }
    }
}
