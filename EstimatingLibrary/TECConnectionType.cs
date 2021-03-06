﻿using EstimatingLibrary.Interfaces;
using EstimatingLibrary.Utilities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EstimatingLibrary
{
    public class TECConnectionType : TECElectricalMaterial, ICatalog<TECConnectionType>
    {
        private double _plenumCost;
        private double _plenumLabor;

        /// <summary>
        /// The delta cost of the plenum version of this connection type.
        /// </summary>
        public double PlenumCost
        {
            get { return _plenumCost; }
            set
            {
                var old = PlenumCost;
                _plenumCost = value;
                notifyCombinedChanged(Change.Edit, "PlenumCost", this, value, old);
                notifyCostChanged(new CostBatch(value - old, 0, Type));
            }
        }
        /// <summary>
        /// The delta labor of the plenum version of this connection type.
        /// </summary>
        public double PlenumLabor
        {
            get { return _plenumLabor; }
            set
            {
                var old = PlenumLabor;
                _plenumLabor = value;
                notifyCombinedChanged(Change.Edit, "PlenumLabor", this, value, old);
                notifyCostChanged(new CostBatch(0, value - old, Type));
            }
        }

        public double TotalPlenumCost
        {
            get
            {
                return (Cost + PlenumCost);
            }
            set
            {
                PlenumCost = (value - Cost);
                raisePropertyChanged("TotalPlenumCost");
            }
        }
        public double TotalPlenumLabor
        {
            get
            {
                return (Labor + PlenumLabor);
            }
            set
            {
                PlenumLabor = (value - Labor);
                raisePropertyChanged("TotalPlenumLabor");
            }
        }

        public TECConnectionType(Guid guid) : base(guid)
        {
            PlenumCost = 0.0;
        }
        public TECConnectionType() : this(Guid.NewGuid()) { }
        public TECConnectionType(TECConnectionType typeSource) : base(typeSource)
        {
            PlenumCost = typeSource.PlenumCost;
            PlenumLabor = typeSource.PlenumLabor;
        }

        public CostBatch GetCosts(double length, bool isPlenum)
        {
            CostBatch outCosts = base.GetCosts(length);
            if (isPlenum)
            {
                outCosts.Add(CostType.Electrical, (length * PlenumCost), (length * PlenumLabor));
            }
            return outCosts;
        }

        #region ICatalog
        TECConnectionType ICatalog<TECConnectionType>.CatalogCopy()
        {
            return new TECConnectionType(this);
        }
        #endregion
    }
}
