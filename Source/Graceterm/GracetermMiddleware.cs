using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace Graceterm
{
    public class GracetermMiddleware
    {
        public const string LoggerCategory = "GracefullShutdown";
        public const int SigtermReceived = 0x6E870001;
        public const int WaitingForPendingRequests = 0x6E870002;
        public const int TerminatingGracefully = 0x6E870003;

        private readonly RequestDelegate _next;
        private readonly object _lockPad = new object();
        private readonly ILogger _logger;
        private int _requestCount = 0;

        public GracetermMiddleware(RequestDelegate next, IApplicationLifetime applicationLifetime, ILoggerFactory loggerFactory)
        {
            _next = next ?? throw new ArgumentNullException(nameof(next));

            if (applicationLifetime == null)
            {
                throw new ArgumentNullException(nameof(applicationLifetime));
            }

            applicationLifetime.ApplicationStopping.Register(OnApplicationStopping);
            _logger = loggerFactory?.CreateLogger(LoggerCategory);
        }

        private void OnApplicationStopping()
        {
            _logger?.LogInformation(SigtermReceived, "Application stopping, waiting for pending requests to complete...");

            do
            {
                Thread.Sleep(1000);
                _logger?.LogInformation(WaitingForPendingRequests, "Current request count: {RequestCount}", _requestCount);
            }
            while (_requestCount > 0);

            _logger?.LogInformation(TerminatingGracefully, "Done! Application will now terminate.");
        }

        public async Task Invoke(HttpContext httpContext)
        {
            lock (_lockPad)
            {
                _requestCount++;
            }

            await _next.Invoke(httpContext);

            lock (_lockPad)
            {
                _requestCount--;
            }
        }
    }

}
