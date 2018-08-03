using Microsoft.VisualStudio.TestTools.UnitTesting;
using EstimatingLibrary.Utilities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Collections.ObjectModel;

namespace Utilities
{
    [TestClass()]
    public class CommonUtilitiesTests
    {
        [TestMethod()]
        public void ObservablyClearTest()
        {
            ObservableCollection<string> collection = new ObservableCollection<string>();
            collection.Add("test1");
            collection.Add("test2");

            int eventNum = 0;
            collection.CollectionChanged += (sender, args) =>
            {
                eventNum += 1;
            };

            collection.ObservablyClear();
            Assert.AreEqual(2, eventNum);
        }

        [TestMethod()]
        public void AddRangeTest()
        {
            ObservableCollection<string> collection = new ObservableCollection<string>();
            List<string> toAdd = new List<string>()
            {
                "test1",
                "test2"
            };

            int eventNum = 0;
            collection.CollectionChanged += (sender, args) =>
            {
                eventNum += 1;
            };

            collection.AddRange(toAdd);
            Assert.AreEqual(2, eventNum);
        }

        [TestMethod()]
        public void MatchesTest()
        {
            Assert.Fail();
        }

        [TestMethod()]
        public void FillScopeCollectionTest()
        {
            Assert.Fail();
        }

        [TestMethod()]
        public void UnionizeScopeCollectionTest()
        {
            Assert.Fail();
        }
    }
}