using NVMQuickSwitch.Helpers;
using System.Text.RegularExpressions;

namespace NVMQuickSwitch.Models
{
    public class NodeVersionModel(
        string? architecture,
        bool isActive,
        string version
        )
    {
        private static readonly Regex _nvmVersionRegex = RegexHelpers.NVMVersionRegex();

        public string? Architecture { get; } = architecture;

        public bool IsActive { get; } = isActive;

        public string Version { get; } = version;

        public string DisplayName
        {
            get
            {
                var tags = new List<string> { Version };

                if (Architecture is not null)
                {
                    tags.Add($"({Architecture})");
                }

                if (IsActive)
                {
                    tags.Add("(active)");
                }

                return string.Join(" ", tags);
            }
        }

        public static NodeVersionModel FromLine(string line)
        {
            var match = _nvmVersionRegex.Match(line);

            if (!match.Success)
            {
                throw new Exception($"Failed to parse line: {line}");
            }

            return new NodeVersionModel(
                architecture: !match.Groups[3].Success ? default : match.Groups[3].Value,
                isActive: match.Groups[1].Success,
                version: match.Groups[2].Value
            );
        }
    }
}
