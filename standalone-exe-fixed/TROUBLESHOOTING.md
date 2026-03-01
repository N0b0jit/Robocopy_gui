# Nexus Copy - Troubleshooting Guide

## Quick Solutions

### 1. Try the Fixed Version
Use the `NexusCopy.exe` in the `standalone-exe-fixed` folder - this version has dependency injection fixes.

### 2. Run Diagnostic Tool
Double-click `Diagnose.bat` to check system compatibility and get detailed error information.

### 3. Run as Administrator
Double-click `Run as Admin.bat` to launch with elevated privileges.

### 4. Check Windows Event Viewer
1. Press Win+R, type `eventvwr.msc`
2. Navigate to Windows Logs → Application
3. Look for recent errors related to NexusCopy.exe

## Common Issues and Solutions

### Issue: Application doesn't start (loading cursor only)
**Causes:**
- Missing .NET 8 Desktop Runtime (for framework-dependent version)
- Antivirus blocking the executable
- Insufficient permissions
- Dependency injection conflicts

**Solutions:**
1. Use the self-contained version (166MB) - no runtime required
2. Temporarily disable antivirus
3. Run as administrator
4. Check Windows Event Viewer for errors

### Issue: "Application cannot start" error
**Solutions:**
1. Run `Diagnose.bat` for system check
2. Ensure Windows 10 or later
3. Install .NET 8 Desktop Runtime (if using framework-dependent version)

### Issue: Context menu not working
**Solutions:**
1. Run NexusCopy as administrator once
2. Check Windows Registry for context menu entries
3. Restart Windows Explorer

## Manual Installation Steps

### Step 1: Download .NET 8 Runtime (if needed)
For the smaller 5MB version, install:
- https://dotnet.microsoft.com/download/dotnet/8.0
- Select ".NET Desktop Runtime 8.0.x"

### Step 2: Extract and Run
1. Extract the standalone executable
2. Right-click `NexusCopy.exe`
3. Select "Run as administrator"

### Step 3: Verify Installation
1. Application should show the main window
2. Check that all tabs (Copy Job, History, Profiles, Settings) are visible
3. Test with a small folder copy operation

## Advanced Troubleshooting

### Check Dependencies
Run these commands in Command Prompt:
```cmd
dotnet --list-runtimes
wmic computersystem get TotalPhysicalMemory
wmic logicaldisk get size,freespace,caption
```

### Create Test Environment
1. Create test folders:
   - `C:\NexusTest\Source`
   - `C:\NexusTest\Destination`
2. Add some test files to Source
3. Try copying with NexusCopy

### Performance Issues
1. Adjust thread count in Settings
2. Disable real-time antivirus scanning during large copies
3. Ensure sufficient disk space
4. Close other applications during large operations

## Contact Support

If issues persist:
1. Run `Diagnose.bat` and save the output
2. Check Windows Event Viewer for errors
3. Try the application on a different computer
4. Report issues with system specifications and error details

## File Locations

- **Application Data**: `%AppData%\NexusCopy\`
- **Logs**: `%AppData%\NexusCopy\logs\`
- **Profiles**: `%AppData%\NexusCopy\profiles\`
- **History**: `%AppData%\NexusCopy\history\`

## Registry Entries (Manual Cleanup if needed)

If context menu integration fails, check these registry keys:
- `HKEY_CLASSES_ROOT\Directory\shell\NexusCopy`
- `HKEY_CLASSES_ROOT\Directory\Background\shell\NexusCopy`
