using System.Collections.Generic;
using EnvoyConfig.Attributes;

namespace EnvoyConfig.Sample
{
    /// <summary>
    /// Main configuration class for SNAPDOG clients.
    /// Maps environment variables like MYAPP_CLIENT_X_* to a list of client configurations.
    /// This replaces SampleConfig as the primary configuration class for the SNAPDOG system.
    /// </summary>
    public class SnapdogConfig
    {
        /// <summary>
        /// Gets or sets the list of SNAPDOG client configurations.
        /// Maps environment variables with pattern: MYAPP_CLIENT_X_*
        /// Where X is the client index (1, 2, 3, etc.)
        ///
        /// Example mappings:
        /// - MYAPP_CLIENT_1_NAME → Clients[0].Name
        /// - MYAPP_CLIENT_1_MAC → Clients[0].Mac
        /// - MYAPP_CLIENT_1_MQTT_BASETOPIC → Clients[0].MqttBaseTopic
        /// - MYAPP_CLIENT_1_MQTT_VOLUME_SET_TOPIC → Clients[0].Mqtt.VolumeSetTopic
        /// - MYAPP_CLIENT_1_KNX_ENABLED → Clients[0].Knx.Enabled
        /// - MYAPP_CLIENT_1_KNX_VOLUME → Clients[0].Knx.Volume
        /// </summary>
        [Env(NestedListPrefix = "MYAPP_CLIENT_", NestedListSuffix = "_")]
        public List<ClientConfig> Clients { get; set; } = [];

        /// <summary>
        /// Gets or sets the list of SNAPDOG radio station configurations.
        /// Maps environment variables with pattern: MYAPP_RADIO_X_*
        /// Where X is the radio station index (1, 2, 3, etc.)
        ///
        /// Example mappings:
        /// - MYAPP_RADIO_1_NAME → RadioStations[0].Name
        /// - MYAPP_RADIO_1_URL → RadioStations[0].URL
        /// - MYAPP_RADIO_2_NAME → RadioStations[1].Name
        /// - MYAPP_RADIO_2_URL → RadioStations[1].URL
        /// </summary>
        [Env(NestedListPrefix = "MYAPP_RADIO_", NestedListSuffix = "_")]
        public List<RadioStation> RadioStations { get; set; } = [];
    }
}
