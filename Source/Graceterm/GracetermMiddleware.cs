using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using System;
using System.Diagnostics;
using System.Threading.Tasks;

namespace Graceterm
{
    public class GracetermMiddleware
    {
        public const string LoggerCategory = "Graceterm";

        public static class EventId
        {
            // Information
            public const int SigtermReceived = 0x0ab0;
            public const int WaitingForPendingRequests = 0x0ab1;
            public const int TerminatingGracefully = 0x0ab2;

            // Critical 
            public const int IrregularRequestReceived = 0x1ab0;
            public const int TimedOut = 0x1ab1;
        }

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

        }

        private volatile static bool _stopRequested = false;

        private void OnApplicationStopping()
        {
            var stopwatch = new Stopwatch();
            stopwatch.Start();

            _stopRequested = true;

            _logger.LogInformation(EventId.SigtermReceived, Messages.SigtermReceived);

            do
            {
                Task.Delay(1000).Wait();
                _logger.LogInformation(EventId.WaitingForPendingRequests, Messages.WaitingForPendingRequests, _requestCount);
            }
            while (_requestCount > 0 && stopwatch.ElapsedMilliseconds <= _options.Timeout);

            stopwatch.Stop();

            if (_requestCount > 0 && stopwatch.ElapsedMilliseconds > _options.Timeout)
            {
                _logger.LogCritical(EventId.TimedOut, Messages.TimedOut, _requestCount);
            }
            else
            {
                _logger.LogInformation(EventId.TerminatingGracefully, Messages.TerminatingGracefully);
            }
        }

        public async Task Invoke(HttpContext httpContext)
        {
            if (_stopRequested)
            {
                _logger.LogCritical(EventId.IrregularRequestReceived, Messages.IrregularRequestReceived);
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
