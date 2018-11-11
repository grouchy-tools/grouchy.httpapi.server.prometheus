using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.DependencyInjection;
using Prometheus;
using Prometheus.Advanced;

namespace Grouchy.HttpApi.Server.Prometheus.Extensions
{
   public static class ApplicationBuilderExtensions
   {
      public static IApplicationBuilder UsePrometheusMiddleware(this IApplicationBuilder app)
      {
         var collectorRegistry = app.ApplicationServices.GetService<ICollectorRegistry>();
         
         app.UseMetricServer("/.metrics", collectorRegistry);

         return app;
      }
   }
}