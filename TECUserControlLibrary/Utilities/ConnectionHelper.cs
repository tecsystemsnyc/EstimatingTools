﻿using EstimatingLibrary;
using EstimatingLibrary.Interfaces;
using EstimatingLibrary.Utilities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TECUserControlLibrary.Utilities
{

    public static class ConnectionHelper
    {
        public static bool CanConnectToController(IEnumerable<IConnectable> items, TECController controller)
        {

            var connectables = items
                .Where(x => x.GetParentConnection() == null);
            if (connectables.Count() == 0)
            {
                return false;
            }

            var availableIO = controller.AvailableIO + ExistingNetworkIO(controller);
            foreach (var connectable in connectables.Where(x => x.AvailableProtocols.Count == 1))
            {
                var protocol = connectable.AvailableProtocols.First();

                if (protocol is TECProtocol netProtocol)
                {
                    if (!availableIO.Contains(netProtocol))
                    {
                        return false;
                    }
                }
                else
                {
                    if (!availableIO.Remove(connectable.HardwiredIO)) return false;
                }
            }

            foreach (var connectable in connectables.Where(x => x.AvailableProtocols.Count > 1))
            {
                var canConnect = false;
                foreach (var protocol in connectable.AvailableProtocols)
                {
                    if (protocol is TECProtocol networkProtocol)
                    {
                        if (availableIO.Contains(networkProtocol))
                        {
                            canConnect = true;
                            break;
                        }
                    }
                }
                if (canConnect == false)
                {
                    if (!(connectable.AvailableProtocols.Any(x => x is TECHardwiredProtocol) && availableIO.Remove(connectable.HardwiredIO))) return false;
                }
            }

            return true;
        }

        public static List<IControllerConnection> ConnectToController(IEnumerable<IConnectable> items, TECController controller)
        {
            var connectables = items
                .Where(x => x.GetParentConnection() == null);

            var availableIO = controller.AvailableIO + ExistingNetworkIO(controller);
            List<IControllerConnection> connections = new List<IControllerConnection>();

            foreach (var connectable in connectables)
            {
                var protocols = connectable.AvailableProtocols;
                if (protocols.Count == 1)
                {
                    connections.Add(controller.Connect(connectable, protocols.First(), true));
                }
                else
                {
                    var connected = false;
                    foreach (var protocol in connectable.AvailableProtocols
                        .Where(x => x is TECProtocol networkProtocol && availableIO.Contains(networkProtocol)))
                    {
                        connected = true;
                        connections.Add(controller.Connect(connectable, protocol, true));
                        break;
                    }
                    if (!connected && connectable.AvailableProtocols.Any(x => x is TECHardwiredProtocol)
                            && availableIO.Contains(connectable.HardwiredIO))
                    {
                        connections.Add(controller.Connect(connectable, connectable.AvailableProtocols.First(x => x is TECHardwiredProtocol), true));
                    }
                }
            }
            return connections;

        }

        public static List<IConnectable> GetConnectables(ITECObject parent, Func<ITECObject, bool> predicate)
        {
            List<IConnectable> outList = new List<IConnectable>();
            if (parent is IConnectable connectable && predicate(parent))
            {
                outList.Add(connectable);
            }
            if (parent is IRelatable relatable)
            {
                relatable.GetDirectChildren().Where(predicate).ForEach(x => outList.AddRange(GetConnectables(x, predicate)));
            }
            return outList;
        }

        public static IOCollection ExistingNetworkIO(TECController controller)
        {
            IOCollection existingNetwork = new IOCollection();
            foreach (TECNetworkConnection connection in controller.ChildrenConnections.Where(x => x is TECNetworkConnection))
            {
                existingNetwork.Add(connection.NetworkProtocol);
            }
            return existingNetwork;
        }
    }
}
