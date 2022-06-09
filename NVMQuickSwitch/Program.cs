namespace NVMQuickSwitch
{
    internal static class Program
    {
        [STAThread]
        static void Main()
        {
            var fileInfo = new FileInfo(Application.ExecutablePath);

            if (fileInfo.DirectoryName is not null)
            {
                // If the app is launched via the registry then the current directory is
                // changed to `C:\Windows\System32`, which breaks all the relative paths.
                // I'm not sure why this happens, but here's the solution.
                Directory.SetCurrentDirectory(fileInfo.DirectoryName);
            }

            ApplicationConfiguration.Initialize();
            Application.Run(new QuickSwitchApp());
        }
    }
}