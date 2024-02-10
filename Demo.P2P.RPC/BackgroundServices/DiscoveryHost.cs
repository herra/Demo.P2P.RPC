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
using System.Net.NetworkInformation;

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
            var discoveryPort = _configuration.GetValue<int>("DiscoveryPort");
            var serverPort = _configuration.GetValue<int>("ServerPort");
            var localAddresses = GetAllLocalIPv4();

            while (!stoppingToken.IsCancellationRequested)
            {
                try
                {
                    using (var client = new UdpClient(discoveryPort))
                    {
                        client.EnableBroadcast = true;

                        var requestData = Encoding.ASCII.GetBytes($"Listening on:{serverPort}");
                        //await client.SendAsync(requestData, requestData.Length, new IPEndPoint(IPAddress.Broadcast, discoveryPort));



                        var receiveData = await client.ReceiveAsync();
                        var clientResponse = Encoding.ASCII.GetString(receiveData.Buffer);

                        //_logger.LogInformation("Received {0} from {1}, sending response", clientResponse, receiveData.RemoteEndPoint.ToString());

                        var parts = clientResponse.Split(":");
                        var port = int.Parse(parts[1]);

                        if (localAddresses.Contains(receiveData.RemoteEndPoint.Address.ToString()) && port == serverPort)
                        {
                            continue;
                        }

                        var nodeIdentifier = $"{receiveData.RemoteEndPoint.Address}:{port}";
                        if (port > 0 && !ConnectedNodes.Nodes.ContainsKey(nodeIdentifier))
                        {
                            using (var webSocket = new ClientWebSocket())
                            {
                                try
                                {
                                    await webSocket.ConnectAsync(new Uri($"wss://{nodeIdentifier}/json-rpc-auction"), CancellationToken.None);

                                    IJsonRpcMessageHandler jsonRpcMessageHandler = new WebSocketMessageHandler(webSocket);

                                    var nodeHandler = new NodeHandler(nodeIdentifier, webSocket, jsonRpcMessageHandler);
                                    ConnectedNodes.Nodes.TryAdd($"{nodeIdentifier}", nodeHandler);

                                    _logger.LogInformation($"Should start ws connection to: {port}");
                                }
                                catch(Exception ex)
                                {
                                    _logger.LogError($"Could not connect to ws {nodeIdentifier}");
                                }
                            }
                        }

                        await Task.Delay(5000);
                    }
                }
                catch (Exception ex)
                {
                    // reading might fail
                }
            }
        }

        public static string[] GetAllLocalIPv4()
        {
            List<string> ipAddrList = new List<string>();
            foreach (NetworkInterface item in NetworkInterface.GetAllNetworkInterfaces())
            {
                if (item.OperationalStatus == OperationalStatus.Up)
                {
                    foreach (UnicastIPAddressInformation ip in item.GetIPProperties().UnicastAddresses)
                    {
                        if (ip.Address.AddressFamily == AddressFamily.InterNetwork)
                        {
                            ipAddrList.Add(ip.Address.ToString());
                        }
                    }
                }
            }
            return ipAddrList.ToArray();
        }
    }
}
