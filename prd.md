# Product Requirements Document (PRD)
## RoboCopy GUI — Fast File Copy Manager for Windows

**Version:** 1.0  
**Author:** Senior Dev Team  
**Date:** March 2026  
**Status:** Draft

---

## 1. Overview

### 1.1 Product Summary
RoboCopy GUI is a Windows desktop application that provides a modern, user-friendly graphical interface on top of Windows' built-in `robocopy` command-line utility. It enables users to perform fast, reliable file and folder copy/move/sync operations without memorizing CLI flags — and integrates directly into Windows Explorer via a context menu.

### 1.2 Problem Statement
Windows' native `robocopy` is one of the most powerful file copy tools available, supporting multi-threading, restartable copies, mirroring, and more — but it is locked behind a command-line interface. The vast majority of Windows users never benefit from it because:
- They don't know it exists
- CLI usage is intimidating and error-prone
- There's no visual feedback or progress tracking

### 1.3 Goal
Build a polished, intuitive GUI wrapper that exposes robocopy's full power to non-technical users, with a context menu integration so users can trigger copies instantly from Windows Explorer.

---

## 2. Target Users

| User Type | Description |
|---|---|
| Home Users | Copying large media libraries, backups |
| IT Admins | Bulk file operations, server migrations |
| Developers | Syncing project directories |
| Power Users | Wanting fine-grained control over copy operations |

---

## 3. Core Features (MVP)

### 3.1 Main GUI Window
- Source folder selector (drag & drop + browse button)
- Destination folder selector (drag & drop + browse button)
- Operation mode selector: **Copy / Move / Mirror / Sync**
- Common options panel (checkboxes):
  - Include Subdirectories (`/e`)
  - Restartable mode (`/z`)
  - Multi-threaded (`/mt`) with thread count slider (1–128)
  - Mirror mode (`/mir`)
  - Backup mode (`/b`)
  - Skip newer files (`/xo`)
  - Exclude hidden/system files
- Real-time progress bar per file and overall
- Live log output panel (scrollable)
- Speed indicator (MB/s)
- Estimated time remaining
- Start / Pause / Cancel buttons

### 3.2 Context Menu Integration
- Right-click any folder in Windows Explorer → **"RoboCopy Here →"**
  - Sub-options: **Copy To...**, **Move To...**, **Mirror To...**
- Registers via Windows Registry on install
- Launches GUI pre-filled with selected source path
- Works for single and multi-selected folders

### 3.3 Preset Profiles
- Save current settings as a named profile (e.g., "Nightly Backup")
- Load, edit, delete profiles
- Profiles stored as JSON locally

### 3.4 Command Preview
- Live-generated robocopy command string shown at bottom of UI
- "Copy Command" button so power users can use it in their own scripts

### 3.5 Job History & Logs
- Log of past copy jobs with: date, source, destination, files copied, duration, status
- Per-job log file export (.txt)

---

## 4. Non-Functional Requirements

| Requirement | Target |
|---|---|
| Startup time | < 2 seconds |
| UI responsiveness | Copy runs on background thread; UI never freezes |
| Windows version support | Windows 10 & Windows 11 |
| Installer size | < 50 MB |
| Language | English (i18n-ready for future) |
| Accessibility | Keyboard navigable, screen-reader compatible |

---

## 5. Out of Scope (v1.0)
- Network/FTP/cloud copy destinations
- Scheduled task builder (v2.0 candidate)
- Linux / macOS support
- Mobile companion app

---

## 6. Success Metrics
- User can complete a copy job in under 30 seconds from first launch
- Context menu integration works on fresh Windows 10/11 install
- Zero UI freezes during copy operations
- Copy speed matches or exceeds native robocopy CLI performance

---

## 7. Constraints
- Must use Windows built-in `robocopy.exe` (no third-party copy engine)
- Context menu registration requires admin rights on install
- Must not require .NET version higher than what ships with Windows 10/11
