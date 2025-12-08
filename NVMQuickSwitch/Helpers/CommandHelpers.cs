using System.Diagnostics;

namespace NVMQuickSwitch.Helpers
{
    public static class CommandHelpers
    {
        private static readonly string _executable = GetExecutable();

        public static async Task<string> RunAsync(string command)
        {
            var process = new Process()
            {
                StartInfo = new ProcessStartInfo(_executable, $"-NoProfile {command}")
                {
                    UseShellExecute = false,
                    RedirectStandardOutput = true,
                    CreateNoWindow = true,
                },
            };

            process.Start();

            var output = await process.StandardOutput.ReadToEndAsync();

            await process.WaitForExitAsync();

            return output;
        }

        private static string GetExecutable()
        {
            try
            {
                var test = Process.Start(new ProcessStartInfo("pwsh", "-NoLogo")
                {
                    UseShellExecute = false,
                    CreateNoWindow = true
                });

                test?.Kill();

                return "pwsh";
            }
            catch
            {
                return "powershell";
            }
        }
    }
}
