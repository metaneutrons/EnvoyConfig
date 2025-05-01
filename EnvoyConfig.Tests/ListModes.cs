using System;
using System.Collections.Generic;
using EnvoyConfig;
using EnvoyConfig.Attributes;
using Xunit;

namespace EnvoyConfig.Tests;

public class ListModes
{
    public class ListConfig
    {
        [Env(Key = "LIST_COMMA", IsList = true)]
        public List<int> CommaList { get; set; } = new();

        [Env(ListPrefix = "LIST_NUM_")]
        public List<string> NumberedList { get; set; } = new();
    }

    [Fact]
    public void Loads_CommaSeparated_And_NumberedList()
    {
        Environment.SetEnvironmentVariable("LIST_COMMA", "1,2,3");
        Environment.SetEnvironmentVariable("LIST_NUM_1", "foo");
        Environment.SetEnvironmentVariable("LIST_NUM_2", "bar");
        Environment.SetEnvironmentVariable("LIST_NUM_3", null);
        var config = EnvConfig.Load<ListConfig>();
        Assert.Equal(new List<int> { 1, 2, 3 }, config.CommaList);
        Assert.Equal(new List<string> { "foo", "bar" }, config.NumberedList);
    }
}
