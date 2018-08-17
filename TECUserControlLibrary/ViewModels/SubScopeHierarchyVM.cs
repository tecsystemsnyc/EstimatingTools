using EstimatingLibrary;
using EstimatingLibrary.Interfaces;
using GalaSoft.MvvmLight;
using GalaSoft.MvvmLight.CommandWpf;
using GongSolutions.Wpf.DragDrop;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TECUserControlLibrary.Utilities;
using TECUserControlLibrary.ViewModels.AddVMs;

namespace TECUserControlLibrary.ViewModels
{
    public class SubScopeHierarchyVM : ViewModelBase, IDropTarget
    {
        private TECCatalogs catalogs;
        private TECScopeManager scopeManager;
        private ViewModelBase selectedVM;
        private TECSubScope selectedSubScope;
        private IEndDevice selectedDevice;
        private TECPoint selectedPoint;
        private object parent;
        private TECInterlockConnection selectedInterlock;

        public ViewModelBase SelectedVM
        {
            get { return selectedVM; }
            set
            {
                selectedVM = value;
                RaisePropertyChanged("SelectedVM");
            }
        }
        public TECSubScope SelectedSubScope
        {
            get { return selectedSubScope; }
            set
            {
                selectedSubScope = value;
                RaisePropertyChanged("SelectedSubScope");
                Selected?.Invoke(value);
            }
        }
        public IEndDevice SelectedDevice
        {
            get { return selectedDevice; }
            set
            {
                selectedDevice = value;
                RaisePropertyChanged("SelectedDevice");
                Selected?.Invoke(value as TECObject);
            }
        }
        public TECPoint SelectedPoint
        {
            get { return selectedPoint; }
            set
            {
                selectedPoint = value;
                RaisePropertyChanged("SelectedPoint");
                Selected?.Invoke(value);
            }
        }
        public TECInterlockConnection SelectedInterlock
        {
            get { return selectedInterlock; }
            set
            {
                selectedInterlock = value;
                RaisePropertyChanged("SelectedInterlock");
                Selected?.Invoke(value);
            }
        }

        public RelayCommand AddSubScopeCommand { get; private set; }
        public RelayCommand<TECSubScope> AddPointCommand { get; private set; }
        public RelayCommand<IInterlockable> AddInterlockCommand { get; private set; }

        public RelayCommand<TECSubScope> DeleteSubScopeCommand { get; private set; }
        public RelayCommand<IEndDevice> DeleteDeviceCommand { get; private set; }
        public RelayCommand<TECPoint> DeletePointCommand { get; private set; }
        public RelayCommand<TECInterlockConnection> DeleteInterlockCommand { get; private set; }

        public SubScopeHierarchyVM(TECScopeManager scopeManager)
        {
            if(scopeManager is TECTemplates)
            {
                parent = scopeManager;
            }
            AddSubScopeCommand = new RelayCommand(addSubScopeExecute, canAddSubScope);
            AddPointCommand = new RelayCommand<TECSubScope>(addPointExecute, canAddPoint);
            AddInterlockCommand = new RelayCommand<IInterlockable>(addInterlockExecute, canAddInterlock);

            DeleteSubScopeCommand = new RelayCommand<TECSubScope>(deleteSubScopeExecute, canDeleteSubScope);
            DeleteDeviceCommand = new RelayCommand<IEndDevice>(deleteDeviceExecute, canDeleteDevice);
            DeletePointCommand = new RelayCommand<TECPoint>(deletePointExecute, canDeletePoint);
            DeleteInterlockCommand = new RelayCommand<TECInterlockConnection>(deleteInterlockExecute, canDeleteInterlock);

            catalogs = scopeManager.Catalogs;
            this.scopeManager = scopeManager;
        }

        public event Action<TECObject> Selected;

        public void Refresh(TECScopeManager scopeManager)
        {
            catalogs = scopeManager.Catalogs;
        }

        private void addSubScopeExecute()
        {
            SelectedVM = new AddSubScopeVM(toAdd =>
            {
                scopeManager.Templates.SubScopeTemplates.Add(toAdd);
            }, scopeManager);
        }
        private bool canAddSubScope()
        {
            return true;
        }

        private void addPointExecute(TECSubScope subScope)
        {
            SelectedVM = new AddPointVM(subScope, scopeManager);
        }
        private bool canAddPoint(TECSubScope subScope)
        {
            return subScope != null;
        }

        private void addInterlockExecute(IInterlockable interlockable)
        {
            SelectedVM = new AddInterlockVM(interlockable, scopeManager);
        }
        private bool canAddInterlock(IInterlockable arg)
        {
            return true;
        }

        private bool canDeleteSubScope(TECSubScope arg)
        {
            return true;
        }
        private void deleteSubScopeExecute(TECSubScope obj)
        {
            scopeManager.Templates.SubScopeTemplates.Remove(obj);
        }

        private bool canDeleteDevice(IEndDevice arg)
        {
            return true;
        }
        private void deleteDeviceExecute(IEndDevice obj)
        {
            SelectedSubScope.RemoveDevice(obj);
        }

        private bool canDeletePoint(TECPoint arg)
        {
            return true;
        }
        private void deletePointExecute(TECPoint obj)
        {
            SelectedSubScope.RemovePoint(obj);
        }

        private bool canDeleteInterlock(TECInterlockConnection arg)
        {
            return true;
        }
        private void deleteInterlockExecute(TECInterlockConnection obj)
        {
            SelectedSubScope.Interlocks.Remove(obj);
        }

        public void DragOver(IDropInfo dropInfo)
        {
            DragDropHelpers.StandardDragOver(dropInfo);

        }
        public void Drop(IDropInfo dropInfo)
        {
            if (dropInfo.Data is TECSubScope subScope)
            {
                SelectedVM = new AddSubScopeVM(toAdd =>
                {
                    scopeManager.Templates.SubScopeTemplates.Add(toAdd);
                }, scopeManager);
                ((AddSubScopeVM)SelectedVM).SetTemplate(subScope);
            }
            else if (dropInfo.Data is TECPoint point)
            {
                SelectedVM = new AddPointVM(SelectedSubScope, scopeManager);
                ((AddPointVM)SelectedVM).SetTemplate(point);
            }
            else if (dropInfo.Data is IEndDevice)
            {
                DragDropHelpers.Drop(dropInfo, obj => SelectedSubScope.AddDevice((obj as IDDCopiable).DragDropCopy(scopeManager) as IEndDevice), false);
            }
        }

        protected void NotifySelected(TECObject item)
        {
            Selected?.Invoke(item);
        }
    }
}
