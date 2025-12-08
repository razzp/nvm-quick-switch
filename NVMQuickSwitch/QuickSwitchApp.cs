using NVMQuickSwitch.Helpers;
using System.Diagnostics;
using System.Reflection;

namespace NVMQuickSwitch
{
    internal class QuickSwitchApp : ApplicationContext
    {
        private readonly System.Windows.Forms.Timer refreshTimer = new()
        {
            Interval = Constants.RefreshInterval,
        };

        private readonly System.Windows.Forms.Timer updateTimer = new()
        {
            Interval = Constants.UpdateCheckInterval,
        };

        private readonly Icon iconApp = new("Resources/icon_app.ico");
        private readonly Icon iconAppAlert = new("Resources/icon_app-alert.ico");

        private readonly Image iconAlert = Image.FromFile("Resources/icon_alert.ico");
        private readonly Image iconExit = Image.FromFile("Resources/icon_exit.ico");
        private readonly Image iconGitHub = Image.FromFile("Resources/icon_github.ico");
        private readonly Image iconRefresh = Image.FromFile("Resources/icon_refresh.ico");
        private readonly Image iconSelected = Image.FromFile("Resources/icon_selected.ico");

        private readonly ContextMenuStrip contextMenu = new();

        private readonly NotifyIcon trayIcon;

        internal QuickSwitchApp()
        {
            trayIcon = new NotifyIcon
            {
                Text = Constants.AppName,
                Icon = iconApp,
                ContextMenuStrip = contextMenu,
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

            trayIcon.Click += (sender, e) => showContextMenu?.Invoke(trayIcon, null);

            Init();
        }

        private async void Init()
        {
            await NodeHelpers.UpdateAsync();
            BuildMenu();

            refreshTimer.Tick += async (sender, e) =>
            {
                refreshTimer.Stop();

                try
                {
                    var update = await NodeHelpers.UpdateAsync();
                    var summary = update.GetSummary();

                    if (summary.Count != 0)
                    {
                        BuildMenu();

                        trayIcon.ShowBalloonTip(
                            Constants.NotificationDuration,
                            "NVM was updated",
                            string.Join("\n", summary),
                            ToolTipIcon.Info
                        );
                    }
                }
                finally
                {
                    refreshTimer.Start();
                }
            };

            updateTimer.Tick += async (sender, e) =>
            {
                updateTimer.Stop();

                try
                {
                    await VersionHelpers.UpdateAsync();
                    BuildMenu();
                }
                finally
                {
                    updateTimer.Start();
                }
            };

            refreshTimer.Start();
            updateTimer.Start();
        }

        private void BuildMenu()
        {
            contextMenu.Items.Clear();

            contextMenu.Items.Add(new ToolStripLabel($"{Constants.AppName} ({VersionHelpers.GetLocalVersion()})")
            {
                Enabled = false,
            });

            var updateVersion = VersionHelpers.GetUpdateVersion();

            if (!string.IsNullOrWhiteSpace(updateVersion))
            {
                contextMenu.Items.Add(
                    $"New version available ({updateVersion})",
                    iconAlert,
                    (sender, e) => OpenUrl(Constants.LatestReleaseUrl)
                );

                trayIcon.Icon = iconAppAlert;
            }
            else
            {
                trayIcon.Icon = iconApp;
            }

            contextMenu.Items.Add("View on GitHub", iconGitHub, (sender, e) => OpenUrl(Constants.AppUrl));
            contextMenu.Items.Add("-");

            foreach (var nodeVersion in NodeHelpers.GetAvailableNodeVersions())
            {
                var image = nodeVersion.IsActive ? iconSelected : null;

                contextMenu.Items.Add(new ToolStripMenuItem(nodeVersion.DisplayName, image, VersionButton_Clicked)
                {
                    Tag = nodeVersion.Version,
                });
            }

            contextMenu.Items.Add("-");
            contextMenu.Items.Add("Refresh", iconRefresh, Refresh);

            contextMenu.Items.Add("-");
            contextMenu.Items.Add("Exit", iconExit, Exit);
        }

        private async void VersionButton_Clicked(object? sender, EventArgs e)
        {
            if (sender is not ToolStripMenuItem menuItem || menuItem.Tag is not string version)
            {
                throw new Exception();
            }

            refreshTimer.Stop();

            try
            {
                var output = await NodeHelpers.SetNodeVersionAsync(version);

                await NodeHelpers.UpdateAsync();

                BuildMenu();

                trayIcon.ShowBalloonTip(
                    Constants.NotificationDuration,
                    "Node version changed",
                    output,
                    ToolTipIcon.Info
                );
            }
            finally
            {
                refreshTimer.Start();
            }
        }

        private async void Refresh(object? sender, EventArgs e)
        {
            refreshTimer.Stop();

            try
            {
                var update = await NodeHelpers.UpdateAsync();
                var activeNodeVersion = update.AvailableNodeVersions.FirstOrDefault(x => x.IsActive);

                BuildMenu();

                trayIcon.ShowBalloonTip(
                    Constants.NotificationDuration,
                    "Refreshed successfully",
                    $"Active node version is {activeNodeVersion?.Version ?? "none"}",
                    ToolTipIcon.Info
                );
            }
            finally
            {
                refreshTimer.Start();
            }
        }

        private void Exit(object? sender, EventArgs e)
        {
            trayIcon.Visible = false;
            trayIcon.Dispose();

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
