using System.ComponentModel.Composition;
using System.Windows.Media;
using Microsoft.VisualStudio.Text.Classification;
using Microsoft.VisualStudio.Utilities;

namespace Vim.VisualStudio.RecentFiles
{
    internal static class RecentFilesFormatDefinitionNames
    {
        internal const string Chrome = "VsVim Recent Files";
        internal const string Selection = "VsVim Recent Files Selection";
        internal const string SearchBox = "VsVim Recent Files Search Box";
    }

    /// <summary>
    /// Background / foreground for the recent files picker. Tracks the current Visual Studio
    /// color theme via IEditorFormatMap; the colors below are only the fallback used before the
    /// theme is applied
    /// </summary>
    [Export(typeof(EditorFormatDefinition))]
    [Name(RecentFilesFormatDefinitionNames.Chrome)]
    [UserVisible(true)]
    internal sealed class RecentFilesChromeFormatDefinition : EditorFormatDefinition
    {
        internal static readonly Color DefaultBackgroundColor = Color.FromRgb(0x25, 0x25, 0x26);
        internal static readonly Color DefaultForegroundColor = Color.FromRgb(0xCC, 0xCC, 0xCC);

        internal RecentFilesChromeFormatDefinition()
        {
            DisplayName = "VsVim Recent Files";
            BackgroundColor = DefaultBackgroundColor;
            ForegroundColor = DefaultForegroundColor;
        }
    }

    /// <summary>
    /// Background for the search box itself, a shade lighter than the picker's chrome so the
    /// input field reads as its own control, similar to Visual Studio's own search boxes (e.g.
    /// the toolbar "Search" box)
    /// </summary>
    [Export(typeof(EditorFormatDefinition))]
    [Name(RecentFilesFormatDefinitionNames.SearchBox)]
    [UserVisible(true)]
    internal sealed class RecentFilesSearchBoxFormatDefinition : EditorFormatDefinition
    {
        internal static readonly Color DefaultBackgroundColor = Color.FromRgb(0x3C, 0x3C, 0x3C);
        internal static readonly Color DefaultBorderColor = Color.FromRgb(0x5A, 0x5A, 0x5A);

        internal RecentFilesSearchBoxFormatDefinition()
        {
            DisplayName = "VsVim Recent Files Search Box";
            BackgroundColor = DefaultBackgroundColor;
            ForegroundColor = DefaultBorderColor;
        }
    }

    /// <summary>
    /// Background for the currently selected row in the recent files picker
    /// </summary>
    [Export(typeof(EditorFormatDefinition))]
    [Name(RecentFilesFormatDefinitionNames.Selection)]
    [UserVisible(true)]
    internal sealed class RecentFilesSelectionFormatDefinition : EditorFormatDefinition
    {
        internal static readonly Color DefaultBackgroundColor = Color.FromRgb(0x09, 0x4F, 0x8C);

        internal RecentFilesSelectionFormatDefinition()
        {
            DisplayName = "VsVim Recent Files Selection";
            BackgroundColor = DefaultBackgroundColor;
        }
    }
}
