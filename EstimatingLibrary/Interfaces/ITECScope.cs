using System;
using System.Collections.ObjectModel;
using EstimatingLibrary.Interfaces;
using EstimatingLibrary.Utilities;

namespace EstimatingLibrary.Interfaces
{
    public interface ITECScope : ITECObject
    {
        ObservableCollection<TECAssociatedCost> AssociatedCosts { get; }
        CostBatch CostBatch { get; }
        string Description { get; set; }
        SaveableMap LinkedObjects { get; }
        string Name { get; set; }
        SaveableMap PropertyObjects { get; }
        ObservableCollection<TECTag> Tags { get; }

        event Action<CostBatch> CostChanged;
    }
}