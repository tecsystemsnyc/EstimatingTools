﻿using EstimatingLibrary;
using EstimatingLibrary.Utilities;
using GalaSoft.MvvmLight;
using GalaSoft.MvvmLight.CommandWpf;
using GongSolutions.Wpf.DragDrop;
using System;
using System.Collections.Generic;
using System.Windows.Input;
using TECUserControlLibrary.Utilities;

namespace TECUserControlLibrary.ViewModels.AddVMs
{
    public class AddEquipmentVM : AddVM
    {
        private TECSystem parent;
        private TECEquipment toAdd;
        private int quantity;
        private Action<TECEquipment> add;
        private TECEquipment underlyingTemplate;
        private bool _displayReferenceProperty = false;
        private ConnectOnAddVM _connectVM;
        private bool _addNewProposalItem = true;
        private TECProposalItem _selectedProposalItem;

        public TECEquipment ToAdd
        {
            get { return toAdd; }
            private set
            {
                toAdd = value;
                RaisePropertyChanged("ToAdd");
            }
        }
        public int Quantity
        {
            get { return quantity; }
            set
            {
                quantity = value;
                RaisePropertyChanged("Quantity");
                updateConnectVMFromQuantity(value);
            }
        }
        public List<TECProposalItem> ExistingProposalItems { get; }
        public bool AddNewProposalItem
        {
            get { return _addNewProposalItem; }
            set
            {
                _addNewProposalItem = value;
                RaisePropertyChanged("AddNewProposalItem");
            }
        }
        public TECProposalItem SelectedProposalItem
        {
            get { return _selectedProposalItem; }
            set
            {
                _selectedProposalItem = value;
                RaisePropertyChanged("SelectedProposalItem");
            }
        }

        private void updateConnectVMFromQuantity(int value)
        {
            if(ConnectVM == null)
            {
                return;
            }
            List<TECSubScope> toConnect = new List<TECSubScope>();
            for(int x = 0; x < quantity; x++)
            {
                toConnect.AddRange(ToAdd.SubScope);
            }
            ConnectVM.Update(toConnect);
        }

        public bool DisplayReferenceProperty {
            get { return _displayReferenceProperty; }
            private set
            {
                _displayReferenceProperty = value;
                RaisePropertyChanged("DisplayReferenceProperty");
            }
        }
        public ConnectOnAddVM ConnectVM
        {
            get { return _connectVM; }
            set
            {
                _connectVM = value;
                RaisePropertyChanged("ConnectVM");
            }
        }

        public AddEquipmentVM(TECSystem parentSystem, TECScopeManager scopeManager) : base(scopeManager)
        {
            parent = parentSystem;
            ExistingProposalItems = new List<TECProposalItem>(parentSystem.ProposalItems);
            toAdd = new TECEquipment();
            add = equip =>
            {
                parentSystem.Equipment.Add(equip);
                if (AddNewProposalItem)
                {
                    parentSystem.ProposalItems.Add(new TECProposalItem(equip));
                }
                else if (SelectedProposalItem != null)
                {
                    SelectedProposalItem.ContainingScope.Add(equip);
                }
            };
            AddCommand = new RelayCommand(addExecute, addCanExecute);
            Quantity = 1;
            ConnectVM = new ConnectOnAddVM(ToAdd.SubScope, parent.Controllers, scopeManager.Catalogs.ConduitTypes);
        }
        public AddEquipmentVM(Action<TECEquipment> addMethod, TECScopeManager scopeManager) : base(scopeManager)
        {
            toAdd = new TECEquipment();
            add = addMethod;
            AddCommand = new RelayCommand(addExecute, addCanExecute);
            Quantity = 1;
            PropertiesVM.DisplayReferenceProperty = false;
        }

        private bool addCanExecute()
        {
            if(ConnectVM != null)
            {
                return ConnectVM.CanConnect();
            }
            else
            {
                return true;
            }
        }
        private void addExecute()
        {
            for (int x = 0; x < Quantity; x++)
            {
                TECEquipment equipment = null;
                if (AsReference && underlyingTemplate != null)
                {
                    equipment = templates.EquipmentSynchronizer.NewItem(underlyingTemplate);
                }
                else if (underlyingTemplate != null)
                {
                    if (templates != null)
                    {
                        equipment = new TECEquipment(underlyingTemplate, ssSynchronizer: templates.SubScopeSynchronizer);
                    }
                    else
                    {
                        equipment = new TECEquipment(underlyingTemplate);
                    }
                }
                else
                {
                    equipment = new TECEquipment(ToAdd);
                }
                equipment.CopyPropertiesFromScope(ToAdd);
                add(equipment);
                if(ConnectVM != null && ConnectVM.Connect)
                {
                    ConnectVM.ExecuteConnection(equipment.SubScope);
                }
                Added?.Invoke(equipment);
            }
        }

        public void SetTemplate(TECEquipment template)
        {
            underlyingTemplate = template;
            ToAdd = new TECEquipment(template);
            if (IsTemplates)
            {
                DisplayReferenceProperty = true;
            }
            if(ConnectVM != null)
            {
                ConnectVM.Update(ToAdd.SubScope);

            }
        }
        
    }
}
