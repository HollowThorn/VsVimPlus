using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Windows;
using System.Windows.Controls;
using Vim;
using Vim.VisualStudio.Implementation.CommandMargin;
using Vim.VisualStudio.Implementation.Settings;

namespace Vim.VisualStudio.Implementation.OptionPages
{
    /// <summary>
    /// Interaction logic for ModeColorsSettingsControl.xaml
    /// </summary>
    public partial class ModeColorsSettingsControl : UserControl
    {
        public static readonly DependencyProperty UseModeColorsProperty = DependencyProperty.Register(
            "UseModeColors",
            typeof(bool),
            typeof(ModeColorsSettingsControl));

        private readonly IVimApplicationSettings _vimApplicationSettings;
        private readonly ObservableCollection<ModeColorRow> _rows = new ObservableCollection<ModeColorRow>();

        public bool UseModeColors
        {
            get { return (bool)GetValue(UseModeColorsProperty); }
            set { SetValue(UseModeColorsProperty, value); }
        }

        public ObservableCollection<ModeColorRow> Rows
        {
            get { return _rows; }
        }

        public ModeColorsSettingsControl(IVimApplicationSettings vimApplicationSettings)
        {
            InitializeComponent();

            _vimApplicationSettings = vimApplicationSettings;
            LoadSettings();
        }

        /// <summary>
        /// Refresh the control from the current persisted settings.  Called both on construction
        /// and whenever the options page is activated, in case the settings changed elsewhere
        /// (e.g. Tools > Import and Export Settings) since the control was created
        /// </summary>
        internal void LoadSettings()
        {
            UseModeColors = _vimApplicationSettings.UseModeColors;

            _rows.Clear();
            foreach (var family in RepresentativeModeKinds)
            {
                var defaultColor = ModeColorDefaults.GetDefaultColor(family.Key);
                var currentColor = _vimApplicationSettings.GetModeColor(family.Value) ?? defaultColor;
                _rows.Add(new ModeColorRow(family.Key.ToString(), family.Value, currentColor, defaultColor));
            }
        }

        internal void Apply()
        {
            _vimApplicationSettings.UseModeColors = UseModeColors;
            foreach (var row in _rows)
            {
                _vimApplicationSettings.SetModeColor(row.ModeKind, row.HexColor);
            }
        }

        private void OnPickColorClick(object sender, RoutedEventArgs e)
        {
            if (!(((FrameworkElement)sender).Tag is ModeColorRow row))
            {
                return;
            }

            using (var dialog = new System.Windows.Forms.ColorDialog { FullOpen = true })
            {
                if (CommandMarginColorSettingsAdapter.TryParseHexColor(row.HexColor, out var current))
                {
                    dialog.Color = System.Drawing.Color.FromArgb(current.R, current.G, current.B);
                }

                if (dialog.ShowDialog() == System.Windows.Forms.DialogResult.OK)
                {
                    row.HexColor = $"#{dialog.Color.R:X2}{dialog.Color.G:X2}{dialog.Color.B:X2}";
                }
            }
        }

        private void OnRestoreDefaultsClick(object sender, RoutedEventArgs e)
        {
            foreach (var row in _rows)
            {
                row.HexColor = row.DefaultHexColor;
            }
        }

        /// <summary>
        /// One representative ModeKind per family, in display order, paired with the family used
        /// to look up its default color
        /// </summary>
        private static readonly KeyValuePair<ModeColorFamily, ModeKind>[] RepresentativeModeKinds =
        {
            new KeyValuePair<ModeColorFamily, ModeKind>(ModeColorFamily.Normal, ModeKind.Normal),
            new KeyValuePair<ModeColorFamily, ModeKind>(ModeColorFamily.Insert, ModeKind.Insert),
            new KeyValuePair<ModeColorFamily, ModeKind>(ModeColorFamily.Visual, ModeKind.VisualCharacter),
            new KeyValuePair<ModeColorFamily, ModeKind>(ModeColorFamily.Replace, ModeKind.Replace),
            new KeyValuePair<ModeColorFamily, ModeKind>(ModeColorFamily.Command, ModeKind.Command),
        };
    }
}
