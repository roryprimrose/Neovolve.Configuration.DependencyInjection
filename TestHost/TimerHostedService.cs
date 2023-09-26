namespace TestHost;

using Microsoft.Extensions.Hosting;

public abstract class TimerHostedService : BackgroundService
{
    private readonly TimeSpan _delay;

    protected TimerHostedService(TimeSpan delay)
    {
        _delay = delay;
    }

    protected abstract Task DoWork(CancellationToken cancellationToken);

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        await DoWork(stoppingToken).ConfigureAwait(false);

        using PeriodicTimer timer = new(_delay);

        try
        {
            while (await timer.WaitForNextTickAsync(stoppingToken).ConfigureAwait(false))
            {
                await DoWork(stoppingToken).ConfigureAwait(false);
            }
        }
        catch (OperationCanceledException)
        {
            Console.WriteLine(GetType() + " has been cancelled");
        }
        catch (Exception ex)
        {
            Console.WriteLine(ex.ToString());
        }
    }
}