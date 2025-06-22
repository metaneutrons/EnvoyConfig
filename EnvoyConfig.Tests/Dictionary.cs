using EnvoyConfig.Attributes;
using Xunit;

namespace EnvoyConfig.Tests;

using System;

[Collection("EnvoyConfig")]
public class Dictionary
{
    public class DictConfig
    {
        [Env(MapPrefix = "MAP_")]
        public Dictionary<string, int> Map { get; set; } = [];
    }

    public class DictConfigUpper
    {
        [Env(MapPrefix = "MAP_", MapKeyCasing = MapKeyCasingMode.Upper)]
        public Dictionary<string, int> Map { get; set; } = [];
    }

    public class DictConfigAsIs
    {
        [Env(MapPrefix = "MAP_", MapKeyCasing = MapKeyCasingMode.AsIs)]
        public Dictionary<string, int> Map { get; set; } = [];
    }

    private void ClearMapVars()
    {
        foreach (System.Collections.DictionaryEntry e in Environment.GetEnvironmentVariables())
        {
            if (e.Key is string k && k.StartsWith("MAP_"))
            {
                Environment.SetEnvironmentVariable(k, null);
            }
        }
    }

    [Fact]
    public void Loads_Dictionary_From_MapPrefix()
    {
        try
        {
            this.ClearMapVars();
            Environment.SetEnvironmentVariable("MAP_foo", "10");
            Environment.SetEnvironmentVariable("MAP_bar", "20");
            var config = EnvConfig.Load<DictConfig>();
            Assert.Equal(2, config.Map.Count);
            Assert.True(config.Map.ContainsKey("foo"));
            Assert.True(config.Map.ContainsKey("bar"));
            Assert.Equal(10, config.Map["foo"]);
            Assert.Equal(20, config.Map["bar"]);
        }
        finally
        {
            // Ensure cleanup even if test fails
            Environment.SetEnvironmentVariable("MAP_foo", null);
            Environment.SetEnvironmentVariable("MAP_bar", null);
            this.ClearMapVars();
        }
    }

    [Fact]
    public void Loads_Dictionary_From_MapPrefix_UpperCase()
    {
        try
        {
            this.ClearMapVars();
            Environment.SetEnvironmentVariable("MAP_Foo", "1");
            Environment.SetEnvironmentVariable("MAP_Bar", "2");
            var config = EnvConfig.Load<DictConfigUpper>();
            Assert.Equal(2, config.Map.Count);
            Assert.True(config.Map.ContainsKey("FOO"));
            Assert.True(config.Map.ContainsKey("BAR"));
            Assert.Equal(1, config.Map["FOO"]);
            Assert.Equal(2, config.Map["BAR"]);
        }
        finally
        {
            // Ensure cleanup even if test fails
            Environment.SetEnvironmentVariable("MAP_Foo", null);
            Environment.SetEnvironmentVariable("MAP_Bar", null);
            this.ClearMapVars();
        }
    }

    [Fact]
    public void Loads_Dictionary_From_MapPrefix_AsIs()
    {
        try
        {
            this.ClearMapVars();
            Environment.SetEnvironmentVariable("MAP_Foo", "100");
            Environment.SetEnvironmentVariable("MAP_Bar", "200");
            var config = EnvConfig.Load<DictConfigAsIs>();
            Assert.Equal(2, config.Map.Count);
            Assert.True(config.Map.ContainsKey("Foo"));
            Assert.True(config.Map.ContainsKey("Bar"));
            Assert.Equal(100, config.Map["Foo"]);
            Assert.Equal(200, config.Map["Bar"]);
        }
        finally
        {
            // Ensure cleanup even if test fails
            Environment.SetEnvironmentVariable("MAP_Foo", null);
            Environment.SetEnvironmentVariable("MAP_Bar", null);
            this.ClearMapVars();
        }
    }
}
