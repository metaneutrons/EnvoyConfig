namespace EnvoyConfig.Sample;

using EnvoyConfig.Abstractions;
using Spectre.Console;

public class SpectreConsoleLogSink : IEnvLogSink
{
    public void Log(EnvLogLevel level, string message, Exception? ex = null)
    {
        var color = level switch
        {
            EnvLogLevel.Critical or EnvLogLevel.Error => "red",
            EnvLogLevel.Warning => "yellow",
            EnvLogLevel.Debug => "grey37",
            _ => "white",
        };
        // Escape brackets in message to avoid Spectre.Console style errors
        var safeMessage = message.Replace("[", "[[").Replace("]", "]]");
        AnsiConsole.MarkupLine($"[{color}]{level}[/]: {safeMessage}");
        if (ex != null)
        {
            AnsiConsole.WriteException(ex, ExceptionFormats.ShortenEverything);
        }
    }
}
