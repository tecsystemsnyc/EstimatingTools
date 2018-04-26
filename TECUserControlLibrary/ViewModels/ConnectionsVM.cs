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
    public class ConnectionsVM : ViewModelBase, IDropTarget
    {
        private static Logger logger = LogManager.GetCurrentClassLogger();

        private readonly IRelatable root;

        private readonly List<TECController> allControllers;
        private readonly List<IConnectable> allConnectables;
        private ScopeGroup _selectedControllerGroup;
        private ScopeGroup _selectedConnectableGroup;
        private TECConnection _selectedConnection;

        public ObservableCollection<ScopeGroup> Controllers { get; }
        public ObservableCollection<ScopeGroup> Connectables { get; }
        
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

        public event Action<TECObject> Selected;

        /// <summary>
        /// 
        /// </summary>
        /// <param name="root"></param>
        /// <param name="watcher"></param>
        /// <param name="includeFilter">Predicate for "where" clause of direct children of root.</param>
        public ConnectionsVM(IRelatable root, ChangeWatcher watcher, Func<ITECObject, bool> includeFilter = null)
        {
            if (includeFilter == null)
            {
                includeFilter = item => true;
            }

            this.root = root;

            watcher.InstanceChanged += parentChanged;

            this.Controllers = new ObservableCollection<ScopeGroup>();
            this.Connectables = new ObservableCollection<ScopeGroup>();

            foreach(ITECObject obj in root.GetDirectChildren().Where(includeFilter))
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
        private static (ScopeGroup connectablesGroup, ScopeGroup controllersGroup) getGroups(TECScope scope)
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
        private static ScopeGroup findGroup(ITECScope scope, IEnumerable<ScopeGroup> groups)
        {
            foreach(ScopeGroup group in groups)
            {
                if (group.Scope == scope)
                {
                    return group;
                }

                ScopeGroup childGroup = findGroup(scope, group.ChildrenGroups);
                if (childGroup != null)
                {
                    return childGroup;
                }
            }

            return null;
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
            ScopeGroup parentConnectableGroup = findGroup(parent, this.Connectables);
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

        }
        
        private void fillGroups(IEnumerable<ScopeGroup> groups, IConnectable connectable)
        {
            foreach(ScopeGroup group in groups)
            {

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
            SelectedController.Connect(((ScopeGroup)dropInfo.Data).Scope as IConnectable);
        }
    }
}
