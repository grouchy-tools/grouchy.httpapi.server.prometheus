using System;
using Grouchy.HttpApi.Client.Abstractions.EventCallbacks;
using Grouchy.HttpApi.Server.Abstractions.EventCallbacks;
using Grouchy.Resilience.Abstractions.CircuitBreaking;
using Microsoft.Extensions.DependencyInjection;
using Prometheus.Advanced;

namespace Grouchy.HttpApi.Server.Prometheus.Extensions
{
   public static class ServiceCollectionExtensions
   {
      public static IServiceCollection AddPrometheusServices(this IServiceCollection services)
      {
         services.AddSingleton<ICollectorRegistry>(CreateCollectorRegistry);
         services.AddSingleton<PrometheusEventCallback>();
         services.AddSingleton<IHttpServerEventCallback>(sp => sp.GetService<PrometheusEventCallback>());
         services.AddSingleton<IHttpClientEventCallback>(sp => sp.GetService<PrometheusEventCallback>());

         return services;
      }

      private static ICollectorRegistry CreateCollectorRegistry(IServiceProvider serviceProvider)
      {
         var circuitBreakerManager = serviceProvider.GetService<ICircuitBreakerManager>();
         
         var collectorRegistry = new DefaultCollectorRegistry();
         collectorRegistry.GetOrAdd(new CircuitBreakerStateCollector(circuitBreakerManager));

         return collectorRegistry;
      }
   }
}