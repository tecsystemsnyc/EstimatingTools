using EstimatingLibrary.Interfaces;
using EstimatingLibrary.Utilities;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;

namespace EstimatingLibrary
{

    abstract public class TECConnection : TECObject, INotifyCostChanged, IRelatable, IConnection
    {
        #region Properties
        protected double _length = 0;
        protected double _conduitLength = 0;
        protected TECElectricalMaterial _conduitType;
        protected bool _isPlenum = false;

        public double Length
        {
            get { return _length; }
            set
            {
                var old = Length;
                var originalCost = this.CostBatch;
                _length = value;
                notifyCombinedChanged(Change.Edit, "Length", this, value, old);
                notifyCostChanged(CostBatch - originalCost);
            }
        }
        public double ConduitLength
        {
            get { return _conduitLength; }
            set
            {
                var old = ConduitLength;
                _conduitLength = value;
                notifyCombinedChanged(Change.Edit, "ConduitLength", this, value, old);
                CostBatch previous = ConduitType != null ? ConduitType.GetCosts(old) : new CostBatch();
                CostBatch current = ConduitType != null ? ConduitType.GetCosts(value) : new CostBatch();
                notifyCostChanged(current - previous);
            }
        }
        public TECElectricalMaterial ConduitType
        {
            get { return _conduitType; }
            set
            {
                var old = ConduitType;
                _conduitType = value;
                notifyCombinedChanged(Change.Edit, "ConduitType", this, value, old);
                CostBatch previous = old != null ? old.GetCosts(ConduitLength) : new CostBatch();
                CostBatch current = value != null ? value.GetCosts(ConduitLength) : new CostBatch();
                notifyCostChanged(current - previous);
            }
        }
        public bool IsPlenum
        {
            get { return _isPlenum; }
            set
            {
                var old = IsPlenum;
                CostBatch oldCost = this.CostBatch;
                _isPlenum = value;
                notifyCombinedChanged(Change.Edit, "IsPlenum", this, value, old);
                notifyCostChanged(this.CostBatch - oldCost);
            }
        }

        public CostBatch CostBatch
        {
            get { return getCosts(); }
        }

        public SaveableMap PropertyObjects
        {
            get { return propertyObjects(); }
        }
        public SaveableMap LinkedObjects
        {
            get { return linkedObjects(); }
        }
        abstract public IProtocol Protocol { get; }
        #endregion //Properties

        public event Action<CostBatch> CostChanged;

        #region Constructors 
        public TECConnection(Guid guid) : base(guid) { }
        public TECConnection() : this(Guid.NewGuid()) { }
        public TECConnection(TECConnection connectionSource, Dictionary<Guid, Guid> guidDictionary = null) : this()
        {
            if (guidDictionary != null)
            { guidDictionary[_guid] = connectionSource.Guid; }

            _length = connectionSource.Length;
            _conduitLength = connectionSource.ConduitLength;
            _isPlenum = connectionSource.IsPlenum;
            if (connectionSource.ConduitType != null)
            { _conduitType = connectionSource.ConduitType; }
        }
        #endregion //Constructors

        protected virtual void notifyCostChanged(CostBatch costs)
        {
            CostChanged?.Invoke(costs);
        }

        protected virtual CostBatch getCosts()
        {
            CostBatch costs = new CostBatch();
            foreach (TECConnectionType connectionType in this.Protocol.ConnectionTypes)
            {
                costs += connectionType.GetCosts(Length, IsPlenum);
            }
            if (ConduitType != null)
            {
                costs += ConduitType.GetCosts(ConduitLength);
            }
            return costs;
        }
        protected virtual SaveableMap propertyObjects()
        {
            SaveableMap saveList = new SaveableMap();
            if(this.ConduitType != null)
            {
                saveList.Add(this.ConduitType, "ConduitType");
            }
            return saveList;
        }
        protected virtual SaveableMap linkedObjects()
        {
            SaveableMap relatedList = new SaveableMap();
            if (this.ConduitType != null)
            {
                relatedList.Add(this.ConduitType, "ConduitType");
            }
            return relatedList;
        }
    }
}
