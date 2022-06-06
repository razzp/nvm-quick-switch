using NVMQuickSwitch.Models;

namespace NVMQuickSwitch.Functions
{
    internal class NvmFunctions
    {
        internal static IEnumerable<NodeVersionModel> GetNodeVersions()
        {
            // Get the list of available Node versions.
            var output = CmdFunctions.RunCommand("nvm list");

            return output
                // Split the ouput on carriage return and newline characters.
                .Split(new char[] { '\r', '\n' }, StringSplitOptions.RemoveEmptyEntries)
                // Map each line to a `NodeVersionModel`.
                .Select(line => NodeVersionModel.FromLine(line));
        }

        internal static string SetNodeVersion(string version) => CmdFunctions.RunCommand($"nvm use {version}");
    }
}
