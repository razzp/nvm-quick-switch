using System.Text.RegularExpressions;

namespace NVMQuickSwitch.Models
{
    internal class NodeVersionModel
    {
        private static readonly Regex VersionRegex = new(
            // Optionally match an asterisk, which indicates the version is active.
            @"^\s*(\*)?\s*" +
            // Match the version number. This is adapted from SemVer's recommended RegEx.
            // https://semver.org/#is-there-a-suggested-regular-expression-regex-to-check-a-semver-string
            @"((?:0|[1-9]\d*)\.(?:0|[1-9]\d*)\.(?:0|[1-9]\d*)(?:-(?:0|[1-9]\d*|\d*[a-zA-Z-][0-9a-zA-Z-]*)(?:\.(?:0|[1-9]\d*|\d*[a-zA-Z-][0-9a-zA-Z-]*))*)?(?:\+(?:[0-9a-zA-Z-]+(?:\.[0-9a-zA-Z-]+)*))?)" +
            // Optionally match the architecture (this is only shown on the active version).
            @"(?:.+((?:64|32)-bit))?"
        );

        internal NodeVersionModel(string? architecture, bool isCurrent, string version)
        {
            Architecture = architecture;
            IsCurrent = isCurrent;
            Version = version ?? throw new ArgumentNullException(nameof(version));
        }

        internal string? Architecture { get; }

        internal bool IsCurrent { get; }

        internal string Version { get; }

        internal static NodeVersionModel FromLine(string line)
        {
            var match = VersionRegex.Match(line);

            if (!match.Success)
            {
                throw new Exception($"Failed to parse line: {line}");
            }

            return new NodeVersionModel(
                isCurrent: match.Groups[1].Success,
                version: match.Groups[2].Value,
                architecture: !match.Groups[3].Success ? default : match.Groups[3].Value
            );
        }

        internal string GetDisplayName()
        {
            var tags = new List<string> { Version };

            if (Architecture is not null)
            {
                tags.Add($"({Architecture})");
            }

            if (IsCurrent)
            {
                tags.Add("(current)");
            }

            return string.Join(" ", tags);
        }
    }
}
