using System.IO;

namespace Vim.VisualStudio.RecentFiles
{
    internal sealed class RecentFileEntry
    {
        // These must be public: WPF's {Binding} only reflects over public properties, so an
        // internal property here silently fails to bind (blank text, no error) instead of
        // throwing.
        public string FilePath { get; }

        public string FileName { get; }

        public string DirectoryPath { get; }

        internal RecentFileEntry(string filePath)
        {
            FilePath = filePath;
            FileName = Path.GetFileName(filePath);
            DirectoryPath = Path.GetDirectoryName(filePath) ?? string.Empty;
        }
    }
}
