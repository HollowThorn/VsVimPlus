using System.Collections.Generic;

namespace Vim.VisualStudio.RecentFiles
{
    /// <summary>
    /// Tracks files as they are activated in the editor, most-recently-used first.
    /// Session-only: the list is not persisted across Visual Studio restarts.
    /// </summary>
    public interface IRecentFilesService
    {
        /// <summary>
        /// Record that the given file was just activated, moving it to the front
        /// of the recent files list
        /// </summary>
        void OnFileActivated(string filePath);

        /// <summary>
        /// The tracked files, most-recently-activated first
        /// </summary>
        IReadOnlyList<string> GetRecentFiles();
    }
}
