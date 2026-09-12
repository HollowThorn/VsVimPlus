using System;
using System.ComponentModel.Composition;
using System.Globalization;
using System.Windows.Media;
using Vim;
using Vim.UI.Wpf.Implementation.CommandMargin;

namespace Vim.VisualStudio.Implementation.CommandMargin
{
    /// <summary>
    /// Bridges the Visual Studio specific IVimApplicationSettings (persisted settings, Options
    /// pages) to the host-neutral ICommandMarginColorSettings that Vim.UI.Wpf's command margin
    /// consumes
    /// </summary>
    [Export(typeof(ICommandMarginColorSettings))]
    internal sealed class CommandMarginColorSettingsAdapter : ICommandMarginColorSettings
    {
        private readonly IVimApplicationSettings _vimApplicationSettings;

        internal event EventHandler ChangedImpl;

        [ImportingConstructor]
        internal CommandMarginColorSettingsAdapter(IVimApplicationSettings vimApplicationSettings)
        {
            _vimApplicationSettings = vimApplicationSettings;
            _vimApplicationSettings.SettingsChanged += OnSettingsChanged;
        }

        private void OnSettingsChanged(object sender, ApplicationSettingsEventArgs e)
        {
            ChangedImpl?.Invoke(this, EventArgs.Empty);
        }

        bool ICommandMarginColorSettings.IsEnabled => _vimApplicationSettings.UseModeColors;

        bool ICommandMarginColorSettings.TryGetColor(ModeKind modeKind, out Color backgroundColor)
        {
            var hexColor = _vimApplicationSettings.GetModeColor(modeKind);
            return TryParseHexColor(hexColor, out backgroundColor);
        }

        event EventHandler ICommandMarginColorSettings.Changed
        {
            add { ChangedImpl += value; }
            remove { ChangedImpl -= value; }
        }

        internal static bool TryParseHexColor(string hexColor, out Color color)
        {
            color = default;
            if (string.IsNullOrEmpty(hexColor) || hexColor.Length != 7 || hexColor[0] != '#')
            {
                return false;
            }

            if (!byte.TryParse(hexColor.Substring(1, 2), NumberStyles.HexNumber, CultureInfo.InvariantCulture, out var r) ||
                !byte.TryParse(hexColor.Substring(3, 2), NumberStyles.HexNumber, CultureInfo.InvariantCulture, out var g) ||
                !byte.TryParse(hexColor.Substring(5, 2), NumberStyles.HexNumber, CultureInfo.InvariantCulture, out var b))
            {
                return false;
            }

            color = Color.FromRgb(r, g, b);
            return true;
        }
    }
}
