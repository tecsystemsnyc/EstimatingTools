using EstimatingLibrary.Interfaces;
using EstimatingLibrary.Utilities;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;

namespace EstimatingLibrary
{

    public abstract class TECConnection : TECObject, INotifyCostChanged, IRelatable, ITypicalable
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
        public TECController ParentController { get; }
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

        public bool IsTypical { get; private set; }
        public IProtocol Protocol { get; }
        abstract public IOCollection IO { get; }

        #endregion //Properties

        public event Action<CostBatch> CostChanged;

        #region Constructors 
        public TECConnection(Guid guid, TECController parent, IProtocol protocol, bool isTypical) : base(guid)
        {
            this.IsTypical = isTypical;
            this.ParentController = parent;
            this.Protocol = protocol;
        }
        public TECConnection(TECController parent, IProtocol protocol, bool isTypical) : this(Guid.NewGuid(), parent, protocol, isTypical) { }
        public TECConnection(TECConnection connectionSource, TECController parent, bool isTypical, Dictionary<Guid, Guid> guidDictionary = null) : this(parent, connectionSource.Protocol, isTypical)
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

        public void notifyCostChanged(CostBatch costs)
        {
            if (!IsTypical)
            {
                CostChanged?.Invoke(costs);
            }
        }

        protected virtual CostBatch getCosts()
        {
            CostBatch costs = new CostBatch();
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
