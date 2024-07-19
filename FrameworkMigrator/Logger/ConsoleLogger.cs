using Microsoft.Extensions.Logging;

namespace FrameworkMigrator.Logger
{
    public class ConsoleLogger : ILogger
    {
        public IDisposable? BeginScope<TState>(TState state) where TState : notnull
        {
            return null;
        }

        public bool IsEnabled(LogLevel logLevel) => true;

        public void Log<TState>(LogLevel logLevel, EventId eventId, TState state, Exception? exception, Func<TState, Exception?, string> formatter)
        {
            Console.ForegroundColor = logLevel switch
            {
                LogLevel.Information => ConsoleColor.Blue,
                LogLevel.Error => ConsoleColor.Red,
                LogLevel.Warning => ConsoleColor.Yellow,
                _ => ConsoleColor.White
            };

            Console.WriteLine($"[{DateTime.Now:HH:mm:ss}] [{logLevel}] {formatter(state, exception)}");
        }
    }
}
