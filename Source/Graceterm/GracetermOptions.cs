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

        public ICollection<PathString> IgnorePaths { get; set; } = new List<PathString>();

        public void AddIgnorePaths(params string[] paths)
        {
            foreach (var path in paths)
            {
                IgnorePaths.Add(path);
            }
        }

        public IServiceUnavailableResponseHandler ServiceUnavailableResponseHandler { get; set; } = new DefaultServiceUnavailableResponseHandler();
    }
}
