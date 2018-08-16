using Microsoft.VisualStudio.TestTools.UnitTesting;
using EstimatingLibrary.Utilities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Collections.ObjectModel;
using EstimatingLibrary;
using EstimatingLibrary.Interfaces;

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
        public void FillScopeCollectionTest()
        {
            TECSubScope scope1 = new TECSubScope();
            TECSubScope scope2 = new TECSubScope();
            TECSubScope scope3 = new TECSubScope();

            List<ITECScope> first = new List<ITECScope>()
            {
                scope1,
                scope2
            };

            List<ITECScope> second = new List<ITECScope>()
            {
                scope2,
                scope3
            };

            CommonUtilities.FillScopeCollection(first, second);

            Assert.IsTrue(first.Contains(scope3));
            Assert.AreEqual(1, first.Where(x => x == scope2).Count());

        }

        [TestMethod()]
        public void UnionizeScopeCollectionTest()
        {
            TECSubScope scope1 = new TECSubScope();
            TECSubScope scope2 = new TECSubScope();
            TECSubScope scopeOverwrite = new TECSubScope(scope2.Guid);
            TECSubScope scope3 = new TECSubScope();

            List<ITECScope> first = new List<ITECScope>()
            {
                scope1,
                scopeOverwrite
            };

            List<ITECScope> second = new List<ITECScope>()
            {
                scope2,
                scope3
            };

            CommonUtilities.UnionizeScopeCollection(first, second);

            Assert.IsTrue(first.Contains(scope3));
            Assert.IsTrue(first.Contains(scope2));
            Assert.IsFalse(first.Contains(scopeOverwrite));
        }
    }
}