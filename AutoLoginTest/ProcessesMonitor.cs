using AutoLoginTest.Config;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using System.Management;

namespace AutoLoginTest;

/// <summary>
/// Воркер, который отслеживает появление запущенных процессов квика. При запуске одного из квиков, путь к exe
/// которого прописан в конфиге, приложение начинает пытаться залогинить его.
/// </summary>
public class ProcessesMonitor : IHostedService
{
    private readonly AutoLoginConfiguration _config;
    private readonly ILogger<ProcessesMonitor> _logger;
    private ManagementEventWatcher _startProcessWatcher;
    private ManagementEventWatcher _stopProcessWatcher;

    public ProcessesMonitor(IOptions<AutoLoginConfiguration> config, ILogger<ProcessesMonitor> logger)
    {
        _config = config.Value;
        _logger = logger;
    }

    public async Task StartAsync(CancellationToken cancellationToken)
    {
        _logger.LogInformation("{ServiceName} starting", nameof(ProcessesMonitor));

        try
        {
            _startProcessWatcher = new ManagementEventWatcher(new WqlEventQuery("SELECT * FROM Win32_ProcessStartTrace"));
            _startProcessWatcher.EventArrived += ProcessStartedHandler;
            _startProcessWatcher.Start();

            _stopProcessWatcher = new ManagementEventWatcher(new WqlEventQuery("SELECT * FROM Win32_ProcessStopTrace"));
            _stopProcessWatcher.EventArrived += ProcessStoppedHandler;
            _stopProcessWatcher.Start();
        }
        catch (Exception e)
        {
            _logger.LogCritical(e, "Failed to start {ServiceName}: {Message}", nameof(ProcessesMonitor), e.Message);
        }

        _logger.LogInformation("{ServiceName} started", nameof(ProcessesMonitor));
    }

    public async Task StopAsync(CancellationToken cancellationToken)
    {
        _logger.LogInformation("{ServiceName} stopping", nameof(ProcessesMonitor));
        try
        {
            _startProcessWatcher.Stop();
            _stopProcessWatcher.Stop();
        }
        catch (Exception e)
        {
            _logger.LogError(e, "Exception while stopping {ServiceName}", nameof(ProcessesMonitor));
        }
        _logger.LogInformation("{ServiceName} stopped", nameof(ProcessesMonitor));
    }

    /// <summary>
    /// Обрабочик события запуска процесса в системе.
    /// </summary>
    /// <param name="sender"></param>
    /// <param name="e"></param>
    private void ProcessStartedHandler(object sender, EventArrivedEventArgs e) {
        _logger.LogInformation("Process started: {ProcessName}", e.NewEvent.Properties["ProcessName"].Value);
    }

    /// <summary>
    /// Обработка события остановки процесса в системе.
    /// </summary>
    private void ProcessStoppedHandler(object sender, EventArrivedEventArgs e) {
        _logger.LogInformation("Process stopped: {ProcessName}", e.NewEvent.Properties["ProcessName"].Value);
    }
}