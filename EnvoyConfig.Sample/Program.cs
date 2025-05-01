using System;
using dotenv.net;
using EnvoyConfig;
using Spectre.Console;

namespace EnvoyConfig.Sample;

class Program
{
    static void Main(string[] args)
    {
        // Resolve absolute path to sample.env
        var envPath = Path.Combine(AppContext.BaseDirectory, "sample.env");
        if (!System.IO.File.Exists(envPath))
        {
            // Try project dir relative to working dir
            envPath = Path.GetFullPath("EnvoyConfig.Sample/sample.env", Environment.CurrentDirectory);
        }

        // Load .env file
        DotEnv.Load(options: new DotEnvOptions(envFilePaths: new[] { envPath }, overwriteExistingVars: true));
        // Set global prefix
        EnvConfig.GlobalPrefix = "SNAPDOG_";

        // Load config
        var config = EnvConfig.Load<SampleConfig>(new SpectreConsoleLogSink());

        AnsiConsole.Write(new Align(new FigletText("Snapdog Config").Color(Color.Green), HorizontalAlignment.Left));

        AnsiConsole.MarkupLine($"[bold yellow]🐶 Welcome to the Snapdog Sample Application![/]");
        AnsiConsole.MarkupLine($"[bold blue]Loaded configuration from:[/] [white]sample.env[/]");
        AnsiConsole.WriteLine();

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
                table.AddRow($"[white]{k}[/]", $"[bold cyan]{v}[/]");
            AnsiConsole.Write(
                new Panel(table).Header("[green]SNAPDOG_ Environment Variables[/]", Justify.Left).Collapse()
            );
        }
        AnsiConsole.Write(new Rule("[grey]--- END ENVIRONMENT VARIABLES ---[/]").RuleStyle("grey"));
        AnsiConsole.WriteLine();

        PrintSection("System", "⚙️", new[] { ($"Environment", config.Env), ($"Log Level", config.LogLevel) });
        PrintSection(
            "Telemetry",
            "📊",
            new[]
            {
                ($"Enabled", config.TelemetryEnabled.ToString()),
                ($"Service Name", config.TelemetryServiceName),
                ($"Sampling Rate", config.TelemetrySamplingRate.ToString()),
            }
        );
        PrintSection(
            "Prometheus",
            "📈",
            new[]
            {
                ($"Enabled", config.PrometheusEnabled.ToString()),
                ($"Path", config.PrometheusPath),
                ($"Port", config.PrometheusPort.ToString()),
            }
        );
        PrintSection(
            "Jaeger",
            "🕵️",
            new[]
            {
                ($"Enabled", config.JaegerEnabled.ToString()),
                ($"Endpoint", config.JaegerEndpoint),
                ($"Agent Host", config.JaegerAgentHost),
                ($"Agent Port", config.JaegerAgentPort.ToString()),
            }
        );
        PrintSection(
            "API Auth",
            "🔑",
            new[] { ($"Enabled", config.ApiAuthEnabled.ToString()), ($"API Keys", string.Join(", ", config.ApiKeys)) }
        );
        PrintSection("Zones", "🗺️", new[] { ($"Zones", string.Join(", ", config.Zones)) });

        // Snapcast configuration as key-value map
        PrintSection("Snapcast", "🎵", config.Snapcast.Select(kv => ($"{kv.Key}", kv.Value)).ToArray());

        // Print all MQTT zone configs
        if (config.ZonesMqtt != null && config.ZonesMqtt.Count > 0)
        {
            int idx = 1;
            foreach (var zone in config.ZonesMqtt)
            {
                PrintSection($"MQTT Zone {idx}", "📡", new[] {
                    ("Control Set Topic", zone.ControlSetTopic),
                    ("Track Set Topic", zone.TrackSetTopic),
                    ("Playlist Set Topic", zone.PlaylistSetTopic),
                    ("Volume Set Topic", zone.VolumeSetTopic),
                    ("Mute Set Topic", zone.MuteSetTopic),
                    ("Control Topic", zone.ControlTopic),
                    ("Track Topic", zone.TrackTopic),
                    ("Playlist Topic", zone.PlaylistTopic),
                    ("Volume Topic", zone.VolumeTopic),
                    ("Mute Topic", zone.MuteTopic),
                    ("State Topic", zone.StateTopic)
                });
                idx++;
            }
        }

        AnsiConsole.Write(new Rule("[green]✔ Ready![/]").RuleStyle("green"));
        AnsiConsole.MarkupLine("[italic grey]Tip: Edit sample.env and rerun to see changes instantly![/]");
    }

    static void PrintSection(string title, string icon, (string, string)[] rows)
    {
        var panel = new Panel(CreateTable(rows)).Header($"{icon} [bold]{title}[/]", Justify.Left).Collapse();
        AnsiConsole.Write(panel);
    }

    static Table CreateTable((string, string)[] rows)
    {
        var table = new Table().NoBorder();
        table.AddColumn(new TableColumn("[grey]Key[/]").LeftAligned());
        table.AddColumn(new TableColumn("[grey]Value[/]").LeftAligned());
        foreach (var (k, v) in rows)
        {
            table.AddRow($"[white]{k}[/]", $"[bold cyan]{v}[/]");
        }

        return table;
    }
}
