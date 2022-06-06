using System.Diagnostics;

namespace NVMQuickSwitch.Functions
{
    internal class CmdFunctions
    {
        internal static string RunCommand(string command)
        {
            var process = new Process()
            {
                StartInfo = new ProcessStartInfo("cmd", $"/c {command}")
                {
                    UseShellExecute = false,
                    RedirectStandardOutput = true,
                    CreateNoWindow = true,
                },
            };

            // Run the command.
            process.Start();

            // Read the resulting output of the command. 
            var output = process.StandardOutput.ReadToEnd();

            // Close the process once finished.
            process.WaitForExit();
            process.Close();

            // Return the output.
            return output;
        }
    }
}
