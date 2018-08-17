using EstimatingLibrary;
using EstimatingLibrary.Utilities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EstimatingUtilitiesLibrary.SummaryItems
{
    public class WireSummaryItem : LengthSummaryItem
    {
        public TECConnectionType ConnectionType
        {
            get { return Material as TECConnectionType; }
        }

        public bool IsPlenum { get; }
        public override double UnitCost
        {
            get
            {
                return this.IsPlenum ? this.ConnectionType.TotalPlenumCost : this.ConnectionType.Cost;
            }
        }
        public override double UnitLabor
        {
            get
            {
                return this.IsPlenum ? this.ConnectionType.TotalPlenumLabor : this.ConnectionType.Labor;
            }
        }
        public string TypeName
        {
            get { return string.Format("{0}{1}", (IsPlenum ? "Plenum " : ""), this.ConnectionType.Name); }
        }
        
        public WireSummaryItem(TECConnectionType type, double length, bool isPlenum) : base(type, length)
        {
            this.IsPlenum = isPlenum;
        }
    }
}
