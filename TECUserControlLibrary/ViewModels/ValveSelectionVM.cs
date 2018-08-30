using EstimatingLibrary;
using EstimatingLibrary.Interfaces;
using EstimatingUtilitiesLibrary;
using GalaSoft.MvvmLight;
using GalaSoft.MvvmLight.CommandWpf;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;
using TECUserControlLibrary.Models;

namespace TECUserControlLibrary.ViewModels
{
    public class ValveSelectionVM : ViewModelBase
    {
        private List<TECValve> catalog { get; }
        private ValveScopeItem _selectedValve;
        private List<TECValve> _results;
        
        public List<ValveScopeItem> Valves { get; }
        public ValveScopeItem SelectedValve
        {
            get { return _selectedValve; }
            set
            {
                _selectedValve = value;
                RaisePropertyChanged("SelectedValve");
            }
        }
        public List<TECValve> Results
        {
            get { return _results; }
            set
            {
                _results = value;
                RaisePropertyChanged("Results");
            }
        }


        private double _searchSize = 0;

        public double SearchSize
        {
            get { return _searchSize; }
            set
            {
                _searchSize = value;
                RaisePropertyChanged("SearchSize");
            }
        }

        private double _searchCv = 0.0;

        public double SearchCv
        {
            get { return _searchCv; }
            set
            {
                _searchCv = value;
                RaisePropertyChanged("SearchCv");
            }
        }

        private String _searchStyle = "";

        public String SearchStyle
        {
            get { return _searchStyle; }
            set
            {
                _searchStyle = value;
                RaisePropertyChanged("SearchStyle");
            }
        }

        private double _searchPressure = 0;

        public double SearchPressure
        {
            get { return _searchPressure; }
            set
            {
                _searchPressure = value;
                RaisePropertyChanged("SearchPressure");
            }
        }

        public ICommand ReplaceValveCommand { get; private set; } 
        public ICommand SearchCatalogCommand { get; private set; }
        public ICommand ResetCatalogCommand { get; private set; }

        public ValveSelectionVM(TECSystem system, IEnumerable<TECValve> valveCatalog)
        {
            catalog = new List<TECValve>(valveCatalog);
            Valves = getValveItems(system);
            Results = new List<TECValve>(catalog);
            ReplaceValveCommand = new RelayCommand<TECValve>(replaceValveExecute, canReplaceValve);
            SearchCatalogCommand = new RelayCommand(searchCatalogExecute, canSearchCatalog);
            ResetCatalogCommand = new RelayCommand(resetCatalogExecute, canResetCatalog);
        }

        private void resetCatalogExecute()
        {
            Results = new List<TECValve>(catalog);
            SearchCv = 0;
            SearchStyle = "";
            SearchSize = 0;
            SearchPressure = 0;
        }

        private bool canResetCatalog()
        {
            return Results.Count != catalog.Count;
        }

        private void searchCatalogExecute()
        {
            List<TECValve> results = new List<TECValve>();
            foreach(TECValve valve in catalog)
            {
                bool hasStyle = SearchStyle == "" || valve.Style.ToUpper() == SearchStyle.ToUpper();
                bool hasCv = SearchCv == 0.0 || valve.Cv >= SearchCv;
                bool hasSize = SearchSize == 0.0 || valve.Size == SearchSize;
                bool hasPressure = SearchPressure == 0.0 || valve.PressureRating > SearchPressure;
                if(hasStyle && hasCv && hasSize && hasPressure)
                {
                    results.Add(valve);
                }
            }
            Results = results;
        }

        private bool canSearchCatalog()
        {
            return (SearchStyle != "" || SearchSize != 0.0 || SearchCv != 0.0 || SearchPressure != 0.0);
        }

        private void replaceValveExecute(TECValve obj)
        {
            SelectedValve.SubScope.RemoveCatalogItem<IEndDevice>(SelectedValve.Valve, obj);
            SelectedValve.Valve = obj;
        }

        private bool canReplaceValve(TECValve arg)
        {
            if (arg == null || SelectedValve?.Valve == null) { return false; }
            return SelectedValve.SubScope.CanChangeDevice(SelectedValve.Valve, arg);
        }

        private List<ValveScopeItem> getValveItems(TECSystem system)
        {
            List<ValveScopeItem> valveItems = new List<ValveScopeItem>();
            foreach(TECEquipment equipment in system.Equipment)
            {
                foreach(TECSubScope subScope in equipment.SubScope)
                {
                    foreach(IEndDevice device in subScope.Devices)
                    {
                        if(device is TECValve valve)
                        {
                            valveItems.Add(new ValveScopeItem(equipment, subScope, valve));
                        }
                    }
                }
            }
            return valveItems;
        }
    }
}
