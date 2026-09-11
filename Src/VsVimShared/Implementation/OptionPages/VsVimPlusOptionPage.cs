using Microsoft.VisualStudio.ComponentModelHost;
using Microsoft.VisualStudio.Shell;
using System.ComponentModel;

namespace Vim.VisualStudio.Implementation.OptionPages
{
    /// <summary>
    /// Home for simple, fork-added settings that don't need a dedicated custom UI (things
    /// like Mode Colors, which does, get their own page too). Kept separate from
    /// DefaultOptionPage on purpose: that page mirrors upstream VsVim's own options and is a
    /// likely spot for merge conflicts when pulling upstream changes, whereas this page is
    /// unique to this fork and won't collide.
    /// </summary>
    public sealed class VsVimPlusOptionPage : DialogPage
    {
        [DisplayName("Use System Clipboard as Unnamed Register")]
        [Description("Equivalent to Vim's 'clipboard=unnamed': y/d/p/P without an explicit register read from and write to the Windows clipboard, so content copied outside Visual Studio can be pasted with p/P. Off by default")]
        [Category("Clipboard")]
        public bool UseSystemClipboardAsUnnamed { get; set; }

        protected override void OnActivate(CancelEventArgs e)
        {
            base.OnActivate(e);

            var vimApplicationSettings = GetVimApplicationSettings();
            if (vimApplicationSettings != null)
            {
                UseSystemClipboardAsUnnamed = vimApplicationSettings.UseSystemClipboardAsUnnamed;
            }
        }

        protected override void OnApply(PageApplyEventArgs e)
        {
            base.OnApply(e);

            var vimApplicationSettings = GetVimApplicationSettings();
            if (vimApplicationSettings != null)
            {
                vimApplicationSettings.UseSystemClipboardAsUnnamed = UseSystemClipboardAsUnnamed;
            }
        }

        private IVimApplicationSettings GetVimApplicationSettings()
        {
            if (Site == null)
            {
                return null;
            }

            var componentModel = (IComponentModel)(Site.GetService(typeof(SComponentModel)));
            return componentModel.DefaultExportProvider.GetExportedValue<IVimApplicationSettings>();
        }
    }
}
