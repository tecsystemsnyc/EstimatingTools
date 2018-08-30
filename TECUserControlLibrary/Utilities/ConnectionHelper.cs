using EstimatingLibrary;
using EstimatingLibrary.Interfaces;
using EstimatingLibrary.Utilities;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TECUserControlLibrary.Utilities
{

    public static class ConnectionHelper
    {
        /// <summary>
        /// Returns true if all the items can be connected, in some way, to the provided controller
        /// </summary>
        /// <param name="items"></param>
        /// <param name="controller"></param>
        /// <returns></returns>
        public static bool CanConnectToController(IEnumerable<IConnectable> items, TECController controller)
        {

            var connectables = items
                .Where(x => x.GetParentConnection() == null || x.AvailableProtocols.Count == 0);
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

        /// <summary>
        /// Connects all items to the controller. Existing network connections will be added to where possible
        /// </summary>
        /// <param name="items"></param>
        /// <param name="controller"></param>
        /// <returns></returns>
        public static List<IControllerConnection> ConnectToController(IEnumerable<IConnectable> items, TECController controller, ConnectionProperties properties)
        {
            var connectables = items
                .Where(x => x.GetParentConnection() == null);

            var availableIO = controller.AvailableIO + ExistingNetworkIO(controller);
            List<IControllerConnection> connections = new List<IControllerConnection>();
            if (!CanConnectToController(items, controller)) return connections;
            foreach (var connectable in connectables)
            {
                var protocols = connectable.AvailableProtocols;
                if (protocols.Count == 1)
                {
                    var connection = controller.Connect(connectable, protocols.First());
                    if (connection == null) throw new Exception("Connection was Null");
                    connections.Add(connection);
                    connection.UpdateFromProperties(properties);
                }
                else
                {
                    var connected = false;
                    foreach (var protocol in connectable.AvailableProtocols
                        .Where(x => x is TECProtocol networkProtocol && availableIO.Contains(networkProtocol)))
                    {
                        connected = true;
                        var connection = controller.Connect(connectable, protocol);
                        if (connection == null) throw new Exception("Connection was Null");
                        connections.Add(connection);
                        connection.UpdateFromProperties(properties);
                        break;
                    }
                    if (!connected && connectable.AvailableProtocols.Any(x => x is TECHardwiredProtocol)
                            && availableIO.Contains(connectable.HardwiredIO))
                    {
                        var connection = controller.Connect(connectable, connectable.AvailableProtocols.First(x => x is TECHardwiredProtocol));
                        connection.UpdateFromProperties(properties);
                        connections.Add(connection);
                    }
                }
            }
            return connections.Distinct().ToList();

        }

        /// <summary>
        /// Returns all connectable objects which are direct descendants of the provided parent and match the provided predicate
        /// </summary>
        /// <param name="parent"></param>
        /// <param name="predicate"></param>
        /// <returns></returns>
        public static List<IConnectable> GetConnectables(ITECObject parent, Func<ITECObject, bool> predicate)
        {
            List<IConnectable> outList = new List<IConnectable>();
            if (parent is IConnectable connectable && predicate(parent))
            {
                outList.Add(connectable);
            }
            if (parent is IRelatable relatable)
            {
                relatable.GetDirectChildren().ForEach(x => outList.AddRange(GetConnectables(x, predicate)));
            }
            return outList;
        }

        /// <summary>
        /// Returns the io which make up existing network connections on a controller
        /// </summary>
        /// <param name="controller"></param>
        /// <returns></returns>
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
    public struct ConnectionProperties
    {
        public double ConduitLength { get; set; }
        public TECElectricalMaterial ConduitType { get; set; }
        public bool IsPlenum { get; set; }
        public double Length { get; set; }        

    }

    public static class IConnectionExtensions
    {
        public static void SetFromProperties(this IConnection connection, ConnectionProperties properties)
        {
            connection.Length = properties.Length;
            connection.ConduitLength = properties.ConduitLength;
            connection.ConduitType = properties.ConduitType;
            connection.IsPlenum = properties.IsPlenum;
        }

        public static void UpdateFromProperties(this IConnection connection, ConnectionProperties properties)
        {
            connection.Length += properties.Length;
            if (properties.ConduitType != null)
            {
                connection.ConduitLength += properties.ConduitLength;
                connection.ConduitType = properties.ConduitType;
            }
            
                
            connection.IsPlenum = properties.IsPlenum;
        }

    }
}
    
