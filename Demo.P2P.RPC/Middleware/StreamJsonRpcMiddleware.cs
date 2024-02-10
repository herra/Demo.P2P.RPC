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
        private readonly IServiceProvider _serviceProvider;

        public StreamJsonRpcMiddleware(RequestDelegate next, IServiceProvider serviceProvider)
        {
            _serviceProvider = serviceProvider;
        }

        public async Task Invoke(HttpContext context)
        {
            if (context.WebSockets.IsWebSocketRequest)
            {
                var webSocket = await context.WebSockets.AcceptWebSocketAsync();

                IJsonRpcMessageHandler jsonRpcMessageHandler = new WebSocketMessageHandler(webSocket);

                using (var jsonRpc = new JsonRpc(jsonRpcMessageHandler, _serviceProvider.GetRequiredService<IAuctionService>()))
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
