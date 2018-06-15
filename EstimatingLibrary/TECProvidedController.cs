﻿using EstimatingLibrary.Interfaces;
using EstimatingLibrary.Utilities;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EstimatingLibrary
{
    public class TECProvidedController : TECController, IDDCopiable
    {
        #region Properties
        private TECControllerType _type;
        private ObservableCollection<TECIOModule> _ioModules = new ObservableCollection<TECIOModule>();

        public TECControllerType Type
        {
            get { return _type; }
            set
            {
                var old = Type;
                _type = value;
                notifyCombinedChanged(Change.Edit, "Type", this, value, old);
                notifyCostChanged(value.CostBatch - old.CostBatch);
            }
        }
        public ObservableCollection<TECIOModule> IOModules
        {
            get { return _ioModules; }
            set
            {
                var old = IOModules;
                IOModules.CollectionChanged -= handleModulesChanged;
                _ioModules = value;
                IOModules.CollectionChanged += handleModulesChanged;
                notifyCombinedChanged(Change.Edit, "IOModules", this, value, old);
            }
        }

        public override IOCollection IO
        {
            get
            {
                IOCollection allIO = new IOCollection(this.Type.IO);
                List<TECIO> moduleIO = new List<TECIO>();
                this.IOModules.ForEach(x => moduleIO.AddRange(x.IO));
                allIO.Add(moduleIO);
                return allIO;
            }
        }
        #endregion

        #region Constructors
        public TECProvidedController(Guid guid, TECControllerType type, bool isTypical) : base(guid, isTypical)
        {
            _type = type;
            this.IOModules.CollectionChanged += handleModulesChanged;
        }
        public TECProvidedController(TECControllerType type, bool isTypical) : this(Guid.NewGuid(), type, isTypical) { }
        public TECProvidedController(TECProvidedController controllerSource, bool isTypical, Dictionary<Guid, Guid> guidDictionary = null) : base(controllerSource, isTypical, guidDictionary)
        {
            this._type = controllerSource.Type;
            foreach (TECIOModule module in controllerSource.IOModules)
            {
                this.IOModules.Add(module);
            }
        }
        #endregion

        #region Methods
        public override TECController CopyController(bool isTypical, Dictionary<Guid, Guid> guidDictionary)
        {
            return new TECProvidedController(this, isTypical, guidDictionary);
        }

        #region Module Methods
        public bool CanAddModule(TECIOModule module)
        {
            return (this.Type.IOModules.Count(mod => (mod == module)) >
                this.IOModules.Count(mod => (mod == module)));
        }
        public void AddModule(TECIOModule module)
        {
            if (CanAddModule(module))
            {
                IOModules.Add(module);
            }
            else
            {
                throw new InvalidOperationException("Controller can't accept IOModule.");
            }
        }

        private List<TECIOModule> getPotentialModules()
        {
            List<TECIOModule> modules = new List<TECIOModule>(this.Type.IOModules);
            foreach (TECIOModule module in this.IOModules)
            {
                modules.Remove(module);
            }
            return modules;
        }
        /// <summary>
        /// Gets nessessary modules from potential modules to handle io.
        /// </summary>
        /// <param name="io"></param>
        /// <returns>Nessessary modules. Returns empty list if no collection of modules exists.</returns>
        private List<TECIOModule> getModulesForIO(IOCollection io)
        {
            IOCollection nessessaryIO = io - (io | AvailableIO);

            List<TECIOModule> potentialModules = getPotentialModules();

            //Check that any singular module can cover the nessessary io
            foreach (TECIOModule module in potentialModules)
            {
                if (module.IOCollection.Contains(io)) return new List<TECIOModule>() { module };
            }

            //List of modules to return
            List<TECIOModule> returnModules = new List<TECIOModule>();
            foreach (TECIO type in io.ToList())
            {
                TECIO singularIO = new TECIO(type);
                singularIO.Quantity = 1;

                //List of remaining potential modules after return modules is considered
                List<TECIOModule> newPotentialModules = new List<TECIOModule>(potentialModules);
                foreach (TECIOModule module in returnModules)
                {
                    newPotentialModules.Remove(module);
                }

                //Add the first module that contains the IOType we're checking
                foreach (TECIOModule module in newPotentialModules)
                {
                    if (module.IOCollection.Contains(singularIO))
                    {
                        returnModules.Add(module);
                        break;
                    }
                }

                //If return modules satisfies our IO, return them
                if (returnModules.ToIOCollection().Contains(io))
                {
                    return returnModules;
                }
            }

            return new List<TECIOModule>();
        }
        #endregion

        public bool CanChangeType(TECControllerType newType)
        {
            if (newType == null) return false;
            TECProvidedController possibleController = new TECProvidedController(newType, false);
            IOCollection necessaryIO = this.UsedIO;
            IOCollection possibleIO = possibleController.getPotentialIO() + possibleController.AvailableIO;
            return possibleIO.Contains(necessaryIO);
        }
        public void ChangeType(TECControllerType newType)
        {
            if (CanChangeType(newType))
            {
                this.IOModules.ObservablyClear();
                this.Type = newType;
                ModelCleanser.addRequiredIOModules(this);
            }
            else
            {
                return;
            }
        }

        protected override CostBatch getCosts()
        {
            if (!IsTypical)
            {
                CostBatch costs = base.getCosts();
                costs += Type.CostBatch;
                foreach (TECIOModule module in IOModules)
                {
                    costs += module.CostBatch;
                }
                return costs;
            }
            else
            {
                return new CostBatch();
            }
        }
        protected override SaveableMap propertyObjects()
        {
            SaveableMap saveList = base.propertyObjects();
            saveList.AddRange(this.IOModules, "IOModules");
            saveList.Add(this.Type, "Type");
            return saveList;
        }
        protected override SaveableMap linkedObjects()
        {
            SaveableMap saveList = base.linkedObjects();
            saveList.AddRange(this.IOModules, "IOModules");
            saveList.Add(this.Type, "Type");
            return saveList;
        }

        private IOCollection getPotentialIO()
        {
            IOCollection potentialIO = new IOCollection();
            foreach (TECIOModule module in Type.IOModules)
            {
                foreach (TECIO io in module.IO)
                {
                    potentialIO.Add(io);
                }
            }
            foreach (TECIOModule module in IOModules)
            {
                foreach (TECIO io in module.IO)
                {
                    potentialIO.Remove(io);
                }
            }
            return potentialIO;
        }

        private void handleModulesChanged(Object sender, System.Collections.Specialized.NotifyCollectionChangedEventArgs e)
        {
            collectionChanged(sender, e, "IOModules");
        }
        #endregion

        #region Interfaces
        #region IDDCopiable
        Object IDDCopiable.DragDropCopy(TECScopeManager scopeManager)
        {
            var outController = new TECProvidedController(this, this.IsTypical);
            ModelLinkingHelper.LinkScopeItem(outController, scopeManager);
            return outController;
        }
        #endregion
        #endregion
    }
}