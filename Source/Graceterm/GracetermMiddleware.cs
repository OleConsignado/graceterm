using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using System;
using System.ComponentModel;
using System.Text;
using System.Threading.Tasks;

namespace Graceterm
{
    /// <summary>
    /// Graceterm middleware provides implementation to ensure graceful shutdown of aspnet core applications. 
    /// It was originally written to get zero downtime while performing Kubernetes rolling updates.
    /// The basic concept is: After aplication received a SIGTERM (a signal asking it to terminate), 
    /// Graceterm will hold it alive till all pending requests are completed or a timeout ocurr. 
    /// </summary>
    public class GracetermMiddleware
    {
        /// <summary>
        /// The logger category for log events created here.
        /// </summary>
        public const string LoggerCategory = "Graceterm";

        private readonly RequestDelegate _next;
        private static volatile object _lockPad = new object();
        private readonly ILogger _logger;
        private static volatile int _requestCount = 0;
        private readonly GracetermOptions _options;

        private Func<HttpContext, Task> _posSigtermRequestsHandler = async (httpContext) => 
        {
            httpContext.Response.StatusCode = 503;
            await httpContext.Response.WriteAsync("503 - Service unavailable.");
        };

        #region [Test properties]

        //
        // this properties exists for test purpose only (used in TimeoutTests.ShouldStopIfTimeoutOccur)
        //

        [EditorBrowsable(EditorBrowsableState.Never)]
        public static int RequestCount => _requestCount;

        [EditorBrowsable(EditorBrowsableState.Never)]
        public static bool TimeoutOccurredWithPenddingRequests { get; private set; }

        #endregion

        public GracetermMiddleware(RequestDelegate next, IApplicationLifetime applicationLifetime, ILoggerFactory loggerFactory, IOptions<GracetermOptions> options)
        {
            _next = next ?? throw new ArgumentNullException(nameof(next));
            _logger = loggerFactory?.CreateLogger(LoggerCategory) ?? throw new ArgumentNullException(nameof(loggerFactory));
            _options = options?.Value ?? throw new ArgumentException(nameof(options));

            if (applicationLifetime == null)
            {
                throw new ArgumentNullException(nameof(applicationLifetime));
            }

            if (_options.CustomPostSigtermRequestsHandler != null)
            {
                _posSigtermRequestsHandler = _options.CustomPostSigtermRequestsHandler;
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
            return ComputeIntegerTimeReference() - _stopRequestedTime > _options.TimeoutSeconds;
        }

        private void OnApplicationStopping()
        {
            _logger.LogInformation("Sigterm received, will waiting for pending requests to complete if has any.");

            do
            {
                Task.Delay(1000).Wait();
                _logger.LogInformation("Waiting for pending requests, current request count: {RequestCount}.", _requestCount);

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

                // This assignment is done for tests purpose only, TimeoutOccurredWithPenddingRequests will be checked in TimeoutTests.ShouldStopIfTimeoutOccur
                // to verify if the condition occured.
                TimeoutOccurredWithPenddingRequests = true;

                // Ensure to terminate process if it dont terminate by it self.
                Task.Run(async () =>
                {
                    await Task.Delay(30000);
                    _logger.LogWarning("(TIMEOUT) Forcing process to exit, it should terminated by it self, if you seeing this message, must be something wrong.");
                    await Task.Delay(1000);
                    Environment.Exit(124);
                });
                
            }
            else
            {
                _logger.LogInformation("Pending requests were completed, application will now terminate gracefully.");
            }
        }

        private void OnApplicationStopped()
        {
            _logger.LogInformation("ApplicationStopped event fired.");
        }

        

        public async Task Invoke(HttpContext httpContext)
        {
            if (ShouldIgnore(httpContext))
            {
                await _next.Invoke(httpContext);
            }
            else if (_stopRequested)
            {
                await HandleIncommingRequestAfterAppAskedToTerminate(httpContext);
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

        private bool ShouldIgnore(HttpContext httpContext)
        {
            foreach (var ignoredPath in _options.IgnoredPaths)
            {
                if (httpContext.Request.Path.StartsWithSegments(ignoredPath))
                {
                    return true;
                }
            }

            return false;
        }

        private async Task HandleIncommingRequestAfterAppAskedToTerminate(HttpContext httpContext)
        {
            _logger.LogCritical("Request received, but this application instance is not accepting new requests because it asked for terminate (eg.: a sigterm were received). Seding response as service unavailable (HTTP 503).");

            await _posSigtermRequestsHandler.Invoke(httpContext);
        }
    }

}
