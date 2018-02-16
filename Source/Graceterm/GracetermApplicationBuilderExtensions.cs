using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.Options;
using System;

namespace Graceterm
{
    public static class GracetermApplicationBuilderExtensions
    {
        public static IApplicationBuilder UseGraceterm(this IApplicationBuilder applicationBuilder)
        {
            applicationBuilder.UseMiddleware<GracetermMiddleware>(Options.Create(new GracetermOptions()
            {
                Timeout = 60
            }));

            return applicationBuilder;
        }

        public static IApplicationBuilder UseGraceterm(this IApplicationBuilder applicationBuilder, GracetermOptions options)
        {
            if (options == null)
            {
                throw new ArgumentNullException(nameof(options));
            }

            applicationBuilder.UseMiddleware<GracetermMiddleware>(Options.Create(options));

            return applicationBuilder;
        }
    }
}
