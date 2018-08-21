using EstimatingLibrary.Interfaces;
using EstimatingLibrary.Utilities;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EstimatingLibrary
{
    public class TECProposalItem : TECObject, IRelatable
    {
        private TECEquipment _displayScope;

        public TECEquipment DisplayScope
        {
            get { return _displayScope; }
            set
            {
                var old = DisplayScope;
                _displayScope = value;
                notifyCombinedChanged(Change.Edit, "DisplayScope", this, value, old);
            }
        }
        public ObservableCollection<TECEquipment> ContainingScope { get; } = new ObservableCollection<TECEquipment>();

        public List<TECScopeBranch> Branches
        {
            get { return DisplayScope.SubScope.SelectMany(x => x.ScopeBranches).ToList(); }
        }
        public String DisplayName
        {
            get {
                var allScope = new List<TECEquipment>() { DisplayScope };
                allScope.AddRange(ContainingScope);
                return allScope.ToList().Aggregate("", (name, equip) => 
            name += equip.Name + (ContainingScope.Count == 0 || equip == ContainingScope.Last() ? "" : "; ")); }
        }

        public TECProposalItem(Guid guid, TECEquipment dislpayScope) : base(guid)
        {
            this.DisplayScope = dislpayScope;
            ContainingScope.CollectionChanged += containingScopeCollectionChanged;
            new ChangeWatcher(dislpayScope).Changed += scopeChanged;
        }
        public TECProposalItem(TECProposalItem source, Dictionary<Guid, Guid> guidDictionary = null) : this(source.Guid, new TECEquipment(source.DisplayScope, guidDictionary))
        {
            foreach(TECEquipment item in source.ContainingScope)
            {
                this.ContainingScope.Add(new TECEquipment(item, guidDictionary));
            }

        }
        public TECProposalItem(TECEquipment displayScope) : this(Guid.NewGuid(), displayScope) { }
        
        private void scopeChanged(TECChangedEventArgs obj)
        {
            if(obj.PropertyName == "ScopeBranches")
            {
                raisePropertyChanged("Branches");
            }
        }
        private void containingScopeCollectionChanged(object sender, System.Collections.Specialized.NotifyCollectionChangedEventArgs e)
        {
            CollectionChangedHandlers.CollectionChangedHandler(sender, e, "ContainingScope", this, notifyCombinedChanged);
            raisePropertyChanged("DisplayName");
            raisePropertyChanged("AllScope");
        }

        #region IRelatable

        public RelatableMap PropertyObjects
        {
            get
            {
                var map = new RelatableMap();
                map.Add(DisplayScope, "DisplayScope");
                map.AddRange(ContainingScope, "ContainingScope");
                return map;
            }
        }

        public RelatableMap LinkedObjects
        {
            get
            {
                var map = new RelatableMap();
                map.Add(DisplayScope, "DisplayScope");
                map.AddRange(ContainingScope, "ContainingScope");
                return map;
            }
        }
        #endregion
    }
}
