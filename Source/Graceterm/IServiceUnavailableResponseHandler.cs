using Microsoft.AspNetCore.Http;
using System.Threading.Tasks;

namespace Graceterm
{
    public interface IServiceUnavailableResponseHandler
    {
        Task GenerateResponseAsync(HttpContext httpContext);
    }
}
