using System;
using System.Collections.Generic;

namespace EstimatingLibrary.Interfaces
{
    public interface IConnectable : ITECScope
    {
        Guid Guid { get; }
        
        /// <summary>
        /// Protocols that can be used to connect to this connectable.
        /// </summary>
        List<IProtocol> AvailableProtocols { get; }
        /// <summary>
        /// List of BMS points 
        /// </summary>
        IOCollection HardwiredIO { get; }

        IConnectable Copy(bool isTypical, Dictionary<Guid, Guid> guidDictionary);
        bool CanSetParentConnection(TECConnection connection);
        void SetParentConnection(TECConnection connection);
        TECConnection GetParentConnection();
    }
}
