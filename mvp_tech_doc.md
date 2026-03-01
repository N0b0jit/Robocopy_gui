# MVP Technical Documentation
## RoboCopy GUI — v1.0 Build Guide

**Version:** 1.0  
**Date:** March 2026  
**Stack:** C# 12 / WPF / .NET 8 / Windows 10+

---

## 1. MVP Scope

The MVP delivers the core value proposition: a working GUI over robocopy with context menu integration. Every feature below must ship in v1.0. Nothing else is required.

### MVP Feature Checklist

| Feature | Priority | Status |
|---|---|---|
| Source / Destination folder picker | P0 | Planned |
| Copy mode selector (Copy/Move/Mirror) | P0 | Planned |
| Multi-thread option + thread slider | P0 | Planned |
| Restartable mode toggle | P0 | Planned |
| Include subdirectories toggle | P0 | Planned |
| Real-time progress bar | P0 | Planned |
| Live log output viewer | P0 | Planned |
| Start / Cancel controls | P0 | Planned |
| Context menu registration (Copy To / Move To / Mirror To) | P0 | Planned |
| Robocopy command preview | P1 | Planned |
| Save/load profiles | P1 | Planned |
| Job history log | P1 | Planned |
| Pause/Resume | P2 | Planned |

---

## 2. Project Setup

### 2.1 Prerequisites
- Visual Studio 2022+ or JetBrains Rider
- .NET 8 SDK
- Windows 10/11 development machine
- WiX Toolset v4 (for installer)

### 2.2 Solution Structure

```
RoboCopyGUI.sln
├── src/
│   ├── RoboCopyGUI.App/              ← WPF Application (entry point)
│   │   ├── App.xaml / App.xaml.cs
│   │   ├── Views/
│   │   ├── ViewModels/
│   │   ├── Styles/
│   │   └── Assets/
│   ├── RoboCopyGUI.Core/             ← Domain models, interfaces
│   │   ├── Models/
│   │   └── Interfaces/
│   ├── RoboCopyGUI.Services/         ← Business logic
│   │   ├── RobocopyService.cs
│   │   ├── CommandBuilder.cs
│   │   ├── ProfileService.cs
│   │   └── LogService.cs
│   └── RoboCopyGUI.Shell/            ← Context menu & registry helpers
│       └── ContextMenuInstaller.cs
├── tests/
│   ├── RoboCopyGUI.Core.Tests/
│   └── RoboCopyGUI.Services.Tests/
└── installer/
    └── RoboCopyGUI.Installer/        ← WiX project
```

### 2.3 NuGet Dependencies

```xml
<!-- RoboCopyGUI.App -->
<PackageReference Include="CommunityToolkit.Mvvm" Version="8.2.*" />
<PackageReference Include="ModernWpfUI" Version="0.9.*" />
<PackageReference Include="Microsoft.Extensions.DependencyInjection" Version="8.*" />

<!-- RoboCopyGUI.Services -->
<PackageReference Include="Serilog" Version="3.*" />
<PackageReference Include="Serilog.Sinks.File" Version="5.*" />

<!-- Tests -->
<PackageReference Include="xunit" Version="2.*" />
<PackageReference Include="FluentAssertions" Version="6.*" />
<PackageReference Include="NSubstitute" Version="5.*" />
```

---

## 3. Key Implementation Details

### 3.1 CommandBuilder — Robocopy Arg Construction

```csharp
// RoboCopyGUI.Services/CommandBuilder.cs
public static class CommandBuilder
{
    public static string Build(CopyOptions opts)
    {
        var args = new StringBuilder();

        // Source and destination (always quoted)
        args.Append($"\"{opts.Source}\" \"{opts.Destination}\"");

        // Copy flags
        if (opts.IncludeSubdirectories) 
            args.Append(opts.IncludeEmpty ? " /e" : " /s");
        
        if (opts.RestartableMode)   args.Append(" /z");
        if (opts.BackupMode)        args.Append(" /b");
        if (opts.MirrorMode)        args.Append(" /mir");
        if (opts.MoveFiles)         args.Append(" /move");

        // Multi-threading
        if (opts.MultiThreaded)
            args.Append($" /mt:{opts.ThreadCount}");

        // File selection
        if (opts.SkipNewerFiles)    args.Append(" /xo");
        if (opts.ExcludeHidden)     args.Append(" /xa:H");

        foreach (var f in opts.ExcludeFiles)
            args.Append($" /xf \"{f}\"");

        foreach (var d in opts.ExcludeDirectories)
            args.Append($" /xd \"{d}\"");

        // Logging (always log to file)
        args.Append($" /log+:\"{opts.LogFilePath}\"");
        args.Append(" /tee");   // also output to stdout for live streaming

        return args.ToString().Trim();
    }
}
```

### 3.2 RobocopyService — Process Execution & Streaming

```csharp
// RoboCopyGUI.Services/RobocopyService.cs
public class RobocopyService
{
    public event Action<CopyProgressUpdate>? ProgressUpdated;
    public event Action<string>? LogLineReceived;
    public event Action<int>? Completed; // robocopy exit code

    private Process? _process;

    public async Task ExecuteAsync(CopyOptions opts, CancellationToken ct)
    {
        var args = CommandBuilder.Build(opts);

        _process = new Process
        {
            StartInfo = new ProcessStartInfo
            {
                FileName = "robocopy.exe",
                Arguments = args,
                UseShellExecute = false,
                RedirectStandardOutput = true,
                RedirectStandardError = true,
                CreateNoWindow = true
            },
            EnableRaisingEvents = true
        };

        _process.OutputDataReceived += (_, e) =>
        {
            if (e.Data == null) return;
            LogLineReceived?.Invoke(e.Data);
            var progress = OutputParser.TryParse(e.Data);
            if (progress != null) ProgressUpdated?.Invoke(progress);
        };

        _process.Start();
        _process.BeginOutputReadLine();

        await _process.WaitForExitAsync(ct);
        Completed?.Invoke(_process.ExitCode);
    }

    public void Cancel() => _process?.Kill(entireProcessTree: true);
}
```

### 3.3 Robocopy Output Parser

Robocopy outputs lines like:
```
100%        New File           1.2 m        Documents\report.pdf
```

```csharp
public static class OutputParser
{
    // Regex: captures percent, status, size, filename
    private static readonly Regex ProgressLine = 
        new(@"^\s*(\d+\.?\d*)%\s+(\w[\w\s]*?)\s+([\d.]+\s*[kmgKMG]?)\s+(.+)$", 
            RegexOptions.Compiled);

    public static CopyProgressUpdate? TryParse(string line)
    {
        var match = ProgressLine.Match(line);
        if (!match.Success) return null;

        return new CopyProgressUpdate(
            FilePercent: double.Parse(match.Groups[1].Value),
            Status: match.Groups[2].Value.Trim(),
            FileName: match.Groups[4].Value.Trim()
        );
    }
}
```

### 3.4 Context Menu Installation

```csharp
// RoboCopyGUI.Shell/ContextMenuInstaller.cs
public static class ContextMenuInstaller
{
    private const string AppExePath = @"C:\Program Files\RoboCopyGUI\RoboCopyGUI.exe";

    public static void Install()
    {
        // Parent key
        var parentKey = @"Directory\shell\RoboCopyGUI";
        Registry.SetValue($@"HKEY_CLASSES_ROOT\{parentKey}", "", "RoboCopy Here →");
        Registry.SetValue($@"HKEY_CLASSES_ROOT\{parentKey}", "Icon", $"{AppExePath},0");
        Registry.SetValue($@"HKEY_CLASSES_ROOT\{parentKey}", "SubCommands", "");

        // Sub-commands
        RegisterSubCommand("CopyTo",   "📋 Copy To...",   "--mode copy --source \"%1\"");
        RegisterSubCommand("MoveTo",   "✂️ Move To...",   "--mode move --source \"%1\"");
        RegisterSubCommand("MirrorTo", "🔁 Mirror To...", "--mode mirror --source \"%1\"");
    }

    private static void RegisterSubCommand(string name, string label, string argTemplate)
    {
        var subKey = $@"Directory\shell\RoboCopyGUI\shell\{name}";
        Registry.SetValue($@"HKEY_CLASSES_ROOT\{subKey}", "", label);
        Registry.SetValue($@"HKEY_CLASSES_ROOT\{subKey}\command", "", 
            $"\"{AppExePath}\" {argTemplate}");
    }

    public static void Uninstall()
    {
        Registry.CurrentUser.DeleteSubKeyTree(@"Directory\shell\RoboCopyGUI", throwOnMissingSubKey: false);
    }
}
```

### 3.5 CLI Argument Parsing (App Startup)

```csharp
// App.xaml.cs
protected override void OnStartup(StartupEventArgs e)
{
    var args = e.Args;
    var launchOptions = CliParser.Parse(args);
    // e.g. --source "C:\Folder" --mode copy

    var mainVm = App.Services.GetRequiredService<MainViewModel>();
    if (launchOptions.Source != null)
    {
        mainVm.CopyJobVm.SourcePath = launchOptions.Source;
        mainVm.CopyJobVm.Mode = launchOptions.Mode;
    }

    new MainWindow { DataContext = mainVm }.Show();
}
```

---

## 4. Robocopy Exit Code Handling

| Exit Code | Meaning | UI Message |
|---|---|---|
| 0 | No files copied, no errors | ✅ Nothing to copy — destination is up to date |
| 1 | Files copied successfully | ✅ Copy completed successfully |
| 2 | Extra files detected in dest | ✅ Completed (extra files found in destination) |
| 4 | Mismatched files found | ⚠️ Completed with mismatches |
| 8 | Some files not copied | ❌ Some files failed to copy |
| 16 | Fatal error | ❌ Fatal error — check log for details |

Exit codes are bitwise flags and can be combined (e.g., code 3 = 1+2 = success + extras).

---

## 5. Testing Strategy

### Unit Tests
- `CommandBuilder` — test every option flag combination produces correct args string
- `OutputParser` — test parsing of real robocopy output lines
- `ExitCodeInterpreter` — test all exit code combinations
- `ProfileService` — test serialize/deserialize profiles

### Integration Tests
- `RobocopyService` — copy a real temp directory, verify files arrive at destination
- Context menu install/uninstall — verify registry keys created/removed

### Manual QA Checklist
- [ ] Context menu appears on fresh Windows 11 install
- [ ] UI never freezes during a large (10 GB+) copy
- [ ] Progress bar accurately reflects copy progress
- [ ] Cancel stops the copy immediately
- [ ] Pause suspends copy and Resume continues it
- [ ] App pre-fills source when launched from context menu

---

## 6. Build & Release

```bash
# Build release
dotnet build -c Release

# Run tests
dotnet test

# Build WiX installer
cd installer/RoboCopyGUI.Installer
dotnet build -c Release
# Output: RoboCopyGUI-v1.0-Setup.msi
```

The MSI installer must:
1. Install app to `%ProgramFiles%\RoboCopyGUI\`
2. Run `ContextMenuInstaller.Install()` as a Custom Action (requires admin elevation)
3. Create Start Menu shortcut
4. Register uninstaller in Add/Remove Programs
5. On uninstall: run `ContextMenuInstaller.Uninstall()`
