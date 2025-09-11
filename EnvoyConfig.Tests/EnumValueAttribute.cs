using EnvoyConfig.Attributes;
using Xunit;

namespace EnvoyConfig.Tests;

public class EnumValueAttributeTests
{
    public enum TranscodingFormat
    {
        [EnvValue("off", "disabled", "none")]
        Disabled,
        
        [EnvValue("mp3")]
        Mp3,
        
        [EnvValue("ogg", "vorbis")]
        Ogg,
        
        Flac // No attribute - should use standard parsing
    }

    public class TestConfig
    {
        [Env(Key = "TRANSCODING_FORMAT")]
        public TranscodingFormat Format { get; set; }
    }

    [Fact]
    public void EnvValueAttribute_ParsesCorrectly()
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
    public void EnvValueAttribute_SerializesCorrectly()
    {
        var config = new TestConfig { Format = TranscodingFormat.Disabled };
        var variables = EnvConfig.GetEnvironmentVariables(config);
        
        var formatVar = variables.FirstOrDefault(v => v.Key == "TRANSCODING_FORMAT");
        Assert.NotNull(formatVar);
        Assert.Equal("off", formatVar.Value); // Should use first EnvValue
    }
}
