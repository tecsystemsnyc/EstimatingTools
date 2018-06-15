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

            //Associated Costs
            catalogs.AssociatedCosts.Add(TestCost(rand, CostType.TEC));
            catalogs.AssociatedCosts.Add(TestCost(rand, CostType.Electrical));

            //Tags
            catalogs.Tags.Add(TestTag(rand));
            catalogs.Tags.Add(TestTag(rand));

            //Conduit Types
            catalogs.ConduitTypes.Add(TestElectricalMaterial(rand, "Conduit Type", catalogs));
            catalogs.ConduitTypes.Add(TestElectricalMaterial(rand, "Conduit Type", catalogs));

            //ConnectionTypes
            var connectionType1 = new TECConnectionType();
            connectionType1.Name = "FourC18";
            connectionType1.Cost = 64.63;
            connectionType1.Labor = 98.16;
            connectionType1.AssignScopeProperties(catalogs);
            connectionType1.RatedCosts.Add(tecCost);
            connectionType1.RatedCosts.Add(elecCost);

            var connectionType2 = new TECConnectionType();
            connectionType2.Name = "ThreeC18";
            connectionType2.Cost = 73.16;
            connectionType2.Labor = 35.49;
            connectionType1.AssignScopeProperties(catalogs);
            connectionType1.RatedCosts.Add(tecCost);
            connectionType1.RatedCosts.Add(elecCost);

            catalogs.ConnectionTypes.Add(connectionType1);
            catalogs.ConnectionTypes.Add(connectionType2);

            //Manufacturers
            var manufacturer1 = new TECManufacturer();
            manufacturer1.Label = "Test";
            manufacturer1.Multiplier = .51;

            catalogs.Manufacturers.Add(manufacturer1);

            //Devices
            List<TECConnectionType> contypes4 = new List<TECConnectionType>();
            contypes4.Add(connectionType1);
            TECDevice device1 = new TECDevice(Guid.NewGuid(), contypes4, new List<TECProtocol>(), manufacturer1);
            device1.Name = "Device 1";
            device1.Description = "Description 1";
            device1.Price = 64.96;
            device1.Tags.Add(tag1);
            device1.AssignScopeProperties(catalogs);

            catalogs.Devices.Add(device1);

            //IO Modules
            TECIOModule testIOModule = new TECIOModule(manufacturer1);
            testIOModule.Name = "Test IO Module";
            testIOModule.Price = 13.46;
            testIOModule.Manufacturer = manufacturer1;
            catalogs.IOModules.Add(testIOModule);

            //Controller Types
            TECControllerType controllerType = new TECControllerType(manufacturer1);
            controllerType.Name = "Test Controller Type";
            controllerType.Price = 196.73;
            controllerType.Labor = 61.34;
            controllerType.AssignScopeProperties(catalogs);

            TECIO io = new TECIO(IOType.AI);
            io.Quantity = 100;
            controllerType.IO.Add(io);

            io = new TECIO(IOType.UI);
            io.Quantity = 100;
            controllerType.IO.Add(io);

            io = new TECIO(IOType.UO);
            io.Quantity = 100;
            controllerType.IO.Add(io);

            catalogs.ControllerTypes.Add(controllerType);

            //Panel Types
            TECPanelType panelType = new TECPanelType(manufacturer1);
            panelType.Price = 16.64;
            panelType.Labor = 91.46;
            panelType.Name = "Test Panel Type";
            panelType.AssignScopeProperties(catalogs);

            catalogs.PanelTypes.Add(panelType);

            TECPanelType otherPanelType = new TECPanelType(manufacturer1);
            otherPanelType.Price = 46.61;
            otherPanelType.Labor = 64.19;
            otherPanelType.Name = "Other Test Panel Type";
            otherPanelType.AssignScopeProperties(catalogs);

            catalogs.PanelTypes.Add(otherPanelType);

            //Valves
            TECDevice actuator = new TECDevice(new ObservableCollection<TECConnectionType>() { connectionType1 },
                new List<TECProtocol>(),
                manufacturer1);
            actuator.Name = "actuator";
            catalogs.Devices.Add(actuator);
            TECValve valve = new TECValve(manufacturer1, actuator);
            catalogs.Valves.Add(valve);

            //Protocols
            TECProtocol protocol = new TECProtocol(new List<TECConnectionType> { connectionType1 });
            protocol.Label = "BACnet IP";
            catalogs.Protocols.Add(protocol);

            controllerType.IO.Add(new TECIO(protocol));

            TECDevice netDevice = new TECDevice(Guid.NewGuid(), new List<TECConnectionType>(), new List<TECProtocol> { protocol }, manufacturer1);
            catalogs.Devices.Add(netDevice);

            return catalogs;
        }

        public static TECAssociatedCost TestCost(Random rand, CostType type)
        {
            return new TECAssociatedCost(type)
            {
                Name = string.Format("Test {0} Cost #{1}", type, rand.Next(100)),
                Cost = (rand.NextDouble() * 100),
                Labor = (rand.NextDouble() * 100)
            };
        }
        public static TECTag TestTag(Random rand)
        {
            return new TECTag()
            {
                Label = string.Format("Test Tag #{0}", rand.Next(100))
            };
        }
        public static TECElectricalMaterial TestElectricalMaterial(Random rand, string type, TECCatalogs catalogs)
        {
            TECElectricalMaterial mat = new TECElectricalMaterial()
            {
                Name = string.Format("Test {0} #{1}", type, rand.Next(100)),
                Cost = (rand.NextDouble() * 100),
                Labor = (rand.NextDouble() * 100)
            };
            mat.AssignScopeProperties(rand, catalogs);
            mat.RatedCosts.Add(catalogs.RandomCost(rand, CostType.TEC));
            mat.RatedCosts.Add(catalogs.RandomCost(rand, CostType.Electrical));
            return mat;
        } 
    }
}
