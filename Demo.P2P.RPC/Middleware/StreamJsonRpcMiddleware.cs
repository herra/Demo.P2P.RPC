using System;
using System.Threading.Tasks;
using Demo.P2P.RPC.Services;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;
using StreamJsonRpc;

namespace Demo.P2P.RPC.Middleware
{
    internal class StreamJsonRpcMiddleware
    {
        public StreamJsonRpcMiddleware(RequestDelegate next)
        {
        }

        public async Task Invoke(HttpContext context)
        {
            if (context.WebSockets.IsWebSocketRequest)
            {
                var webSocket = await context.WebSockets.AcceptWebSocketAsync();

                IJsonRpcMessageHandler jsonRpcMessageHandler = new WebSocketMessageHandler(webSocket);
                var nodeIdentifier = $"{context.Request.HttpContext.Connection.RemoteIpAddress}:{context.Request.HttpContext.Connection.RemotePort}";

                var handler = new NodeHandler(nodeIdentifier, webSocket, jsonRpcMessageHandler);
                ConnectedNodes.ClientNodes.TryAdd(nodeIdentifier, handler);

                using (var jsonRpc = new JsonRpc(jsonRpcMessageHandler, handler))
                {
                    jsonRpc.StartListening();

                    await jsonRpc.Completion;
                }
            }
            else
            {
                context.Response.StatusCode = StatusCodes.Status400BadRequest;
            }
        }
    }
}
