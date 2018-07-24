using EstimatingLibrary.Interfaces;
using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EstimatingLibrary.Utilities
{
    public class CollectionChangedHandlers
    {
        public static void CollectionChangedHandler(object sender, NotifyCollectionChangedEventArgs e, string propertyName, ITECObject parent,
            Action<Change, string, ITECObject, object, object> notifyChanged = null,
            Action<CostBatch> notifyCost = null, Action<int> notifyPoint = null,
            Action<object> onAdd = null,
            Action<object> onRemove = null,
            bool notifyReorder = true)
        {
            CostBatch costs = new CostBatch();
            int pointNumber = 0;
            bool costChanged = false;

            if (e.Action == NotifyCollectionChangedAction.Add)
            {
                foreach (TECObject item in e.NewItems)
                {
                    if(parent is ITypicalable parentTypical && parentTypical.IsTypical && item is ITypicalable typ) { typ.MakeTypical(); }
                    onAdd?.Invoke(item);
                    if (item is ITypicalable typItem && typItem.IsTypical) { notifyCost = null; }
                    if (item is INotifyCostChanged cost)
                    {
                        costs += cost.CostBatch;
                        costChanged = true;
                    }
                    pointNumber += (item as INotifyPointChanged)?.PointNumber ?? 0;
                    
                    notifyChanged?.Invoke(Change.Add, propertyName, parent, item, null);
                }
                if (costChanged) notifyCost?.Invoke(costs);
                if (pointNumber != 0) notifyPoint?.Invoke(pointNumber);
            }
            else if (e.Action == NotifyCollectionChangedAction.Remove)
            {
                foreach (TECObject item in e.OldItems)
                {
                    onRemove?.Invoke(item);
                    if (item is ITypicalable typItem && typItem.IsTypical) { notifyCost = null; }
                    if (item is INotifyCostChanged cost)
                    {
                        costs += cost.CostBatch;
                        costChanged = true;
                    }
                    pointNumber += (item as INotifyPointChanged)?.PointNumber ?? 0;
                    
                    notifyChanged?.Invoke(Change.Remove, propertyName, parent, item, null);
                }
                if (costChanged) notifyCost?.Invoke(costs * -1);
                if (pointNumber != 0) notifyPoint?.Invoke(pointNumber * -1);
            }
            else if (e.Action == NotifyCollectionChangedAction.Move && notifyReorder)
            {
                notifyChanged?.Invoke(Change.Edit, propertyName, parent, sender, sender);
            }
        }
    }
}
