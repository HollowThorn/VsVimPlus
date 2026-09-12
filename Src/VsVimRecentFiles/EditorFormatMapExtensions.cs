using System.Windows.Media;
using Microsoft.VisualStudio.Text.Classification;

namespace Vim.VisualStudio.RecentFiles
{
    /// <summary>
    /// Small local copies of the IEditorFormatMap color-lookup helpers VsVimShared also has;
    /// duplicated here (rather than referencing VsVimShared) to keep this project independent
    /// </summary>
    internal static class EditorFormatMapExtensions
    {
        internal static Color GetBackgroundColor(this IEditorFormatMap map, string name, Color defaultColor)
        {
            var properties = map.GetProperties(name);
            var key = EditorFormatDefinition.BackgroundColorId;
            return properties != null && properties.Contains(key) ? (Color)properties[key] : defaultColor;
        }

        internal static Brush GetBackgroundBrush(this IEditorFormatMap map, string name, Color defaultColor) =>
            new SolidColorBrush(GetBackgroundColor(map, name, defaultColor));

        internal static Color GetForegroundColor(this IEditorFormatMap map, string name, Color defaultColor)
        {
            var properties = map.GetProperties(name);
            var key = EditorFormatDefinition.ForegroundColorId;
            return properties != null && properties.Contains(key) ? (Color)properties[key] : defaultColor;
        }

        internal static Brush GetForegroundBrush(this IEditorFormatMap map, string name, Color defaultColor) =>
            new SolidColorBrush(GetForegroundColor(map, name, defaultColor));
    }
}
