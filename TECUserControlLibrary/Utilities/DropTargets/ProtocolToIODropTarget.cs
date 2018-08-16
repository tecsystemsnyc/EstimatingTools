using EstimatingLibrary;
using GongSolutions.Wpf.DragDrop;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TECUserControlLibrary.Utilities.DropTargets
{
    public class ProtocolToIODropTarget : IDropTarget
    {
        public void DragOver(IDropInfo dropInfo)
        {
            if (dropInfo.Data is TECProtocol && DragDropHelpers.TargetCollectionIsType(dropInfo, typeof(TECIO)))
            {
                DragDropHelpers.SetDragAdorners(dropInfo);
            }
        }

        public void Drop(IDropInfo dropInfo)
        {
            DragDropHelpers.Drop(dropInfo, x => convertToIO(x, (IList)dropInfo.TargetCollection), false);
        }

        static private object convertToIO(object protocol, IList collection)
        {
            foreach(TECIO io in collection)
            {
                if (protocol == io.Protocol)
                {
                    io.Quantity++;
                    return null;
                }
            }
            collection.Add(new TECIO(protocol as TECProtocol));
            return null;
        }
    }
}
