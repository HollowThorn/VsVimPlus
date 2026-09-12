using System.ComponentModel.Composition;

namespace Vim.VisualStudio.Implementation.Misc
{
    /// <summary>
    /// Applies the "use system clipboard as unnamed register" application setting to Vim's
    /// 'clipboard' global setting.
    ///
    /// This is intentionally one-directional when the setting is off: we only ever add the
    /// Unnamed flag when the setting is on, never remove it just because the setting is off.
    /// That keeps this from fighting with a 'clipboard' value a user already configured
    /// themselves via a vimrc, which is how this behavior already worked before this option
    /// existed. Explicitly toggling the option off in Tools &gt; Options does clear the flag
    /// though, since that's a direct user action on this specific control.
    /// </summary>
    [Export(typeof(IVimBufferCreationListener))]
    internal sealed class ClipboardSettingSync : IVimBufferCreationListener
    {
        private readonly IVimApplicationSettings _vimApplicationSettings;
        private IVimGlobalSettings _globalSettings;
        private bool _subscribed;

        [ImportingConstructor]
        internal ClipboardSettingSync(IVimApplicationSettings vimApplicationSettings)
        {
            _vimApplicationSettings = vimApplicationSettings;
        }

        private void Apply()
        {
            if (_globalSettings == null)
            {
                return;
            }

            if (_vimApplicationSettings.UseSystemClipboardAsUnnamed)
            {
                _globalSettings.ClipboardOptions |= ClipboardOptions.Unnamed;
            }
        }

        private void OnSettingsChanged(object sender, ApplicationSettingsEventArgs e)
        {
            if (_globalSettings == null)
            {
                return;
            }

            if (_vimApplicationSettings.UseSystemClipboardAsUnnamed)
            {
                _globalSettings.ClipboardOptions |= ClipboardOptions.Unnamed;
            }
            else
            {
                _globalSettings.ClipboardOptions &= ~ClipboardOptions.Unnamed;
            }
        }

        void IVimBufferCreationListener.VimBufferCreated(IVimBuffer vimBuffer)
        {
            _globalSettings = vimBuffer.GlobalSettings;

            if (!_subscribed)
            {
                _subscribed = true;
                _vimApplicationSettings.SettingsChanged += OnSettingsChanged;
            }

            Apply();
        }
    }
}
