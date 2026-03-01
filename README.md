# Nexus Copy

Fast. Powerful. Simple. File copying reimagined.

## Overview

Nexus Copy is a modern Windows desktop application that provides a user-friendly GUI for the powerful robocopy command-line tool. Built with C# 12, .NET 8, and WPF, it offers real-time progress tracking, profile management, and seamless Windows Explorer integration.

## Features

- **Multiple Copy Modes**: Copy, Move, Mirror, and Sync operations
- **Real-time Progress**: Live file-by-file progress with speed and ETA calculations
- **Profile Management**: Save and reuse copy configurations
- **Job History**: Track and review past copy operations
- **Windows Integration**: Right-click context menu in Windows Explorer
- **CLI Support**: Command-line argument parsing for automation
- **Modern UI**: Clean, intuitive interface with dark/light themes
- **Pause/Resume**: Control copy operations in real-time

## Installation

### Option 1: Self-Contained Executable (Recommended)
Download the `NexusCopy.exe` from the `exe-release` folder. This version includes all dependencies and doesn't require .NET runtime to be installed separately.

### Option 2: Framework-Dependent Executable
Download the smaller `NexusCopy.exe` from `exe-release` folder. Requires .NET 8.0 Desktop Runtime to be installed on your system.

### Option 3: Build from Source
1. Clone this repository
2. Open the solution in Visual Studio 2022
3. Build and run the `NexusCopy.App` project

## Usage

### Basic Usage
1. Launch Nexus Copy
2. Select source and destination folders
3. Choose copy mode (Copy, Move, Mirror, or Sync)
4. Configure options as needed
5. Click "Start" to begin copying

### Context Menu Integration
Right-click on any folder in Windows Explorer and select "Nexus Copy Here →" to quickly start a copy operation.

### Command Line Usage
```bash
# Basic copy
NexusCopy.exe --source "C:\Source" --destination "D:\Destination" --mode copy

# Mirror operation
NexusCopy.exe --source "C:\Source" --destination "D:\Destination" --mode mirror

# Move operation
NexusCopy.exe --source "C:\Source" --destination "D:\Destination" --mode move
```

## Configuration

### Default Settings
- Thread Count: 16
- Include Subdirectories: Yes
- Restartable Mode: Yes
- Multi-threaded: Yes

### Profiles
Save frequently used configurations as profiles for quick access:
1. Configure your copy options
2. Click "Save Profile"
3. Enter a name and description
4. Access saved profiles from the Profiles tab

## Technical Details

### Architecture
- **Core**: Domain models and interfaces
- **Services**: Business logic and robocopy integration
- **Shell**: Windows context menu and CLI integration
- **App**: WPF MVVM application

### Technologies
- **Framework**: .NET 8.0
- **Language**: C# 12
- **UI**: WPF with MVVM pattern
- **Dependencies**: CommunityToolkit.Mvvm, ModernWpfUI
- **Testing**: xUnit, FluentAssertions

### Robocopy Integration
Nexus Copy wraps the Windows robocopy.exe utility, providing:
- Real-time output parsing
- Progress tracking
- Pause/resume functionality via P/Invoke
- Exit code interpretation
- Comprehensive logging

## Development

### Building the Project
```bash
# Build the solution
dotnet build

# Run tests
dotnet test

# Create self-contained executable
dotnet publish src/NexusCopy.App/NexusCopy.App.csproj -c Release -r win-x64 --self-contained true -p:PublishSingleFile=true

# Create framework-dependent executable
dotnet publish src/NexusCopy.App/NexusCopy.App.csproj -c Release -r win-x64 --self-contained false -p:PublishSingleFile=true
```

### Project Structure
```
Nexus Copy/
├── src/
│   ├── NexusCopy.Core/          # Models & Interfaces
│   ├── NexusCopy.Services/      # Business Logic
│   ├── NexusCopy.Shell/         # Context Menu & CLI
│   └── NexusCopy.App/           # WPF Application
├── tests/
│   ├── NexusCopy.Core.Tests/    # Core Tests
│   └── NexusCopy.Services.Tests/ # Service Tests
├── installer/
│   └── NexusCopy.Installer/     # WiX Installer
├── exe-release/                 # Release Executables
└── publish/                     # Self-contained Build
```

## Requirements

- **Operating System**: Windows 10 or later
- **Runtime**: .NET 8.0 Desktop Runtime (for framework-dependent version)
- **Architecture**: x64

## Troubleshooting

### Common Issues

1. **"robocopy.exe not found"**
   - Nexus Copy uses the built-in Windows robocopy.exe
   - Ensure you're running on a supported Windows version

2. **"Access denied" errors**
   - Run as Administrator if copying system files
   - Check folder permissions

3. **Performance issues**
   - Adjust thread count in settings
   - Disable antivirus real-time scanning for large copies

### Logging
All operations are logged to:
- `%AppData%\NexusCopy\logs\` - Detailed operation logs
- Application displays live log output during operations

## Contributing

1. Fork the repository
2. Create a feature branch
3. Make your changes
4. Add tests for new functionality
5. Submit a pull request

## License

This project is licensed under the MIT License - see the LICENSE file for details.

## Support

For issues, questions, or feature requests:
- Create an issue on GitHub
- Check the troubleshooting section above
- Review the operation logs for detailed error information

## Changelog

### Version 1.0.0
- Initial release
- Core copy functionality
- Profile management
- Job history
- Windows context menu integration
- CLI support
- Modern WPF interface
