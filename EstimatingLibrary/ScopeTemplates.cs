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
        public ObservableCollection<TECSystem> SystemTemplates { get; } = new ObservableCollection<TECSystem>();
        public ObservableCollection<TECEquipment> EquipmentTemplates { get; } = new ObservableCollection<TECEquipment>();
        public ObservableCollection<TECSubScope> SubScopeTemplates { get; } = new ObservableCollection<TECSubScope>();
        public ObservableCollection<TECController> ControllerTemplates { get; } = new ObservableCollection<TECController>();
        public ObservableCollection<TECMisc> MiscCostTemplates { get; } = new ObservableCollection<TECMisc>();
        public ObservableCollection<TECPanel> PanelTemplates { get; } = new ObservableCollection<TECPanel>();
        public ObservableCollection<TECParameters> Parameters { get; } = new ObservableCollection<TECParameters>();
        
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
            UnionizeScopeCollection(this.SystemTemplates, templatesToAdd.SystemTemplates);
            UnionizeScopeCollection(this.EquipmentTemplates, templatesToAdd.EquipmentTemplates);
            UnionizeScopeCollection(this.SubScopeTemplates, templatesToAdd.SubScopeTemplates);
            UnionizeScopeCollection(this.ControllerTemplates, templatesToAdd.ControllerTemplates);
            UnionizeScopeCollection(this.MiscCostTemplates, templatesToAdd.MiscCostTemplates);
            UnionizeScopeCollection(this.PanelTemplates, templatesToAdd.PanelTemplates);
            UnionizeScopeCollection(this.Parameters, templatesToAdd.Parameters);
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
