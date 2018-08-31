using EstimatingLibrary;
using EstimatingLibrary.Interfaces;
using EstimatingLibrary.Utilities;
using GongSolutions.Wpf.DragDrop;
using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using System.Windows;

namespace TECUserControlLibrary.Utilities
{
    public static class DragDropHelpers
    {
        /// <summary>
        /// Standard method for applying drop adorners if the sourceitem and targetcollection are compatible.
        /// </summary>
        /// <param name="dropInfo">
        /// DropInfo passed into the DragOver method in the IDropTarget interface from GongSolutions DragDrop.
        /// </param>
        /// <param name="dropCondition">
        /// A method which takes the soure item, source type, and target type and returns a bool indicating if the drop can occur.
        /// </param>
        /// <param name="failAction">
        /// An Action invoked when the drop is determined to be illegal.
        /// </param>
        public static void DragOver(IDropInfo dropInfo, Func<object, Type, Type, bool> dropCondition, Action failAction = null)
        {
            var sourceItem = dropInfo.Data;
            if (dropInfo.Data is IDragDropable dropbable)
            {
                sourceItem = dropbable.DropData();
            }
            Type sourceType;
            if (sourceItem is IList sourceList && sourceList.Count > 0)
            { sourceType = sourceList[0].GetType(); }
            else
            { sourceType = sourceItem.GetType(); }

            Type targetType = dropInfo.TargetCollection.GetItemType() ?? dropInfo.TargetItem?.GetType();

            if (targetType != null)
            {
                bool sourceNotNull = sourceItem != null;
                bool allowDrop = dropCondition(sourceItem, sourceType, targetType);
                if (sourceItem is IList listItems)
                {
                    allowDrop = listItems.Count > 0 ? true : false;
                    foreach (var item in listItems)
                    {
                        if (!dropCondition(item, sourceType, targetType))
                        {
                            allowDrop = false;
                            break;
                        }
                    }
                }
                if (sourceNotNull && allowDrop)
                {
                    SetDragAdorners(dropInfo);
                }
                else
                {
                    failAction?.Invoke();
                }
            }
        }
        /// <summary>
        /// Standard method for performing drop operations after DragOver has allowed such a drop.
        /// </summary>
        /// <param name="dropInfo">
        /// DropInfo passed into the DragOver method in the IDropTarget interface from GongSolutions DragDrop.
        /// </param>
        /// <param name="dropObject">
        /// A Method which takes a dropped object and return the object to add to the target collection.
        /// </param>
        public static void Drop(IDropInfo dropInfo, Func<object, object> dropObject, bool addObjectToCollection = true)
        {
            var sourceItem = dropInfo.Data;

            if(dropObject == null)
            {
                dropObject = obj =>
                {
                    if (dropInfo.Data is IDragDropable dropbable)
                    {
                        return sourceItem = dropbable.DropData();
                    }
                    else {
                        return obj;
                    }
                };
                
            }
            
            Type targetType = dropInfo.TargetCollection.GetItemType() ?? dropInfo.TargetItem?.GetType();

            if (dropInfo.VisualTarget != dropInfo.DragInfo.VisualSource)
            {
                if (sourceItem is IList)
                {
                    var outSource = new List<object>();
                    foreach (object item in ((IList)sourceItem))
                    {
                        var toAdd = dropObject(item);
                        outSource.Add(toAdd);
                    }
                    sourceItem = outSource;
                }
                else
                {
                    sourceItem = dropObject(sourceItem);
                }

                if (addObjectToCollection)
                {
                    if (dropInfo.InsertIndex > ((IList)dropInfo.TargetCollection).Count)
                    {
                        if (sourceItem is IList)
                        {
                            foreach (object item in ((IList)sourceItem))
                            {
                                ((IList)dropInfo.TargetCollection).Add(item);
                            }
                        }
                        else
                        {
                            ((IList)dropInfo.TargetCollection).Add(sourceItem);
                        }
                    }
                    else
                    {
                        if (sourceItem is IList)
                        {
                            var x = dropInfo.InsertIndex;
                            foreach (object item in ((IList)sourceItem))
                            {
                                ((IList)dropInfo.TargetCollection).Insert(x, item);
                                x += 1;
                            }
                        }
                        else
                        {
                            ((IList)dropInfo.TargetCollection).Insert(dropInfo.InsertIndex, sourceItem);
                        }
                    }
                }

            }
            else
            {
                handleReorderDrop(dropInfo);
            }
        }

        public static void StandardDragOver(IDropInfo dropInfo, Func<Type, bool> typeMeetsCondition = null, bool allowReorder = false)
        {
            if (typeMeetsCondition == null)
            {
                typeMeetsCondition = type => { return false; };
            }
            if (!allowReorder && dropInfo.TargetCollection == dropInfo.DragInfo.SourceCollection)
            {
                return;
            }

            DragOver(dropInfo, dropCondition);

            bool dropCondition(object sourceItem, Type sourceType, Type targetType)
            {
                return StandardDropCondition(sourceItem, sourceType, targetType) || typeMeetsCondition(targetType);
            }
        }
        public static void StandardDrop(IDropInfo dropInfo, TECScopeManager scopeManager, Func<object, object> dropMethod = null)
        {
            if (dropMethod == null)
            {
                dropMethod = item =>
                {
                    return ((IDragDropable)item).DropData();
                };
            }
            Drop(dropInfo, dropMethod, true);
        }

        public static void SystemToTypicalDragOver(IDropInfo dropInfo)
        {
            DragOver(dropInfo, dropCondition);

            bool dropCondition(object sourceItem, Type sourceType, Type targetType)
            {
                bool isDragDropable = sourceItem is IDragDropable;
                bool sourceNotNull = sourceItem != null;
                bool sourceMatchesTarget = sourceType == typeof(TECSystem) && (targetType == typeof(TECTypical) || targetType == typeof(TECSystem));
                return sourceNotNull && sourceMatchesTarget && isDragDropable;
            }

        }
        public static void SystemToTypicalDrop(IDropInfo dropInfo, TECBid bid)
        {
            Drop(dropInfo, copySystem, true);

            TECTypical copySystem(object sourceItem)
            {
                TECSystem copiedSystem = (sourceItem as TECSystem).DropData() as TECSystem;
                return new TECTypical(copiedSystem, bid);
            }
        }

        public static void FileDragOver(IDropInfo dropInfo, IEnumerable<string> extensions)
        {
            if (dropInfo.Data is DataObject)
            {
                var dragFileList = ((DataObject)dropInfo.Data).GetFileDropList().Cast<string>();
                if (dragFileList.Count() == 1)
                {
                    if (extensions.Contains(Path.GetExtension(dragFileList.First())))
                    {
                        dropInfo.DropTargetAdorner = DropTargetAdorners.Insert;
                        dropInfo.Effects = DragDropEffects.Copy;
                    }
                }
            }
        }
        public static string FileDrop(IDropInfo dropInfo, IEnumerable<string> extensions)
        {
            if (dropInfo.Data is DataObject)
            {
                var dragFileList = ((DataObject)dropInfo.Data).GetFileDropList().Cast<string>();
                if (dragFileList.Count() == 1)
                {
                    if (extensions.Contains(Path.GetExtension(dragFileList.First())))
                    {
                        return dragFileList.First();
                    }
                    else
                    {
                        return null;
                    }
                }
                else
                {
                    return null;
                }
            }
            else
            {
                return null;
            }

        }

        public static bool StandardDropCondition(object sourceItem, Type sourceType, Type targetType)
        {
            bool isDragDropable = sourceItem is IDragDropable;
            bool sourceNotNull = sourceItem != null;
            bool sourceMatchesTarget = targetType.IsInstanceOfType(sourceItem);
            return sourceNotNull && (sourceMatchesTarget) && isDragDropable;
        }

        private static void handleReorderDrop(IDropInfo dropInfo)
        {
            var sourceItem = dropInfo.Data;
            int currentIndex = ((IList)dropInfo.TargetCollection).IndexOf(sourceItem);
            int finalIndex = dropInfo.InsertIndex;
            if (currentIndex == finalIndex) { return; }

            if (sourceItem is IList)
            {
                currentIndex = ((IList)dropInfo.TargetCollection).IndexOf(((IList)sourceItem)[0]);
                if (dropInfo.InsertIndex > currentIndex)
                {
                    finalIndex -= 1;
                }
                if (dropInfo.InsertIndex > ((IList)dropInfo.TargetCollection).Count)
                {
                    finalIndex = ((IList)dropInfo.TargetCollection).Count - 1;
                }
                var x = 0;
                if (currentIndex == finalIndex) { return; }
                foreach (object item in ((IList)sourceItem))
                {
                    currentIndex = ((IList)dropInfo.TargetCollection).IndexOf(((IList)sourceItem)[x]);
                    ((dynamic)dropInfo.TargetCollection).Move(currentIndex, finalIndex);
                    x += 1;
                }
            }
            else
            {
                if (dropInfo.InsertIndex > currentIndex)
                {
                    finalIndex -= 1;
                }
                if (dropInfo.InsertIndex > ((IList)dropInfo.TargetCollection).Count)
                {
                    finalIndex = ((IList)dropInfo.TargetCollection).Count - 1;
                }
                if (currentIndex == finalIndex) { return; }
                ((dynamic)dropInfo.TargetCollection).Move(currentIndex, finalIndex);
            }
        }

        public static bool TargetCollectionIsType(IDropInfo dropInfo, Type type)
        {
            var targetCollection = dropInfo.TargetCollection;
            if (targetCollection.GetType().GetTypeInfo().GenericTypeArguments.Length > 0)
            {
                Type targetType = targetCollection.GetType().GetTypeInfo().GenericTypeArguments[0];
                return targetType == type;
            }
            else
            {
                return false;
            }
        }

        public static void SetDragAdorners(IDropInfo dropInfo)
        {
            dropInfo.DropTargetAdorner = DropTargetAdorners.Insert;
            dropInfo.Effects = DragDropEffects.Copy;
        }

    }
}
