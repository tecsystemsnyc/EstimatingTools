using System;
using System.Collections.Generic;

namespace EstimatingLibrary.Interfaces
{
    public interface INetworkConnectable : ITECObject
    {
        IOCollection AvailableProtocols { get; }
        TECNetworkConnection ParentConnection { get; set; }

        INetworkConnectable Copy(INetworkConnectable item, bool isTypical, Dictionary<Guid, Guid> guidDictionary);
    }
}
