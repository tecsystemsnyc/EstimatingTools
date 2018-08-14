using Microsoft.VisualStudio.TestTools.UnitTesting;
using EstimatingLibrary.Utilities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using EstimatingLibrary.Interfaces;
using EstimatingLibrary;
using System.ComponentModel;
using System.Collections.ObjectModel;

namespace Utilities
{
    [TestClass()]
    public class CollectionChangedHandlersTests
    {
        [TestMethod()]
        public void CollectionChangedHandlerTest()
        {
            bool notifiedTEC = false;
            CostBatch notifiedCost = new CostBatch();
            int notifiedPoint = 0;
            bool notifiedAdd = false;
            bool notifiedRemoved = false;

            NotifyMockParent parent = new NotifyMockParent();
            parent.IsTypical = false;
            parent.Collection.CollectionChanged += Collection_CollectionChanged;


            void Collection_CollectionChanged(object sender, System.Collections.Specialized.NotifyCollectionChangedEventArgs e)
            {
                CollectionChangedHandlers.CollectionChangedHandler(sender, e, "Collection", parent, 
                    (change, prop, par, obj, item) => notifiedTEC = true, 
                    cost => notifiedCost += cost,
                    points => notifiedPoint += points,
                    obj => notifiedAdd = true, 
                    obj => notifiedRemoved = true);
            }


            parent.Collection.Add(new NotifyMock());

            Assert.IsTrue(notifiedTEC);
            Assert.IsTrue(new CostBatch(10, 11, CostType.Electrical).CostsEqual(notifiedCost));
            Assert.AreEqual(12, notifiedPoint);
            Assert.IsTrue(notifiedAdd);
            
            parent.Collection.Remove(parent.Collection.First());
            
            Assert.IsTrue(notifiedRemoved);
            Assert.IsTrue(new CostBatch().CostsEqual(notifiedCost));
            Assert.AreEqual(0, notifiedPoint);

        }

        [TestMethod()]
        public void CollectionChangedHandlerTest_Typical()
        {
            bool notifiedTEC = false;
            CostBatch notifiedCost = null;
            int notifiedPoint = 0;
            bool notifiedAdd = false;
            bool notifiedRemoved = false;

            NotifyMockParent parent = new NotifyMockParent();
            parent.IsTypical = true;
            parent.Collection.CollectionChanged += Collection_CollectionChanged;


            void Collection_CollectionChanged(object sender, System.Collections.Specialized.NotifyCollectionChangedEventArgs e)
            {
                CollectionChangedHandlers.CollectionChangedHandler(sender, e, "Collection", parent,
                    (change, prop, par, obj, item) => notifiedTEC = true,
                    cost => notifiedCost = cost,
                    points => notifiedPoint = points,
                    obj => notifiedAdd = true,
                    obj => notifiedRemoved = true);
            }


            parent.Collection.Add(new NotifyMock());

            Assert.IsTrue(notifiedTEC);
            Assert.IsTrue(new CostBatch().CostsEqual(notifiedCost));
            Assert.AreEqual(0, notifiedPoint);
            Assert.IsTrue(notifiedAdd);

            parent.Collection.Remove(parent.Collection.First());

            Assert.IsTrue(notifiedRemoved);

        }



        private class NotifyMockParent : ITypicalable, ITECObject
        {
            public ObservableCollection<NotifyMock> Collection = new ObservableCollection<NotifyMock>();

            public bool IsTypical { get; set; } = true;

            public Guid Guid => throw new NotImplementedException();

            public event PropertyChangedEventHandler PropertyChanged;
            public event Action<TECChangedEventArgs> TECChanged;

            public void AddChildForProperty(string property, ITECObject item)
            {
                throw new NotImplementedException();
            }

            public bool ContainsChildForProperty(string property, ITECObject item)
            {
                throw new NotImplementedException();
            }

            public ITECObject CreateInstance(ObservableListDictionary<ITECObject> typicalDictionary = null)
            {
                throw new NotImplementedException();
            }

            public void MakeTypical()
            {
                throw new NotImplementedException();
            }

            public bool RemoveChildForProperty(string property, ITECObject item)
            {
                throw new NotImplementedException();
            }
        }

        private class NotifyMock : INotifyPointChanged, INotifyCostChanged, INotifyTECChanged, ITypicalable, ITECObject
        {
            public bool IsTypical { get; private set; } = false;

            public CostBatch CostBatch => new CostBatch(10,11,CostType.Electrical);

            public Guid Guid { get; } = Guid.NewGuid();

            public int PointNumber { get; private set; } = 12;

            public event Action<TECChangedEventArgs> TECChanged;
            public event Action<CostBatch> CostChanged;
            public event PropertyChangedEventHandler PropertyChanged;
            public event Action<int> PointChanged;

            public void AddChildForProperty(string property, ITECObject item)
            {
                throw new NotImplementedException();
            }

            public bool ContainsChildForProperty(string property, ITECObject item)
            {
                throw new NotImplementedException();
            }

            public ITECObject CreateInstance(ObservableListDictionary<ITECObject> typicalDictionary = null)
            {
                throw new NotImplementedException();
            }

            public void MakeTypical()
            {
                this.IsTypical = true;
            }

            public bool RemoveChildForProperty(string property, ITECObject item)
            {
                throw new NotImplementedException();
            }
        }
    }
}