using EstimatingLibrary;
using EstimatingLibrary.Interfaces;
using EstimatingLibrary.Utilities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TestLibrary.ModelTestingUtilities
{
    public static class ModelCreation
    {
        public static TECCatalogs TestCatalogs(Random rand, int maxEachItem = 3)
        {
            TECCatalogs catalogs = new TECCatalogs();

            //Tags
            rand.RepeatAction(() => catalogs.Tags.Add(TestTag(rand)), maxEachItem);

            //Associated Costs
            rand.RepeatAction(() => catalogs.AssociatedCosts.Add(TestCost(catalogs, rand, CostType.TEC)), maxEachItem);
            rand.RepeatAction(() => catalogs.AssociatedCosts.Add(TestCost(catalogs, rand, CostType.Electrical)), maxEachItem);

            //Conduit Types
            rand.RepeatAction(() => catalogs.ConduitTypes.Add(TestElectricalMaterial(catalogs, rand, "Conduit Type")), maxEachItem);

            //ConnectionTypes
            rand.RepeatAction(() => catalogs.ConnectionTypes.Add(TestConnectionType(catalogs, rand)), maxEachItem);

            //Manufacturers
            rand.RepeatAction(() => catalogs.Manufacturers.Add(TestManufacturer(rand)), maxEachItem);

            //Protocols
            rand.RepeatAction(() => catalogs.Protocols.Add(TestProtocol(catalogs, rand)), maxEachItem);

            //Devices
            rand.RepeatAction(() => catalogs.Devices.Add(TestDevice(catalogs, rand)), maxEachItem);

            //IO Modules
            rand.RepeatAction(() => catalogs.IOModules.Add(TestIOModule(catalogs, rand)), maxEachItem);

            //Controller Types
            rand.RepeatAction(() => catalogs.ControllerTypes.Add(TestControllerType(catalogs, rand)), maxEachItem);

            //Panel Types
            rand.RepeatAction(() => catalogs.PanelTypes.Add(TestPanelType(catalogs, rand)), maxEachItem);

            //Valves
            rand.RepeatAction(() => catalogs.Valves.Add(TestValve(catalogs, rand)), maxEachItem);
            
            return catalogs;
        }

        public static TECBid TestBid(Random rand, int maxEachItem = 3)
        {
            TECBid bid = new TECBid();

            //Bid Info
            bid.Name = "Randomized Test Bid";
            bid.BidNumber = string.Format("{0}-{1}", rand.Next(1000, 9999), rand.Next(1000, 9999));
            bid.DueDate = new DateTime(rand.Next(2000, 3000), rand.Next(1, 12), rand.Next(1, 28));
            bid.Salesperson = string.Format("Salesperson #{0}", rand.Next(1, 100));
            bid.Estimator = string.Format("Estimator #{0}", rand.Next(1, 100));

            //Bid Objects
            bid.ExtraLabor = TestLabor(rand, bid);
            bid.Parameters = TestParameters(rand, bid);
            bid.Catalogs = TestCatalogs(rand, maxEachItem);

            //Locations
            int numFloors = rand.Next(1, maxEachItem);
            for (int floor = 1; floor < numFloors; floor++)
            {
                bid.Locations.Add(new TECLocation()
                {
                    Name = string.Format("Level {0}", floor),
                    Label = floor.ToString(),
                });
            }

            //Scope Branches
            rand.RepeatAction(() => bid.ScopeTree.Add(TestScopeBranch(rand, maxEachItem)), maxEachItem);

            //Notes
            rand.RepeatAction(() => bid.Notes.Add(TestLabel(rand)), maxEachItem);

            //Internal Notes
            rand.RepeatAction(() => bid.InternalNotes.Add(TestInternalNote(rand)), maxEachItem);

            //Exclusions
            rand.RepeatAction(() => bid.Exclusions.Add(TestLabel(rand)), maxEachItem);

            //Misc Costs
            rand.RepeatAction(() => bid.MiscCosts.Add(TestMisc(bid.Catalogs, rand, CostType.TEC)), maxEachItem);
            rand.RepeatAction(() => bid.MiscCosts.Add(TestMisc(bid.Catalogs, rand, CostType.Electrical)), maxEachItem);

            //Controllers
            rand.RepeatAction(() => bid.AddController(TestProvidedController(bid.Catalogs, rand)), maxEachItem);
            rand.RepeatAction(() => bid.AddController(TestFBOController(bid.Catalogs, rand)), maxEachItem);

            //Panels
            rand.RepeatAction(() => bid.Panels.Add(TestPanel(bid.Catalogs, rand)), maxEachItem);

            //Systems
            rand.RepeatAction(() => bid.Systems.Add(TestTypical(bid.Catalogs, rand)), maxEachItem);
            foreach (TECTypical typ in bid.Systems)
            {
                rand.RepeatAction(() => typ.AddInstance(), maxEachItem);
            }

            //Connections
            rand.RepeatAction(() => AddBidConnections(bid, bid.Systems, bid.Catalogs, rand), 5);


            //Assign Locations
            foreach (TECController controller in bid.Controllers)
            {
                setLocation(controller, rand);
            }
            foreach (TECPanel panel in bid.Panels)
            {
                setLocation(panel, rand);
            }
            foreach (TECTypical typ in bid.Systems)
            {
                setLocation(typ, rand);
            }

            void setLocation(TECLocated located, Random randNum)
            {
                //if (randNum.NextBool())
                //{
                //    located.Location = bid.Locations.RandomElement(randNum);
                //}
                located.Location = bid.Locations.RandomElement(randNum);
                foreach (TECLocated child in located.GetDirectChildren().Where(x => x is TECLocated))
                {
                    setLocation(child, randNum);
                }
            }

            return bid;
        }

        public static TECTemplates TestTemplates(Random rand)
        {
            TECTemplates templates = new TECTemplates();

            templates.Catalogs = TestCatalogs(rand, 5);

            //Parameters
            rand.RepeatAction(() => templates.Templates.Parameters.Add(TestParameters(rand)), 5);

            //Systems
            rand.RepeatAction(() => templates.Templates.SystemTemplates.Add(TestSystem(templates.Catalogs, rand)), 10);

            //Equipment
            rand.RepeatAction(() => templates.Templates.EquipmentTemplates.Add(TestEquipment(templates.Catalogs, rand)), 10);

            //SubScope
            rand.RepeatAction(() => templates.Templates.SubScopeTemplates.Add(TestSubScope(templates.Catalogs, rand)), 10);

            //Controllers
            rand.RepeatAction(() => templates.Templates.ControllerTemplates.Add(TestProvidedController(templates.Catalogs, rand)), 10);

            //Misc Costs
            rand.RepeatAction(() => templates.Templates.MiscCostTemplates.Add(TestMisc(templates.Catalogs, rand, CostType.TEC)), 10);
            rand.RepeatAction(() => templates.Templates.MiscCostTemplates.Add(TestMisc(templates.Catalogs, rand, CostType.Electrical)), 10);

            //Panels
            rand.RepeatAction(() => templates.Templates.PanelTemplates.Add(TestPanel(templates.Catalogs, rand)), 10);

            return templates;
        }
        
        #region Catalog Models
        public static TECTag TestTag(Random rand)
        {
            TECTag tag = new TECTag();
            tag.AssignTestLabel();
            return tag;
        }
        public static TECAssociatedCost TestCost(TECCatalogs catalogs, Random rand, CostType type)
        {
            TECAssociatedCost cost = new TECAssociatedCost(type);
            cost.Description = string.Format("Test Associated {0} Cost", type);
            cost.AssignRandomCostProperties(catalogs, rand);
            return cost;
        }
        public static TECElectricalMaterial TestElectricalMaterial(TECCatalogs catalogs, Random rand, string type)
        {
            TECElectricalMaterial mat = new TECElectricalMaterial();
            mat.Description = string.Format("Test {0}", type);
            mat.AssignRandomElectricalMaterialProperties(catalogs, rand);
            return mat;
        }
        public static TECConnectionType TestConnectionType(TECCatalogs catalogs, Random rand)
        {
            TECConnectionType type = new TECConnectionType();
            type.Description = string.Format("Test Connection Type");
            type.AssignRandomElectricalMaterialProperties(catalogs, rand);
            type.PlenumCost = (rand.NextDouble() * 100);
            type.PlenumLabor = (rand.NextDouble() * 100);
            return type;
        }
        public static TECManufacturer TestManufacturer(Random rand)
        {
            TECManufacturer man = new TECManufacturer();
            man.AssignTestLabel();
            man.Multiplier = (rand.NextDouble() * 100);
            return man;
        }
        public static TECProtocol TestProtocol(TECCatalogs catalogs, Random rand)
        {
            List<TECConnectionType> connectionTypes = new List<TECConnectionType>();
            rand.RepeatAction(() => connectionTypes.Add(catalogs.ConnectionTypes.RandomElement(rand)), 5);
            TECProtocol prot = new TECProtocol(connectionTypes);
            prot.AssignTestLabel();
            return prot;
        }
        public static TECDevice TestDevice(TECCatalogs catalogs, Random rand)
        {
            List<TECConnectionType> types = catalogs.ConnectionTypes.RandomElements(rand, false, 10);
            List<TECProtocol> protocols = catalogs.Protocols.RandomElements(rand, true, 10);
            TECManufacturer man = catalogs.Manufacturers.RandomElement(rand);
            TECDevice dev = new TECDevice(types, protocols, man);
            dev.Description = string.Format("Test Device");
            dev.AssignRandomCostProperties(catalogs, rand);
            return dev;
        }
        public static TECIOModule TestIOModule(TECCatalogs catalogs, Random rand)
        {
            TECManufacturer man = catalogs.Manufacturers.RandomElement(rand);
            TECIOModule mod = new TECIOModule(man);
            mod.Description = string.Format("Test Module");
            mod.IO.Add(TestIO(rand, IOType.AI));
            mod.IO.Add(TestIO(rand, IOType.AO));
            mod.IO.Add(TestIO(rand, IOType.DI));
            mod.IO.Add(TestIO(rand, IOType.DO));
            mod.AssignRandomCostProperties(catalogs, rand);
            return mod;
        }
        public static TECControllerType TestControllerType(TECCatalogs catalogs, Random rand)
        {
            TECManufacturer man = catalogs.Manufacturers.RandomElement(rand);
            TECControllerType type = new TECControllerType(man);
            type.Description = string.Format("Test Controller Type");
            type.IO.Add(TestIO(rand, IOType.AI));
            type.IO.Add(TestIO(rand, IOType.AO));
            type.IO.Add(TestIO(rand, IOType.DI));
            type.IO.Add(TestIO(rand, IOType.DO));
            type.IO.Add(TestIO(rand, catalogs.Protocols.RandomElement(rand)));
            type.AssignRandomCostProperties(catalogs, rand);
            return type;
        }
        public static TECPanelType TestPanelType(TECCatalogs catalogs, Random rand)
        {
            TECManufacturer man = catalogs.Manufacturers.RandomElement(rand);
            TECPanelType type = new TECPanelType(man);
            type.Description = string.Format("Test Panel Type");
            type.AssignRandomCostProperties(catalogs, rand);
            return type;
        }
        public static TECValve TestValve(TECCatalogs catalogs, Random rand)
        {
            TECManufacturer man = catalogs.Manufacturers.RandomElement(rand);
            TECDevice dev = catalogs.Devices.RandomElement(rand);
            TECValve valve = new TECValve(man, dev);
            valve.Description = string.Format("Test Valve");
            valve.AssignRandomCostProperties(catalogs, rand);
            return valve;
        }
        #endregion
        
        #region Scope Models
        public static TECExtraLabor TestLabor(Random rand, TECBid bid)
        {
            return new TECExtraLabor(bid.Guid)
            {
                PMExtraHours = rand.NextDouble(),
                CommExtraHours = rand.NextDouble(),
                ENGExtraHours = rand.NextDouble(),
                SoftExtraHours = rand.NextDouble(),
                GraphExtraHours = rand.NextDouble()
            };
        }
        public static TECParameters TestParameters(Random rand, TECScopeManager scopeManager = null)
        {
            Guid guid = scopeManager?.Guid ?? Guid.NewGuid();
            return new TECParameters(guid)
            {
                PMCoef = rand.NextDouble(),
                PMCoefStdError = rand.NextDouble(),
                PMRate = (rand.NextDouble() * 100),

                ENGCoef = rand.NextDouble(),
                ENGCoefStdError = rand.NextDouble(),
                ENGRate = (rand.NextDouble() * 100),

                CommCoef = rand.NextDouble(),
                CommCoefStdError = rand.NextDouble(),
                CommRate = (rand.NextDouble() * 100),

                SoftCoef = rand.NextDouble(),
                SoftCoefStdError = rand.NextDouble(),
                SoftRate = (rand.NextDouble() * 100),

                GraphCoef = rand.NextDouble(),
                GraphCoefStdError = rand.NextDouble(),
                GraphRate = (rand.NextDouble() * 100),

                ElectricalRate = (rand.NextDouble() * 100),
                ElectricalSuperRate = (rand.NextDouble() * 100),
                ElectricalSuperRatio = rand.NextDouble(),
                ElectricalNonUnionRate = (rand.NextDouble() * 100),
                ElectricalSuperNonUnionRate = (rand.NextDouble() * 100),

                IsTaxExempt = (rand.NextDouble() < .5),
                ElectricalIsOnOvertime = (rand.NextDouble() < .5),
                ElectricalIsUnion = (rand.NextDouble() < .5)
            };
        }
        
        public static TECScopeBranch TestScopeBranch(Random rand, int maxDepth)
        {
            TECScopeBranch branch = new TECScopeBranch();
            branch.AssignTestLabel();
            if(maxDepth > 0)
            {
                int nextMaxDepth = rand.Next(maxDepth);
                if (nextMaxDepth >= maxDepth) throw new Exception("This may cause an infinite loop.");
                rand.RepeatAction(() => branch.Branches.Add(TestScopeBranch(rand, nextMaxDepth)), rand.Next(1, 5));
            }
            return branch;
        }
        public static TECLabeled TestLabel(Random rand)
        {
            TECLabeled labeled = new TECLabeled();
            labeled.AssignTestLabel();
            return labeled;
        }
        public static TECInternalNote TestInternalNote(Random rand)
        {
            TECInternalNote labeled = new TECInternalNote();
            labeled.AssignTestLabel();
            return labeled;
        }
        public static TECMisc TestMisc(TECCatalogs catalogs, Random rand, CostType type)
        {
            TECMisc misc = new TECMisc(type);
            misc.Description = string.Format("Test Misc {0}", type.ToString());
            misc.Quantity = rand.Next(1, 10);
            misc.AssignRandomCostProperties(catalogs, rand);
            return misc;
        }
        public static TECProvidedController TestProvidedController(TECCatalogs catalogs, Random rand)
        {
            TECProvidedController controller = new TECProvidedController(catalogs.ControllerTypes.RandomElement(rand));
            controller.Description = "Test Provided Controller";
            controller.AssignRandomScopeProperties(catalogs, rand);
            return controller;
        }
        public static TECFBOController TestFBOController(TECCatalogs catalogs, Random rand)
        {
            TECFBOController controller = new TECFBOController(catalogs);
            controller.Description = "Test FBO Controller";
            controller.AssignRandomScopeProperties(catalogs, rand);
            return controller;
        }
        public static TECPanel TestPanel(TECCatalogs catalogs, Random rand)
        {
            TECPanel panel = new TECPanel(catalogs.PanelTypes.RandomElement(rand));
            panel.Description = "Test Panel";
            panel.AssignRandomScopeProperties(catalogs, rand);
            return panel;
        }
        public static IOType TestIOType(Random rand)
        {
            int randInt = rand.Next(1, 6);
            switch (randInt)
            {
                case 0:
                    return IOType.AI;
                case 1:
                    return IOType.AO;
                case 2:
                    return IOType.DI;
                case 3:
                    return IOType.DO;
                case 4:
                    return IOType.UI;
                case 5:
                    return IOType.UO;
                default:
                    return IOType.Protocol;
            }
        }
        public static IOType TestPointType(Random rand)
        {
            int randInt = rand.Next(1, 3);
            switch (randInt)
            {
                case 0:
                    return IOType.AI;
                case 1:
                    return IOType.AO;
                case 2:
                    return IOType.DI;
                case 3:
                    return IOType.DO;
                default:
                    return IOType.AI;
            }
        }
        public static TECIO TestIO(Random rand, IOType type)
        {
            return new TECIO(type)
            {
                Quantity = rand.Next(1, 100)
            };
        }
        public static TECIO TestIO(Random rand, TECProtocol protocol)
        {
            return new TECIO(protocol)
            {
                Quantity = rand.Next(1, 5)
            };
        }
        public static TECPoint TestPoint(Random rand, IOType type = IOType.Protocol)
        {
            TECPoint point = new TECPoint();
            point.AssignTestLabel();
            if (!TECIO.PointIO.Contains(type))
            {
                point.Type = TestPointType(rand);
            }
            point.Quantity = rand.Next(1, 5);
            return point;
        }
        public static TECSubScope TestSubScope(TECCatalogs catalogs, Random rand)
        {
            TECSubScope ss = new TECSubScope();
            ss.Description = "Test SubScope";
            ss.AssignRandomScopeProperties(catalogs, rand);
            List<IEndDevice> endDevs = new List<IEndDevice>(catalogs.Devices.RandomElements(rand, false, 5));
            endDevs.AddRange(catalogs.Valves.RandomElements(rand, false, 5));
            endDevs.ForEach(dev => ss.Devices.Add(dev));
            rand.RepeatAction(() => ss.Points.Add(TestPoint(rand)), 5);
            return ss;
        }
        public static TECEquipment TestEquipment(TECCatalogs catalogs, Random rand)
        {
            TECEquipment equip = new TECEquipment();
            equip.Description = "Test Equipment";
            equip.AssignRandomScopeProperties(catalogs, rand);
            rand.RepeatAction(() => equip.SubScope.Add(TestSubScope(catalogs, rand)), 5);
            return equip;
        }
        public static TECSystem TestSystem(TECCatalogs catalogs, Random rand)
        {
            TECSystem sys = new TECSystem();
            sys.Description = "Test System";
            sys.AssignRandomSystemProperties(catalogs, rand);
            return sys;
        }
        public static TECTypical TestTypical(TECCatalogs catalogs, Random rand)
        {
            TECTypical typ = new TECTypical();
            typ.Description = "Typical";
            typ.AssignRandomSystemProperties(catalogs, rand);
            return typ;
        }

        #endregion

        #region Case Setup
        public static void AddSystemConnections(TECSystem system, TECCatalogs catalogs, Random rand)
        {
            var hardwiredDevice = catalogs.Devices.Where(x => x.HardwiredConnectionTypes.Count > 0).RandomElement(rand) ??
                new TECDevice(catalogs.ConnectionTypes.RandomElements(rand, false), new List<TECProtocol>(), catalogs.Manufacturers.RandomElement(rand));
            if (!catalogs.Devices.Contains(hardwiredDevice)) catalogs.Devices.Add(hardwiredDevice);
            var networkDevice = catalogs.Devices.Where(x => x.PossibleProtocols.Count > 0 && x != hardwiredDevice).RandomElement(rand) ??
                new TECDevice(new List<TECConnectionType>(), new List<TECProtocol> { catalogs.Protocols.RandomElement(rand) }, catalogs.Manufacturers.RandomElement(rand));

            var hardwiredSubScope = new TECSubScope();
            hardwiredSubScope.Devices.Add(hardwiredDevice);
            hardwiredSubScope.AddPoint(TestPoint(rand));

            var hardwiredProtocol = hardwiredSubScope.AvailableProtocols.Where(x => x is TECHardwiredProtocol).RandomElement(rand) as TECHardwiredProtocol;

            var networkSubScope = new TECSubScope();
            networkSubScope.Devices.Add(networkDevice);
            networkSubScope.AddPoint(TestPoint(rand));

            var networkProtocol = networkSubScope.AvailableProtocols.Where(x => x is TECProtocol).RandomElement(rand) as TECProtocol;

            system.Equipment.RandomElement(rand).SubScope.Add(hardwiredSubScope);
            system.Equipment.RandomElement(rand).SubScope.Add(networkSubScope);

            var hardwiredControllerType = catalogs.ControllerTypes.Where(x => new TECProvidedController(x).AvailableIO.Contains((hardwiredSubScope as IConnectable).HardwiredIO)).RandomElement(rand) ??
                new TECControllerType(catalogs.Manufacturers.RandomElement(rand));
            if (!catalogs.ControllerTypes.Contains(hardwiredControllerType))
            {
                catalogs.ControllerTypes.Add(hardwiredControllerType);
                hardwiredControllerType.IO.AddRange((hardwiredSubScope as IConnectable).HardwiredIO.ToList());
            }
            var hardwiredController = system.Controllers.Where(x => x is TECProvidedController y && y.Type == hardwiredControllerType).RandomElement(rand) ??
                new TECProvidedController(hardwiredControllerType);
            if (!system.Controllers.Contains(hardwiredController)) system.AddController(hardwiredController);

            var networkControllerType = catalogs.ControllerTypes.Where(x => new TECProvidedController(x).AvailableProtocols.Any(y => y == networkProtocol)).RandomElement(rand) ??
                new TECControllerType(catalogs.Manufacturers.RandomElement(rand));
            if (!catalogs.ControllerTypes.Contains(networkControllerType))
            {
                catalogs.ControllerTypes.Add(networkControllerType);
                networkControllerType.IO.Add(new TECIO(networkProtocol));
            }
            var networkController = system.Controllers.Where(x => x is TECProvidedController y && y.Type == networkControllerType && y.AvailableProtocols.Contains(networkProtocol)).RandomElement(rand) ??
                new TECProvidedController(networkControllerType);
            if (!system.Controllers.Contains(networkController)) system.AddController(networkController);

            var hardwiredConnection = hardwiredController.Connect(hardwiredSubScope, hardwiredProtocol);
            var networkConnection = networkController.Connect(networkSubScope, networkProtocol);

            hardwiredConnection.AssignRandomConnectionProperties(catalogs, rand);
            networkConnection.AssignRandomConnectionProperties(catalogs, rand);

        }
        public static void AddBidConnections(TECBid bid, IEnumerable<TECTypical> typicals, TECCatalogs catalogs, Random rand)
        {
            var typ = typicals.Where(x => x.Instances.Count > 0).RandomElement(rand);
            var system = typ.Instances.RandomElement(rand);

            var hardwiredDevice = catalogs.Devices.Where(x => x.HardwiredConnectionTypes.Count > 0).RandomElement(rand) ??
                new TECDevice(catalogs.ConnectionTypes.RandomElements(rand, false), new List<TECProtocol>(), catalogs.Manufacturers.RandomElement(rand));
            if (!catalogs.Devices.Contains(hardwiredDevice)) catalogs.Devices.Add(hardwiredDevice);
            var networkDevice = catalogs.Devices.Where(x => x.PossibleProtocols.Count > 0 && x != hardwiredDevice).RandomElement(rand) ??
                new TECDevice(new List<TECConnectionType>(), new List<TECProtocol> { catalogs.Protocols.RandomElement(rand) }, catalogs.Manufacturers.RandomElement(rand));

            var hardwiredSubScope = new TECSubScope();
            hardwiredSubScope.Devices.Add(hardwiredDevice);
            hardwiredSubScope.AddPoint(TestPoint(rand));

            var hardwiredProtocol = hardwiredSubScope.AvailableProtocols.Where(x => x is TECHardwiredProtocol).RandomElement(rand) as TECHardwiredProtocol;

            var networkSubScope = new TECSubScope();
            networkSubScope.Devices.Add(networkDevice);
            networkSubScope.AddPoint(TestPoint(rand));

            var networkProtocol = networkSubScope.AvailableProtocols.Where(x => x is TECProtocol).RandomElement(rand) as TECProtocol;

            typ.Equipment.RandomElement(rand).SubScope.Add(hardwiredSubScope);
            typ.Equipment.RandomElement(rand).SubScope.Add(networkSubScope);

            networkSubScope = system.GetAllSubScope().First(x => typ.GetInstancesFromTypical(networkSubScope).Contains(x));
            hardwiredSubScope = system.GetAllSubScope().First(x => typ.GetInstancesFromTypical(hardwiredSubScope).Contains(x));

            var hardwiredControllerType = catalogs.ControllerTypes.Where(x => new TECProvidedController(x).AvailableIO.Contains((hardwiredSubScope as IConnectable).HardwiredIO)).RandomElement(rand) ??
                new TECControllerType(catalogs.Manufacturers.RandomElement(rand));
            if (!catalogs.ControllerTypes.Contains(hardwiredControllerType))
            {
                catalogs.ControllerTypes.Add(hardwiredControllerType);
                hardwiredControllerType.IO.AddRange((hardwiredSubScope as IConnectable).HardwiredIO.ToList());
            }
            var hardwiredController = bid.Controllers.Where(x => x is TECProvidedController y && y.Type == hardwiredControllerType).RandomElement(rand) ??
                new TECProvidedController(hardwiredControllerType);
            if (!bid.Controllers.Contains(hardwiredController)) bid.AddController(hardwiredController);

            var networkControllerType = catalogs.ControllerTypes.Where(x => new TECProvidedController(x).AvailableProtocols.Any(y => y == networkProtocol)).RandomElement(rand) ??
                new TECControllerType(catalogs.Manufacturers.RandomElement(rand));
            if (!catalogs.ControllerTypes.Contains(networkControllerType))
            {
                catalogs.ControllerTypes.Add(networkControllerType);
                networkControllerType.IO.Add(new TECIO(networkProtocol));
            }
            var networkController = bid.Controllers.Where(x => x is TECProvidedController y && y.Type == networkControllerType && y.AvailableProtocols.Contains(networkProtocol)).RandomElement(rand) ??
                new TECProvidedController(networkControllerType);
            if (!bid.Controllers.Contains(networkController)) bid.AddController(networkController);

            var hardwiredConnection = hardwiredController.Connect(hardwiredSubScope, hardwiredProtocol);
            var networkConnection = networkController.Connect(networkSubScope, networkProtocol);

            hardwiredConnection.AssignRandomConnectionProperties(catalogs, rand);
            networkConnection.AssignRandomConnectionProperties(catalogs, rand);

        }
        #endregion
    }
}
