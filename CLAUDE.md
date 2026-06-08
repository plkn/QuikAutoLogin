# CLAUDE.md

Guidance for AI assistants (Claude Code) working in this repository.

## What this project is

QuikAutoLogin (solution/project name `AutoLoginTest`) is a small Windows desktop
background service that automatically fills in and submits the login dialog of
the **QUIK** trading terminal (a Russian stock-trading application). It watches
for QUIK processes to start, waits for the "Идентификация пользователя"
(user identification) login window to appear, and uses UI Automation +
`SendKeys` to type the login/password and press the "Вход" (Login) button.

This is a **Windows-only** desktop app (`net7.0-windows`, WinForms + WPF UI
Automation) that must run with **administrator privileges** (see
`AutoLoginTest/app.manifest`, `requestedExecutionLevel="requireAdministrator"`).
It cannot be run or meaningfully tested on Linux/macOS — at best it can be
*built* cross-platform thanks to `EnableWindowsTargeting`.

## Repository layout

```
AutoLoginTest.sln                  # Visual Studio solution (single project)
AutoLoginTest/
  Program.cs                       # Entry point — builds & runs a Generic Host
  AutoLoginWorker.cs               # IHostedService: detects QUIK login window, types credentials
  ProcessesMonitor.cs              # IHostedService: logs OS process start/stop via WMI
  Config/
    AutoLoginConfiguration.cs      # List<QuikConfiguration>, bound from "AutoLogin" config section
    QuikConfiguration.cs           # record(ExePath, Login, Password) — one QUIK instance to auto-login
  appsettings.json                 # Logging (Serilog) + AutoLogin entries (exe path, login, password)
  app.manifest                     # Forces requireAdministrator execution level
  AutoLoginTest.csproj
.github/workflows/dotnet.yml       # CI: restore/build/test/upload-artifact on push/PR to master
```

There are no test projects in the solution despite the `dotnet test` step in CI
and the `AutoLoginTest` name — that name refers to the *app* ("a test/prototype
of auto-login"), not a test suite.

## Architecture

The app is a .NET Generic Host (`Host.CreateDefaultBuilder`) with two
`IHostedService` implementations registered in `Program.cs`:

1. **`ProcessesMonitor`** — subscribes to WMI events
   (`Win32_ProcessStartTrace` / `Win32_ProcessStopTrace` via `System.Management`)
   and logs every process start/stop on the machine. Currently it only logs;
   it does not yet trigger anything in `AutoLoginWorker` (see "Known
   gaps" below).
2. **`AutoLoginWorker`** — on startup, finds a running process whose
   `MainWindowTitle` contains `"QUIK"`, then registers a UI Automation
   `WindowPattern.WindowOpenedEvent` handler on that window's subtree. When a
   window whose name contains `"пользователя"` appears, it locates the two
   `Edit` controls (login + password) and the `"Вход"` button, focuses each
   field, clears it (`Ctrl+Home`, `Ctrl+Shift+End`, `Delete`), types the
   credentials via `SendKeys.SendWait`, and presses Enter on the login button.

Configuration binding: `AutoLoginConfiguration` (a `List<QuikConfiguration>`)
is bound from the `"AutoLogin"` section of `appsettings.json` via
`services.AddOptions<AutoLoginConfiguration>().Bind(...)`.

Logging uses **Serilog**, configured entirely from `appsettings.json`
(`Serilog` section) with Console and rolling File sinks (`Logs/log.txt`),
enriched with machine name / thread id / log context.

## Known gaps / things to be careful about when changing code

- **Hardcoded credentials**: `AutoLoginWorker` currently sends the literal
  strings `"test"` / `"test"` instead of reading `Login`/`Password` from
  `QuikConfiguration`/`AutoLoginConfiguration` (it doesn't even inject the
  config). If asked to "fix" or "wire up" auto-login, this is almost certainly
  the place to use the bound configuration instead of hardcoded values.
- **`ProcessesMonitor` and `AutoLoginWorker` are not connected**: the monitor
  only logs process start/stop; `AutoLoginWorker` independently polls
  `Process.GetProcesses()` once at `StartAsync`. A more robust design would
  have the monitor notify the worker when a configured QUIK exe starts (and
  clean up automation handlers when it stops), and support multiple QUIK
  instances per `AutoLoginConfiguration` entries (only one is currently
  handled).
- `appsettings.json` in source control contains placeholder credentials
  (`"my login"` / `"my pass"`) and a sample path
  (`c:\quik\info.exe`) — treat any real paths/credentials a user adds locally
  as sensitive; never commit real values.
- Strings/comments mixing Russian (UI element names like `"пользователя"`,
  `"Вход"`, XML-doc comments in `ProcessesMonitor`/`Config` classes) and
  English are intentional — QUIK's UI is Russian, so window/control name
  matches must stay in Russian. Keep this convention when adding similar
  lookups.

## Build / run / test

This is a Windows-targeted project; building requires the .NET 7 SDK
(`net7.0-windows`, WPF + WinForms via `Microsoft.WindowsDesktop.App.WPF` and
`System.Windows.Forms`). It can be *restored and built* on Linux thanks to
`<EnableWindowsTargeting>true</EnableWindowsTargeting>`, but **cannot be run**
outside Windows (it depends on `System.Windows.Automation`, `SendKeys`,
`System.Management`/WMI, and process window inspection).

```bash
dotnet restore
dotnet build
```

Running/testing the actual auto-login behavior requires a Windows machine with
QUIK installed, and the app must be launched **as Administrator**. There is no
automated test suite — `dotnet test` in CI currently has nothing to run.

## CI

`.github/workflows/dotnet.yml` runs on push/PR to `master`: restore → build →
`dotnet test` → upload build output as an artifact named `quikautologin`. It
runs on `ubuntu-latest` (cross-compiling thanks to `EnableWindowsTargeting`).

## Conventions

- Target framework: `net7.0-windows`; nullable reference types and implicit
  usings are enabled (`<Nullable>enable</Nullable>`,
  `<ImplicitUsings>enable</ImplicitUsings>`).
- Dependency injection / hosting via `Microsoft.Extensions.Hosting`; new
  long-running components should be added as `IHostedService` and registered
  in `Program.cs`.
- Logging goes through `ILogger<T>` (Serilog backend) — use structured
  logging with named placeholders (e.g. `_logger.LogInformation("Process
  started: {ProcessName}", ...)`), matching the existing style.
- Configuration is POCO/record-based and bound via `IOptions<T>` from
  `appsettings.json`; follow the existing `record` pattern for simple config
  shapes (see `QuikConfiguration`).
- XML-doc summary comments on classes/methods are written in Russian,
  matching the existing files in `Config/` and `ProcessesMonitor.cs`.
