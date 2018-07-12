using EstimatingLibrary.Interfaces;
using EstimatingLibrary.Utilities;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static EstimatingLibrary.Utilities.CommonUtilities;

namespace EstimatingLibrary
{
    public class ScopeTemplates : TECObject, IRelatable
    {
        private ObservableCollection<TECSystem> _systemTemplates = new ObservableCollection<TECSystem>();
        private ObservableCollection<TECEquipment> _equipmentTemplates = new ObservableCollection<TECEquipment>();
        private ObservableCollection<TECSubScope> _subScopeTemplates = new ObservableCollection<TECSubScope>();
        private ObservableCollection<TECController> _controllerTemplates = new ObservableCollection<TECController>();
        private ObservableCollection<TECMisc> _miscCostTemplates = new ObservableCollection<TECMisc>();
        private ObservableCollection<TECPanel> _panelTemplates = new ObservableCollection<TECPanel>();
        private ObservableCollection<TECParameters> _parameters = new ObservableCollection<TECParameters>();

        public ObservableCollection<TECSystem> SystemTemplates
        {
            get { return _systemTemplates; }
            set
            {
                var old = SystemTemplates;
                SystemTemplates.CollectionChanged -= (sender, args) => collectionChanged(sender, args, "SystemTemplates");
                _systemTemplates = value;
                SystemTemplates.CollectionChanged += (sender, args) => collectionChanged(sender, args, "SystemTemplates");
                notifyCombinedChanged(Change.Edit, "SystemTemplates", this, value, old);
            }
        }
        public ObservableCollection<TECEquipment> EquipmentTemplates
        {
            get { return _equipmentTemplates; }
            set
            {
                var old = EquipmentTemplates;
                EquipmentTemplates.CollectionChanged -= (sender, args) => collectionChanged(sender, args, "EquipmentTemplates");
                _equipmentTemplates = value;
                EquipmentTemplates.CollectionChanged += (sender, args) => collectionChanged(sender, args, "EquipmentTemplates");
                notifyCombinedChanged(Change.Edit, "EquipmentTemplates", this, value, old);
            }
        }
        public ObservableCollection<TECSubScope> SubScopeTemplates
        {
            get { return _subScopeTemplates; }
            set
            {
                var old = SubScopeTemplates;
                SubScopeTemplates.CollectionChanged -= (sender, args) => collectionChanged(sender, args, "SubScopeTemplates");
                _subScopeTemplates = value;
                SubScopeTemplates.CollectionChanged += (sender, args) => collectionChanged(sender, args, "SubScopeTemplates");
                notifyCombinedChanged(Change.Edit, "SubScopeTemplates", this, value, old);
            }
        }
        public ObservableCollection<TECController> ControllerTemplates
        {
            get { return _controllerTemplates; }
            set
            {
                var old = ControllerTemplates;
                ControllerTemplates.CollectionChanged -= (sender, args) => collectionChanged(sender, args, "ControllerTemplates");
                _controllerTemplates = value;
                ControllerTemplates.CollectionChanged += (sender, args) => collectionChanged(sender, args, "ControllerTemplates");
                notifyCombinedChanged(Change.Edit, "ControllerTemplates", this, value, old);
            }
        }
        public ObservableCollection<TECMisc> MiscCostTemplates
        {
            get { return _miscCostTemplates; }
            set
            {
                var old = MiscCostTemplates;
                MiscCostTemplates.CollectionChanged -= (sender, args) => collectionChanged(sender, args, "MiscCostTemplates");
                _miscCostTemplates = value;
                MiscCostTemplates.CollectionChanged += (sender, args) => collectionChanged(sender, args, "MiscCostTemplates");
                notifyCombinedChanged(Change.Edit, "MiscCostTemplates", this, value, old);
            }
        }
        public ObservableCollection<TECPanel> PanelTemplates
        {
            get { return _panelTemplates; }
            set
            {
                var old = PanelTemplates;
                PanelTemplates.CollectionChanged -= (sender, args) => collectionChanged(sender, args, "PanelTemplates");
                _panelTemplates = value;
                PanelTemplates.CollectionChanged += (sender, args) => collectionChanged(sender, args, "PanelTemplates");
                notifyCombinedChanged(Change.Edit, "PanelTemplates", this, value, old);
            }
        }
        public ObservableCollection<TECParameters> Parameters
        {
            get { return _parameters; }
            set
            {
                var old = Parameters;
                Parameters.CollectionChanged -= (sender, args) => collectionChanged(sender, args, "Parameters");
                _parameters = value;
                Parameters.CollectionChanged += (sender, args) => collectionChanged(sender, args, "Parameters");
                notifyCombinedChanged(Change.Edit, "Parameters", this, value, old);
            }
        }
        
        public ScopeTemplates() : this(Guid.NewGuid()) { }
        public ScopeTemplates(Guid guid) : base(guid)
        {
            SystemTemplates.CollectionChanged += (sender, args) => collectionChanged(sender, args, "SystemTemplates");
            EquipmentTemplates.CollectionChanged += (sender, args) => collectionChanged(sender, args, "EquipmentTemplates");
            SubScopeTemplates.CollectionChanged += (sender, args) => collectionChanged(sender, args, "SubScopeTemplates");
            ControllerTemplates.CollectionChanged += (sender, args) => collectionChanged(sender, args, "ControllerTemplates");
            MiscCostTemplates.CollectionChanged += (sender, args) => collectionChanged(sender, args, "MiscCostTemplates");
            PanelTemplates.CollectionChanged += (sender, args) => collectionChanged(sender, args, "PanelTemplates");
            Parameters.CollectionChanged += (sender, args) => collectionChanged(sender, args, "Parameters");
        }

        public void Fill(ScopeTemplates templatesToAdd)
        {
            FillScopeCollection(this.SystemTemplates, templatesToAdd.SystemTemplates);
            FillScopeCollection(this.EquipmentTemplates, templatesToAdd.EquipmentTemplates);
            FillScopeCollection(this.SubScopeTemplates, templatesToAdd.SubScopeTemplates);
            FillScopeCollection(this.ControllerTemplates, templatesToAdd.ControllerTemplates);
            FillScopeCollection(this.MiscCostTemplates, templatesToAdd.MiscCostTemplates);
            FillScopeCollection(this.PanelTemplates, templatesToAdd.PanelTemplates);
            FillScopeCollection(this.Parameters, templatesToAdd.Parameters);
        }
        public void Unionize(ScopeTemplates templatesToAdd)
        {
            UnionizeScopeColelction(this.SystemTemplates, templatesToAdd.SystemTemplates);
            UnionizeScopeColelction(this.EquipmentTemplates, templatesToAdd.EquipmentTemplates);
            UnionizeScopeColelction(this.SubScopeTemplates, templatesToAdd.SubScopeTemplates);
            UnionizeScopeColelction(this.ControllerTemplates, templatesToAdd.ControllerTemplates);
            UnionizeScopeColelction(this.MiscCostTemplates, templatesToAdd.MiscCostTemplates);
            UnionizeScopeColelction(this.PanelTemplates, templatesToAdd.PanelTemplates);
            UnionizeScopeColelction(this.Parameters, templatesToAdd.Parameters);
        }

        #region Collection Changed
        private void collectionChanged(object sender, System.Collections.Specialized.NotifyCollectionChangedEventArgs e, string propertyName)
        {
            CollectionChangedHandlers.CollectionChangedHandler(sender, e, propertyName, this, notifyCombinedChanged);
        }
        #endregion

        #region IRelatable
        public SaveableMap PropertyObjects { get { return propertyObjects(); } }
        public SaveableMap LinkedObjects { get { return new SaveableMap(); } }

        private SaveableMap propertyObjects()
        {
            SaveableMap saveList = new SaveableMap();
            saveList.AddRange(this.SystemTemplates, "SystemTemplates");
            saveList.AddRange(this.EquipmentTemplates, "EquipmentTemplates");
            saveList.AddRange(this.SubScopeTemplates, "SubScopeTemplates");
            saveList.AddRange(this.ControllerTemplates, "ControllerTemplates");
            saveList.AddRange(this.MiscCostTemplates, "MiscCostTemplates");
            saveList.AddRange(this.PanelTemplates, "PanelTemplates");
            saveList.AddRange(this.Parameters, "Parameters");
            return saveList;
        }
        #endregion
    }
}
