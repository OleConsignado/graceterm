using Microsoft.AspNetCore.Builder;

namespace Graceterm
{
    public static class GracetermApplicationBuilderExtensions
    {
        public static IApplicationBuilder UseGraceterm(this IApplicationBuilder applicationBuilder)
        {
            applicationBuilder.UseMiddleware<GracetermMiddleware>();

            return applicationBuilder;
        }
    }
}
