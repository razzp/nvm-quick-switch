#define AppName "NVM Quick Switch"
#define AppPublisher "razzp"
#define AppURL "https://github.com/razzp/nvm-quick-switch"
#define AppExeName "NVMQuickSwitch.exe"
#define PublishPath SourcePath + "NVMQuickSwitch\bin\Release\net8.0-windows\publish"

#if !Defined(AppVersion)
    // You can temporarily change this version number
    // if you're running the compiler manually.
    #define AppVersion "0.0.0"
#endif

[Setup]
AppId={{46CD8E0E-21AE-443B-8B1C-81AABC1F637A}
AppName={#AppName}
AppVersion={#AppVersion}
AppPublisher={#AppPublisher}
AppPublisherURL={#AppURL}
AppSupportURL={#AppURL}
AppUpdatesURL={#AppURL}
DefaultDirName={autopf}\{#AppName}
DefaultGroupName={#AppName}
AllowNoIcons=yes
LicenseFile={#SourcePath}\LICENSE
PrivilegesRequired=admin
OutputDir={#SourcePath}\release
OutputBaseFilename=NVMQuickSwitchSetup-{#AppVersion}
SetupIconFile={#SourcePath}\NVMQuickSwitch\Resources\icon_app.ico
Compression=lzma
SolidCompression=yes
WizardStyle=modern

[Components]
Name: "main"; Description: "Main application"; Types: full compact custom; Flags: fixed  
Name: "registry"; Description: "Run on startup"; Types: full custom;

[Languages]
Name: "english"; MessagesFile: "compiler:Default.isl"

[Files]
Source: "{#PublishPath}\{#AppExeName}"; DestDir: "{app}"; Flags: ignoreversion
Source: "{#PublishPath}\Resources\*"; DestDir: "{app}\Resources"; Flags: ignoreversion
Source: "{#PublishPath}\NVMQuickSwitch.deps.json"; DestDir: "{app}"; Flags: ignoreversion
Source: "{#PublishPath}\NVMQuickSwitch.dll"; DestDir: "{app}"; Flags: ignoreversion
Source: "{#PublishPath}\NVMQuickSwitch.pdb"; DestDir: "{app}"; Flags: ignoreversion
Source: "{#PublishPath}\NVMQuickSwitch.runtimeconfig.json"; DestDir: "{app}"; Flags: ignoreversion
Source: "{#PublishPath}\VERSION"; DestDir: "{app}"; Flags: ignoreversion

[Registry]
Root: HKLM; Subkey: "SOFTWARE\Microsoft\Windows\CurrentVersion\Run"; ValueType: string; ValueName: "{#AppName}"; ValueData: """{app}\{#AppExeName}"""; Flags: uninsdeletevalue; Components: registry

[Run]
Filename: "{app}\{#AppExeName}"; Description: "{cm:LaunchProgram,{#StringChange(AppName, '&', '&&')}}"; Flags: nowait postinstall shellexec skipifsilent
