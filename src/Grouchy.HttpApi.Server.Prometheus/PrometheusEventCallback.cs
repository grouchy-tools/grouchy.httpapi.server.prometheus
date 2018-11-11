using Grouchy.HttpApi.Client.Abstractions.EventCallbacks;
using Grouchy.HttpApi.Client.Abstractions.Events;
using Grouchy.HttpApi.Server.Abstractions.EventCallbacks;
using Grouchy.HttpApi.Server.Abstractions.Events;
using Prometheus;
using Prometheus.Advanced;

namespace Grouchy.HttpApi.Server.Prometheus
{
   public class PrometheusEventCallback : IHttpClientEventCallback, IHttpServerEventCallback
   {
      private readonly Counter _httpServerRequestCounter;
      private readonly Histogram _httpServerResponseHistogram;
      private readonly Histogram _httpServerExceptionHistogram;
      private readonly Counter _httpClientRequestCounter;
      private readonly Histogram _httpClientResponseHistogram;
      private readonly Counter _httpClientRetryCounter;
      private readonly Histogram _httpClientExceptionHistogram;
      private readonly Histogram _httpClientTimedOutHistogram;
      private readonly Histogram _httpClientServerUnavailableHistogram;

      public PrometheusEventCallback(ICollectorRegistry collectorRegistry)
      {
         var metricsFactory = new MetricFactory(collectorRegistry);

         _httpServerRequestCounter = metricsFactory.CreateCounter("http_server_request", "HTTP server requests",
            new CounterConfiguration
            {
               // TODO: Useful part of path?
               LabelNames = new[] {"method"}
            });

         _httpServerResponseHistogram = metricsFactory.CreateHistogram("http_server_response",
            "HTTP server response duration (ms)", new HistogramConfiguration
            {
               // TODO: Useful part of path?
               LabelNames = new[] {"method", "statusCode"},
               Buckets = new double[] {10, 30, 100, 300, 1000, 3000, 10000}
            });

         _httpServerExceptionHistogram = metricsFactory.CreateHistogram("http_server_exception",
            "HTTP server exception duration (ms)", new HistogramConfiguration
            {
               // TODO: Useful part of path?
               // TODO: Exception name maybe?
               LabelNames = new[] {"method"},
               Buckets = new double[] {10, 30, 100, 300, 1000, 3000, 10000}
            });

         _httpClientRequestCounter = metricsFactory.CreateCounter("http_client_request", "HTTP client requests",
            new CounterConfiguration
            {
               // TODO: Useful part of path?
               LabelNames = new[] {"method", "targetService"}
            });

         _httpClientResponseHistogram = metricsFactory.CreateHistogram("http_client_response",
            "HTTP client response duration (ms)", new HistogramConfiguration
            {
               // TODO: Useful part of path?
               LabelNames = new[] {"method", "statusCode", "targetService"},
               Buckets = new double[] {10, 30, 100, 300, 1000, 3000, 10000}
            });

         _httpClientRetryCounter = metricsFactory.CreateCounter("http_client_retry", "HTTP client retrys",
            new CounterConfiguration
            {
               // TODO: Useful part of path?
               LabelNames = new[] {"method", "targetService"}
            });

         _httpClientExceptionHistogram = metricsFactory.CreateHistogram("http_client_exception",
            "HTTP client exception duration (ms)", new HistogramConfiguration
            {
               // TODO: Useful part of path?
               LabelNames = new[] {"method", "targetService"},
               Buckets = new double[] {10, 30, 100, 300, 1000, 3000, 10000}
            });

         _httpClientTimedOutHistogram = metricsFactory.CreateHistogram("http_client_timed_out",
            "HTTP client timed out duration (ms)", new HistogramConfiguration
            {
               // TODO: Useful part of path?
               LabelNames = new[] {"method", "targetService"},
               Buckets = new double[] {10, 30, 100, 300, 1000, 3000, 10000}
            });

         _httpClientServerUnavailableHistogram = metricsFactory.CreateHistogram("http_client_server_unavailable",
            "HTTP client server unavailable duration (ms)", new HistogramConfiguration
            {
               // TODO: Useful part of path?
               LabelNames = new[] {"method", "targetService"},
               Buckets = new double[] {10, 30, 100, 300, 1000, 3000, 10000}
            });
      }

      // TODO: Disconnect this from the request thread by using BlockingCollection
      public void Invoke(IHttpClientEvent @event)
      {
         Handle((dynamic) @event);
      }

      // TODO: Disconnect this from the request thread by using BlockingCollection
      public void Invoke(IHttpServerEvent @event)
      {
         Handle((dynamic) @event);
      }

      private void Handle(IHttpServerRequestEvent e)
      {
         _httpServerRequestCounter.WithLabels(e.Method).Inc();
      }

      private void Handle(IHttpServerResponseEvent e)
      {
         _httpServerResponseHistogram.WithLabels(e.Method, e.StatusCode.ToString()).Observe(e.DurationMs);
      }

      private void Handle(IHttpServerExceptionEvent e)
      {
         _httpServerExceptionHistogram.WithLabels(e.Method).Observe(e.DurationMs);
      }

      private void Handle(IHttpClientRequestEvent e)
      {
         _httpClientRequestCounter.WithLabels(e.Method, e.TargetService).Inc();
      }

      private void Handle(IHttpClientResponseEvent e)
      {
         _httpClientResponseHistogram.WithLabels(e.Method, e.StatusCode.ToString(), e.TargetService)
            .Observe(e.DurationMs);
      }

      private void Handle(IHttpClientRetryEvent e)
      {
         _httpClientRetryCounter.WithLabels(e.Method, e.TargetService).Inc();
      }

      private void Handle(IHttpClientExceptionEvent e)
      {
         Histogram histogram;

         switch (e.EventType)
         {
            case "HttpClientTimedOut":
               histogram = _httpClientTimedOutHistogram;
               break;
            case "HttpClientServerUnavailable":
               histogram = _httpClientServerUnavailableHistogram;
               break;
            default:
               histogram = _httpClientExceptionHistogram;
               break;
         }

         histogram.WithLabels(e.Method, e.TargetService).Observe(e.DurationMs);
      }
   }
}