namespace NVMQuickSwitch.Helpers
{
    public static class VersionHelpers
    {
        private static readonly HttpClient _httpClient = new();
        private static readonly Version _localVersion = new(File.ReadAllText("VERSION").Trim());
        private static Version? _remoteVersion;

        public static async Task UpdateAsync()
        {
            try
            {
                var timeout = TimeSpan.FromSeconds(10);
                var url = Constants.LatestVersionUrl;
                var latestVersion = await _httpClient.GetStringAsync(url);

                _remoteVersion = new Version(latestVersion.Trim());
            }
            catch
            {
                _remoteVersion = null;
            }
        }

        public static string GetLocalVersion() =>
            _localVersion.ToString();

        public static string? GetUpdateVersion() =>
            _remoteVersion != null && _remoteVersion > _localVersion
                ? _remoteVersion.ToString()
                : null;
    }
}
