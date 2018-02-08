using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Logging;
using System.Net;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using System;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using System.Net.Http;
using System.Collections.Generic;
using System.Net.Sockets;

namespace Graceterm.Tests
{
    public class Server
    {
        public const string ResponseContent = "hello";
        private readonly int port;
        //private const string ip = "localhost";

        public Task ServerTask { get; set; }

        private IApplicationLifetime _applicationLifetime;

        public Task<HttpResponseMessage> CreateRequest()
        {
            var requestMessage = new HttpRequestMessage(new HttpMethod("GET"), "/");

            return Task.Factory.StartNew(() =>
                    new HttpClient() { BaseAddress = new Uri($"http://localhost:{port}") }
                        .SendAsync(requestMessage).Result,
                TaskCreationOptions.LongRunning);
        }

        public IEnumerable<Task<HttpResponseMessage>> CreateRequests(int num)
        {
            var requestTasks = new List<Task<HttpResponseMessage>>();

            for (int i = 0; i < num; i++)
            {
                requestTasks.Add(CreateRequest());
            }

            return requestTasks;
        }

        public void Stop()
        {
            _applicationLifetime.StopApplication();
        }

        public static Server Create(GracetermOptions gracetermOptions)
        {
            return new Server(gracetermOptions);
        }

        public static Server Create()
        {
            return new Server(null);
        }

        protected Server(GracetermOptions gracetermOptions)
        {
            port = FreeTcpPort();

            var webHostBuilder = new WebHostBuilder()
                .UseKestrel(o => o.Listen(IPAddress.Parse("::1"), port))
                .ConfigureLogging(loggingBuilder =>
                {
                    loggingBuilder.AddDebug();
                })
                .Configure(app => {
                    _applicationLifetime = app.ApplicationServices.GetService<IApplicationLifetime>();

                    if(_applicationLifetime == null)
                    {
                        throw new InvalidOperationException("Could not get IApplicationLifetime service!");
                    }

                    if (gracetermOptions == null)
                    {
                        app.UseGraceterm();
                    }
                    else
                    {
                        app.UseGraceterm(gracetermOptions);
                    }

                    app.Run(async context =>
                    {
                        context.Response.StatusCode = 200;
                        await Task.Delay(new Random().Next(10000, 20000));
                        await context.Response.WriteAsync(ResponseContent);
                    });
                }
            );

            ServerTask = Task.Factory.StartNew(() =>
            {
                using (var webHost = webHostBuilder.Build())
                {
                    webHost.Run();
                }
            }, TaskCreationOptions.LongRunning);
        }

        // based on https://stackoverflow.com/questions/138043/find-the-next-tcp-port-in-net
        private static int FreeTcpPort()
        {
            TcpListener l = new TcpListener(IPAddress.Loopback, 0);
            l.Start();
            int port = ((IPEndPoint)l.LocalEndpoint).Port;
            l.Stop();
            return port;
        }
    }
}
