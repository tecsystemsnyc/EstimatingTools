using Microsoft.VisualStudio.TestTools.UnitTesting;
using EstimatingLibrary.Utilities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using EstimatingLibrary.Interfaces;
using EstimatingLibrary;

namespace Utilities
{
    [TestClass()]
    public class ObservableListDictionaryTests
    {

        [TestMethod()]
        public void AddItemTest()
        {
            ObservableListDictionary<ITECScope> dict = new ObservableListDictionary<ITECScope>();

            TECSubScope keySS = new TECSubScope();
            TECSubScope valueSS = new TECSubScope();

            dict.AddItem(keySS, valueSS);

            Assert.IsTrue(dict.GetFullDictionary().ContainsKey(keySS));
            Assert.IsTrue(dict.GetFullDictionary()[keySS].Contains(valueSS));
        }

        [TestMethod()]
        public void RemoveItemTest()
        {
            ObservableListDictionary<ITECScope> dict = new ObservableListDictionary<ITECScope>();

            TECSubScope keySS = new TECSubScope();
            TECSubScope valueSS1 = new TECSubScope();
            TECSubScope valueSS2 = new TECSubScope();

            dict.AddItem(keySS, valueSS1);
            dict.AddItem(keySS, valueSS2);

            dict.RemoveItem(keySS, valueSS1);

            Assert.IsTrue(dict.GetFullDictionary().ContainsKey(keySS));
            Assert.IsFalse(dict.GetFullDictionary()[keySS].Contains(valueSS1));
            Assert.IsTrue(dict.GetFullDictionary()[keySS].Contains(valueSS2));

        }

        [TestMethod()]
        public void RemoveKeyTest()
        {
            ObservableListDictionary<ITECScope> dict = new ObservableListDictionary<ITECScope>();

            TECSubScope keySS = new TECSubScope();
            TECSubScope valueSS1 = new TECSubScope();
            TECSubScope valueSS2 = new TECSubScope();

            dict.AddItem(keySS, valueSS1);
            dict.AddItem(keySS, valueSS2);

            dict.RemoveKey(keySS);

            Assert.IsFalse(dict.GetFullDictionary().ContainsKey(keySS));
        }

        [TestMethod()]
        public void GetTypicalTest()
        {
            ObservableListDictionary<ITECScope> dict = new ObservableListDictionary<ITECScope>();

            TECSubScope keySS = new TECSubScope();
            TECSubScope valueSS1 = new TECSubScope();
            TECSubScope valueSS2 = new TECSubScope();

            dict.AddItem(keySS, valueSS1);
            dict.AddItem(keySS, valueSS2);

            var typ = dict.GetTypical(valueSS1);

            Assert.AreEqual(keySS, typ);
        }

        [TestMethod()]
        public void GetInstancesTest()
        {
            ObservableListDictionary<ITECScope> dict = new ObservableListDictionary<ITECScope>();

            TECSubScope keySS = new TECSubScope();
            TECSubScope valueSS1 = new TECSubScope();
            TECSubScope valueSS2 = new TECSubScope();

            dict.AddItem(keySS, valueSS1);
            dict.AddItem(keySS, valueSS2);

            var instances = dict.GetInstances(keySS);

            Assert.IsTrue(instances.Contains(valueSS1));
            Assert.IsTrue(instances.Contains(valueSS2));

        }

        [TestMethod()]
        public void ContainsKeyTest()
        {
            ObservableListDictionary<ITECScope> dict = new ObservableListDictionary<ITECScope>();

            TECSubScope keySS = new TECSubScope();
            TECSubScope valueSS1 = new TECSubScope();
            TECSubScope valueSS2 = new TECSubScope();

            dict.AddItem(keySS, valueSS1);
            dict.AddItem(keySS, valueSS2);

            Assert.IsTrue(dict.ContainsKey(keySS));
        }

        [TestMethod()]
        public void ContainsValueTest()
        {
            ObservableListDictionary<ITECScope> dict = new ObservableListDictionary<ITECScope>();

            TECSubScope keySS = new TECSubScope();
            TECSubScope valueSS1 = new TECSubScope();
            TECSubScope valueSS2 = new TECSubScope();

            dict.AddItem(keySS, valueSS1);
            dict.AddItem(keySS, valueSS2);

            Assert.IsTrue(dict.ContainsValue(valueSS1));
            Assert.IsTrue(dict.ContainsValue(valueSS2));
        }

        [TestMethod()]
        public void GetFullDictionaryTest()
        {
            ObservableListDictionary<ITECScope> dict = new ObservableListDictionary<ITECScope>();

            TECSubScope keySS = new TECSubScope();
            TECSubScope valueSS1 = new TECSubScope();
            TECSubScope valueSS2 = new TECSubScope();

            dict.AddItem(keySS, valueSS1);
            dict.AddItem(keySS, valueSS2);

            var underlying = dict.GetFullDictionary();
            Assert.AreEqual(1, underlying.Keys.Count);
            Assert.AreEqual(2, underlying[keySS].Count);
        }

        [TestMethod()]
        public void RemoveValuesForKeysTest()
        {
            ObservableListDictionary<ITECScope> dict = new ObservableListDictionary<ITECScope>();

            TECSubScope keySS = new TECSubScope();
            TECSubScope valueSS1 = new TECSubScope();
            TECSubScope valueSS2 = new TECSubScope();

            dict.AddItem(keySS, valueSS1);
            dict.AddItem(keySS, valueSS2);

            dict.RemoveValuesForKeys(new List<ITECScope>() { valueSS1 }, new List<ITECScope>() { keySS });

            Assert.IsFalse(dict.GetFullDictionary()[keySS].Contains(valueSS1));

        }
    }
}