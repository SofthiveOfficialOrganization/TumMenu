using Serilog;

namespace WebAPI.Logging.Providers;

public abstract class LoggerServiceBase
{
    protected Serilog.ILogger Logger { get; set; }

    protected LoggerServiceBase()
    {
        Logger = null!;
    }

    protected LoggerServiceBase(Serilog.ILogger logger)
    {
        Logger = logger;
    }

    /// <summary>
    /// Writes a verbose log message.
    /// </summary>
    /// <param name="message">The message to be logged.</param>
    public void Verbose(string message) => Logger.Verbose(message);

    /// <summary>
    /// Writes a fatal log message.
    /// </summary>
    /// <param name="message">The message to be logged.</param>
    public void Fatal(string message) => Logger.Fatal(message);

    /// <summary>
    /// Writes an information log message.
    /// </summary>
    /// <param name="message">The message to be logged.</param>
    public void Info(string message) => Logger.Information(message);

    /// <summary>
    /// Writes a warning log message.
    /// </summary>
    /// <param name="message">The message to be logged.</param>
    public void Warn(string message) => Logger.Warning(message);

    /// <summary>
    /// Writes a debug log message.
    /// </summary>
    /// <param name="message">The message to be logged.</param>
    public void Debug(string message) => Logger.Debug(message);

    /// <summary>
    /// Writes an error log message.
    /// </summary>
    /// <param name="message">The message to be logged.</param>
    public void Error(string message) => Logger.Error(message);
}