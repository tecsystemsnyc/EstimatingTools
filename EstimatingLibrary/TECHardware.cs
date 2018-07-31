using EstimatingLibrary.Interfaces;
using EstimatingLibrary.Utilities;
using System;

namespace EstimatingLibrary
{
    public abstract class TECHardware : TECCost, IDDCopiable
    {
        #region Fields
        private TECManufacturer _manufacturer;
        private double _price = 0;
        private double _quotedPrice = -1;
        private bool _requireQuote = false;
        #endregion

        #region Constructors
        public TECHardware(Guid guid, TECManufacturer manufacturer, CostType type) : base(guid, type)
        {
            _manufacturer = manufacturer;
        }
        #endregion

        #region Properties
        public TECManufacturer Manufacturer
        {
            get { return _manufacturer; }
            set
            {
                if(_manufacturer != value)
                {
                    var old = Manufacturer;
                    var originalCost = CostBatch;
                    _manufacturer = value;
                    notifyCombinedChanged(Change.Edit, "Manufacturer", this, value, old);
                    notifyCostChanged(CostBatch - originalCost);
                }
            }
        }
        public override double Cost
        {
            get
            {
                return QuotedPrice == -1 ? Price * Manufacturer.Multiplier : QuotedPrice;
            }
            set
            {
                Price = value / Manufacturer.Multiplier;
            }
        }
        public double Price
        {
            get { return _price; }
            set
            {
                var old = Price;
                _price = value;
                notifyCombinedChanged(Change.Edit, "Price", this, value, old);
                notifyCostChanged(new CostBatch(value - old, 0, Type));
                raisePropertyChanged("Cost");
            }
        }
        public double QuotedPrice
        {
            get { return _quotedPrice; }
            set
            {
                var old = QuotedPrice;
                _quotedPrice = value;
                notifyCombinedChanged(Change.Edit, "QuotedPrice", this, value, old);
                notifyCostChanged(new CostBatch(value - old, 0, Type));
                raisePropertyChanged("Cost");
            }
        }
        public bool RequireQuote
        {
            get { return _requireQuote; }
            set
            {
                var old = RequireQuote;
                _requireQuote = value;
                notifyCombinedChanged(Change.Edit, "RequireQuote", this, value, old);
            }
        }

        #endregion

        #region Methods
        public object DragDropCopy(TECScopeManager scopeManager)
        {
            return this;
        }

        protected void copyPropertiesFromHardware(TECHardware hardware)
        {
            copyPropertiesFromCost(hardware);
            _manufacturer = hardware.Manufacturer;
            _price = hardware.Price;
        }
        
        protected override SaveableMap propertyObjects()
        {
            SaveableMap saveList = new SaveableMap();
            saveList.AddRange(base.propertyObjects());
            saveList.Add(this.Manufacturer, "Manufacturer");
            return saveList;
        }
        protected override SaveableMap linkedObjects()
        {
            SaveableMap saveList = new SaveableMap();
            saveList.AddRange(base.linkedObjects());
            saveList.Add(this.Manufacturer, "Manufacturer");
            return saveList;
        }
        #endregion
    }
}
