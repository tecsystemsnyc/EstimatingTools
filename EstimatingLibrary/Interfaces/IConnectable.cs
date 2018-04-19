using System;
using System.Collections.Generic;

namespace EstimatingLibrary.Interfaces
{
    public interface IConnectable : ITECObject
    {
        IOCollection AvailableProtocols { get; }
        IOCollection HardwiredIO { get; }
        bool IsNetwork { get; }
        List<TECConnectionType> RequiredConnectionTypes { get; }

        IConnectable Copy(bool isTypical, Dictionary<Guid, Guid> guidDictionary);
        bool CanSetParentConnection(TECConnection connection);
        void SetParentConnection(TECConnection connection);
        TECConnection GetParentConnection();
    }
}
