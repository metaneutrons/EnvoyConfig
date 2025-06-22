using EnvoyConfig.Attributes;
using EnvoyConfig.Internal;
using Xunit;

namespace EnvoyConfig.Tests
{
    using System;

    public class DefaultValue
    {
        private class BoolConfig
        {
            [Env(Key = "BOOL_TRUE_STR", Default = "true")]
            public bool BoolTrueStr { get; set; }

            [Env(Key = "BOOL_TRUE_BOOL", Default = true)]
            public bool BoolTrueBool { get; set; }
        }

        private class IntConfig
        {
            [Env(Key = "INT_STR", Default = "42")]
            public int IntStr { get; set; }

            [Env(Key = "INT_INT", Default = 42)]
            public int IntInt { get; set; }
        }

        [Fact]
        public void Bool_Default_String_And_Bool_Both_Work()
        {
            // Ensure env is not set
            Environment.SetEnvironmentVariable("BOOL_TRUE_STR", null);
            Environment.SetEnvironmentVariable("BOOL_TRUE_BOOL", null);

            var config = ReflectionHelper.PopulateInstance<BoolConfig>();
            Assert.True(config.BoolTrueStr);
            Assert.True(config.BoolTrueBool);
        }

        [Fact]
        public void Int_Default_String_And_Int_Both_Work()
        {
            Environment.SetEnvironmentVariable("INT_STR", null);
            Environment.SetEnvironmentVariable("INT_INT", null);

            var config = ReflectionHelper.PopulateInstance<IntConfig>();
            Assert.Equal(42, config.IntStr);
            Assert.Equal(42, config.IntInt);
        }
    }
}
