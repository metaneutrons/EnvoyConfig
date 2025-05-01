using System;
using System.Collections.Generic;
using System.IO;
using dotenv.net;
using EnvoyConfig;
using Spectre.Console;

namespace EnvoyConfig.Sample
{
    public class Program
    {
        public static void Main(string[] args)
        {
            AnsiConsole.MarkupLine("[bold green]🐕 EnvoyConfig Sample Application[/]");
            AnsiConsole.Write(new Rule("[yellow]Loading environment[/]"));

            var cwd = AppContext.BaseDirectory;
            var envFile = args.Length > 0 ? args[0] : Path.Combine(cwd, "sample.env");
            AnsiConsole.MarkupLine($"[grey]Env file path:[/] [italic]{envFile}[/]");
            AnsiConsole.MarkupLine($"[grey]Env file exists:[/] [bold yellow]{File.Exists(envFile)}[/]");
            if (File.Exists(envFile))
            {
                DotEnv.Load(options: new DotEnvOptions(envFilePaths: [envFile]));
                AnsiConsole.MarkupLine("[green]✔[/] Loaded env file at [italic]{0}[/]", envFile);
            }
            else
            {
                AnsiConsole.MarkupLine("[yellow]⚠[/] Env file not found at [italic]{0}[/]", envFile);
            }
            // DEBUG: Print SNAPDOG_ZONES at startup
            var snapdogZonesRaw = Environment.GetEnvironmentVariable("SNAPDOG_ZONES");
            AnsiConsole.MarkupLine($"[bold yellow]SNAPDOG_ZONES:[/] [italic]{snapdogZonesRaw ?? "<not set>"}[/]");
            // Print all environment variables after loading
            // AnsiConsole.MarkupLine("[grey]All Environment Variables (after loading):[/]");
            // foreach (System.Collections.DictionaryEntry entry in Environment.GetEnvironmentVariables())
            // {
            //     AnsiConsole.MarkupLine(
            //         $"[blue]{Markup.Escape(entry.Key.ToString())}[/] = [white]{Markup.Escape(entry.Value?.ToString() ?? "")}[/]"
            //     );
            // }

            EnvConfig.GlobalPrefix = "SNAPDOG_";
            AnsiConsole.MarkupLine($"[grey]Global Prefix:[/] [bold yellow]{EnvConfig.GlobalPrefix}[/]");

            var config = EnvConfig.Load<SampleConfig>(logger: new SpectreConsoleLogSink()); // Central static API

            // Helper for table output
            void PrintConfigTable(string title, params (string EnvVar, string Type, string Value)[] rows)
            {
                var table = new Table()
                    .Border(TableBorder.Rounded)
                    .AddColumn("ENV Variable")
                    .AddColumn("Type")
                    .AddColumn("Value");
                foreach (var row in rows)
                {
                    table.AddRow(row.EnvVar, row.Type, row.Value);
                }

                AnsiConsole.Write(new Rule($"[yellow]{title}[/]"));
                AnsiConsole.Write(table);
            }

            var prefix = EnvConfig.GlobalPrefix;
            var props = typeof(SampleConfig).GetProperties();

            PrintConfigTable(
                "System Configuration",
                (
                    prefix + GetEnvKey(props, nameof(SampleConfig.LogLevel)),
                    props.First(p => p.Name == nameof(SampleConfig.LogLevel)).PropertyType.Name,
                    config.LogLevel ?? "<unset>"
                ),
                (
                    prefix + GetEnvKey(props, nameof(SampleConfig.Env)),
                    props.First(p => p.Name == nameof(SampleConfig.Env)).PropertyType.Name,
                    config.Env ?? "<unset>"
                )
            );

            PrintConfigTable(
                "Telemetry Configuration",
                (
                    prefix + GetEnvKey(props, nameof(SampleConfig.TelemetryEnabled)),
                    props.First(p => p.Name == nameof(SampleConfig.TelemetryEnabled)).PropertyType.Name,
                    config.TelemetryEnabled.ToString()
                ),
                (
                    prefix + GetEnvKey(props, nameof(SampleConfig.TelemetryServiceName)),
                    props.First(p => p.Name == nameof(SampleConfig.TelemetryServiceName)).PropertyType.Name,
                    config.TelemetryServiceName ?? "<unset>"
                ),
                (
                    prefix + GetEnvKey(props, nameof(SampleConfig.TelemetrySamplingRate)),
                    props.First(p => p.Name == nameof(SampleConfig.TelemetrySamplingRate)).PropertyType.Name,
                    config.TelemetrySamplingRate.ToString()
                )
            );

            PrintConfigTable(
                "Prometheus Configuration",
                (
                    prefix + GetEnvKey(props, nameof(SampleConfig.PrometheusEnabled)),
                    props.First(p => p.Name == nameof(SampleConfig.PrometheusEnabled)).PropertyType.Name,
                    config.PrometheusEnabled.ToString()
                ),
                (
                    prefix + GetEnvKey(props, nameof(SampleConfig.PrometheusPath)),
                    props.First(p => p.Name == nameof(SampleConfig.PrometheusPath)).PropertyType.Name,
                    config.PrometheusPath ?? "<unset>"
                ),
                (
                    prefix + GetEnvKey(props, nameof(SampleConfig.PrometheusPort)),
                    props.First(p => p.Name == nameof(SampleConfig.PrometheusPort)).PropertyType.Name,
                    config.PrometheusPort.ToString()
                )
            );

            PrintConfigTable(
                "Jaeger Configuration",
                (
                    prefix + GetEnvKey(props, nameof(SampleConfig.JaegerEnabled)),
                    props.First(p => p.Name == nameof(SampleConfig.JaegerEnabled)).PropertyType.Name,
                    config.JaegerEnabled.ToString()
                ),
                (
                    prefix + GetEnvKey(props, nameof(SampleConfig.JaegerEndpoint)),
                    props.First(p => p.Name == nameof(SampleConfig.JaegerEndpoint)).PropertyType.Name,
                    config.JaegerEndpoint ?? "<unset>"
                ),
                (
                    prefix + GetEnvKey(props, nameof(SampleConfig.JaegerAgentHost)),
                    props.First(p => p.Name == nameof(SampleConfig.JaegerAgentHost)).PropertyType.Name,
                    config.JaegerAgentHost ?? "<unset>"
                ),
                (
                    prefix + GetEnvKey(props, nameof(SampleConfig.JaegerAgentPort)),
                    props.First(p => p.Name == nameof(SampleConfig.JaegerAgentPort)).PropertyType.Name,
                    config.JaegerAgentPort.ToString()
                )
            );

            var apiRows = new List<(string EnvVar, string Type, string Value)>();
            apiRows.Add(
                (
                    prefix + GetEnvKey(props, nameof(SampleConfig.ApiAuthEnabled)),
                    props.First(p => p.Name == nameof(SampleConfig.ApiAuthEnabled)).PropertyType.Name,
                    config.ApiAuthEnabled.ToString()
                )
            );
            // Debug output: show count and values of ApiKeys
            AnsiConsole.MarkupLine($"[grey]ApiKeys count:[/] [bold yellow]{config.ApiKeys.Count}[/]");
            foreach (var key in config.ApiKeys)
            {
                AnsiConsole.MarkupLine($"[grey]ApiKey:[/] [white]{Markup.Escape(key)}[/]");
            }
            int apiKeyIndex = 1;
            foreach (var apiKey in config.ApiKeys)
            {
                apiRows.Add((prefix + $"API_APIKEY_{apiKeyIndex}", typeof(string).Name, apiKey));
                apiKeyIndex++;
            }
            PrintConfigTable("API Configuration", apiRows.ToArray());

            var zonesRows = new List<(string EnvVar, string Type, string Value)>();
            zonesRows.Add(
                (
                    prefix + GetEnvKey(props, nameof(SampleConfig.Zones)),
                    props.First(p => p.Name == nameof(SampleConfig.Zones)).PropertyType.Name,
                    config.Zones.Count > 0 ? string.Join(",", config.Zones) : "<unset>"
                )
            );
            PrintConfigTable("Zone Configuration", zonesRows.ToArray());

            // Helper to get Env attribute key
            static string GetEnvKey(System.Reflection.PropertyInfo[] props, string propName)
            {
                var prop = props.First(p => p.Name == propName);
                var attr = (EnvoyConfig.Attributes.EnvAttribute?)
                    Attribute.GetCustomAttribute(prop, typeof(EnvoyConfig.Attributes.EnvAttribute));
                return attr?.Key?.Replace(EnvConfig.GlobalPrefix, "") ?? propName.ToUpper();
            }

            AnsiConsole.Write(new Rule("[green]✔[/] Done!"));
        }
    }
}
