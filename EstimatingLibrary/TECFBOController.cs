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
    public class TECFBOController : TECController, INotifyPointChanged, IDDCopiable
    {
        #region Properties
        private readonly TECCatalogs catalogs;

        private ObservableCollection<TECPoint> _points = new ObservableCollection<TECPoint>();
        public ObservableCollection<TECPoint> Points
        {
            get { return _points; }
            set
            {
                if (Points != null)
                {
                    Points.CollectionChanged -= pointsCollectionChanged;
                }
                var old = Points;
                _points = value;
                Points.CollectionChanged += pointsCollectionChanged;
                notifyCombinedChanged(Change.Edit, "Points", this, value, old);
            }
        }

        //---Derived---
        public override IOCollection IO { get; }

        public event Action<int> PointChanged;
        #endregion

        #region Constructors
        /// <summary>
        /// General constructor for TECFBOController.
        /// </summary>
        /// <param name="guid"></param>
        /// <param name="isTypical"></param>
        public TECFBOController(Guid guid, bool isTypical, TECCatalogs catalogs) : base(guid, isTypical)
        {
            this.catalogs = catalogs;
            this.IO = getInitialIO();
        }
        /// <summary>
        /// Constructor for new TECFBOController with generated GUID.
        /// </summary>
        /// <param name="isTypical"></param>
        public TECFBOController(bool isTypical, TECCatalogs catalogs) : this(Guid.NewGuid(), isTypical, catalogs) { }
        /// <summary>
        /// Copy constructor for TECFBOController
        /// </summary>
        /// <param name="controllerSource"></param>
        /// <param name="isTypical"></param>
        /// <param name="guidDictionary"></param>
        public TECFBOController(TECFBOController controllerSource, bool isTypical, Dictionary<Guid, Guid> guidDictionary = null) : base(controllerSource, isTypical, guidDictionary)
        {
            this.catalogs = controllerSource.catalogs;
            this.IO = getInitialIO();
        }
        #endregion

        #region Methods
        public override TECController CopyController(bool isTypical, Dictionary<Guid, Guid> guidDictionary)
        {
            return new TECFBOController(this, isTypical, guidDictionary);
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
            collectionChanged(sender, e, "Points");
        }
        #endregion

        #region Interfaces
        #region IDDCopiable
        Object IDDCopiable.DragDropCopy(TECScopeManager scopeManager)
        {
            var outController = new TECFBOController(this, this.IsTypical);
            ModelLinkingHelper.LinkScopeItem(outController, scopeManager);
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
        #endregion
    }
}
