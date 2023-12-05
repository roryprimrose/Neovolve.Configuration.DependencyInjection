namespace TestHost;

public abstract class ConsoleUpdateService : TimerHostedService
{
    private static readonly object _syncLock = new();
    private static int _serviceCount;
    private readonly int _line;

    protected ConsoleUpdateService()
        : this(TimeSpan.FromMilliseconds(3000))
    {
    }

    protected ConsoleUpdateService(TimeSpan delay)
        : base(delay)
    {
        lock (_syncLock)
        {
            _line = _serviceCount;
            _serviceCount++;
        }

        WriteValue("<pending>");
    }

    protected Task WriteValue(string text)
    {
        var output = GetType() + ": " + text;

        lock (_syncLock)
        {
            Console.WriteLine(output);
        }

        return Task.CompletedTask;
    }
}