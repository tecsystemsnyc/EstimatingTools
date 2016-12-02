﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using EstimatingLibrary;
using EstimatingUtilitiesLibrary;
using System.Collections.ObjectModel;
using System.IO;

namespace Tests
{
    public static class TestHelper
    {
        static public string StaticTestBidPath = Directory.GetParent(Environment.CurrentDirectory).Parent.FullName + @"\Resources\StaticTestBid.bdb";

        public static TECBid CreateTestBid()
        {
            TECBid bid = new TECBid();

            //Bid Info
            bid.Name = "Bid Name";
            bid.BidNumber = "1701-117";
            bid.DueDate = DateTime.Now;
            bid.Salesperson = "Mrs. Test";
            bid.Estimator = "Mr. Test";

            //Bid Labor
            bid.Labor.PMCoef = 0.1;
            bid.Labor.ENGCoef = 0.2;
            bid.Labor.CommCoef = 0.3;
            bid.Labor.SoftCoef = 0.4;
            bid.Labor.GraphCoef = 0.5;
            bid.Labor.ElectricalRate = 0.6;

            //Locations
            var cellar = new TECLocation("Cellar");
            var location1 = new TECLocation("1st Floor");
            var location2 = new TECLocation("2nd Floor");
            var location3 = new TECLocation("3rd Floor");

            var allLocations = new ObservableCollection<TECLocation>();

            allLocations.Add(cellar);
            allLocations.Add(location1);
            allLocations.Add(location2);
            allLocations.Add(location3);

            //Points
            var point1 = new TECPoint(PointTypes.Serial, "Point 1", "Description 1");
            point1.Quantity = 321;

            //Devices
            var device1 = new TECDevice("Device 1", "Description 1", 987.6, "Test Wire", new TECManufacturer());
            device1.Quantity = 987;
            var allDevices = new ObservableCollection<TECDevice>();
            allDevices.Add(device1);

            //SubScope
            var subScope1 = new TECSubScope("SubScope 1", "Description 1", allDevices, new ObservableCollection<TECPoint>());
            subScope1.Quantity = 654;
            subScope1.Location = location3;
            subScope1.Points.Add(point1);

            var subScope2 = new TECSubScope("Empty SubScope", "Description 2", new ObservableCollection<TECDevice>(), new ObservableCollection<TECPoint>());

            //Equipment
            var equipment1 = new TECEquipment("Equipment 1", "Description 1", 123.4, new ObservableCollection<TECSubScope>());
            equipment1.Quantity = 1234;
            equipment1.Location = location1;
            equipment1.SubScope.Add(subScope1);

            var equipment2 = new TECEquipment("Equipment 2", "Description 2", 0, new ObservableCollection<TECSubScope>());
            equipment2.SubScope.Add(subScope2);

            //Systems
            var system1 = new TECSystem("System 1", "Locations all the way", 234.5, new ObservableCollection<TECEquipment>());
            system1.Quantity = 2345;
            system1.Location = location1;
            system1.Equipment.Add(equipment1);

            var system2 = new TECSystem("System 2", "Description 2", 234.52, new ObservableCollection<TECEquipment>());
            system2.Quantity = 23452;
            system2.Location = location2;

            var system3 = new TECSystem("System 3", "No Location", 349, new ObservableCollection<TECEquipment>());
            system3.Equipment.Add(equipment2);

            var allSystems = new ObservableCollection<TECSystem>();
            allSystems.Add(system1);
            allSystems.Add(system2);
            allSystems.Add(system3);

            //Pages
            var pages1 = new TECPage("Testpath", 2);
            var allPages = new ObservableCollection<TECPage>();

            //Drawings
            var drawing1 = new TECDrawing("Test", "Desc", Guid.NewGuid(), allPages);
            var allDrawings = new ObservableCollection<TECDrawing>();
            allDrawings.Add(drawing1);

            //Manufacturers
            var manufacturer1 = new TECManufacturer("Test", "Desc", 0.8);
            var allManufacturers = new ObservableCollection<TECManufacturer>();
            allManufacturers.Add(manufacturer1);

            //Tags
            var tag1 = new TECTag("Test");
            var allTags = new ObservableCollection<TECTag>();
            allTags.Add(tag1);

            //Devices Catalog
            var deviceC1 = new TECDevice("Device C1", "Description C1", 987.6, "Test Wire", new TECManufacturer());
            var deviceCatalog = new ObservableCollection<TECDevice>();
            deviceCatalog.Add(deviceC1);
            deviceCatalog.Add(device1);

            //Notes
            var note1 = new TECNote("Note 1");

            var allNotes = new ObservableCollection<TECNote>();
            allNotes.Add(note1);

            //Exclusions
            var exclusion1 = new TECExclusion("Exlusions 1");

            var allExclusions = new ObservableCollection<TECExclusion>();
            allExclusions.Add(exclusion1);

            //Bid
            bid.Systems = allSystems;
            bid.DeviceCatalog = deviceCatalog;
            bid.Drawings = allDrawings;
            bid.ManufacturerCatalog = allManufacturers;
            bid.Tags = allTags;
            bid.Locations = allLocations;
            bid.Notes = allNotes;
            bid.Exclusions = allExclusions;

            return bid;
        }

        public static TECBid LoadTestBid(string path)
        {
            return EstimatingLibraryDatabase.LoadDBToBid(path, new TECTemplates());
        }
    }
}
