namespace TestHost
{
    using Microsoft.Extensions.Hosting;

    public abstract class TimerHostedService : BackgroundService
    {
        // Could also be a async method, that can be awaited in ExecuteAsync above
        protected abstract Task DoWork(CancellationToken cancellationToken);

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            // When the timer should have no due-time, then do the work once now.
            await DoWork(stoppingToken).ConfigureAwait(false);

            using PeriodicTimer timer = new(TimeSpan.FromSeconds(5));

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
}