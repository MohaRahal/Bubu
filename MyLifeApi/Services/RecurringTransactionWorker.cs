namespace MyLifeApi.Services;

public sealed class RecurringTransactionWorker(
    IServiceScopeFactory scopeFactory,
    ILogger<RecurringTransactionWorker> logger) : BackgroundService
{
    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        await ProcessDueTransactions(stoppingToken);

        using var timer = new PeriodicTimer(TimeSpan.FromHours(6));
        while (await timer.WaitForNextTickAsync(stoppingToken))
            await ProcessDueTransactions(stoppingToken);
    }

    private async Task ProcessDueTransactions(CancellationToken cancellationToken)
    {
        try
        {
            await using var scope = scopeFactory.CreateAsyncScope();
            var processor = scope.ServiceProvider.GetRequiredService<RecurringTransactionProcessor>();
            await processor.ProcessAllDueAsync(cancellationToken);
        }
        catch (OperationCanceledException) when (cancellationToken.IsCancellationRequested)
        {
        }
        catch (Exception exception)
        {
            logger.LogError(exception, "Não foi possível processar as movimentações recorrentes.");
        }
    }
}
