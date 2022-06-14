using NVMQuickSwitch.Models;
using System.Diagnostics;

namespace NVMQuickSwitch.Functions
{
    internal static class NvmFunctions
    {
        private static IEnumerable<NodeVersionModel> _installedNodeVersions
            = Enumerable.Empty<NodeVersionModel>();

        internal static IEnumerable<NodeVersionModel> RefreshNodeVersions()
        {
            var output = RunCommand("nvm list");

            _installedNodeVersions = output
                .Split(new char[] { '\r', '\n' }, StringSplitOptions.RemoveEmptyEntries)
                .Select(line => NodeVersionModel.FromLine(line));

            return _installedNodeVersions;
        }

        internal static IEnumerable<NodeVersionModel> GetNodeVersions() =>
            _installedNodeVersions;

        internal static string SetNodeVersion(string version)
        {
            return RunCommand($"nvm use {version}");
        }

        private static string RunCommand(string command)
        {
            var process = new Process()
            {
                StartInfo = new ProcessStartInfo("powershell", command)
                {
                    UseShellExecute = false,
                    RedirectStandardOutput = true,
                    CreateNoWindow = true,
                },
            };

            process.Start();

            var output = process.StandardOutput.ReadToEnd();

            process.WaitForExit();
            process.Close();

            return output;
        }
    }
}
