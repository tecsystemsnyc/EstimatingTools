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
        public Guid Guid => throw new NotImplementedException();

        public List<IProtocol> AvailableProtocols => throw new NotImplementedException();

        public IOCollection HardwiredIO => throw new NotImplementedException();

        #region Consequential Interfaces
        bool ITypicalable.IsTypical => throw new NotImplementedException();

        ObservableCollection<TECAssociatedCost> ITECScope.AssociatedCosts { get => throw new NotImplementedException(); set => throw new NotImplementedException(); }

        CostBatch ITECScope.CostBatch => throw new NotImplementedException();

        string ITECScope.Description { get => throw new NotImplementedException(); set => throw new NotImplementedException(); }

        SaveableMap ITECScope.LinkedObjects => throw new NotImplementedException();

        string ITECScope.Name { get => throw new NotImplementedException(); set => throw new NotImplementedException(); }

        SaveableMap ITECScope.PropertyObjects => throw new NotImplementedException();

        ObservableCollection<TECTag> ITECScope.Tags { get => throw new NotImplementedException(); set => throw new NotImplementedException(); }

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

        public bool CanSetParentConnection(IControllerConnection connection)
        {
            throw new NotImplementedException();
        }

        public IConnectable Copy(bool isTypical, Dictionary<Guid, Guid> guidDictionary)
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
    }
}
