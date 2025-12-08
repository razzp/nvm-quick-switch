namespace NVMQuickSwitch.Models
{
    public class UpdateInformationModel(
        IEnumerable<NodeVersionModel> availableNodeVersions,
        List<NodeVersionModel> addedNodeVersions,
        List<NodeVersionModel> removedNodeVersions,
        bool activeVersionHasChanged
        )
    {
        public IEnumerable<NodeVersionModel> AvailableNodeVersions { get; } = availableNodeVersions;

        public List<NodeVersionModel> AddedNodeVersions { get; } = addedNodeVersions;

        public List<NodeVersionModel> RemovedNodeVersions { get; } = removedNodeVersions;

        public bool ActiveVersionHasChanged { get; } = activeVersionHasChanged;

        public List<string> GetSummary()
        {
            var summary = new List<string>();

            if (AddedNodeVersions.Count != 0)
            {
                summary.Add($"Added: {string.Join(", ", AddedNodeVersions.Select(x => x.Version))}");
            }

            if (RemovedNodeVersions.Count != 0)
            {
                summary.Add($"Removed: {string.Join(", ", RemovedNodeVersions.Select(x => x.Version))}");
            }

            if (ActiveVersionHasChanged)
            {
                summary.Add($"Active node version: {AvailableNodeVersions.FirstOrDefault(x => x.IsActive)?.Version ?? "none"}");
            }

            return summary;
        }
    }
}
