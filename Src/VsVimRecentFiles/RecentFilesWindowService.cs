using System;
using System.ComponentModel.Composition;
using System.Linq;
using System.Windows;
using System.Windows.Threading;
using Microsoft.VisualStudio;
using Microsoft.VisualStudio.Editor;
using Microsoft.VisualStudio.Shell;
using Microsoft.VisualStudio.Text;
using Microsoft.VisualStudio.Text.Classification;
using Microsoft.VisualStudio.Text.Editor;
using Microsoft.VisualStudio.TextManager.Interop;

namespace Vim.VisualStudio.RecentFiles
{
    /// <summary>
    /// Builds and shows the recent files picker window, and opens whatever file the user chose
    /// from it. Invoked from RecentFilesPackage in response to the "VsVimPlus.ShowRecentFiles"
    /// command.
    /// </summary>
    [Export(typeof(RecentFilesWindowService))]
    internal sealed class RecentFilesWindowService
    {
        private readonly IRecentFilesService _recentFilesService;
        private readonly IEditorFormatMapService _editorFormatMapService;
        private readonly ITextDocumentFactoryService _textDocumentFactoryService;
        private readonly IVsEditorAdaptersFactoryService _editorAdaptersFactoryService;
        private readonly SVsServiceProvider _serviceProvider;
        private readonly IVim _vim;

        [ImportingConstructor]
        internal RecentFilesWindowService(
            IRecentFilesService recentFilesService,
            IEditorFormatMapService editorFormatMapService,
            ITextDocumentFactoryService textDocumentFactoryService,
            IVsEditorAdaptersFactoryService editorAdaptersFactoryService,
            SVsServiceProvider serviceProvider,
            IVim vim)
        {
            _recentFilesService = recentFilesService;
            _editorFormatMapService = editorFormatMapService;
            _textDocumentFactoryService = textDocumentFactoryService;
            _editorAdaptersFactoryService = editorAdaptersFactoryService;
            _serviceProvider = serviceProvider;
            _vim = vim;
        }

        internal void ShowPicker()
        {
            // Called synchronously from whatever invoked the "VsVimPlus.ShowRecentFiles" command -
            // when that's a vim key mapping via ":vscmd", we're still deep inside Vim's key
            // processing. Visual Studio does its own focus bookkeeping once that finishes, which
            // runs *after* this method returns and would stomp on any focus set up here - the
            // picker would appear but keyboard input (typing, Delete, ...) kept going to the
            // editor underneath. Deferring the actual window creation to the next dispatcher pass
            // lets that finish first, so our window's focus is the last thing set and sticks.
            //
            // Background (not ApplicationIdle): ApplicationIdle only runs once the dispatcher has
            // *nothing* else pending, which in an active editor (caret blink, focus events, ongoing
            // key processing) can be starved indefinitely - the picker would only actually appear
            // once some unrelated keystroke (e.g. the user manually pressing Enter) happened to
            // create a gap. Background still defers past the current call stack, but is guaranteed
            // to run on the very next dispatcher pass.
            Application.Current?.Dispatcher.BeginInvoke(new Action(ShowPickerCore), DispatcherPriority.Background);
        }

        private bool TryGetActiveTextView(out IWpfTextView textView)
        {
            textView = null;
            if (!(_serviceProvider.GetService(typeof(SVsTextManager)) is IVsTextManager textManager))
            {
                return false;
            }

            if (textManager.GetActiveView(fMustHaveFocus: 0, pBuffer: null, ppView: out IVsTextView vsTextView) != VSConstants.S_OK ||
                vsTextView == null)
            {
                return false;
            }

            textView = _editorAdaptersFactoryService.GetWpfTextView(vsTextView);
            return textView != null;
        }

        private void ShowPickerCore()
        {
            TryGetActiveTextView(out IWpfTextView activeTextView);

            string currentFilePath = null;
            if (activeTextView != null &&
                _textDocumentFactoryService.TryGetTextDocument(activeTextView.TextBuffer, out ITextDocument document))
            {
                currentFilePath = document.FilePath;
            }

            var entries = _recentFilesService.GetRecentFiles()
                .Where(path => !string.Equals(path, currentFilePath, StringComparison.OrdinalIgnoreCase))
                .Select(path => new RecentFileEntry(path))
                .ToList();

            var editorFormatMap = activeTextView != null
                ? _editorFormatMapService.GetEditorFormatMap(activeTextView)
                : _editorFormatMapService.GetEditorFormatMap("text");

            // While the popup is open, the underlying vim buffer must stop treating Enter,
            // Backspace, Delete, etc. as vim commands - otherwise VsVim's own command routing
            // (which tracks Visual Studio's "active view", not raw window focus) keeps claiming
            // those keys for the editor behind the popup instead of letting them reach our
            // TextBox/ListBox. ExternalEdit is the same mechanism VsVim uses for inline rename.
            IVimBuffer activeVimBuffer = null;
            if (activeTextView != null && _vim.TryGetVimBuffer(activeTextView, out activeVimBuffer))
            {
                activeVimBuffer.SwitchMode(ModeKind.ExternalEdit, ModeArgument.None);
            }

            var window = new RecentFilesWindow(editorFormatMap, entries)
            {
                Owner = Application.Current?.MainWindow
            };

            PositionWindow(window);

            // DialogWindow.ShowDialog() (unlike a plain Window's) is designed to be called from
            // exactly this kind of context - it coordinates with IVsUIShell's own modal state
            // instead of just nesting a raw WPF message loop, which is what was crashing Visual
            // Studio with a plain Window.
            window.ShowDialog();

            if (activeVimBuffer != null && activeVimBuffer.ModeKind == ModeKind.ExternalEdit)
            {
                activeVimBuffer.SwitchPreviousMode();
            }

            if (!string.IsNullOrEmpty(window.ChosenFilePath))
            {
                VsShellUtilities.OpenDocument(_serviceProvider, window.ChosenFilePath);
            }
        }

        private static void PositionWindow(RecentFilesWindow window)
        {
            var owner = window.Owner;
            if (owner == null)
            {
                return;
            }

            window.Left = owner.Left + ((owner.Width - window.Width) / 2);
            window.Top = owner.Top + (owner.Height * 0.2);
        }
    }
}
