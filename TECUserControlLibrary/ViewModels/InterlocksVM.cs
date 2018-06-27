using EstimatingLibrary;
using EstimatingLibrary.Interfaces;
using EstimatingLibrary.Utilities;
using EstimatingLibrary.Utilities.WatcherFilters;
using GalaSoft.MvvmLight;
using GalaSoft.MvvmLight.CommandWpf;
using NLog;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TECUserControlLibrary.Models;
using TECUserControlLibrary.Utilities.DropTargets;

namespace TECUserControlLibrary.ViewModels
{
    public class InterlocksVM : ViewModelBase
    {
        private static Logger logger = LogManager.GetCurrentClassLogger();

        private readonly Func<ITECObject, bool> filterPredicate;
        private readonly IRelatable root;

        private TECElectricalMaterial noneConduit;
        private double _length = 0.0;
        private double _conduitLength = 0.0;
        private TECElectricalMaterial _conduitType;
        private bool _isPlenum = false;
        private string _name = "";

        private readonly ScopeGroup rootInterlockablesGroup;

        private ScopeGroup _selectedInterlockableGroup;
        public ScopeGroup SelectedInterlockableGroup
        {
            get { return _selectedInterlockableGroup; }
            set
            {
                _selectedInterlockableGroup = value;
                RaisePropertyChanged("SelectedInterlockableGroup");
                RaisePropertyChanged("SelectedInterlockable");
                Selected?.Invoke(value?.Scope as TECObject);
            }
        }
        public IInterlockable SelectedInterlockable
        {
            get { return SelectedInterlockableGroup?.Scope as IInterlockable; }
        }
        public ObservableCollection<ScopeGroup> Interlockables
        {
            get
            {
                return rootInterlockablesGroup.ChildrenGroups;
            }
        }
        
        public RelayCommand AddInterlockCommand { get; private set; }
        public RelayCommand<TECInterlockConnection> DeleteInterlockCommand { get; private set; }

        public ObservableCollection<TECConnectionType> ConnectionTypes { get; } 
            = new ObservableCollection<TECConnectionType>();
        public List<TECElectricalMaterial> ConduitTypes { get; }

        public double Length
        {
            get { return _length; }
            set
            {
                _length = value;
                RaisePropertyChanged("Length");
            } 
        }
        public double ConduitLength
        {
            get { return _conduitLength; }
            set
            {
                _conduitLength = value;
                RaisePropertyChanged("ConduitLength");
            }
        }
        public TECElectricalMaterial ConduitType
        {
            get { return _conduitType; }
            set
            {
                _conduitType = value;
                RaisePropertyChanged("ConduitType");
            }
        } 
        public bool IsPlenum
        {
            get { return _isPlenum; }
            set
            {
                _isPlenum = value;
                RaisePropertyChanged("IsPlenum");
            }
        }
        public string Name
        {
            get { return _name; }
            set
            {
                _name = value;
                RaisePropertyChanged("Name");
            }
        }

        public CatalogDropTarget CatalogDropHandler { get; } = new CatalogDropTarget();

        public event Action<TECObject> Selected;
        
        public InterlocksVM(IRelatable root, ChangeWatcher watcher, 
            TECCatalogs catalogs, Func<ITECObject, bool> filterPredicate = null)
        {

            if (filterPredicate == null)
            {
                filterPredicate = item => true;
            }
            this.filterPredicate = filterPredicate;

            this.root = root;
            new DirectRelationshipChangedFilter(watcher).DirectRelationshipChanged += parentScopeChanged;


            noneConduit = new TECElectricalMaterial();
            noneConduit.Name = "None";
            this.rootInterlockablesGroup = new ScopeGroup("root");
            this.AddInterlockCommand = new RelayCommand(addInterlockExecute, canAddInterlock);
            this.DeleteInterlockCommand = new RelayCommand<TECInterlockConnection>(deleteInterlockExecute, canDeleteInterlock);
            this.ConduitTypes = new List<TECElectricalMaterial>(catalogs.ConduitTypes);
            this.ConduitTypes.Add(noneConduit);
            this.ConduitType = noneConduit;

            repopulateGroups(root, addInterlockable);

        }

        private void deleteInterlockExecute(TECInterlockConnection interlock)
        {
            SelectedInterlockable.Interlocks.Remove(interlock);
        }

        private bool canDeleteInterlock(TECInterlockConnection interlock)
        {
            return interlock != null 
                && SelectedInterlockable != null 
                && SelectedInterlockable.Interlocks.Contains(interlock);
        }

        private void addInterlockExecute()
        {
            var typical = SelectedInterlockable as ITypicalable;
            TECInterlockConnection connection = new TECInterlockConnection(ConnectionTypes);
            connection.Name = Name;
            connection.Length = Length;
            if(ConduitType != noneConduit) { connection.ConduitType = ConduitType; }
            connection.ConduitLength = ConduitLength;
            connection.IsPlenum = IsPlenum;

            SelectedInterlockable.Interlocks.Add(connection);
        }
        private bool canAddInterlock()
        {
            return ConnectionTypes.Count != 0;
        }
        
        private void repopulateGroups(ITECObject item, Action<ScopeGroup, IInterlockable> action)
        {
            if (item is IInterlockable connectable)
            {
                action(this.rootInterlockablesGroup, connectable);
            }
            else if (item is IRelatable relatable)
            {
                foreach (ITECObject child in relatable.GetDirectChildren().Where(filterPredicate))
                {
                    repopulateGroups(child, action);
                }
            }
        }

        private void parentScopeChanged(TECChangedEventArgs obj)
        {
            if (obj.Value is ITECObject tecObj)
            {
                if (obj.Change == Change.Add)
                {
                    repopulateGroups(tecObj, addInterlockable);
                }
                else if (obj.Change == Change.Remove)
                {
                    repopulateGroups(tecObj, removeInterloackable);
                }
            }
        }
        private void addInterlockable(ScopeGroup rootGroup, IInterlockable interlockable)
        {
            if (!filterPredicate(interlockable)) return;
            bool isDescendant = root.IsDirectDescendant(interlockable);
            if (!root.IsDirectDescendant(interlockable))
            {
                logger.Error("New connectable doesn't exist in root object.");
                return;
            }

            List<ITECObject> path = this.root.GetObjectPath(interlockable);

            ScopeGroup lastGroup = rootGroup;
            int lastIndex = 0;

            for (int i = path.Count - 1; i > 0; i--)
            {
                if (path[i] is ITECScope scope)
                {
                    ScopeGroup group = rootGroup.GetGroup(scope);

                    if (group != null)
                    {
                        lastGroup = group;
                        lastIndex = i;
                        break;
                    }
                }
                else
                {
                    logger.Error("Object in path to interlockable isn't ITECScope, cannot build group hierarchy.");
                    return;
                }
            }

            ScopeGroup currentGroup = lastGroup;
            for (int i = lastIndex + 1; i < path.Count; i++)
            {
                if (path[i] is ITECScope scope)
                {
                    currentGroup = currentGroup.Add(scope);
                }
                else
                {
                    logger.Error("Object in path to interlockable isn't ITECScope, cannot build group hierarchy.");
                    return;
                }
            }
        }
        private void removeInterloackable(ScopeGroup rootGroup, IInterlockable interlockable)
        {
            if (!filterPredicate(interlockable)) return;
            List<ScopeGroup> path = rootGroup.GetPath(interlockable as ITECScope);

            path[path.Count - 2].Remove(path.Last());

            ScopeGroup parentGroup = rootGroup;
            for (int i = 1; i < path.Count; i++)
            {
                if (!containsInterlockable(path[i]))
                {
                    parentGroup.Remove(path[i]);
                    return;
                }
                else
                {
                    parentGroup = path[i];
                }
            }
        }
        private bool containsInterlockable(ScopeGroup group)
        {
            if (group.Scope is IInterlockable)
            {
                return true;
            }

            foreach (ScopeGroup childGroup in group.ChildrenGroups)
            {
                if (containsInterlockable(childGroup))
                {
                    return true;
                }
            }
            return false;
        }
        
    }
}
