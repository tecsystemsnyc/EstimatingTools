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
    public class TECProposalItem : TECObject
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
            get { return ContainingScope.Aggregate("", (name, equip) => 
            name += equip.Name + (equip == ContainingScope.Last() ? "" : "; ")); }
        }
        
        public TECProposalItem(Guid guid, TECEquipment dislpayScope) : base(guid)
        {
            this.DisplayScope = dislpayScope;
            ContainingScope.CollectionChanged += containingScopeCollectionChanged;
        }
        public TECProposalItem(TECEquipment displayScope) : this(Guid.NewGuid(), displayScope) { }
        
        private void containingScopeCollectionChanged(object sender, System.Collections.Specialized.NotifyCollectionChangedEventArgs e)
        {
            CollectionChangedHandlers.CollectionChangedHandler(sender, e, "ContainingScope", this, notifyCombinedChanged);
        }
    }
}
