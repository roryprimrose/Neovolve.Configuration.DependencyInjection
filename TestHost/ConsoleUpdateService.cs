namespace TestHost;

public abstract class ConsoleUpdateService : TimerHostedService
{
    private static readonly object _syncLock = new();
    private static int _serviceCount;
    private readonly int _line;

    protected ConsoleUpdateService()
        : this(TimeSpan.FromMilliseconds(500))
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
        var output = GetType() + ": " + text + new string(' ', 50);

        lock (_syncLock)
        {
            var position = Console.GetCursorPosition();

            //Console.SetCursorPosition(0, _line);
            //Console.WriteLine(output);
            //Console.SetCursorPosition(position.Left, position.Top);
        }

        return Task.CompletedTask;
    }
}