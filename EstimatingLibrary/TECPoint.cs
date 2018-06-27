using EstimatingLibrary.Interfaces;
using EstimatingLibrary.Utilities;
using System;

namespace EstimatingLibrary
{
    public class TECPoint : TECLabeled, INotifyPointChanged, ITypicalable
    {
        #region Properties
        private IOType _type = IOType.AI;
        private int _quantity = 1;
        private bool _isNetwork = false;

        public event Action<int> PointChanged;

        public IOType Type
        {
            get { return _type; }
            set
            {
                if (TECIO.PointIO.Contains(value))
                {
                    var old = Type;
                    _type = value;
                    // Call raisePropertyChanged whenever the property is updated
                    notifyCombinedChanged(Change.Edit, "Type", this, value, old);
                }
                else
                {
                    throw new InvalidOperationException("TECPoint cannot be non PointIO.");
                }
            }
        }
        public int Quantity
        {
            get { return _quantity; }
            set
            {
                var old = Quantity;
                if (!IsTypical)
                {
                    PointChanged?.Invoke(old - value);
                }
                _quantity = value;
                notifyCombinedChanged(Change.Edit, "Quantity", this, value, old);

            }
        }
        public bool IsNetwork
        {
            get { return _isNetwork; }
            set
            {
                var old = IsNetwork;
                _isNetwork = value;
                notifyCombinedChanged(Change.Edit, "IsNetwork", this, value, old);
            }
        }
        
        public bool IsTypical { get; private set; }
        #endregion //Properties

        #region Constructors
        public TECPoint(Guid guid) : base(guid) { IsTypical = false; }
        public TECPoint() : this(Guid.NewGuid()) { }

        public TECPoint(TECPoint pointSource) : this()
        {
            _type = pointSource.Type;
            _label = pointSource.Label;
            _quantity = pointSource.Quantity;
        }
        #endregion //Constructors

        #region Methods
        public void notifyPointChanged(int numPoints)
        {
            if (!IsTypical)
            {
                PointChanged?.Invoke(numPoints);
            }
        }

        #endregion

        #region INotityPointChanged
        int INotifyPointChanged.PointNumber
        {
            get
            {
                return Quantity;
            }
        }
        #endregion

        #region ITypicalable

        ITECObject ITypicalable.CreateInstance(ObservableListDictionary<ITECObject> typicalDictionary)
        {
            if (!this.IsTypical)
            {
                throw new Exception("Attempted to create an instance of an object which is already instanced.");
            }
            else
            {
                return new TECPoint(this);
            }
        }

        void ITypicalable.AddChildForProperty(string property, ITECObject item)
        {
            throw new Exception(String.Format("There is no compatible add method for the property {0} with an object of type {1}", property, item.GetType().ToString()));
        }

        bool ITypicalable.RemoveChildForProperty(string property, ITECObject item)
        {
            throw new Exception(String.Format("There is no compatible remove method for the property {0} with an object of type {1}", property, item.GetType().ToString()));
        }

        bool ITypicalable.ContainsChildForProperty(string property, ITECObject item)
        {
            throw new Exception(String.Format("There is no compatible property {0} with an object of type {1}", property, item.GetType().ToString()));
        }

        void ITypicalable.MakeTypical()
        {
            this.IsTypical = true;
        }
        #endregion

    }
}
