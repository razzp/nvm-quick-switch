using NVMQuickSwitch.Helpers;
using NVMQuickSwitch.Models;
using System.Diagnostics;
using System.Reflection;

namespace NVMQuickSwitch
{
    internal class QuickSwitchApp : ApplicationContext
    {
        private readonly System.Windows.Forms.Timer _refreshTimer = new()
        {
            Interval = Constants.RefreshInterval,
        };

        private readonly System.Windows.Forms.Timer _updateTimer = new()
        {
            Interval = Constants.UpdateCheckInterval,
        };

        private readonly Icon _iconApp = new("Resources/icon_app.ico");
        private readonly Icon _iconAppAlert = new("Resources/icon_app-alert.ico");

        private readonly Image _iconAdd = Image.FromFile("Resources/icon_add.ico");
        private readonly Image _iconAlert = Image.FromFile("Resources/icon_alert.ico");
        private readonly Image _iconExit = Image.FromFile("Resources/icon_exit.ico");
        private readonly Image _iconGitHub = Image.FromFile("Resources/icon_github.ico");
        private readonly Image _iconRefresh = Image.FromFile("Resources/icon_refresh.ico");
        private readonly Image _iconSelected = Image.FromFile("Resources/icon_selected.ico");

        private readonly ContextMenuStrip _contextMenu = new();

        private readonly NotifyIcon _trayIcon;

        internal QuickSwitchApp()
        {
            _trayIcon = new NotifyIcon
            {
                Text = Constants.AppName,
                Icon = _iconApp,
                ContextMenuStrip = _contextMenu,
                Visible = true,
            };

            /*
            Because of the way the tray icon is designed, it's a nightmare
            to write a left-click event which actually does the same thing
            as the default right-click event (positioning, auto-close etc).

            So we have to get the default ShowContextMenu method using
            reflection (because for some reason it's marked internal),
            and then call that in the left-click event bound to the icon.

            Note we only get the method once and then close over the method
            reference in the event handler, so we're not doing slow dynamic
            reflection on every click.
            */

            var showContextMenu = typeof(NotifyIcon)
                .GetMethod("ShowContextMenu", BindingFlags.Instance | BindingFlags.NonPublic);

            _trayIcon.Click += (sender, e) => showContextMenu?.Invoke(_trayIcon, null);

            _refreshTimer.Tick += async (sender, e) => await Update();

            _updateTimer.Tick += async (sender, e) =>
            {
                _updateTimer.Stop();

                try
                {
                    await VersionHelpers.UpdateAsync();
                    BuildMenu();
                }
                finally
                {
                    _updateTimer.Start();
                }
            };

            Init();
        }

        private async void Init()
        {
            await NodeHelpers.UpdateAsync();
            BuildMenu();

            _refreshTimer.Start();
            _updateTimer.Start();
        }

        private void BuildMenu()
        {
            _contextMenu.Items.Clear();

            _contextMenu.Items.Add(new ToolStripLabel($"{Constants.AppName} ({VersionHelpers.GetLocalVersion()})")
            {
                Enabled = false,
            });

            var updateVersion = VersionHelpers.GetUpdateVersion();

            if (!string.IsNullOrWhiteSpace(updateVersion))
            {
                _contextMenu.Items.Add(
                    $"New version available ({updateVersion})",
                    _iconAlert,
                    (sender, e) => OpenUrl(Constants.LatestReleaseUrl)
                );

                _trayIcon.Icon = _iconAppAlert;
            }
            else
            {
                _trayIcon.Icon = _iconApp;
            }

            _contextMenu.Items.Add("View on GitHub", _iconGitHub, (sender, e) => OpenUrl(Constants.AppUrl));
            _contextMenu.Items.Add("-");

            foreach (var nodeVersion in NodeHelpers.GetAvailableNodeVersions())
            {
                var image = nodeVersion.IsActive ? _iconSelected : null;

                _contextMenu.Items.Add(nodeVersion.DisplayName, image, async (sender, e) => await OnNodeVersionSelected(nodeVersion));
            }

            _contextMenu.Items.Add("-");

            _contextMenu.Items.Add("Install", _iconAdd, async (sender, e) =>
            {
                _refreshTimer.Stop();

                try
                {
                    var command = @"
                        $version = Read-Host 'Enter version to install'
                        & nvm install $version
                        pause
                    ";

                    using var process = Process.Start(new ProcessStartInfo
                    {
                        FileName = "powershell.exe",
                        Arguments = $"-Command \"{command}\"",
                        UseShellExecute = true,
                        Verb = "runas"
                    });

                    if (process != null)
                    {
                        await process.WaitForExitAsync();

                        if (process.ExitCode == 0)
                        {
                            await Update();
                        }
                    }
                    else
                    {
                        MessageBox.Show("Could not start PowerShell.");
                    }
                }
                finally
                {
                    _refreshTimer.Start();
                }
            });

            _contextMenu.Items.Add("-");
            _contextMenu.Items.Add("Refresh", _iconRefresh, async (sender, e) => await Refresh());
            _contextMenu.Items.Add("-");
            _contextMenu.Items.Add("Exit", _iconExit, (sender, e) => Exit());
        }

        private async Task OnNodeVersionSelected(NodeVersionModel version)
        {
            _refreshTimer.Stop();

            try
            {
                var output = await NodeHelpers.SetNodeVersionAsync(version.Version);

                await NodeHelpers.UpdateAsync();

                BuildMenu();

                _trayIcon.ShowBalloonTip(
                    Constants.NotificationDuration,
                    "Node version changed",
                    output,
                    ToolTipIcon.Info
                );
            }
            finally
            {
                _refreshTimer.Start();
            }
        }

        private async Task Refresh()
        {
            _refreshTimer.Stop();

            try
            {
                var update = await NodeHelpers.UpdateAsync();
                var activeNodeVersion = update.AvailableNodeVersions.FirstOrDefault(x => x.IsActive);

                BuildMenu();

                _trayIcon.ShowBalloonTip(
                    Constants.NotificationDuration,
                    "Refreshed successfully",
                    $"Active node version is {activeNodeVersion?.Version ?? "none"}",
                    ToolTipIcon.Info
                );
            }
            finally
            {
                _refreshTimer.Start();
            }
        }

        private async Task Update()
        {
            _refreshTimer.Stop();

            try
            {
                var update = await NodeHelpers.UpdateAsync();
                var summary = update.GetSummary();

                if (summary.Count != 0)
                {
                    BuildMenu();

                    _trayIcon.ShowBalloonTip(
                        Constants.NotificationDuration,
                        "NVM was updated",
                        string.Join(Environment.NewLine, summary),
                        ToolTipIcon.Info
                    );
                }
            }
            finally
            {
                _refreshTimer.Start();
            }
        }

        private void Exit()
        {
            _trayIcon.Visible = false;
            _trayIcon.Dispose();

            Application.Exit();
        }

        private static void OpenUrl(string url)
        {
            Process.Start(new ProcessStartInfo(url)
            {
                UseShellExecute = true,
            });
        }
    }
}
