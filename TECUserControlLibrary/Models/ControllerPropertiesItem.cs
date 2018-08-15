using EstimatingLibrary;
using GalaSoft.MvvmLight;
using GalaSoft.MvvmLight.CommandWpf;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;

namespace TECUserControlLibrary.Models
{
    public class ProvidedControllerPropertiesItem : ViewModelBase
    {
        private List<TECIO> _io;
        private List<TECIO> _availableIO;
        private ObservableCollection<ModuleGroup> _modules;

        public TECProvidedController Controller
        {
            get; private set;
        }

        public List<TECIO> IO
        {
            get { return _io; }
            set
            {
                _io = value;
                RaisePropertyChanged("IO");
            }
        }
        public List<TECIO> AvailableIO
        {
            get { return _availableIO; }
            set
            {
                _availableIO = value;
                RaisePropertyChanged("AvailableIO");
            }
        }
        public ObservableCollection<ModuleGroup> Modules
        {
            get { return _modules; }
            set
            {
                _modules = value;
                RaisePropertyChanged("Modules");
            }
        }
        
        public RelayCommand<TECIOModule> AddModuleCommand { get; private set; }
        public RelayCommand<TECIOModule> RemoveModuleCommand { get; private set; }
        public RelayCommand OptimizeModulesCommand { get; private set; }

        public ProvidedControllerPropertiesItem(TECProvidedController controller)
        {
            Controller = controller;
            AddModuleCommand = new RelayCommand<TECIOModule>(addModuleExecute, canAddModule);
            RemoveModuleCommand = new RelayCommand<TECIOModule>(removeModuleExecute, canRemoveModule);
            OptimizeModulesCommand = new RelayCommand(optimizeModulesExecute);
            populateIO();
            populateModules();

        }

        private void optimizeModulesExecute()
        {
            foreach(ModuleGroup item in Modules)
            {
                while (canRemoveModule(item.Module))
                {
                    removeModuleExecute(item.Module);
                }
            }
        }

        private void addModuleExecute(TECIOModule obj)
        {
            Controller.IOModules.Add(obj);
            populateModules();
            populateIO();
        }

        private bool canAddModule(TECIOModule arg)
        {
            return Controller.CanAddModule(arg);
        }

        private void removeModuleExecute(TECIOModule obj)
        {
            Controller.IOModules.Remove(obj);
            populateModules();
            populateIO();
        }

        private bool canRemoveModule(TECIOModule arg)
        {
            return Controller.CanRemoveModule(arg);
            
        }

        private void populateIO()
        {
            IO = Controller.IO.ToList();
            AvailableIO = Controller.AvailableIO.ToList();
        }
        private void populateModules()
        {
            Modules = new ObservableCollection<ModuleGroup>();
            foreach(TECIOModule module in Controller.Type.IOModules.Distinct())
            {
                Modules.Add(new ModuleGroup(module,
                    Controller.IOModules.Where(item => item == module).Count()));
            }
        }
    }

    public class ModuleGroup
    {
        public TECIOModule Module
        {
            get; private set;
        }
        public int Quantity
        {
            get; private set;
        }
        public ModuleGroup(TECIOModule module, int qty)
        {
            Module = module;
            Quantity = qty;
        }
    }
}
