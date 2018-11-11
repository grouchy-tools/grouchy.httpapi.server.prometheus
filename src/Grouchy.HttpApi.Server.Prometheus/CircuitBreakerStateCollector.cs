using System.Collections.Generic;
using Grouchy.Resilience.Abstractions.CircuitBreaking;
using Prometheus.Advanced;
using Prometheus.Advanced.DataContracts;

namespace Grouchy.HttpApi.Server.Prometheus
{
   public class CircuitBreakerStateCollector : ICollector
   {
      private readonly ICircuitBreakerManager _circuitBreakerManager;
      
      public string Name { get; } = "circuit_breaker_state";
      
      public string[] LabelNames { get; } = { "policy" };

      public CircuitBreakerStateCollector(ICircuitBreakerManager circuitBreakerManager)
      {
         _circuitBreakerManager = circuitBreakerManager;
      }
      
      public IEnumerable<MetricFamily> Collect()
      {
         var httpClientWithheldCounter = new MetricFamily { name = "http_client_withheld", type = MetricType.COUNTER};
         var httpClientCircuitClosedPctGauge = new MetricFamily { name = "http_client_circuit_closed_pct", type = MetricType.GAUGE};

         foreach (var state in _circuitBreakerManager.GetStates())
         {
            httpClientCircuitClosedPctGauge.metric.Add(CreateCounter(state));
         }
         
         yield return httpClientWithheldCounter;
         yield return httpClientCircuitClosedPctGauge;
      }

      private Metric CreateCounter(ICircuitBreakerState circuitBreakerState)
      {
         return new Metric
         {
            gauge = new Gauge
            {
               value = circuitBreakerState.ClosedPct,
            },
            label = new List<LabelPair> {new LabelPair {name = "policy", value = circuitBreakerState.Policy}}
         };
      }   
   }
}