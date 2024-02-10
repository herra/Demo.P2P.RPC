using Microsoft.Extensions.Hosting;
using System.Net.Sockets;
using System.Net;
using System.Text;
using System;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Configuration;
using System.Collections;
using System.Collections.Generic;
using Microsoft.Extensions.Logging;
using System.Linq;
using StreamJsonRpc;
using System.Net.WebSockets;

namespace Demo.P2P.RPC.BackgroundServices
{
    public class DiscoveryHost : BackgroundService
    {
        private readonly IConfiguration _configuration;
        private readonly ILogger<DiscoveryHost> _logger;

        public DiscoveryHost(IConfiguration configuration, ILogger<DiscoveryHost> logger)
        {
            _configuration = configuration;
            _logger = logger;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            var discoveryPorts = _configuration.GetSection("DiscoveryPorts").GetChildren().Select(i => int.Parse(i.Value)).ToArray();
            var serverPort = _configuration.GetValue<int>("ServerPort");

            while (!stoppingToken.IsCancellationRequested)
            {
                foreach (var discoveryPort in discoveryPorts)
                {
                    try
                    {
                        using (var server = new UdpClient(discoveryPort))
                        {
                            var receiveData = await server.ReceiveAsync();

                            var clientResponse = Encoding.ASCII.GetString(receiveData.Buffer);

                            _logger.LogInformation("Received {0} from {1}, sending response", clientResponse, receiveData.RemoteEndPoint.ToString());

                            var parts = clientResponse.Split(":");
                            var port = int.Parse(parts[1]);

                            var nodeIdentifier = $"localhost:{port}";
                            if (port > 0 && port != serverPort && ConnectedNodes.Nodes.ContainsKey(nodeIdentifier))
                            {
                                using (var webSocket = new ClientWebSocket())
                                {
                                    await webSocket.ConnectAsync(new Uri($"ws://{nodeIdentifier}/json-rpc-auction"), CancellationToken.None);

                                    IJsonRpcMessageHandler jsonRpcMessageHandler = new WebSocketMessageHandler(webSocket);

                                    _ = new NodeHandler(nodeIdentifier, webSocket, jsonRpcMessageHandler);

                                    _logger.LogInformation($"Should start ws connection to: {port}");
                                }
                            }

                            await Task.Delay(1000);
                        }
                    }
                    catch (Exception ex)
                    {
                        // reading might fail
                    }
                }
            }
        }
    }
}
