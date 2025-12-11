# NVM Quick Switch

A no-frills tray icon for NVM Windows that lets you quickly switch between Node versions.

## Table of Contents

-   [Installation](#installation)
-   [Contributing](#contributing)
    -   [Minimum requirements](#minimum-requirements)
    -   [Versioning](#versioning)
    -   [Releasing](#releasing)
-   [License](#license)

## Installation

View the [latest release](https://github.com/razzp/nvm-quick-switch/releases/latest) for download options.

## Contributing

### Minimum requirements

-   [Visual Studio 2022 version 17.8](https://visualstudio.microsoft.com/downloads/)
-   [.NET 8.0 Runtime](https://dotnet.microsoft.com/en-us/download/dotnet/8.0)
-   .NET desktop development workload (configurable via VS Installer)
-   [Inno Setup](https://jrsoftware.org/isinfo.php)

### Versioning

This project adheres to [Semantic Versioning](https://semver.org/spec/v2.0.0.html). The current version is kept in a global `VERSION` file in the `.\NVMQuickSwitch` directory, and is referenced in multiple places.

### Releasing

First and foremost, don't forget to update `CHANGELOG.md`.

Nect, running `.\tools\publish.ps1` will guide you through the release process, and automate the steps that are detailed below. This can be used as a reference in the event the script fails and/or you need to release manually.

> [!TIP]
> You can run `publish.ps1` with the `-dry` or `-d` flags to simulate a release.

#### 1. Update global `VERSION` file

Open `.\NVMQuickSwitch\VERSION` and update the version number.

#### 2. Publish the project

This can be done from within Visual Studio, or via the CLI.

If using the CLI, you can run run:

```shell
dotnet publish "NVMQuickSwitch\NVMQuickSwitch.csproj" -c Release /p:PublishProfile="Default"
```

Or, from Visual Studio, right click the project and select "Publish". Ensure that the "Default" publish profile is selected (`Default.pubxml`) and click "Publish" again.

#### 3. Create installer

This can be done from within Inno Setup Compiler, or via the CLI.

If using the CLI, you'll need to find the install path of ISCC.exe, since the app doesn't create a PATH environment variable during setup. On Windows this will most likely be `C:\Program Files (x86)\Inno Setup 6\ISCC.exe`.

Then you can run:

```shell
[PATH]\ISCC.exe /Qp "/DAppVersion=[VERSION]" "../inno-build.iss"
```

Replace `[PATH]` with the full path to `ISCC.exe`, and `[VERSION]` with the **exact** same version you set in the `VERSION` file.

If using Inno Setup Compiler, start by opening `.\inno-build.iss`. You'll need to find and manually set the version number, again exactly as it is in the `VERSION` file. It's easy to find:

```pascal
#if !Defined(AppVersion)
    #define AppVersion "0.0.0"
#endif
```

> [!IMPORTANT]
> Once finished, revert this version change! This is a manual override and should not be committed.

Then select _Build → Compile_ (or Ctrl+F9 on Windows) to create the installer.

#### 4. Zip up release files

Now you've published, the build output should be available under `.\NVMQuickSwitch\bin\Release\net8.0-windows`. Zip the contents of this folder up and name it `NVMQuickSwitch-[VERSION].zip`, replacing the version number as before. It's not important where you put the zip file, but the automation tool will put it in the `.\release` directory along with the install file that was created in the previous step.

#### 5. Create a Git tag

Ensure there are no uncommitted changes, and that you're on the master branch (`main`), and then create and push a tag using either your Git client or the CLI:

```shell
git tag [VERSION]
git push origin [VERSION]
```

#### 6. Create a release on GitHub

-   Draft a new release (`/releases/new`).
-   Select the tag you created in the previous step.
-   Set the "Release title" to "Version [VERSION]".
-   Add a description, or generate release notes.
-   Attach both the installer and the zip file you created earlier.
-   Check the "Set as the latest release" checkbox.
-   Finally, click "Publish release".

And you're done!

## License

Made with ❤️

Published under the [MIT License](./LICENCE).
