using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EstimatingLibrary.Interfaces
{
    public interface ICost: ITECObject
    {
        double Cost { get; }
        double Labor { get; }
        CostType Type { get; }
    }
}
