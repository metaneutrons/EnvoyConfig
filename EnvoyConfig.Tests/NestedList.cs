using System;
using System.Collections.Generic;
using EnvoyConfig;
using EnvoyConfig.Attributes;
using Xunit;

namespace EnvoyConfig.Tests;

public class NestedList
{
    public class ZoneMqttConfig
    {
        [Env(Key = "CONTROL_SET_TOPIC")] public string ControlSetTopic { get; set; }
        [Env(Key = "TRACK_SET_TOPIC")] public string TrackSetTopic { get; set; }
        [Env(Key = "PLAYLIST_SET_TOPIC")] public string PlaylistSetTopic { get; set; }
        [Env(Key = "VOLUME_SET_TOPIC")] public string VolumeSetTopic { get; set; }
        [Env(Key = "MUTE_SET_TOPIC")] public string MuteSetTopic { get; set; }
        [Env(Key = "CONTROL_TOPIC")] public string ControlTopic { get; set; }
        [Env(Key = "TRACK_TOPIC")] public string TrackTopic { get; set; }
        [Env(Key = "PLAYLIST_TOPIC")] public string PlaylistTopic { get; set; }
        [Env(Key = "VOLUME_TOPIC")] public string VolumeTopic { get; set; }
        [Env(Key = "MUTE_TOPIC")] public string MuteTopic { get; set; }
        [Env(Key = "STATE_TOPIC")] public string StateTopic { get; set; }
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
                Environment.SetEnvironmentVariable(k, null);
        }
    }

    [Fact]
    public void Loads_NestedList_From_Env()
    {
        ClearZoneVars();
        Environment.SetEnvironmentVariable("ZONE_1_MQTT_CONTROL_SET_TOPIC", "control/set");
        Environment.SetEnvironmentVariable("ZONE_1_MQTT_TRACK_SET_TOPIC", "track/set");
        Environment.SetEnvironmentVariable("ZONE_2_MQTT_CONTROL_SET_TOPIC", "control/set2");
        Environment.SetEnvironmentVariable("ZONE_2_MQTT_TRACK_SET_TOPIC", "track/set2");
        var config = EnvConfig.Load<ZonesConfig>();
        Assert.Equal(2, config.ZonesMqtt.Count);
        Assert.Equal("control/set", config.ZonesMqtt[0].ControlSetTopic);
        Assert.Equal("track/set", config.ZonesMqtt[0].TrackSetTopic);
        Assert.Equal("control/set2", config.ZonesMqtt[1].ControlSetTopic);
        Assert.Equal("track/set2", config.ZonesMqtt[1].TrackSetTopic);
    }
}
