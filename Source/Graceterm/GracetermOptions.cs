using Microsoft.AspNetCore.Http;
using System.Collections.Generic;

namespace Graceterm
{
    /// <summary>
    /// Graceterm options.
    /// </summary>
    public class GracetermOptions
    {
        /// <summary>
        /// Timeout in seconds (default is 60 seconds).
        /// </summary>
        public int TimeoutSeconds { get; set; } = 60;

        internal ICollection<PathString> IgnoredPaths { get; set; } = new List<PathString>();

        /// <summary>
        /// Add paths that should be ignored by graceterm.
        /// If a incoming request path starts with any item in this list, this requests
        /// will be not handle by Graceterm, this means that Graceterm will not ensure 
        /// this particular requests get complete before application shutdown.
        /// </summary>
        public void IgnorePaths(params string[] paths)
        {
            foreach (var path in paths)
            {
                IgnoredPaths.Add(path);
            }
        }

        /// <summary>
        /// By default graceterm send a 503 response, with a "503 - Service unavailable" text body 
        /// for requests initiated after application has asked to terminated. You may modify this
        /// behavior by providing a <see cref="IIncommingRequestsAfterAppAskedToTerminateHandler"/> custom
        /// implementation.
        /// </summary>
        public IIncommingRequestsAfterAppAskedToTerminateHandler IncommingRequestsAfterAppAskedToTerminateHandler { get; set; } = new DefaultIncommingRequestsAfterAppAskedToTerminateHandler();
    }
}
