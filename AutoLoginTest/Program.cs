// https://stackoverflow.com/questions/40241044/detect-when-a-specific-window-in-another-process-opens-or-closes

using System.Diagnostics;
using System.Windows.Automation;
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
});

builder.Build().Run();

var quikProc = GetQuikProcess();
var quikMainWindow = AutomationElement.FromHandle(quikProc.MainWindowHandle);

Automation.AddAutomationEventHandler(
    WindowPattern.WindowOpenedEvent, quikMainWindow,
    TreeScope.Subtree, (s, _) =>
    {

        var loginWindow = s as AutomationElement;
        if (loginWindow.Current.Name.Contains("пользователя"))
        {
            var inputsLookupCondition = new PropertyCondition(
                AutomationElement.ClassNameProperty, "Edit", PropertyConditionFlags.IgnoreCase);
            var inputs = loginWindow.FindAll(TreeScope.Element | TreeScope.Children, inputsLookupCondition);

            var buttonsLookupCondition = new PropertyCondition(
                AutomationElement.NameProperty, "Вход", PropertyConditionFlags.IgnoreCase);
            var loginButton = loginWindow.FindFirst(TreeScope.Element | TreeScope.Children, buttonsLookupCondition);

            var loginInput = inputs[0];
            var passInput = inputs[1];

            loginInput.SetFocus();
            SendKeys.SendWait("^{HOME}");
            SendKeys.SendWait("^+{END}"); // Select everything
            SendKeys.SendWait("{DEL}"); // Delete selection
            SendKeys.SendWait("test"); // Type new text

            passInput.SetFocus();
            SendKeys.SendWait("^{HOME}");
            SendKeys.SendWait("^+{END}"); // Select everything
            SendKeys.SendWait("{DEL}"); // Delete selection
            SendKeys.SendWait("test"); // Type new text

            loginButton.SetFocus();
            SendKeys.SendWait("{ENTER}"); // Press Enter key
        }
    });


Console.ReadLine();
return;

Process? GetQuikProcess()
{
    var processes = Process.GetProcesses();
    foreach (var p in processes)
    {
        if (p.MainWindowTitle.Contains("QUIK"))
        {
            return p;
        }
    }

    return null;
}