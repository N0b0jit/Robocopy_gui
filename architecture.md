# Architecture Document
## RoboCopy GUI — Application Architecture

**Version:** 1.0  
**Date:** March 2026

---

## 1. Architecture Overview

RoboCopy GUI follows a **layered desktop architecture** with clean separation between UI, business logic, and system integration layers. The app is a native Windows desktop application built with **C# / WPF (.NET 8)**, leveraging MVVM pattern for maintainability and testability.

```
┌─────────────────────────────────────────────────────────┐
│                      Presentation Layer                  │
│              WPF Views + XAML + Styles/Themes            │
├─────────────────────────────────────────────────────────┤
│                    ViewModel Layer (MVVM)                 │
│         MainViewModel, JobViewModel, SettingsViewModel   │
├─────────────────────────────────────────────────────────┤
│                    Application / Service Layer            │
│     RobocopyService  |  ProfileService  |  LogService    │
├─────────────────────────────────────────────────────────┤
│                    Core / Domain Layer                    │
│      CopyJob Model  |  CopyOptions  |  CommandBuilder    │
├──────────────────────────┬──────────────────────────────┤
│   Windows Shell Layer    │    File System Layer          │
│   Context Menu Registry  │    Process (robocopy.exe)     │
│   Shell Extension COM    │    File I/O (logs, profiles)  │
└──────────────────────────┴──────────────────────────────┘
```

---

## 2. Technology Stack

| Layer | Technology | Reason |
|---|---|---|
| UI Framework | WPF (.NET 8) | Native Windows, rich UI, XAML styling |
| Language | C# 12 | Strong typing, async/await, rich ecosystem |
| MVVM Framework | CommunityToolkit.Mvvm | Lightweight, source-generator based |
| Robocopy Execution | `System.Diagnostics.Process` | Spawn & stream robocopy.exe output |
| Context Menu | Windows Registry + COM Shell Extension | Native Explorer integration |
| Data Storage | JSON files (System.Text.Json) | Profiles, history, settings |
| Logging | Serilog | Structured logging to file |
| Installer | WiX Toolset v4 | MSI installer with registry actions |
| UI Design | ModernWPF / Fluent WPF | Windows 11 Fluent Design look |

---

## 3. Component Breakdown

### 3.1 Presentation Layer

**Views (XAML)**
- `MainWindow.xaml` — Main application shell, tab host
- `CopyJobView.xaml` — Source/destination pickers, options panel, progress
- `HistoryView.xaml` — Past job list with filter/search
- `SettingsView.xaml` — App preferences, theme, default options
- `ProfileManagerView.xaml` — Save/load/delete named profiles

**Controls (Reusable)**
- `FolderPickerControl` — Drag-and-drop + browse button combo
- `ProgressPanelControl` — File progress + overall progress + speed
- `LogViewerControl` — Auto-scrolling live log output
- `CommandPreviewControl` — Generated robocopy command display

### 3.2 ViewModel Layer

```
MainViewModel
├── CopyJobViewModel
│   ├── SourcePath : string
│   ├── DestinationPath : string
│   ├── Options : CopyOptionsViewModel
│   ├── Progress : CopyProgressViewModel
│   └── Commands: StartCopy, PauseCopy, CancelCopy
├── HistoryViewModel
└── SettingsViewModel
```

### 3.3 Service Layer

**RobocopyService**
- Builds robocopy command from `CopyOptions` model
- Spawns `robocopy.exe` as a child process
- Reads `stdout`/`stderr` streams asynchronously
- Parses output lines → fires progress events
- Supports pause (suspend process) and cancel (kill process)

**CommandBuilder**
- Pure function: `CopyOptions → string` (robocopy args string)
- Fully unit testable
- Used by both RobocopyService and CommandPreview UI

**ProfileService**
- CRUD for named profiles serialized to `%AppData%\RoboCopyGUI\profiles.json`
- Import/export profile as `.rcprofile` file

**LogService**
- Writes per-job logs to `%AppData%\RoboCopyGUI\logs\`
- Maintains job history in `history.json`

### 3.4 Context Menu Shell Extension

**Approach:** Registry-based static context menu (simpler, no COM DLL needed for MVP)

```
Registry Path:
HKEY_CLASSES_ROOT\Directory\shell\RoboCopyGUI\
  → (Default) = "RoboCopy Here →"
  → Icon = path\to\app.ico

HKEY_CLASSES_ROOT\Directory\shell\RoboCopyGUI\command\
  → (Default) = "C:\Program Files\RoboCopyGUI\RoboCopyGUI.exe" --source "%1"
```

Sub-commands registered under `shell\RoboCopyGUI\shell\`:
- `CopyTo` → launches with `--mode copy --source "%1"`
- `MoveTo` → launches with `--mode move --source "%1"`
- `MirrorTo` → launches with `--mode mirror --source "%1"`

The app reads CLI args on startup and pre-fills the UI accordingly.

---

## 4. Process Flow

```
User right-clicks folder in Explorer
        ↓
Context Menu entry appears ("RoboCopy Here →")
        ↓
User picks "Copy To..."
        ↓
RoboCopyGUI.exe launched with --source "C:\path" --mode copy
        ↓
MainWindow opens, CopyJobView pre-filled with source path
        ↓
User picks destination → clicks Start
        ↓
CopyJobViewModel.StartCopyCommand fires
        ↓
RobocopyService.ExecuteAsync(CopyJob job)
        ↓
Process.Start("robocopy.exe", args)
        ↓
Output stream read line-by-line on background thread
        ↓
Progress events → UI updates via Dispatcher (no freeze)
        ↓
Job completes → LogService.SaveJobLog() → HistoryViewModel updated
```

---

## 5. Threading Model

- **UI Thread:** WPF main thread — only UI updates
- **Copy Thread:** `Task.Run` background task — runs robocopy process, reads streams
- **Progress Updates:** Marshalled back to UI via `Application.Current.Dispatcher.InvokeAsync`
- **Pause:** `Process.Suspend()` via `NtSuspendProcess` P/Invoke
- **Cancel:** `Process.Kill()` + cleanup partial files (optional, user-configurable)

---

## 6. Data Models

```csharp
public record CopyOptions
{
    string Source { get; init; }
    string Destination { get; init; }
    CopyMode Mode { get; init; }          // Copy | Move | Mirror | Sync
    bool IncludeSubdirectories { get; init; }
    bool RestartableMode { get; init; }
    bool BackupMode { get; init; }
    bool MultiThreaded { get; init; }
    int ThreadCount { get; init; }        // 1–128
    bool SkipNewerFiles { get; init; }
    bool ExcludeHiddenFiles { get; init; }
    string[] ExcludeFiles { get; init; }
    string[] ExcludeDirectories { get; init; }
    // ... more flags
}

public record CopyJob
{
    Guid Id { get; init; }
    CopyOptions Options { get; init; }
    DateTime StartedAt { get; set; }
    DateTime? CompletedAt { get; set; }
    JobStatus Status { get; set; }       // Running | Paused | Completed | Failed | Cancelled
    CopyProgress Progress { get; set; }
    string LogFilePath { get; set; }
}
```

---

## 7. Error Handling Strategy

- Robocopy exit codes parsed and mapped to user-friendly messages (codes 0–16)
- Network path unavailability → retry dialog with configurable retry count
- Access denied → prompt to relaunch as admin
- All exceptions caught at service layer → surfaced to ViewModel as error state → shown in UI status bar

---

## 8. Security Considerations

- Context menu registration done at install time with admin rights (WiX installer action)
- App itself does not require admin for normal operation
- No data sent externally — fully offline application
- User source/destination paths never logged to external services
