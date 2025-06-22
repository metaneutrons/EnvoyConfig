using System;
using System.Collections.Generic;
using System.IO;
using EnvoyConfig.Attributes;
using Xunit;

namespace EnvoyConfig.Tests
{
    [Collection("NonParallel")]
    public class Save
    {
        private class TestConfig
        {
            [Env(Key = "TEST_STRING", Default = "default_value")]
            public string TestString { get; set; } = "current_value";

            [Env(Key = "TEST_INT", Default = 42)]
            public int TestInt { get; set; } = 123;

            [Env(Key = "TEST_BOOL", Default = false)]
            public bool TestBool { get; set; } = true;

            [Env(Key = "TEST_LIST", IsList = true)]
            public List<string> TestList { get; set; } = new() { "item1", "item2" };

            [Env(ListPrefix = "TEST_NUMBERED_")]
            public List<string> TestNumberedList { get; set; } = new() { "num1", "num2", "num3" };

            [Env(MapPrefix = "TEST_MAP_")]
            public Dictionary<string, string> TestMap { get; set; } =
                new() { ["key1"] = "value1", ["key2"] = "value2" };
        }

        private class NestedTestConfig
        {
            [Env(Key = "NESTED_VALUE")]
            public string NestedValue { get; set; } = "nested";
        }

        private class TestConfigWithNested
        {
            [Env(Key = "MAIN_VALUE")]
            public string MainValue { get; set; } = "main";

            [Env(NestedPrefix = "NESTED_")]
            public NestedTestConfig Nested { get; set; } = new();

            [Env(NestedListPrefix = "ITEM_", NestedListSuffix = "_")]
            public List<NestedTestConfig> NestedList { get; set; } =
                new()
                {
                    new() { NestedValue = "item1" },
                    new() { NestedValue = "item2" },
                };
        }

        private readonly string _testDir = Path.Combine(Path.GetTempPath(), "EnvoyConfigTests");

        public Save()
        {
            if (Directory.Exists(_testDir))
                Directory.Delete(_testDir, true);
            Directory.CreateDirectory(_testDir);
        }

        [Fact]
        public void Save_CreatesValidEnvFile()
        {
            // Arrange
            var config = new TestConfig();
            var filePath = Path.Combine(_testDir, "test-save.env");

            // Act
            EnvConfig.Save(config, filePath);

            // Assert
            Assert.True(File.Exists(filePath));
            var content = File.ReadAllText(filePath);

            Assert.Contains("TEST_STRING=current_value", content);
            Assert.Contains("TEST_INT=123", content);
            Assert.Contains("TEST_BOOL=true", content);
            Assert.Contains("TEST_LIST=item1,item2", content);
            Assert.Contains("TEST_NUMBERED_1=num1", content);
            Assert.Contains("TEST_NUMBERED_2=num2", content);
            Assert.Contains("TEST_NUMBERED_3=num3", content);
            Assert.Contains("TEST_MAP_key1=value1", content);
            Assert.Contains("TEST_MAP_key2=value2", content);
        }

        [Fact]
        public void SaveDefaults_CreatesValidEnvFile()
        {
            // Arrange
            var filePath = Path.Combine(_testDir, "test-defaults.env");

            // Act
            EnvConfig.SaveDefaults<TestConfig>(filePath);

            // Assert
            Assert.True(File.Exists(filePath));
            var content = File.ReadAllText(filePath);

            Assert.Contains("TEST_STRING=default_value", content);
            Assert.Contains("TEST_INT=42", content);
            Assert.Contains("TEST_BOOL=false", content);
            Assert.Contains("TEST_LIST=", content);
            Assert.Contains("TEST_NUMBERED_1=", content);
        }

        [Fact]
        public void Save_WithGlobalPrefix_AppliesPrefix()
        {
            // Arrange
            var config = new TestConfig();
            var filePath = Path.Combine(_testDir, "test-prefix.env");
            var originalPrefix = EnvConfig.GlobalPrefix;

            try
            {
                EnvConfig.GlobalPrefix = "MYAPP_";

                // Act
                EnvConfig.Save(config, filePath);

                // Assert
                var content = File.ReadAllText(filePath);
                Assert.Contains("MYAPP_TEST_STRING=current_value", content);
                Assert.Contains("MYAPP_TEST_INT=123", content);
                Assert.Contains("MYAPP_TEST_NUMBERED_1=num1", content);
            }
            finally
            {
                EnvConfig.GlobalPrefix = originalPrefix;
            }
        }

        [Fact]
        public void Save_WithNestedObjects_SerializesCorrectly()
        {
            // Arrange
            var config = new TestConfigWithNested();
            var filePath = Path.Combine(_testDir, "test-nested.env");

            // Act
            EnvConfig.Save(config, filePath);

            // Assert
            var content = File.ReadAllText(filePath);
            Assert.Contains("MAIN_VALUE=main", content);
            Assert.Contains("NESTED_NESTED_VALUE=nested", content);
            Assert.Contains("ITEM_1_NESTED_VALUE=item1", content);
            Assert.Contains("ITEM_2_NESTED_VALUE=item2", content);
        }

        [Fact]
        public void SaveDefaults_WithNestedList_CreatesPlaceholder()
        {
            // Arrange
            var filePath = Path.Combine(_testDir, "test-nested-defaults.env");

            // Act
            EnvConfig.SaveDefaults<TestConfigWithNested>(filePath);

            // Assert
            var content = File.ReadAllText(filePath);
            Assert.Contains("MAIN_VALUE=", content);
            Assert.Contains("NESTED_NESTED_VALUE=", content);
            Assert.Contains("ITEM_1_NESTED_VALUE=", content);
            // Should only have one placeholder entry, not multiple
            Assert.DoesNotContain("ITEM_2_NESTED_VALUE=", content);
        }

        [Fact]
        public void Save_ThrowsArgumentNullException_ForNullInstance()
        {
            Assert.Throws<ArgumentNullException>(() => EnvConfig.Save<TestConfig>(null!, "test.env"));
        }

        [Fact]
        public void Save_ThrowsArgumentNullException_ForNullFilePath()
        {
            var config = new TestConfig();
            Assert.Throws<ArgumentNullException>(() => EnvConfig.Save(config, null!));
        }

        [Fact]
        public void SaveDefaults_ThrowsArgumentNullException_ForNullFilePath()
        {
            Assert.Throws<ArgumentNullException>(() => EnvConfig.SaveDefaults<TestConfig>(null!));
        }
    }
}
