#define MyAppName "NVM Quick Switch"
#define MyAppVersion "1.3.0"
#define MyAppPublisher "razzp"
#define MyAppURL "https://github.com/razzp/nvm-quick-switch"
#define MyAppExeName "NVMQuickSwitch.exe"
#define PublishPath SourcePath + "NVMQuickSwitch\bin\Release\net6.0-windows\publish"

[Setup]
AppId={{46CD8E0E-21AE-443B-8B1C-81AABC1F637A}
AppName={#MyAppName}
AppVersion={#MyAppVersion}
AppPublisher={#MyAppPublisher}
AppPublisherURL={#MyAppURL}
AppSupportURL={#MyAppURL}
AppUpdatesURL={#MyAppURL}
DefaultDirName={autopf}\{#MyAppName}
DefaultGroupName={#MyAppName}
AllowNoIcons=yes
LicenseFile={#SourcePath}\LICENSE
PrivilegesRequired=admin
OutputDir={#SourcePath}\release
OutputBaseFilename=NVMQuickSwitchSetup-{#MyAppVersion}
SetupIconFile={#SourcePath}\NVMQuickSwitch\Resources\icon-app.ico
Compression=lzma
SolidCompression=yes
WizardStyle=modern

[Components]
Name: "main"; Description: "Main application"; Types: full compact custom; Flags: fixed  
Name: "registry"; Description: "Run on startup"; Types: full custom;

[Languages]
Name: "english"; MessagesFile: "compiler:Default.isl"

[Files]
Source: "{#PublishPath}\{#MyAppExeName}"; DestDir: "{app}"; Flags: ignoreversion
Source: "{#PublishPath}\Resources\*"; DestDir: "{app}\Resources"; Flags: ignoreversion
Source: "{#PublishPath}\NVMQuickSwitch.deps.json"; DestDir: "{app}"; Flags: ignoreversion
Source: "{#PublishPath}\NVMQuickSwitch.dll"; DestDir: "{app}"; Flags: ignoreversion
Source: "{#PublishPath}\NVMQuickSwitch.pdb"; DestDir: "{app}"; Flags: ignoreversion
Source: "{#PublishPath}\NVMQuickSwitch.runtimeconfig.json"; DestDir: "{app}"; Flags: ignoreversion

[Registry]
Root: HKLM; Subkey: "SOFTWARE\Microsoft\Windows\CurrentVersion\Run"; ValueType: string; ValueName: "{#MyAppName}"; ValueData: """{app}\{#MyAppExeName}"""; Flags: uninsdeletevalue; Components: registry

[Run]
Filename: "{app}\{#MyAppExeName}"; Description: "{cm:LaunchProgram,{#StringChange(MyAppName, '&', '&&')}}"; Flags: nowait postinstall shellexec skipifsilent
