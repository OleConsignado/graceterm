using System.Threading.Tasks;
using Xunit;

namespace Graceterm.Tests
{
    public class GracefullyTerminationTests
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
            await server.ServerTask;
        }
    }
}
