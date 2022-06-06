﻿using System.Text.RegularExpressions;

namespace NVMQuickSwitch.Models
{
    internal class NodeVersionModel
    {
        internal NodeVersionModel(bool isCurrent, string version)
        {
            IsCurrent = isCurrent;
            Version = version ?? throw new ArgumentNullException(nameof(version));
        }

        internal bool IsCurrent { get; }

        internal string Version { get; }

        internal static NodeVersionModel FromLine(string line)
        {
            // Use a Regular Expression to derive the version number.
            var match = Regex.Match(line, @"^\s*(\*)?\s*(\d+\.\d+\.\d+)");

            if (!match.Success)
            {
                // A version couldn't be found, so throw.
                throw new Exception($"Failed to parse line: {line}");
            }

            return new NodeVersionModel(
                isCurrent: match.Groups[1].Success,
                version: match.Groups[2].Value);
        }
    }
}