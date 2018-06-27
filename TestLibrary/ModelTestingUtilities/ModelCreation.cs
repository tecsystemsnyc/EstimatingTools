using EstimatingLibrary;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TestLibrary.ModelTestingUtilities
{
    public static class ModelCreation
    {
        public static TECCatalogs TestCatalogs(Random rand)
        {
            TECCatalogs catalogs = new TECCatalogs();

            //Tags
            rand.RepeatAction(() => catalogs.Tags.Add(TestTag(rand)), 10);

            //Associated Costs
            rand.RepeatAction(() => catalogs.AssociatedCosts.Add(TestCost(catalogs, rand, CostType.TEC)), 10);
            rand.RepeatAction(() => catalogs.AssociatedCosts.Add(TestCost(catalogs, rand, CostType.Electrical)), 10);

            //Conduit Types
            rand.RepeatAction(() => catalogs.ConduitTypes.Add(TestElectricalMaterial(catalogs, rand, "Conduit Type")), 10);

            //ConnectionTypes
            rand.RepeatAction(() => catalogs.ConnectionTypes.Add(TestConnectionType(catalogs, rand)), 10);

            //Manufacturers
            rand.RepeatAction(() => catalogs.Manufacturers.Add(TestManufacturer(rand)), 10);

            //Protocols
            rand.RepeatAction(() => catalogs.Protocols.Add(TestProtocol(catalogs, rand)), 10);

            //Devices
            rand.RepeatAction(() => catalogs.Devices.Add(TestDevice(catalogs, rand)), 10);

            //IO Modules
            rand.RepeatAction(() => catalogs.IOModules.Add(TestIOModule(catalogs, rand)), 10);

            //Controller Types
            rand.RepeatAction(() => catalogs.ControllerTypes.Add(TestControllerType(catalogs, rand)), 10);

            //Panel Types
            rand.RepeatAction(() => catalogs.PanelTypes.Add(TestPanelType(catalogs, rand)), 10);

            //Valves
            rand.RepeatAction(() => catalogs.Valves.Add(TestValve(catalogs, rand)), 10);

            return catalogs;
        }

        public static TECIO TestIO(Random rand, IOType type)
        {
            return new TECIO(type)
            {
                Quantity = rand.Next(100),
            };
        }

        #region Catalog Models
        public static TECTag TestTag(Random rand)
        {
            return new TECTag()
            {
                Label = string.Format("Test Tag #{0}", rand.Next(100))
            };
        }
        public static TECAssociatedCost TestCost(TECCatalogs catalogs, Random rand, CostType type)
        {
            TECAssociatedCost cost = new TECAssociatedCost(type);
            cost.Description = string.Format("Test {0} Cost #{1}", type, rand.Next(100));
            cost.AssignRandomCostProperties(catalogs, rand);
            return cost;
        }
        public static TECElectricalMaterial TestElectricalMaterial(TECCatalogs catalogs, Random rand, string type)
        {
            TECElectricalMaterial mat = new TECElectricalMaterial();
            mat.Description = string.Format("Test {0} #{1}", type, rand.Next(100));
            mat.AssignRandomElectricalMaterialProperties(catalogs, rand);
            return mat;
        }
        public static TECConnectionType TestConnectionType(TECCatalogs catalogs, Random rand)
        {
            TECConnectionType type = new TECConnectionType();
            type.Description = string.Format("Test Connection Type #{0}", rand.Next(100));
            type.AssignRandomElectricalMaterialProperties(catalogs, rand);
            type.PlenumCost = (rand.NextDouble() * 100);
            type.PlenumLabor = (rand.NextDouble() * 100);
            return type;
        }
        public static TECManufacturer TestManufacturer(Random rand)
        {
            return new TECManufacturer()
            {
                Label = string.Format("Test Manufacturer #{0}", rand.Next(100)),
                Multiplier = (rand.NextDouble() * 100)
            };
        }
        public static TECProtocol TestProtocol(TECCatalogs catalogs, Random rand)
        {
            List<TECConnectionType> connectionTypes = new List<TECConnectionType>();
            rand.RepeatAction(() => connectionTypes.Add(catalogs.ConnectionTypes.RandomElement(rand)), connectionTypes.Count());
            return new TECProtocol(connectionTypes)
            {
                Label = string.Format("Test Protocol #{0}", rand.Next(100))
            };
        }
        public static TECDevice TestDevice(TECCatalogs catalogs, Random rand)
        {
            List<TECConnectionType> types = catalogs.ConnectionTypes.RandomElements(rand, false, 10);
            List<TECProtocol> protocols = catalogs.Protocols.RandomElements(rand, true, 10);
            TECManufacturer man = catalogs.Manufacturers.RandomElement(rand);
            TECDevice dev = new TECDevice(types, protocols, man);
            dev.Description = string.Format("Test Device #{0}", rand.Next(100));
            dev.AssignRandomCostProperties(catalogs, rand);
            return dev;
        }
        public static TECIOModule TestIOModule(TECCatalogs catalogs, Random rand)
        {
            TECManufacturer man = catalogs.Manufacturers.RandomElement(rand);
            TECIOModule mod = new TECIOModule(man);
            mod.Description = string.Format("Test Module #{0}", rand.Next(100));
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
            type.Description = string.Format("Test Controller Type #{0}", rand.Next(100));
            type.IO.Add(TestIO(rand, IOType.AI));
            type.IO.Add(TestIO(rand, IOType.AO));
            type.IO.Add(TestIO(rand, IOType.DI));
            type.IO.Add(TestIO(rand, IOType.DO));
            type.AssignRandomCostProperties(catalogs, rand);
            return type;
        }
        public static TECPanelType TestPanelType(TECCatalogs catalogs, Random rand)
        {
            TECManufacturer man = catalogs.Manufacturers.RandomElement(rand);
            TECPanelType type = new TECPanelType(man);
            type.Description = string.Format("Test Panel Type #{0}", rand.Next(100));
            type.AssignRandomCostProperties(catalogs, rand);
            return type;
        }
        public static TECValve TestValve(TECCatalogs catalogs, Random rand)
        {
            TECManufacturer man = catalogs.Manufacturers.RandomElement(rand);
            TECDevice dev = catalogs.Devices.RandomElement(rand);
            TECValve valve = new TECValve(man, dev);
            valve.Description = string.Format("Test Valve #{0}", rand.Next(100));
            valve.AssignRandomCostProperties(catalogs, rand);
            return valve;
        }
        #endregion

        public static TECBid TestBid(Random rand)
        {
            TECBid bid = new TECBid();

            //Bid Info
            bid.Name = "Randomized Test Bid";
            bid.BidNumber = string.Format("{0}-{1}", rand.Next(1000, 9999), rand.Next(1000, 9999));
            bid.DueDate = new DateTime(rand.Next(2000, 3000), rand.Next(12), rand.Next(29));
            bid.Salesperson = string.Format("Salesperson #{0}", rand.Next(100));
            bid.Estimator = string.Format("Estimator #{0}", rand.Next(100));

            //Bid Objects
            bid.ExtraLabor = TestLabor(rand, bid);
            bid.Parameters = TestParameters(rand, bid);
            bid.Catalogs = TestCatalogs(rand);

            //Locations
            int numFloors = rand.Next(100);
            for(int floor = 1; floor < numFloors; floor++)
            {
                bid.Locations.Add(new TECLocation()
                {
                    Name = string.Format("Level {0}", floor),
                    Label = floor.ToString(),
                });
            }

            //Scope Branches
            rand.RepeatAction(() => bid.ScopeTree.Add(TestScopeBranch(rand, 5)), 10);

            //Notes
            rand.RepeatAction(() => bid.Notes.Add(TestLabel(rand, "Note")), 10);

            //Exclusions
            rand.RepeatAction(() => bid.Exclusions.Add(TestLabel(rand, "Exclusion")), 10);

            //Misc Costs
            rand.RepeatAction(() => bid.MiscCosts.Add(TestMisc(bid.Catalogs, rand, CostType.TEC)), 10);
            rand.RepeatAction(() => bid.MiscCosts.Add(TestMisc(bid.Catalogs, rand, CostType.Electrical)), 10);

            //Controllers
            rand.RepeatAction(() => bid.AddController(TestProvidedController(bid.Catalogs, rand)), 10);
            rand.RepeatAction(() => bid.AddController(TestFBOController(bid.Catalogs, rand)), 10);

            //Panels
            rand.RepeatAction(() => bid.Panels.Add(TestPanel(bid.Catalogs, rand)), 10);

            //Systems

        }

        #region Bid Models
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
        public static TECParameters TestParameters(Random rand, TECBid bid)
        {
            return new TECParameters(bid.Guid)
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
            TECScopeBranch branch = new TECScopeBranch(false);
            branch.Label = string.Format("Branch {0}", branch.Guid.ToString().Substring(0, 5));
            rand.RepeatAction(() => branch.Branches.Add(TestScopeBranch(rand, rand.Next(maxDepth))), rand.Next(5));
            return branch;
        }
        public static TECLabeled TestLabel(Random rand, string type)
        {
            TECLabeled labeled = new TECLabeled();
            labeled.Label = string.Format("{0} {1}", type, labeled.Guid.ToString().Substring(0, 5));
            return labeled;
        }
        public static TECMisc TestMisc(TECCatalogs catalogs, Random rand, CostType type)
        {
            TECMisc misc = new TECMisc(type, false);
            misc.Description = string.Format("Misc {0}", type.ToString());
            misc.Quantity = rand.Next(10);
            misc.AssignRandomCostProperties(catalogs, rand);
            return misc;
        }
        public static TECProvidedController TestProvidedController(TECCatalogs catalogs, Random rand)
        {
            TECProvidedController controller = new TECProvidedController(catalogs.ControllerTypes.RandomElement(rand), false);
            controller.Description = "Provided Controller";
            controller.AssignRandomScopeProperties(catalogs, rand);
            return controller;
        }
        public static TECFBOController TestFBOController(TECCatalogs catalogs, Random rand)
        {
            TECFBOController controller = new TECFBOController(false, catalogs);
            controller.Description = "FBO Controller";
            controller.AssignRandomScopeProperties(catalogs, rand);
            return controller;
        }
        public static TECPanel TestPanel(TECCatalogs catalogs, Random rand)
        {
            TECPanel panel = new TECPanel(catalogs.PanelTypes.RandomElement(rand), false);
            panel.Description = "Panel";
            panel.AssignRandomScopeProperties(catalogs, rand);
            return panel;
        }
        public static TECPoint TestPoint(TECCatalogs catalogs, Random rand)
        {
            throw new NotImplementedException();
        }
        public static TECTypical TestTypical(TECCatalogs catalogs, Random rand)
        {
            TECTypical typ = new TECTypical();
            typ.Description = "Typical";
            typ.ProposeEquipment = rand.NextBool();
            typ.AssignRandomScopeProperties(catalogs, rand);

            throw new NotImplementedException();
        }
        #endregion
    }
}
