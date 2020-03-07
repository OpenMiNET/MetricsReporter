
using System;
using System.IO;
using System.Reflection;
using App.Metrics;
using log4net;
using MetricsReporter.Models;
using Newtonsoft.Json;
using OpenAPI;
using OpenAPI.Plugins;

namespace MetricsReporter
{
    [OpenPluginInfo(Name = "OpenMetrics", Author = "Kenny van Vulpen",
        Description = "Reports server metrics to InfluxDB", Version = "1.0",
        Website = "https://github.com/OpenMiNET/MetricsReporter")]
    public class MetricsPlugin : OpenPlugin
    {
        private static readonly ILog Log = LogManager.GetLogger(typeof(MetricsPlugin));
        
        private PluginConfiguration Config { get; set; }

        private static string ConfigDirectory = Path.Combine(Assembly.GetExecutingAssembly().Location, "OpenMetrics");
        private string ConfigPath =
            Path.Combine(ConfigDirectory, "config.json");

        private bool Ready { get; set; } = false;
        private MetricCollector Collector { get; set; }
        public MetricsPlugin()
        {
            if (!Directory.Exists(ConfigDirectory))
                Directory.CreateDirectory(ConfigDirectory);
        }

        private void LoadConfig()
        {
            if (File.Exists(ConfigPath))
            {
                Config = JsonConvert.DeserializeObject<PluginConfiguration>(File.ReadAllText(ConfigPath));
            }
            else
            {
                Config = new PluginConfiguration();
                File.WriteAllText(ConfigPath, JsonConvert.SerializeObject(Config, Formatting.Indented));
            }
        }

        private void SetupMetricsReporter()
        {
            var metrics = new MetricsBuilder().Report.ToInfluxDb(options =>
                {
                    options.InfluxDb.BaseUri = new Uri(Config.Host);
                    options.InfluxDb.Database = Config.Database;

                    if (!string.IsNullOrWhiteSpace(Config.Username) && !string.IsNullOrWhiteSpace(Config.Password))
                    {
                        options.InfluxDb.UserName = Config.Username;
                        options.InfluxDb.Password = Config.Password;
                    }
                }).Build();

            Collector = new MetricCollector(metrics);
        }
        
        public override void Enabled(OpenApi api)
        {
            LoadConfig();

            if (string.IsNullOrWhiteSpace(Config.Host))
            {
                Log.Warn($"!!! Configuration required, please setup the config file !!!");
                return;
            }
            
            SetupMetricsReporter();

            Collector.Setup(api);
            
            Ready = true;
        }

        public override void Disabled(OpenApi api)
        {
            
        }
    }
}