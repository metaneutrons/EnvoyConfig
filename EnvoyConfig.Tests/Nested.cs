using System;
using EnvoyConfig;
using EnvoyConfig.Attributes;
using Xunit;

namespace EnvoyConfig.Tests;

public class Nested
{
    public class Child
    {
        [Env(Key = "CHILD_VAL")]
        public int Value { get; set; }
    }

    public class Parent
    {
        [Env(NestedPrefix = "PARENT_")]
        public Child? Child { get; set; }
    }

    [Fact]
    public void Loads_Nested_Object_With_Prefix()
    {
        Environment.SetEnvironmentVariable("PARENT_CHILD_VAL", "99");
        try
        {
            var config = EnvConfig.Load<Parent>();
            Assert.NotNull(config.Child);
            Assert.Equal(99, config.Child!.Value);
        }
        finally
        {
            Environment.SetEnvironmentVariable("PARENT_CHILD_VAL", null);
        }
    }
}
