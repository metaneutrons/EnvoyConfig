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
            {
                table.AddRow($"[white]{k}[/]", $"[bold cyan]{v}[/]");
            }

            AnsiConsole.Write(
                new Panel(table).Header("[green]SNAPDOG_ Environment Variables[/]", Justify.Left).Collapse()
            );
        }
        AnsiConsole.Write(new Rule("[grey]--- END ENVIRONMENT VARIABLES ---[/]").RuleStyle("grey"));
        AnsiConsole.WriteLine();

        PrintSection(
            "System",
            "⚙️",
            new[]
            {
                ("Environment", typeof(string).Name, config.Env),
                ("Log Level", typeof(string).Name, config.LogLevel),
            }
        );
        PrintSection(
            "Telemetry",
            "📊",
            new[]
            {
                ("Enabled", typeof(bool).Name, config.TelemetryEnabled.ToString()),
                ("Service Name", typeof(string).Name, config.TelemetryServiceName),
                ("Sampling Rate", typeof(int).Name, config.TelemetrySamplingRate.ToString()),
            }
        );
        PrintSection(
            "Prometheus",
            "📈",
            new[]
            {
                ("Enabled", typeof(bool).Name, config.PrometheusEnabled.ToString()),
                ("Path", typeof(string).Name, config.PrometheusPath),
                ("Port", typeof(int).Name, config.PrometheusPort.ToString()),
            }
        );
        PrintSection(
            "Jaeger",
            "🕵️",
            new[]
            {
                ("Enabled", typeof(bool).Name, config.JaegerEnabled.ToString()),
                ("Endpoint", typeof(string).Name, config.JaegerEndpoint),
                ("Agent Host", typeof(string).Name, config.JaegerAgentHost),
                ("Agent Port", typeof(int).Name, config.JaegerAgentPort.ToString()),
            }
        );
        PrintSection(
            "API Auth",
            "🔑",
            new[]
            {
                ("Enabled", typeof(bool).Name, config.ApiAuthEnabled.ToString()),
                ("API Keys", "string[]", string.Join(", ", config.ApiKeys)),
            }
        );
        PrintSection("Zones", "🗺️", new[] { ("Zones", "string[]", string.Join(", ", config.Zones)) });

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

        AnsiConsole.Write(new Rule("[green]✔ Ready![/]").RuleStyle("green"));
        AnsiConsole.MarkupLine("[italic grey]Tip: Edit sample.env and rerun to see changes instantly![/]");
    }

    static void PrintSection(string title, string icon, (string, string, string)[] rows)
    {
        var panel = new Panel(CreateTable(rows)).Header($"{icon} [bold]{title}[/]", Justify.Left).Collapse();
        AnsiConsole.Write(panel);
    }

    static Table CreateTable((string, string, string)[] rows)
    {
        var table = new Table().NoBorder();
        table.AddColumn(new TableColumn("[grey]Key[/]").LeftAligned());
        table.AddColumn(new TableColumn("[grey]Type[/]").LeftAligned());
        table.AddColumn(new TableColumn("[grey]Value[/]").LeftAligned());
        foreach (var (k, t, v) in rows)
        {
            var safeType = Spectre.Console.Markup.Escape(t);
            string[] lines;
            if ((t.Contains("[]") || t.ToLower().Contains("array")) && v.Contains(","))
            {
                // Split by comma and display each on a new line
                lines = v.Split(',').Select(s => Spectre.Console.Markup.Escape(s.Trim())).ToArray();
            }
            else if (v.Contains("\n"))
            {
                lines = v.Split('\n').Select(s => Spectre.Console.Markup.Escape(s)).ToArray();
            }
            else
            {
                lines = new[] { Spectre.Console.Markup.Escape(v) };
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
}
