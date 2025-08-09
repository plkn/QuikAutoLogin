// https://stackoverflow.com/questions/40241044/detect-when-a-specific-window-in-another-process-opens-or-closes

using AutoLoginTest;
using AutoLoginTest.Config;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Serilog;

var builder = Host.CreateDefaultBuilder(args);

builder.ConfigureAppConfiguration((_, configurationBuilder) =>
{
    configurationBuilder.AddJsonFile("appsettings.json", optional: false, reloadOnChange: true);
});

builder.ConfigureLogging((context, loggingBuilder) =>
{
    Log.Logger = new LoggerConfiguration()
        .ReadFrom.Configuration(context.Configuration)
        .CreateLogger();
    loggingBuilder.ClearProviders();
    loggingBuilder.AddSerilog();
});

builder.ConfigureServices((context, services) =>
{
    services.AddOptions<AutoLoginConfiguration>().Bind(context.Configuration.GetSection("AutoLogin"));
    services.AddHostedService<ProcessesMonitor>();
    services.AddHostedService<AutoLoginWorker>();
});

builder.Build().Run();
