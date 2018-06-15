using EstimatingLibrary;
using EstimatingLibrary.Interfaces;
using GongSolutions.Wpf.DragDrop;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TECUserControlLibrary.BaseVMs;
using TECUserControlLibrary.Utilities;
using static TECUserControlLibrary.ViewModels.MaterialVM;

namespace TECUserControlLibrary.ViewModels.CatalogVMs
{
    public abstract class CatalogVMBase : TECVMBase, IDropTarget
    {
        public TECTemplates Templates { get; }
        public ReferenceDropper ReferenceDropHandler { get; }

        protected CatalogVMBase(TECTemplates templates, ReferenceDropper dropHandler)
        {
            this.Templates = templates;
            this.ReferenceDropHandler = dropHandler;
        }
        
        public void DragOver(IDropInfo dropInfo)
        {
            UIHelpers.StandardDragOver(dropInfo);
        }
        public void Drop(IDropInfo dropInfo)
        {
            object drop<T>(T item)
            {
                bool isCatalog = item.GetType().GetInterfaces().Where(i => i.IsGenericType)
                    .Any(i => i.GetGenericTypeDefinition() == typeof(ICatalog<>));
                if (isCatalog)
                {
                    return ((dynamic)item).CatalogCopy();
                }
                else if (item is IDDCopiable dropable)
                {
                    return dropable.DragDropCopy(Templates);
                }
                else
                {
                    throw new NotImplementedException();
                }
            }

            UIHelpers.StandardDrop(dropInfo, Templates, drop);
        }
    }
}
