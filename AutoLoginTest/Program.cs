// https://stackoverflow.com/questions/40241044/detect-when-a-specific-window-in-another-process-opens-or-closes

using System.Runtime.InteropServices;
using System.Text;
using System.Windows.Automation;

const uint EVENT_SYSTEM_FOREGROUND = 0x0003;
const uint EVENT_OBJECT_DESTROY = 0x8001;
const uint WINEVENT_OUTOFCONTEXT = 0;

[DllImport("user32.dll")]
static extern IntPtr SetWinEventHook(uint eventMin, uint eventMax, IntPtr
        hmodWinEventProc, WinEventDelegate lpfnWinEventProc, uint idProcess,
    uint idThread, uint dwFlags);

[DllImport("user32.dll")]
static extern bool UnhookWinEvent(IntPtr hWinEventHook);

[DllImport("user32.dll", CharSet = CharSet.Auto, SetLastError = true)]
static extern int GetWindowText(IntPtr hWnd, StringBuilder lpString, int nMaxCount);

System.Diagnostics.Process[] processes = System.Diagnostics.Process.GetProcesses();
foreach (System.Diagnostics.Process p in processes)
{
    if (p.MainWindowTitle.Contains("QUIK"))
    {
        var quikElement = AutomationElement.FromHandle(p.MainWindowHandle);
        Automation.AddAutomationEventHandler(
            WindowPattern.WindowOpenedEvent, quikElement,
            TreeScope.Subtree, (s1, e1) =>
            {
                var element = s1 as AutomationElement;
                if (element.Current.Name.Contains("пользователя"))
                {
                    //Page setup opened.
                    // this.Invoke(new Action(() => { this.Text = "Page Setup Opened"; }));
                    // Automation.AddAutomationEventHandler(
                    //     WindowPattern.WindowClosedEvent, element,
                    //     TreeScope.Subtree, (s2, e2) =>
                    //     {
                    //         //Page setup closed.
                    //         this.Invoke(new Action(() => { this.Text = "Closed"; }));
                    //     });
                }
            });

        break;
    }
}

Console.ReadLine();

delegate void WinEventDelegate(IntPtr hWinEventHook, uint eventType, IntPtr hwnd,
    int idObject, int idChild, uint dwEventThread, uint dwmsEventTime);