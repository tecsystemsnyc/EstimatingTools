using EstimatingLibrary;
using EstimatingLibrary.Utilities;
using GalaSoft.MvvmLight;
using GalaSoft.MvvmLight.CommandWpf;
using GongSolutions.Wpf.DragDrop;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;
using TECUserControlLibrary.Utilities;

namespace TECUserControlLibrary.ViewModels
{
    public class WorkBoxVM : ViewModelBase, IDropTarget
    {
        private TECScopeManager manager;

        public ObservableCollection<TECObject> BoxItems { get; }

        public ICommand ClearCommand { get; private set; }

        public WorkBoxVM(TECScopeManager manager)
        {
            this.manager = manager;
            BoxItems = new ObservableCollection<TECObject>();
            ClearCommand = new RelayCommand(clearExecute, canClear);
        }

        private void clearExecute()
        {
            BoxItems.ObservablyClear();
        }

        private bool canClear()
        {
            return BoxItems.Count > 0;
        }

        void IDropTarget.DragOver(IDropInfo dropInfo)
        {
            DragDropHelpers.StandardDragOver(dropInfo);
        }

        void IDropTarget.Drop(IDropInfo dropInfo)
        {
            if (dropInfo.Data is TECTypical typ)
            {
                BoxItems.Add(new TECSystem(typ as TECSystem));
            }
            else
            {
                DragDropHelpers.StandardDrop(dropInfo, manager);
            }
        }
    }
}
