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

        public static Dictionary<AllSearchableObjects, string> SearchSelectorList = new Dictionary<AllSearchableObjects, string>
        {
            { AllSearchableObjects.System, "Systems" },
            { AllSearchableObjects.Equipment, "Equipment" },
            { AllSearchableObjects.SubScope, "Points" },
            { AllSearchableObjects.Devices, "Devices" },
            { AllSearchableObjects.Valves, "Valves" },
            { AllSearchableObjects.Wires, "Connection Types" },
            { AllSearchableObjects.Conduits, "Conduit Types" },
            { AllSearchableObjects.AssociatedCosts, "Associated Costs" },
            { AllSearchableObjects.MiscCosts, "Misc. Costs" },
            { AllSearchableObjects.MiscWiring, "Misc. Wiring" },
            { AllSearchableObjects.Tags, "Tags" },
            { AllSearchableObjects.ControllerTypes, "Controller Types" },
            { AllSearchableObjects.PanelTypes, "Panel Types" },
            { AllSearchableObjects.IOModules, "IO Modules" },
            { AllSearchableObjects.Protocols, "Protocols" }
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
