using System.Windows.Media;

namespace Vim.UI.Wpf.Implementation.CommandMargin
{
    internal static class ColorContrastUtil
    {
        /// <summary>
        /// Pick a black or white foreground that is readable against the given background
        /// color, using the standard relative luminance formula
        /// </summary>
        internal static Color GetReadableForeground(Color background)
        {
            var luminance =
                0.299 * background.R +
                0.587 * background.G +
                0.114 * background.B;

            return luminance > 150 ? Colors.Black : Colors.White;
        }
    }
}
