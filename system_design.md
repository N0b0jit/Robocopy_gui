# System Design Document
## RoboCopy GUI — Technical System Design

**Version:** 1.0  
**Date:** March 2026

---

## 1. High-Level System Diagram

```
┌─────────────────────────────────────────────────────────────────────┐
│                        Windows OS Environment                        │
│                                                                     │
│  ┌──────────────────┐     Right-click     ┌──────────────────────┐  │
│  │  Windows Explorer│ ─────────────────→  │  Context Menu Entry  │  │
│  │  (Shell)         │                     │  (Registry Keys)     │  │
│  └──────────────────┘                     └──────────┬───────────┘  │
│                                                      │ Launch with  │
│                                                      │ --source arg │
│  ┌───────────────────────────────────────────────────▼───────────┐  │
│  │                    RoboCopyGUI.exe (WPF App)                  │  │
│  │                                                               │  │
│  │  ┌─────────────┐   ┌──────────────┐   ┌──────────────────┐   │  │
│  │  │   Main UI   │←→ │  ViewModels  │←→ │    Services      │   │  │
│  │  │  (XAML/WPF) │   │  (MVVM)      │   │  (Business Logic)│   │  │
│  │  └─────────────┘   └──────────────┘   └────────┬─────────┘   │  │
│  │                                                 │             │  │
│  └─────────────────────────────────────────────────┼─────────────┘  │
│                                                    │ Spawn          │
│                                                    ▼                │
│                              ┌─────────────────────────────────┐   │
│                              │        robocopy.exe             │   │
│                              │   (Windows Built-in Tool)       │   │
│                              └────────────┬────────────────────┘   │
│                                           │ Read/Write              │
│                              ┌────────────▼────────────────────┐   │
│                              │         File System             │   │
│                              │   Source ──────────→ Destination│   │
│                              └─────────────────────────────────┘   │
│                                                                     │
│  ┌──────────────────────────────────────────────────────────────┐   │
│  │                  App Data Storage                            │   │
│  │  %AppData%\RoboCopyGUI\                                      │   │
│  │  ├── profiles.json     (saved copy presets)                  │   │
│  │  ├── history.json      (job history)                         │   │
│  │  ├── settings.json     (user preferences)                    │   │
│  │  └── logs\             (per-job log files)                   │   │
│  └──────────────────────────────────────────────────────────────┘   │
└─────────────────────────────────────────────────────────────────────┘
```

---

## 2. Component Interaction Design

### 2.1 Startup Flow

```
App.OnStartup()
    │
    ├── Parse CLI args (--source, --mode, --destination)
    │
    ├── Register DI services
    │   ├── RobocopyService (Transient)
    │   ├── ProfileService (Singleton)
    │   ├── LogService (Singleton)
    │   └── ViewModels (Transient)
    │
    ├── Load user settings from settings.json
    │
    └── Show MainWindow
            └── If CLI args present → pre-fill CopyJobView
```

### 2.2 Copy Job Execution Flow

```
User clicks "Start"
        │
        ▼
CopyJobViewModel.StartCopyCommand.Execute()
        │
        ├── Validate: source exists, destination writable
        │
        ├── Build CopyJob object from current options
        │
        ├── Create CancellationTokenSource
        │
        └── await RobocopyService.ExecuteAsync(job, ct)
                    │
                    ├── CommandBuilder.Build(job.Options) → args string
                    │
                    ├── Process.Start("robocopy.exe", args)
                    │
                    ├── BeginOutputReadLine() → OutputDataReceived event loop
                    │        │
                    │        ├── OutputParser.TryParse(line)
                    │        │
                    │        ├── Fire ProgressUpdated event
                    │        │       └── ViewModel updates UI via Dispatcher
                    │        │
                    │        └── Fire LogLineReceived event
                    │                └── ViewModel appends to log viewer
                    │
                    └── WaitForExitAsync()
                                │
                                └── ExitCode → ExitCodeInterpreter.Interpret()
                                            │
                                            └── Show result banner in UI
                                                LogService.SaveJob(job)
```

### 2.3 Context Menu Launch Flow

```
User right-clicks folder "C:\MyFolder" in Explorer
        │
        ▼
Windows reads HKCR\Directory\shell\RoboCopyGUI\shell\CopyTo\command
        │
        Value: "C:\Program Files\RoboCopyGUI\RoboCopyGUI.exe" --mode copy --source "C:\MyFolder"
        │
        ▼
OS launches RoboCopyGUI.exe with args: --mode copy --source "C:\MyFolder"
        │
        ▼
App.OnStartup() parses args
        │
        ▼
MainWindow opens with:
  - Source field = "C:\MyFolder"
  - Mode = Copy
  - Destination field = empty (user must fill)
  - Focus on destination picker
```

---

## 3. Data Flow Design

### 3.1 CopyOptions → Robocopy Command

```
CopyOptions {                          robocopy.exe args:
  Source: "C:\Files"        ────→      "C:\Files"
  Destination: "D:\Backup"  ────→      "D:\Backup"
  IncludeSubdirs: true      ────→      /e
  RestartableMode: true     ────→      /z
  MultiThreaded: true       ────→      /mt:16
  ThreadCount: 16           ─┘
  MirrorMode: false
  SkipNewerFiles: true      ────→      /xo
  ExcludeFiles: ["*.tmp"]   ────→      /xf "*.tmp"
  LogFilePath: "C:\...\log" ────→      /log+:"C:\...\log" /tee
}

Final command:
robocopy.exe "C:\Files" "D:\Backup" /e /z /mt:16 /xo /xf "*.tmp" /log+:"..." /tee
```

### 3.2 Robocopy Output → UI Progress

```
Robocopy stdout line:
"	100%	New File 		   2.5 m	Documents\report.pdf"
        │
        ▼
OutputParser.TryParse(line)
        │
        ▼
CopyProgressUpdate {
    FilePercent: 100.0,
    Status: "New File",
    FileName: "Documents\report.pdf"
}
        │
        ▼
ProgressUpdated event
        │
        ▼ (via Dispatcher.InvokeAsync)
CopyJobViewModel {
    CurrentFileName = "Documents\report.pdf"
    CurrentFileProgress = 100.0
    StatusText = "New File"
    CopiedCount++
    SpeedMBps = calculated from bytes/time
}
        │
        ▼
WPF Data Binding → UI redraws progress bar, filename label, speed
```

---

## 4. State Machine — Copy Job

```
                ┌─────────────────┐
                │      IDLE       │ ← Default state on app open
                └────────┬────────┘
                         │ User clicks Start
                         ▼
                ┌─────────────────┐
                │    VALIDATING   │ ← Check source/dest paths
                └────────┬────────┘
                         │ Valid
                         ▼
                ┌─────────────────┐     User clicks Pause
                │    RUNNING      │ ──────────────────────→ ┌──────────┐
                └────────┬────────┘                         │  PAUSED  │
                         │                         Resume → └────┬─────┘
                         │                                       │
                   ┌─────┤◄──────────────────────────────────────┘
                   │     │
          Exit=0,1 │     │ Exit=8,16
                   ▼     ▼
            ┌──────────┐ ┌──────────┐     User clicks Cancel
            │COMPLETED │ │  FAILED  │  ┌──────────────────────────┐
            └──────────┘ └──────────┘  │       CANCELLED          │
                                       └──────────────────────────┘
                                        (from any non-idle state)
```

---

## 5. Context Menu Registry Design

```
HKEY_CLASSES_ROOT
└── Directory
    └── shell
        └── RoboCopyGUI                ← Parent entry
            ├── (Default)  = "RoboCopy Here →"
            ├── Icon       = "C:\...\RoboCopyGUI.exe,0"
            ├── SubCommands = ""       ← Makes it a cascading menu
            └── shell
                ├── CopyTo
                │   ├── (Default)  = "📋 Copy To..."
                │   ├── Icon       = "C:\...\RoboCopyGUI.exe,1"
                │   └── command
                │       └── (Default) = "\"C:\...\RoboCopyGUI.exe\" --mode copy --source \"%1\""
                ├── MoveTo
                │   ├── (Default)  = "✂️ Move To..."
                │   └── command
                │       └── (Default) = "\"C:\...\RoboCopyGUI.exe\" --mode move --source \"%1\""
                └── MirrorTo
                    ├── (Default)  = "🔁 Mirror To..."
                    └── command
                        └── (Default) = "\"C:\...\RoboCopyGUI.exe\" --mode mirror --source \"%1\""

Note: Also register under HKEY_CLASSES_ROOT\Directory\Background\shell\
to support right-clicking inside a folder (not just on folder icons)
```

---

## 6. File Storage Schema

### profiles.json
```json
{
  "profiles": [
    {
      "id": "uuid-here",
      "name": "Nightly Backup",
      "createdAt": "2026-03-01T00:00:00Z",
      "options": {
        "source": "C:\\Work",
        "destination": "D:\\Backups\\Work",
        "mode": "Mirror",
        "includeSubdirectories": true,
        "includeEmpty": true,
        "restartableMode": true,
        "multiThreaded": true,
        "threadCount": 16,
        "skipNewerFiles": false,
        "excludeHidden": false,
        "excludeFiles": ["*.tmp", "*.log"],
        "excludeDirectories": [".git", "node_modules"]
      }
    }
  ]
}
```

### history.json
```json
{
  "jobs": [
    {
      "id": "uuid-here",
      "startedAt": "2026-03-01T10:00:00Z",
      "completedAt": "2026-03-01T10:04:32Z",
      "status": "Completed",
      "source": "C:\\Work",
      "destination": "D:\\Backups\\Work",
      "mode": "Mirror",
      "filesCopied": 1247,
      "filesSkipped": 89,
      "bytesCopied": 4831838208,
      "exitCode": 1,
      "logFilePath": "C:\\Users\\...\\logs\\job-uuid.log"
    }
  ]
}
```

---

## 7. UI Layout Wireframe (Text)

```
┌──────────────────────────────────────────────────────────────────┐
│  🚀 RoboCopy GUI                               [_ □ ×]           │
├──────────────────────────────────────────────────────────────────┤
│  [Copy Job]  [History]  [Profiles]  [Settings]                   │
├──────────────────────────────────────────────────────────────────┤
│                                                                  │
│  SOURCE FOLDER                                                   │
│  ┌─────────────────────────────────────────┐  [Browse]          │
│  │ C:\Users\Alex\Documents                  │  [  📂  ]          │
│  └─────────────────────────────────────────┘                    │
│                                                                  │
│  DESTINATION FOLDER                                              │
│  ┌─────────────────────────────────────────┐  [Browse]          │
│  │ D:\Backup\Documents                      │  [  📂  ]          │
│  └─────────────────────────────────────────┘                    │
│                                                                  │
│  MODE:  (●) Copy  ( ) Move  ( ) Mirror  ( ) Sync                │
│                                                                  │
│  OPTIONS:                                                        │
│  [✓] Include Subdirectories   [✓] Restartable Mode              │
│  [✓] Multi-Threaded  Threads: [────●─────] 16                   │
│  [ ] Backup Mode              [✓] Skip Older Files              │
│  [ ] Exclude Hidden Files     [ ] Mirror (overwrite dest)        │
│                                                                  │
│  ┌──────────────────────────────────────────────────────┐       │
│  │ CMD: robocopy "C:\..." "D:\..." /e /z /mt:16 /xo     │ [📋]  │
│  └──────────────────────────────────────────────────────┘       │
│                                                                  │
│  [▶ Start]  [⏸ Pause]  [⏹ Cancel]           [💾 Save Profile]  │
│                                                                  │
├──────────────────────────────────────────────────────────────────┤
│  PROGRESS                                                        │
│  Current: report.pdf                                             │
│  [████████████████████░░░░░░░░] 67%    Speed: 1.2 GB/s          │
│  Overall: [████████░░░░░░░░░░░░] 41%   ETA: 00:02:14            │
│  Files: 512 / 1247 copied   Skipped: 89   Errors: 0             │
├──────────────────────────────────────────────────────────────────┤
│  LOG OUTPUT                                              [Clear] │
│  ┌────────────────────────────────────────────────────────────┐  │
│  │ New File     2.5 m   Documents\report.pdf                  │  │
│  │ New File     1.1 m   Documents\budget.xlsx                 │  │
│  │ Newer        800 k   Documents\notes.txt                   │  │
│  │ ...                                                        │  │
│  └────────────────────────────────────────────────────────────┘  │
└──────────────────────────────────────────────────────────────────┘
```

---

## 8. Performance Design Decisions

| Decision | Rationale |
|---|---|
| Use `/mt:16` as default | 16 threads is sweet spot for most storage; user adjustable |
| Use `/z` (restartable) as default | Safer for large copies over network; small perf cost acceptable |
| Stream stdout in real-time | Avoids buffering; keeps UI live even during slow copies |
| No buffering stdout | `BeginOutputReadLine()` fires per line — avoids lag in log viewer |
| Background Task for process | Ensures UI thread is never blocked |
| Dispatcher.InvokeAsync for updates | Prevents cross-thread WPF exceptions |
| Lazy log file writes | Write log to disk only on job complete to avoid I/O overhead |

---

## 9. Installer Design (WiX)

The MSI installer performs these steps in order:

1. **Check OS:** Abort if not Windows 10+
2. **Check robocopy.exe:** Verify `%SystemRoot%\System32\robocopy.exe` exists
3. **Install files** to `%ProgramFiles%\RoboCopyGUI\`
4. **Run Custom Action (as admin):** `ContextMenuInstaller.Install()`
5. **Create Start Menu shortcut**
6. **Register Add/Remove Programs entry**
7. **On Uninstall:** Run `ContextMenuInstaller.Uninstall()`, remove app files, remove AppData (prompt user)
