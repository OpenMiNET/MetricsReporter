using System;
using System.Collections.Generic;
using System.Threading;
using App.Metrics;
using App.Metrics.Counter;
using log4net;
using MetricsReporter.Models;
using OpenAPI;
using OpenAPI.Events;

namespace MetricsReporter
{
    public class MetricCollector
    {
        private static readonly ILog Log = LogManager.GetLogger(typeof(MetricCollector));
        
        private IMetricsRoot Root { get; }
        private List<ICollectMetrics> MetricCollectors { get; }
        private Timer ReportTimer { get; set; }
        public MetricCollector(IMetricsRoot root)
        {
            Root = root;
            MetricCollectors = new List<ICollectMetrics>();
        }

        private void ReportMetrics(object? state)
        {
            
        }

        public void Setup(OpenAPI.OpenApi api)
        {
            MetricCollectors.Add(new ServerMetrics(Root, api.ServerInfo));
            
            
            ReportTimer = new System.Threading.Timer(ReportMetrics, null, 1000, 1000);
        }
    }

    public class ServerMetrics : IEventHandler, ICollectMetrics
    {
        private IMetricsRoot Root { get; }
        private OpenServerInfo ServerInfo { get; }

        private ICounter NetworkUpload { get; }
        private ICounter NetworkDownload { get; }
        public ServerMetrics(IMetricsRoot root, OpenServerInfo serverInfo)
        {
            Root = root;
            ServerInfo = serverInfo;
            
            NetworkUpload = root.Provider.Counter.Instance(new CounterOptions()
            {
                Name = "NETWORK_TRAFFIC_UP",
                MeasurementUnit = Unit.Bytes
            });
            
            NetworkDownload = root.Provider.Counter.Instance(new CounterOptions()
            {
                Name = "NETWORK_TRAFFIC_DOWN",
                MeasurementUnit = Unit.Bytes
            });
        }
        
        public void Collect(DateTime time)
        {
            
        }
    }
}