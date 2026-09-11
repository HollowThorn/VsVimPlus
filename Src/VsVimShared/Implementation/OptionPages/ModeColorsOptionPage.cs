using Microsoft.VisualStudio.ComponentModelHost;
using Microsoft.VisualStudio.Shell;
using System.ComponentModel;
using System.Windows.Forms;
using System.Windows.Forms.Integration;

namespace Vim.VisualStudio.Implementation.OptionPages
{
    public sealed class ModeColorsOptionPage : DialogPage
    {
        private ElementHost _elementHost;
        private ModeColorsSettingsControl _modeColorsSettingsControl;

        protected override IWin32Window Window
        {
            get
            {
                if (_elementHost == null)
                {
                    _modeColorsSettingsControl = CreateModeColorsSettingsControl();
                    _elementHost = new ElementHost
                    {
                        Child = _modeColorsSettingsControl
                    };
                }

                return _elementHost;
            }
        }

        protected override void OnActivate(CancelEventArgs e)
        {
            base.OnActivate(e);
            _modeColorsSettingsControl?.LoadSettings();
        }

        protected override void OnApply(PageApplyEventArgs e)
        {
            _modeColorsSettingsControl?.Apply();
            base.OnApply(e);
        }

        private ModeColorsSettingsControl CreateModeColorsSettingsControl()
        {
            if (Site == null)
            {
                return null;
            }

            var componentModel = (IComponentModel)(Site.GetService(typeof(SComponentModel)));
            var exportProvider = componentModel.DefaultExportProvider;
            var vimApplicationSettings = exportProvider.GetExportedValue<IVimApplicationSettings>();
            return new ModeColorsSettingsControl(vimApplicationSettings);
        }
    }
}
