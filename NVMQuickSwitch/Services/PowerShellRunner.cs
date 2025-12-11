using System.Management.Automation;

namespace NVMQuickSwitch.Services
{
    public static class PowerShellRunner
    {
        public static async Task<string> RunAsync(string script)
        {
            using var ps = PowerShell.Create();

            ps.AddScript(script);

            var results = await ps.InvokeAsync().ConfigureAwait(false);

            return string.Join(Environment.NewLine, results.Select(r => r.ToString()));
        }
    }
}