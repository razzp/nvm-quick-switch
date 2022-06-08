using NVMQuickSwitch.Models;
using System.Diagnostics;

namespace NVMQuickSwitch.Functions
{
    internal class NvmFunctions
    {
        internal static IEnumerable<NodeVersionModel> GetNodeVersions()
        {
            var output = RunCommand("nvm list");

            return output
                .Split(new char[] { '\r', '\n' }, StringSplitOptions.RemoveEmptyEntries)
                .Select(line => NodeVersionModel.FromLine(line));
        }

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
