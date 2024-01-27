namespace MoviesBE.Services.Common;

public abstract class BaseHostedService : IHostedService, IDisposable
{
    private readonly string _configSection;
    protected readonly IConfiguration Configuration;
    protected readonly ILogger Logger;
    protected bool IsEnabled;
    protected CancellationToken StoppingToken;
    protected Timer? Timer;

    protected BaseHostedService(IConfiguration configuration, ILogger logger, string configSection)
    {
        Configuration = configuration;
        Logger = logger;
        _configSection = configSection;
        IsEnabled = Configuration.GetSection(_configSection).GetValue<bool>("Enabled");
    }

    public virtual void Dispose()
    {
        Timer?.Dispose();
    }

    public virtual Task StartAsync(CancellationToken cancellationToken)
    {
        Logger.LogInformation($"{GetType().Name} is starting.");
        StoppingToken = cancellationToken;

        if (IsEnabled)
        {
            Logger.LogInformation($"{GetType().Name} is enabled and will run.");
            Timer = new Timer(ExecuteTask, null, TimeSpan.Zero, GetInterval());
        }
        else
        {
            Logger.LogInformation($"{GetType().Name} is disabled and will not run.");
        }

        return Task.CompletedTask;
    }

    public virtual Task StopAsync(CancellationToken cancellationToken)
    {
        Logger.LogInformation($"{GetType().Name} is stopping.");
        Timer?.Change(Timeout.Infinite, 0);
        return Task.CompletedTask;
    }

    protected abstract void ExecuteTask(object? state);
    protected abstract TimeSpan GetInterval();
}