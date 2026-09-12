using System.Windows;
using System.Windows.Media;
using Vim;
using Vim.VisualStudio.Implementation.CommandMargin;

namespace Vim.VisualStudio.Implementation.OptionPages
{
    /// <summary>
    /// One row of the Mode Colors options page: a mode family (e.g. "Visual", which covers
    /// several ModeKind values that all share one color), its currently configured hex color,
    /// and a live color swatch for that hex value
    /// </summary>
    public sealed class ModeColorRow : DependencyObject
    {
        public static readonly DependencyProperty HexColorProperty = DependencyProperty.Register(
            "HexColor",
            typeof(string),
            typeof(ModeColorRow),
            new PropertyMetadata(string.Empty, OnHexColorChanged));

        public static readonly DependencyProperty SwatchBrushProperty = DependencyProperty.Register(
            "SwatchBrush",
            typeof(Brush),
            typeof(ModeColorRow),
            new PropertyMetadata(Brushes.Transparent));

        public string FamilyName { get; }

        /// <summary>
        /// A representative ModeKind for this family; used to read/write the color through
        /// IVimApplicationSettings.Get/SetModeColor
        /// </summary>
        public ModeKind ModeKind { get; }

        public string DefaultHexColor { get; }

        public string HexColor
        {
            get { return (string)GetValue(HexColorProperty); }
            set { SetValue(HexColorProperty, value); }
        }

        public Brush SwatchBrush
        {
            get { return (Brush)GetValue(SwatchBrushProperty); }
            private set { SetValue(SwatchBrushProperty, value); }
        }

        public ModeColorRow(string familyName, ModeKind modeKind, string hexColor, string defaultHexColor)
        {
            FamilyName = familyName;
            ModeKind = modeKind;
            DefaultHexColor = defaultHexColor;
            HexColor = hexColor;
        }

        private static void OnHexColorChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            var row = (ModeColorRow)d;
            row.SwatchBrush = CommandMarginColorSettingsAdapter.TryParseHexColor((string)e.NewValue, out var color)
                ? new SolidColorBrush(color)
                : Brushes.Transparent;
        }
    }
}
