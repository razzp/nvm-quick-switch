using NVMQuickSwitch.Models;
using NVMQuickSwitch.Services;

namespace NVMQuickSwitch.Helpers
{
    public static class NodeHelpers
    {
        private static IEnumerable<NodeVersionModel> _availableNodeVersions = [];

        public static async Task<UpdateInformationModel> UpdateAsync()
        {
            var output = await PowerShellRunner.RunAsync("nvm list");

            var latestNodeVersions = output
                .Split(['\r', '\n'], StringSplitOptions.RemoveEmptyEntries)
                .Select(NodeVersionModel.FromLine);

            var cachedNodeVersionsHash = new HashSet<string>(_availableNodeVersions.Select(b => b.Version));
            var latestNodeVersionsHash = new HashSet<string>(latestNodeVersions.Select(b => b.Version));

            var added = latestNodeVersions.Where(nv => !cachedNodeVersionsHash.Contains(nv.Version)).ToList();
            var removed = _availableNodeVersions.Where(nv => !latestNodeVersionsHash.Contains(nv.Version)).ToList();

            var activeNow = _availableNodeVersions.FirstOrDefault(nv => nv.IsActive)?.Version;
            var activeNext = latestNodeVersions.FirstOrDefault(nv => nv.IsActive)?.Version;

            _availableNodeVersions = latestNodeVersions;

            return new UpdateInformationModel(
                availableNodeVersions: latestNodeVersions,
                addedNodeVersions: added,
                removedNodeVersions: removed,
                activeVersionHasChanged: activeNow != activeNext
            );
        }

        public static async Task<string> SetNodeVersionAsync(string version) =>
            await PowerShellRunner.RunAsync($"nvm use {version}");

        public static IEnumerable<NodeVersionModel> GetAvailableNodeVersions() =>
            _availableNodeVersions;
    }
}
