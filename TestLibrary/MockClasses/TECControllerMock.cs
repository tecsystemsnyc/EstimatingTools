using EstimatingLibrary;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TestLibrary.MockClasses
{
    public class TECControllerMock : TECController
    {
        private IOCollection _io;

        public override IOCollection IO
        {
            get { return _io; }
        }

        public TECControllerMock(Guid guid, bool isTypical) : base(guid, isTypical) { }
        public TECControllerMock(bool isTypical) : base(isTypical) { }
        public TECControllerMock(TECController controllerSource, bool isTypical, Dictionary<Guid, Guid> guidDictionary = null) : base(controllerSource, isTypical, guidDictionary) { }

        public override TECController CopyController(bool isTypical, Dictionary<Guid, Guid> guidDictionary = null)
        {
            return new TECControllerMock(this, isTypical, guidDictionary);
        }

        /// <summary>
        /// Used to set the abstract IO property.
        /// </summary>
        /// <param name="io"></param>
        public void SetIO(IOCollection io)
        {
            this._io = io;
        }
    }
}
