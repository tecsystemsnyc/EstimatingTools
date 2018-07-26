using EstimatingLibrary;
using GalaSoft.MvvmLight;
using GalaSoft.MvvmLight.CommandWpf;
using GongSolutions.Wpf.DragDrop;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
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
                return System?.Equipment.Where(x => !System.ProposalItems.Any(y => y.ContainingScope.Contains(x) || y.DisplayScope == x)).ToList();
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
                bool correctSource = sourceType == typeof(TECEquipment) || sourceType == typeof(TECProposalItem);
                bool correctTarget = dropInfo.TargetCollection is IList<TECEquipment> || 
                    targetType == typeof(TECProposalItem) ||
                    System.ProposalItems.Any(x => x.ContainingScope == dropInfo.TargetCollection);

                return correctSource && correctTarget;
            }
        }
        
        public void Drop(IDropInfo dropInfo)
        {
            UIHelpers.Drop(dropInfo, dropObject, false);

            object dropObject(object arg)
            {
                if(arg is TECEquipment equip)
                {
                    var originalProposalItems = new List<TECProposalItem>(System.ProposalItems);
                    foreach (TECProposalItem item in originalProposalItems)
                    {
                        if (item.DisplayScope == equip)
                        {
                            var previousScope = item.ContainingScope;
                            System.ProposalItems.Remove(item);
                            if (previousScope.Count > 0)
                            {
                                var newItem = new TECProposalItem(previousScope.First());
                                previousScope.Remove(previousScope.First());
                                foreach (var scope in previousScope) { newItem.ContainingScope.Add(scope); }
                                System.ProposalItems.Add(newItem);
                            }
                        }
                        else if (item.ContainingScope.Contains(equip))
                        {
                            item.ContainingScope.Remove(equip);
                        }
                    }

                    if (dropInfo.TargetCollection is IList<TECEquipment> collection)
                    {
                        collection.Add(equip);
                    }
                    else
                    {
                        System.ProposalItems.Add(new TECProposalItem(equip));
                    }
                    

                }
                else
                {
                    if (dropInfo.TargetCollection is IList<TECEquipment> collection)
                    {
                        TECProposalItem item = arg as TECProposalItem;
                        if(item.ContainingScope == collection) { return null; }
                        System.ProposalItems.Remove(item);
                        collection.Add(item.DisplayScope);
                        foreach(var sub in item.ContainingScope) { collection.Add(sub); }
                    }
                }
                RaisePropertyChanged("PotentialEquipment");
                return null;

            }
        }        
    }
}
