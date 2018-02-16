using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using System;
using System.Text;
using System.Threading.Tasks;

namespace Graceterm
{
    public class GracetermMiddleware
    {
        public const string LoggerCategory = "Graceterm";
        private readonly RequestDelegate _next;
        private static volatile object _lockPad = new object();
        private readonly ILogger _logger;
        private static volatile int _requestCount = 0;
        private readonly GracetermOptions _options;

        public GracetermMiddleware(RequestDelegate next, IApplicationLifetime applicationLifetime, ILoggerFactory loggerFactory, IOptions<GracetermOptions> options)
        {
            _next = next ?? throw new ArgumentNullException(nameof(next));
            _logger = loggerFactory?.CreateLogger(LoggerCategory) ?? throw new ArgumentNullException(nameof(loggerFactory));
            _options = options?.Value ?? throw new ArgumentException(nameof(options));

            if (applicationLifetime == null)
            {
                throw new ArgumentNullException(nameof(applicationLifetime));
            }

            applicationLifetime.ApplicationStopping.Register(OnApplicationStopping);
            applicationLifetime.ApplicationStopped.Register(OnApplicationStopped);

        }

        private volatile static bool _stopRequested = false;
        private volatile static int _stopRequestedTime = 0;
        private static long AssemblyLoadedWhenInTicks = DateTime.Now.Ticks;

        private int ComputeIntegerTimeReference()
            =>
            (int)(((DateTime.Now.Ticks - AssemblyLoadedWhenInTicks) / TimeSpan.TicksPerMillisecond / 1000) & 0x3fffffff);

        private bool TimeoutOccurred()
        {
            return ComputeIntegerTimeReference() - _stopRequestedTime > _options.Timeout;
        }

        private void OnApplicationStopping()
        {
            _logger.LogInformation("Sigterm received, will waiting for pending requests to complete if has any.");

            do
            {
                Task.Delay(1000).Wait();
                _logger.LogDebug("Waiting for pending requests, current request count: {RequestCount}.", _requestCount);

                if (!_stopRequested)
                {
                    _stopRequested = true;
                    _stopRequestedTime = ComputeIntegerTimeReference();
                }
            }
            while (_requestCount > 0 && !TimeoutOccurred());

            if (_requestCount > 0 && TimeoutOccurred())
            {
                _logger.LogCritical("Timeout ocurred! Application will terminate with {RequestCount} pedding requests.", _requestCount);
            }
            else
            {
                _logger.LogInformation("Pending requests were completed, application will now terminate gracefully.");
            }
        }

        private void OnApplicationStopped()
        {
            _logger.LogDebug("ApplicationStopped event fired.");
        }

        public async Task Invoke(HttpContext httpContext)
        {
            if (_stopRequested)
            {
                using (_logger.BeginScope("Irregular request received"))
                {
                    _logger.LogCritical("Request received, but this application instance is not accepting new requests because it asked for terminate (eg.: a sigterm were received). Responding as service unavailable (HTTP 503).");
                    var sb = new StringBuilder();
                    sb.AppendLine("Request Headers:");

                    foreach (var header in httpContext.Request.Headers)
                    {
                        sb.AppendLine($"{header.Key}: {header.Value}");
                    }

                    _logger.LogDebug(sb.ToString());
                }

                httpContext.Response.StatusCode = 503;
                await httpContext.Response.WriteAsync("503 - Service unavailable.");
            }
            else
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

}
