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
    public class TECDevice : TECHardware, IDragDropable, ITECConnectable
    {
        #region Constants
        private const CostType COST_TYPE = CostType.TEC;
        #endregion

        #region Fields
        private ObservableCollection<TECElectricalMaterial> _connectionTypes;

        #endregion

        #region Properties
        virtual public ObservableCollection<TECElectricalMaterial> ConnectionTypes
        {
            get { return _connectionTypes; }
            set
            {
                if(ConnectionTypes != null)
                {
                    ConnectionTypes.CollectionChanged -= (sender, args) => ConnectionTypes_CollectionChanged(sender, args, "ConnectionTypes");
                }
                var old = ConnectionTypes;
                _connectionTypes = value; if (ConnectionTypes != null)
                {
                    ConnectionTypes.CollectionChanged += (sender, args) => ConnectionTypes_CollectionChanged(sender, args, "ConnectionTypes");
                }
                notifyCombinedChanged(Change.Edit, "ConnectionTypes", this, value, old);
            }
        }
        #endregion//Properties

        #region Constructors
        public TECDevice(Guid guid,
            ObservableCollection<TECElectricalMaterial> connectionTypes,
            TECManufacturer manufacturer) : base(guid, manufacturer, COST_TYPE)
        {
            _connectionTypes = connectionTypes;
            ConnectionTypes.CollectionChanged += (sender, args) => ConnectionTypes_CollectionChanged(sender, args, "ConnectionTypes");
        }
        public TECDevice(ObservableCollection<TECElectricalMaterial> connectionTypes,
            TECManufacturer manufacturer) : this(Guid.NewGuid(), connectionTypes, manufacturer) { }

        //Copy Constructor
        public TECDevice(TECDevice deviceSource)
            : this(deviceSource.Guid, deviceSource.ConnectionTypes, deviceSource.Manufacturer)
        {
            this.copyPropertiesFromHardware(deviceSource);
        }
        #endregion //Constructors

        #region Methods
        public new Object DragDropCopy(TECScopeManager scopeManager)
        {
            foreach(TECDevice device in scopeManager.Catalogs.Devices)
            {
                if(device.Guid == this.Guid)
                {
                    return device;
                }
            }
            throw new Exception();
        }

        private void ConnectionTypes_CollectionChanged(object sender, System.Collections.Specialized.NotifyCollectionChangedEventArgs e, string propertyName)
        {
            if (e.Action == System.Collections.Specialized.NotifyCollectionChangedAction.Add)
            {
                foreach (TECElectricalMaterial type in e.NewItems)
                {
                    notifyCombinedChanged(Change.Add, propertyName, this, type);
                }
            }
            else if (e.Action == System.Collections.Specialized.NotifyCollectionChangedAction.Remove)
            {
                foreach (TECElectricalMaterial type in e.OldItems)
                {
                    notifyCombinedChanged(Change.Remove, propertyName, this, type);
                }
            }
        }

        protected override SaveableMap saveObjects()
        {
            SaveableMap saveList = new SaveableMap();
            saveList.AddRange(base.saveObjects());
            saveList.AddRange(this.ConnectionTypes, "ConnectionTypes");
            return saveList;
        }
        protected override SaveableMap relatedObjects()
        {
            SaveableMap saveList = new SaveableMap();
            saveList.AddRange(base.relatedObjects());
            saveList.AddRange(this.ConnectionTypes, "ConnectionTypes");
            return saveList;
        }
        #endregion
    }
}
