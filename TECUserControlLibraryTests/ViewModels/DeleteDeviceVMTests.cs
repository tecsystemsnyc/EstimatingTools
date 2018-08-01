using EstimatingLibrary;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using TECUserControlLibrary.Interfaces;
using TECUserControlLibrary.ViewModels;

namespace ViewModels
{
    [TestClass]
    public class DeleteDeviceVMTests
    {
        [TestMethod]
        public void DeleteDeviceUserInputYes()
        {
            //Arrange
            TECTemplates templatesManager = new TECTemplates();
            TECScopeTemplates templates = templatesManager.Templates;

            TECManufacturer man = new TECManufacturer();
            templatesManager.Catalogs.Manufacturers.Add(man);

            TECDevice dev = new TECDevice(new List<TECConnectionType>(), new List<TECProtocol>(), man);
            templatesManager.Catalogs.Devices.Add(dev);

            TECSystem sys = new TECSystem();
            templates.SystemTemplates.Add(sys);
            TECEquipment sysEquip = new TECEquipment();
            sys.Equipment.Add(sysEquip);
            TECSubScope sysSS = new TECSubScope();
            sysEquip.SubScope.Add(sysSS);
            sysSS.Devices.Add(dev);

            TECEquipment equip = new TECEquipment();
            templates.EquipmentTemplates.Add(equip);
            TECSubScope equipSS = new TECSubScope();
            equip.SubScope.Add(equipSS);
            equipSS.Devices.Add(dev);

            TECSubScope ss = new TECSubScope();
            templates.SubScopeTemplates.Add(ss);
            ss.Devices.Add(dev);
            
            //Act
            Mock<IUserConfirmable> mockMessageBox = new Mock<IUserConfirmable>();
            mockMessageBox
                .Setup(x => x.Show(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<MessageBoxButton>(), It.IsAny<MessageBoxImage>()))
                .Returns(MessageBoxResult.Yes);

            DeleteEndDeviceVM vm = new DeleteEndDeviceVM(dev, templatesManager);
            vm.messageBox = mockMessageBox.Object;
            vm.DeleteCommand.Execute(null);

            //Assert
            Assert.IsFalse(templatesManager.Catalogs.Devices.Contains(dev), "Device not removed from device templates properly.");
            Assert.IsFalse(sysSS.Devices.Contains(dev), "Device not removed from system template properly.");
            Assert.IsFalse(equipSS.Devices.Contains(dev), "Device not removed from equipment template properly.");
            Assert.IsFalse(ss.Devices.Contains(dev), "Device not removed from subscope template properly.");
        }

        [TestMethod]
        public void DeleteDeviceUserInputNo()
        {
            //Arrange
            TECTemplates templatesManager = new TECTemplates();
            TECScopeTemplates templates = templatesManager.Templates;

            TECManufacturer man = new TECManufacturer();
            templatesManager.Catalogs.Manufacturers.Add(man);

            TECDevice dev = new TECDevice(new List<TECConnectionType>(), new List<TECProtocol>(), man);
            templatesManager.Catalogs.Devices.Add(dev);

            TECSystem sys = new TECSystem();
            templates.SystemTemplates.Add(sys);
            TECEquipment sysEquip = new TECEquipment();
            sys.Equipment.Add(sysEquip);
            TECSubScope sysSS = new TECSubScope();
            sysEquip.SubScope.Add(sysSS);
            sysSS.Devices.Add(dev);

            TECEquipment equip = new TECEquipment();
            templates.EquipmentTemplates.Add(equip);
            TECSubScope equipSS = new TECSubScope();
            equip.SubScope.Add(equipSS);
            equipSS.Devices.Add(dev);

            TECSubScope ss = new TECSubScope();
            templates.SubScopeTemplates.Add(ss);
            ss.Devices.Add(dev);
            
            //Act
            Mock<IUserConfirmable> mockMessageBox = new Mock<IUserConfirmable>();
            mockMessageBox
                .Setup(x => x.Show(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<MessageBoxButton>(), It.IsAny<MessageBoxImage>()))
                .Returns(MessageBoxResult.No);

            DeleteEndDeviceVM vm = new DeleteEndDeviceVM(dev, templatesManager);
            vm.messageBox = mockMessageBox.Object;
            vm.DeleteCommand.Execute(null);

            //Assert
            Assert.IsTrue(templatesManager.Catalogs.Devices.Contains(dev), "Device not removed from device templates properly.");
            Assert.IsTrue(sysSS.Devices.Contains(dev), "Device not removed from system template properly.");
            Assert.IsTrue(equipSS.Devices.Contains(dev), "Device not removed from equipment template properly.");
            Assert.IsTrue(ss.Devices.Contains(dev), "Device not removed from subscope template properly.");
        }
    }
}
