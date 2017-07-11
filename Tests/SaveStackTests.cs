﻿using System;
using System.Text;
using System.Collections.Generic;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using EstimatingLibrary;
using EstimatingUtilitiesLibrary;

namespace Tests
{
    [TestClass]
    public class SaveStackTests
    {
        [TestMethod]
        public void Bid_AddSystem()
        {
            //Arrange
            TECBid bid = new TECBid();
            ChangeStack stack = new ChangeStack(bid);
            
            //Act
            bid.Systems.Add(new TECSystem());

            int expectedCount = 1;

            //Assert
            Assert.AreEqual(expectedCount, stack.SaveStack.Count);
        }

        [TestMethod]
        public void Bid_AddSystemInstance()
        {
            //Arrange
            TECBid bid = new TECBid();
            ChangeStack stack = new ChangeStack(bid);
            TECSystem system = new TECSystem();
            bid.Systems.Add(new TECSystem());

            int initialCount = stack.SaveStack.Count;

            //Act
            system.AddInstance(bid);

            int expectedCount = 1;
            int actualCount = stack.SaveStack.Count - initialCount;

            //Assert
            Assert.AreEqual(expectedCount, actualCount);
        }
    }
}
