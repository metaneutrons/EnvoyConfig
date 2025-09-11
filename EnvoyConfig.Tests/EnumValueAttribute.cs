using EnvoyConfig.Attributes;
using Xunit;

namespace EnvoyConfig.Tests;

public class EnumValueAttributeTests
{
    public enum TranscodingFormat
    {
        [EnvoyConfigValue("off", "disabled", "none")]
        Disabled,
        
        [EnvoyConfigValue("mp3")]
        Mp3,
        
        [EnvoyConfigValue("ogg", "vorbis")]
        Ogg,
        
        Flac // No attribute - should use standard parsing
    }

    public class TestConfig
    {
        [EnvoyConfig(Key = "TRANSCODING_FORMAT")]
        public TranscodingFormat Format { get; set; }
    }

    [Fact]
    public void EnvoyConfigValue_ParsesCorrectly()
    {
        Environment.SetEnvironmentVariable("TRANSCODING_FORMAT", "disabled");
        var config = EnvConfig.Load<TestConfig>();
        Assert.Equal(TranscodingFormat.Disabled, config.Format);

        Environment.SetEnvironmentVariable("TRANSCODING_FORMAT", "mp3");
        config = EnvConfig.Load<TestConfig>();
        Assert.Equal(TranscodingFormat.Mp3, config.Format);

        Environment.SetEnvironmentVariable("TRANSCODING_FORMAT", "vorbis");
        config = EnvConfig.Load<TestConfig>();
        Assert.Equal(TranscodingFormat.Ogg, config.Format);

        // Standard enum parsing still works
        Environment.SetEnvironmentVariable("TRANSCODING_FORMAT", "Flac");
        config = EnvConfig.Load<TestConfig>();
        Assert.Equal(TranscodingFormat.Flac, config.Format);
        
        // Clean up
        Environment.SetEnvironmentVariable("TRANSCODING_FORMAT", null);
    }

    [Fact]
    public void EnvoyConfigValue_SerializesCorrectly()
    {
        var config = new TestConfig { Format = TranscodingFormat.Disabled };
        var variables = EnvConfig.GetEnvironmentVariables(config);
        
        var formatVar = variables.FirstOrDefault(v => v.Key == "TRANSCODING_FORMAT");
        Assert.NotNull(formatVar);
        Assert.Equal("off", formatVar.Value); // Should use first EnvoyConfigValue
    }

    // Test backward compatibility with old attributes
    public enum LegacyFormat
    {
        [EnvValue("legacy1", "legacy2")]
        Legacy,
        
        Standard
    }

    public class LegacyTestConfig
    {
        [Env(Key = "LEGACY_FORMAT")]
        public LegacyFormat Format { get; set; }
    }

    [Fact]
    public void LegacyAttributes_StillWork()
    {
        Environment.SetEnvironmentVariable("LEGACY_FORMAT", "legacy1");
        var config = EnvConfig.Load<LegacyTestConfig>();
        Assert.Equal(LegacyFormat.Legacy, config.Format);
        
        // Clean up
        Environment.SetEnvironmentVariable("LEGACY_FORMAT", null);
    }
}
