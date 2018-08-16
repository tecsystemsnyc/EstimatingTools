using GongSolutions.Wpf.DragDrop;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TECUserControlLibrary.Utilities.DropTargets
{
    public class EmptyDropTarget : IDropTarget
    {
        public Action<IDropInfo> DragOverAction;
        public Action<IDropInfo> DropAction;

        public void DragOver(IDropInfo dropInfo)
        {
            DragOverAction?.Invoke(dropInfo);
        }

        public void Drop(IDropInfo dropInfo)
        {
            DropAction?.Invoke(dropInfo);
        }
    }
}
