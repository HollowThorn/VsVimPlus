using System;
using System.Windows.Media;

namespace Vim.UI.Wpf.Implementation.CommandMargin
{
    /// <summary>
    /// Optional source of per-mode command margin colors.  This lives in Vim.UI.Wpf (shared by
    /// every host, including the standalone VimApp) rather than depending on a host-specific
    /// settings type, so it is imported with AllowDefault and simply does nothing when no host
    /// provides an implementation (e.g. the Visual Studio host, via VsVimShared).
    /// </summary>
    public interface ICommandMarginColorSettings
    {
        /// <summary>
        /// Whether the command margin should be colored based on the current Vim mode
        /// </summary>
        bool IsEnabled { get; }

        /// <summary>
        /// Get the configured background color for the given mode, if any
        /// </summary>
        bool TryGetColor(ModeKind modeKind, out Color backgroundColor);

        /// <summary>
        /// Raised when IsEnabled or any mode color changes
        /// </summary>
        event EventHandler Changed;
    }
}
