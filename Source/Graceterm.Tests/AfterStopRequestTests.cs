using System.Net;
using System.Threading.Tasks;
using Xunit;

namespace Graceterm.Tests
{
    public class AfterStopRequestTests
    {
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

            await Task.Delay(100); // Ensure the stopTask will run
            await Task.Delay(1500); // Wait to GracetermMiddleware._stopRequested assume true

            var result = await server.CreateRequest();
            Assert.Equal(HttpStatusCode.ServiceUnavailable, result.StatusCode);
            await stopTask;
        }
    }
}
