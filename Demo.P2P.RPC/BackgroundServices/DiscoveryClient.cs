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

namespace Demo.P2P.RPC.BackgroundServices
{
    public class DiscoveryClient : BackgroundService
    {
        private readonly IConfiguration _configuration;
        private readonly ILogger<DiscoveryClient> _logger;

        public DiscoveryClient(IConfiguration configuration, ILogger<DiscoveryClient> logger)
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
                        using (var client = new UdpClient(discoveryPort))
                        {
                            client.EnableBroadcast = true;

                            var requestData = Encoding.ASCII.GetBytes($"Listening on:{serverPort}");
                            await client.SendAsync(requestData, requestData.Length, new IPEndPoint(IPAddress.Broadcast, discoveryPort));

                            //_logger.LogInformation($"Broadcasted on port:{discoveryPort}");
                        }
                    }
                    catch (Exception ex)
                    {
                        // We are looping through discoveryPorts to find available port. App might be second, thrid .. nth starting on the machine
                    }
                }

                await Task.Delay(500); // broadcast every 5 seconds
            }
        }
    }
}
