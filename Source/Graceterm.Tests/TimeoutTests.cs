using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using Xunit;

namespace Graceterm.Tests
{
    public class TimeoutTests
    {
        [Fact]
        public async Task ShouldStopIfTimeoutOccur()
        {
            var server = Server.Create(new GracetermOptions()
            {
                TimeoutSeconds = 5
            });

            int requests = 10;

            var requestTasks = server.CreateRequests(requests);

            await Task.Delay(1000); // Ensure that requests will be all submitted

            // simulate sigterm
            var stopTask = Task.Run(() =>
            {
                server.Stop();
            });

            await Task.Delay(8000);

            Assert.True(GracetermMiddleware.TimeoutOccurredWithPenddingRequests);
            Assert.True(GracetermMiddleware.RequestCount >= 10, 
                $"GracetermMiddleware.RequestCount < {requests}");
            
            await stopTask;
        }
    }
}
