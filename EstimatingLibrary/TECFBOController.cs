using EstimatingLibrary.Interfaces;
using EstimatingLibrary.Utilities;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EstimatingLibrary
{
    public class TECFBOController : TECController, INotifyPointChanged, IDragDropable
    {
        #region Properties
        private readonly TECCatalogs catalogs;

        public ObservableCollection<TECPoint> Points { get; } = new ObservableCollection<TECPoint>();

        //---Derived---
        public override IOCollection IO { get; }

        public event Action<int> PointChanged;
        #endregion

        #region Constructors
        /// <summary>
        /// General constructor for TECFBOController.
        /// </summary>
        /// <param name="guid"></param>
        public TECFBOController(Guid guid, TECCatalogs catalogs) : base(guid)
        {
            this.catalogs = catalogs;
            this.IO = getInitialIO();
            this.Points.CollectionChanged += pointsCollectionChanged;
        }
        /// <summary>
        /// Constructor for new TECFBOController with generated GUID.
        /// </summary>
        /// <param name="isTypical"></param>
        public TECFBOController(TECCatalogs catalogs) : this(Guid.NewGuid(), catalogs) { }
        /// <summary>
        /// Copy constructor for TECFBOController
        /// </summary>
        /// <param name="controllerSource"></param>
        /// <param name="guidDictionary"></param>
        public TECFBOController(TECFBOController controllerSource, Dictionary<Guid, Guid> guidDictionary = null) : base(controllerSource, guidDictionary)
        {
            this.catalogs = controllerSource.catalogs;
            this.IO = getInitialIO();
            this.Points.CollectionChanged += pointsCollectionChanged;
        }
        #endregion

        #region Methods
        public override TECController CopyController(Dictionary<Guid, Guid> guidDictionary)
        {
            return new TECFBOController(this, guidDictionary);
        }

        private IOCollection getInitialIO()
        {
            IOCollection io = new IOCollection();
            TECIO ui = new TECIO(IOType.UI);
            ui.Quantity = 100;
            io.Add(ui);
            TECIO uo = new TECIO(IOType.UO);
            uo.Quantity = 100;
            io.Add(uo);
            foreach(TECProtocol prot in this.catalogs.Protocols)
            {
                io.Add(prot);
            }
            return io;
        } 

        private void pointsCollectionChanged(object sender, NotifyCollectionChangedEventArgs e)
        {
            CollectionChangedHandlers.CollectionChangedHandler(sender, e, "Points", this, notifyCombinedChanged, notifyPoint: notifyPointChanged);
        }
        #endregion

        #region IDragDropable
        Object IDragDropable.DropData()
        {
            var outController = new TECFBOController(this);
            return outController;
        }
        #endregion

        #region INotifyPointChanged
        int INotifyPointChanged.PointNumber
        {
            get
            {
                var totalPoints = 0;
                foreach (TECPoint point in this.Points)
                {
                    totalPoints += point.Quantity;
                }
                return totalPoints;
            }
        }
        
        private void notifyPointChanged(int numPoints)
        {
            if (!IsTypical)
            {
                PointChanged?.Invoke(numPoints);
            }
        }
        #endregion

        #region ITypicalable
        protected override ITECObject createInstance(ObservableListDictionary<ITECObject> typicalDictionary)
        {
            if (!this.IsTypical)
            {
                throw new Exception("Attempted to create an instance of an object which is already instanced.");
            }
            else
            {
                return new TECFBOController(this);
            }
        }

        protected override void addChildForProperty(string property, ITECObject item)
        {
            if (property == "ChildrenConnections") { }
            else
            {
                this.AddChildForScopeProperty(property, item);
            }
        }

        protected override bool removeChildForProperty(string property, ITECObject item)
        {
            if (property == "ChildrenConnections") { return true; }
            else
            {
                return this.RemoveChildForScopeProperty(property, item);
            }
        }

        protected override bool containsChildForProperty(string property, ITECObject item)
        {
            if (property == "ChildrenConnections") { return true; }

            else
            {
                return this.ContainsChildForScopeProperty(property, item);
            }
        }

        protected override void makeTypical()
        {
            this.IsTypical = true;
            TypicalableUtilities.MakeChildrenTypical(this);
        }
        #endregion

        #region IRelatable
        protected override RelatableMap propertyObjects()
        {
            var map = base.propertyObjects();
            map.AddRange(this.Points, "Points");
            return map;
        }
        #endregion
    }
}
