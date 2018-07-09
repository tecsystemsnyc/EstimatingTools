using EstimatingLibrary;
using EstimatingLibrary.Interfaces;
using EstimatingLibrary.Utilities;
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

        public TECControllerMock(Guid guid) : base(guid) { }
        public TECControllerMock() : base() { }
        public TECControllerMock(TECController controllerSource, Dictionary<Guid, Guid> guidDictionary = null) : base(controllerSource, guidDictionary) { }

        public override TECController CopyController(Dictionary<Guid, Guid> guidDictionary = null)
        {
            return new TECControllerMock(this, guidDictionary);
        }

        /// <summary>
        /// Used to set the abstract IO property.
        /// </summary>
        /// <param name="io"></param>
        public void SetIO(IOCollection io)
        {
            this._io = io;
        }
        
        #region ITypicalable
        protected override ITECObject createInstance(ObservableListDictionary<ITECObject> typicalDictionary)
        {
            throw new NotImplementedException();
        }

        protected override void addChildForProperty(string property, ITECObject item)
        {
            throw new NotImplementedException();
        }

        protected override bool removeChildForProperty(string property, ITECObject item)
        {
            throw new NotImplementedException();
        }

        protected override bool containsChildForProperty(string property, ITECObject item)
        {
            throw new NotImplementedException();
        }

        protected override void makeTypical()
        {
            this.IsTypical = true;
        }
        #endregion
    }
}
