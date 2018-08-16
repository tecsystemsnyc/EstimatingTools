using EstimatingLibrary;
using EstimatingLibrary.Interfaces;
using EstimatingLibrary.Utilities;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TestLibrary.MockClasses
{
    public class IConnectableMock : IConnectable, ITECScope, ITypicalable
    {
        #region Mocked Properties
        private Guid _guid;
        private List<IProtocol> _availableProtocols;
        private IOCollection _hardwiredIO;

        public Guid Guid { get { return _guid; } }
        public List<IProtocol> AvailableProtocols { get { return _availableProtocols; } }
        public IOCollection HardwiredIO { get { return _hardwiredIO; } }
        #endregion
        
        #region Mocked Methods
        public bool CanSetParentConnection(IControllerConnection connection)
        {
            throw new NotImplementedException();
        }
        public IConnectable Copy(Dictionary<Guid, Guid> guidDictionary)
        {
            throw new NotImplementedException();
        }
        public IControllerConnection GetParentConnection()
        {
            throw new NotImplementedException();
        }
        public void SetParentConnection(IControllerConnection connection)
        {
            throw new NotImplementedException();
        }
        #endregion

        #region Setters
        public void SetGuid(Guid guid)
        {
            this._guid = guid;
        }
        public void SetAvailableProtocols(List<IProtocol> protocols)
        {
            this._availableProtocols = protocols;
        }
        public void SetHardwiredIO(IOCollection io)
        {
            this._hardwiredIO = io;
        }
        #endregion

        #region Interfaces
        bool ITypicalable.IsTypical => throw new NotImplementedException();

        ObservableCollection<TECAssociatedCost> ITECScope.AssociatedCosts { get => throw new NotImplementedException(); }

        CostBatch ITECScope.CostBatch => throw new NotImplementedException();

        string ITECScope.Description { get => throw new NotImplementedException(); set => throw new NotImplementedException(); }

        RelatableMap ITECScope.LinkedObjects => throw new NotImplementedException();

        string ITECScope.Name { get => throw new NotImplementedException(); set => throw new NotImplementedException(); }

        RelatableMap ITECScope.PropertyObjects => throw new NotImplementedException();

        ObservableCollection<TECTag> ITECScope.Tags { get => throw new NotImplementedException(); }

        Guid ITECObject.Guid => throw new NotImplementedException();

        event Action<CostBatch> ITECScope.CostChanged
        {
            add
            {
                throw new NotImplementedException();
            }

            remove
            {
                throw new NotImplementedException();
            }
        }

        event PropertyChangedEventHandler ITECObject.PropertyChanged
        {
            add
            {
                throw new NotImplementedException();
            }

            remove
            {
                throw new NotImplementedException();
            }
        }

        event Action<TECChangedEventArgs> ITECObject.TECChanged
        {
            add
            {
                throw new NotImplementedException();
            }

            remove
            {
                throw new NotImplementedException();
            }
        }
        
        ITECObject ITypicalable.CreateInstance(ObservableListDictionary<ITECObject> typicalDictionary)
        {
            throw new NotImplementedException();
        }

        void ITypicalable.AddChildForProperty(string property, ITECObject item)
        {
            throw new NotImplementedException();
        }

        bool ITypicalable.RemoveChildForProperty(string property, ITECObject item)
        {
            throw new NotImplementedException();
        }

        bool ITypicalable.ContainsChildForProperty(string property, ITECObject item)
        {
            throw new NotImplementedException();
        }

        void ITypicalable.MakeTypical()
        {
            throw new NotImplementedException();
        }
        #endregion
    }
}
