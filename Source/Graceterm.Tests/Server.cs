using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Logging;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using System;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using System.Net.Http;
using System.Collections.Generic;
using Microsoft.AspNetCore.TestHost;

namespace Graceterm.Tests
{
    public class Server
    {
        public const string ResponseContent = "hello";
        private TestServer _testServer;
        private IApplicationLifetime _applicationLifetime;

        public Task<HttpResponseMessage> CreateRequest()
        {
            var requestMessage = new HttpRequestMessage(new HttpMethod("GET"), "/");

            return Task.Factory.StartNew(() =>
                    _testServer.CreateClient().GetAsync("/").Result,
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
            var webHostBuilder = new WebHostBuilder()
                .ConfigureLogging(loggingBuilder =>
                {
                    loggingBuilder.AddDebug();
                    loggingBuilder.SetMinimumLevel(LogLevel.Trace);
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

            _testServer = new TestServer(webHostBuilder);
        }
    }
}
