using System.Net;
using System.Threading.Tasks;
using Xunit;

namespace Graceterm.Tests
{
    public class GracetermTests
    {
        [Fact]
        public async Task ShouldProcessAllPendingRequestsAfterStopRequest()
        {
            var server = Server.Create();
            var requestTasks = server.CreateRequests(10);

            await Task.Delay(1000); // Ensure that requests will be all submitted

            // simulate sigterm
            var stopTask = Task.Run(() =>
            {
                server.Stop();
            });

            await Task.Delay(10); // Ensure the stopTask will run

            foreach (var requestTask in requestTasks)
            {
                var responseMessage = await requestTask;
                var resultMessage = await responseMessage.Content.ReadAsStringAsync();
                Assert.Equal(Server.ResponseContent, resultMessage);
            }

            await stopTask;
            //await server.ServerTask;
        }

        [Fact]
        public async Task ShouldReturnServiceUnavailableForResquestCreatedAfterStopRequest()
        {
            var server = Server.Create();
            server.CreateRequests(1); // Create a request before ask to terminate in order to hold server up

            await Task.Delay(1000); // Ensure that requests will be all submitted

            // simulate sigterm
            var stopTask = Task.Run(() =>
            {
                server.Stop();
            });

            await Task.Delay(10); // Ensure the stopTask will run

            var result = await server.CreateRequest();
            Assert.Equal(HttpStatusCode.ServiceUnavailable, result.StatusCode);
            await stopTask;
            //await server.ServerTask;
        }
    }
}
