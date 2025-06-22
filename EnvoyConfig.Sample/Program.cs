using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using dotenv.net;
using EnvoyConfig;
using EnvoyConfig.Conversion;
using Spectre.Console;

namespace EnvoyConfig.Sample;

/// <summary>
/// Main program for the SNAPDOG configuration demonstration.
/// Shows how to register custom type converters and load nested configurations using the SampleConfig with SNAPDOG clients.
/// </summary>
internal class Program
{
    private static void Main(string[] args)
    {
        // Parse command-line arguments for save operations and flags
        var (saveOperations, showEnvVars) = ParseCommandLineArguments(args);

        // Register the custom KnxAddress converter first
        RegisterCustomConverters();

        // Resolve absolute path to sample.env
        var envPath = Path.Combine(AppContext.BaseDirectory, "sample.env");
        if (!File.Exists(envPath))
        {
            // Try project dir relative to working dir
            envPath = Path.GetFullPath("EnvoyConfig.Sample/sample.env", Environment.CurrentDirectory);
        }

        // Load .env file
        DotEnv.Load(options: new DotEnvOptions(envFilePaths: [envPath], overwriteExistingVars: true));

        // Set global prefix
        EnvConfig.GlobalPrefix = "SNAPDOG_";

        // Create logger for both config loading and save operations
        var logger = new SpectreConsoleLogSink();

        // Load config using SampleConfig (which now includes SnapdogClients)
        var config = EnvConfig.Load<SampleConfig>(logger);

        DisplayHeader();
        DisplayVersionInformation();

        // Only display environment variables if the flag is set
        if (showEnvVars)
        {
            DisplayEnvironmentVariables();
        }

        DisplayConfiguration(config);
        DisplayFooter();

        // Process save operations if any were specified
        ProcessSaveOperations(saveOperations, config, logger);

        // Display command line options at the end
        DisplayCommandLineOptions(args);
    }

    private static (
        List<(string operation, string filename)> saveOperations,
        bool showEnvVars
    ) ParseCommandLineArguments(string[] args)
    {
        var saveOperations = new List<(string operation, string filename)>();
        var showEnvVars = false;

        for (int i = 0; i < args.Length; i++)
        {
            if (args[i] == "--save" && i + 1 < args.Length)
            {
                saveOperations.Add(("save", args[i + 1]));
                i++; // Skip the filename argument
            }
            else if (args[i] == "--save-defaults" && i + 1 < args.Length)
            {
                saveOperations.Add(("save-defaults", args[i + 1]));
                i++; // Skip the filename argument
            }
            else if (args[i] == "--show-env-vars")
            {
                showEnvVars = true;
            }
            else if (args[i] == "--help" || args[i] == "-h")
            {
                DisplayHelp();
                Environment.Exit(0);
            }
        }

        return (saveOperations, showEnvVars);
    }

    private static void ProcessSaveOperations(
        List<(string operation, string filename)> saveOperations,
        SampleConfig config,
        SpectreConsoleLogSink logger
    )
    {
        if (saveOperations.Count == 0)
            return;

        AnsiConsole.WriteLine();
        AnsiConsole.Write(new Rule("[blue]Save Operations[/]").RuleStyle("blue"));
        AnsiConsole.WriteLine();

        foreach (var (operation, filename) in saveOperations)
        {
            try
            {
                if (operation == "save")
                {
                    EnvConfig.Save(config, filename, logger);
                    AnsiConsole.MarkupLine(
                        $"[green]✓ Successfully saved current configuration to:[/] [white]{filename}[/]"
                    );
                }
                else if (operation == "save-defaults")
                {
                    EnvConfig.SaveDefaults<SampleConfig>(filename, logger);
                    AnsiConsole.MarkupLine(
                        $"[green]✓ Successfully saved defaults template to:[/] [white]{filename}[/]"
                    );
                }
            }
            catch (Exception ex)
            {
                AnsiConsole.MarkupLine($"[red]❌ Error saving to {filename}: {ex.Message}[/]");
            }
        }

        AnsiConsole.WriteLine();
    }

    private static void RegisterCustomConverters()
    {
        // Register the custom KnxAddress converter
        TypeConverterRegistry.RegisterConverter(typeof(KnxAddress), new KnxAddressConverter());
        TypeConverterRegistry.RegisterConverter(typeof(KnxAddress?), new KnxAddressConverter());
    }

    private static void DisplayHeader()
    {
        AnsiConsole.Write(new Align(new FigletText("Snapdog Config").Color(Color.Green), HorizontalAlignment.Left));
        AnsiConsole.MarkupLine($"[bold yellow]🐶 Welcome to the Snapdog Sample Application![/]");
        AnsiConsole.MarkupLine($"[bold blue]Loaded configuration from:[/] [white]sample.env[/]");
        AnsiConsole.WriteLine();
    }

    private static void DisplayVersionInformation()
    {
        try
        {
            var assembly = Assembly.GetExecutingAssembly();

            // Get version information from assembly attributes
            var assemblyVersion = assembly.GetName().Version?.ToString() ?? "Unknown";
            var fileVersion = assembly.GetCustomAttribute<AssemblyFileVersionAttribute>()?.Version ?? "Unknown";
            var informationalVersion =
                assembly.GetCustomAttribute<AssemblyInformationalVersionAttribute>()?.InformationalVersion ?? "Unknown";
            var configuration =
                assembly.GetCustomAttribute<AssemblyConfigurationAttribute>()?.Configuration ?? "Unknown";

            // Try to get GitVersion information from multiple sources
            var gitVersionType = assembly.GetTypes().FirstOrDefault(t => t.Name == "GitVersionInformation");

            // Also try to find any other GitVersion-related types
            var allGitVersionTypes = assembly
                .GetTypes()
                .Where(t => t.Name.Contains("GitVersion", StringComparison.OrdinalIgnoreCase))
                .ToList();

            AnsiConsole.Write(new Rule("[blue]Version Information[/]").RuleStyle("blue"));

            var versionTable = new Table().NoBorder();
            versionTable.AddColumn(new TableColumn("[grey]Property[/]").LeftAligned());
            versionTable.AddColumn(new TableColumn("[grey]Value[/]").LeftAligned());

            versionTable.AddRow("[white]Assembly Version[/]", $"[bold cyan]{assemblyVersion}[/]");
            versionTable.AddRow("[white]File Version[/]", $"[bold cyan]{fileVersion}[/]");
            versionTable.AddRow("[white]Informational Version[/]", $"[bold cyan]{informationalVersion}[/]");
            versionTable.AddRow("[white]Configuration[/]", $"[bold cyan]{configuration}[/]");

            // Parse more detailed version information from InformationalVersion
            if (!string.IsNullOrEmpty(informationalVersion) && informationalVersion != "Unknown")
            {
                var parts = informationalVersion.Split('+');
                if (parts.Length > 0)
                {
                    var version = parts[0]; // e.g., "0.1.0-beta.87"
                    versionTable.AddRow("[white]SemVer[/]", $"[bold yellow]{version}[/]");

                    if (parts.Length > 1)
                    {
                        var metadata = parts[1]; // e.g., "Branch.develop.Sha.27335d6..."
                        var metadataParts = metadata.Split('.');

                        for (int i = 0; i < metadataParts.Length; i += 2)
                        {
                            if (i + 1 < metadataParts.Length)
                            {
                                var key = metadataParts[i];
                                var value = metadataParts[i + 1];

                                if (key.Equals("Branch", StringComparison.OrdinalIgnoreCase))
                                {
                                    versionTable.AddRow("[white]Git Branch[/]", $"[bold green]{value}[/]");
                                }
                                else if (key.Equals("Sha", StringComparison.OrdinalIgnoreCase))
                                {
                                    // Show only first 8 characters of SHA for readability
                                    var shortSha = value.Length > 8 ? value.Substring(0, 8) : value;
                                    versionTable.AddRow("[white]Git Commit[/]", $"[bold magenta]{shortSha}[/]");
                                }
                            }
                        }
                    }
                }
            }

            // Add detailed GitVersion information section
            versionTable.AddRow("", ""); // Empty row for spacing
            versionTable.AddRow("[bold white]GitVersion Details[/]", "");

            if (gitVersionType != null)
            {
                // Get all public static properties from GitVersionInformation
                var properties = gitVersionType
                    .GetProperties(BindingFlags.Public | BindingFlags.Static)
                    .OrderBy(p => p.Name);

                var gitVersionInfo = new List<(string key, string value)>();

                foreach (var prop in properties)
                {
                    try
                    {
                        var value = prop.GetValue(null)?.ToString();
                        if (!string.IsNullOrEmpty(value))
                        {
                            gitVersionInfo.Add((prop.Name, value));
                        }
                    }
                    catch
                    {
                        // Skip properties that can't be read
                    }
                }

                if (gitVersionInfo.Count > 0)
                {
                    // Display key GitVersion properties
                    var importantProps = new[]
                    {
                        "MajorMinorPatch",
                        "SemVer",
                        "FullSemVer",
                        "BranchName",
                        "Sha",
                        "CommitDate",
                        "VersionSourceSha",
                        "CommitsSinceVersionSource",
                    };

                    foreach (var propName in importantProps)
                    {
                        var prop = gitVersionInfo.FirstOrDefault(p =>
                            p.key.Equals(propName, StringComparison.OrdinalIgnoreCase)
                        );
                        if (!string.IsNullOrEmpty(prop.value))
                        {
                            var displayName = propName switch
                            {
                                "MajorMinorPatch" => "Version",
                                "BranchName" => "Branch",
                                "Sha" => "Commit SHA",
                                "CommitDate" => "Commit Date",
                                "VersionSourceSha" => "Version Source SHA",
                                "CommitsSinceVersionSource" => "Commits Since Version",
                                _ => propName,
                            };

                            var displayValue =
                                propName == "Sha" && prop.value.Length > 8 ? prop.value.Substring(0, 8) : prop.value;

                            versionTable.AddRow($"[white]  {displayName}[/]", $"[cyan]{displayValue}[/]");
                        }
                    }

                    // Add any remaining properties not in the important list
                    var remainingProps = gitVersionInfo
                        .Where(p => !importantProps.Contains(p.key, StringComparer.OrdinalIgnoreCase))
                        .Take(5);
                    foreach (var (key, value) in remainingProps)
                    {
                        var displayValue =
                            key.ToLower().Contains("sha") && value.Length > 8 ? value.Substring(0, 8) : value;
                        versionTable.AddRow($"[white]  {key}[/]", $"[dim cyan]{displayValue}[/]");
                    }
                }
                else
                {
                    versionTable.AddRow("[white]  GitVersion Properties[/]", "[yellow]None accessible[/]");
                }
            }
            else
            {
                versionTable.AddRow("[white]  GitVersion[/]", "[yellow]Not available[/]");
            }

            AnsiConsole.Write(
                new Panel(versionTable).Header("[blue]📦 Version Information[/]", Justify.Left).Collapse()
            );
            AnsiConsole.WriteLine();
        }
        catch (Exception ex)
        {
            AnsiConsole.MarkupLine($"[red]❌ Error retrieving version information: {ex.Message}[/]");
            AnsiConsole.WriteLine();
        }
    }

    private static void DisplayEnvironmentVariables()
    {
        AnsiConsole.Write(new Rule("[grey]--- ENVIRONMENT VARIABLES ---[/]").RuleStyle("grey"));

        // Print all SNAPDOG_ env vars
        var snapdogVars = Environment
            .GetEnvironmentVariables()
            .Cast<System.Collections.DictionaryEntry>()
            .Where(e => e.Key is string k && k.StartsWith("SNAPDOG_"))
            .Select(e => ($"{e.Key}", e.Value?.ToString() ?? "<null>"))
            .OrderBy(t => t.Item1)
            .ToArray();

        if (snapdogVars.Length > 0)
        {
            var table = new Table().NoBorder();
            table.AddColumn(new TableColumn("[grey]Env Key[/]").LeftAligned());
            table.AddColumn(new TableColumn("[grey]Value[/]").LeftAligned());
            foreach (var (k, v) in snapdogVars)
            {
                table.AddRow($"[white]{k}[/]", $"[bold cyan]{v}[/]");
            }

            AnsiConsole.Write(
                new Panel(table).Header("[green]SNAPDOG_ Environment Variables[/]", Justify.Left).Collapse()
            );
        }
        AnsiConsole.Write(new Rule("[grey]--- END ENVIRONMENT VARIABLES ---[/]").RuleStyle("grey"));
        AnsiConsole.WriteLine();
    }

    private static void DisplayConfiguration(SampleConfig config)
    {
        // Display basic system configuration
        PrintSection(
            "System",
            "⚙️",
            [("Environment", typeof(string).Name, config.Env), ("Log Level", typeof(string).Name, config.LogLevel)]
        );

        PrintSection(
            "Telemetry",
            "📊",
            [
                ("Enabled", typeof(bool).Name, config.TelemetryEnabled.ToString()),
                ("Service Name", typeof(string).Name, config.TelemetryServiceName),
                ("Sampling Rate", typeof(int).Name, config.TelemetrySamplingRate.ToString()),
            ]
        );

        PrintSection(
            "Prometheus",
            "📈",
            [
                ("Enabled", typeof(bool).Name, config.PrometheusEnabled.ToString()),
                ("Path", typeof(string).Name, config.PrometheusPath),
                ("Port", typeof(int).Name, config.PrometheusPort.ToString()),
            ]
        );

        PrintSection(
            "Jaeger",
            "🕵️",
            [
                ("Enabled", typeof(bool).Name, config.JaegerEnabled.ToString()),
                ("Endpoint", typeof(string).Name, config.JaegerEndpoint),
                ("Agent Host", typeof(string).Name, config.JaegerAgentHost),
                ("Agent Port", typeof(int).Name, config.JaegerAgentPort.ToString()),
            ]
        );

        PrintSection(
            "API Auth",
            "🔑",
            [
                ("Enabled", typeof(bool).Name, config.ApiAuthEnabled.ToString()),
                ("API Keys", "string[]", string.Join(", ", config.ApiKeys)),
            ]
        );

        PrintSection("Zones", "🗺️", [("Zones", "string[]", string.Join(", ", config.Zones))]);

        // Snapcast configuration as key-value map
        PrintSection(
            "Snapcast (key-value object)",
            "🎵",
            config.Snapcast.Select(kv => (kv.Key, typeof(string).Name, kv.Value)).ToArray()
        );

        // Print all MQTT zone configs
        if (config.ZonesMqtt != null && config.ZonesMqtt.Count > 0)
        {
            int idx = 1;
            foreach (var zone in config.ZonesMqtt)
            {
                var props = zone.GetType().GetProperties();
                var rows = props
                    .Select(p =>
                        (
                            p.Name.Replace("Topic", " Topic")
                                .Replace("Set ", "Set ")
                                .Replace("Base", "Base ")
                                .Replace("Control", "Control ")
                                .Replace("Track", "Track ")
                                .Replace("Playlist", "Playlist ")
                                .Replace("Volume", "Volume ")
                                .Replace("Mute", "Mute ")
                                .Replace("State", "State ")
                                .Replace("  ", " ")
                                .Trim(),
                            p.PropertyType.Name,
                            p.GetValue(zone)?.ToString() ?? "<null>"
                        )
                    )
                    .ToArray();
                PrintSection($"MQTT Zone {idx}", "📡", rows);
                idx++;
            }
        }

        // Display SNAPDOG clients configuration - NEW SECTION
        DisplaySnapdogClients(config.SnapdogClients);

        // Display radio stations
        if (config.RadioStations != null && config.RadioStations.Count > 0)
        {
            AnsiConsole.WriteLine();
            AnsiConsole.Write(new Rule("[green]Radio Stations[/]").RuleStyle("green"));
            AnsiConsole.WriteLine();

            for (int i = 0; i < config.RadioStations.Count; i++)
            {
                var station = config.RadioStations[i];
                PrintSection(
                    $"Radio Station {i + 1}: {station.Name}",
                    "📻",
                    [("Name", typeof(string).Name, station.Name), ("URL", typeof(string).Name, station.URL)]
                );
            }
        }
        else
        {
            AnsiConsole.MarkupLine("[yellow]No radio stations configured.[/]");
        }
    }

    private static void DisplaySnapdogClients(System.Collections.Generic.List<ClientConfig> clients)
    {
        if (clients == null || clients.Count == 0)
        {
            AnsiConsole.MarkupLine("[yellow]No SNAPDOG clients configured.[/]");
            return;
        }

        AnsiConsole.WriteLine();
        AnsiConsole.Write(new Rule($"[green]SNAPDOG Clients ({clients.Count} configured)[/]").RuleStyle("green"));
        AnsiConsole.WriteLine();

        for (int i = 0; i < clients.Count; i++)
        {
            var client = clients[i];

            // Basic client info
            PrintSection(
                $"Client {i + 1}: {client.Name}",
                "🎛️",
                [
                    ("Name", typeof(string).Name, client.Name),
                    ("MAC Address", typeof(string).Name, client.Mac),
                    ("MQTT Base Topic", typeof(string).Name, client.MqttBaseTopic),
                    ("Default Zone", typeof(int).Name, client.DefaultZone.ToString()),
                ]
            );

            // MQTT Topics
            var mqttRows = new[]
            {
                ("Volume Set Topic", typeof(string).Name, client.Mqtt.VolumeSetTopic ?? "<null>"),
                ("Mute Set Topic", typeof(string).Name, client.Mqtt.MuteSetTopic ?? "<null>"),
                ("Latency Set Topic", typeof(string).Name, client.Mqtt.LatencySetTopic ?? "<null>"),
                ("Zone Set Topic", typeof(string).Name, client.Mqtt.ZoneSetTopic ?? "<null>"),
                ("Control Topic", typeof(string).Name, client.Mqtt.ControlTopic ?? "<null>"),
                ("Connected Topic", typeof(string).Name, client.Mqtt.ConnectedTopic ?? "<null>"),
                ("Volume Topic", typeof(string).Name, client.Mqtt.VolumeTopic ?? "<null>"),
                ("Mute Topic", typeof(string).Name, client.Mqtt.MuteTopic ?? "<null>"),
                ("Latency Topic", typeof(string).Name, client.Mqtt.LatencyTopic ?? "<null>"),
                ("Zone Topic", typeof(string).Name, client.Mqtt.ZoneTopic ?? "<null>"),
                ("State Topic", typeof(string).Name, client.Mqtt.StateTopic ?? "<null>"),
            };
            PrintSection($"MQTT Topics (Client {i + 1})", "📡", mqttRows);

            // KNX Configuration
            var knxRows = new[]
            {
                ("Enabled", typeof(bool).Name, client.Knx.Enabled.ToString()),
                ("Volume", typeof(KnxAddress).Name, client.Knx.Volume?.ToString() ?? "<null>"),
                ("Volume Status", typeof(KnxAddress).Name, client.Knx.VolumeStatus?.ToString() ?? "<null>"),
                ("Volume Up", typeof(KnxAddress).Name, client.Knx.VolumeUp?.ToString() ?? "<null>"),
                ("Volume Down", typeof(KnxAddress).Name, client.Knx.VolumeDown?.ToString() ?? "<null>"),
                ("Mute", typeof(KnxAddress).Name, client.Knx.Mute?.ToString() ?? "<null>"),
                ("Mute Status", typeof(KnxAddress).Name, client.Knx.MuteStatus?.ToString() ?? "<null>"),
                ("Mute Toggle", typeof(KnxAddress).Name, client.Knx.MuteToggle?.ToString() ?? "<null>"),
                ("Latency", typeof(KnxAddress).Name, client.Knx.Latency?.ToString() ?? "<null>"),
                ("Latency Status", typeof(KnxAddress).Name, client.Knx.LatencyStatus?.ToString() ?? "<null>"),
                ("Zone", typeof(KnxAddress).Name, client.Knx.Zone?.ToString() ?? "<null>"),
                ("Zone Status", typeof(KnxAddress).Name, client.Knx.ZoneStatus?.ToString() ?? "<null>"),
                ("Connected Status", typeof(KnxAddress).Name, client.Knx.ConnectedStatus?.ToString() ?? "<null>"),
            };
            PrintSection($"KNX Configuration (Client {i + 1})", "🏠", knxRows);
        }
    }

    private static void DisplayFooter()
    {
        AnsiConsole.Write(new Rule("[green]✔ Ready![/]").RuleStyle("green"));
        AnsiConsole.MarkupLine("[italic grey]Tip: Edit sample.env and rerun to see changes instantly![/]");
    }

    private static void PrintSection(string title, string icon, (string, string, string)[] rows)
    {
        var panel = new Panel(CreateTable(rows)).Header($"{icon} [bold]{title}[/]", Justify.Left).Collapse();
        AnsiConsole.Write(panel);
    }

    private static Table CreateTable((string, string, string)[] rows)
    {
        var table = new Table().NoBorder();
        table.AddColumn(new TableColumn("[grey]Key[/]").LeftAligned());
        table.AddColumn(new TableColumn("[grey]Type[/]").LeftAligned());
        table.AddColumn(new TableColumn("[grey]Value[/]").LeftAligned());
        foreach (var (k, t, v) in rows)
        {
            var safeType = Markup.Escape(t);
            string[] lines;
            if ((t.Contains("[]") || t.ToLower().Contains("array")) && v.Contains(","))
            {
                // Split by comma and display each on a new line
                lines = v.Split(',').Select(s => Markup.Escape(s.Trim())).ToArray();
            }
            else if (v.Contains("\n"))
            {
                lines = v.Split('\n').Select(s => Markup.Escape(s)).ToArray();
            }
            else
            {
                lines = [Markup.Escape(v)];
            }
            for (int i = 0; i < lines.Length; i++)
            {
                if (i == 0)
                {
                    table.AddRow($"[white]{k}[/]", $"[yellow]{safeType}[/]", $"[bold cyan]{lines[i]}[/]");
                }
                else
                {
                    table.AddRow("", "", $"[bold cyan]{lines[i]}[/]");
                }
            }
        }

        return table;
    }

    private static void DisplayHelp()
    {
        AnsiConsole.Write(new FigletText("Snapdog Config").Color(Color.Green));
        AnsiConsole.MarkupLine("[bold yellow]🐶 SNAPDOG Configuration Sample Application[/]");
        AnsiConsole.WriteLine();

        AnsiConsole.MarkupLine("[bold]Usage:[/]");
        AnsiConsole.MarkupLine("  [cyan]dotnet run[/] [[options]]");
        AnsiConsole.WriteLine();

        AnsiConsole.MarkupLine("[bold]Options:[/]");
        AnsiConsole.MarkupLine("  [cyan]--show-env-vars[/]          Show all SNAPDOG environment variables");
        AnsiConsole.MarkupLine("  [cyan]--save <filename>[/]        Save current configuration to file");
        AnsiConsole.MarkupLine("  [cyan]--save-defaults <filename>[/] Save default configuration template to file");
        AnsiConsole.MarkupLine("  [cyan]--help, -h[/]              Show this help message");
        AnsiConsole.WriteLine();

        AnsiConsole.MarkupLine("[bold]Examples:[/]");
        AnsiConsole.MarkupLine("  [dim]dotnet run --show-env-vars[/]");
        AnsiConsole.MarkupLine("  [dim]dotnet run --save config.env --show-env-vars[/]");
        AnsiConsole.MarkupLine("  [dim]dotnet run --save-defaults template.env[/]");
        AnsiConsole.WriteLine();
    }

    private static void DisplayCommandLineOptions(string[] args)
    {
        if (args.Length == 0)
        {
            AnsiConsole.WriteLine();
            AnsiConsole.Write(new Rule("[grey]Command Line Info[/]").RuleStyle("grey"));
            AnsiConsole.MarkupLine(
                "[dim]No command line arguments provided. Use [cyan]--help[/] to see available options.[/]"
            );
            AnsiConsole.WriteLine();
            return;
        }

        AnsiConsole.WriteLine();
        AnsiConsole.Write(new Rule("[blue]Command Line Arguments[/]").RuleStyle("blue"));

        var table = new Table().NoBorder();
        table.AddColumn(new TableColumn("[grey]Argument[/]").LeftAligned());
        table.AddColumn(new TableColumn("[grey]Description[/]").LeftAligned());

        foreach (var arg in args)
        {
            var description = arg switch
            {
                "--show-env-vars" => "Display environment variables",
                "--help" or "-h" => "Show help information",
                var s when s.StartsWith("--save-defaults") => "Save default configuration template",
                var s when s.StartsWith("--save") => "Save current configuration",
                _ => arg.StartsWith("--") ? "Unknown option" : "Parameter/filename",
            };

            table.AddRow($"[cyan]{arg}[/]", $"[white]{description}[/]");
        }

        AnsiConsole.Write(
            new Panel(table).Header("[blue]📋 Executed with these arguments[/]", Justify.Left).Collapse()
        );
        AnsiConsole.WriteLine();
    }
}
