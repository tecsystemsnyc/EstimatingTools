﻿using EstimatingLibrary.Interfaces;
using EstimatingLibrary.Utilities;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;

namespace EstimatingLibrary
{
    public class TECValve: TECHardware, IEndDevice, ICatalog<TECValve>, ICatalogContainer
    {
        #region Constants
        private const CostType COST_TYPE = CostType.TEC;
        #endregion

        #region Fields
        private TECDevice _actuator;
        private double _cv;
        private double _size;
        private string _style;
        private double _pressureRating;
        #endregion

        #region Properties
        public TECDevice Actuator
        {
            get { return _actuator; }
            set
            {
                var old = Actuator;
                _actuator = value;
                notifyCombinedChanged(Change.Edit, "Actuator", this, _actuator, old);
                notifyCostChanged(value.CostBatch - old.CostBatch);
                raisePropertyChanged("Price");
                raisePropertyChanged("Cost");
                raisePropertyChanged("Labor");
            }
        }
        public double Cv
        {
            get { return _cv; }
            set
            {
                var old = _cv;
                _cv = value;
                notifyCombinedChanged(Change.Edit, "Cv", this, _cv, old);
            }
        }
        public double Size
        {
            get { return _size; }
            set
            {
                var old = _size;
                _size = value;
                notifyCombinedChanged(Change.Edit, "Size", this, _size, old);
            }
        }
        public string Style
        {
            get { return _style; }
            set
            {
                var old = _style;
                _style = value;
                notifyCombinedChanged(Change.Edit, "Style", this, _style, old);
            }
        }
        public double PressureRating
        {
            get { return _pressureRating; }
            set
            {
                var old = _pressureRating;
                _pressureRating = value;
                notifyCombinedChanged(Change.Edit, "PressureRating", this, _pressureRating, old);
            }
        }

        #endregion

        public TECValve(TECManufacturer manufacturer, TECDevice actuator) : this (Guid.NewGuid(), manufacturer, actuator) {}
        public TECValve(Guid guid, TECManufacturer manufacturer, TECDevice actuator) : base(guid, manufacturer, COST_TYPE)
        {
            _style = "";
            _size = 0;
            _cv = 0;
            _actuator = actuator;
        }
        public TECValve(TECValve valveSource) : this(valveSource.Manufacturer, valveSource.Actuator)
        {
            this.copyPropertiesFromHardware(valveSource);

        }
        protected override RelatableMap propertyObjects()
        {
            RelatableMap saveList = new RelatableMap();
            saveList.AddRange(base.propertyObjects());
            saveList.Add(this.Actuator, "Actuator");
            return saveList;
        }
        protected override RelatableMap linkedObjects()
        {
            RelatableMap saveList = new RelatableMap();
            saveList.AddRange(base.linkedObjects());
            saveList.Add(this.Actuator, "Actuator");
            return saveList;
        }

        public TECValve CatalogCopy()
        {
            return new TECValve(this);
        }

        protected override CostBatch getCosts()
        {
            return base.getCosts() + Actuator.CostBatch;
        }

        #region IEndDevice
        public List<IProtocol> ConnectionMethods => ((IEndDevice)Actuator).ConnectionMethods;

        public ObservableCollection<TECConnectionType> HardwiredConnectionTypes => ((IEndDevice)Actuator).HardwiredConnectionTypes;

        public ObservableCollection<TECProtocol> PossibleProtocols => ((IEndDevice)Actuator).PossibleProtocols;
        #endregion

        #region ICatalogContainer
        public override bool RemoveCatalogItem<T>(T item, T replacement)
        {
            bool alreadyRemoved = base.RemoveCatalogItem(item, replacement);

            bool replacedActuator = false;
            if (item == this.Actuator)
            {
                if (replacement is TECDevice newActuator)
                {
                    this.Actuator = newActuator;
                    replacedActuator = true;
                }
                else throw new ArgumentNullException("Replacement Actuator cannot be null.");
            }
            return (replacedActuator || alreadyRemoved);
        }
        #endregion
    }
}
