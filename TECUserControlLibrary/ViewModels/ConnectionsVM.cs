using EstimatingLibrary;
using EstimatingLibrary.Interfaces;
using EstimatingLibrary.Utilities;
using GalaSoft.MvvmLight;
using GongSolutions.Wpf.DragDrop;
using NLog;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TECUserControlLibrary.Models;
using TECUserControlLibrary.Utilities;

namespace TECUserControlLibrary.ViewModels
{
    public class ConnectionsVM<T> : ViewModelBase, IDropTarget where T : IRelatable, ITECScope
    {
        private static Logger logger = LogManager.GetCurrentClassLogger();

        private readonly T root;
        private readonly ScopeGroup rootConnectableGroup;
        private readonly ScopeGroup rootControllersGroup;
        private readonly Func<ITECObject, bool> filterPredicate;

        private readonly List<TECController> allControllers;
        private readonly List<IConnectable> allConnectables;
        private ScopeGroup _selectedControllerGroup;
        private ScopeGroup _selectedConnectableGroup;
        private TECConnection _selectedConnection;
        private Double _defaultWireLength = 50.0;
        private Double _defaultConduitLength = 30.0;
        private TECElectricalMaterial _defaultConduitType;
        private bool _defaultPlenum = false;

        public ObservableCollection<ScopeGroup> Controllers { get; }
        public ObservableCollection<ScopeGroup> Connectables { get; }
        
        
        public ObservableCollection<ScopeGroup> Connectables
        {
            get
            {
                return rootConnectableGroup.ChildrenGroups;
            }
        }
        public ObservableCollection<ScopeGroup> Controllers
        {
            get
            {
                return rootControllersGroup.ChildrenGroups;
            }
        }

        public ScopeGroup SelectedControllerGroup
        {
            get { return _selectedControllerGroup; }
            set
            {
                _selectedControllerGroup = value;
                RaisePropertyChanged("SelectedControllerGroup");
                RaisePropertyChanged("SelectedController");
                Selected?.Invoke(value?.Scope as TECObject);
            }
        }
        public ScopeGroup SelectedConnectableGroup
        {
            get { return _selectedConnectableGroup; }
            set
            {
                _selectedConnectableGroup = value;
                RaisePropertyChanged("SelectedConnectableGroup");
                RaisePropertyChanged("SelectedConnectable");
                Selected?.Invoke(value?.Scope as TECObject);
            }
        }

        public TECController SelectedController
        {
            get { return SelectedControllerGroup?.Scope as TECController; }
        }
        public IConnectable SelectedConnectable
        {
            get { return SelectedConnectableGroup?.Scope as IConnectable; }
        }
        public TECConnection SelectedConnection
        {
            get { return _selectedConnection; }
            set
            {
                _selectedConnection = value;
                RaisePropertyChanged("SelectedConnection");
                Selected?.Invoke(value as TECObject);
            }
        }

        public Double DefaultWireLength
        {
            get { return _defaultWireLength; }
            set
            {
                _defaultWireLength = value;
                RaisePropertyChanged("DefaultWireLength");
            }
        }
        public Double DefaultConduitLength
        {
            get { return _defaultConduitLength; }
            set
            {
                _defaultConduitLength = value;
                RaisePropertyChanged("DefaultConduitLength");
            }
        }
        public TECElectricalMaterial DefaultConduitType
        {
            get { return _defaultConduitType; }
            set
            {
                _defaultConduitType = value;
                RaisePropertyChanged("DefaultConduitType");
            }
        }
        public bool DefaultPlenum
        {
            get { return _defaultPlenum; }
            set
            {
                _defaultPlenum = value;
                RaisePropertyChanged("DefaultPlenum");
            }
        }
        public TECCatalogs Catalogs { get; }
        
        public event Action<TECObject> Selected;

        /// <summary>
        /// 
        /// </summary>
        /// <param name="root"></param>
        /// <param name="watcher"></param>
        /// <param name="includeFilter">Predicate for "where" clause of direct children of root.</param>
        public ConnectionsVM(T root, ChangeWatcher watcher, TECCatalogs catalogs, Func<ITECObject, bool> filterPredicate = null)
        {
            if (filterPredicate == null)
            {
                filterPredicate = item => true;
            }
            this.filterPredicate = filterPredicate;

            this.root = root;
            this.Catalogs = catalogs;
            if(this.Catalogs.ConduitTypes.Count > 0)
            {
                this.DefaultConduitType = this.Catalogs.ConduitTypes[0];
            }

            watcher.InstanceChanged += parentChanged;

            this.rootConnectableGroup = new ScopeGroup(root);
            this.rootControllersGroup = new ScopeGroup(root);

            foreach(ITECObject obj in root.GetDirectChildren().Where(filterPredicate))
            {
                if (obj is TECScope scope)
                {
                    var childGroups = getGroups(scope);
                    if (childGroups.controllersGroup != null)
                    {
                        this.Controllers.Add(childGroups.controllersGroup);
                    }
                    if (childGroups.connectablesGroup != null)
                    {
                        this.Connectables.Add(childGroups.connectablesGroup);
                    }
                }
            }
        }

        /// <summary>
        /// Gets ScopeGroups constructed from the scope passed in which contain connectables and controllers.
        /// </summary>
        /// <param name="scope"></param>
        /// <returns>ScopeGroups condensed to only groups that contain connectables and controllers respectively.</returns>
        private static (ScopeGroup connectablesGroup, ScopeGroup controllersGroup) getGroups(ITECScope scope)
        {
            ScopeGroup connectablesGroup = null;
            ScopeGroup controllersGroup = null;

            if (scope is IConnectable)
            {
                connectablesGroup = new ScopeGroup(scope);
                if (scope is TECController)
                {
                    controllersGroup = new ScopeGroup(scope);
                }
            }
            else if (scope is IRelatable relatable)
            {
                foreach(TECObject child in relatable.GetDirectChildren())
                {
                    if (child is TECScope childScope)
                    {
                        var childGroup = getGroups(childScope);
                        if (childGroup.connectablesGroup != null)
                        {
                            if (connectablesGroup == null)
                            {
                                connectablesGroup = new ScopeGroup(scope);
                            }
                            connectablesGroup.Add(childGroup.connectablesGroup);
                            if (childGroup.controllersGroup != null)
                            {
                                if (controllersGroup == null)
                                {
                                    controllersGroup = new ScopeGroup(scope);
                                }
                                controllersGroup.Add(childGroup.controllersGroup);
                            }
                        }
                    }
                }
            }
            return (connectablesGroup, controllersGroup);
        }
        private static bool containsConnectable(ScopeGroup group)
        {
            if (group.Scope is IConnectable)
            {
                return true;
            }
            
            foreach(ScopeGroup childGroup in group.ChildrenGroups)
            {
                if (containsConnectable(childGroup))
                {
                    return true;
                }
            }
            return false;
        }

        private void parentChanged(TECChangedEventArgs obj)
        {
            if (obj.Value is IConnectable connectable)
            {
                if (obj.Change == Change.Add)
                {
                    if (obj.Sender is ITECScope scopeSender)
                    {
                        addConnectable(connectable, scopeSender);
                    }
                    else
                    {
                        logger.Error("Connectable added to {0}, Guid: {1}. Parent is not ITECScope.",
                            obj.Sender.GetType(), obj.Sender.Guid);
                    }
                }
                else if (obj.Change == Change.Remove)
                {
                    if (obj.Sender is ITECScope scopeSender)
                    {
                        removeConnectable(connectable, scopeSender);
                    }
                    else
                    {
                        logger.Error("Connectable removed from {0}, Guid: {1}. Parent is not ITECScope.",
                            obj.Sender.GetType(), obj.Sender.Guid);
                    }
                }
            }
        }

        private void addConnectable(IConnectable connectable, ITECScope parent)
        {
            ScopeGroup parentConnectableGroup = this.rootConnectableGroup.GetGroup(parent);
            if (parentConnectableGroup != null)
            {
                parentConnectableGroup.Add(connectable);
            }
            else
            {
                fillGroups(this.Connectables, connectable);
            }
        }
        private void removeConnectable(IConnectable connectable, ITECScope parent)
        {
            ScopeGroup parentGroup = this.rootConnectableGroup.GetGroup(parent);
            parentGroup.Remove(connectable);

            List<ITECObject> parentPath = root.GetObjectPath(parent);

            ScopeGroup currentGroup = this.rootConnectableGroup;
            for(int i = 1; i < parentPath.Count; i++)
            {
                if (parentPath[i] is ITECScope scope)
                {
                    ScopeGroup nextGroup = currentGroup.ChildrenGroups.First(group => group.Scope == parentPath[i + 1]);
                    if (!containsConnectable(nextGroup))
                    {
                        currentGroup.Remove(nextGroup);
                        return;
                    }
                    else
                    {
                        currentGroup = nextGroup;
                    }
                }
            }
        }

        private void fillGroups(IEnumerable<ScopeGroup> groups, IConnectable connectable)
        {
            if (!root.IsDirectDescendant(connectable))
            {
                throw new Exception("New connectable doesn't exist in root object.");
            }

            List<ITECObject> path = this.root.GetObjectPath(connectable);

            //Optimization Idea: Use FindGroup on each ITECObject in path in reverse until finding one. Fill from there.

            ScopeGroup currentGroup = new ScopeGroup(this.root);
            groups.ForEach(group => currentGroup.Add(group));

            int nextIndex = 1;
            while (currentGroup.Scope != connectable)
            {
                bool scopeFound = false;
                foreach(ScopeGroup group in currentGroup.ChildrenGroups)
                {
                    if (group.Scope == path[nextIndex])
                    {
                        currentGroup = group;
                        scopeFound = true;
                        break;
                    }
                }

                if (!scopeFound)
                {
                    if (path[nextIndex] is ITECScope nextScope)
                    {
                        currentGroup.Add(nextScope);
                    }
                    else
                    {
                        logger.Error("Object in path to connectable isn't ITECScope, cannot build group hierarchy.");
                        return;
                    }
                }
                nextIndex++;
            }
        }

        public void DragOver(IDropInfo dropInfo)
        {
            if(dropInfo.Data is ScopeGroup group && SelectedController != null)
            {
                if(SelectedController.CanConnect(group.Scope as IConnectable))
                {
                    UIHelpers.SetDragAdorners(dropInfo);
                }
            }
        }
        public void Drop(IDropInfo dropInfo)
        {
            var connection = SelectedController.Connect(((ScopeGroup)dropInfo.Data).Scope as IConnectable);
            connection.Length = this.DefaultWireLength;
            connection.ConduitType = this.DefaultConduitType;
            connection.ConduitLength = this.DefaultConduitLength;
            connection.IsPlenum = this.DefaultPlenum;
        }


    }
}
