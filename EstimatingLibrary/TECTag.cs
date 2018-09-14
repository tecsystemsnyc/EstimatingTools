using EstimatingLibrary.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EstimatingLibrary
{
    public class TECTag : TECLabeled, ICatalog<TECTag>, IDragDropable
    {
        public TECTag() : base() { }
        public TECTag(Guid guid) : base(guid) { }
        public TECTag(TECTag source) : base(source) { }

        public TECTag CatalogCopy()
        {
            return new TECTag(this);
        }

        public override object DropData()
        {
            return this;
        }
    }
}
