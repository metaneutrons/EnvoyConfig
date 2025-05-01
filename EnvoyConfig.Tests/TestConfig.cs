namespace EnvoyConfig.Tests;

using System.Collections.Generic;
using EnvoyConfig.Attributes;

public partial class TestConfig : EnvoyConfig.Abstractions.IEnvInitializable
{
    [Env(Key = "TEST_SIMPLE")]
    public int SimpleInt { get; set; }

    [Env(Key = "TEST_MISSING_ISOLATED")]
    public int SimpleIntIsolated { get; set; }
}

// Comma-separated list config
public partial class SepListConfig : EnvoyConfig.Abstractions.IEnvInitializable
{
    [Env(Key = "NUMBERS", IsList = true)]
    public List<int> Numbers { get; set; } = new List<int>();
}

// Numbered list config
public partial class NumberedListConfig : EnvoyConfig.Abstractions.IEnvInitializable
{
    [Env(ListPrefix = "SEQ_")]
    public List<string> Items { get; set; } = new List<string>();
}

// Nested prefix nested type
public partial class NestedChild : EnvoyConfig.Abstractions.IEnvInitializable
{
    [Env(Key = "CHILD_VALUE")]
    public int ChildValue { get; set; }
}

// Nested prefix config
public partial class NestedConfig : EnvoyConfig.Abstractions.IEnvInitializable
{
    [Env(NestedPrefix = "PARENT_")]
    public NestedChild Child { get; set; } = new NestedChild();
}

// Verbose comma/semicolon/quoted list config for advanced EnvMode.SepList test
public partial class VerboseSepListConfig : EnvoyConfig.Abstractions.IEnvInitializable
{
    [Env(Key = "STRINGS_COMMA", IsList = true)]
    public List<string> StringsComma { get; set; } = new List<string>();

    [Env(Key = "STRINGS_QUOTED", IsList = true)]
    public List<string> StringsQuoted { get; set; } = new List<string>();

    [Env(Key = "STRINGS_SEMICOLON", IsList = true, ListSeparator = ';')]
    public List<string> StringsSemicolon { get; set; } = new List<string>();

    [Env(Key = "STRINGS_QUOTED_SEMICOLON", IsList = true, ListSeparator = ';')]
    public List<string> StringsQuotedSemicolon { get; set; } = new List<string>();

    [Env(Key = "INTS_COMMA", IsList = true)]
    public List<int> IntsComma { get; set; } = new List<int>();

    [Env(Key = "INTS_SEMICOLON", IsList = true, ListSeparator = ';')]
    public List<int> IntsSemicolon { get; set; } = new List<int>();
}
