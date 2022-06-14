# NVM Quick Switch

A no-frills tray icon for NVM Windows that lets you quickly switch between Node versions.

## Installation

View [releases](https://github.com/razzp/nvm-quick-switch/releases) for download options. Alternatively the repo can be cloned and built locally.

### Prerequisites for local build

- Visual Studio 2022
- .NET 6.0 Runtime
- .NET desktop development workload (configurable via VS Installer)

## Contributing

### Versioning

This project adheres to [Semantic Versioning](https://semver.org/spec/v2.0.0.html). Versions are specified in the following files:

- `\inno-build.iss`
- `\NVMQuickSwitch\QuickSwitchApp.cs`

### Releasing

1. Ensure correct versions are set (see above)
2. Ensure `CHANGELOG.md` is updated accordingly
3. Publish the project using the default publish profile
4. Run `inno-build.iss` to generate the installer
5. Run `make-zip.ps1` to ZIP up the publish output
6. Create an appropriate GitHub release & tag
