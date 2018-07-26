using EstimatingLibrary;
using EstimatingLibrary.Interfaces;
using EstimatingLibrary.Utilities;
using EstimatingLibrary.Utilities.WatcherFilters;
using GalaSoft.MvvmLight;
using NLog;
using System;
using System.Linq;
using TECUserControlLibrary.Utilities;
using TECUserControlLibrary.ViewModels.Interfaces;

namespace TECUserControlLibrary.ViewModels
{
    public class MaterialSummaryVM : ViewModelBase
    {
        private static Logger logger = LogManager.GetCurrentClassLogger();

        #region Fields and Properties
        private double _totalTECCost;
        private double _totalTECLabor;
        private double _totalElecCost;
        private double _totalElecLabor;

        private HardwareSummaryVM _deviceSummaryVM;
        private ValveSummaryVM _valveSummaryVM;
        private HardwareSummaryVM _controllerSummaryVM;
        private HardwareSummaryVM _panelSummaryVM;
        private LengthSummaryVM _wireSummaryVM;
        private LengthSummaryVM _conduitSummaryVM;
        private MiscCostsSummaryVM _miscSummaryVM;

        private IComponentSummaryVM _currentVM;
        private MaterialSummaryIndex _selectedIndex;
        private string _currentType;

        public double TotalTECCost
        {
            get { return _totalTECCost; }
            private set
            {
                _totalTECCost = value;
                RaisePropertyChanged("TotalTECCost");
            }
        }
        public double TotalTECLabor
        {
            get { return _totalTECLabor; }
            private set
            {
                _totalTECLabor = value;
                RaisePropertyChanged("TotalTECLabor");
            }
        }
        public double TotalElecCost
        {
            get { return _totalElecCost; }
            set
            {
                _totalElecCost = value;
                RaisePropertyChanged("TotalElecCost");
            }
        }
        public double TotalElecLabor
        {
            get { return _totalElecLabor; }
            set
            {
                _totalElecLabor = value;
                RaisePropertyChanged("TotalElecLabor");
            }
        }

        public HardwareSummaryVM DeviceSummaryVM
        {
            get { return _deviceSummaryVM; }
            set
            {
                if (DeviceSummaryVM != value)
                {
                    _deviceSummaryVM = value;
                    RaisePropertyChanged("DeviceSummaryVM");
                }
            }
        }
        public ValveSummaryVM ValveSummaryVM
        {
            get { return _valveSummaryVM; }
            set
            {
                if (ValveSummaryVM != value)
                {
                    _valveSummaryVM = value;
                    RaisePropertyChanged("ValveSummaryVM");
                }
            }
        }
        public HardwareSummaryVM ControllerSummaryVM
        {
            get { return _controllerSummaryVM; }
            set
            {
                if (ControllerSummaryVM != value)
                {
                    _controllerSummaryVM = value;
                    RaisePropertyChanged("ControllerSummaryVM");
                }
            }
        }
        public HardwareSummaryVM PanelSummaryVM
        {
            get { return _panelSummaryVM; }
            set
            {
                if (PanelSummaryVM != value)
                {
                    _panelSummaryVM = value;
                    RaisePropertyChanged("PanelSummaryVM");
                }
            }
        }
        public LengthSummaryVM WireSummaryVM
        {
            get { return _wireSummaryVM; }
            set
            {
                if (WireSummaryVM != value)
                {
                    _wireSummaryVM = value;
                    RaisePropertyChanged("WireSummaryVM");
                }
            }
        }
        public LengthSummaryVM ConduitSummaryVM
        {
            get { return _conduitSummaryVM; }
            set
            {
                if (ConduitSummaryVM != value)
                {
                    _conduitSummaryVM = value;
                    RaisePropertyChanged("ConduitSummaryVM");
                }
            }
        }
        public MiscCostsSummaryVM MiscSummaryVM
        {
            get { return _miscSummaryVM; }
            set
            {
                if (MiscSummaryVM != value)
                {
                    _miscSummaryVM = value;
                    RaisePropertyChanged("MiscSummaryVM");
                }
            }
        }

        public IComponentSummaryVM CurrentVM
        {
            get
            {
                return _currentVM;
            }
            private set
            {
                _currentVM = value;
                RaisePropertyChanged("CurrentVM");
            }
        }
        public MaterialSummaryIndex SelectedIndex
        {
            set
            {
                switch (value)
                {
                    case MaterialSummaryIndex.Devices:
                        CurrentVM = DeviceSummaryVM;
                        CurrentType = "Device";
                        break;
                    case MaterialSummaryIndex.Valves:
                        CurrentVM = ValveSummaryVM;
                        CurrentType = "Valve";
                        break;
                    case MaterialSummaryIndex.Controllers:
                        CurrentVM = ControllerSummaryVM;
                        CurrentType = "Controller";
                        break;
                    case MaterialSummaryIndex.Panels:
                        CurrentVM = PanelSummaryVM;
                        CurrentType = "Panel";
                        break;
                    case MaterialSummaryIndex.Wire:
                        CurrentVM = WireSummaryVM;
                        CurrentType = "Wire";
                        break;
                    case MaterialSummaryIndex.Conduit:
                        CurrentVM = ConduitSummaryVM;
                        CurrentType = "Conduit";
                        break;
                    case MaterialSummaryIndex.Misc:
                        CurrentVM = MiscSummaryVM;
                        CurrentType = "Misc";
                        break;
                    default:
                        logger.Error("Material Summary Index not found.");
                        break;
                }
                _selectedIndex = value;
                RaisePropertyChanged("SelectedIndex");
            }
            get
            {
                return _selectedIndex;
            }
        }
        public string CurrentType
        {
            get { return _currentType; }
            set
            {
                _currentType = value;
                RaisePropertyChanged("CurrentType");
            }
        }
        #endregion

        //Constructor
        public MaterialSummaryVM(TECBid bid, ChangeWatcher changeWatcher)
        {
            reinitializeTotals();
            initializeVMs();
            loadBid(bid);
            new InstanceWatcherFilter(changeWatcher).InstanceChanged += instanceChanged;
            SelectedIndex = MaterialSummaryIndex.Devices;
        }

        #region Methods
        #region Initialization Methods
        private void reinitializeTotals()
        {
            TotalTECCost = 0;
            TotalTECLabor = 0;
            TotalElecCost = 0;
            TotalElecLabor = 0;
        }

        private void loadBid(TECBid bid)
        {
            foreach (TECTypical typical in bid.Systems)
            {
                foreach (TECSystem instance in typical.Instances)
                {
                    updateTotals(addSystem(instance));
                }
            }
            foreach(TECController controller in bid.Controllers)
            {
                updateTotals(addController(controller));
            }
            foreach(TECPanel panel in bid.Panels)
            {
                updateTotals(addPanel(panel));
            }
            foreach(TECMisc misc in bid.MiscCosts)
            {
                updateTotals(MiscSummaryVM.AddCost(misc));
            }
        }

        //private void resubscribe(ChangeWatcher changeWatcher)
        //{
        //    changeWatcher.InstanceChanged -= instanceChanged;
        //    changeWatcher.InstanceChanged += instanceChanged;
        //}

        private void initializeVMs()
        {
            DeviceSummaryVM = new HardwareSummaryVM();
            ValveSummaryVM = new ValveSummaryVM();
            ControllerSummaryVM = new HardwareSummaryVM();
            PanelSummaryVM = new HardwareSummaryVM();
            WireSummaryVM = new LengthSummaryVM();
            ConduitSummaryVM = new LengthSummaryVM();
            MiscSummaryVM = new MiscCostsSummaryVM();
        }
        #endregion

        #region Add/Remove Methods
        private CostBatch addSystem(TECSystem system)
        {
            CostBatch deltas = new CostBatch();
            foreach(TECEquipment equip in system.Equipment)
            {
                deltas += (addEquipment(equip));
            }
            foreach(TECController controller in system.Controllers)
            {
                deltas += addController(controller);
            }
            foreach(TECPanel panel in system.Panels)
            {
                deltas += addPanel(panel);
            }
            foreach(TECMisc misc in system.MiscCosts)
            {
                deltas += (MiscSummaryVM.AddCost(misc));
            }
            foreach(ICost cost in system.AssociatedCosts)
            {
                deltas += (MiscSummaryVM.AddCost(cost));
            }
            return deltas;
        }
        private CostBatch addEquipment(TECEquipment equip)
        {
            CostBatch deltas = new CostBatch();
            foreach (TECSubScope ss in equip.SubScope)
            {
                deltas += (addSubScope(ss));
            }
            foreach(ICost cost in equip.AssociatedCosts)
            {
                deltas += (MiscSummaryVM.AddCost(cost));
            }
            return deltas;
        }
        private CostBatch addSubScope(TECSubScope ss)
        {
            CostBatch deltas = new CostBatch();
            foreach (IEndDevice endDev in ss.Devices)
            {
                if (endDev is TECDevice dev)
                {
                    deltas += (DeviceSummaryVM.AddHardware(dev));
                }
                else if (endDev is TECValve valve)
                {
                    deltas += (ValveSummaryVM.AddValve(valve));
                }
                else
                {
                    logger.Error("IEndDevice isn't recognized. Not valve or device. " +
                        "MaterialSummaryVM cannot add IEndDevice. IEndDevice: {0}", endDev.Name);
                }
            }
            foreach(ICost cost in ss.AssociatedCosts)
            {
                deltas += (MiscSummaryVM.AddCost(cost));
            }
            return deltas;
        }
        private CostBatch addController(TECController controller)
        {
            CostBatch deltas = new CostBatch();
            if (controller is TECProvidedController provided)
            {
                deltas += (ControllerSummaryVM.AddHardware(provided.Type));
                foreach (TECIOModule module in provided.IOModules)
                {
                    deltas += (addIOModule(module));
                }
            }
            foreach(ICost cost in controller.AssociatedCosts)
            {
                deltas += (ControllerSummaryVM.AddCost(cost));
            }
            foreach(IControllerConnection connection in controller.ChildrenConnections)
            {
                deltas += (addConnection(connection));
            }
            
            return deltas;
        }
        private CostBatch addIOModule(TECIOModule module)
        {
            //Costs associated with IO Modules will fall under controller associated costs.
            CostBatch deltas = new CostBatch();
            deltas += (ControllerSummaryVM.AddCost(module));
            foreach(ICost cost in module.AssociatedCosts)
            {
                deltas += (ControllerSummaryVM.AddCost(cost));
            }
            return deltas;
        }
        private CostBatch addPanel(TECPanel panel)
        {
            CostBatch deltas = new CostBatch();
            deltas += (PanelSummaryVM.AddHardware(panel.Type));
            foreach(ICost cost in panel.AssociatedCosts)
            {
                deltas += (PanelSummaryVM.AddCost(cost));
            }
            return deltas;
        }
        private CostBatch addConnection(IControllerConnection connection)
        {
            if (!connection.IsTypical)
            {
                CostBatch deltas = new CostBatch();
                foreach (TECElectricalMaterial connectionType in connection.Protocol.ConnectionTypes)
                {
                    deltas += (WireSummaryVM.AddRun(connectionType, connection.Length));
                }
                if (connection.ConduitType != null)
                {
                    deltas += (ConduitSummaryVM.AddRun(connection.ConduitType, connection.ConduitLength));
                }
                return deltas;
            }
            else
            {
                return new CostBatch();
            }
        }
        
        private CostBatch removeSystem(TECSystem system)
        {
            CostBatch deltas = new CostBatch();
            foreach (TECEquipment equip in system.Equipment)
            {
                deltas += removeEquipment(equip);
            }
            foreach (TECController controller in system.Controllers)
            {
                deltas += removeController(controller);
            }
            foreach (TECPanel panel in system.Panels)
            {
                deltas += removePanel(panel);
            }
            foreach(TECMisc misc in system.MiscCosts)
            {
                deltas += (MiscSummaryVM.RemoveCost(misc));
            }
            foreach(ICost cost in system.AssociatedCosts) {
                deltas += (MiscSummaryVM.RemoveCost(cost));
            }
            return deltas;
        }
        private CostBatch removeEquipment(TECEquipment equip)
        {
            CostBatch deltas = new CostBatch();
            foreach (TECSubScope ss in equip.SubScope)
            {
                deltas += (removeSubScope(ss));
            }
            foreach(ICost cost in equip.AssociatedCosts)
            {
                deltas += (MiscSummaryVM.RemoveCost(cost));
            }
            return deltas;
        }
        private CostBatch removeSubScope(TECSubScope ss)
        {
            CostBatch deltas = new CostBatch();
            foreach (IEndDevice endDev in ss.Devices)
            {
                if (endDev is TECDevice dev)
                {
                    deltas += (DeviceSummaryVM.RemoveHardware(dev));
                }
                else if (endDev is TECValve valve)
                {
                    deltas += (ValveSummaryVM.RemoveValve(valve));
                }
                else
                {
                    logger.Error("IEndDevice isn't recognized. Not valve or device. " +
                        "MaterialSummaryVM cannot add IEndDevice. IEndDevice: {0}", endDev.Name);
                }
            }
            foreach(ICost cost in ss.AssociatedCosts)
            {
                deltas += (MiscSummaryVM.RemoveCost(cost));
            }
            return deltas;
        }
        private CostBatch removeController(TECController controller)
        {
            CostBatch deltas = new CostBatch();
            if (controller is TECProvidedController provided)
            {
                deltas += (ControllerSummaryVM.RemoveHardware(provided.Type));
                foreach (TECIOModule module in provided.IOModules)
                {
                    deltas += (removeIOModule(module));
                }
            }
            foreach(ICost cost in controller.AssociatedCosts)
            {
                deltas += (ControllerSummaryVM.RemoveCost(cost));
            }
            foreach(IControllerConnection connection in controller.ChildrenConnections)
            {
                deltas += (removeConnection(connection));
            }
            return deltas;
        }
        private CostBatch removeIOModule(TECIOModule module)
        {
            //Costs associated with IO Modules will fall under controller associated costs.
            CostBatch deltas = new CostBatch();
            deltas += (ControllerSummaryVM.RemoveCost(module));
            foreach(ICost cost in module.AssociatedCosts)
            {
                deltas += (ControllerSummaryVM.RemoveCost(cost));
            }
            return deltas;
        }
        private CostBatch removePanel(TECPanel panel)
        {
            CostBatch deltas = new CostBatch();
            deltas += (PanelSummaryVM.RemoveHardware(panel.Type));
            foreach(ICost cost in panel.AssociatedCosts)
            {
                deltas += (PanelSummaryVM.RemoveCost(cost));
            }
            return deltas;
        }
        private CostBatch removeConnection(IControllerConnection connection)
        {
            CostBatch deltas = new CostBatch();
            foreach (TECElectricalMaterial connectionType in connection.Protocol.ConnectionTypes)
            {
                deltas += (WireSummaryVM.RemoveRun(connectionType, connection.Length));
            }
            if (connection.ConduitType != null)
            {
                deltas += (ConduitSummaryVM.RemoveRun(connection.ConduitType, connection.ConduitLength));
            }
            return deltas;
        }
        #endregion

        private void instanceChanged(TECChangedEventArgs args)
        {
            //Checks for a material change in the bid
            if(args.Sender is IRelatable rel && args.Value != null)
            {
                bool hasLinked = rel.LinkedObjects.Contains(args.PropertyName);
                bool isCatalog = (args.Value.GetType().IsImplementationOf(typeof(ICatalog<>)));
                if (hasLinked && !isCatalog)
                {
                    return;
                }

            }
            
            if (args.Change == Change.Add)
            {
                if (args.Value is TECSystem system)
                {
                    updateTotals(addSystem(system));
                }
                else if (args.Value is TECEquipment equip)
                {
                    updateTotals(addEquipment(equip));
                }
                else if (args.Value is TECSubScope ss)
                {
                    updateTotals(addSubScope(ss));
                }
                else if (args.Value is TECController controller)
                {
                    updateTotals(addController(controller));
                }
                else if (args.Value is TECIOModule module)
                {
                    updateTotals(addIOModule(module));
                }
                else if (args.Value is TECPanel panel)
                {
                    updateTotals(addPanel(panel));
                }
                else if (args.Value is IControllerConnection connection)
                {
                    updateTotals(addConnection(connection));
                }
                else if (args.Value is IEndDevice endDev && args.Sender is TECSubScope sub)
                {
                    if (endDev is TECDevice dev)
                    {
                        updateTotals(DeviceSummaryVM.AddHardware(dev));
                    }
                    else if (endDev is TECValve valve)
                    {
                        updateTotals(ValveSummaryVM.AddValve(valve));
                    }
                    else
                    {
                        logger.Error("IEndDevice isn't recognized. Not valve or device. " +
                        "MaterialSummaryVM cannot add IEndDevice. IEndDevice: {0}", endDev.Name);
                    }
                    
                    if (sub.Connection != null)
                    {
                        foreach(TECElectricalMaterial connectionType in sub.Connection.Protocol.ConnectionTypes)
                        {
                            updateTotals(WireSummaryVM.AddRun(connectionType, sub.Connection.Length));
                        }
                    }
                }
                else if (args.Value is TECMisc misc)
                {
                    updateTotals(MiscSummaryVM.AddCost(misc));
                }
                else if (args.Value is TECCost cost)
                {
                    if (args.Sender is TECHardware hardware)
                    {
                        logger.Error("TECHardware raised as value in instance changed args. Item: {0}, Parent:{1}", 
                            hardware.Name,
                            args.Sender.Guid.ToString());
                    }
                    else if (args.Sender is TECElectricalMaterial elecMat)
                    {
                        logger.Error("TECElectricalMaterial raise as value in instance changed args. Item: {0}, Parent:{1}",
                            elecMat.Name,
                            args.Sender.Guid.ToString());
                    }
                    else
                    {
                        updateTotals(MiscSummaryVM.AddCost(cost));
                    }
                }
            }
            else if (args.Change == Change.Remove)
            {
                if (args.Value is TECSystem system)
                {
                    updateTotals(removeSystem(system));
                }
                else if (args.Value is TECEquipment equip)
                {
                    updateTotals(removeEquipment(equip));
                }
                else if (args.Value is TECSubScope ss)
                {
                    updateTotals(removeSubScope(ss));
                }
                else if (args.Value is TECController controller)
                {
                    updateTotals(removeController(controller));
                }
                else if (args.Value is TECIOModule module)
                {
                    updateTotals(removeIOModule(module));
                }
                else if (args.Value is TECPanel panel)
                {
                    updateTotals(removePanel(panel));
                }
                else if (args.Value is IControllerConnection connection)
                {
                    updateTotals(removeConnection(connection));
                }
                else if (args.Value is IEndDevice endDev && args.Sender is TECSubScope sub)
                {
                    if (endDev is TECDevice dev)
                    {
                        updateTotals(DeviceSummaryVM.RemoveHardware(dev));
                    }
                    else if (endDev is TECValve valve)
                    {
                        updateTotals(ValveSummaryVM.RemoveValve(valve));
                    }
                    else
                    {
                        logger.Error("IEndDevice isn't recognized. Not valve or device. " +
                        "MaterialSummaryVM cannot add IEndDevice. IEndDevice: {0}", endDev.Name);
                    }
                    
                    if (sub.Connection != null)
                    {
                        foreach(TECElectricalMaterial connectionType in sub.Connection.Protocol.ConnectionTypes)
                        {
                            updateTotals(WireSummaryVM.AddRun(connectionType, sub.Connection.Length));
                        }
                    }
                }
                else if (args.Value is TECMisc misc)
                {
                    updateTotals(MiscSummaryVM.RemoveCost(misc));
                }
                else if (args.Value is TECCost cost)
                {
                    if (args.Sender is TECHardware hardware)
                    {
                        logger.Error("TECHardware raised as value in instance changed args. Item: {0}, Parent:{1}",
                            hardware.Name,
                            args.Sender.Guid.ToString());
                    }
                    else if (args.Sender is TECElectricalMaterial elecMat)
                    {
                        logger.Error("TECElectricalMaterial raise as value in instance changed args. Item: {0}, Parent:{1}",
                            elecMat.Name,
                            args.Sender.Guid.ToString());
                    }
                    else
                    {
                        updateTotals(MiscSummaryVM.RemoveCost(cost));
                    }
                }
            }
            else if (args.Change == Change.Edit)
            {
                if (args.Sender is IControllerConnection connection)
                {
                    if (args.PropertyName == "Length")
                    {
                        double deltaLength = (double)args.Value - (double)args.OldValue;
                        foreach (TECElectricalMaterial connectionType in connection.Protocol.ConnectionTypes)
                        {
                            updateTotals(WireSummaryVM.AddLength(connectionType, deltaLength));
                        }
                    }
                    else if (args.PropertyName == "ConduitLength")
                    {
                        if (connection.ConduitType != null)
                        {
                            double deltaLength = (double)args.Value - (double)args.OldValue;
                            updateTotals(ConduitSummaryVM.AddLength(connection.ConduitType, deltaLength));
                        }
                    }
                    else if (args.PropertyName == "ConnectionType")
                    {
                        updateTotals(WireSummaryVM.RemoveRun(args.OldValue as TECElectricalMaterial, connection.Length));
                        updateTotals(WireSummaryVM.AddRun(args.Value as TECElectricalMaterial, connection.Length));
                    }
                    else if (args.PropertyName == "ConduitType")
                    {
                        if (args.OldValue != null)
                        {
                            updateTotals(ConduitSummaryVM.RemoveRun(args.OldValue as TECElectricalMaterial, connection.ConduitLength));
                        }
                        if (args.Value != null)
                        {
                            updateTotals(ConduitSummaryVM.AddRun(args.Value as TECElectricalMaterial, connection.ConduitLength));
                        }
                    }
                }
                else if (args.Sender is TECMisc misc)
                {
                    if (args.PropertyName == "Quantity")
                    {
                        int deltaQuantity = (int)args.Value - (int)args.OldValue;
                        updateTotals(MiscSummaryVM.ChangeQuantity(misc, deltaQuantity));
                    }
                    else if (args.PropertyName == "Cost")
                    {
                        updateTotals(MiscSummaryVM.UpdateCost(misc));
                    }
                    else if (args.PropertyName == "Labor")
                    {
                        updateTotals(MiscSummaryVM.UpdateCost(misc));
                    }
                }
            }
            else
            {
                logger.Error("Change type in instance changed args not recognized. Change: {0}",
                    args.Change.ToString());
            }
        }

        private void updateTotals(CostBatch deltas)
        {
            TotalTECCost += deltas.GetCost(CostType.TEC);
            TotalTECLabor += deltas.GetLabor(CostType.TEC);
            TotalElecCost += deltas.GetCost(CostType.Electrical);
            TotalElecLabor += deltas.GetLabor(CostType.Electrical);
        }
        #endregion
    }
}
