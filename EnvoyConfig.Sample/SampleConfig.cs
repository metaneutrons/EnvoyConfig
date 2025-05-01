using EnvoyConfig.Attributes;

namespace EnvoyConfig.Sample;

public class SampleConfig
{
    [Env(Key = "ENVIRONMENT", Default = "Development")]
    public string Env { get; set; } = null!;

    [Env(Key = "LOG_LEVEL", Default = "Information")]
    public string LogLevel { get; set; } = null!;

    [Env(Key = "TELEMETRY_ENABLED", Default = "true")]
    public bool TelemetryEnabled { get; set; }

    [Env(Key = "TELEMETRY_SERVICE_NAME", Default = "sample")]
    public string TelemetryServiceName { get; set; } = null!;

    [Env(Key = "TELEMETRY_SAMPLING_RATE")]
    public double TelemetrySamplingRate { get; set; }

    [Env(Key = "PROMETHEUS_ENABLED")]
    public bool PrometheusEnabled { get; set; }

    [Env(Key = "PROMETHEUS_PATH", Default = "/metrics")]
    public string PrometheusPath { get; set; } = null!;

    [Env(Key = "PROMETHEUS_PORT")]
    public int PrometheusPort { get; set; }

    [Env(Key = "JAEGER_ENABLED")]
    public bool JaegerEnabled { get; set; }

    [Env(Key = "JAEGER_ENDPOINT", Default = "http://jaeger:14268")]
    public string JaegerEndpoint { get; set; } = null!;

    [Env(Key = "JAEGER_AGENT_HOST", Default = "jaeger")]
    public string JaegerAgentHost { get; set; } = null!;

    [Env(Key = "JAEGER_AGENT_PORT")]
    public int JaegerAgentPort { get; set; }

    [Env(Key = "API_AUTH_ENABLED")]
    public bool ApiAuthEnabled { get; set; }

    [Env(ListPrefix = "API_APIKEY_")]
    public List<string> ApiKeys { get; set; } = new();

    [Env(Key = "ZONES", IsList = true)]
    public List<string> Zones { get; set; } = new();

    // Snapcast configuration as a map
    [Env(MapPrefix = "SNAPCAST_")]
    public Dictionary<string, string> Snapcast { get; set; } = new();

    // List of MQTT zone configs
    [Env(NestedListPrefix = "ZONE_", NestedListSuffix = "_MQTT_")]
    public List<SampleZoneMqttConfig> ZonesMqtt { get; set; } = new();
}
