using EstimatingLibrary;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TestLibrary.MockClasses
{
    public class TECScopeMock : TECScope
    {
        public TECScopeMock() : base(Guid.NewGuid()) { }
    }
}
