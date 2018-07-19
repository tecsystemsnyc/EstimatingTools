using EstimatingLibrary.Interfaces;
using EstimatingLibrary.Utilities;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;

namespace EstimatingLibrary
{
    public class TECBid : TECScopeManager, INotifyCostChanged, INotifyPointChanged
    {
        #region Fields
        private string _name = "";
        private string _bidNumber = "";
        private DateTime _dueDate = DateTime.Now;
        private string _salesperson = "";
        private string _estimator = "";
        private double _duration = 0.0;
        private TECParameters _parameters;
        private TECExtraLabor _extraLabor;
        private TECSchedule _schedule;

        public event Action<CostBatch> CostChanged;
        public event Action<int> PointChanged;
        
        private ObservableCollection<TECController> _controllers = new ObservableCollection<TECController>();
        #endregion

        #region Properties
        public string Name
        {
            get { return _name; }
            set
            {
                if (_name != value)
                {
                    var old = _name;
                    _name = value;
                    notifyCombinedChanged(Change.Edit, "Name", this, value, old);
                }
            }
        }
        public string BidNumber
        {
            get { return _bidNumber; }
            set
            {
                var old = BidNumber;
                _bidNumber = value;
                notifyCombinedChanged(Change.Edit, "BidNumber", this, value, old);
            }
        }
        public DateTime DueDate
        {
            get { return _dueDate; }
            set
            {
                var old = DueDate;
                _dueDate = value;
                notifyCombinedChanged(Change.Edit, "DueDate", this, value, old);
            }
        }
        public string DueDateString
        {
            get { return _dueDate.ToString("O"); }
        }
        public string Salesperson
        {
            get { return _salesperson; }
            set
            {
                var old = Salesperson;
                _salesperson = value;
                notifyCombinedChanged(Change.Edit, "Salesperson", this, value, old);
            }
        }
        public string Estimator
        {
            get { return _estimator; }
            set
            {
                var old = Estimator;
                _estimator = value;
                notifyCombinedChanged(Change.Edit, "Estimator", this, value, old);
            }
        }
        public double Duration
        {
            get { return _duration; }
            set
            {
                var old = Duration;
                _duration = value;
                notifyCombinedChanged(Change.Edit, "Duration", this, value, old);
            }
        }

        public TECParameters Parameters
        {
            get { return _parameters; }
            set
            {
                var old = Parameters;
                _parameters = value;
                notifyCombinedChanged(Change.Edit, "Parameters", this, value, old);
            }
        }
        public TECExtraLabor ExtraLabor
        {
            get { return _extraLabor; }
            set
            {
                var old = ExtraLabor;
                _extraLabor = value;
                notifyCombinedChanged(Change.Edit, "ExtraLabor", this, value, old);
                CostChanged?.Invoke(value.CostBatch - old.CostBatch);
            }

        }
        public TECSchedule Schedule
        {
            get { return _schedule; }
            set
            {
                var old = _schedule;
                _schedule = value;
                notifyCombinedChanged(Change.Edit, "Schedule", this, value, old);
            }
        }

        public ObservableCollection<TECScopeBranch> ScopeTree { get; } = new ObservableCollection<TECScopeBranch>();
        public ObservableCollection<TECTypical> Systems { get; } = new ObservableCollection<TECTypical>();
        public ObservableCollection<TECLabeled> Notes { get; } = new ObservableCollection<TECLabeled>();
        public ObservableCollection<TECLabeled> Exclusions { get; } = new ObservableCollection<TECLabeled>();
        public ObservableCollection<TECLocation> Locations { get; } = new ObservableCollection<TECLocation>();
        
        public ReadOnlyObservableCollection<TECController> Controllers
        {
            get { return new ReadOnlyObservableCollection<TECController>(_controllers); }
        }
        public ObservableCollection<TECMisc> MiscCosts { get; } = new ObservableCollection<TECMisc>();
        public ObservableCollection<TECPanel> Panels { get; } = new ObservableCollection<TECPanel>();
        public ObservableCollection<TECInternalNote> InternalNotes { get; } = new ObservableCollection<TECInternalNote>();

        public CostBatch CostBatch
        {
            get { return getCosts();  }
        }
        public int PointNumber
        {
            get
            {
                return pointNumber();
            }
        }
        #endregion //Properties

        #region Constructors
        public TECBid(Guid guid) : base(guid)
        {
            _extraLabor = new TECExtraLabor(this.Guid);
            _parameters = new TECParameters(this.Guid);

            Systems.CollectionChanged += (sender, args) => collectionChanged(sender, args, "Systems");
            ScopeTree.CollectionChanged += (sender, args) => collectionChanged(sender, args, "ScopeTree");
            Notes.CollectionChanged += (sender, args) => collectionChanged(sender, args, "Notes");
            Exclusions.CollectionChanged += (sender, args) => collectionChanged(sender, args, "Exclusions");
            Locations.CollectionChanged += locationsCollectionChanged;
            MiscCosts.CollectionChanged += (sender, args) => collectionChanged(sender, args, "MiscCosts");
            Panels.CollectionChanged += (sender, args) => collectionChanged(sender, args, "Panels");
            InternalNotes.CollectionChanged += (sender, args) => collectionChanged(sender, args, "InternalNotes");
        }

        public TECBid() : this(Guid.NewGuid())
        {
            foreach (string item in Defaults.Scope)
            {
                var branchToAdd = new TECScopeBranch();
                branchToAdd.Label = item;
                ScopeTree.Add(new TECScopeBranch(branchToAdd));
            }
            foreach (string item in Defaults.Exclusions)
            {
                var exclusionToAdd = new TECLabeled();
                exclusionToAdd.Label = item;
                Exclusions.Add(new TECLabeled(exclusionToAdd));
            }
            foreach (string item in Defaults.Notes)
            {
                var noteToAdd = new TECLabeled();
                noteToAdd.Label = item;
                Notes.Add(new TECLabeled(noteToAdd));
            }
            _parameters.Markup = 20;
        }

        #endregion //Constructors

        #region Methods
        #region Add/Remove Object Methods
        public void AddController(TECController controller)
        {
            _controllers.Add(controller);
            notifyCombinedChanged(Change.Add, "Controllers", this, controller);
            CostChanged?.Invoke(controller.CostBatch);
        }
        public void RemoveController(TECController controller)
        {
            controller.DisconnectAll();
            _controllers.Remove(controller);
            foreach(TECPanel panel in this.Panels)
            {
                if (panel.Controllers.Contains(controller)) { panel.Controllers.Remove(controller); }
            }
            notifyCombinedChanged(Change.Remove, "Controllers", this, controller);
            CostChanged?.Invoke(-controller.CostBatch);
        }
        public void SetControllers(IEnumerable<TECController> newControllers)
        {
            IEnumerable<TECController> oldControllers = Controllers;
            _controllers = new ObservableCollection<TECController>(newControllers);
            notifyCombinedChanged(Change.Edit, "Controllers", this, newControllers, oldControllers);
        }
        #endregion

        #region Event Handlers
        private void collectionChanged(object sender, System.Collections.Specialized.NotifyCollectionChangedEventArgs e, string collectionName)
        {
            if (e.Action == System.Collections.Specialized.NotifyCollectionChangedAction.Add)
            {
                int pointNumber = 0;
                CostBatch costs = new CostBatch();
                bool pointChanged = false;
                bool costChanged = false;
                foreach (object item in e.NewItems)
                {
                    if (item is INotifyPointChanged pointItem)
                    {
                        pointNumber += pointItem.PointNumber;
                        pointChanged = true;
                    }
                    if (item is INotifyCostChanged costItem)
                    {
                        costs += costItem.CostBatch;
                        costChanged = true;
                    }
                    notifyCombinedChanged(Change.Add, collectionName, this, item);
                    if (item is TECTypical typical)
                    {
                        costChanged = false;
                        pointChanged = false;
                    }
                }
                if (pointChanged) PointChanged?.Invoke(pointNumber);
                if (costChanged) CostChanged?.Invoke(costs);
            }
            else if (e.Action == System.Collections.Specialized.NotifyCollectionChangedAction.Remove)
            {
                int pointNumber = 0;
                CostBatch costs = new CostBatch();
                bool pointChanged = false;
                bool costChanged = false;
                foreach (object item in e.OldItems)
                {
                    if (item is INotifyPointChanged pointItem)
                    {
                        pointNumber += pointItem.PointNumber;
                        pointChanged = true;
                    }
                    if (item is INotifyCostChanged costItem)
                    {
                        costs += costItem.CostBatch;
                        costChanged = true;
                    }
                    notifyCombinedChanged(Change.Remove, collectionName, this, item);
                    if (item is TECTypical typ)
                    {
                        if (typ.Instances.Count == 0)
                        {
                            costChanged = false;
                            pointChanged = false;
                        }
                        foreach(TECSystem instance in typ.Instances)
                        {
                            foreach(TECSubScope subScope in instance.GetAllSubScope())
                            {
                                TECController parentController = subScope.Connection?.ParentController;
                                if(parentController != null && this.Controllers.Contains(parentController))
                                {
                                    parentController.Disconnect(subScope);
                                }
                            }
                            foreach(TECController controller in instance.Controllers)
                            {
                                TECController parentController = controller.ParentConnection?.ParentController;
                                if(parentController != null && this.Controllers.Contains(parentController))
                                {
                                    parentController.Disconnect(controller);
                                }
                            }
                        }
                    }
                }
                if (pointChanged) PointChanged?.Invoke(pointNumber * -1);
                if (costChanged) CostChanged?.Invoke(costs * -1);
            }
            else if (e.Action == System.Collections.Specialized.NotifyCollectionChangedAction.Move)
            {
                notifyCombinedChanged(Change.Edit, collectionName, this, sender, sender);
            }
        }
        private void locationsCollectionChanged(object sender, System.Collections.Specialized.NotifyCollectionChangedEventArgs e)
        {
            collectionChanged(sender, e, "Locations");
            if (e.Action == System.Collections.Specialized.NotifyCollectionChangedAction.Remove)
            {
                foreach (TECLabeled location in e.OldItems)
                {
                    removeLocationFromScope(location);
                }
            }
        }
        #endregion
        
        private CostBatch getCosts()
        {
            CostBatch costs = new CostBatch();
            foreach(TECMisc misc in this.MiscCosts)
            {
                costs += misc.CostBatch;
            }
            foreach(TECTypical system in this.Systems)
            {
                costs += system.CostBatch;
            }
            foreach(TECController controller in this.Controllers)
            {
                costs += controller.CostBatch;
            }
            foreach(TECPanel panel in this.Panels)
            {
                costs += panel.CostBatch;
            }
            return costs;
        }
        protected override SaveableMap propertyObjects()
        {
            SaveableMap saveList = base.propertyObjects();
            saveList.Add(this.Parameters, "Parameters");
            saveList.Add(this.ExtraLabor, "ExtraLabor");
            saveList.Add(this.Schedule, "Schedule");
            saveList.AddRange(this.ScopeTree, "ScopeTree");
            saveList.AddRange(this.Notes, "Notes");
            saveList.AddRange(this.Exclusions, "Exclusions");
            saveList.AddRange(this.Systems, "Systems");
            saveList.AddRange(this.Controllers, "Controllers");
            saveList.AddRange(this.Panels, "Panels");
            saveList.AddRange(this.MiscCosts, "MiscCosts");
            saveList.AddRange(this.Locations, "Locations");
            saveList.AddRange(this.InternalNotes, "InternalNotes");
            return saveList;
        }

        private int pointNumber()
        {
            int totalPoints = 0;
            foreach (TECSystem sys in Systems)
            {
                totalPoints += sys.PointNumber;
            }
            return totalPoints;
        }
        
        private void removeLocationFromScope(TECLabeled location)
        {
            foreach(TECTypical typical in this.Systems)
            {
                if (typical.Location == location) typical.Location = null;
                foreach(TECSystem instance in typical.Instances)
                {
                    if (instance.Location == location) instance.Location = null;
                    foreach(TECEquipment equip in instance.Equipment)
                    {
                        if (equip.Location == location) equip.Location = null;
                        foreach(TECSubScope ss in equip.SubScope)
                        {
                            if (ss.Location == location) ss.Location = null;
                        }
                    }
                }
                foreach (TECEquipment equip in typical.Equipment)
                {
                    if (equip.Location == location) equip.Location = null;
                    foreach (TECSubScope ss in equip.SubScope)
                    {
                        if (ss.Location == location) ss.Location = null;
                    }
                }
            }
        }
        #endregion
    }
}
