using EnvoyConfig.Attributes;
using Xunit;

namespace EnvoyConfig.Tests;

using System;
using System.Collections.Generic;

[Collection("NonParallel")]
public class ListModes
{
    public class ListConfig
    {
        [Env(Key = "LIST_COMMA", IsList = true)]
        public List<int> CommaList { get; set; } = [];

        [Env(ListPrefix = "LIST_NUM_")]
        public List<string> NumberedList { get; set; } = [];
    }

    [Fact]
    public void Loads_CommaSeparated_And_NumberedList()
    {
        Environment.SetEnvironmentVariable("LIST_COMMA", "1,2,3");
        Environment.SetEnvironmentVariable("LIST_NUM_1", "foo");
        Environment.SetEnvironmentVariable("LIST_NUM_2", "bar");
        Environment.SetEnvironmentVariable("LIST_NUM_3", null);
        var config = EnvConfig.Load<ListConfig>();
        Assert.Equal([1, 2, 3], config.CommaList);
        Assert.Equal(["foo", "bar"], config.NumberedList);
    }
}
