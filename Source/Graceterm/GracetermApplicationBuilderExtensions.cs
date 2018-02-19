using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.Options;
using System;

namespace Graceterm
{
    /// <summary>
    /// Graceterm ApplicationBuilder extensions provides convenient way to add Graceterm middleware to your application pipeline.
    /// </summary>
    public static class GracetermApplicationBuilderExtensions
    {
        /// <summary>
        /// Add Graceterm middleware to requests pipeline with default options <see cref="GracetermOptions"/>.
        /// In order to graceterm work properly, you should add it just after log configuration (if you have one), before any other middleware like Mvc, Swagger etc.
        /// </summary>
        /// <param name="applicationBuilder">The applicationBuilder to configure.</param>
        /// <returns>The applicationBuilder</returns>
        public static IApplicationBuilder UseGraceterm(this IApplicationBuilder applicationBuilder)
        {
            return UseGraceterm(applicationBuilder, (GracetermOptions)null);
        }

        /// <summary>
        /// Add Graceterm middleware to requests pipeline.
        /// In order to graceterm work properly, you should add it just after log configuration (if you have one), before any other middleware like Mvc, Swagger etc.
        /// </summary>
        /// <param name="applicationBuilder">The applicationBuilder to configure.</param>
        /// <param name="options">User options <see cref="GracetermOptions"/>.</param>
        /// <returns>The applicationBuilder</returns>        
        public static IApplicationBuilder UseGraceterm(this IApplicationBuilder applicationBuilder, GracetermOptions options)
        {
            if (options == null)
            {
                options = new GracetermOptions();
            }

            applicationBuilder.UseMiddleware<GracetermMiddleware>(Options.Create(options));

            return applicationBuilder;
        }

        /// <summary>
        /// Add Graceterm middleware to requests pipeline.
        /// In order to graceterm work properly, you should add it just after log configuration (if you have one), before any other middleware like Mvc, Swagger etc.
        /// </summary>
        /// <param name="applicationBuilder">The applicationBuilder to configure.</param>
        /// <param name="optionsLambda">Convenient way to pass user configuration as lambda <see cref="GracetermOptions"/>.</param>
        /// <returns>The applicationBuilder</returns> 
        public static IApplicationBuilder UseGraceterm(this IApplicationBuilder applicationBuilder, Action<GracetermOptions> optionsLambda)
        {
            if(optionsLambda == null)
            {
                throw new ArgumentNullException(nameof(optionsLambda));
            }

            var options = new GracetermOptions();
            optionsLambda.Invoke(options);

            return UseGraceterm(applicationBuilder, options);
        }
    }
}
