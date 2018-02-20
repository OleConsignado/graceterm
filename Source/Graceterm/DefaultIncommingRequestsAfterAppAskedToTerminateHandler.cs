using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;

namespace Graceterm
{
    internal class DefaultIncommingRequestsAfterAppAskedToTerminateHandler : IIncommingRequestsAfterAppAskedToTerminateHandler
    {
        public async Task HandleRequestAsync(HttpContext httpContext)
        {
            httpContext.Response.StatusCode = 503;
            await httpContext.Response.WriteAsync("503 - Service unavailable.");
        }
    }
}
