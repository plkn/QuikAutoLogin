# QuikAutoLogin

QuikAutoLogin is a Windows-specific .NET 7.0 console application that automatically monitors and logs into QUIK trading platform instances. It uses Windows UI Automation, process monitoring, and hosted services to detect QUIK startup and perform automatic authentication.

Always reference these instructions first and fallback to search or bash commands only when you encounter unexpected information that does not match the info here.

## Working Effectively

### Prerequisites and Environment
- **Windows ONLY**: This application uses Windows-specific APIs (Windows Forms, WPF, UI Automation, System.Management) and CANNOT be built or run on Linux/macOS.
- **Administrator privileges**: Required due to process monitoring and UI automation (specified in app.manifest).
- **.NET 7.0 Windows Desktop workload**: Must be installed for building and running.
- **QUIK trading platform**: Required for full functionality testing (optional for build validation).

### Build and Development Commands
- **Restore dependencies**: `dotnet restore` -- takes 45 seconds. NEVER CANCEL. Set timeout to 90+ seconds.
- **Build** (Windows only): `dotnet build --no-restore` -- takes ~2 minutes on Windows. NEVER CANCEL. Set timeout to 5+ minutes.
- **Run** (Windows only, requires admin): `dotnet run --no-build` -- application runs continuously until stopped.

### Cross-Platform Limitations
- **Linux/macOS builds FAIL** with error: "Microsoft.NET.Sdk.WindowsDesktop.targets was not found"
- **Only `dotnet restore` works** on non-Windows platforms (for dependency analysis)
- **CI/CD**: GitHub Actions workflow (.github/workflows/dotnet.yml) fails on ubuntu-latest due to Windows dependencies
- **Development**: Use Windows development environment or Windows containers for full functionality

## Validation

### Build Validation (Windows Required)
Always validate builds on Windows environment:
1. `dotnet restore` -- 45 seconds initially, NEVER CANCEL
2. `dotnet build --no-restore` -- 2 minutes, NEVER CANCEL  
3. Check for warnings about .NET 7.0 being out of support (expected)
4. Verify output in `AutoLoginTest/bin/Debug/net7.0-windows/`

**Linux/macOS validation** (limited):
- `dotnet sln list` -- verify solution structure
- `dotnet restore` -- test dependency resolution (45s initial, 1s cached)
- `dotnet build` -- will fail with "WindowsDesktop.targets not found" (expected)

### Configuration Validation (All Platforms)
Validate appsettings.json structure:
1. Check `appsettings.json` has valid JSON syntax: `python3 -c "import json; print('✓ Valid JSON' if json.load(open('AutoLoginTest/appsettings.json', encoding='utf-8-sig')) else '✗ Invalid JSON')"`
2. Verify `AutoLogin` array contains objects with: `ExePath`, `Login`, `Password`
3. Ensure `Serilog` configuration is properly formatted
4. Note: File uses UTF-8 with BOM encoding (common in Windows development)

### Runtime Validation (Windows + Admin Required)
Full functionality testing requires Windows with QUIK installed:
1. **NEVER run without administrator privileges** - will fail with permission errors
2. **Basic service startup**: Run without QUIK running to test service initialization
3. **QUIK integration**: Install QUIK demo version for complete testing
4. **Log validation**: Check `Logs/log.txt` for proper service startup and error handling (directory created at runtime)
5. **Process monitoring**: Verify ProcessesMonitor service detects new processes

### Validation Scenarios
When making changes, test these scenarios:
- **Service startup**: Application starts and hosted services initialize properly
- **Configuration loading**: Settings load correctly from appsettings.json  
- **Logging**: Serilog writes to both console and file correctly
- **Process monitoring**: ProcessesMonitor service starts and logs process events
- **UI automation**: AutoLoginWorker service starts (requires QUIK for full testing)

## Common Tasks

### Repository Structure
```
QuikAutoLogin/
├── .github/workflows/dotnet.yml    # CI/CD (has deprecated actions)
├── .gitignore                      # Build artifacts exclusions
├── AutoLoginTest.sln              # Solution file
└── AutoLoginTest/                 # Main project
    ├── AutoLoginTest.csproj       # Project file (.NET 7.0-windows)
    ├── Program.cs                 # Application entry point
    ├── AutoLoginWorker.cs         # QUIK UI automation service
    ├── ProcessesMonitor.cs        # Process monitoring service  
    ├── appsettings.json          # Configuration file
    ├── app.manifest              # Admin privileges requirement
    └── Config/                   # Configuration models
        ├── AutoLoginConfiguration.cs
        └── QuikConfiguration.cs
```

### Key Configuration Files

#### appsettings.json Structure
```json
{
  "Serilog": {
    "MinimumLevel": "Debug",
    "WriteTo": [
      { "Name": "Console" },
      { "Name": "File", "Args": { "path": "Logs/log.txt" } }
    ]
  },
  "AutoLogin": [
    {
      "ExePath": "c:\\quik\\info.exe",
      "Login": "test_user", 
      "Password": "test_password"
    }
  ]
}
```

### Dependencies and Packages
- **Microsoft.Extensions.Hosting**: 7.0.1 (hosted services)
- **Serilog**: 3.0.1 (logging framework)
- **Serilog.Extensions**: 3.4.2 (configuration integration)
- **Serilog.Settings.Configuration**: 7.0.1 (appsettings.json binding)
- **System.Management**: 7.0.2 (Windows process monitoring)
- **Windows Forms/WPF**: Built-in (UI automation)

### Known Issues and Limitations
- **GitHub Actions**: Uses deprecated `actions/upload-artifact@v3.1.3` (should upgrade to v4)
- **.NET 7.0**: Out of support, shows warnings during build (expected)
- **Platform limitation**: Windows-only due to UI Automation and System.Management dependencies
- **Privilege requirement**: Must run as administrator for process monitoring and UI automation
- **QUIK dependency**: Full testing requires QUIK trading platform installation

### Build Troubleshooting
- **Linux/macOS error**: "WindowsDesktop.targets not found" - expected, use Windows environment
- **Permission errors**: Run terminal/IDE as administrator on Windows
- **Missing workloads**: Install .NET Windows Desktop workload via Visual Studio installer
- **Restore timeout**: Extend timeout to 90+ seconds for `dotnet restore`
- **Build timeout**: Extend timeout to 5+ minutes for `dotnet build` on Windows

### Development Workflow
1. **Windows development environment required** for full development cycle
2. **Always run as administrator** when testing runtime functionality  
3. **Configuration first**: Set up appsettings.json before running application
4. **Check logs**: Monitor `Logs/log.txt` and console output for debugging
5. **Service isolation**: Test individual hosted services for debugging
6. **QUIK simulation**: Use QUIK demo version for safe testing

### Timing Expectations
- **dotnet restore**: 45 seconds (NEVER CANCEL, timeout: 90+ seconds)
- **dotnet build**: 2 minutes on Windows (NEVER CANCEL, timeout: 5+ minutes)
- **Application startup**: 5-10 seconds for service initialization
- **Process monitoring**: Continuous background operation
- **QUIK detection**: Immediate when QUIK process starts

## Critical Reminders
- **WINDOWS ONLY**: Cannot build or run on Linux/macOS due to Windows-specific dependencies
- **ADMIN REQUIRED**: Must run with administrator privileges for proper functionality
- **NEVER CANCEL BUILDS**: dotnet restore takes 45s, build takes 2+ minutes on Windows
- **GitHub Actions broken**: CI/CD currently fails due to deprecated actions and Windows dependency conflicts
- **Configuration required**: Application needs valid appsettings.json to start properly

## Quick Command Reference

### Cross-Platform Commands (Linux/macOS/Windows)
```bash
dotnet sln list                    # List solution projects
dotnet restore                     # Restore dependencies (45s initial, NEVER CANCEL)
# JSON validation:
python3 -c "import json; print('✓ Valid JSON' if json.load(open('AutoLoginTest/appsettings.json', encoding='utf-8-sig')) else '✗ Invalid JSON')"
```

### Windows-Only Commands (Require Windows + Admin)
```bash
dotnet build --no-restore         # Build project (2+ minutes, NEVER CANCEL)
dotnet run --no-build             # Run application (continuous, requires admin)
```

### Expected Error on Linux/macOS
```
error MSB4019: WindowsDesktop.targets was not found
```
This is **expected behavior** - the application is Windows-only.