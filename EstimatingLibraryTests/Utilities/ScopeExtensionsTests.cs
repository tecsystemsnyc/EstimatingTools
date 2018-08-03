using Microsoft.VisualStudio.TestTools.UnitTesting;
using EstimatingLibrary.Utilities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TestLibrary.MockClasses;
using EstimatingLibrary;

namespace Utilities
{
    [TestClass()]
    public class ScopeExtensionsTests
    {
        [TestMethod()]
        public void CopyPropertiesFromScopeTest()
        {
            TECScopeMock scope = new TECScopeMock();

            TECScopeMock toCopy = new TECScopeMock();
            toCopy.Name = "test";
            toCopy.Description = "test d";

            scope.CopyPropertiesFromScope(toCopy);

            Assert.AreEqual(toCopy.Name, scope.Name);
            Assert.AreEqual(toCopy.Description, scope.Description);
        }

        [TestMethod()]
        public void CopyChildrenFromScopeTest()
        {
            TECScopeMock scope = new TECScopeMock();

            TECScopeMock toCopy = new TECScopeMock();
            toCopy.AssociatedCosts.Add(new TECAssociatedCost(CostType.TEC));
            toCopy.Tags.Add(new TECTag());

            scope.CopyChildrenFromScope(toCopy);

            Assert.IsTrue(toCopy.AssociatedCosts.SequenceEqual(scope.AssociatedCosts));
            Assert.IsTrue(toCopy.Tags.SequenceEqual(scope.Tags));
        }
        
    }
}