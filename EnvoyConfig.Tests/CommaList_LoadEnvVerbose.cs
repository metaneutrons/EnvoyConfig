using System;
using System.Collections.Generic;
using EnvoyConfig;
using Xunit;

namespace EnvoyConfig.Tests
{
    public class CommaList_LoadEnvVerbose
    {
        [Fact]
        public void CommaList_LoadEnvVerbose_Test()
        {
            // Basic comma-separated string list
            Environment.SetEnvironmentVariable("STRINGS_COMMA", "a,b,c");
            // Quoted values, comma separator
            Environment.SetEnvironmentVariable("STRINGS_QUOTED", "\"a,b\",c,\"d,e\"");
            // Semicolon separator
            Environment.SetEnvironmentVariable("STRINGS_SEMICOLON", "x;y;z");
            // Quoted values, semicolon separator
            Environment.SetEnvironmentVariable("STRINGS_QUOTED_SEMICOLON", "\"x;y\";z;\"w;v\"");
            // Int list, comma
            Environment.SetEnvironmentVariable("INTS_COMMA", "1,2,3");
            // Int list, semicolon
            Environment.SetEnvironmentVariable("INTS_SEMICOLON", "4;5;6");
            var config = EnvConfig.Load<VerboseCommaListConfig>();
            Assert.Equal(["a", "b", "c"], config.StringsComma);
            Assert.Equal(["a,b", "c", "d,e"], config.StringsQuoted);
            Assert.Equal(["x", "y", "z"], config.StringsSemicolon);
            Assert.Equal(["x;y", "z", "w;v"], config.StringsQuotedSemicolon);
            Assert.Equal([1, 2, 3], config.IntsComma);
            Assert.Equal([4, 5, 6], config.IntsSemicolon);
        }
    }
}
