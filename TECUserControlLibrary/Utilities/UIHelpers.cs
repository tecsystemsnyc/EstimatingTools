using EstimatingLibrary;
using EstimatingLibrary.Interfaces;
using GongSolutions.Wpf.DragDrop;
using Microsoft.Win32;
using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Windows;
using System.Windows.Media;
using TECUserControlLibrary.Models;

namespace TECUserControlLibrary.Utilities
{
    public static class UIHelpers
    {       
        public static Type GetItemType(IEnumerable enumerable)
        {
            var args = enumerable.GetType().GetInterface("IEnumerable`1");
            if(args == null)
            {
                return null;
            }

            if (args.GenericTypeArguments.Length > 0)
            {
                return args.GenericTypeArguments[0];
            }else
            {
                return null;
            }
        }
        
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
            Type targetType = GetItemType(dropInfo.TargetCollection);
            
            if (targetType != null)
            {
                bool sourceNotNull = sourceItem != null;
                bool allowDrop = dropCondition(sourceItem, sourceType, targetType);
                if(sourceItem is IList listItems)
                {
                    allowDrop = listItems.Count > 0 ? true : false;
                    foreach(var item in listItems)
                    {
                        if(!dropCondition(item, sourceType, targetType))
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
            if (dropInfo.Data is IDragDropable dropbable)
            {
                sourceItem = dropbable.DropData();
            }
            Type targetType = GetItemType(dropInfo.TargetCollection);
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
            if(typeMeetsCondition == null)
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
            if(dropMethod == null)
            {
                dropMethod = item =>
                {
                    return ((IDDCopiable)item).DragDropCopy(scopeManager);
                };
            }
            Drop(dropInfo, dropMethod, true);
        }

        public static void SystemToTypicalDragOver(IDropInfo dropInfo)
        {
            DragOver(dropInfo, dropCondition);

            bool dropCondition(object sourceItem, Type sourceType, Type targetType)
            {
                bool isDragDropable = sourceItem is IDDCopiable;
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
                TECSystem copiedSystem = (sourceItem as TECSystem).DragDropCopy(bid) as TECSystem;
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
            }else
            {
                return null;
            }
            
        }
        
        public static bool StandardDropCondition(object sourceItem, Type sourceType, Type targetType)
        {
            bool isDragDropable = sourceItem is IDDCopiable;
            bool sourceNotNull = sourceItem != null;
            bool sourceMatchesTarget = targetType.IsInstanceOfType(sourceItem);
            return sourceNotNull && (sourceMatchesTarget) && isDragDropable;
        }

        private static void handleReorderDrop(IDropInfo dropInfo)
        {
            var sourceItem = dropInfo.Data;
            int currentIndex = ((IList)dropInfo.TargetCollection).IndexOf(sourceItem);
            int finalIndex = dropInfo.InsertIndex;
            if(currentIndex == finalIndex) { return; }

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
            } else
            {
                return false;
            }
        }

        public static void SetDragAdorners(IDropInfo dropInfo)
        {
            dropInfo.DropTargetAdorner = DropTargetAdorners.Insert;
            dropInfo.Effects = DragDropEffects.Copy;
        }
        #region Get Path Methods
        public static string GetSavePath(FileDialogParameters fileParams, string defaultFileName, string initialDirectory = "")
        {
            SaveFileDialog saveFileDialog = new SaveFileDialog();
            saveFileDialog.InitialDirectory = 
                (initialDirectory != "" ? initialDirectory : Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments));
            saveFileDialog.FileName = defaultFileName;
            saveFileDialog.Filter = fileParams.Filter;
            saveFileDialog.DefaultExt = fileParams.DefaultExtension;
            saveFileDialog.AddExtension = true;

            string savePath = null;
            if (saveFileDialog.ShowDialog() == true)
            {
                savePath = saveFileDialog.FileName;
            }
            return savePath;
        }
        public static string GetLoadPath(FileDialogParameters fileParams, string initialDirectory = "")
        {
            OpenFileDialog openFileDialog = new OpenFileDialog();
            openFileDialog.InitialDirectory =
                (initialDirectory != "" ? initialDirectory : Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments));
            openFileDialog.Filter = fileParams.Filter;
            openFileDialog.DefaultExt = fileParams.DefaultExtension;
            openFileDialog.AddExtension = true;

            string savePath = null;
            if (openFileDialog.ShowDialog() == true)
            {
                savePath = openFileDialog.FileName;
            }
            return savePath;
        }
        #endregion

        public static List<Tuple<string, TypicalInstanceEnum>> TypicalInstanceSelectorList = new List<Tuple<string, TypicalInstanceEnum>>
        {
            new Tuple<string, TypicalInstanceEnum>("Typical System Defintions", TypicalInstanceEnum.Typical),
            new Tuple<string, TypicalInstanceEnum>("Physical Instances", TypicalInstanceEnum.Instance)
        };

        public static List<Tuple<string, MaterialType>> MaterialSelectorList = new List<Tuple<string, MaterialType>>
        {
            new Tuple<string, MaterialType>("Devices", MaterialType.Device),
            new Tuple<string, MaterialType>("Wiring", MaterialType.ConnectionType),
            new Tuple<string, MaterialType>("Conduit", MaterialType.ConduitType),
            new Tuple<string, MaterialType>("Controller Types", MaterialType.ControllerType),
            new Tuple<string, MaterialType>("Panel Types", MaterialType.PanelType),
            new Tuple<string, MaterialType>("Associated Costs", MaterialType.AssociatedCost),
            new Tuple<string, MaterialType>("IO Modules", MaterialType.IOModule),
            new Tuple<string, MaterialType>("Valves", MaterialType.Valve),
            new Tuple<string, MaterialType>("Protocols", MaterialType.Protocol),
            new Tuple<string, MaterialType>("Manufacturer", MaterialType.Manufacturer),
            new Tuple<string, MaterialType>("Tag", MaterialType.Tag)
        };

        public static List<Tuple<string, Confidence>> ConfidenceSelectorList = new List<Tuple<string, Confidence>>
        {
            new Tuple<string, Confidence>("33%", Confidence.ThirtyThree),
            new Tuple<string, Confidence>("50%", Confidence.Fifty),
            new Tuple<string, Confidence>("66%", Confidence.SixtySix),
            new Tuple<string, Confidence>("80%", Confidence.Eighty),
            new Tuple<string, Confidence>("90%", Confidence.Ninety),
            new Tuple<string, Confidence>("95%", Confidence.NinetyFive)
        };

        public static List<Tuple<string, AllSearchableObjects>> SearchSelectorList = new List<Tuple<string, AllSearchableObjects>>
        {
            new Tuple<string, AllSearchableObjects>("Systems", AllSearchableObjects.System),
            new Tuple<string, AllSearchableObjects>("Equipment", AllSearchableObjects.Equipment),
            new Tuple<string, AllSearchableObjects>("Points", AllSearchableObjects.SubScope),
            new Tuple<string, AllSearchableObjects>("Devices", AllSearchableObjects.Devices),
            new Tuple<string, AllSearchableObjects>("Valves", AllSearchableObjects.Valves),
            new Tuple<string, AllSearchableObjects>("Wire Types", AllSearchableObjects.Wires),
            new Tuple<string, AllSearchableObjects>("Conduit Types", AllSearchableObjects.Conduits),
            new Tuple<string, AllSearchableObjects>("Associated Costs", AllSearchableObjects.AssociatedCosts),
            new Tuple<string, AllSearchableObjects>("Misc. Costs", AllSearchableObjects.MiscCosts),
            new Tuple<string, AllSearchableObjects>("Misc. Wiring", AllSearchableObjects.MiscWiring),
            new Tuple<string, AllSearchableObjects>("Tags", AllSearchableObjects.Tags),
            new Tuple<string, AllSearchableObjects>("Controller Types", AllSearchableObjects.ControllerTypes),
            new Tuple<string, AllSearchableObjects>("Panel Types", AllSearchableObjects.PanelTypes),
            new Tuple<string, AllSearchableObjects>("IO Modules", AllSearchableObjects.IOModules),
            new Tuple<string, AllSearchableObjects>("Protocols", AllSearchableObjects.Protocols),

        };

        public static List<Tuple<string, IOType>> IOSelectorList = new List<Tuple<string, IOType>>
        {
            new Tuple<string, IOType>("AI", IOType.AI),
            new Tuple<string, IOType>("AO", IOType.AO),
            new Tuple<string, IOType>("DI", IOType.DI),
            new Tuple<string, IOType>("DO", IOType.DO),
            new Tuple<string, IOType>("UI", IOType.UI),
            new Tuple<string, IOType>("UO", IOType.UO)
        };

        public static List<Tuple<string, ScopeTemplateIndex>> ScopeTemplateSelectorList = new List<Tuple<string, ScopeTemplateIndex>>
        {
            new Tuple<string, ScopeTemplateIndex>("Systems", ScopeTemplateIndex.System),
            new Tuple<string, ScopeTemplateIndex>("Equipment", ScopeTemplateIndex.Equipment),
            new Tuple<string, ScopeTemplateIndex>("Points", ScopeTemplateIndex.SubScope),
            new Tuple<string, ScopeTemplateIndex>("Miscellaneous", ScopeTemplateIndex.Misc)
        };

        public static IEnumerable<T> FindVisualChildren<T>(DependencyObject depObj)
       where T : DependencyObject
        {
            if (depObj != null)
            {
                for (int i = 0; i < VisualTreeHelper.GetChildrenCount(depObj); i++)
                {
                    DependencyObject child = VisualTreeHelper.GetChild(depObj, i);
                    if (child != null && child is T)
                    {
                        yield return (T)child;
                    }

                    foreach (T childOfChild in FindVisualChildren<T>(child))
                    {
                        yield return childOfChild;
                    }
                }
            }
        }

        public static childItem FindVisualChild<childItem>(DependencyObject obj)
            where childItem : DependencyObject
        {
            foreach (childItem child in FindVisualChildren<childItem>(obj))
            {
                return child;
            }

            return null;
        }

        public static T FindVisualParent<T>(DependencyObject element) where T : DependencyObject
        {
            var parent = VisualTreeHelper.GetParent(element);
            if(parent == null)
            {
                return null;
            }
            else
            {
                if(parent is T)
                {
                    return parent as T;
                }
                else
                {
                    return FindVisualParent<T>(parent as FrameworkElement);
                }
            }
        }


    }

    public enum EditIndex { System, Equipment, SubScope, Device, Point, Controller, Panel, PanelType, Nothing };
    public enum GridIndex { Systems = 1, DDC, Location, Proposal, Budget, Misc, Settings };
    public enum TemplateGridIndex { None, Systems, Equipment, SubScope, Devices, DDC, Materials, Constants };
    public enum ScopeCollectionIndex { None, System, Equipment, SubScope, Devices, Tags, Manufacturers, AddDevices, AddControllers, Controllers, AssociatedCosts, Panels, AddPanel, MiscCosts, MiscWiring };
    public enum LocationScopeType { System, Equipment, SubScope };
    public enum MaterialType { Device, ConnectionType, ConduitType, ControllerType,
        PanelType, AssociatedCost, IOModule, Protocol, Valve, Manufacturer, Tag};
    public enum TypicalSystemIndex { Edit, Instances };
    public enum SystemComponentIndex { Equipment, Controllers, Connections, Misc, Valves, Proposal };
    public enum ProposalIndex { Scope, Systems, Notes }
    public enum SystemsSubIndex { Typical, Instance, Location}
    public enum ScopeTemplateIndex { System, Equipment, SubScope, Misc }
    public enum AllSearchableObjects
    {
        System,
        Equipment,
        SubScope,
        Controllers,
        Panels,
        ControllerTypes,
        PanelTypes,
        Devices,
        MiscCosts,
        MiscWiring,
        Valves,
        Tags,
        AssociatedCosts,
        Wires,
        Conduits,
        IOModules,
        Protocols
    }
    public enum TypicalInstanceEnum { Typical, Instance }
    public enum MaterialSummaryIndex { Devices, Valves, Controllers, Panels, Wire, Conduit, Misc }
    
}
