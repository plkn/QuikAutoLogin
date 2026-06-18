using System.Diagnostics;
using System.Windows.Automation;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace AutoLoginTest;

/// <summary>
/// Воркер автоматического входа в QUIK. Отслеживает появление окон входа в систему QUIK и автоматически 
/// заполняет поля логина и пароля, а затем выполняет вход в систему.
/// </summary>
public class AutoLoginWorker : IHostedService
{
    private readonly ILogger<AutoLoginWorker> _logger;

    public AutoLoginWorker(ILogger<AutoLoginWorker> logger)
    {
        _logger = logger;
    }

    public Task StartAsync(CancellationToken cancellationToken)
    {
        var quikProc = GetQuikProcess();
        if (quikProc == null)
        {
            _logger.LogWarning("QUIK process not found");
            return Task.CompletedTask;
        }

        var quikMainWindow = AutomationElement.FromHandle(quikProc.MainWindowHandle);

        Automation.AddAutomationEventHandler(
            WindowPattern.WindowOpenedEvent,
            quikMainWindow,
            TreeScope.Subtree,
            (s, _) =>
            {
                var loginWindow = s as AutomationElement;
                if (loginWindow != null && loginWindow.Current.Name.Contains("пользователя"))
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
                    SendKeys.SendWait("^+{END}");
                    SendKeys.SendWait("{DEL}");
                    SendKeys.SendWait("test");

                    passInput.SetFocus();
                    SendKeys.SendWait("^{HOME}");
                    SendKeys.SendWait("^+{END}");
                    SendKeys.SendWait("{DEL}");
                    SendKeys.SendWait("test");

                    loginButton.SetFocus();
                    SendKeys.SendWait("{ENTER}");
                }
            });

        return Task.CompletedTask;
    }

    public Task StopAsync(CancellationToken cancellationToken)
    {
        Automation.RemoveAllEventHandlers();
        return Task.CompletedTask;
    }

    private Process? GetQuikProcess()
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
}
