using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace AutoLoginTest;

/// <summary>
/// Воркер, который отслеживает появление запущенных процессов квика.
/// </summary>
public class ProcessesMonitor : IHostedService
{
    private readonly ILogger<ProcessesMonitor> _logger;

    public ProcessesMonitor(ILogger<ProcessesMonitor> logger)
    {
        _logger = logger;
    }

    public async Task StartAsync(CancellationToken cancellationToken)
    {
        _logger.LogInformation("{ServiceName} started", nameof(ProcessesMonitor));
    }

    public async Task StopAsync(CancellationToken cancellationToken)
    {
        _logger.LogInformation("{ServiceName} stopped", nameof(ProcessesMonitor));
    }
}