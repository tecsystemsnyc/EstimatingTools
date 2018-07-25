using EstimatingLibrary;
using GalaSoft.MvvmLight;
using GalaSoft.MvvmLight.CommandWpf;
using GongSolutions.Wpf.DragDrop;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Controls;
using TECUserControlLibrary.Utilities;

namespace TECUserControlLibrary.ViewModels
{
    public class SystemProposalVM : ViewModelBase, IDropTarget
    {
        private TECEquipment _selectedEquipment;

        public TECSystem System { get; }
        public TECEquipment SelectedEquipment
        {
            get { return _selectedEquipment; }
            set
            {
                _selectedEquipment = value;
                RaisePropertyChanged("SelectedEquipment");
            }
        }

        public List<TECEquipment> PotentialEquipment
        {
            get
            {
                return System?.Equipment.Where(x => !System.ProposalItems.Any(y => y.ContainingScope.Contains(x))).ToList();
            }
        }

        public RelayCommand AddItemCommand { get; private set; }
        
        public SystemProposalVM(TECSystem system)
        {
            this.System = system;
            AddItemCommand = new RelayCommand(addItemExecute, canAddItem);
        }

        private void addItemExecute()
        {
            System.ProposalItems.Add(new TECProposalItem(SelectedEquipment));
            SelectedEquipment = null;
            RaisePropertyChanged("PotentialEquipment");
        }

        private bool canAddItem()
        {
            return SelectedEquipment != null;
        }

        public void DragOver(IDropInfo dropInfo)
        {
            UIHelpers.DragOver(dropInfo, dropCondition);

            bool dropCondition(object item, Type sourceType, Type targetType)
            {
                return sourceType == typeof(TECEquipment) && targetType == typeof(TECProposalItem);
            }
        }
        
        public void Drop(IDropInfo dropInfo)
        {
            UIHelpers.Drop(dropInfo, dropObject, false);

            object dropObject(object arg)
            {
                if (dropInfo.TargetItem is TECProposalItem item)
                {
                    item.ContainingScope.Add(arg as TECEquipment);
                }
                else {
                    System.ProposalItems.Add(new TECProposalItem(arg as TECEquipment));
                }
                
                return null;
            }
        }        
    }
}
