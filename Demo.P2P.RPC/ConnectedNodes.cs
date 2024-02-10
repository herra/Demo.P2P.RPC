using System.Collections.Concurrent;

namespace Demo.P2P.RPC
{
    public class ConnectedNodes
    {
        // Key format IP:PORT
        public static ConcurrentDictionary<string, NodeHandler> Nodes = new ConcurrentDictionary<string, NodeHandler>();

        public static ConcurrentDictionary<string, NodeHandler> ClientNodes = new ConcurrentDictionary<string, NodeHandler>();

    }
}
