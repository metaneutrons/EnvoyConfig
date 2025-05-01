using EnvoyConfig.Attributes;
using Xunit;

namespace EnvoyConfig.Tests;

[Collection("NonParallel")]
public class NestedListDefaultIndex
{
    public class ZoneMqttConfigWithDefault
    {
        [Env(Key = "BASETOPIC", Default = "snapdog/zones/{index}")]
        public string BaseTopic { get; set; } = null!;

        [Env(Key = "CONTROL_SET_TOPIC")]
        public string ControlSetTopic { get; set; } = null!;
    }

    public class ZonesConfigWithDefault
    {
        [Env(NestedListPrefix = "ZONE_", NestedListSuffix = "_MQTT_")]
        public List<ZoneMqttConfigWithDefault> ZonesMqtt { get; set; } = new();
    }

    private void ClearZoneVars()
    {
        foreach (System.Collections.DictionaryEntry e in Environment.GetEnvironmentVariables())
        {
            if (e.Key is string k && k.StartsWith("ZONE_") && k.Contains("_MQTT_"))
            {
                Environment.SetEnvironmentVariable(k, null);
            }
        }
    }

    [Fact]
    public void Uses_Index_In_Default()
    {
        ClearZoneVars();
        try
        {
            // Only set CONTROL_SET_TOPIC, not BASETOPIC
            Environment.SetEnvironmentVariable("ZONE_1_MQTT_CONTROL_SET_TOPIC", "c1");
            Environment.SetEnvironmentVariable("ZONE_2_MQTT_CONTROL_SET_TOPIC", "c2");
            var config = EnvConfig.Load<ZonesConfigWithDefault>();
            Assert.Equal(2, config.ZonesMqtt.Count);
            // Debug output
            Console.WriteLine($"ZONE_1_MQTT_BASETOPIC: {Environment.GetEnvironmentVariable("ZONE_1_MQTT_BASETOPIC")}");
            Console.WriteLine($"ZONE_2_MQTT_BASETOPIC: {Environment.GetEnvironmentVariable("ZONE_2_MQTT_BASETOPIC")}");
            Console.WriteLine(
                $"ZONE_1_MQTT_CONTROL_SET_TOPIC: {Environment.GetEnvironmentVariable("ZONE_1_MQTT_CONTROL_SET_TOPIC")}"
            );
            Console.WriteLine(
                $"ZONE_2_MQTT_CONTROL_SET_TOPIC: {Environment.GetEnvironmentVariable("ZONE_2_MQTT_CONTROL_SET_TOPIC")}"
            );
            Console.WriteLine($"config.ZonesMqtt[0].BaseTopic: {config.ZonesMqtt[0].BaseTopic}");
            Console.WriteLine($"config.ZonesMqtt[1].BaseTopic: {config.ZonesMqtt[1].BaseTopic}");
            Console.WriteLine($"config.ZonesMqtt[0].ControlSetTopic: {config.ZonesMqtt[0].ControlSetTopic}");
            Console.WriteLine($"config.ZonesMqtt[1].ControlSetTopic: {config.ZonesMqtt[1].ControlSetTopic}");

            Assert.Equal("snapdog/zones/1", config.ZonesMqtt[0].BaseTopic);
            Assert.Equal("snapdog/zones/2", config.ZonesMqtt[1].BaseTopic);
            Assert.Equal("c1", config.ZonesMqtt[0].ControlSetTopic);
            Assert.Equal("c2", config.ZonesMqtt[1].ControlSetTopic);
        }
        finally
        {
            ClearZoneVars();
        }
    }
}
