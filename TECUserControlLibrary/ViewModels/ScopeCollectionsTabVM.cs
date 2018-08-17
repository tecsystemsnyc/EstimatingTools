using EstimatingLibrary;
using EstimatingLibrary.Interfaces;
using EstimatingUtilitiesLibrary;
using GalaSoft.MvvmLight;
using GalaSoft.MvvmLight.CommandWpf;
using GongSolutions.Wpf.DragDrop;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Windows.Input;
using TECUserControlLibrary.Utilities;

namespace TECUserControlLibrary.ViewModels
{
    public class ScopeCollectionsTabVM : ViewModelBase, IDropTarget
    {
        #region Properties
        public TECScopeTemplates Templates
        {
            get { return _templates; }
            set
            {
                _templates = value;
                RaisePropertyChanged("Templates");
            }
        }
        public ObservableCollection<ITECObject> ResultCollection
        {
            get { return _resultCollection; }
            private set
            {
                _resultCollection = value;
                RaisePropertyChanged("ResultCollection");
            }

        }
        public AllSearchableObjects ChosenType
        {
            get { return _chosenType; }
            set
            {
                _chosenType = value;
                RaisePropertyChanged("ChosenType");
            }
        }
        public Dictionary<AllSearchableObjects, String> CollectionTypes {get; private set;}
        
        private ObservableCollection<ITECObject> _resultCollection;
        private TECScopeTemplates _templates;
        private AllSearchableObjects _chosenType;
        private TECCatalogs catalogs;

        public ICommand SearchCollectionCommand { get; private set; }
        public ICommand EndSearchCommand { get; private set; }

        #region Delegates
        public Action<IDropInfo> DragHandler;
        public Action<IDropInfo> DropHandler;
        #endregion
        
        #region Search
        private string _searchString;

        public string SearchString
        {
            get { return _searchString; }
            set
            {
                _searchString = value;
                RaisePropertyChanged("SearchString");
            }
        }
        #endregion
        #endregion
        
        public ScopeCollectionsTabVM(TECScopeManager manager)
        {
            Templates = manager.Templates;
            this.catalogs = manager.Catalogs; 
            SearchCollectionCommand = new RelayCommand(SearchCollectionExecute, SearchCanExecute);
            EndSearchCommand = new RelayCommand(EndSearchExecute);
            SearchString = "";
            CollectionTypes = new Dictionary<AllSearchableObjects, string>(UIHelpers.SearchSelectorList);
            SearchCollectionExecute();
        }

        #region Methods
        
        public void OmitCollections(List<AllSearchableObjects> ommisions)
        {
            foreach(var item in ommisions)
            {
                CollectionTypes.Remove(item);
            }
            RaisePropertyChanged("CollectionTypes");
            ChosenType = CollectionTypes.First().Key;
            SearchCollectionExecute();
        }

        #region Commands
        private void SearchCollectionExecute()
        {
            switch (ChosenType)
            {
                case AllSearchableObjects.System:
                    ResultCollection = getResultCollection(Templates.SystemTemplates, SearchString);
                    break;
                case AllSearchableObjects.Equipment:
                    ResultCollection = getResultCollection(Templates.EquipmentTemplates, SearchString);
                    break;
                case AllSearchableObjects.SubScope:
                    ResultCollection = getResultCollection(Templates.SubScopeTemplates, SearchString);
                    break;
                case AllSearchableObjects.Devices:
                    ResultCollection = getResultCollection(catalogs.Devices, SearchString);
                    break;
                case AllSearchableObjects.AssociatedCosts:
                    ResultCollection = getResultCollection(catalogs.AssociatedCosts, SearchString);
                    break;
                case AllSearchableObjects.MiscCosts:
                    ResultCollection = getResultCollection(Templates.MiscCostTemplates.Where(x => x.Type == CostType.TEC), SearchString);
                    break;
                case AllSearchableObjects.MiscWiring:
                    ResultCollection = getResultCollection(Templates.MiscCostTemplates.Where(x => x.Type == CostType.Electrical), SearchString);
                    break;
                case AllSearchableObjects.ControllerTypes:
                    ResultCollection = getResultCollection(catalogs.ControllerTypes, SearchString);
                    break;
                case AllSearchableObjects.PanelTypes:
                    ResultCollection = getResultCollection(catalogs.PanelTypes, SearchString);
                    break;
                case AllSearchableObjects.Tags:
                    ResultCollection = getResultCollection(catalogs.Tags, SearchString);
                    break;
                case AllSearchableObjects.Wires:
                    ResultCollection = getResultCollection(catalogs.ConnectionTypes, SearchString);
                    break;
                case AllSearchableObjects.Conduits:
                    ResultCollection = getResultCollection(catalogs.ConduitTypes, SearchString);
                    break;
                case AllSearchableObjects.Valves:
                    ResultCollection = getResultCollection(catalogs.Valves, SearchString);
                    break;
                case AllSearchableObjects.IOModules:
                    ResultCollection = getResultCollection(catalogs.IOModules, SearchString);
                    break;
                case AllSearchableObjects.Protocols:
                    ResultCollection = getResultCollection(catalogs.Protocols, SearchString);
                    break;
                default:
                    break;

            }
        }
        private bool SearchCanExecute()
        {
            return true;
        }
        private void EndSearchExecute()
        {
            SearchString = "";
            SearchCollectionExecute();
        }
        #endregion
        
        public void DragOver(IDropInfo dropInfo)
        {
            DragHandler(dropInfo);
        }
        public void Drop(IDropInfo dropInfo)
        {
            DropHandler(dropInfo);
        }
        
        private ObservableCollection<ITECObject> getResultCollection(IEnumerable<ITECObject> source, string searchString)
        {
            return new ObservableCollection<ITECObject>(source.GetSearchResult(searchString));
        }
        #endregion
        
    }
}
