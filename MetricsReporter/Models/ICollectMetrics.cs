using System;

namespace MetricsReporter.Models
{
    public interface ICollectMetrics
    {
        public void Collect(DateTime time);
    }
}