using EstimatingLibrary;
using EstimatingLibrary.Interfaces;
using GongSolutions.Wpf.DragDrop;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TECUserControlLibrary.Utilities.DropTargets
{
    public class NetworkConnectionDropTarget : IDropTarget
    {
        NetworkConnectionDropTargetDelegate dropDelegate;

        public NetworkConnectionDropTarget(NetworkConnectionDropTargetDelegate dropDelegate)
        {
            this.dropDelegate = dropDelegate;
        }

        public void DragOver(IDropInfo dropInfo)
        {
            UIHelpers.DragOver(dropInfo, dragCondition);
        }

        public void Drop(IDropInfo dropInfo)
        {
            UIHelpers.Drop(dropInfo, dropHandler, false);
        }

        private bool dragCondition(object data, Type sourceType, Type targetType)
        {
            if(dropDelegate.SelectedConnection == null) { return false; }
            IConnectable item = data as IConnectable;
            if(item == null || targetType != typeof(IConnectable)) { return false; }
            return item.AvailableProtocols.Contains(dropDelegate.SelectedConnection.Protocol);

        }

        private object dropHandler(object arg)
        {
            dropDelegate.SelectedConnection.AddChild(arg as IConnectable);
            return null;
        }

    }

    public interface NetworkConnectionDropTargetDelegate
    {
        TECNetworkConnection SelectedConnection { get; }
    }

    public class CatalogDropTarget : IDropTarget
    {
        public void DragOver(IDropInfo dropInfo)
        {
            UIHelpers.DragOver(dropInfo,
                (item, sourceType, targetType) =>
                {
                    return sourceType == targetType;
                });
        }
        public void Drop(IDropInfo dropInfo)
        {
            object drop<T>(T item)
            {
                return item;
            }

            UIHelpers.Drop(dropInfo, drop);
        }
    }
}
