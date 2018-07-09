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

        IConnectable Copy(Dictionary<Guid, Guid> guidDictionary);
        bool CanSetParentConnection(IControllerConnection connection);
        void SetParentConnection(IControllerConnection connection);
        IControllerConnection GetParentConnection();
    }

    public static class IConnectableExtensions
    {
        public static bool IsConnected(this IConnectable connectable)
        {
            return connectable.GetParentConnection() != null;
        }
    }
}
