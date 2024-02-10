using Demo.P2P.RPC.BackgroundServices;
using Demo.P2P.RPC.Middleware;
using Demo.P2P.RPC.Services;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.FileSystemGlobbing.Internal;
using Microsoft.Extensions.Hosting;
using System;

namespace Demo.P2P.RPC
{
    public class Startup
    {
        public void ConfigureServices(IServiceCollection services)
        {
            services.AddHostedService<DiscoveryHost>();

            services.AddSingleton<IAuctionService, AuctionService>();

            services.AddSingleton<IServiceProvider>(sp => sp);
        }

        public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }

            app.UseWebSockets()
                .UseRouting()
                .UseEndpoints(endpoints =>
                {
                    RequestDelegate pipeline = endpoints.CreateApplicationBuilder().UseMiddleware<StreamJsonRpcMiddleware>().Build();

                    endpoints.Map("/json-rpc-auction", pipeline);
                })
                .Run(async (context) =>
                {
                    await context.Response.WriteAsync("-- Demo.P2P.RPC --");
                });
        }
    }
}
