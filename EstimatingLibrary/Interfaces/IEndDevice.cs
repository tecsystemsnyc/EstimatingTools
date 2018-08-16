using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;

namespace EstimatingLibrary.Interfaces
{
    public interface IEndDevice : ITECObject, ICatalog
    {
        Guid Guid { get; }

        List<IProtocol> ConnectionMethods { get; }
        String Name { get; }
        String Description { get; }

        ObservableCollection<TECConnectionType> HardwiredConnectionTypes { get; }
        ObservableCollection<TECProtocol> PossibleProtocols { get; }
    }
}
