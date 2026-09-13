using System;
using System.ComponentModel.Composition.Hosting;
using System.Runtime.InteropServices;
using System.Threading;
using Microsoft.VisualStudio;
using Microsoft.VisualStudio.ComponentModelHost;
using Microsoft.VisualStudio.OLE.Interop;
using Microsoft.VisualStudio.Shell;

namespace Vim.VisualStudio.RecentFiles
{
    /// <summary>
    /// Owns exactly one command (".VsVimPlus.ShowRecentFiles") so it can be bound the ordinary
    /// Visual Studio way (Tools &gt; Options &gt; Keyboard) or invoked from a vimrc via
    /// ":vscmd .VsVimPlus.ShowRecentFiles". Deliberately separate from VsVim's own package so this
    /// feature has no footprint in VsVimShared/VsVim2022 at all.
    /// </summary>
    [PackageRegistration(UseManagedResourcesOnly = true, AllowsBackgroundLoading = true)]
    [ProvideMenuResource("Menus.ctmenu", 1)]
    [Guid(GuidList.PackageGuidString)]
    public sealed class RecentFilesPackage : AsyncPackage, IOleCommandTarget
    {
        private ExportProvider _exportProvider;

        protected override async System.Threading.Tasks.Task InitializeAsync(CancellationToken cancellationToken, IProgress<ServiceProgressData> progress)
        {
            await JoinableTaskFactory.SwitchToMainThreadAsync(cancellationToken);
            var componentModel = (IComponentModel)await GetServiceAsync(typeof(SComponentModel));
            _exportProvider = componentModel.DefaultExportProvider;
        }

        int IOleCommandTarget.Exec(ref Guid commandGroup, uint commandId, uint commandExecOpt, IntPtr variantIn, IntPtr variantOut)
        {
            if (commandGroup != GuidList.CommandSet || commandId != CommandIds.Show)
            {
                return VSConstants.E_FAIL;
            }

            _exportProvider.GetExportedValue<RecentFilesWindowService>().ShowPicker();
            return VSConstants.S_OK;
        }

        int IOleCommandTarget.QueryStatus(ref Guid commandGroup, uint commandsCount, OLECMD[] commands, IntPtr pCmdText)
        {
            if (commandGroup == GuidList.CommandSet && commandsCount == 1 && commands[0].cmdID == CommandIds.Show)
            {
                commands[0].cmdf = (uint)(OLECMDF.OLECMDF_ENABLED | OLECMDF.OLECMDF_SUPPORTED);
                return VSConstants.S_OK;
            }

            return VSConstants.E_FAIL;
        }
    }
}
