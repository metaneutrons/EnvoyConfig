using EnvoyConfig.Attributes;
using Xunit;

namespace EnvoyConfig.Tests;

[Collection("NonParallel")]
public class NestedList
{
    public class ZoneMqttConfig
    {
        [Env(Key = "CONTROL_SET_TOPIC")]
        public string ControlSetTopic { get; set; } = null!;

        [Env(Key = "TRACK_SET_TOPIC")]
        public string TrackSetTopic { get; set; } = null!;

        [Env(Key = "PLAYLIST_SET_TOPIC")]
        public string PlaylistSetTopic { get; set; } = null!;

        [Env(Key = "VOLUME_SET_TOPIC")]
        public string VolumeSetTopic { get; set; } = null!;

        [Env(Key = "MUTE_SET_TOPIC")]
        public string MuteSetTopic { get; set; } = null!;

        [Env(Key = "CONTROL_TOPIC")]
        public string ControlTopic { get; set; } = null!;

        [Env(Key = "TRACK_TOPIC")]
        public string TrackTopic { get; set; } = null!;

        [Env(Key = "PLAYLIST_TOPIC")]
        public string PlaylistTopic { get; set; } = null!;

        [Env(Key = "VOLUME_TOPIC")]
        public string VolumeTopic { get; set; } = null!;

        [Env(Key = "MUTE_TOPIC")]
        public string MuteTopic { get; set; } = null!;

        [Env(Key = "STATE_TOPIC")]
        public string StateTopic { get; set; } = null!;
    }

    public class ZonesConfig
    {
        [Env(NestedListPrefix = "ZONE_", NestedListSuffix = "_MQTT_")]
        public List<ZoneMqttConfig> ZonesMqtt { get; set; } = new();
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
    public void Loads_NestedList_From_Env()
    {
        ClearZoneVars();
        try
        {
            Environment.SetEnvironmentVariable("ZONE_1_MQTT_CONTROL_SET_TOPIC", "control/set");
            Environment.SetEnvironmentVariable("ZONE_1_MQTT_TRACK_SET_TOPIC", "track/set");
            Environment.SetEnvironmentVariable("ZONE_2_MQTT_CONTROL_SET_TOPIC", "control/set2");
            Environment.SetEnvironmentVariable("ZONE_2_MQTT_TRACK_SET_TOPIC", "track/set2");
            var config = EnvConfig.Load<ZonesConfig>();
            Assert.Equal(2, config.ZonesMqtt.Count);
            // Debug output
            Console.WriteLine(
                $"ZONE_1_MQTT_CONTROL_SET_TOPIC: {Environment.GetEnvironmentVariable("ZONE_1_MQTT_CONTROL_SET_TOPIC")}"
            );
            Console.WriteLine(
                $"ZONE_1_MQTT_TRACK_SET_TOPIC: {Environment.GetEnvironmentVariable("ZONE_1_MQTT_TRACK_SET_TOPIC")}"
            );
            Console.WriteLine(
                $"ZONE_2_MQTT_CONTROL_SET_TOPIC: {Environment.GetEnvironmentVariable("ZONE_2_MQTT_CONTROL_SET_TOPIC")}"
            );
            Console.WriteLine(
                $"ZONE_2_MQTT_TRACK_SET_TOPIC: {Environment.GetEnvironmentVariable("ZONE_2_MQTT_TRACK_SET_TOPIC")}"
            );
            Console.WriteLine($"config.ZonesMqtt[0].ControlSetTopic: {config.ZonesMqtt[0].ControlSetTopic}");
            Console.WriteLine($"config.ZonesMqtt[0].TrackSetTopic: {config.ZonesMqtt[0].TrackSetTopic}");
            Console.WriteLine($"config.ZonesMqtt[1].ControlSetTopic: {config.ZonesMqtt[1].ControlSetTopic}");
            Console.WriteLine($"config.ZonesMqtt[1].TrackSetTopic: {config.ZonesMqtt[1].TrackSetTopic}");

            Assert.Equal("control/set", config.ZonesMqtt[0].ControlSetTopic);
            Assert.Equal("track/set", config.ZonesMqtt[0].TrackSetTopic);
            Assert.Equal("control/set2", config.ZonesMqtt[1].ControlSetTopic);
            Assert.Equal("track/set2", config.ZonesMqtt[1].TrackSetTopic);
        }
        finally
        {
            ClearZoneVars();
        }
    }
}
