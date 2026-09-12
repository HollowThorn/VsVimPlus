using System.Collections.Generic;
using Vim;

namespace Vim.VisualStudio.Implementation.Settings
{
    /// <summary>
    /// The families of modes that can be independently colored in the command margin.  Several
    /// ModeKind values (e.g. the various visual/select sub modes) share a single color so the
    /// options UI and persisted settings don't need a row per ModeKind
    /// </summary>
    internal enum ModeColorFamily
    {
        Normal,
        Insert,
        Visual,
        Replace,
        Command
    }

    internal static class ModeColorDefaults
    {
        /// <summary>
        /// Default hex color for each mode family, loosely inspired by common Neovim statusline
        /// color schemes
        /// </summary>
        internal static readonly IReadOnlyDictionary<ModeColorFamily, string> Colors = new Dictionary<ModeColorFamily, string>
        {
            { ModeColorFamily.Normal, "#005F87" },
            { ModeColorFamily.Insert, "#2E7D32" },
            { ModeColorFamily.Visual, "#6A1B9A" },
            { ModeColorFamily.Replace, "#C62828" },
            { ModeColorFamily.Command, "#EF6C00" },
        };

        /// <summary>
        /// Which family, if any, a given mode belongs to.  Modes with no family (e.g.
        /// Uninitialized, ExternalEdit) are never mode-colored
        /// </summary>
        internal static ModeColorFamily? GetFamily(ModeKind modeKind)
        {
            switch (modeKind)
            {
                case ModeKind.Normal:
                    return ModeColorFamily.Normal;
                case ModeKind.Insert:
                    return ModeColorFamily.Insert;
                case ModeKind.VisualCharacter:
                case ModeKind.VisualLine:
                case ModeKind.VisualBlock:
                case ModeKind.SelectCharacter:
                case ModeKind.SelectLine:
                case ModeKind.SelectBlock:
                    return ModeColorFamily.Visual;
                case ModeKind.Replace:
                case ModeKind.SubstituteConfirm:
                    return ModeColorFamily.Replace;
                case ModeKind.Command:
                    return ModeColorFamily.Command;
                default:
                    return null;
            }
        }

        internal static string GetDefaultColor(ModeColorFamily family) => Colors[family];
    }
}
