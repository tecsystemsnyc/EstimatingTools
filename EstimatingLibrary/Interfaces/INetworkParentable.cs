using System.Collections.Generic;

namespace EstimatingLibrary.Interfaces
{
    public interface INetworkParentable : INetworkConnectable
    {
        bool IsServer { get; }
        bool IsTypical { get; }
        string Name { get; }
        
        IEnumerable<TECNetworkConnection> ChildNetworkConnections { get; }

        bool CanAddNetworkConnection(TECProtocol protocol);
        TECNetworkConnection AddNetworkConnection(TECProtocol protocol);
        void RemoveNetworkConnection(TECNetworkConnection connection);
        
    }
}
