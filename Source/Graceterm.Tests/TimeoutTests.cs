using System;
using System.Threading.Tasks;
using Xunit;

namespace Graceterm.Tests
{
    public class TimeoutTests
    {
        //[Fact]
        //public async Task ShouldTerminaIfTimeout()
        //{
        //    int timeout = 9000;
        //    var server = Server.Create(new GracetermOptions() { Timeout = timeout });
        //    var requestTasks = server.CreateRequests(10);

        //    await Task.Delay(1000); // Ensure that requests will be all submitted

        //    // simulate sigterm
        //    var stopTask = Task.Run(() =>
        //    {
        //        server.Stop();
        //        server._testServer.Dispose();
        //    });

        //    //await Task.Delay(10); // Ensure the stopTask will run

        //    // TODO: Here is expected a HttpRequestException but an AggregateException is
        //    // throwing with HttpRequestException as it's InnerException
        //    // The todo task is to find a better way to handle this.
        //    await Assert.ThrowsAnyAsync<AggregateException>(async () =>
        //    {
        //        foreach (var requestTask in requestTasks)
        //        {
        //            var responseMessage = await requestTask;
        //        }
        //    });

        //    await stopTask;
        //    //await server.ServerTask;
        //}
    }
}
