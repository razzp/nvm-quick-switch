using NVMQuickSwitch.Functions;
using System.Diagnostics;
using System.Reflection;

namespace NVMQuickSwitch
{
    internal class QuickSwitchApp : ApplicationContext
    {
        private const string AppName = "NVM Quick Switch";
        private const string AppVersion = "1.2.0";
        private const string AppURL = "https://github.com/razzp/nvm-quick-switch";

        private readonly Image iconSelected = Image.FromFile("Resources/icon-selected.ico");
        private readonly Image iconUnSelected = Image.FromFile("Resources/icon-unselected.ico");

        private readonly ContextMenuStrip contextMenu = new();

        private readonly NotifyIcon trayIcon;

        internal QuickSwitchApp()
        {
            trayIcon = new NotifyIcon
            {
                Text = AppName,
                Icon = new Icon("Resources/icon-app.ico"),
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

            NodeFunctions.RefreshNodeVersions();

            BuildMenu();
        }

        private void BuildMenu()
        {
            contextMenu.Items.Clear();

            contextMenu.Items.Add(new ToolStripLabel($"{AppName} ({AppVersion})")
            {
                Enabled = false,
            });

            contextMenu.Items.Add("View on GitHub", null, AppUrlButton_Clicked);
            contextMenu.Items.Add("-");

            foreach (var nodeVersion in NodeFunctions.GetNodeVersions())
            {
                var name = $"{nodeVersion.Version}{(nodeVersion.IsCurrent ? " (current)" : string.Empty)}";
                var image = nodeVersion.IsCurrent ? iconSelected : iconUnSelected;

                contextMenu.Items.Add(new ToolStripMenuItem(name, image, VersionButton_Clicked)
                {
                    Tag = nodeVersion.Version,
                });
            }

            contextMenu.Items.Add("-");
            contextMenu.Items.Add("Refresh installed versions", null, Refresh);

            contextMenu.Items.Add("-");
            contextMenu.Items.Add("Exit", null, Exit);
        }

        private void AppUrlButton_Clicked(object? sender, EventArgs e)
        {
            Process.Start(new ProcessStartInfo(AppURL)
            {
                UseShellExecute = true,
            });
        }

        private void VersionButton_Clicked(object? sender, EventArgs e)
        {
            if (sender is null || sender is not ToolStripMenuItem)
            {
                throw new Exception();
            }

            var output = NodeFunctions.SetNodeVersion((string)((ToolStripMenuItem)sender).Tag);

            NodeFunctions.RefreshNodeVersions();

            BuildMenu();

            trayIcon.ShowBalloonTip(3000, "Node version changed", output, ToolTipIcon.Info);
        }

        private void Refresh(object? sender, EventArgs e)
        {
            var versions = NodeFunctions.RefreshNodeVersions();

            BuildMenu();

            trayIcon.ShowBalloonTip(3000, "Node versions refreshed", $"Found versions {string.Join(", ", versions.Select(v => v.Version))}", ToolTipIcon.Info);
        }

        private void Exit(object? sender, EventArgs e)
        {
            trayIcon.Visible = false;
            trayIcon.Dispose();

            Application.Exit();
        }
    }
}
