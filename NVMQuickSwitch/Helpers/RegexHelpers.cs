using System.Text.RegularExpressions;

namespace NVMQuickSwitch.Helpers
{
    partial class RegexHelpers
    {
        [GeneratedRegex(@"
            # Leading asterisk (optional)
            ^\s*(\*)?\s*

            # Version number
            # https://semver.org/#is-there-a-suggested-regular-expression-regex-to-check-a-semver-string
            ((?:0|[1-9]\d*)\.(?:0|[1-9]\d*)\.(?:0|[1-9]\d*)(?:-(?:0|[1-9]\d*|\d*[a-zA-Z-][0-9a-zA-Z-]*)(?:\.(?:0|[1-9]\d*|\d*[a-zA-Z-][0-9a-zA-Z-]*))*)?(?:\+(?:[0-9a-zA-Z-]+(?:\.[0-9a-zA-Z-]+)*))?)

            # Architecture (optional)
            (?:.+((?:64|32)-bit))?
        ", RegexOptions.IgnorePatternWhitespace)]
        internal static partial Regex NVMVersionRegex();
    }
}
