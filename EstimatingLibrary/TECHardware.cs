using EstimatingLibrary.Interfaces;
using EstimatingLibrary.Utilities;
using System;

namespace EstimatingLibrary
{
    public abstract class TECHardware : TECCost, IDDCopiable, ICatalogContainer
    {
        #region Fields
        private TECManufacturer _manufacturer;
        private double _price;
        #endregion

        #region Constructors
        public TECHardware(Guid guid, TECManufacturer manufacturer, CostType type) : base(guid, type)
        {
            _price = 0;
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
                return Price * Manufacturer.Multiplier;
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

        #region ICatalogContainer
        public override bool RemoveCatalogItem<T>(T item, T replacement)
        {
            bool alreadyRemoved = base.RemoveCatalogItem(item, replacement);

            bool replacedMan = false;
            if (this.Manufacturer == item)
            {
                if (replacement is TECManufacturer newMan)
                {
                    this.Manufacturer = newMan;
                    replacedMan = true;
                }
                else throw new ArgumentNullException("Replacement Manufacturer cannot be null.");
            }
            return (replacedMan || alreadyRemoved);
        }
        #endregion
    }
}
