using Microsoft.VisualStudio.TestTools.UnitTesting;
using EstimatingLibrary;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Models
{
    [TestClass()]
    public class TECPointTests
    {
        [TestMethod()]
        public void notifyPointChangedTest()
        {
            int pointNumber = 0;
            TECPoint point = new TECPoint();
            point.Type = IOType.AI;
            point.PointChanged += num =>
            {
                pointNumber += num;
            };

            point.Quantity = 3;

            //started at 1
            Assert.AreEqual(2, pointNumber);

        }

        [TestMethod()]
        public void notifyPointChangedTest1()
        {
            int pointNumber = 0;
            TECPoint point = new TECPoint();
            point.Type = IOType.AI;
            point.Quantity = 3;
            point.PointChanged += num =>
            {
                pointNumber += num;
            };

            point.Quantity = 0;
            
            Assert.AreEqual(-3, pointNumber);

        }

        [TestMethod()]
        public void setPointTypeTest()
        {
            TECPoint point = new TECPoint();
            point.Type = IOType.AI;

            Assert.ThrowsException<InvalidOperationException>(() => point.Type = IOType.Protocol);
        }
    }
}