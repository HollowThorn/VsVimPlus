using System;
using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.Linq;
using Microsoft.VisualStudio;
using Microsoft.VisualStudio.Shell;
using Microsoft.VisualStudio.Shell.Interop;
using Microsoft.VisualStudio.Text.Editor;
using Microsoft.VisualStudio.Utilities;

namespace Vim.VisualStudio.RecentFiles
{
    /// <summary>
    /// Tracks recently activated files by advising the running document table directly - this is
    /// deliberately independent of VsVim's own host/RDT plumbing so this feature has no footprint
    /// in VsVimShared. The IWpfTextViewCreationListener export exists only to get this part
    /// eagerly instantiated (MEF parts are otherwise lazy) as soon as the user opens any file, so
    /// tracking starts immediately rather than only after the picker is first shown.
    /// </summary>
    [Export(typeof(IRecentFilesService))]
    [Export(typeof(IWpfTextViewCreationListener))]
    [ContentType("text")]
    [TextViewRole(PredefinedTextViewRoles.Document)]
    internal sealed class RecentFilesTracker : IRecentFilesService, IWpfTextViewCreationListener, IVsRunningDocTableEvents3
    {
        internal const int MaxCount = 50;

        private readonly SVsServiceProvider _serviceProvider;
        private readonly List<string> _filePaths = new List<string>();
        private bool _advised;

        [ImportingConstructor]
        internal RecentFilesTracker(SVsServiceProvider serviceProvider)
        {
            _serviceProvider = serviceProvider;
        }

        void IWpfTextViewCreationListener.TextViewCreated(IWpfTextView textView)
        {
            EnsureAdvised();
        }

        private void EnsureAdvised()
        {
            if (_advised)
            {
                return;
            }

            _advised = true;
            if (_serviceProvider.GetService(typeof(SVsRunningDocumentTable)) is IVsRunningDocumentTable runningDocumentTable)
            {
                runningDocumentTable.AdviseRunningDocTableEvents(this, out uint _);
            }
        }

        void IRecentFilesService.OnFileActivated(string filePath)
        {
            if (string.IsNullOrEmpty(filePath))
            {
                return;
            }

            _filePaths.RemoveAll(path => string.Equals(path, filePath, StringComparison.OrdinalIgnoreCase));
            _filePaths.Insert(0, filePath);
            if (_filePaths.Count > MaxCount)
            {
                _filePaths.RemoveRange(MaxCount, _filePaths.Count - MaxCount);
            }
        }

        IReadOnlyList<string> IRecentFilesService.GetRecentFiles() => _filePaths.ToList();

        // Plain (non-explicit) methods below: these VS SDK COM interfaces don't merge same-named
        // members across their inheritance chain the way ordinary C# interfaces do, so an
        // explicit "IVsRunningDocTableEvents.Foo" implementation only satisfies that one
        // interface, not IVsRunningDocTableEvents2/3's copy of the same member. A plain public
        // method satisfies all of them at once.
        #region IVsRunningDocTableEvents3

        public int OnBeforeSave(uint docCookie) => VSConstants.S_OK;

        public int OnAfterFirstDocumentLock(uint docCookie, uint dwRDTLockType, uint dwReadLocksRemaining, uint dwEditLocksRemaining) => VSConstants.S_OK;

        public int OnBeforeLastDocumentUnlock(uint docCookie, uint dwRDTLockType, uint dwReadLocksRemaining, uint dwEditLocksRemaining) => VSConstants.S_OK;

        public int OnAfterSave(uint docCookie) => VSConstants.S_OK;

        public int OnAfterAttributeChange(uint docCookie, uint grfAttribs) => VSConstants.S_OK;

        public int OnBeforeDocumentWindowShow(uint docCookie, int fFirstShow, IVsWindowFrame pFrame)
        {
            if (_serviceProvider.GetService(typeof(SVsRunningDocumentTable)) is IVsRunningDocumentTable runningDocumentTable &&
                runningDocumentTable.GetDocumentInfo(
                    docCookie,
                    out uint _,
                    out uint _,
                    out uint _,
                    out string moniker,
                    out IVsHierarchy _,
                    out uint _,
                    out IntPtr _) == VSConstants.S_OK &&
                !string.IsNullOrEmpty(moniker))
            {
                ((IRecentFilesService)this).OnFileActivated(moniker);
            }

            return VSConstants.S_OK;
        }

        public int OnAfterDocumentWindowHide(uint docCookie, IVsWindowFrame pFrame) => VSConstants.S_OK;

        public int OnAfterAttributeChangeEx(uint docCookie, uint grfAttribs, IVsHierarchy pHierOld, uint itemidOld, string pszMkDocumentOld, IVsHierarchy pHierNew, uint itemidNew, string pszMkDocumentNew) => VSConstants.S_OK;

        #endregion
    }
}
