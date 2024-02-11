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
using Makaretu.Dns;

namespace Demo.P2P.RPC.BackgroundServices
{
    public class DiscoveryHost : BackgroundService
    {
        const string ServiceName = "p2p.rpc";
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

            
            var service = new ServiceProfile("x", ServiceName, (ushort)discoveryPort);
            service.AddProperty("ip", localAddresses.FirstOrDefault());
            service.AddProperty("port", serverPort.ToString());

            var sd = new ServiceDiscovery();
            sd.Advertise(service);

            await AttemptDiscoveryAsync(discoveryPort, serverPort, localAddresses, stoppingToken);
        }

        private async Task AttemptDiscoveryAsync(int discoveryPort, int serverPort, string[] localAddresses, CancellationToken stoppingToken)
        {
            var mdns = new MulticastService();
            var sd = new ServiceDiscovery(mdns);

            mdns.AnswerReceived += (s, e) =>
            {
                string ip = null;
                string port = null;

                var props = e.Message.AdditionalRecords.OfType<TXTRecord>().ToList();
                foreach (var p in props)
                {
                    ip ??= p.Strings.FirstOrDefault(x => x.StartsWith("ip="));
                    port ??= p.Strings.FirstOrDefault(x => x.StartsWith("port="));
                }

                if (!string.IsNullOrEmpty(ip) && !string.IsNullOrEmpty(port))
                {
                    int portNum = int.Parse(port.Split("=").Last());
                    string ipStr = ip.Split("=").Last();
                    if (serverPort != portNum)
                    {
                        ConnectToNodeAsync(portNum, $"{ipStr}:{portNum}");
                    }
                }
            };

            mdns.Start();
            sd.QueryServiceInstances(new DomainName(ServiceName));
        }

        private async Task ConnectToNodeAsync(int port, string nodeIdentifier)
        {
            if (port <= 0 || ConnectedNodes.Nodes.ContainsKey(nodeIdentifier))
            {
                return;
            }

            try
            {
                var webSocket = new ClientWebSocket();
                await webSocket.ConnectAsync(new Uri($"wss://{nodeIdentifier}/json-rpc-auction"), CancellationToken.None);

                IJsonRpcMessageHandler jsonRpcMessageHandler = new WebSocketMessageHandler(webSocket);

                var nodeHandler = new NodeHandler(nodeIdentifier, webSocket, jsonRpcMessageHandler);
                ConnectedNodes.Nodes.TryAdd($"{nodeIdentifier}", nodeHandler);

                _logger.LogInformation($"Should start ws connection to: {port}");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Could not connect to ws {nodeIdentifier}");
            }
        }

        private static string[] GetAllLocalIPv4()
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
