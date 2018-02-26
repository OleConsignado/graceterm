using Microsoft.AspNetCore.Http;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Threading.Tasks;

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
        /// Add a single path to be ignored by graceterm, if you interested in ignore multple
        /// paths, you may use <see cref="IgnorePaths(string[])"/>.
        /// </summary>
        public void IgnorePath(string path)
        {
            IgnorePaths(path);
        }

        internal Func<HttpContext, Task> CustomPostSigtermRequestsHandler { get; private set; }

        /// <summary>
        /// By default graceterm send a 503 response, with a "503 - Service unavailable" text body 
        /// for requests initiated after application has asked to terminated. You may modify this
        /// behavior by providing a delegate to handle this requests.
        /// </summary>
        public void UseCustomPostSigtermIncommingRequestsHandler(Func<HttpContext, Task> customPosSigtermRequestsHandler)
        {
            CustomPostSigtermRequestsHandler = customPosSigtermRequestsHandler;
        }

        [EditorBrowsable(EditorBrowsableState.Never)]
        [Obsolete("The method name is misspelled, use UseCustomPostSigtermIncommingRequestsHandler")]
        public void UseCustomPosSigtermIncommingRequestsHandler(Func<HttpContext, Task> customPosSigtermRequestsHandler)
        {
            UseCustomPostSigtermIncommingRequestsHandler(customPosSigtermRequestsHandler);
        }
    }
}
