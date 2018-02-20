using Microsoft.AspNetCore.Http;
using System.Threading.Tasks;

namespace Graceterm
{
    public interface IIncommingRequestsAfterAppAskedToTerminateHandler
    {
        /// <summary>
        /// Handle wrong incomming resquests that is initiated after application asked to terminate.
        /// </summary>
        Task HandleRequestAsync(HttpContext httpContext);
    }
}
