using Demo.P2P.RPC.Services;
using Demo.P2P.RPC.Services.Contract;
using StreamJsonRpc;
using System;
using System.Net.WebSockets;
using System.Threading;
using System.Xml.Serialization;

namespace Demo.P2P.RPC
{
    public class NodeHandler : IDisposable, IAuctionService
    {
        private readonly string _nodeIdentifier;
        private readonly WebSocket _webSocket;
        private readonly IJsonRpcMessageHandler _jsonRpcMessageHandler;
        private readonly IAuctionService _auctionService;

        public NodeHandler(string nodeIdentifier, WebSocket webSocket, IJsonRpcMessageHandler jsonRpcMessageHandler)
        {
            _nodeIdentifier = nodeIdentifier;
            _webSocket = webSocket;
            _jsonRpcMessageHandler = jsonRpcMessageHandler;
            _auctionService = JsonRpc.Attach<IAuctionService>(jsonRpcMessageHandler);

            ConnectedNodes.Nodes.TryAdd($"{nodeIdentifier}", this);
        }

        public Task CloseAuction(AuctionItem auctionItem, AuctionBid winningBid)
        {
            throw new NotImplementedException();
        }

        public Task<AuctionBid> PlaceBid(string itemIdentifier, decimal amount)
        {
            throw new NotImplementedException();
        }

        public Task StartAuction(AuctionItem auctionItem)
        {
            throw new NotImplementedException();
        }

        public void Dispose()
        {
            if (_webSocket != null)
            {
                NodeHandler n;
                while (!ConnectedNodes.Nodes.TryRemove(_nodeIdentifier, out n)) ;
                _webSocket.CloseAsync(WebSocketCloseStatus.NormalClosure, "Client closing", CancellationToken.None);
                _webSocket.Dispose();
                // Expecting issues here
            }
        }
    }
}
