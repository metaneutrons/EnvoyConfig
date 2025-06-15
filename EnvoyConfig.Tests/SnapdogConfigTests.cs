using System;
using System.Collections.Generic;
using EnvoyConfig.Conversion;
using EnvoyConfig.Sample;
using Xunit;

namespace EnvoyConfig.Tests
{
    /// <summary>
    /// Tests for the SNAPDOG client configuration system.
    /// </summary>
    [Collection("NonParallel")]
    public class SnapdogConfigTests
    {
        public SnapdogConfigTests()
        {
            // Register the KnxAddress converter for tests
            TypeConverterRegistry.RegisterConverter(typeof(KnxAddress), new KnxAddressConverter());
            TypeConverterRegistry.RegisterConverter(typeof(KnxAddress?), new KnxAddressConverter());

            // Clean up any existing SNAPDOG environment variables
            ClearSnapdogEnvironmentVariables();
        }

        [Fact]
        public void Load_SingleClient_WithAllProperties()
        {
            // Arrange
            SetEnvironmentVariable("SNAPDOG_CLIENT_1_NAME", "Living Room");
            SetEnvironmentVariable("SNAPDOG_CLIENT_1_MAC", "02:42:ac:11:00:10");
            SetEnvironmentVariable("SNAPDOG_CLIENT_1_MQTT_BASETOPIC", "snapdog/clients/livingroom");
            SetEnvironmentVariable("SNAPDOG_CLIENT_1_DEFAULT_ZONE", "1");

            // MQTT topics
            SetEnvironmentVariable("SNAPDOG_CLIENT_1_MQTT_VOLUME_SET_TOPIC", "volume/set");
            SetEnvironmentVariable("SNAPDOG_CLIENT_1_MQTT_MUTE_SET_TOPIC", "mute/set");
            SetEnvironmentVariable("SNAPDOG_CLIENT_1_MQTT_CONTROL_TOPIC", "control");
            SetEnvironmentVariable("SNAPDOG_CLIENT_1_MQTT_STATE_TOPIC", "state");

            // KNX configuration
            SetEnvironmentVariable("SNAPDOG_CLIENT_1_KNX_ENABLED", "true");
            SetEnvironmentVariable("SNAPDOG_CLIENT_1_KNX_VOLUME", "2/1/1");
            SetEnvironmentVariable("SNAPDOG_CLIENT_1_KNX_MUTE", "2/1/5");

            try
            {
                // Act
                var config = EnvConfig.Load<SnapdogConfig>();

                // Assert
                Assert.Single(config.Clients);

                var client = config.Clients[0];
                Assert.Equal("Living Room", client.Name);
                Assert.Equal("02:42:ac:11:00:10", client.Mac);
                Assert.Equal("snapdog/clients/livingroom", client.MqttBaseTopic);
                Assert.Equal(1, client.DefaultZone);

                // MQTT configuration
                Assert.Equal("volume/set", client.Mqtt.VolumeSetTopic);
                Assert.Equal("mute/set", client.Mqtt.MuteSetTopic);
                Assert.Equal("control", client.Mqtt.ControlTopic);
                Assert.Equal("state", client.Mqtt.StateTopic);

                // KNX configuration
                Assert.True(client.Knx.Enabled);
                Assert.NotNull(client.Knx.Volume);
                Assert.Equal("2/1/1", client.Knx.Volume.ToString());
                Assert.NotNull(client.Knx.Mute);
                Assert.Equal("2/1/5", client.Knx.Mute.ToString());
            }
            finally
            {
                ClearSnapdogEnvironmentVariables();
            }
        }

        [Fact]
        public void Load_MultipleClients_WithDifferentConfigurations()
        {
            // Arrange
            // Client 1 - Full configuration
            SetEnvironmentVariable("SNAPDOG_CLIENT_1_NAME", "Living Room");
            SetEnvironmentVariable("SNAPDOG_CLIENT_1_MAC", "02:42:ac:11:00:10");
            SetEnvironmentVariable("SNAPDOG_CLIENT_1_MQTT_BASETOPIC", "snapdog/clients/livingroom");
            SetEnvironmentVariable("SNAPDOG_CLIENT_1_DEFAULT_ZONE", "1");
            SetEnvironmentVariable("SNAPDOG_CLIENT_1_MQTT_VOLUME_SET_TOPIC", "volume/set");
            SetEnvironmentVariable("SNAPDOG_CLIENT_1_KNX_ENABLED", "true");
            SetEnvironmentVariable("SNAPDOG_CLIENT_1_KNX_VOLUME", "2/1/1");

            // Client 2 - Minimal configuration
            SetEnvironmentVariable("SNAPDOG_CLIENT_2_NAME", "Kitchen");
            SetEnvironmentVariable("SNAPDOG_CLIENT_2_MAC", "02:42:ac:11:00:20");
            SetEnvironmentVariable("SNAPDOG_CLIENT_2_MQTT_BASETOPIC", "snapdog/clients/kitchen");
            SetEnvironmentVariable("SNAPDOG_CLIENT_2_MQTT_CONTROL_TOPIC", "control");
            SetEnvironmentVariable("SNAPDOG_CLIENT_2_KNX_ENABLED", "false");

            try
            {
                // Act
                var config = EnvConfig.Load<SnapdogConfig>();

                // Assert
                Assert.Equal(2, config.Clients.Count);

                // Client 1
                var client1 = config.Clients[0];
                Assert.Equal("Living Room", client1.Name);
                Assert.Equal("02:42:ac:11:00:10", client1.Mac);
                Assert.Equal(1, client1.DefaultZone);
                Assert.True(client1.Knx.Enabled);
                Assert.NotNull(client1.Knx.Volume);

                // Client 2
                var client2 = config.Clients[1];
                Assert.Equal("Kitchen", client2.Name);
                Assert.Equal("02:42:ac:11:00:20", client2.Mac);
                Assert.Equal(1, client2.DefaultZone); // Default value
                Assert.False(client2.Knx.Enabled);
                Assert.Equal("control", client2.Mqtt.ControlTopic);
            }
            finally
            {
                ClearSnapdogEnvironmentVariables();
            }
        }

        [Fact]
        public void Load_KnxAddress_ParsesCorrectly()
        {
            // Arrange
            SetEnvironmentVariable("SNAPDOG_CLIENT_1_NAME", "Test Client");
            SetEnvironmentVariable("SNAPDOG_CLIENT_1_MAC", "02:42:ac:11:00:10");
            SetEnvironmentVariable("SNAPDOG_CLIENT_1_MQTT_BASETOPIC", "test/topic");
            SetEnvironmentVariable("SNAPDOG_CLIENT_1_KNX_ENABLED", "true");
            SetEnvironmentVariable("SNAPDOG_CLIENT_1_KNX_VOLUME", "15/7/255");
            SetEnvironmentVariable("SNAPDOG_CLIENT_1_KNX_MUTE", "0/0/1");

            try
            {
                // Act
                var config = EnvConfig.Load<SnapdogConfig>();

                // Assert
                var client = config.Clients[0];
                Assert.NotNull(client.Knx.Volume);
                Assert.Equal(15, client.Knx.Volume.Value.Main);
                Assert.Equal(7, client.Knx.Volume.Value.Middle);
                Assert.Equal(255, client.Knx.Volume.Value.Sub);

                Assert.NotNull(client.Knx.Mute);
                Assert.Equal(0, client.Knx.Mute.Value.Main);
                Assert.Equal(0, client.Knx.Mute.Value.Middle);
                Assert.Equal(1, client.Knx.Mute.Value.Sub);
            }
            finally
            {
                ClearSnapdogEnvironmentVariables();
            }
        }

        [Fact]
        public void Load_InvalidKnxAddress_HandledGracefully()
        {
            // Arrange
            SetEnvironmentVariable("SNAPDOG_CLIENT_1_NAME", "Test Client");
            SetEnvironmentVariable("SNAPDOG_CLIENT_1_MAC", "02:42:ac:11:00:10");
            SetEnvironmentVariable("SNAPDOG_CLIENT_1_MQTT_BASETOPIC", "test/topic");
            SetEnvironmentVariable("SNAPDOG_CLIENT_1_KNX_ENABLED", "true");
            SetEnvironmentVariable("SNAPDOG_CLIENT_1_KNX_VOLUME", "invalid/address/format");

            try
            {
                // Act
                var config = EnvConfig.Load<SnapdogConfig>();

                // Assert
                var client = config.Clients[0];
                Assert.Null(client.Knx.Volume); // Invalid address should result in null
            }
            finally
            {
                ClearSnapdogEnvironmentVariables();
            }
        }

        [Fact]
        public void Load_EmptyConfiguration_ReturnsEmptyClientList()
        {
            // Act
            var config = EnvConfig.Load<SnapdogConfig>();

            // Assert
            Assert.Empty(config.Clients);
        }

        private static void SetEnvironmentVariable(string name, string value)
        {
            Environment.SetEnvironmentVariable(name, value);
        }

        [Fact]
        public void Load_SingleRadioStation_WithAllProperties()
        {
            // Arrange
            SetEnvironmentVariable("SNAPDOG_RADIO_1_NAME", "DLF Kultur");
            SetEnvironmentVariable("SNAPDOG_RADIO_1_URL", "https://st02.sslstream.dlf.de/dlf/02/high/aac/stream.aac");

            try
            {
                // Act
                var config = EnvConfig.Load<SnapdogConfig>();

                // Assert
                Assert.Single(config.RadioStations);

                var radioStation = config.RadioStations[0];
                Assert.Equal("DLF Kultur", radioStation.Name);
                Assert.Equal("https://st02.sslstream.dlf.de/dlf/02/high/aac/stream.aac", radioStation.URL);
            }
            finally
            {
                ClearSnapdogEnvironmentVariables();
            }
        }

        [Fact]
        public void Load_MultipleRadioStations_WithDifferentConfigurations()
        {
            // Arrange
            // Radio Station 1
            SetEnvironmentVariable("SNAPDOG_RADIO_1_NAME", "DLF Kultur");
            SetEnvironmentVariable("SNAPDOG_RADIO_1_URL", "https://st02.sslstream.dlf.de/dlf/02/high/aac/stream.aac");

            // Radio Station 2
            SetEnvironmentVariable("SNAPDOG_RADIO_2_NAME", "MDR Kultur");
            SetEnvironmentVariable("SNAPDOG_RADIO_2_URL", "http://avw.mdr.de/streams/284310-0_aac_high.m3u");

            // Radio Station 3
            SetEnvironmentVariable("SNAPDOG_RADIO_3_NAME", "WDR 3");
            SetEnvironmentVariable(
                "SNAPDOG_RADIO_3_URL",
                "https://wdr-wdr3-live.icecastssl.wdr.de/wdr/wdr3/live/mp3/128/stream.mp3"
            );

            try
            {
                // Act
                var config = EnvConfig.Load<SnapdogConfig>();

                // Assert
                Assert.Equal(3, config.RadioStations.Count);

                // Radio Station 1
                var radio1 = config.RadioStations[0];
                Assert.Equal("DLF Kultur", radio1.Name);
                Assert.Equal("https://st02.sslstream.dlf.de/dlf/02/high/aac/stream.aac", radio1.URL);

                // Radio Station 2
                var radio2 = config.RadioStations[1];
                Assert.Equal("MDR Kultur", radio2.Name);
                Assert.Equal("http://avw.mdr.de/streams/284310-0_aac_high.m3u", radio2.URL);

                // Radio Station 3
                var radio3 = config.RadioStations[2];
                Assert.Equal("WDR 3", radio3.Name);
                Assert.Equal("https://wdr-wdr3-live.icecastssl.wdr.de/wdr/wdr3/live/mp3/128/stream.mp3", radio3.URL);
            }
            finally
            {
                ClearSnapdogEnvironmentVariables();
            }
        }

        [Fact]
        public void Load_EmptyRadioConfiguration_ReturnsEmptyRadioList()
        {
            // Act
            var config = EnvConfig.Load<SnapdogConfig>();

            // Assert
            Assert.Empty(config.RadioStations);
        }

        [Fact]
        public void Load_PartialRadioConfiguration_HandledGracefully()
        {
            // Arrange - Only set NAME for radio station 1, missing URL
            SetEnvironmentVariable("SNAPDOG_RADIO_1_NAME", "Incomplete Station");

            try
            {
                // Act
                var config = EnvConfig.Load<SnapdogConfig>();

                // Assert
                Assert.Single(config.RadioStations);
                var radioStation = config.RadioStations[0];
                Assert.Equal("Incomplete Station", radioStation.Name);
                // URL should be null or empty since it wasn't provided
                Assert.True(string.IsNullOrEmpty(radioStation.URL) || radioStation.URL == null!);
            }
            finally
            {
                ClearSnapdogEnvironmentVariables();
            }
        }

        [Fact]
        public void Load_ClientsAndRadioStations_BothLoadCorrectly()
        {
            // Arrange
            // Client configuration
            SetEnvironmentVariable("SNAPDOG_CLIENT_1_NAME", "Living Room");
            SetEnvironmentVariable("SNAPDOG_CLIENT_1_MAC", "02:42:ac:11:00:10");
            SetEnvironmentVariable("SNAPDOG_CLIENT_1_MQTT_BASETOPIC", "snapdog/clients/livingroom");

            // Radio station configuration
            SetEnvironmentVariable("SNAPDOG_RADIO_1_NAME", "DLF Kultur");
            SetEnvironmentVariable("SNAPDOG_RADIO_1_URL", "https://st02.sslstream.dlf.de/dlf/02/high/aac/stream.aac");

            try
            {
                // Act
                var config = EnvConfig.Load<SnapdogConfig>();

                // Assert
                Assert.Single(config.Clients);
                Assert.Single(config.RadioStations);

                var client = config.Clients[0];
                Assert.Equal("Living Room", client.Name);
                Assert.Equal("02:42:ac:11:00:10", client.Mac);

                var radioStation = config.RadioStations[0];
                Assert.Equal("DLF Kultur", radioStation.Name);
                Assert.Equal("https://st02.sslstream.dlf.de/dlf/02/high/aac/stream.aac", radioStation.URL);
            }
            finally
            {
                ClearSnapdogEnvironmentVariables();
            }
        }

        private static void ClearSnapdogEnvironmentVariables()
        {
            foreach (System.Collections.DictionaryEntry entry in Environment.GetEnvironmentVariables())
            {
                if (entry.Key is string key && (key.StartsWith("SNAPDOG_CLIENT_") || key.StartsWith("SNAPDOG_RADIO_")))
                {
                    Environment.SetEnvironmentVariable(key, null);
                }
            }
        }
    }
}
