using EstimatingLibrary;
using EstimatingLibrary.Interfaces;
using EstimatingLibrary.Utilities;
using GalaSoft.MvvmLight;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TECUserControlLibrary.Models;

namespace TECUserControlLibrary.ViewModels
{
    public class ConnectionsVM : ViewModelBase
    {
        private readonly IRelatable parent;

        private readonly List<TECController> allControllers;
        private readonly List<IConnectable> allConnectables;

        public ObservableCollection<ScopeGroup> Controllers { get; }
        public ObservableCollection<ScopeGroup> Connectables { get; }

        public ConnectionsVM(IRelatable parent, ChangeWatcher watcher)
        {
            this.parent = parent;

            watcher.InstanceChanged += parentChanged;

            this.Controllers = new ObservableCollection<ScopeGroup>();
            this.Connectables = new ObservableCollection<ScopeGroup>();

            foreach(TECObject obj in parent.GetDirectChildren())
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

        private void parentChanged(TECChangedEventArgs obj)
        {
            if (obj.Change == Change.Add)
            {
                if (obj.Value is IConnectable connectable)
                {

                }
            }
            else if (obj.Change == Change.Remove)
            {

            }
        }

        private void addConnectable(IConnectable connectable, TECScope parent)
        {

        }
    }
}
